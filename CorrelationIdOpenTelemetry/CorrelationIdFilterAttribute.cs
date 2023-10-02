using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

#pragma warning disable CS0618 // Type or member is obsolete
namespace CorrelationIdOpenTelemetry;

public class CorrelationIdFilterAttribute : FunctionInvocationFilterAttribute
{
    public override Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
    {
        var request = executingContext.Arguments.ContainsKey("req") ? (HttpRequest)executingContext.Arguments["req"] : null;

        if (request == null)
        {
            return Task.CompletedTask;
        }

        var textMapPropagator = Propagators.DefaultTextMapPropagator;
        if (textMapPropagator is not TraceContextPropagator)
        {
            var ctx = textMapPropagator.Extract(default, request, ExtractTraceContextFromHeaders);

            if (ctx.ActivityContext.IsValid())
            {
                // Create a new activity with its parent set from the extracted context.
                // This makes the new activity as a "sibling" of the activity created by
                // Asp.Net Core.
#if NET7_0_OR_GREATER
                // For NET7.0 onwards activity is created using ActivitySource so,
                // we will use the source of the activity to create the new one.
                Activity newOne = activity.Source.CreateActivity(ActivityOperationName, ActivityKind.Server, ctx.ActivityContext);
#else
                Activity newOne = new Activity("RequestIn");
                newOne.SetParentId(ctx.ActivityContext.TraceId, ctx.ActivityContext.SpanId, ctx.ActivityContext.TraceFlags);
#endif
                newOne.TraceStateString = ctx.ActivityContext.TraceState;

                newOne.SetTag("IsCreatedByInstrumentation", bool.TrueString);

                // Starting the new activity make it the Activity.Current one.
                newOne.Start();
            }

            Baggage.Current = ctx.Baggage;
        }

        return Task.CompletedTask;
    }

    private IEnumerable<string> ExtractTraceContextFromHeaders(HttpRequest props, string key)
    {
        return props.Headers.TryGetValue(key, out var value) ? value : Enumerable.Empty<string>();
    }

}
#pragma warning restore CS0618 // Type or member is obsolete