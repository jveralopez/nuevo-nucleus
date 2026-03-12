using Microsoft.AspNetCore.Http;

namespace PortalBffService.Services;

public static class RequestUser
{
    public static string GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst("sub") ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return claim?.Value ?? "unknown";
    }
}
