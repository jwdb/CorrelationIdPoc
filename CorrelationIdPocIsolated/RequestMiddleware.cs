using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace CorrelationIdPocIsolated
{
    public class RequestMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var http = await context.GetHttpRequestDataAsync();
            var customHeader = http?.Headers.FirstOrDefault(x => x.Key == "x-correlation-id");

            if (http != null && customHeader != null)
            {
                http.Headers.Add("traceparent", customHeader.Value.Value.Single());
            }
            await next(context);
        }
    }
}
