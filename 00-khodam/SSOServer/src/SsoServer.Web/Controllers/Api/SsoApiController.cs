using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using SsoServer.Web.Helpers;

namespace SsoServer.Web.Controllers.Api;

/// <summary>کلاس پایهٔ endpointهای مدیریت با حفاظت JWT (نیازمند دامنهٔ api).</summary>
/// کلاس پایه همه APIهای مدیریت (فلو ۶):
/// احراز هویت با توکن OpenIddict + الزام scope سفارشی api (SsoPolicies.ApiScope).
/// همه کنترلرهای api/v1 از این کلاس ارث می‌برند و خطاهای فارسی مشترک دارند.
[ApiController]
[Route("api/v1")]
[Authorize(
    AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
    Policy = SsoPolicies.ApiScope)]
public abstract class SsoApiController : ControllerBase
{
    protected ObjectResult PersianProblem(int statusCode, string detail) =>
        Problem(detail: detail, statusCode: statusCode, title: "درخواست ناموفق بود.") as ObjectResult
            ?? new ObjectResult(new ProblemDetails { Status = statusCode, Title = "درخواست ناموفق بود.", Detail = detail }) { StatusCode = statusCode };
}
