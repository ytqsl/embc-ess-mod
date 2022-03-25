﻿using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EMBC.Utilities.Configuration;
using EMBC.Utilities.Extensions;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Grpc.Net.Client.Configuration;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EMBC.Utilities.Messaging
{
    public class Configuration : IConfigureComponentServices, IHaveGrpcServices, IConfigureComponentPipeline
    {
        public void ConfigureServices(ConfigurationServices configurationServices)
        {
            var options = configurationServices.Configuration.GetSection("messaging").Get<MessagingOptions>() ?? new MessagingOptions() { Mode = MessagingMode.Server };

            configurationServices.Services.AddGrpc(opts =>
            {
                opts.EnableDetailedErrors = configurationServices.Environment.IsDevelopment();
            });
            if (options.Mode == MessagingMode.Server || options.Mode == MessagingMode.Both)
            {
                configurationServices.Services.Configure<MessageHandlerRegistryOptions>(opts => { });
                configurationServices.Services.AddSingleton<MessageHandlerRegistry>();
                configurationServices.Services.AddAuthentication(opts =>
                {
                    opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
                {
                    configurationServices.Configuration.GetSection("auth:jwt").Bind(opts);

                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = true,
                        RequireSignedTokens = true,
                        RequireAudience = false,
                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(60),
                        NameClaimType = ClaimTypes.Upn,
                        RoleClaimType = ClaimTypes.Role,
                        ValidateActor = true,
                        ValidateIssuerSigningKey = true,
                    };
                    opts.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = async c =>
                        {
                            await Task.CompletedTask;
                            var logger = c.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
                            logger.LogError(c.Exception, $"Error authenticating token");
                        },
                        OnTokenValidated = async c =>
                        {
                            await Task.CompletedTask;
                            var logger = c.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("JwtBearer");
                            logger.LogDebug("Token validated for {0}", c.Principal?.Identity?.Name);
                        }
                    };
                    opts.Validate();
                });
                configurationServices.Services.AddAuthorization(options =>
                {
                    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                    {
                        policy
                        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .RequireScope("ess-backend")
                        ;
                    });

                    options.DefaultPolicy = options.GetPolicy(JwtBearerDefaults.AuthenticationScheme) ?? null!;
                });
            }

            if (options.Mode == MessagingMode.Client || options.Mode == MessagingMode.Both)
            {
                configurationServices.Services.TryAddSingleton<ResolverFactory>(new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(15)));
                configurationServices.Services.TryAddSingleton<LoadBalancerFactory, RoundRobinBalancerFactory>();
                configurationServices.Services.TryAddTransient<AuthInterceptor>();
                if (options.Url == null) throw new Exception($"Messaging url is missing - can't configure messaging client");
                configurationServices.Services.AddGrpcClient<Dispatcher.DispatcherClient>((sp, opts) =>
                {
                    opts.Address = options.Url;
                }).ConfigurePrimaryHttpMessageHandler(sp =>
                {
                    var handler = new SocketsHttpHandler()
                    {
                        EnableMultipleHttp2Connections = true,
                        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                        PooledConnectionLifetime = TimeSpan.FromSeconds(20),
                        KeepAlivePingDelay = TimeSpan.FromSeconds(20),
                        KeepAlivePingTimeout = TimeSpan.FromSeconds(20),
                        KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests
                    };
                    if (options.AllowInvalidServerCertificate)
                    {
                        handler.SslOptions = new SslClientAuthenticationOptions { RemoteCertificateValidationCallback = DangerousCertificationValidation };
                    }
                    return handler;
                }).ConfigureChannel(opts =>
                {
                    if (options.Url.Scheme == "dns")
                    {
                        opts.Credentials = ChannelCredentials.SecureSsl;
                    }
                    opts.ServiceConfig = new ServiceConfig
                    {
                        LoadBalancingConfigs = { new RoundRobinConfig() },
                        MethodConfigs =
                        {
                            new MethodConfig
                            {
                                RetryPolicy = new RetryPolicy
                                {
                                    MaxAttempts = 5,
                                    InitialBackoff = TimeSpan.FromSeconds(1),
                                    MaxBackoff = TimeSpan.FromSeconds(5),
                                    BackoffMultiplier = 1.5,
                                    RetryableStatusCodes = { StatusCode.Unavailable }
                                }
                            }
                        }
                    };
                }).AddInterceptor<AuthInterceptor>(InterceptorScope.Client)
                .EnableCallContextPropagation(opts => opts.SuppressContextNotFoundErrors = true);

                configurationServices.Services
                    .AddTransient<ITokenProvider, OAuthTokenProvider>()
                    .AddHttpClient("messaging_token").SetHandlerLifetime(TimeSpan.FromMinutes(30));

                configurationServices.Services.AddTransient<IMessagingClient, MessagingClient>();

                if (options.Mode == MessagingMode.Client)
                {
                    configurationServices.Services.AddTransient<IVersionInformationProvider, VersionInformationProvider>();
                }
            }
        }

        public void ConfigurePipeline(PipelineServices services)
        {
            var options = services.Configuration.GetSection("messaging").Get<MessagingOptions>() ?? new MessagingOptions() { Mode = MessagingMode.Server };
            if (options.Mode == MessagingMode.Server || options.Mode == MessagingMode.Both)
            {
                services.Application.UseAuthentication();
                services.Application.UseAuthorization();
            }
        }

        private static bool DangerousCertificationValidation(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public Type[] GetGrpcServiceTypes()
        {
            return new[] { typeof(DispatcherService) };
        }
    }

    public class MessagingOptions
    {
        public Uri? Url { get; set; }

        public bool AllowInvalidServerCertificate { get; set; } = false;
        public MessagingMode Mode { get; set; } = MessagingMode.Both;
        public OAuthOptions? OAuth { get; set; }
    }

    public class OAuthOptions
    {
        public Uri Url { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string Scope { get; set; } = "openid ess-backend";
    }

    public enum MessagingMode
    {
        Both,
        Client,
        Server,
    }
}
