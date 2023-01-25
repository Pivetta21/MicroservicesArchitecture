using Common.RabbitMq.Enums;

namespace Common.RabbitMq;

public class SagaInfo
{
    public required SagasEnum SagaName { get; set; }

    public required Guid SagaCorrelationId { get; set; }
}
