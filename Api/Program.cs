using Api.Extensions;
using AspNetCoreRateLimit;
using AutoMapper.Internal;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Framework Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplicationAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddAuthorization();

// Options
builder.Services.AddApplicationOptions();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("name=DefaultConnection")
);

// CORS
builder.Services.AddApplicationCors(builder.Configuration);

// Versioning
builder.Services.AddApplicationApiVersioning();

// External libraries
builder.Services.AddApplicationAutoMapper();
builder.Services.AddApplicationRateLimiting();

// Application Services
builder.Services.AddApplicationServices();

// Logging
builder.Logging.AddApplicationSerilog(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Migrations and Seeder
{
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var seedLogger = services.GetRequiredService<ILogger<DataSeeder>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        await DataSeeder.SeedAsync(context, seedLogger);
        await DataSeeder.SeedRolesAsync(context, seedLogger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error durante la inicialización de la base de datos");
        throw;
    }
}

// Security
app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseCors("DefaultPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();

app.Run();
