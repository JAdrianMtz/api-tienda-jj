using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Framework Services
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("name=DefaultConnection")
);

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
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error durante la inicialización de la base de datos");
        throw;
    }
}

// Security
app.UseHttpsRedirection();

app.Run();
