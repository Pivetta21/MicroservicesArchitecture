using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq.Enums;
using OpenTelemetry;
using RabbitMQ.Client;

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

    public void Publish(DungeonEntranceGameDto @event, SagaInfo sagaInfo)
    {
        var props = CreateBasicProperties(sagaInfo);

        try
        {
            using var activity = AppConfig.DungeonEntranceSource.StartActivity(ActivityKind.Producer);
            RabbitMqTracingUtil.AddActivityTags(activity, Queue.ToString(), @event.DungeonEntranceEvent.ToString());
            RabbitMqTracingUtil.InjectCarrierIntoContext(activity, props);

            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            BasicPublish(body, props);

            LogInformation(@event, props);
        }
        catch (Exception ex)
        {
            LogError(@event, ex, props);
        }
    }

    public void Publish(DungeonEntranceGameDto @event, IBasicProperties props)
    {
        try
        {
            var parentContext = RabbitMqTracingUtil.ExtractParentContext(props);
            Baggage.Current = parentContext.Baggage;

            using var activity = AppConfig.DungeonEntranceSource.StartActivity(
                kind: ActivityKind.Consumer,
                parentContext: parentContext.ActivityContext
            );
            RabbitMqTracingUtil.AddActivityTags(activity, Queue.ToString(), @event.DungeonEntranceEvent.ToString());

            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            BasicPublish(body, props);

            LogInformation(@event, props);
        }
        catch (Exception ex)
        {
            LogError(@event, ex, props);
        }
    }

    private void LogInformation(DungeonEntranceGameDto @event, IBasicProperties props)
    {
        _logger.LogInformation(
            "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] Successfully published a {EventName} event",
            props.Headers[SagaInfo.SagaNameKey],
            props.Headers[SagaInfo.CorrelationIdKey],
            @event.DungeonEntranceTransactionId,
            @event.DungeonEntranceEvent
        );
    }

    private void LogError(DungeonEntranceGameDto @event, Exception ex, IBasicProperties props)
    {
        _logger.LogError(
            ex,
            "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] Publish of {EventName} event failed. Message: {ProducerMessage}",
            props.Headers[SagaInfo.SagaNameKey],
            props.Headers[SagaInfo.CorrelationIdKey],
            @event.DungeonEntranceTransactionId,
            @event.DungeonEntranceEvent,
            string.IsNullOrEmpty(ex.Message) ? "Unknown error" : ex.Message
        );
    }
}
