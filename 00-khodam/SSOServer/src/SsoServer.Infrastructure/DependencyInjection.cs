using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Options;
using SsoServer.Infrastructure.ActiveDirectory;
using SsoServer.Infrastructure.Audit;
using SsoServer.Infrastructure.OpenIddict;
using SsoServer.Infrastructure.Persistence;
using SsoServer.Infrastructure.Security;
using SsoServer.Infrastructure.Settings;

namespace SsoServer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SsoOptions>(configuration.GetSection("Sso"));
        services.Configure<SecurityOptions>(configuration.GetSection("Security"));
        services.Configure<SeedOptions>(configuration.GetSection("Seed"));
        services.Configure<ActiveDirectorySettings>(configuration.GetSection(ActiveDirectorySettings.SectionName));

        var connectionString = configuration.GetConnectionString("SqlServerExpress")
            ?? throw new InvalidOperationException(
                "رشته اتصال پایگاه داده (ConnectionStrings:SqlServerExpress) در تنظیمات یافت نشد.");

        services.AddDbContext<AppDbContext>(options =>
        {
            // محیط تست/توسعه لینوکس (TestLinux): پایگاه داده SQLite با فایل محلی؛
            // در سایر محیط‌ها SQL Server (رشته اتصال SqlServerExpress).
            if (string.Equals(configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }

            // Registers the OpenIddict entity sets (applications, authorizations, scopes, tokens).
            options.UseOpenIddict();
        });

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<ISigningCertificateProvider, SigningCertificateProvider>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IClientRepository, OidcClientRepository>();
        services.AddScoped<ISessionTokenStore, SessionTokenStore>();
        services.AddScoped<IClientUserAssignmentRepository, ClientUserAssignmentRepository>();
        services.AddScoped<IClientAccessStore, ClientAccessStore>();

        // Active Directory (LDAP) authentication: the factory is stateless, the service
        // is scoped like the other repositories.
        services.AddSingleton<ILdapConnectionFactory, LdapConnectionFactory>();
        services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();

        return services;
    }
}
