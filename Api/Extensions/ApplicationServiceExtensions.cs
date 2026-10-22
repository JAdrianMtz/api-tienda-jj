using Api.Helpers;
using Api.Services;
using Asp.Versioning;
using AspNetCoreRateLimit;
using AutoMapper.Internal;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api.Extensions;

public static class ApplicationServiceExtensions
{
    public static void AddApplicationCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? [];

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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
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

    public static void AddApplicationOptions(this IServiceCollection services) 
    {
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    public static void AddApplicationAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration
            .GetSection(JwtSettings.Section)
            .Get<JwtSettings>()!;

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // false en desarrollo; true para producción
                options.UseSecurityTokenValidators = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                    ),

                    ClockSkew = TimeSpan.Zero
                };
            });
    }
}
