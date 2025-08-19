using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace E_Commerce.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // Handle non-success status codes
                if (!context.Response.HasStarted)
                {
                    switch (context.Response.StatusCode)
                    {
                        case 400:
                            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, "Bad Request");
                            break;
                        case 401:
                            await WriteErrorResponseAsync(context, HttpStatusCode.Unauthorized, "Unauthorized");
                            break;
                        case 403:
                            await WriteErrorResponseAsync(context, HttpStatusCode.Forbidden, "Forbidden");
                            break;
                        case 404:
                            await WriteErrorResponseAsync(context, HttpStatusCode.NotFound, "Resource not found");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.", ex.Message);
            }
        }

        private async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, string message, string? details = null)
        {
            if (context.Response.HasStarted) return;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = message,
                Detail = details
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
