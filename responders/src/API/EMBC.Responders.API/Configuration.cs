﻿using System;
using EMBC.Responders.API.Services;
using EMBC.Utilities.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace EMBC.Responders.API;

public class Configuration : IConfigureComponentServices, IConfigureComponentPipeline
{
    public void ConfigureServices(ConfigurationServices configurationServices)
    {
        var services = configurationServices.Services;

        services.Configure<OpenApiDocumentMiddlewareSettings>(options =>
        {
            options.Path = "/api/openapi/{documentName}/openapi.json";
            options.DocumentName = "Responders Portal API";
            options.PostProcess = (document, req) =>
            {
                document.Info.Title = "Responders Portal API";
            };
        });

        services.Configure<SwaggerUiSettings>(options =>
        {
            options.Path = "/api/openapi";
            options.DocumentTitle = "responders Portal API Documentation";
            options.DocumentPath = "/api/openapi/{documentName}/openapi.json";
        });

        services.AddOpenApiDocument(document =>
        {
            document.AddSecurity("bearer token", Array.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "paste token here",
                In = OpenApiSecurityApiKeyLocation.Header
            });

            document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer token"));
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                RequireSignedTokens = true,
                RequireAudience = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(60),
                ValidateActor = true,
                ValidateIssuerSigningKey = true,
                NameClaimType = "bceid_username"
            };

            configurationServices.Configuration.GetSection("jwt").Bind(options);

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async c =>
                {
                    var userService = c.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    c.Principal = await userService.GetPrincipal(c.Principal);
                }
            };
            options.Validate();
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
            {
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .RequireClaim("user_role")
                    .RequireClaim("user_team");
            });
            options.DefaultPolicy = options.GetPolicy(JwtBearerDefaults.AuthenticationScheme) ?? null!;
        });

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IEvacuationSearchService, EvacuationSearchService>();
    }

    public void ConfigurePipeline(PipelineServices services)
    {
        var app = services.Application;
        var env = services.Environment;

        if (services.Environment.IsDevelopment())
        {
            IdentityModelEventSource.ShowPII = true;
        }

        if (!env.IsProduction())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
