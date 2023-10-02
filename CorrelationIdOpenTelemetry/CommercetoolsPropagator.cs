using System;
using System.Collections.Generic;
using OpenTelemetry.Context.Propagation;

namespace CorrelationIdOpenTelemetry;

public class CommercetoolsPropagator : TextMapPropagator
{
    private readonly TraceContextPropagator _defaultPropagator = new();
    private readonly string CorrelationId = "X-Correlation-Id";
    private readonly string TraceParent = "traceparent";

    public override ISet<string> Fields { get; } = new HashSet<string>(new[]
    {
        "traceparent",
        "requestid",
        "tracestate",
        "bagage",
        "correlationcontext",
        "X-Correlation-Id"
    });

    public override void Inject<T>(PropagationContext context, T carrier, Action<T, string, string> setter)
    {
        _defaultPropagator.Inject(context, carrier, setter);
    }

    public override PropagationContext Extract<T>(PropagationContext context, T carrier, Func<T, string, IEnumerable<string>> getter)
    {
        if (getter == null)
        {
            return context;
        }

        IEnumerable<string> PatchedGetter(T obj, string name) => getter(obj, name == TraceParent ? CorrelationId : name);

        return _defaultPropagator.Extract(context, carrier, PatchedGetter);
    }
}