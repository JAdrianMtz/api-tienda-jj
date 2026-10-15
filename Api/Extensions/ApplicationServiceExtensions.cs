using AutoMapper.Internal;
using Core.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;

namespace Api.Extensions;

public static class ApplicationServiceExtensions
{
    public static void AddApplicationCors(this IServiceCollection services, string[] allowedOrigins)
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

    public static void AddApplicationRepositories(this IServiceCollection services)
    {
        //services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        //services.AddScoped<IBrandRepository, BrandRepository>();
        //services.AddScoped<ICategoryRepository, CategoryRepository>();
        //services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    public static void AddApplicationAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(config =>
        {
            config.AddMaps(typeof(Program));
            config.Internal().ForAllMaps((typeMap, expression) =>
            {
                expression.MaxDepth(32);
            });
        });
    }
}
