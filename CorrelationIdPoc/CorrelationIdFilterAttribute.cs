using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CS0618 // Type or member is obsolete
namespace CorrelationIdPoc;

public class CorrelationIdFilterAttribute : FunctionInvocationFilterAttribute
{
    public override Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
    {
        var req = executingContext.Arguments.ContainsKey("req") ? (HttpRequest)executingContext.Arguments["req"] : null;

        var customHeader = req?.Headers["x-correlation-id"].FirstOrDefault();

        if (customHeader != null)
        {
            req.Headers.Add("traceparent", customHeader);
        }

        return Task.CompletedTask;
    }
}
#pragma warning restore CS0618 // Type or member is obsolete