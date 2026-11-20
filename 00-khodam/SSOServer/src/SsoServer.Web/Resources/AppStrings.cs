namespace SsoServer.Web.Resources;

/// <summary>
/// All Persian UI strings in a single resources file (offline, no satellite assemblies needed).
/// </summary>
public static class AppStrings
{
    public const string AppName = "سرور ورود یکپارچه (SSO)";
    public const string FooterText = "سامانه احراز هویت یکپارچه سازمانی — تمام دارایی‌ها به‌صورت آفلاین میزبانی می‌شوند.";

    public const string NavProfile = "پروفایل من";
    public const string NavSessions = "نشست‌های فعال من";
    public const string NavDashboard = "داشبورد";
    public const string NavUsers = "کاربران";
    public const string NavClients = "کلاینت‌ها";
    public const string NavSettings = "تنظیمات";
    public const string NavAudit = "گزارش رویدادها";
    public const string NavHealth = "سلامت سامانه";
    public const string NavLogout = "خروج";
    public const string NavManageSessions = "مدیریت نشست‌های فعال";

    public const string LoginTitle = "ورود به سامانه";
    public const string LoginSubtitle = "با یک بار ورود، به همه برنامه‌های سازمان دسترسی خواهید داشت.";
    public const string LoginUsername = "نام کاربری";
    public const string LoginPassword = "رمز عبور";
    public const string LoginRememberMe = "مرا به خاطر بسپار";
    public const string LoginButton = "ورود";

    public const string BtnSave = "ذخیره";
    public const string BtnCreate = "ایجاد";
    public const string BtnEdit = "ویرایش";
    public const string BtnDelete = "حذف";
    public const string BtnCancel = "انصراف";
    public const string BtnBack = "بازگشت";
    public const string BtnEnable = "فعال‌سازی";
    public const string BtnDisable = "غیرفعال‌سازی";
    public const string BtnRevoke = "ابطال";
    public const string BtnConfirm = "تأیید";

    public const string TableAction = "عملیات";
    public const string TableCreated = "تاریخ ایجاد";
    public const string TableStatus = "وضعیت";
    public const string StatusActive = "فعال";
    public const string StatusInactive = "غیرفعال";

    public const string ScopeOpenId = "احراز هویت (openid)";
    public const string ScopeProfile = "اطلاعات پروفایل (profile)";
    public const string ScopeRoles = "نقش‌ها (roles)";
    public const string ScopeEmail = "ایمیل (email)";
    public const string ScopeApi = "دسترسی به API (api)";
    public const string ScopeOfflineAccess = "ورود خودکار (offline_access)";

    public const string ConsentHeader = "درخواست دسترسی برنامه";
    public const string ConsentIntro = "برنامه";
    public const string ConsentWants = "می‌خواهد به اطلاعات زیر دسترسی داشته باشد:";
    public const string ConsentAccept = "اجازه می‌دهم";
    public const string ConsentDeny = "اجازه نمی‌دهم";

    public const string LogoutHeader = "خروج از حساب کاربری";
    public const string LogoutIntro = "با خروج، نشست ورود یکپارچه شما در این مرورگر پایان می‌یابد و برای ورود به برنامه‌ها باید دوباره رمز خود را وارد کنید.";
    public const string LogoutConfirm = "خروج از همه جای سامانه";
    public const string LogoutKeep = "ماندن";

    public const string SessionsHeader = "نشست‌های فعال من";
    public const string SessionsEmpty = "نشست فعالی وجود ندارد.";
    public const string SessionClient = "برنامه";
    public const string SessionCreated = "شروع نشست";
    public const string SessionLastUsed = "آخرین استفاده";
    public const string SessionExpires = "انقضا";
    public const string SessionType = "نوع";
    public const string SessionTypePermanent = "ورود تعاملی";
    public const string SessionTypeAdHoc = "ورود مستقیم API";
    public const string RevokeAllSessions = "خروج از همه دستگاه‌ها";

    public const string FlowAuthorizationCode = "Authorization Code + PKCE";
    public const string FlowRefreshToken = "Refresh Token";
    public const string FlowPassword = "Password (قدیمی)";
    public const string FlowClientCredentials = "Client Credentials";

    public const string KindPublic = "Public (بدون Secret)";
    public const string KindConfidential = "Confidential (با Secret)";

    public const string RestartRequiredBadge = "نیازمند ری‌استارت";
}
