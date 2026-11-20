using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace SsoServer.Web.Swagger;

// Shared component schemas, tag descriptions and the small schema builders.
// Part of SsoProtocolDocumentFilter — see the main file for the rationale.
public sealed partial class SsoProtocolDocumentFilter
{
    // ---------------------------------------------------------------- shared schemas

    private static void AddSharedSchemas(OpenApiDocument swaggerDoc)
    {
        swaggerDoc.Components ??= new OpenApiComponents();
        swaggerDoc.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        var schemas = swaggerDoc.Components.Schemas;

        if (!schemas.ContainsKey("TokenResponse"))
        {
            var schema = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "پاسخ موفق صدور توکن (refresh_token فقط در جریان‌های کاربردار با دامنه offline_access صادر می‌شود).",
            };
            schema.Required ??= new HashSet<string>();
            schema.Required.Add("access_token");
            schema.Required.Add("token_type");
            schema.Required.Add("expires_in");
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
            schema.Properties["access_token"] = StringSchema("توکن دسترسی JWT (RS256)");
            schema.Properties["token_type"] = StringSchema("نوع توکن");
            schema.Properties["expires_in"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Description = "ثانیه‌های باقی‌مانده اعتبار." };
            schema.Properties["refresh_token"] = StringSchema("توکن تمدید (فقط با دامنه offline_access)");
            schema.Properties["scope"] = StringSchema("دامنه‌های نهایی اعطاشده");
            schema.Properties["id_token"] = StringSchema("توکن هویت OIDC (در جریان‌های openid)");
            schemas["TokenResponse"] = schema;
        }

        if (!schemas.ContainsKey("TokenError"))
        {
            var schema = new OpenApiSchema { Type = JsonSchemaType.Object };
            schema.Required ??= new HashSet<string>();
            schema.Required.Add("error");
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
            schema.Properties["error"] = EnumSchema("کد خطای OAuth2", "invalid_grant", "invalid_client", "unsupported_grant_type");
            schema.Properties["error_description"] = StringSchema("توضیح فارسی خطا");
            schemas["TokenError"] = schema;
        }

        if (!schemas.ContainsKey("IntrospectionResponse"))
        {
            var schema = new OpenApiSchema { Type = JsonSchemaType.Object };
            schema.Required ??= new HashSet<string>();
            schema.Required.Add("active");
            schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
            schema.Properties["active"] = new OpenApiSchema { Type = JsonSchemaType.Boolean, Description = "آیا توکن هنوز معتبر است." };
            schema.Properties["scope"] = StringSchema("دامنه‌های توکن");
            schema.Properties["client_id"] = StringSchema("کلاینت صادرکننده");
            schema.Properties["token_type"] = StringSchema("نوع توکن");
            schema.Properties["exp"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Description = "زمان انقضا (Unix epoch)." };
            schema.Properties["iat"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Description = "زمان صدور (Unix epoch)." };
            schema.Properties["sub"] = StringSchema("شناسه کاربر");
            schemas["IntrospectionResponse"] = schema;
        }
    }

    private static void DescribeTags(OpenApiDocument swaggerDoc)
    {
        swaggerDoc.Tags ??= new HashSet<OpenApiTag>();
        var tags = swaggerDoc.Tags;
        var byName = new Dictionary<string, OpenApiTag>();
        foreach (var tag in tags)
        {
            byName[tag.Name] = tag;
        }

        foreach (var (name, description) in new (string, string)[]
                 {
                     (ProtocolTag, "پروتکل ورود و توکن (OAuth2 / OpenID Connect): endpointهای ماشینیِ JSON - صدور توکن (ورود/Login)، اطلاعات کاربر، استعلام و ابطال. فرم‌های HTML ورود/Authorize/خروج مخصوص نسخه وب‌اند و در API جایی ندارند؛ ورود تعاملی Swagger از پنجره Authorize (جریان oauth2/password) انجام می‌شود."),
                     (HealthTag, "سلامت سامانه: بررسی زنده پایگاه داده، گواهی امضا و اکتیو دایرکتوری."),
                     ("Audit", "گزارش رویدادهای امنیتی (معادل API صفحه گزارش رویدادها)."),
                     ("Clients", "مدیریت کلاینت‌ها (برنامه‌های متصل) و تخصیص کاربران."),
                     ("Users", "مدیریت کاربران، نقش‌ها، claims و وضعیت حساب."),
                     ("Sessions", "نشست‌های فعال: مشاهده و ابطال."),
                     ("Settings", "تنظیمات سامانه و مقادیر مؤثر."),
                     ("Me", "اطلاعات و عملیات حساب شخصیِ صاحب توکن."),
                 })
        {
            if (!byName.TryGetValue(name, out var tag))
            {
                tag = new OpenApiTag { Name = name };
                tags.Add(tag);
                byName[name] = tag;
            }

            tag.Description = description;
        }
    }

    // ---------------------------------------------------------------- small builders

    private static OpenApiParameter Query(string name, string? description = null, bool required = false) => new()
    {
        Name = name,
        In = ParameterLocation.Query,
        Required = required,
        Description = description ?? string.Empty,
        Schema = new OpenApiSchema { Type = JsonSchemaType.String },
    };

    private static OpenApiSchema StringSchema(string description) => new()
    {
        Type = JsonSchemaType.String,
        Description = description,
    };

    /// <summary>String schema with a prefilled "example" for Swagger UI's Try-it-out.</summary>
    private static OpenApiSchema ExampleSchema(string description, string example) => new()
    {
        Type = JsonSchemaType.String,
        Description = description,
        Example = JsonValue.Create(example),
    };

    private static OpenApiSchema EnumSchema(string description, params string[] values)
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.String, Description = description };
        schema.Enum ??= new List<JsonNode>();
        foreach (var value in values)
        {
            schema.Enum.Add(JsonValue.Create(value));
        }

        return schema;
    }

    private static OpenApiSchemaReference SchemaRef(string id) => new(id);

    private static OpenApiResponse JsonOk(string description, IOpenApiSchema schema)
    {
        var response = new OpenApiResponse { Description = description };
        response.Content ??= new Dictionary<string, OpenApiMediaType>();
        response.Content["application/json"] = new OpenApiMediaType { Schema = schema };
        return response;
    }

    private static OpenApiResponse TextPlain(string description)
    {
        var response = new OpenApiResponse { Description = description };
        response.Content ??= new Dictionary<string, OpenApiMediaType>();
        response.Content["text/plain"] = new OpenApiMediaType { Schema = new OpenApiSchema { Type = JsonSchemaType.String } };
        return response;
    }

    /// <summary>application/x-www-form-urlencoded body from (name, description, required) tuples.</summary>
    private static OpenApiRequestBody FormBody(params (string Name, string Description, bool Required)[] fields)
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Object };
        schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
        schema.Required ??= new HashSet<string>();
        foreach (var (name, description, required) in fields)
        {
            schema.Properties[name] = StringSchema(description);
            if (required)
            {
                schema.Required.Add(name);
            }
        }

        var body = new OpenApiRequestBody { Required = true };
        body.Content ??= new Dictionary<string, OpenApiMediaType>();
        body.Content["application/x-www-form-urlencoded"] = new OpenApiMediaType { Schema = schema };
        return body;
    }
}
