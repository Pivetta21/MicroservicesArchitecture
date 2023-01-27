using System.Text;
using RabbitMQ.Client;

namespace Common.RabbitMq;

public class SagaInfo
{

    #region IBasicProperties

    public const String SagaNameKey = "saga-name";

    public const String CorrelationIdKey = "saga-correlation-id";

    public static SagaInfo ExtractSagaInfo(IBasicProperties props)
    {
        var sagaName = Encoding.UTF8.GetString((byte[])props.Headers[SagaNameKey]);
        var sagaCorrelationId = Encoding.UTF8.GetString((byte[])props.Headers[CorrelationIdKey]);

        props.Headers[SagaNameKey] = sagaName;
        props.Headers[CorrelationIdKey] = sagaCorrelationId;

        return new SagaInfo
        {
            SagaName = sagaName,
            SagaCorrelationId = sagaCorrelationId,
        };
    }

    #endregion

    public required string SagaName { get; set; }

    public required string SagaCorrelationId { get; set; }
}
