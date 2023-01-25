using System.Text;
using System.Text.Json;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq.Enums;

namespace Armory.AsyncDataServices;

public class DungeonEntranceProducer : RabbitMqProducerBase, IRabbitMqProducer<DungeonEntranceArmoryDto>
{
    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.DungeonEntrance;
    private const QueuesEnum Queue = QueuesEnum.DungeonEntranceArmory;

    private readonly ILogger<DungeonEntranceProducer> _logger;

    public DungeonEntranceProducer(
        ILogger<DungeonEntranceProducer> logger,
        RabbitMqConnectionManager connectionManager
    ) : base(connectionManager.ProducerConnection, ExchangeType, Exchange, Queue)
    {
        _logger = logger;
    }

    public void Publish(DungeonEntranceArmoryDto @event)
    {
        try
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            BasicPublish(body);

            _logger.LogInformation(
                "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] Successfully published a {EventName} event",
                @event.SagaName,
                @event.SagaCorrelationId,
                @event.DungeonEntranceTransactionId,
                @event.DungeonEntranceEvent
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] Publish of {EventName} event failed. Message: {ProducerMessage}",
                @event.SagaName,
                @event.SagaCorrelationId,
                @event.DungeonEntranceTransactionId,
                @event.DungeonEntranceEvent,
                string.IsNullOrEmpty(ex.Message) ? "Unknown error" : ex.Message
            );
        }
    }
}
