using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CorrelationIdPocIsolated
{
    public class TestFunction
    {
        private readonly ILogger _logger;

        public TestFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TestFunction>();
        }

        [Function("TestFunction")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var headers = req.Headers.ToDictionary(c => c.Key, c => c.Value);
            string? correlationId = headers["x-correlation-id"].FirstOrDefault();
            string? traceParent = headers["traceparent"].FirstOrDefault();

            string responseMessage = $"This HTTP triggered function executed successfully. CorrelationId: {correlationId}, traceParent: {traceParent}";
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(responseMessage);

            return response;
        }
    }
}
