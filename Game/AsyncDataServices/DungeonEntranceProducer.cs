using System.Text;
using System.Text.Json;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq;
using Common.RabbitMq.Enums;

namespace Game.AsyncDataServices;

public class DungeonEntranceProducer : RabbitMqProducerBase, IRabbitMqProducer<DungeonEntranceGameDto>
{
    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.DungeonEntrance;
    private const QueuesEnum Queue = QueuesEnum.DungeonEntranceGame;

    private readonly ILogger<DungeonEntranceProducer> _logger;

    public DungeonEntranceProducer(
        ILogger<DungeonEntranceProducer> logger,
        RabbitMqConnectionManager connectionManager
    ) : base(connectionManager.ProducerConnection, ExchangeType, Exchange, Queue)
    {
        _logger = logger;
    }

    public void Publish(DungeonEntranceGameDto @event)
    {
        try
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            BasicPublish(body);

            _logger.LogInformation(
                "Successfully published a {DungeonEntranceEvent} for dungeon entrance {DungeonEntranceTransactionId}",
                @event.DungeonEntranceEvent,
                @event.DungeonEntranceTransactionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Publishing of a {DungeonEntranceEvent} for dungeon entrance {DungeonEntranceTransactionId} failed",
                @event.DungeonEntranceEvent,
                @event.DungeonEntranceTransactionId
            );
        }
    }
}
