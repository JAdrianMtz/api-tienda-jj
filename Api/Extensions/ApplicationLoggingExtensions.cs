using Serilog;

namespace Api.Extensions;

public static class ApplicationLoggingExtensions
{
    public static void AddApplicationSerilog(this ILoggingBuilder logging, IConfiguration configuration)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        logging.ClearProviders();
        logging.AddSerilog(logger);
    }
}
