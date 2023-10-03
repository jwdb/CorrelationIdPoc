using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace CorrelationIdPocIsolated;

public class RequestMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext?.Request == null)
        {
            await next(context);
            return;
        }

        var textMapPropagator = Propagators.DefaultTextMapPropagator;
        StartActivity(textMapPropagator, httpContext);

        httpContext.Response.OnStarting(() => BeforeResponseSent(context, textMapPropagator));

        await next(context);
    }

    private void StartActivity(TextMapPropagator textMapPropagator, HttpContext httpContext)
    {
        if (textMapPropagator is TraceContextPropagator) return;

        var ctx = textMapPropagator.Extract(default, httpContext.Request, ExtractTraceContextFromHeaders);

        httpContext.TraceIdentifier = ctx.ActivityContext.TraceId.ToString();
        if (ctx.ActivityContext.IsValid())
        {
            Activity newOne = new("RequestIn");
            newOne.SetParentId(ctx.ActivityContext.TraceId, ctx.ActivityContext.SpanId, ctx.ActivityContext.TraceFlags);
            newOne.TraceStateString = ctx.ActivityContext.TraceState;

            newOne.SetTag("IsCreatedByInstrumentation", bool.TrueString);

            // Starting the new activity make it the Activity.Current one.
            newOne.Start();
        }

        Baggage.Current = ctx.Baggage;
    }

    private static async Task BeforeResponseSent(FunctionContext context, TextMapPropagator? textMapPropagator)
    {
        var activity = Activity.Current;

        if (activity is { IsAllDataRequested: true })
        {
            var response = context.GetHttpContext()?.Response;

            activity.SetStatus(ResolveSpanStatusForHttpStatusCode(activity.Kind, response?.StatusCode));

            textMapPropagator?.Inject(
                new PropagationContext(activity.Context, Baggage.Current),
                response,
                (t, n, v) => t?.Headers.Add(n, v));
        }

        if (activity?.Tags.FirstOrDefault(q => q.Key == "IsCreatedByInstrumentation").Value != bool.TrueString)
            return;

        // If instrumentation started a new Activity, it must
        // be stopped here.
        activity.SetTag("IsCreatedByInstrumentation", null);
        activity.Stop();

        await Task.CompletedTask;
    }

    private static IEnumerable<string> ExtractTraceContextFromHeaders(HttpRequest? props, string key)
    {
        if (props == null)
        {
            return Enumerable.Empty<string>();
        }

        return props.Headers.TryGetValue(key, out var value) ? value : Enumerable.Empty<string>();
    }

    public static ActivityStatusCode ResolveSpanStatusForHttpStatusCode(ActivityKind kind, int? httpStatusCode)
    {
        var upperBound = kind == ActivityKind.Client ? 399 : 499;
        if (httpStatusCode >= 100 && httpStatusCode <= upperBound)
        {
            return ActivityStatusCode.Unset;
        }

        return ActivityStatusCode.Error;
    }
}