using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CorrelationIdPocIsolated;

public class TestFunction
{
    private readonly ILogger _logger;

    public TestFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TestFunction>();
    }

    [Function(nameof(TestFunction))]
    public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string? correlationId = req.Headers["x-correlation-id"].FirstOrDefault();

        string responseMessage = $"This HTTP triggered function executed successfully. CorrelationId: {correlationId}";
        req.HttpContext.Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        await req.HttpContext.Response.WriteAsync(responseMessage);
    }
}