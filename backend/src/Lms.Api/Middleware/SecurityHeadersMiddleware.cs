namespace Lms.Api.Middleware;

public static class SecurityHeadersMiddleware
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["X-XSS-Protection"] = "0";
            headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none'";
            await next();
        });
}
