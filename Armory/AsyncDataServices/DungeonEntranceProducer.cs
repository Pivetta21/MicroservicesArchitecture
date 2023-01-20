using System.Text;
using System.Text.Json;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq;
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
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Publishing of a new dungeon entrance '{DungeonTransactionId}' failed",
                @event.DungeonTransactionId
            );
        }
    }
}
