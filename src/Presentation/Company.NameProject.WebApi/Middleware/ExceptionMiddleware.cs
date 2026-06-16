using Company.NameProject.Domain.Common;
using Company.NameProject.Shared.Exceptions;

using FluentValidation;

using Microsoft.Extensions.Logging;

namespace Company.NameProject.WebApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                await HandleExceptionAsync(context, ex.Message, 400);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                await HandleExceptionAsync(context, errors, 400);
            }
            catch (ApiException ex)
            {
                await HandleExceptionAsync(context, ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, "Error interno del servidor.", 500);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, object message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            ApiResponse<object> response = message is List<string> errors
                ? ApiResponse<object>.Fail(errors, statusCode)
                : ApiResponse<object>.Fail(message.ToString()!, statusCode);

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}

