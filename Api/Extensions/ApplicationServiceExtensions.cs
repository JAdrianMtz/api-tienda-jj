namespace Api.Extensions;

public static class ApplicationServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services, string[] allowedOrigins)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
                builder.WithOrigins(allowedOrigins)    // WithOrigins("https://dominio.com")
                .AllowAnyMethod()           // WithMethods("GET", "POST")
                .AllowAnyHeader()           // WithHeaders("accept", "content-type")
            );
        });
    }
}
