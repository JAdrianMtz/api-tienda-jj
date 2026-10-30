using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Excepción no controlada al procesar la solicitud.");

            // No es posible reemplazar una respuesta que ya comenzó
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();

            // Intentar guardar la excepción sin impedir la respuesta 500
            try
            {
                var dbContext = context.RequestServices
                    .GetRequiredService<ApplicationDbContext>();

                var error = new Error
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    OcurredAtUtc = DateTime.UtcNow
                };

                dbContext.Add(error);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception loggingException)
            {
                _logger.LogError(
                    loggingException,
                    "No se pudo guardar la excepción en la base de datos.");
            }

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Error interno del servidor",
                Detail = "Ha ocurrido un error inesperado."
            };

            await Results.Problem(problemDetails).ExecuteAsync(context);
        }
    }
}
