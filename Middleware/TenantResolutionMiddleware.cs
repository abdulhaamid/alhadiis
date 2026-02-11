using Alhadis.Models;

namespace Alhadis.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        var tenantQuery = context.Request.Query["tenantId"].FirstOrDefault();

        if (int.TryParse(tenantHeader, out var headerTenantId) && headerTenantId > 0)
        {
            tenantContext.TenantId = headerTenantId;
        }
        else if (int.TryParse(tenantQuery, out var queryTenantId) && queryTenantId > 0)
        {
            tenantContext.TenantId = queryTenantId;
        }

        context.Items["TenantId"] = tenantContext.TenantId;
        await _next(context);
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
