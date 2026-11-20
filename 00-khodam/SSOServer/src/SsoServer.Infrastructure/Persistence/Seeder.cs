using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;
using SsoServer.Infrastructure.Security;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SsoServer.Infrastructure.Persistence;

/// <summary>
/// Idempotent first-run seeder: roles, admin account, OIDC scopes, first-party clients
/// from configuration and the six integration sample clients.
/// </summary>
public static class Seeder
{
    private const string DefaultScopeList = "openid,profile,roles,email,api,offline_access";

    public static async Task SeedAsync(IServiceProvider provider)
    {
        var users = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = provider.GetRequiredService<RoleManager<ApplicationRole>>();
        var applications = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopes = provider.GetRequiredService<IOpenIddictScopeManager>();
        var context = provider.GetRequiredService<AppDbContext>();
        var seed = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SeedOptions>>().Value;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");

        await EnsureRolesAsync(roles);
        await EnsureAdminAsync(users, seed, logger);
        await EnsureScopesAsync(scopes);

        foreach (var client in seed.FirstPartyClients)
        {
            // سیاست امنیتی ۲۰۲۶-۱۱-۱۹: همه کلاینت‌ها Confidential با Client Secret هستند.
            // اگر در تنظیمات secret تعریف نشده باشد، یک secret تصادفی قوی تولید و در لاگ
            // ثبت می‌شود تا ادمین آن را به برنامه کلاینت بدهد (یا با Regenerate عوض کند).
            var secret = client.ClientSecret;
            if (string.IsNullOrWhiteSpace(secret))
            {
                secret = SsoServer.Application.Services.ClientAdminService.GenerateSecret();
                logger.LogWarning(
                    "Client {ClientId} has no ClientSecret configured; generated a random one. " +
                    "Set 'Seed:FirstPartyClients[*].ClientSecret' in appsettings to a known value or regenerate it from the admin UI.",
                    client.ClientId);
            }

            await EnsureClientAsync(applications, context, ToClientSeed(
                clientId: client.ClientId,
                displayName: string.IsNullOrWhiteSpace(client.DisplayName) ? client.ClientId : client.DisplayName,
                persianDisplayName: string.IsNullOrWhiteSpace(client.PersianDisplayName) ? client.ClientId : client.PersianDisplayName,
                kind: ClientKind.Confidential,
                flows: ParseFlows(client.Flows),
                scopesList: client.Scopes,
                redirectUris: ParseList(client.RedirectUris),
                postLogoutUris: ParseList(client.PostLogoutRedirectUris),
                secret: secret));
        }

        foreach (var client in BuildSampleClients(seed.SampleClientSecret))
        {
            await EnsureClientAsync(applications, context, client);
        }

        await EnsureCorsDefaultAsync(context);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureRolesAsync(RoleManager<ApplicationRole> roles)
    {
        if (!await roles.RoleExistsAsync(SsoRoles.Admin))
        {
            await roles.CreateAsync(new ApplicationRole(SsoRoles.Admin, "مدیر سامانه"));
        }

        if (!await roles.RoleExistsAsync(SsoRoles.User))
        {
            await roles.CreateAsync(new ApplicationRole(SsoRoles.User, "کاربر عادی"));
        }
    }

    private static async Task EnsureAdminAsync(UserManager<ApplicationUser> users, SeedOptions seed, ILogger logger)
    {
        if (await users.FindByNameAsync(seed.AdminUsername) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = seed.AdminUsername,
            Email = seed.AdminEmail,
            EmailConfirmed = true,
            DisplayName = "مدیر سامانه",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await users.CreateAsync(admin, seed.AdminPassword);
        if (result.Succeeded)
        {
            await users.AddToRoleAsync(admin, SsoRoles.Admin);
            logger.LogInformation("Seeded default admin account {Username} - change its password immediately", seed.AdminUsername);
        }
        else
        {
            logger.LogError("Failed to seed the default admin account: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task EnsureScopesAsync(IOpenIddictScopeManager scopes)
    {
        foreach (var name in new[] { SsoScopes.OpenId, SsoScopes.Profile, SsoScopes.Roles, SsoScopes.Email, SsoScopes.OfflineAccess })
        {
            if (await scopes.FindByNameAsync(name) is null)
            {
                await scopes.CreateAsync(new OpenIddictScopeDescriptor { Name = name });
            }
        }

        if (await scopes.FindByNameAsync(SsoScopes.Api) is null)
        {
            // The "api" scope maps to the "api" audience so access tokens carry aud=api.
            await scopes.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = SsoScopes.Api,
                Resources = { SsoScopes.Api },
            });
        }
    }

    private static async Task EnsureCorsDefaultAsync(AppDbContext context)
    {
        if (!await context.AppSettings.AnyAsync(s => s.Key == SettingKeys.CorsOrigins))
        {
            context.AppSettings.Add(new AppSetting
            {
                Key = SettingKeys.CorsOrigins,
                Value = "http://localhost:5500",
                UpdatedBy = "seeder",
                UpdatedAt = DateTimeOffset.UtcNow,
            });
        }
    }

    private static async Task EnsureClientAsync(
        IOpenIddictApplicationManager applications,
        AppDbContext context,
        ClientSeed client)
    {
        var existing = await applications.FindByClientIdAsync(client.ClientId);

        // Descriptor هدف: هر دو مسیر create و update از همین یک تعریف می‌سازند تا
        // کلاینت‌های seed شده با تغییر seed (redirect جدید، فلو جدید، PKCE) همگام بمانند.
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            DisplayName = client.DisplayName,
            ClientType = client.Kind == ClientKind.Confidential ? ClientTypes.Confidential : ClientTypes.Public,
            ConsentType = ConsentTypes.Implicit,
        };

        foreach (var permission in BuildPermissions(client.Kind, client.Flows, ParseScopes(client.Scopes)))
        {
            descriptor.Permissions.Add(permission);
        }

        if (client.Flows.HasFlag(ClientFlow.AuthorizationCode))
        {
            descriptor.Requirements.Add(Requirements.Features.ProofKeyForCodeExchange);
        }

        if (client.Kind == ClientKind.Confidential && !string.IsNullOrEmpty(client.Secret))
        {
            descriptor.ClientSecret = client.Secret;
        }

        foreach (var uri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(uri, UriKind.Absolute));
        }

        foreach (var uri in client.PostLogoutRedirectUris)
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(uri, UriKind.Absolute));
        }

