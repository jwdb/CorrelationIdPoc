using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using OpenTelemetry;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;

[assembly: FunctionsStartup(typeof(CorrelationIdOpenTelemetry.Startup))]
namespace CorrelationIdOpenTelemetry
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            Sdk.SetDefaultTextMapPropagator(new CommercetoolsPropagator());
#pragma warning disable CS0618 // Type or member is obsolete
            builder.Services.AddSingleton<IFunctionFilter, CorrelationIdFilterAttribute>();
#pragma warning restore CS0618 // Type or member is obsolete
            // Enable Logging with OpenTelemetry
            builder.Services
                .AddSingleton<TextMapPropagator, CommercetoolsPropagator>()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddOpenTelemetry();
                })
                .AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddSource(nameof(CorrelationIdOpenTelemetry), nameof(Microsoft.Azure))
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .ConfigureResource(r => r.AddService(nameof(CorrelationIdOpenTelemetry)))
                        .AddConsoleExporter();
                });
        }
    }

}
