using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Options;

namespace SsoServer.Infrastructure.Security;

/// <summary>
/// Loads the RS256 signing certificate. Falls back to generating a self-signed
/// certificate on first run (persisted next to the app so restarts keep the same key).
/// Only in Development can an ephemeral key be used when the file cannot be written.
/// </summary>
public sealed class SigningCertificateProvider : ISigningCertificateProvider
{
    private readonly Lazy<(X509Certificate2? Certificate, SigningCertificateInfo Info)> _lazy;

    public SigningCertificateProvider(IWebHostEnvironment environment, IOptions<SsoOptions> options, ILogger<SigningCertificateProvider> logger)
    {
        _lazy = new Lazy<(X509Certificate2?, SigningCertificateInfo)>(() => Load(environment, options.Value, logger));
    }

    public X509Certificate2? GetSigningCertificate() => _lazy.Value.Certificate;

    public SigningCertificateInfo Describe() => _lazy.Value.Info;

    private static (X509Certificate2? Certificate, SigningCertificateInfo Info) Load(
        IWebHostEnvironment environment, SsoOptions options, ILogger logger)
    {
        var path = ResolvePath(environment, options.SigningCertificate.Path);
        var configuredPassword = options.SigningCertificate.Password;
        var passwordIsConfigured = !string.IsNullOrEmpty(configuredPassword)
            && configuredPassword != SigningCertificateOptions.NotSetValue;

        if (File.Exists(path))
        {
            var password = passwordIsConfigured ? configuredPassword! : TryReadSidecarPassword(path);
            try
            {
                var certificate = X509CertificateLoader.LoadPkcs12FromFile(path, password);
                logger.LogInformation("Loaded signing certificate from {Path} (subject: {Subject})", path, certificate.Subject);
                var status = certificate.NotAfter - DateTimeOffset.UtcNow < TimeSpan.FromDays(30)
                    ? SigningKeyStatus.NearExpiry
                    : SigningKeyStatus.FileLoaded;
                return (certificate, new SigningCertificateInfo(status, certificate.Subject, certificate.NotAfter, path));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not load the signing certificate at {Path}; falling back to a generated certificate", path);
            }
        }

        // First run (or unreadable file): generate a self-signed certificate and persist it.
        var generatedPassword = passwordIsConfigured ? configuredPassword! : Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=SsoServer Signing, O=OnPremise", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var certificate2 = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10));
        var persisted = X509CertificateLoader.LoadPkcs12(
            certificate2.Export(X509ContentType.Pfx, generatedPassword),
            generatedPassword,
            X509KeyStorageFlags.EphemeralKeySet);

        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, certificate2.Export(X509ContentType.Pfx, generatedPassword));
            if (!passwordIsConfigured)
            {
                // Dev convenience: remember the generated password so the next start can reload the same key.
                File.WriteAllText(path + ".pwd", generatedPassword);
                logger.LogWarning("Generated a self-signed signing certificate at {Path}. For production, replace it with a real certificate and configure its password via environment (see README).", path);
            }
            else
            {
                logger.LogInformation("Generated a self-signed signing certificate at {Path}", path);
            }

            return (persisted, new SigningCertificateInfo(SigningKeyStatus.Generated, persisted.Subject, persisted.NotAfter, path));
        }
        catch (Exception ex)
        {
            if (environment.IsDevelopment())
            {
                logger.LogWarning(ex, "Could not persist the generated signing certificate; using an ephemeral key (tokens break on restart).");
                return (persisted, new SigningCertificateInfo(SigningKeyStatus.Ephemeral, persisted.Subject, persisted.NotAfter, "ephemeral"));
            }

            throw new InvalidOperationException(
                $"امکان بارگذاری یا ساخت گواهی امضا در «{path}» وجود ندارد. مسیر فایل را بررسی کنید یا رمز گواهی را از طریق متغیر محیطی Sso__SigningCertificate__Password تنظیم کنید. " +
                "(Could not load or create the signing certificate.)", ex);
        }
    }

    private static string ResolvePath(IWebHostEnvironment environment, string configured)
        => Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured);

    private static string? TryReadSidecarPassword(string certificatePath)
    {
        var sidecar = certificatePath + ".pwd";
        return File.Exists(sidecar) ? File.ReadAllText(sidecar).Trim() : null;
    }
}
