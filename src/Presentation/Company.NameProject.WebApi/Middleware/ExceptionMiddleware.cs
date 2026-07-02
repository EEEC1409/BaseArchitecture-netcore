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
            var token = context.Response.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? context.TraceIdentifier;
            var metodo = $"{context.Request.Method} {context.Request.Path}";
            const string capa = "Presentation";

            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Excepcion de dominio. Token: {Token}. TipoTransaccion: {TipoTransaccion}. Metodo: {Metodo}. Capa: {Capa}. Mensaje: {Mensaje}",
                    token,
                    "WAR",
                    metodo,
                    capa,
                    ex.Message);

                await HandleExceptionAsync(context, ex.Message, 400);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();

                _logger.LogWarning(
                    ex,
                    "Excepcion de validacion. Token: {Token}. TipoTransaccion: {TipoTransaccion}. Metodo: {Metodo}. Capa: {Capa}. Mensaje: {Mensaje}",
                    token,
                    "WAR",
                    metodo,
                    capa,
                    string.Join(" | ", errors));

                await HandleExceptionAsync(context, errors, 400);
            }
            catch (ApiException ex)
            {
                var tipoTransaccion = ex.StatusCode >= 500 ? "ERROR" : "WAR";

                _logger.Log(
                    ex.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning,
                    ex,
                    "Excepcion de aplicacion. Token: {Token}. TipoTransaccion: {TipoTransaccion}. Metodo: {Metodo}. Capa: {Capa}. Mensaje: {Mensaje}",
                    token,
                    tipoTransaccion,
                    metodo,
                    capa,
                    ex.Message);

                await HandleExceptionAsync(context, ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Excepcion no controlada. Token: {Token}. TipoTransaccion: {TipoTransaccion}. Metodo: {Metodo}. Capa: {Capa}. Mensaje: {Mensaje}",
                    token,
                    "ERROR",
                    metodo,
                    capa,
                    ex.Message);

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

