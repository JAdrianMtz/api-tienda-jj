using Asp.Versioning;
using AspNetCoreRateLimit;
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

    public static void AddApplicationRateLimiting(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddInMemoryRateLimiting();

        services.Configure<IpRateLimitOptions>(options =>
        {
            options.EnableEndpointRateLimiting = true;
            options.StackBlockedRequests = false;
            options.HttpStatusCode = 429;

            options.QuotaExceededMessage =
                "Has excedido el límite de solicitudes. Intenta más tarde.";

            options.GeneralRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Period = "10s",
                    Limit = 5
                }
            };
        });
    }

    public static void AddApplicationApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            //options.ApiVersionReader = new UrlSegmentApiVersionReader();
            //options.ApiVersionReader = new QueryStringApiVersionReader("v");
            //options.ApiVersionReader = new HeaderApiVersionReader("X-Version");
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("v"),
                new HeaderApiVersionReader("X-Version")
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

    }
}
