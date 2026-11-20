using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;

namespace SsoServer.Application.Services;

/// <summary>Admin use-cases over registered client applications.</summary>
public sealed class ClientAdminService
{
    private readonly IClientRepository _clients;
    private readonly IAuditService _audit;
    private readonly ILogger<ClientAdminService> _logger;

    public ClientAdminService(IClientRepository clients, IAuditService audit, ILogger<ClientAdminService> logger)
    {
        _clients = clients;
        _audit = audit;
        _logger = logger;
    }

    public Task<IReadOnlyList<ClientDto>> ListAsync(CancellationToken ct = default) => _clients.ListAsync(ct);

    public Task<ClientDto?> GetAsync(string clientId, CancellationToken ct = default) => _clients.FindAsync(clientId, ct);

    public async Task<ServiceResult<NewClientSecretDto>> CreateAsync(SaveClientDto dto, string? clientSecret, string actorUserId, CancellationToken ct = default)
    {
        var secret = dto.Kind == ClientKind.Confidential
            ? (string.IsNullOrWhiteSpace(clientSecret) ? GenerateSecret() : clientSecret)
            : null;

        var errors = Validation.ClientValidator.Validate(dto, isCreate: true, secret);
        if (errors.Count > 0)
        {
            return ServiceResult<NewClientSecretDto>.Fail(errors);
        }

        var result = await _clients.CreateAsync(dto, secret, ct);
        if (!result.Succeeded)
        {
            return ServiceResult<NewClientSecretDto>.Fail(result.Errors);
        }

        await _audit.LogAsync(AuditAction.ClientCreated, actorUserId, actorUserId, data: new { dto.ClientId, dto.Kind, dto.Flows }, ct: ct);
        return ServiceResult<NewClientSecretDto>.Ok(new NewClientSecretDto { ClientId = dto.ClientId, ClientSecret = secret ?? string.Empty });
    }

    public async Task<ServiceResult> UpdateAsync(SaveClientDto dto, string actorUserId, CancellationToken ct = default)
    {
        var errors = Validation.ClientValidator.Validate(dto, isCreate: false, clientSecret: null);
        if (errors.Count > 0)
        {
            return ServiceResult.Fail(errors);
        }

        var result = await _clients.UpdateAsync(dto, ct);
        if (!result.Succeeded)
        {
            return result;
        }

        await _audit.LogAsync(AuditAction.ClientUpdated, actorUserId, actorUserId, data: new { dto.ClientId }, ct: ct);
        return ServiceResult.Ok();
    }

    /// <summary>Generates and persists a new secret; the plaintext is returned exactly once.</summary>
    public async Task<ServiceResult<NewClientSecretDto>> RegenerateSecretAsync(string clientId, string actorUserId, CancellationToken ct = default)
    {
        var client = await _clients.FindAsync(clientId, ct);
        if (client is null)
        {
            return ServiceResult<NewClientSecretDto>.Fail(PersianMessages.ClientNotFound);
        }

        if (client.Kind != ClientKind.Confidential)
        {
            return ServiceResult<NewClientSecretDto>.Fail("کلاینت‌های Public فاقد Client Secret هستند.");
        }

        var result = await _clients.RegenerateSecretAsync(clientId, ct);
        if (!result.Succeeded || result.Value is null)
        {
            return ServiceResult<NewClientSecretDto>.Fail(result.Errors.Count > 0 ? result.Errors : [PersianMessages.ClientNotFound]);
        }

        await _audit.LogAsync(AuditAction.ClientSecretRegenerated, actorUserId, actorUserId, data: new { clientId }, ct: ct);
        return ServiceResult<NewClientSecretDto>.Ok(new NewClientSecretDto { ClientId = clientId, ClientSecret = result.Value });
    }

    public async Task<ServiceResult> SetStatusAsync(string clientId, bool enabled, string actorUserId, CancellationToken ct = default)
    {
        var client = await _clients.FindAsync(clientId, ct);
        if (client is null)
        {
            return ServiceResult.Fail(PersianMessages.ClientNotFound);
        }

        var dto = ToSaveDto(client);
        dto.Enabled = enabled;
        var result = await _clients.UpdateAsync(dto, ct);
        if (!result.Succeeded)
        {
            return result;
        }

        await _audit.LogAsync(AuditAction.ClientUpdated, actorUserId, actorUserId, data: new { clientId, Enabled = enabled }, ct: ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(string clientId, string actorUserId, CancellationToken ct = default)
    {
        var client = await _clients.FindAsync(clientId, ct);
        if (client is null)
        {
            return ServiceResult.Fail(PersianMessages.ClientNotFound);
        }

        var result = await _clients.DeleteAsync(clientId, ct);
        if (!result.Succeeded)
        {
            return result;
        }

        await _audit.LogAsync(AuditAction.ClientDeleted, actorUserId, actorUserId, data: new { clientId }, ct: ct);
        return ServiceResult.Ok();
    }

    internal static SaveClientDto ToSaveDto(ClientDto client) => new()
    {
        ClientId = client.ClientId,
        DisplayName = client.DisplayName,
        PersianDisplayName = client.PersianDisplayName,
        Description = client.Description,
        Owner = client.Owner,
        Kind = client.Kind,
        Flows = client.Flows,
        Scopes = client.Scopes,
        RedirectUris = client.RedirectUris,
        PostLogoutRedirectUris = client.PostLogoutRedirectUris,
        RequireConsent = client.RequireConsent,
        Enabled = client.Enabled,
    };

    /// <summary>Generates a cryptographically random, URL-safe client secret.</summary>
    public static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