        if (existing is null)
        {
            await applications.CreateAsync(descriptor);
        }
        else if (!await MatchesSeedAsync(applications, existing, descriptor))
        {
            // به‌روزرسانی کلاینت seed شده‌ی قدیمی (idempotent upgrade): اگر فیلدهای
            // تعریف‌شده توسط seed با seed فعلی فرق دارند (مثلاً redirect_uri یا مجوزها
            // در نسخه‌ی قدیمی سرور ثبت شده)، همگام می‌کنیم تا DBهای قدیمی بدون
            // دست‌کاری دستی کار کنند. ویرایش‌های غیر-seed مدیر نیز بازنویسی می‌شوند،
            // اما فقط روی همین فیلدهای seed — secret فقط برای کلاینت‌های seed شده.
            await applications.UpdateAsync(existing, descriptor);
        }

        var profile = await context.ClientProfiles.FindAsync([client.ClientId]);
        if (profile is null)
        {
            context.ClientProfiles.Add(new ClientProfile
            {
                ClientId = client.ClientId,
                PersianDisplayName = client.PersianDisplayName,
                Description = client.Description,
                Owner = client.Owner,
                Enabled = true,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }
    }

    /// <summary>
    /// آیا application موجود با seed فعلی هم‌خوانی دارد؟ مقایسه‌ی فیلدهایی که seed
    /// تعریف می‌کند: permissions، requirements، redirectها و نوع کلاینت.
    /// </summary>
    private static async Task<bool> MatchesSeedAsync(
        IOpenIddictApplicationManager applications, object existing, OpenIddictApplicationDescriptor seed)
    {
        static bool SetEquals<T>(IEnumerable<T> a, IEnumerable<T> b)
            => new HashSet<T>(a).SetEquals(b);

        var existingPermissions = await applications.GetPermissionsAsync(existing);
        if (!SetEquals(existingPermissions, seed.Permissions)) return false;

        var existingRequirements = await applications.GetRequirementsAsync(existing);
        if (!SetEquals(existingRequirements, seed.Requirements)) return false;

        var existingRedirects = await applications.GetRedirectUrisAsync(existing);
        if (!SetEquals(existingRedirects.Select(u => new Uri(u, UriKind.Absolute).AbsoluteUri),
                seed.RedirectUris.Select(u => u.AbsoluteUri))) return false;

        var existingPostLogout = await applications.GetPostLogoutRedirectUrisAsync(existing);
        if (!SetEquals(existingPostLogout.Select(u => new Uri(u, UriKind.Absolute).AbsoluteUri),
                seed.PostLogoutRedirectUris.Select(u => u.AbsoluteUri))) return false;

        var existingType = await applications.GetClientTypeAsync(existing);
        if (!string.Equals(existingType, seed.ClientType, StringComparison.Ordinal)) return false;

        return true;
    }

    internal static HashSet<string> BuildPermissions(ClientKind kind, ClientFlow flows, IEnumerable<string> scopes)
    {
        var permissions = new HashSet<string>(StringComparer.Ordinal);
        if (flows.HasFlag(ClientFlow.AuthorizationCode))
        {
            permissions.Add(Permissions.Endpoints.Authorization);
            permissions.Add(Permissions.Endpoints.EndSession);
            permissions.Add(Permissions.GrantTypes.AuthorizationCode);
            permissions.Add(Permissions.ResponseTypes.Code);
        }

        if (flows.HasFlag(ClientFlow.RefreshToken))
        {
            permissions.Add(Permissions.Endpoints.Token);
            permissions.Add(Permissions.GrantTypes.RefreshToken);
        }

        if (flows.HasFlag(ClientFlow.Password))
        {
            permissions.Add(Permissions.Endpoints.Token);
            permissions.Add(Permissions.GrantTypes.Password);
        }

        if (flows.HasFlag(ClientFlow.ClientCredentials))
        {
            permissions.Add(Permissions.Endpoints.Token);
            permissions.Add(Permissions.GrantTypes.ClientCredentials);
        }

        if (kind == ClientKind.Confidential)
        {
            permissions.Add(Permissions.Endpoints.Introspection);
            permissions.Add(Permissions.Endpoints.Revocation);
        }
        else
        {
            permissions.Add(Permissions.Endpoints.Revocation);
        }

        foreach (var scope in scopes)
        {
            permissions.Add(Permissions.Prefixes.Scope + scope);
        }

        return permissions;
    }

    internal static ClientFlow ParseFlows(string flows)
    {
        var result = ClientFlow.None;
        foreach (var part in flows.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            result |= part.ToLowerInvariant() switch
            {
                "authorization_code" or "authorization_code_pkce" or "code" => ClientFlow.AuthorizationCode,
                "refresh_token" or "refresh" => ClientFlow.RefreshToken,
                "password" => ClientFlow.Password,
                "client_credentials" => ClientFlow.ClientCredentials,
                _ => ClientFlow.None,
            };
        }

        return result;
    }

    private static string[] ParseList(string? list)
        => string.IsNullOrWhiteSpace(list)
            ? []
            : list.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    internal static string[] ParseScopes(string? list)
        => ParseList(list) is { Length: > 0 } parsed ? parsed : [.. DefaultScopeList.Split(',')];

    internal sealed record ClientSeed(
        string ClientId,
        string DisplayName,
        string PersianDisplayName,
        ClientKind Kind,
        ClientFlow Flows,
        string Scopes,
        string[] RedirectUris,
        string[] PostLogoutRedirectUris,
        string? Secret,
        string Description = "",
        string Owner = "system");

    private static ClientSeed ToClientSeed(
        string clientId, string displayName, string persianDisplayName, ClientKind kind,
        ClientFlow flows, string scopesList, string[] redirectUris, string[] postLogoutUris, string? secret)
        => new(clientId, displayName, persianDisplayName, kind, flows,
            string.IsNullOrWhiteSpace(scopesList) ? DefaultScopeList : scopesList,
            redirectUris, postLogoutUris, secret);

    /// <summary>The six integration sample clients of FR-9. Seeded on first run.</summary>
    internal static IEnumerable<ClientSeed> BuildSampleClients(string sampleSecret)
    {
        var allScopes = DefaultScopeList;

        // winforms-sample: the desktop client ALSO demonstrates the password grant
        // ("direct login" mode). It sends its own client_id there — the per-user client
        // whitelist (ClientUserAssignments) only knows real client ids like this one,
        // so firstparty-api would always be rejected for restricted non-admin users.
        var winformsFlows = ClientFlow.AuthorizationCode | ClientFlow.RefreshToken | ClientFlow.Password;

        yield return new ClientSeed(
            "winforms-sample", "WinForms Sample (.NET Framework 4.8.1)", "نمونه وین‌فورمز (دسکتاپ)",
            ClientKind.Confidential, winformsFlows, allScopes,
            ["http://localhost:5100/authorize"], ["http://localhost:5100/authorize"],
            sampleSecret, "کلاینت دسکتاپ وین‌فورمز با مرورگر سیستمی و PKCE (Confidential با Client Secret)", "samples");

        var flows = ClientFlow.AuthorizationCode | ClientFlow.RefreshToken;

        yield return new ClientSeed(
            "aspnetcore-sample", "ASP.NET Core 10 MVC Sample", "نمونه وب ASP.NET Core",
            ClientKind.Confidential, flows, allScopes,
            ["http://localhost:5002/signin-oidc"], ["http://localhost:5002/signout-callback-oidc"],
            sampleSecret, "وب‌اپلیکیشن MVC با OpenIdConnect استاندارد", "samples");

        yield return new ClientSeed(
            "php-sample", "Plain PHP 8 Sample", "نمونه PHP خام",
            ClientKind.Confidential, flows, allScopes,
            ["http://localhost:8000/callback.php"], ["http://localhost:8000/index.php"],
            sampleSecret, "کلاینت PHP بدون فریمورک با PKCE دستی", "samples");

        yield return new ClientSeed(
            "yii-sample", "PHP Yii2 Sample", "نمونه Yii2",
            ClientKind.Confidential, flows, allScopes,
            ["http://localhost:8080/index.php?r=site/callback"], ["http://localhost:8080/index.php?r=site/index"],
            sampleSecret, "کلاینت Yii2 با Auth Code + PKCE", "samples");

        yield return new ClientSeed(
            "spring-sample", "Java Spring Boot 3 Sample", "نمونه Spring Boot",
            ClientKind.Confidential, flows, allScopes,
            ["http://localhost:8090/login/oauth2/code/sso"], ["http://localhost:8090/"],
            sampleSecret, "کلاینت جاوا Spring Boot 3 با discovery", "samples");

        yield return new ClientSeed(
            "js-spa-sample", "Vanilla JS SPA Sample", "نمونه SPA جاوااسکریپت",
            ClientKind.Confidential, flows, allScopes,
            ["http://localhost:5500/callback.html"], ["http://localhost:5500/index.html"],
            sampleSecret, "SPA خالص با Web Crypto PKCE — Confidential با Client Secret (نیازمند CORS)", "samples");
    }
}
