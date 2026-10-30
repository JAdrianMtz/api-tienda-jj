using Api.Helpers;
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
                Title = StatusCodeTitles.GetTitle(response.StatusCode)
            };

            await Results.Problem(problemDetails)
                .ExecuteAsync(statusCodeContext.HttpContext);
        });

        return app;
    }
}
