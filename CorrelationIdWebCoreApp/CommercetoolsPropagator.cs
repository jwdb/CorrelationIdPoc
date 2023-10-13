using OpenTelemetry.Context.Propagation;

namespace CorrelationIdWebCoreApp;

public class CommercetoolsPropagator : TextMapPropagator
{
    private readonly TraceContextPropagator _defaultPropagator = new();
    private readonly string _correlationId = "x-correlation-id";
    private readonly string _traceParent = "traceparent";

    public override ISet<string> Fields => _defaultPropagator.Fields;

    public override void Inject<T>(PropagationContext context, T carrier, Action<T, string, string> setter) =>
        _defaultPropagator.Inject(context, carrier, setter);

    public override PropagationContext Extract<T>(PropagationContext context, T carrier,
        Func<T, string, IEnumerable<string>> getter) =>
        _defaultPropagator.Extract(context, carrier, (obj, name) =>
            getter(obj, name == _traceParent ? _correlationId : name)
                .Concat(getter(obj, name == _traceParent ? _traceParent : name)));
}