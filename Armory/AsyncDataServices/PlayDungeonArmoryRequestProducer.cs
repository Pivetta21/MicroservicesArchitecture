using System.Text;
using System.Text.Json;
using Common.DTOs.PlayDungeon;
using Common.RabbitMq.Enums;

namespace Armory.AsyncDataServices;

public class PlayDungeonArmoryRequestProducer : RabbitMqProducerBase, IRabbitMqProducer<PlayDungeonArmoryDto>
{
    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.PlayDungeon;
    private const QueuesEnum Queue = QueuesEnum.PlayDungeonGameRequest;

    private readonly ILogger<PlayDungeonArmoryRequestProducer> _logger;

    public PlayDungeonArmoryRequestProducer(
        ILogger<PlayDungeonArmoryRequestProducer> logger,
        RabbitMqConnectionManager connectionManager
    ) : base(connectionManager.ProducerConnection, ExchangeType, Exchange, Queue)
    {
        _logger = logger;
    }

    public void Publish(PlayDungeonArmoryDto @event)
    {
        try
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            BasicPublish(body);

            _logger.LogInformation(
                "Successfully published a {PlayDungeonEvent} event for dungeon entrance {DungeonEntranceTransactionId}",
                @event.PlayDungeonEvent,
                @event.DungeonEntranceTransactionId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Publishing a {PlayDungeonEvent} for dungeon entrance '{DungeonEntranceTransactionId}' failed",
                @event.PlayDungeonEvent,
                @event.DungeonEntranceTransactionId
            );
        }
    }
}