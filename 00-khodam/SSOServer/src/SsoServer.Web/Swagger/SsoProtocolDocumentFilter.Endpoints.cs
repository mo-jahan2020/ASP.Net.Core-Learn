using Microsoft.OpenApi;

namespace SsoServer.Web.Swagger;

// Endpoint builders for the machine-facing OAuth2/OIDC and health paths.
// Part of SsoProtocolDocumentFilter — see the main file for the rationale.
public sealed partial class SsoProtocolDocumentFilter
{
    // ---------------------------------------------------------------- token (login)

    private static OpenApiPathItem ConnectToken()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Object };
        schema.Required ??= new HashSet<string>();
        schema.Required.Add("grant_type");
        schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
        schema.Properties["grant_type"] = EnumSchema("نوع اعطا", "password", "client_credentials", "authorization_code", "refresh_token");
        schema.Properties["client_id"] = ExampleSchema("شناسه کلاینت", "firstparty-api");
        schema.Properties["client_secret"] = StringSchema("رمز کلاینت محرمانه - از appsettings.json (بخش Seed:FirstPartyClients)؛ همان مقداری که سرور با آن کلاینت را ثبت کرده است.");
        schema.Properties["username"] = ExampleSchema("نام کاربری (grant_type=password)", "admin");
        schema.Properties["password"] = StringSchema("رمز عبور کاربر (grant_type=password)؛ برای کاربران اکتیو دایرکتوری، رمز دامنه.");
        schema.Properties["scope"] = ExampleSchema("دامنه‌های درخواستی؛ برای endpointهای مدیریتی حتماً «api» لازم است", "openid profile roles email api offline_access");
        schema.Properties["code"] = StringSchema("کد مجوز دریافتی از authorize (grant_type=authorization_code)");
        schema.Properties["code_verifier"] = StringSchema("اثبات‌کننده PKCE (grant_type=authorization_code)");
        schema.Properties["refresh_token"] = StringSchema("توکن تمدید (grant_type=refresh_token)");

        var body = new OpenApiRequestBody
        {
            Required = true,
            Description = "پارامترها به صورت application/x-www-form-urlencoded ارسال می‌شوند (یا در هدر Authorization: Basic برای کلاینت‌های محرمانه).",
        };
        body.Content ??= new Dictionary<string, OpenApiMediaType>();
        body.Content["application/x-www-form-urlencoded"] = new OpenApiMediaType { Schema = schema };

        var responses = new OpenApiResponses();
        responses["200"] = JsonOk("توکن صادر شد.", SchemaRef("TokenResponse"));
        responses["400"] = JsonOk("خطای اعطا (invalid_grant / invalid_client / unsupported_grant_type) همراه با error_description فارسی.", SchemaRef("TokenError"));

        return NewPathItem((HttpMethod.Post, WithProtocolTag(new OpenApiOperation
        {
            OperationId = "connectToken",
            Summary = "ورود (Login) و صدور توکن",
            Description = "نقطه ورود ماشینی سامانه و تنها راه Login برای API. grant_type=password همان ورود با نام کاربری/رمز است (کاربران اکتیو دایرکتوری نیز از همین مسیر با رمز دامنه احراز می‌شوند)؛ refresh_token توکن را تمدید می‌کند؛ authorization_code جریان کد را کامل می‌کند و client_credentials برای سرویس‌های بدون کاربر است. پاسخ موفق، یک JSON حاوی access_token است. در Swagger UI معمولاً نیازی به اجرای دستی این endpoint نیست: دکمه «Authorize» بالای صفحه فرم ورود دارد و خودش از همین endpoint توکن می‌گیرد؛ برای مصرف خارج از Swagger (curl و...) خروجی همین عملیات را در پنجره Authorize پیست کنید.",
            RequestBody = body,
            Responses = responses,
            Security = [new OpenApiSecurityRequirement()],
        })));
    }

    // ---------------------------------------------------------------- userinfo

    private static OpenApiPathItem ConnectUserinfo()
    {
        OpenApiOperation Build(string operationId) => WithProtocolTag(new()
        {
            OperationId = operationId,
            Summary = "اطلاعات کاربر (UserInfo)",
            Description = "claims کاربرِ احراز هویت‌شده (نام، ایمیل، نقش‌ها و claims سفارشی) را برمی‌گرداند. نیازمند توکن Bearer با دامنه openid (و برای claims بیشتر، profile/roles/email).",
            // No per-operation security override: this endpoint is bearer-protected
            // and inherits the document-wide "Bearer" requirement.
            Responses = UserinfoResponses(),
        });

        return NewPathItem((HttpMethod.Get, Build("connectUserinfo")), (HttpMethod.Post, Build("connectUserinfoPost")));
    }

    private static OpenApiResponses UserinfoResponses()
    {
        var claimsSchema = new OpenApiSchema { Type = JsonSchemaType.Object };
        claimsSchema.AdditionalProperties = new OpenApiSchema { Type = JsonSchemaType.String };

        var responses = new OpenApiResponses();
        responses["200"] = JsonOk("claims کاربر به صورت JSON.", claimsSchema);
        responses["401"] = new OpenApiResponse { Description = "توکن نامعتبر یا دامنه openid وجود ندارد." };
        return responses;
    }

    // ---------------------------------------------------------------- introspection / revocation

    private static OpenApiPathItem ConnectIntrospect()
    {
        var responses = new OpenApiResponses();
        responses["200"] = JsonOk("نتیجه استعلام.", SchemaRef("IntrospectionResponse"));
        responses["401"] = new OpenApiResponse { Description = "کلاینت مجاز به استعلام نیست." };

        return NewPathItem((HttpMethod.Post, ProtocolOperation(
            "connectIntrospect",
            "استعلام اعتبار توکن",
            "برای سرویس‌های داخلی (resource serverها): وضعیت active و متادیتای یک access/refresh token را برمی‌گرداند. احراز هویت کلاینت با client_id/client_secret در بدنه یا هدر Basic انجام می‌شود.",
            requestBody: FormBody(
                ("token", "توکن مورد استعلام", true),
                ("token_type_hint", "نوع احتمالی توکن", false),
                ("client_id", "شناسه کلاینت مجاز", false),
                ("client_secret", "رمز کلاینت", false)),
            responses: responses)));
    }

    private static OpenApiPathItem ConnectRevoke()
    {
        var responses = new OpenApiResponses();
        responses["200"] = new OpenApiResponse { Description = "توکن باطل شد (یا از قبل باطل بود)." };
        responses["401"] = new OpenApiResponse { Description = "کلاینت مجاز به ابطال نیست." };

        return NewPathItem((HttpMethod.Post, ProtocolOperation(
            "connectRevoke",
            "ابطال توکن (Revoke)",
            "یک access/refresh token را باطل می‌کند. عملیات idempotent است: همیشه 200 برمی‌گردد، حتی اگر توکن معتبر نباشد (RFC 7009). احراز هویت کلاینت با client_id/client_secret در بدنه یا هدر Basic.",
            requestBody: FormBody(
                ("token", "توکنی که باید باطل شود", true),
                ("token_type_hint", "نوع احتمالی توکن", false),
                ("client_id", "شناسه کلاینت مجاز", false),
                ("client_secret", "رمز کلاینت", false)),
            responses: responses)));
    }

    // ---------------------------------------------------------------- basic health

    private static OpenApiPathItem BasicHealth()
    {
        var responses = new OpenApiResponses();
        responses["200"] = TextPlain("سامانه سالم است.");
        responses["503"] = TextPlain("سامانه ناسالم است (پایگاه داده یا گواهی امضا در دسترس نیست).");

        var operation = new OpenApiOperation
        {
            OperationId = "healthLiveness",
            Summary = "سلامت پایه (Liveness)",
            Description = "بدون احراز هویت: فقط وضعیت کلی را برمی‌گرداند (۲۰۰ = سالم، ۵۰۳ = ناسالم) و برای مانیتورینگ و بالانس‌ها مناسب است. گزارش تفصیلی با احراز هویت مدیر در GET /api/v1/health/detail موجود است.",
            Responses = responses,
            Security = [new OpenApiSecurityRequirement()],
        };
        operation.Tags ??= new HashSet<OpenApiTagReference>();
        operation.Tags.Add(new OpenApiTagReference(HealthTag));

        return NewPathItem((HttpMethod.Get, operation));
    }
}
