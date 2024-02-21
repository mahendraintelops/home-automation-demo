using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Infrastructure.Extensions
{
    public static class OpenTelemetryRegistration
    {
        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, string serviceName)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation())
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation());
            return services;
        }

        public static ILoggingBuilder AddCustomOpenTelemetryLogging(this ILoggingBuilder builder, string serviceName)
        {
            return builder.AddOpenTelemetry(options =>
            {
                options
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName));
            });
        }

        public static IServiceCollection AddHoneycomb(this IServiceCollection services, IConfiguration configuration)
        {
            var honeycombOptions = configuration.GetHoneycombOptions();

            // Setup OpenTelemetry Tracing
            services.AddOpenTelemetry().WithTracing(otelBuilder =>
            {
                otelBuilder
                    .AddHoneycomb(honeycombOptions)
                    .AddCommonInstrumentations();
            });

            // Register Tracer so it can be injected into other components (eg Controllers)
            services.AddSingleton(TracerProvider.Default.GetTracer(honeycombOptions.ServiceName));

            return services;
        }
    }
}
