using OpenTelemetry.Context.Propagation;

namespace CorrelationIdPocIsolated;

public class CommercetoolsPropagator : TextMapPropagator
{
    private readonly TraceContextPropagator _defaultPropagator = new();
    private readonly string _correlationId = "X-Correlation-Id";
    private readonly string _traceParent = "traceparent";

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
        IEnumerable<string> PatchedGetter(T obj, string name) => getter(obj, name == _traceParent ? _correlationId : name);

        return _defaultPropagator.Extract(context, carrier, PatchedGetter);
    }
}