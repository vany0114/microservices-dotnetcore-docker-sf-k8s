using Duber.Domain.Driver.Persistence;
using Duber.Domain.Driver.Repository;
using Duber.Domain.User.Persistence;
using Duber.Domain.User.Repository;
using Duber.Infrastructure.Resilience.Abstractions;
using Duber.Infrastructure.Resilience.Http;
using Duber.Infrastructure.Resilience.Sql;
using Duber.WebSite.Infrastructure.Persistence;
using Duber.WebSite.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using System.Reflection;
using Duber.Infrastructure.EventBus.RabbitMQ.IoC;
using Duber.Infrastructure.EventBus.ServiceBus.IoC;
using Duber.WebSite.Application.IntegrationEvents.Handlers;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Duber.WebSite.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static string ResiliencePolicy = "ResiliencePolicy";

        public static IServiceCollection AddPersistenceAndRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserContext>(options =>
            {
                options.UseSqlServer(
                    configuration["ConnectionStrings:WebsiteDB"],
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(UserContext).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), null);
                    });
            });

            services.AddDbContext<DriverContext>(options =>
            {
                options.UseSqlServer(
                    configuration["ConnectionStrings:WebsiteDB"],
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(DriverContext).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), null);
                    });
            });

            services.AddDbContext<ReportingContext>(options =>
            {
                options.UseSqlServer(
                    configuration["ConnectionStrings:WebsiteDB"],
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ReportingContext).GetTypeInfo().Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), null);
                    });
            });

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IDriverRepository, DriverRepository>();
            services.AddTransient<IReportingRepository, ReportingRepository>();

            return services;
        }

        public static IServiceCollection AddResilientStrategies(this IServiceCollection services, IConfiguration configuration)
        {
            // Resilient Async SQL Executor configuration.
            services.AddSingleton<IPolicyAsyncExecutor>(sp =>
            {
                var sqlPolicyBuilder = new SqlPolicyBuilder();
                return sqlPolicyBuilder
                    .UseAsyncExecutor()
                    .WithDefaultPolicies()
                    .Build();
            });

            // Resilient Sync SQL Executor configuration.
            services.AddSingleton<IPolicySyncExecutor>(sp =>
            {
                var sqlPolicyBuilder = new SqlPolicyBuilder();
                return sqlPolicyBuilder
                    .UseSyncExecutor()
                    .WithDefaultPolicies()
                    .Build();
            });

            // Create (and register with DI) a policy registry containing some policies we want to use.
            var policyRegistry = services.AddPolicyRegistry();
            policyRegistry[ResiliencePolicy] = GetHttpResiliencePolicy(configuration);

            // Resilient Http Invoker onfiguration.
            // Register a typed client via HttpClientFactory, set to use the policy we placed in the policy registry.
            services.AddHttpClient<ResilientHttpClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(50);
            }).AddPolicyHandlerFromRegistry(ResiliencePolicy);

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetHttpResiliencePolicy(IConfiguration configuration)
        {
            var retryCount = 5;
            if (!string.IsNullOrEmpty(configuration["HttpClientRetryCount"]))
            {
                retryCount = int.Parse(configuration["HttpClientRetryCount"]);
            }

            var exceptionsAllowedBeforeBreaking = 4;
            if (!string.IsNullOrEmpty(configuration["HttpClientExceptionsAllowedBeforeBreaking"]))
            {
                exceptionsAllowedBeforeBreaking = int.Parse(configuration["HttpClientExceptionsAllowedBeforeBreaking"]);
            }

            // Define a couple of policies which will form our resilience strategy.
            var policies = HttpPolicyExtensions.HandleTransientHttpError()
                .RetryAsync(retryCount)
                .WrapAsync(HttpPolicyExtensions.HandleTransientHttpError()
                    .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking, TimeSpan.FromSeconds(5)));

            return policies;
        }

        public static IServiceCollection AddServiceBroker(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddServiceBus(configuration);
            }
            else
            {
                services.AddRabbitMQ(configuration);
            }

            services.AddTransient<TripCreatedIntegrationEventHandler>();
            services.AddTransient<TripUpdatedIntegrationEventHandler>();
            services.AddTransient<InvoiceCreatedIntegrationEventHandler>();
            services.AddTransient<InvoicePaidIntegrationEventHandler>();

            return services;
        }

        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var hcBuilder = services.AddHealthChecks();

            hcBuilder
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddUrlGroup(new Uri($"{configuration["InvoiceApiSettings:BaseUrl"]}/readiness"), name: "invoice-service-check", tags: new string[] { "invoice-service" })
                .AddUrlGroup(new Uri($"{configuration["TripApiSettings:BaseUrl"]}/readiness"), name: "trip-service-check", tags: new string[] { "trip-service" })
                .AddUrlGroup(new Uri($"{configuration["TripApiSettings:NotificationsServerUrl"]}/readiness"), name: "notifications-service-check", tags: new string[] { "notifications-service" })
                .AddSqlServer(
                    configuration["ConnectionStrings:WebsiteDB"],
                    name: "WebsiteDB-check",
                    tags: new string[] { "websitedb" });

            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                hcBuilder.AddAzureServiceBusTopic(configuration, "website-az-servicebus-check");
            }
            else
            {
                hcBuilder.AddRabbitMQ(configuration, "website-rabbitmqbus-check");
            }

            services.AddHealthChecksUI();
            return services;
        }
    }
}
