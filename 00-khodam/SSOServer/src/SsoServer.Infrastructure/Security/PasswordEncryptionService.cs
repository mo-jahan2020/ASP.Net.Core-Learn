using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;

namespace SsoServer.Infrastructure.Security;

/// <summary>
/// رمزنگاری نامتقارن رمز عبور کاربران برای انتقال امن روی شبکه‌ی بدون HTTPS.
/// بروزرسانی امنیتی ۲۰۲۶-۱۱-۱۹ (بند ۲):
/// - کلاینت‌ها رمز عبور را با «کلید عمومی» این سرویس (RSA-OAEP-SHA256) رمز می‌کنند
///   و فقط به‌صورت Base64 در پارامتر «password_cipher» می‌فرستند.
/// - فقط سرور کلید خصوصی را دارد؛ شنودکننده‌ی شبکه حتی با دیدن کل درخواست،
///   رمز عبور را به دست نمی‌آورد (برخلاف رمزنگاری متقارن با کلید مشتق از
///   ClientSecret که خودش در همان درخواست ارسال می‌شود و فایده‌ای ندارد).
/// - کلید در اولین اجرا ساخته و در پوشه‌ی keys/ کنار برنامه ذخیره می‌شود
///   (همان الگوی SigningCertificateProvider) تا با ری‌استارت ثابت بماند.
/// نکته: استفاده از TLS در محیط تولید همچنان توصیه اکید است — این لایه،
/// محافظت اضافی برای شبکه‌های داخلی بدون HTTPS است.
/// | Asymmetric RSA-OAEP password encryption for plain-HTTP LANs: the client encrypts
/// the password with the server's public key; only the server can decrypt it.
/// </summary>
public sealed class PasswordEncryptionService
{
    private const int RsaKeySize = 2048;

    private readonly object _initLock = new();
    private RSA _rsa;
    private readonly string _keyPath;

    public PasswordEncryptionService(IWebHostEnvironment environment)
    {
        var dir = Path.Combine(environment.ContentRootPath, "keys");
        Directory.CreateDirectory(dir);
        _keyPath = Path.Combine(dir, "password-encryption.key");
        _rsa = LoadOrCreate();
    }

    /// <summary>کلید عمومی به فرمت Base64 (SPKI) — کلاینت با آن رمز می‌کند.</summary>
    public string GetPublicKeyBase64()
    {
        lock (_initLock)
        {
            return Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo());
        }
    }

    /// <summary>
    /// رمزگشایی رمز عبور ارسالی توسط کلاینت. خروجی null یعنی ورودی معتبر نبود
    /// (Base64 خراب، طول غلط، یا کلید هم‌خوان نیست) — فراخواننده باید مثل
    /// «رمز اشتباه» رفتار کند تا جزئیات فنی لو نرود.
    /// </summary>
    public string? Decrypt(string cipherBase64)
    {
        if (string.IsNullOrWhiteSpace(cipherBase64))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(cipherBase64.Trim());
            if (bytes.Length == 0 || bytes.Length > RsaKeySize / 8)
            {
                return null;
            }

            var plain = _rsa.Decrypt(bytes, RSAEncryptionPadding.OaepSHA256);
            return System.Text.Encoding.UTF8.GetString(plain);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    /// <summary>ابزار داخلی: رمز کردن متنی با کلید عمومی (برای تست‌های دوطرفه).</summary>
    public string EncryptForTests(string plaintext)
    {
        using var pub = RSA.Create();
        pub.ImportSubjectPublicKeyInfo(Convert.FromBase64String(GetPublicKeyBase64()), out _);
        var bytes = pub.Encrypt(System.Text.Encoding.UTF8.GetBytes(plaintext), RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(bytes);
    }

    private RSA LoadOrCreate()
    {
        var rsa = RSA.Create(RsaKeySize);
        try
        {
            if (File.Exists(_keyPath))
            {
                rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(File.ReadAllText(_keyPath).Trim()), out _);
                return rsa;
            }

            var pem = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            File.WriteAllText(_keyPath, pem);
            return rsa;
        }
        catch
        {
            // کلید فایل خراب/ناخواناست؛ کلید تازه بساز و همان را ذخیره کن.
            var fresh = RSA.Create(RsaKeySize);
            try
            {
                File.WriteAllText(_keyPath, Convert.ToBase64String(fresh.ExportPkcs8PrivateKey()));
            }
            catch
            {
                // اگر نوشتن ممکن نبود (فقط‌خواندنی)، با کلید درون‌حافظه‌ای ادامه بده؛
                // ری‌استارت بعدی کلید تازه می‌سازد و کلاینت‌ها کلید عمومی را تازه می‌خوانند.
            }

            return fresh;
        }
    }
}
