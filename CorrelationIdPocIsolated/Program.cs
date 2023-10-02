using CorrelationIdPocIsolated;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(configure => configure.UseWhen<RequestMiddleware>(context =>
    {
        return context.FunctionDefinition.InputBindings.Values
            .First(a => a.Type.EndsWith("Trigger")).Type == "httpTrigger";

    }))
    .Build();

host.Run();
