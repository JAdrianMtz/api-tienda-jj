using Api.Configuration;
using Scalar.AspNetCore;

namespace Api.Extensions;

public static class OpenApiDocumentationExtensions
{
    public static void AddApplicationOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi(options =>
        {
            // Configurar autenticación JWT Bearer
            options.UseJwtBearerAuthentication();

            // Configurar información de la API
            options.ConfigureApiInfo(configuration);
        });
    }


    public static IEndpointRouteBuilder UseApplicationOpenApi(this IEndpointRouteBuilder app, IConfiguration configuration)
    {
        var scalarSettings = configuration
            .GetSection(ScalarSettings.Section)
            .Get<ScalarSettings>()!;

        if (!scalarSettings.Enabled)
            return app;

        // Mapear el endpoint OpenAPI
        app.MapOpenApi();

        // Mapear Scalar como interfaz de documentación
        app.MapScalarApiReference(options =>
        {
            options.Title = scalarSettings.Title;
            options.DefaultFonts = false;
        });

        // Redireccionar la raíz a la documentación
        app.MapGet("/", () => Results.Redirect(scalarSettings.RedirectPath))
            .ExcludeFromDescription();

        return app;
    }
}
