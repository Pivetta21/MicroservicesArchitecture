using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Common.DTOs.PlayDungeon;
using Common.RabbitMq.Enums;
using OpenTelemetry;
using RabbitMQ.Client;

namespace Game.AsyncDataServices;

public class PlayDungeonReplyProducer : RabbitMqProducerBase, IRabbitMqProducer<PlayDungeonReplyDto>
{
    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.PlayDungeon;
    private const QueuesEnum Queue = QueuesEnum.PlayDungeonReply;

    private readonly ILogger<PlayDungeonReplyProducer> _logger;

    public PlayDungeonReplyProducer(
        ILogger<PlayDungeonReplyProducer> logger,
        RabbitMqConnectionManager connectionManager
    ) : base(connectionManager.ProducerConnection, ExchangeType, Exchange, Queue)
    {
        _logger = logger;
    }

    public void Publish(PlayDungeonReplyDto @event, SagaInfo sagaInfo)
    {
        var props = CreateBasicProperties(sagaInfo);

        try
        {
            using var activity = AppConfig.DungeonEntranceSource.StartActivity(ActivityKind.Producer);
            RabbitMqTracingUtil.AddActivityTags(activity, Queue.ToString(), @event.PlayDungeonEvent.ToString());
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

    public void Publish(PlayDungeonReplyDto @event, IBasicProperties props)
    {
        try
        {
            var parentContext = RabbitMqTracingUtil.ExtractParentContext(props);
            Baggage.Current = parentContext.Baggage;
            using var activity = AppConfig.DungeonEntranceSource.StartActivity(ActivityKind.Consumer, parentContext.ActivityContext);
            RabbitMqTracingUtil.AddActivityTags(activity, Queue.ToString(), @event.PlayDungeonEvent.ToString());

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

    private void LogInformation(PlayDungeonReplyDto @event, IBasicProperties props)
    {
        _logger.LogInformation(
            "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] Successfully published a {EventName} event",
            props.Headers[SagaInfo.SagaNameKey],
            props.Headers[SagaInfo.CorrelationIdKey],
            @event.DungeonEntranceTransactionId,
            @event.PlayDungeonEvent
        );
    }

    private void LogError(PlayDungeonReplyDto @event, Exception ex, IBasicProperties props)
    {
        _logger.LogError(
            ex,
            "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] Publish of {EventName} event failed. Message: {ProducerMessage}",
            props.Headers[SagaInfo.SagaNameKey],
            props.Headers[SagaInfo.CorrelationIdKey],
            @event.DungeonEntranceTransactionId,
            @event.PlayDungeonEvent,
            string.IsNullOrEmpty(ex.Message) ? "Unknown error" : ex.Message
        );
    }
}
