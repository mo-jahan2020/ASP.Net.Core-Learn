using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.EntityFrameworkCore.Models;

namespace SsoServer.Infrastructure.Persistence;

/// <summary>
/// Creates/migrates the database and runs the idempotent seeder.
/// SQL Server: applies EF Core migrations. Other providers (integration tests): EnsureCreated.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider scopedProvider)
    {
        var context = scopedProvider.GetRequiredService<AppDbContext>();
        var logger = scopedProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("SQL Server database migrations applied");
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database schema created (provider: {Provider})", context.Database.ProviderName);
        }

        // سیاست امنیتی ۲۰۲۶-۱۱-۱۹: کلاینت‌های Public قدیمی (بدون Client Secret) دیگر
        // پذیرفته نمی‌شوند. هر کلاینت Public موجود به Confidential با یک Secret تصادفی
        // قوی ارتقا می‌یابد؛ ادمین باید از صفحه «ویرایش کلاینت → بازتولید Client Secret»
        // secret جدید بگیرد و به برنامه کلاینت بدهد. بدون این کار آن برنامه دیگر
        // نمی‌تواند توکن بگیرد (unauthorized_client).
        await UpgradePublicClientsToConfidentialAsync(context, logger);

        await Seeder.SeedAsync(scopedProvider);
    }

    /// <summary>
    /// Migrates legacy public clients to confidential with a random secret so that
    /// no application can talk to the SSO server without a client secret anymore.
    /// </summary>
    private static async Task UpgradePublicClientsToConfidentialAsync(AppDbContext context, ILogger logger)
    {
        var publicClients = await context.Set<OpenIddictEntityFrameworkCoreApplication>()
            .Where(a => a.ClientType == "public")
            .ToListAsync();

        if (publicClients.Count == 0)
        {
            return;
        }

        foreach (var client in publicClients)
        {
            client.ClientType = "confidential";
            client.ClientSecret = SsoServer.Application.Services.ClientAdminService.GenerateSecret();
            logger.LogWarning(
                "Public client '{ClientId}' upgraded to confidential with a random secret. " +
                "Its old connection will stop working; regenerate the secret from the admin UI and hand it to the application owner.",
                client.ClientId);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("{Count} public client(s) upgraded to confidential", publicClients.Count);
    }
}
