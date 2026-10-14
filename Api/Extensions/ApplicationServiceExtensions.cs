using Core.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;

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

    public static void AddApplicationServices(this IServiceCollection services)
    {
        //services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        //services.AddScoped<IBrandRepository, BrandRepository>();
        //services.AddScoped<ICategoryRepository, CategoryRepository>();
        //services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
