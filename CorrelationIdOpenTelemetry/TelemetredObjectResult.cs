using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace CorrelationIdOpenTelemetry;

public class TelemetredObjectResult : ObjectResult
{
    private ActivityContext activityContext;
    public TelemetredObjectResult(object value, ActivityContext activityContext, HttpStatusCode httpStatusCode = HttpStatusCode.OK) : base(value)
    {
        this.activityContext = activityContext;
        StatusCode = (int)httpStatusCode;
    }

    public override void OnFormatting(ActionContext context)
    {
        base.OnFormatting(context);

        var propagator = context.HttpContext.RequestServices.GetService<TextMapPropagator>();
        propagator.Inject(new PropagationContext(activityContext, Baggage.Current), context, ActionResultSetter);
    }

    private void ActionResultSetter(ActionContext target, string name, string value)
    {
        target.HttpContext.Response.Headers[name] = value;
    }
}