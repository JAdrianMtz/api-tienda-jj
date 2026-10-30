using Api.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ApplicationMiddlewareExtensions
{
    public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app)
    {
        // Exception Handling
        app.UseMiddleware<ExceptionMiddleware>();

        // HTTP Status Codes
        app.UseStatusCodePages(async statusCodeContext =>
        {
            var response = statusCodeContext.HttpContext.Response;

            var problemDetails = new ProblemDetails
            {
                Status = response.StatusCode,
                Title = GetStatusCodeTitle(response.StatusCode)
            };

            await Results.Problem(problemDetails)
                .ExecuteAsync(statusCodeContext.HttpContext);
        });

        return app;
    }

    private static string GetStatusCodeTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Petición incorrecta",
            401 => "No autenticado",
            403 => "Acceso prohibido",
            404 => "Recurso no encontrado",
            405 => "Método no permitido",
            409 => "Conflicto",
            _ => "Error en la solicitud"
        };
    }
}
