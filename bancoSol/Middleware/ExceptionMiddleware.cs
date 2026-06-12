using System.Net;
using System.Text.Json;
using bancoSol.Exceptions;

namespace bancoSol.Middleware
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
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context, Exception ex, ILogger logger)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = ex switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,       
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,      
                ConflictException => (int)HttpStatusCode.Conflict,         
                _ => (int)HttpStatusCode.InternalServerError               
            };

            if (context.Response.StatusCode == 500)
                logger.LogError(ex, "Error inesperado");
            else
                logger.LogWarning(ex, "Error controlado: {Message}", ex.Message);

            var response = new
            {
                message = ex.Message,
                status = context.Response.StatusCode
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}