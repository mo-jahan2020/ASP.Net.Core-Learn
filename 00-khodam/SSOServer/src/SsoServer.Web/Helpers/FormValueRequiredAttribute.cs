using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace SsoServer.Web.Helpers;

/// <summary>Selects an action only when the submitted form contains the given button name.</summary>
public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
{
    private readonly string _name;

    public FormValueRequiredAttribute(string name) => _name = name;

    public override bool IsValidForRequest(RouteContext context, ActionDescriptor action)
    {
        if (context.HttpContext.Request.Method is "GET" or "HEAD" or "DELETE" or "TRACE")
        {
            return false;
        }

        if (string.IsNullOrEmpty(context.HttpContext.Request.ContentType) ||
            !context.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.IsNullOrEmpty(context.HttpContext.Request.Form[_name]);
    }
}
