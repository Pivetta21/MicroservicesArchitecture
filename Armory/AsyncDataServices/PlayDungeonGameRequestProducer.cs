using System.Text;
using System.Text.Json;
using Common.DTOs.PlayDungeon;
using Common.RabbitMq.Enums;

namespace Armory.AsyncDataServices;

public class PlayDungeonGameRequestProducer : RabbitMqProducerBase, IRabbitMqProducer<PlayDungeonGameDto>
{
    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.PlayDungeon;
    private const QueuesEnum Queue = QueuesEnum.PlayDungeonGameRequest;

    private readonly ILogger<PlayDungeonGameRequestProducer> _logger;

    public PlayDungeonGameRequestProducer(
        ILogger<PlayDungeonGameRequestProducer> logger,
        RabbitMqConnectionManager connectionManager
    ) : base(connectionManager.ProducerConnection, ExchangeType, Exchange, Queue)
    {
        _logger = logger;
    }

    public void Publish(PlayDungeonGameDto @event)
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
                @event.PlayDungeonEvent
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] Publish of {} event failed. Message: {ProducerMessage}",
                @event.SagaName,
                @event.SagaCorrelationId,
                @event.DungeonEntranceTransactionId,
                @event.PlayDungeonEvent,
                string.IsNullOrEmpty(ex.Message) ? "Unknown error" : ex.Message
            );
        }
    }
}
