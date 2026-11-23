using System.Text.Json;

namespace Magazing.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var statusCode = ex switch
            {
                _ when IsDatabaseUnavailable(ex) => StatusCodes.Status503ServiceUnavailable,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "Error no controlado al procesar la solicitud.");
            }

            var message = statusCode switch
            {
                StatusCodes.Status503ServiceUnavailable => "La base de datos no está disponible en este momento. Intente nuevamente.",
                StatusCodes.Status500InternalServerError => "Ocurrió un error inesperado.",
                _ => ex.Message
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new { error = message });
            await context.Response.WriteAsync(body);
        }
    }

    private static bool IsDatabaseUnavailable(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TimeoutException or System.Data.Common.DbException { IsTransient: true } or System.Net.Sockets.SocketException)
            {
                return true;
            }
        }

        return false;
    }
}
