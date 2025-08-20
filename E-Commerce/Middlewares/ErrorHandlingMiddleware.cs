using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (!context.Response.HasStarted)
                {
                    await HandleKnownStatusCodesAsync(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                if (!context.Response.HasStarted)
                {
                    await HandleExceptionAsync(context, ex);
                }
                else
                {
                    _logger.LogError("Response has already started, cannot send error response to client");
                }
            }
        }

        private async Task HandleKnownStatusCodesAsync(HttpContext context)
        {
            var statusCode = (HttpStatusCode)context.Response.StatusCode;
            
            if ((int)statusCode >= 400)
            {
                var problem = new ProblemDetails
                {
                    Status = (int)statusCode,
                    Type = $"https://httpstatuses.com/{(int)statusCode}",
                    Title = GetDefaultMessageForStatusCode(statusCode),
                    Instance = context.Request.Path
                };

                await WriteResponseAsync(context, problem, statusCode);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");

            var (statusCode, message) = ex switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access"),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
                DirectoryNotFoundException => (HttpStatusCode.InternalServerError, "File system error occurred"),
                IOException => (HttpStatusCode.InternalServerError, "File system error occurred"),
                DbUpdateException => (HttpStatusCode.BadRequest, "Database error occurred"),
                // Add more exception types as needed
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };

            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Type = $"https://httpstatuses.com/{(int)statusCode}",
                Title = message,
                Instance = context.Request.Path,
                Detail = _env.IsDevelopment() ? ex.ToString() : null
            };

            // Include more detailed information in development
            if (_env.IsDevelopment())
            {
                problem.Extensions["exception"] = new
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    InnerException = ex.InnerException?.Message
                };
            }

            await WriteResponseAsync(context, problem, statusCode);
        }

        private static string GetDefaultMessageForStatusCode(HttpStatusCode statusCode) => statusCode switch
        {
            HttpStatusCode.BadRequest => "The request was malformed or contains invalid parameters",
            HttpStatusCode.Unauthorized => "Authentication is required and has failed or has not been provided",
            HttpStatusCode.Forbidden => "You do not have permission to access this resource",
            HttpStatusCode.NotFound => "The requested resource was not found",
            HttpStatusCode.MethodNotAllowed => "The HTTP method used is not supported for this resource",
            HttpStatusCode.Conflict => "The request conflicts with the current state of the server",
            HttpStatusCode.UnprocessableEntity => "The request was well-formed but contains invalid parameters",
            HttpStatusCode.TooManyRequests => "Too many requests have been made in a given amount of time",
            HttpStatusCode.InternalServerError => "An unexpected error occurred while processing your request",
            _ => "An error occurred while processing your request"
        };

        private static async Task WriteResponseAsync(HttpContext context, ProblemDetails problem, HttpStatusCode statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/problem+json";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }
    }

    // Extension method for easier middleware registration
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
