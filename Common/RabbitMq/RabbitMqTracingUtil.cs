using System.Diagnostics;
using System.Text;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;

namespace Common.RabbitMq;

public static class RabbitMqTracingUtil
{
    public static void AddActivityTags(Activity? activity, string queueName, string? eventName)
    {
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination_kind", "queue");
        activity?.SetTag("messaging.rabbitmq.queue", queueName);
        activity?.SetTag("messaging.rabbitmq.event_name", eventName ?? string.Empty);
    }

    public static void InjectCarrierIntoContext(Activity? activity, IBasicProperties props)
    {
        Propagators.DefaultTextMapPropagator.Inject(
            context: new PropagationContext(activity!.Context, Baggage.Current),
            carrier: props,
            setter: InjectContextIntoHeader
        );
    }

    public static PropagationContext ExtractParentContext(IBasicProperties props)
    {
        return Propagators.DefaultTextMapPropagator.Extract(
            context: default,
            carrier: props,
            getter: ExtractTraceContextFromBasicProperties
        );
    }

    private static void InjectContextIntoHeader(IBasicProperties props, string key, string value)
    {
        try
        {
            props.Headers ??= new Dictionary<string, object>();
            props.Headers[key] = value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to inject trace context. Message: {ex.Message}");
        }
    }

    private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
    {
        try
        {
            if (props.Headers.TryGetValue(key, out object? value))
            {
                var bytes = (byte[])value;
                return new[] { Encoding.UTF8.GetString(bytes) };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to extract trace context. Message: {ex.Message}");
        }

        return Enumerable.Empty<string>();
    }
}
