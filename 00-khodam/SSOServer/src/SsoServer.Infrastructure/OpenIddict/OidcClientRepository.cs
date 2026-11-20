using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Infrastructure.Persistence;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Infrastructure.OpenIddict;

/// <summary>
/// <see cref="IClientRepository"/> implementation backed by the OpenIddict application
/// manager plus the <see cref="ClientProfile"/> metadata table.
/// </summary>
public sealed class OidcClientRepository : IClientRepository
{
    private readonly IOpenIddictApplicationManager _applications;
    private readonly AppDbContext _context;

    public OidcClientRepository(IOpenIddictApplicationManager applications, AppDbContext context)
    {
        _applications = applications;
        _context = context;
    }

    public async Task<ClientDto?> FindAsync(string clientId, CancellationToken ct = default)
    {
        var application = await _applications.FindByClientIdAsync(clientId);
        return application is null ? null : await ToDtoAsync(application, ct);
    }

    public async Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken ct = default)
    {
        var list = new List<ClientDto>();
        await foreach (var application in _applications.ListAsync(cancellationToken: ct))
        {
            var dto = await ToDtoAsync(application, ct);
            if (dto is not null)
            {
                list.Add(dto);
            }
        }

        return list.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public async Task<ServiceResult<string>> CreateAsync(SaveClientDto dto, string? clientSecret, CancellationToken ct = default)
    {
        if (await _applications.FindByClientIdAsync(dto.ClientId, ct) is not null)
        {
            return ServiceResult<string>.Fail("کلاینتی با این شناسه قبلاً ثبت شده است.");
        }

        var descriptor = BuildDescriptor(dto, clientSecret);
        foreach (var permission in Seeder.BuildPermissions(dto.Kind, dto.Flows, dto.Scopes))
        {
            descriptor.Permissions.Add(permission);
        }

        await _applications.CreateAsync(descriptor, ct);
        await UpsertProfileAsync(dto, ct);
        return ServiceResult<string>.Ok(dto.ClientId);
    }

    public async Task<ServiceResult> UpdateAsync(SaveClientDto dto, CancellationToken ct = default)
    {
        var application = await _applications.FindByClientIdAsync(dto.ClientId, ct);
        if (application is null)
        {
            return ServiceResult.Fail(PersianMessagesClientNotFound);
        }

        // Populate a descriptor from the CURRENT application first so the existing
        // (hashed) client secret survives the update - OpenIddict rejects confidential
        // applications whose secret becomes null via the descriptor.
        var descriptor = new OpenIddictApplicationDescriptor();
        await _applications.PopulateAsync(descriptor, application, ct);

        descriptor.ClientId = dto.ClientId;
        descriptor.DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? dto.ClientId : dto.DisplayName;
        descriptor.ClientType = dto.Kind == ClientKind.Confidential ? ClientTypes.Confidential : ClientTypes.Public;
        descriptor.ConsentType = ConsentTypes.Implicit;

        descriptor.Permissions.Clear();
        foreach (var permission in Seeder.BuildPermissions(dto.Kind, dto.Flows, dto.Scopes))
        {
            descriptor.Permissions.Add(permission);
        }

        descriptor.Requirements.Clear();
        if (dto.Flows.HasFlag(ClientFlow.AuthorizationCode))
        {
            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        }

        descriptor.RedirectUris.Clear();
        foreach (var uri in dto.RedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            descriptor.RedirectUris.Add(new Uri(uri.Trim(), UriKind.Absolute));
        }

        descriptor.PostLogoutRedirectUris.Clear();
        foreach (var uri in dto.PostLogoutRedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(uri.Trim(), UriKind.Absolute));
        }

        await _applications.UpdateAsync(application, descriptor, ct);
        await UpsertProfileAsync(dto, ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string clientId, CancellationToken ct = default)
    {
        var application = await _applications.FindByClientIdAsync(clientId, ct);
        if (application is null)
        {
            return ServiceResult.Fail(PersianMessagesClientNotFound);
        }

        await _applications.DeleteAsync(application, ct);
        var profile = await _context.ClientProfiles.FindAsync([clientId], ct);
        if (profile is not null)
        {
            _context.ClientProfiles.Remove(profile);
            await _context.SaveChangesAsync(ct);
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<string>> RegenerateSecretAsync(string clientId, CancellationToken ct = default)
    {
        var application = await _applications.FindByClientIdAsync(clientId, ct);
        if (application is null)
        {
            return ServiceResult<string>.Fail(PersianMessagesClientNotFound);
        }

        var secret = SsoServer.Application.Services.ClientAdminService.GenerateSecret();
        await _applications.UpdateAsync(application, secret, ct);
        return ServiceResult<string>.Ok(secret);
    }

    public async Task<bool> IsEnabledAsync(string clientId, CancellationToken ct = default)
    {
        var profile = await _context.ClientProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.ClientId == clientId, ct);
        return profile?.Enabled ?? true;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetPersianDisplayNamesAsync(IReadOnlyCollection<string> clientIds, CancellationToken ct = default)
    {
        var profiles = await _context.ClientProfiles.AsNoTracking()
            .Where(p => clientIds.Contains(p.ClientId))
            .ToDictionaryAsync(p => p.ClientId, p => p.PersianDisplayName, ct);

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var missing = new List<string>();
        await foreach (var application in _applications.ListAsync(cancellationToken: ct))
        {
            var id = await _applications.GetClientIdAsync(application, ct);
            if (id is not null && clientIds.Contains(id) && !profiles.ContainsKey(id))
            {
                missing.Add(id);
            }
        }

        foreach (var pair in profiles)
        {
            result[pair.Key] = pair.Value;
        }

        foreach (var id in missing)
        {
            result[id] = id;
        }

        return result;
    }

    private const string PersianMessagesClientNotFound = "کلاینت یافت نشد.";

    private static OpenIddictApplicationDescriptor BuildDescriptor(SaveClientDto dto, string? clientSecret)
    {
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = dto.ClientId,
            DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? dto.ClientId : dto.DisplayName,
            ClientType = dto.Kind == ClientKind.Confidential ? ClientTypes.Confidential : ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit,
        };

        if (dto.Flows.HasFlag(ClientFlow.AuthorizationCode))
        {
            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        }

        if (dto.Kind == ClientKind.Confidential && !string.IsNullOrEmpty(clientSecret))
        {
            descriptor.ClientSecret = clientSecret;
        }

        foreach (var uri in dto.RedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            descriptor.RedirectUris.Add(new Uri(uri.Trim(), UriKind.Absolute));
        }

        foreach (var uri in dto.PostLogoutRedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(uri.Trim(), UriKind.Absolute));
        }

        return descriptor;
    }

    private async Task UpsertProfileAsync(SaveClientDto dto, CancellationToken ct)
    {
        var profile = await _context.ClientProfiles.FindAsync([dto.ClientId], ct);
        if (profile is null)
        {
            profile = new ClientProfile { ClientId = dto.ClientId, CreatedAt = DateTimeOffset.UtcNow };
            _context.ClientProfiles.Add(profile);
        }

        profile.PersianDisplayName = dto.PersianDisplayName;
        profile.Description = dto.Description;
        profile.Owner = dto.Owner;
        profile.Enabled = dto.Enabled;
        profile.RequireConsent = dto.RequireConsent;
        await _context.SaveChangesAsync(ct);
    }

    private async Task<ClientDto?> ToDtoAsync(object application, CancellationToken ct)
    {
        var clientId = await _applications.GetClientIdAsync(application, ct);
        if (clientId is null)
        {
            return null;
        }

        var permissions = await _applications.GetPermissionsAsync(application, ct);
        var redirectUris = await _applications.GetRedirectUrisAsync(application, ct);
        var postLogoutUris = await _applications.GetPostLogoutRedirectUrisAsync(application, ct);
        var kind = await _applications.HasClientTypeAsync(application, ClientTypes.Confidential, ct)
            ? ClientKind.Confidential
            : ClientKind.Public;
        var profile = await _context.ClientProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.ClientId == clientId, ct);

        return new ClientDto
        {
            ClientId = clientId,
            DisplayName = await _applications.GetDisplayNameAsync(application, ct) ?? clientId,
            PersianDisplayName = profile?.PersianDisplayName ?? clientId,
            Description = profile?.Description ?? string.Empty,
            Owner = profile?.Owner ?? string.Empty,
            Kind = kind,
            Flows = ParseFlows(permissions),
            Scopes = [.. permissions.Where(p => p.StartsWith(Permissions.Prefixes.Scope, StringComparison.Ordinal))
                .Select(p => p[Permissions.Prefixes.Scope.Length..])],
            RedirectUris = [.. redirectUris],
            PostLogoutRedirectUris = [.. postLogoutUris],
            RequireConsent = profile?.RequireConsent,
            Enabled = profile?.Enabled ?? true,
            HasSecret = kind == ClientKind.Confidential,
            CreatedAt = profile?.CreatedAt ?? DateTimeOffset.UtcNow,
        };
    }

    private static ClientFlow ParseFlows(ImmutableArray<string> permissions)
    {
        var flows = ClientFlow.None;
        foreach (var permission in permissions)
        {
            flows |= permission switch
            {
                Permissions.GrantTypes.AuthorizationCode => ClientFlow.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken => ClientFlow.RefreshToken,
                Permissions.GrantTypes.Password => ClientFlow.Password,
                Permissions.GrantTypes.ClientCredentials => ClientFlow.ClientCredentials,
                _ => ClientFlow.None,
            };
        }

        return flows;
    }
}
