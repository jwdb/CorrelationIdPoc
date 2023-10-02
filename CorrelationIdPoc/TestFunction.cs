using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CorrelationIdPoc;

public static class TestFunction
{
    [FunctionName("TestFunction")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string correlationId = req.Headers["X-Correlation-ID"];
        string traceParent = req.Headers["traceparent"];

        string responseMessage = $"This HTTP triggered function executed successfully. CorrelationId: {correlationId}, traceParent: {traceParent}";

        return new OkObjectResult(responseMessage);
    }
}