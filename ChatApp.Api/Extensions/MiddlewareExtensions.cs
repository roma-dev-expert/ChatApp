using ChatApp.Api.Middleware;

namespace ChatApp.Api.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
