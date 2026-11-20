using Microsoft.Extensions.DependencyInjection;
using SsoServer.Application.Services;
using SsoServer.Domain.Interfaces;

namespace SsoServer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Singleton by design: resolves scoped repositories through IServiceScopeFactory.
        services.AddSingleton<IAuditService, AuditService>();

        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<ClientAdminService>();
        services.AddScoped<ClientAssignmentService>();
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }
}
