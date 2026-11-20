using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;

namespace SsoServer.Application.Services;

/// <summary>Audit service: persists security events with a JSON data payload.</summary>
public sealed class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IServiceScopeFactory scopeFactory, ILogger<AuditService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task LogAsync(AuditAction action, string? actorUserId = null, string? actorUserName = null,
        string? clientId = null, string? ip = null, object? data = null, CancellationToken ct = default)
    {
        try
        {
            var entry = new AuditLog
            {
                Action = action,
                ActorUserId = actorUserId,
                ActorUserName = actorUserName,
                ClientId = clientId,
                Ip = ip,
                DataJson = data is null ? "{}" : JsonSerializer.Serialize(data, SerializerOptions),
                CreatedAt = DateTimeOffset.UtcNow,
            };

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IAuditRepository>();
            await repository.AddAsync(entry, ct);

            var level = action switch
            {
                AuditAction.LoginFailed or AuditAction.Lockout or AuditAction.ConsentDenied => LogLevel.Warning,
                _ => LogLevel.Information,
            };

            _logger.Log(level, "Audit {Action} user={User} client={Client} ip={Ip}", action, actorUserName ?? actorUserId, clientId, ip);
        }
        catch (Exception ex)
        {
            // Auditing must never break the authentication flow.
            _logger.LogError(ex, "Failed to persist audit entry {Action}", action);
        }
    }

    public async Task<IReadOnlyList<AuditLog>> QueryAsync(int skip, int take, AuditAction? action = null,
        string? userId = null, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAuditRepository>();
        return await repository.QueryAsync(Math.Max(0, skip), Math.Min(200, Math.Max(1, take)), action, userId, ct);
    }
}
