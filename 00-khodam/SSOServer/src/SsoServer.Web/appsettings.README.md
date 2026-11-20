# راهنمای appsettings.json — پیکربندی SsoServer
# | Configuration guide for SsoServer (appsettings.json)

## ConnectionStrings:SqlServerExpress
رشته‌ی اتصال SQL Server Express با احراز ویندوز. پایگاه داده (SsoServerDb) با
دستور «dotnet ef database update» یا خودکار در اولین اجرا ساخته می‌شود.

## Sso (بارگذاری هنگام راه‌اندازی — تغییر = ری‌استارت)
- IssuerUri: نشانی صادرکننده؛ باید دقیقاً همان نشانی‌ای باشد که کلاینت‌ها
  در «Authority» می‌گذارند (مثلاً http://localhost:5001).
- SigningCertificate: مسیر PFX گواهی امضای توکن‌ها (RS256) و رمز آن.
  مقدار «SET_VIA_ENVIRONMENT» یعنی رمز را از متغیر محیطی
  Sso__SigningCertificate__Password بدهید. اگر فایل نباشد، در اولین اجرا
  خودکار self-signed ساخته و ذخیره می‌شود.
- AccessTokenLifetimeMinutes / IdentityTokenLifetimeMinutes /
  RefreshTokenLifetimeDays / RefreshTokenAbsoluteLifetimeDays:
  عمر توکن‌ها (توکن دسترسی ۶۰ دقیقه، id_token ۲۰ دقیقه، refresh ۱۴ روز با
  سقف مطلق ۹۰ روز).
- UseSlidingRefreshTokens: با هر استفاده، refresh token تمدید شود (تا سقف مطلق).
- RequireConsentByDefault: نمایش صفحه‌ی رضایت برای کلاینت‌هایی که تنظیم
  اختصاصی ندارند (false = بدون پرسش).
- SsoSessionCookieHours: عمر کوکی نشست SSO — «یک بار ورود» تا همین زمان
  معتبر می‌ماند (پیش‌فرض ۱۶۸ ساعت = یک هفته).

## Security (مقادیر اولیه؛ نسخه‌ی قابل‌تغییر در پنل مدیریت ذخیره می‌شود)
- MaxFailedLoginAttempts / LockoutMinutes: قفل حساب پس از تلاش‌های ناموفق.
- TokenRateLimitPerMinute: سقف درخواست‌های ورود/توکن/API در دقیقه.
- CorsOrigins: ریشه‌های مجاز پیش‌فرض (در پنل قابل ویرایش) — برای SPA لازم است.

## ActiveDirectory (ادغام LDAP)
- Enabled: کلید اصلی ورود AD.
- Host/Port/UseSsl: نشانی دایرکتوری (ldap:// یا ldaps:// هم در Host پذیرفته می‌شود).
- AuthType: Negotiate (ویندوز دامنه‌ای) یا Basic (bind ساده — برای سرورهای
  تست مثل ldap.forumsys.com).
- Username/Password: حساب سرویس برای «جست‌وجوی» کاربران.
- SearchBase: محدوده‌ی جست‌وجو (مثلاً dc=example,dc=com).
- UserSearchFilter: فیلتر سفارشی؛ خالی = پیش‌فرض
  (|(sAMAccountName={0})(userPrincipalName={0})(uid={0})).

## Seed (فقط اولین اجرا)
- AdminUsername/AdminPassword: حساب مدیر پیش‌فرض — رمز را فوراً عوض کنید!
- SampleClientSecret: رمز dev کلاینت‌های نمونه (sample-dev-secret).
- FirstPartyClients: کلاینت‌های سازمانی خودتان — Flows با کاما:
  authorization_code_pkce, refresh_token, password, client_credentials.
  کلاینت بدون ClientSecret = عمومی (Public).

نکته‌ی امنیتی: در production مقدارهای حساس (رمز گواهی، رمز مدیر، رمز
کلاینت‌ها) را با متغیرهای محیطی بدهید (الگوی Sso__SigningCertificate__Password)،
نه داخل فایل.
