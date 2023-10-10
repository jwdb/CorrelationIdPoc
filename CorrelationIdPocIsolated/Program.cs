using Azure.Monitor.OpenTelemetry.AspNetCore;
using CorrelationIdPocIsolated;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;

Sdk.SetDefaultTextMapPropagator(new CommercetoolsPropagator());

var host = new HostBuilder()
    .ConfigureServices((_, services) =>
    {
        services
            .AddHttpClient()
            .Configure<WorkerOptions>(workerOptions => workerOptions.Capabilities["WorkerApplicationInsightsLoggingEnabled"] = bool.TrueString);

        services
            .AddOpenTelemetry()
            .UseAzureMonitor();
    })
    .ConfigureAppConfiguration(configure =>
    {
        configure.AddJsonFile("appsettings.json");
    })
    .ConfigureFunctionsWebApplication(configure =>
    {
        configure.UseWhen<RequestMiddleware>(c =>
        {
            return c.FunctionDefinition.InputBindings.Values
                .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";
        });
    })
    .Build();

host.Run();