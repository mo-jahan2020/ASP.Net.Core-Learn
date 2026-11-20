using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SsoServer.Application.Validation;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;

namespace SsoServer.Application.Services;

/// <summary>
/// Hot-reloadable settings service. Singleton by design: it resolves the scoped
/// <see cref="ISettingsRepository"/> through <see cref="IServiceScopeFactory"/> and keeps a
/// synchronous <see cref="Current"/> snapshot for middleware and options factories.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private const string CacheKey = "sso-effective-settings";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    private readonly SsoOptions _sso;
    private readonly SecurityOptions _security;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SettingsService> _logger;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    private EffectiveSettings _current;

    public SettingsService(
        IOptions<SsoOptions> sso,
        IOptions<SecurityOptions> security,
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache,
        ILogger<SettingsService> logger)
    {
        _sso = sso.Value;
        _security = security.Value;
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
        _current = BuildFromOptions();
    }

    public event Action? SettingsChanged;

    public EffectiveSettings Current => Volatile.Read(ref _current);

    public async Task<EffectiveSettings> GetEffectiveAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out EffectiveSettings? cached) && cached is not null)
        {
            return cached;
        }

        await _loadLock.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue(CacheKey, out cached) && cached is not null)
            {
                return cached;
            }

            var settings = await LoadAsync(ct);
            _cache.Set(CacheKey, settings, CacheTtl);
            return settings;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public async Task<ServiceResult> UpdateAsync(UpdateSettingsDto dto, string actorUserId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var errors = SettingsValidator.Validate(dto);
        if (errors.Count > 0)
        {
            return ServiceResult.Fail(errors);
        }

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();

        if (dto.CorsOrigins is not null)
        {
            var normalized = string.Join(",", ParseOrigins(dto.CorsOrigins));
            await repository.SetAsync(SettingKeys.CorsOrigins, normalized, actorUserId, ct);
        }

        if (dto.RequireConsentByDefault.HasValue)
        {
            await repository.SetAsync(SettingKeys.RequireConsentByDefault, dto.RequireConsentByDefault.Value.ToString(), actorUserId, ct);
        }

        if (dto.MaxFailedLoginAttempts.HasValue)
        {
            await repository.SetAsync(SettingKeys.MaxFailedLoginAttempts, dto.MaxFailedLoginAttempts.Value.ToString(), actorUserId, ct);
        }

        if (dto.LockoutMinutes.HasValue)
        {
            await repository.SetAsync(SettingKeys.LockoutMinutes, dto.LockoutMinutes.Value.ToString(), actorUserId, ct);
        }

        if (dto.TokenRateLimitPerMinute.HasValue)
        {
            await repository.SetAsync(SettingKeys.TokenRateLimitPerMinute, dto.TokenRateLimitPerMinute.Value.ToString(), actorUserId, ct);
        }

        if (dto.SessionCookieSliding.HasValue)
        {
            await repository.SetAsync(SettingKeys.SessionCookieSliding, dto.SessionCookieSliding.Value.ToString(), actorUserId, ct);
        }

        if (dto.LoginTitle is not null)
        {
            await repository.SetAsync(SettingKeys.LoginTitle, dto.LoginTitle, actorUserId, ct);
        }

        if (dto.LoginSubtitle is not null)
        {
            await repository.SetAsync(SettingKeys.LoginSubtitle, dto.LoginSubtitle, actorUserId, ct);
        }

        _cache.Remove(CacheKey);
        await LoadAsync(ct);

        _logger.LogInformation("Effective settings updated by {Actor}", actorUserId);
        SettingsChanged?.Invoke();
        return ServiceResult.Ok();
    }

    private async Task<EffectiveSettings> LoadAsync(CancellationToken ct)
    {
        IReadOnlyDictionary<string, string> overrides;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();
            overrides = await repository.GetAllAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load settings overrides from the database; falling back to configuration defaults.");
            return Current;
        }

        var settings = BuildFromOptions();

        if (overrides.TryGetValue(SettingKeys.CorsOrigins, out var origins) && !string.IsNullOrWhiteSpace(origins))
        {
            settings.CorsOrigins = ParseOrigins(origins);
        }

        if (overrides.TryGetValue(SettingKeys.RequireConsentByDefault, out var consent) && bool.TryParse(consent, out var requireConsent))
        {
            settings.RequireConsentByDefault = requireConsent;
        }

        if (overrides.TryGetValue(SettingKeys.MaxFailedLoginAttempts, out var attempts) && int.TryParse(attempts, out var maxAttempts) && maxAttempts is >= 1 and <= 100)
        {
            settings.MaxFailedLoginAttempts = maxAttempts;
        }

        if (overrides.TryGetValue(SettingKeys.LockoutMinutes, out var lockout) && int.TryParse(lockout, out var lockoutMinutes) && lockoutMinutes is >= 1 and <= 1440)
        {
            settings.LockoutMinutes = lockoutMinutes;
        }

        if (overrides.TryGetValue(SettingKeys.TokenRateLimitPerMinute, out var rate) && int.TryParse(rate, out var rateLimit) && rateLimit is >= 1 and <= 100000)
        {
            settings.TokenRateLimitPerMinute = rateLimit;
        }

        if (overrides.TryGetValue(SettingKeys.SessionCookieSliding, out var sliding) && bool.TryParse(sliding, out var cookieSliding))
        {
            settings.SessionCookieSliding = cookieSliding;
        }

        if (overrides.TryGetValue(SettingKeys.LoginTitle, out var title))
        {
            settings.LoginTitle = title;
        }

        if (overrides.TryGetValue(SettingKeys.LoginSubtitle, out var subtitle))
        {
            settings.LoginSubtitle = subtitle;
        }

        Volatile.Write(ref _current, settings);
        return settings;
    }

    private EffectiveSettings BuildFromOptions() => new()
    {
        CorsOrigins = [.. _security.CorsOrigins.Select(o => o.Trim()).Where(o => o.Length > 0)],
        RequireConsentByDefault = _sso.RequireConsentByDefault,
        MaxFailedLoginAttempts = _security.MaxFailedLoginAttempts,
        LockoutMinutes = _security.LockoutMinutes,
        TokenRateLimitPerMinute = _security.TokenRateLimitPerMinute,
        SessionCookieSliding = true,
        IssuerUri = _sso.IssuerUri,
        AccessTokenLifetimeMinutes = _sso.AccessTokenLifetimeMinutes,
        IdentityTokenLifetimeMinutes = _sso.IdentityTokenLifetimeMinutes,
        RefreshTokenLifetimeDays = _sso.RefreshTokenLifetimeDays,
        RefreshTokenAbsoluteLifetimeDays = _sso.RefreshTokenAbsoluteLifetimeDays,
        UseSlidingRefreshTokens = _sso.UseSlidingRefreshTokens,
        SsoSessionCookieHours = _sso.SsoSessionCookieHours,
    };

    internal static string[] ParseOrigins(string raw)
        => raw.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
