using Car_Configuration.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace Car_Configuration.Middleware
{
    public class UseExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public UseExceptionHandlerMiddleware(RequestDelegate next, ILogger<UseExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (NotFoundException e)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "text/html";

                await httpContext.Response.WriteAsync("<html><body>");
                await httpContext.Response.WriteAsync($"We're sorry, an error occurred.{e.Message}. ");
                await httpContext.Response.WriteAsync("</body></html>");
            }
            catch (Exception e)
            {

                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "text/html";

                await httpContext.Response.WriteAsync("<html><body>");
                await httpContext.Response.WriteAsync("We're sorry, an error occurred. Please try again later.");
                await httpContext.Response.WriteAsync("</body></html>");

                _logger.LogError($"Message: {e.Message}, StackTrace: {e.StackTrace}", "Internal error");
                return;
            }
        }

    }

    public static class UseExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder,string? error)
        {
            return builder.UseMiddleware<UseExceptionHandlerMiddleware>();
        }
    }
}
