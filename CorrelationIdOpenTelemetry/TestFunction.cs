using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CorrelationIdOpenTelemetry;

public class TestFunction
{
    private static readonly ActivitySource Activity = new(nameof(CorrelationIdOpenTelemetry), "1.0.0");

    [FunctionName("TestFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");
        using var activity = Activity.StartActivity("Process Message", ActivityKind.Consumer);

        string responseMessage = $"This HTTP triggered function executed successfully. traceId: {activity.TraceId}";

        return new TelemetredObjectResult(responseMessage, activity.Context);
    }
}