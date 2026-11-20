using System.Text.RegularExpressions;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Models;

namespace SsoServer.Application.Validation;

public static partial class ClientValidator
{
    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9._-]{1,99}$", RegexOptions.Compiled)]
    private static partial Regex ClientIdRegex();

    /// <summary>
    /// Validates a client registration. Enforces the permission matrix:
    /// password and client_credentials grants are confidential-only;
    /// authorization code requires registered redirect URIs (PKCE is always required);
    /// offline_access scope requires the refresh token flow.
    /// </summary>
    public static IReadOnlyList<string> Validate(SaveClientDto dto, bool isCreate, string? clientSecret)
    {
        var errors = new List<string>();

        if (dto is null)
        {
            return ["اطلاعات کلاینت ارسال نشده است."];
        }

        if (string.IsNullOrWhiteSpace(dto.ClientId) || !ClientIdRegex().IsMatch(dto.ClientId))
        {
            errors.Add("شناسه کلاینت باید ۲ تا ۱۰۰ کاراکتر و فقط شامل حروف انگلیسی، عدد، نقطه، خط تیره و زیرخط باشد.");
        }

        if (string.IsNullOrWhiteSpace(dto.PersianDisplayName))
        {
            errors.Add("نام نمایشی فارسی کلاینت الزامی است.");
        }

        if (dto.Flows == ClientFlow.None)
        {
            errors.Add("حداقل یک جریان احراز هویت باید انتخاب شود.");
        }

        if (dto.Flows.HasFlag(ClientFlow.AuthorizationCode) && dto.RedirectUris.Count == 0)
        {
            errors.Add("برای جریان Authorization Code حداقل یک Redirect URI باید ثبت شود.");
        }

        if ((dto.Flows.HasFlag(ClientFlow.Password) || dto.Flows.HasFlag(ClientFlow.ClientCredentials))
            && dto.Kind != ClientKind.Confidential)
        {
            errors.Add("جریان‌های password و client_credentials فقط برای کلاینت‌های Confidential مجاز هستند.");
        }

        // سیاست امنیتی سرور (از بروزرسانی ۲۰۲۶-۱۱-۱۹): نوع Public حذف شد؛
        // هر کلاینت باید Confidential باشد و Client Secret معتبر داشته باشد.
        if (dto.Kind != ClientKind.Confidential)
        {
            errors.Add("برای اتصال به سرور SSO، کلاینت باید از نوع Confidential (دارای Client Secret) باشد.");
        }

        if (dto.Kind == ClientKind.Confidential && string.IsNullOrWhiteSpace(clientSecret))
        {
            errors.Add("کلاینت Confidential باید دارای Client Secret باشد.");
        }
        else if (dto.Kind == ClientKind.Confidential && clientSecret is not null && clientSecret.Trim().Length < 16)
        {
            errors.Add("Client Secret باید حداقل ۱۶ کاراکتر باشد.");
        }

        if (dto.Scopes.Count == 0)
        {
            errors.Add("حداقل یک Scope باید انتخاب شود.");
        }

        foreach (var scope in dto.Scopes)
        {
            if (!SsoScopes.All.Contains(scope, StringComparer.Ordinal))
            {
                errors.Add($"اسکوپ «{scope}» در فهرست مجاز نیست.");
            }
        }

        if (dto.Scopes.Contains(SsoScopes.OfflineAccess, StringComparer.Ordinal) && !dto.Flows.HasFlag(ClientFlow.RefreshToken))
        {
            errors.Add("اسکوپ offline_access نیاز به جریان refresh_token دارد.");
        }

        foreach (var uri in dto.RedirectUris)
        {
            if (!RedirectUriValidator.IsWellFormed(uri))
            {
                errors.Add($"Redirect URI نامعتبر است: {uri}");
            }
        }

        foreach (var uri in dto.PostLogoutRedirectUris)
        {
            if (!RedirectUriValidator.IsWellFormed(uri))
            {
                errors.Add($"Post-Logout Redirect URI نامعتبر است: {uri}");
            }
        }

        return errors;
    }
}
