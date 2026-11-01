using Api.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Api.Extensions;

public static class OpenApiTransformersExtensions
{
    public static OpenApiOptions UseJwtBearerAuthentication(this OpenApiOptions options)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingrese el token JWT."
        };

        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, scheme);
            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;

            var requiresAuthorization =
                metadata.OfType<IAuthorizeData>().Any() &&
                !metadata.OfType<IAllowAnonymous>().Any();

            if (requiresAuthorization)
            {
                var scheme = new OpenApiSecuritySchemeReference(
                    JwtBearerDefaults.AuthenticationScheme);

                operation.Security =
                [
                    new()
                    {
                        [scheme] = []
                    }
                ];
            }

            return Task.CompletedTask;
        });

        return options;
    }

    public static OpenApiOptions ConfigureApiInfo(this OpenApiOptions options, IConfiguration configuration)
    {
        var scalarSettings = configuration
            .GetSection(ScalarSettings.Section)
            .Get<ScalarSettings>()!;

        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = scalarSettings.Title,
                Version = scalarSettings.Version,
                Description = scalarSettings.Description,

                Contact = new OpenApiContact
                {
                    Name = scalarSettings.ContactName,
                    Email = scalarSettings.ContactEmail,
                    Url = string.IsNullOrWhiteSpace(scalarSettings.ContactUrl)
                        ? null
                        : new Uri(scalarSettings.ContactUrl)
                },

                License = new OpenApiLicense
                {
                    Name = scalarSettings.LicenseName,
                    Url = string.IsNullOrWhiteSpace(scalarSettings.LicenseUrl)
                        ? null
                        : new Uri(scalarSettings.LicenseUrl)
                }
            };

            return Task.CompletedTask;
        });

        return options;
    }
}
