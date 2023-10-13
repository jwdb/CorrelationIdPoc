# CorrelationId PoC
In this repo there are multiple examples on how to use a x-correlation-id header field as a traceparent for OpenTelemetry / Azure Monitor.

## Projects
### CorrelationIdOpenTelemetry
This project has a basic implementation with a Azure isolated function with a manual setup of OpenTelemetry.
> Using a Propegator (to replace traceparent), a requestmiddleaware (to start the activity) and a custom object result (to hand back the traceparent).

### CorrelationIdPoc
This project describes how to swap the header in a Azure In-Process function. This is 'too late' for the telemetry.
 > It uses a filter attribute to complete this task.

### CorrelationIdPocIsolated
This an example how to use the x-correlation-id as traceparent with Azure Monitor, resulting in minimal amount of logging with a different traceparent. This extra logging is because the functions host starts logging outside of our control. (Effect is minimalized because of the `WorkerApplicationInsightsLoggingEnabled` setting).
> Using a Propegator (to replace traceparent) and a requestmiddleaware (to start the activity).

### CorrelationIdWebCoreApp
With this the the traceparent is swapped out using the CommercetoolsPropegator to swap the get call. All logging in Azure Monitor is done through the correct traceparent.
> Using a Propegator (to replace traceparent).


## short explanation
The problem mostly with azure functions is that it's not ran independently. The functions host receives the initial call, starts a trace and uses gRPC to call the specific function. The function itself does not have a 'normal' understanding of the httprequest it received. Also the default request start logging for webapi's is not called because it's not a httprequest but a gRPC call.

With a normal webapp you're in full control of the request chain and you're able to influence the logging before it happens using relatively normal code.