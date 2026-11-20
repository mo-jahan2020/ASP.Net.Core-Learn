using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using Serilog;
using Serilog.Events;
using SsoServer.Application;
using SsoServer.Application.Services;
using SsoServer.Application.Tokens;
using SsoServer.Domain.Entities;
using SsoServer.Domain.Interfaces;
using SsoServer.Domain.Models;
using SsoServer.Domain.Options;
using SsoServer.Infrastructure;
using SsoServer.Infrastructure.Persistence;
using SsoServer.Infrastructure.Security;
using SsoServer.Web.HealthChecks;
using SsoServer.Web.Helpers;
using SsoServer.Web.Middleware;
using SsoServer.Web.OpenIddict;
using SsoServer.Web.Services;
using SsoServer.Web.Services.HotReload;
using SsoServer.Web.Services.RateLimiting;
using SsoServer.Web.Swagger;
using static OpenIddict.Abstractions.OpenIddictConstants;

// Serilog bootstrap: console + rolling file with correlation-friendly output.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}({SourceContext})")
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "logs", "ssoserver-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting SsoServer");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ----- Layers -----
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplicationServices();

    // Options bound once for program-level decisions (issuer, lifetimes, certificate).
    var ssoOptions = builder.Configuration.GetSection("Sso").Get<SsoOptions>() ?? new SsoOptions();
    var lifetimes = TokenLifetimeMapper.Map(ssoOptions);

    // The signing certificate is resolved once at startup so the OpenIddict server
    // and the health check/validators all share the exact same key material.
    var certificateProvider = new SigningCertificateProvider(
        builder.Environment,
        Microsoft.Extensions.Options.Options.Create(ssoOptions),
        Microsoft.Extensions.Logging.Abstractions.NullLogger<SigningCertificateProvider>.Instance);
    var signingCertificate = certificateProvider.GetSigningCertificate()
        ?? throw new InvalidOperationException("گواهی امضای توکن در دسترس نیست؛ بخش Sso:SigningCertificate را بررسی کنید.");
    builder.Services.AddSingleton<ISigningCertificateProvider>(certificateProvider);

    // بروزرسانی امنیتی ۲۰۲۶-۱۱-۱۹: رمزنگاری نامتقارن رمز عبور (RSA-OAEP) — کلید عمومی
    // از /connect/crypto/key (با جفت client_id + client_secret معتبر — اصلاح endpoint)،
    // رمزگشایی فقط سمت سرور (keys/password-encryption.key).
    builder.Services.AddSingleton<SsoServer.Infrastructure.Security.PasswordEncryptionService>();

    // Hot-reloadable settings (singleton on purpose, see class remarks).
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ISettingsService, SettingsService>();

    // ----- ASP.NET Core Identity cookie UX -----
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "SsoServer.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(Math.Max(1, ssoOptions.SsoSessionCookieHours));
        options.SlidingExpiration = true;
    });

    // Sliding expiration is hot-reloadable: recompute cookie options when settings change.
    builder.Services.AddSingleton<IConfigureNamedOptions<CookieAuthenticationOptions>, ConfigureSsoCookieOptions>();

    // SSO شفاف ویندوزی (بند ۳): Negotiate «بدون» ثبت در pipeline احراز هویت پیاده شده —
    // WindowsAuthController خودش با NegotiateAuthentication (GSSAPI/SSPI) token را پردازش
    // می‌کند و چالش مرورگر را دستی (401 + WWW-Authenticate) برمی‌گرداند. این‌طوری هیچ
    // middlewareای روی مسیرهای دیگر (مثل /connect/token روی TestServer) دخالتی نمی‌کند.
    builder.Services.AddSingleton<IOptionsChangeTokenSource<CookieAuthenticationOptions>, SettingsCookieChangeTokenSource>();

    // Lockout policy defaults at startup; hot values are applied by SettingsHotReloadService.
    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.Lockout.MaxFailedAccessAttempts = Math.Max(1, builder.Configuration.GetSection("Security").Get<SecurityOptions>()?.MaxFailedLoginAttempts ?? 5);
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(Math.Max(1, builder.Configuration.GetSection("Security").Get<SecurityOptions>()?.LockoutMinutes ?? 15));
    });

    // ----- OpenIddict: core (EF stores) + server + validation -----
    builder.Services.AddOpenIddict()

        .AddCore(options =>
        {
            options.UseEntityFrameworkCore()
                   .UseDbContext<AppDbContext>();
        })

        .AddServer(options =>
        {
            options.SetAuthorizationEndpointUris("connect/authorize")
                   .SetTokenEndpointUris("connect/token")
                   .SetUserInfoEndpointUris("connect/userinfo")
                   .SetIntrospectionEndpointUris("connect/introspect")
                   .SetRevocationEndpointUris("connect/revoke")
                   .SetEndSessionEndpointUris("connect/logout");

            options.AllowAuthorizationCodeFlow()
                   .AllowRefreshTokenFlow()
                   .AllowPasswordFlow()
                   .AllowClientCredentialsFlow();

            options.RegisterScopes(Scopes.OpenId, Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OfflineAccess, SsoScopes.Api);

            options.SetIssuer(new Uri(ssoOptions.IssuerUri));
            options.SetAccessTokenLifetime(lifetimes.AccessToken);
            options.SetIdentityTokenLifetime(lifetimes.IdentityToken);
            options.SetRefreshTokenLifetime(lifetimes.RefreshToken);
            // Strict rotation: a used refresh token is rejected immediately (no reuse grace window).
            options.SetRefreshTokenReuseLeeway(TimeSpan.Zero);
            if (!lifetimes.SlidingRefresh)
            {
                options.DisableSlidingRefreshTokenExpiration();
            }

            // Access tokens stay standard, self-contained RS256 JWTs (decodable offline by any JWT library).
            options.DisableAccessTokenEncryption();

            options.AddSigningCertificate(signingCertificate);
            options.AddEncryptionCertificate(signingCertificate);

            options.UseAspNetCore()
                   // The prompt locks the issuer to http://localhost:5001; HTTPS is strongly
                   // recommended for production (see README) but must not be forced here.
                   .DisableTransportSecurityRequirement()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableEndSessionEndpointPassthrough()
                   .EnableTokenEndpointPassthrough()
                   .EnableUserInfoEndpointPassthrough()
                   .EnableStatusCodePagesIntegration();
        })

        .AddValidation(options =>
        {
            // The management API validates tokens issued by this very server, fully offline.
            options.UseLocalServer();
            options.UseAspNetCore();

            // Enforce immediate revocation for self-contained JWTs by checking the
            // authorization/token entries still exist and are valid in the database.
            options.EnableAuthorizationEntryValidation();
            options.EnableTokenEntryValidation();
        });

    // ----- MVC + API -----
    builder.Services.AddControllersWithViews(options =>
    {
        options.MaxModelValidationErrors = 50;
    });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Title = "درخواست نامعتبر است.",
                Detail = "یک یا چند فیلد واردشده معتبر نیستند.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path,
            };
            return new BadRequestObjectResult(problemDetails) { ContentTypes = { "application/problem+json" } };
        };
    });

    // ----- Authorization policies -----
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(SsoPolicies.ApiScope, policy => policy.RequireAssertion(context =>
            context.User.HasScope(SsoScopes.Api)));

        options.AddPolicy(SsoPolicies.Admin, policy => policy.RequireAssertion(context =>
            context.User.HasScope(SsoScopes.Api)
            && (context.User.HasClaim(Claims.Role, SsoRoles.Admin) || context.User.IsInRole(SsoRoles.Admin))));
    });

    // ----- CORS (origins hot-reloadable from the settings store) -----
    builder.Services.AddCors();
    builder.Services.AddSingleton<ICorsPolicyProvider, DynamicCorsPolicyProvider>();

    // ----- Rate limiting (limits hot-reloadable from the settings store) -----
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = static async (context, cancellationToken) =>
        {
            context.HttpContext.Response.ContentType = "application/problem+json";
            await context.HttpContext.Response.WriteAsync(
                """{"title":"تعداد درخواست‌ها بیش از حد مجاز است.","detail":"چند لحظه دیگر دوباره تلاش کنید.","status":429}""",
                cancellationToken);
        };

        options.AddPolicy(RateLimitPolicies.Auth, new SsoRateLimitPolicy(RateLimitPolicies.Auth));
        options.AddPolicy(RateLimitPolicies.Token, new SsoRateLimitPolicy(RateLimitPolicies.Token));
        options.AddPolicy(RateLimitPolicies.Api, new SsoRateLimitPolicy(RateLimitPolicies.Api));
    });

    // ----- Swagger for the management API -----
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "SsoServer API",
            Version = "v1",
            Description = "API سرور احراز هویت یکپارچه (SSO).\n\nورود و گرفتن توکن: دکمه «Authorize» بالای صفحه → در پنجره بازشده Username/Password و ClientId/ClientSecret را وارد کنید، همه دامنه‌ها را تیک بزنید (حداقل api) و Login بزنید — Swagger خودش از /connect/token توکن می‌گیرد و روی همه درخواست‌ها می‌گذارد. (راه دستی: POST /connect/token را اجرا و access_token را در همان پنجره پیست کنید.)\n\nدو بخش دارد:\n• احراز هویت و توکن (connect/token، connect/userinfo، introspect و revoke) — ماشینی و JSON.\n• API مدیریت (api/v1/...) — نیازمند توکن با دامنه api.\n\nendpointهای مرورگری (فرم HTML ورود/Authorize/خروج) مخصوص نسخه وب‌اند و عمداً در این سند نیستند.",
        });

        // XML summaries of the management controllers feed the Swagger descriptions.
        var xmlPath = Path.Combine(AppContext.BaseDirectory, "SsoServer.Web.xml");
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }

        // The OAuth2/OIDC protocol endpoints (connect/*), the browser login page and
        // the basic /health endpoint are implemented by OpenIddict/MVC and stay hidden
        // from the ApiExplorer; this filter documents them by hand.
        options.DocumentFilter<SsoProtocolDocumentFilter>();

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "توکن دسترسی صادرشده توسط SSO — روش دستی: access_token را همین‌جا پیست کنید. روش راحت‌تر، همان فرم بالایی (oauth2) است.",
        });

        // Interactive login for Swagger UI: the Authorize dialog renders
        // username/password (password grant) fields and posts them straight to
        // /connect/token, so no token has to be fetched by hand.
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "ورود تعاملی Swagger (Password Grant): در پنجره Authorize، Username و Password (به‌علاوه ClientId و ClientSecret) را وارد کنید، دامنه‌ها را تیک بزنید و Login بزنید تا توکن گرفته و روی همه درخواست‌ها اعمال شود. برای endpointهای مدیریتی (api/v1/...) دامنه api الزامی است.",
            Flows = new OpenApiOAuthFlows
            {
                Password = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri("/connect/token", UriKind.Relative),
                    Scopes = new Dictionary<string, string>
                    {
                        ["openid"] = "احراز هویت OIDC",
                        ["profile"] = "نام نمایشی و مشخصات کاربر",
                        ["roles"] = "نقش‌های کاربر",
                        ["email"] = "ایمیل کاربر",
                        ["api"] = "دسترسی به API مدیریت (برای api/v1/... الزامی است)",
                        ["offline_access"] = "توکن تمدید (refresh_token)",
                    },
                },
            },
        });

        // Either scheme unlocks the protected endpoints (OR): "oauth2" = interactive
        // login via the Authorize dialog; "Bearer" = paste an existing token by hand.
        // NOTE: the scheme references must be created with the host document so they
        // resolve against components - unresolved references serialize as an empty
        // object {} and the Swagger UI would show no locks at all.
        options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("oauth2", doc, null)] = [],
            [new OpenApiSecuritySchemeReference("Bearer", doc, null)] = [],
        });
    });

    // ----- Health checks + background services -----
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database")
        .AddCheck<SigningKeyHealthCheck>("signing-key");

    builder.Services.AddHostedService<SettingsHotReloadService>();
    builder.Services.AddSingleton<TokenPruningStatus>();
    builder.Services.AddHostedService<TokenPruningService>();

    var app = builder.Build();

    // ----- Database init + seeding (EF design-time tooling aborts before this point) -----
    using (var scope = app.Services.CreateAsyncScope())
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider);

        var settings = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        await settings.GetEffectiveAsync();
    }

    // ----- Middleware pipeline -----
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<DisabledClientMiddleware>();

    app.UseStatusCodePages((Func<StatusCodeContext, Task>)StatusCodeHandler);

    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("sso");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "SsoServer API";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SsoServer API v1");
    });

    app.MapControllers();
    app.MapDefaultControllerRoute();
    app.MapHealthChecks("/health");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SsoServer terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

    static async Task StatusCodeHandler(StatusCodeContext context)
    {
        var http = context.HttpContext;
        var path = http.Request.Path;
        if (path.StartsWithSegments("/api") || path.StartsWithSegments("/health"))
        {
            http.Response.ContentType = "application/problem+json";
            await http.Response.WriteAsync(
                $$"""{"title":"درخواست یافت نشد یا دسترسی ندارید.","status":{{(int)http.Response.StatusCode}}}""",
                http.RequestAborted);
            return;
        }

        // MVC pages: a small friendly page.
        await context.Next(http);
        if (!http.Response.HasStarted)
        {
            http.Response.ContentType = "text/html; charset=utf-8";
            await http.Response.WriteAsync(
                "<html dir=\"rtl\" lang=\"fa\"><body style=\"font-family:Tahoma;text-align:center;padding-top:60px\"><h2>درخواست یافت نشد یا خطایی رخ داد.</h2><a href=\"/\">بازگشت به صفحه اصلی</a></body></html>",
                http.RequestAborted);
        }
    }

/// <summary>Marker class for the integration test host factory.</summary>
public partial class Program;
