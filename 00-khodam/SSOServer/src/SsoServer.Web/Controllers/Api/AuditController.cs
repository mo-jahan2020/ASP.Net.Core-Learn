using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SsoServer.Application.Services;
using SsoServer.Domain.Enums;
using SsoServer.Domain.Interfaces;
using SsoServer.Web.Helpers;

namespace SsoServer.Web.Controllers.Api;

/// <summary>
/// گزارش رویدادهای امنیتی (Audit): معادل API صفحه «گزارش رویدادها» در پنل مدیریت وب.
/// صفحه‌بندی و فیلترها دقیقاً همان معنایی صفحه وب را دارد.
/// </summary>
[Authorize(Policy = SsoPolicies.Admin)]
public class AuditController : SsoApiController
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    private readonly IAuditService _audit;

    public AuditController(IAuditService audit)
    {
        _audit = audit;
    }

    /// <summary>فهرست رویدادهای امنیتی با صفحه‌بندی و فیلتر اختیاری نوع رویداد/کاربر.</summary>
    /// <param name="page">شماره صفحه (از 1).</param>
    /// <param name="pageSize">تعداد ردیف در هر صفحه (1 تا 200، پیش‌فرض 50، هماهنگ با صفحه وب).</param>
    /// <param name="action">فیلتر نوع رویداد (اختیاری).</param>
    /// <param name="userId">فیلتر بر اساس شناسه کاربر (اختیاری).</param>
    [HttpGet("audit")]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] AuditAction? action = null,
        [FromQuery] string? userId = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));

        var entries = await _audit.QueryAsync((page - 1) * pageSize, pageSize, action, userId);

        return Ok(entries.Select(e => new
        {
            e.Id,
            action = e.Action.ToString(),
            actorUserId = e.ActorUserId,
            actorUserName = e.ActorUserName,
            clientId = e.ClientId,
            ip = e.Ip,
            data = SafeJson(e.DataJson),
            createdAt = e.CreatedAt,
        }));
    }

    /// <summary>seeder برای بارهای خالی «{}» می‌نویسد؛ در پاسخ API به‌صورت شیء JSON برگردانده می‌شود.</summary>
    private static JsonNode SafeJson(string dataJson)
    {
        try
        {
            return JsonSerializer.Deserialize<JsonNode>(dataJson ?? "{}") ?? new JsonObject();
        }
        catch (JsonException)
        {
            return new JsonObject();
        }
    }
}
