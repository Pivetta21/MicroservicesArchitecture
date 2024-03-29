using System.Diagnostics;
using Common.RabbitMq.Enums;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;
using Armory.Services.Interfaces;
using Common.DTOs.PlayDungeon;
using OpenTelemetry;

namespace Armory.AsyncDataServices;

public class PlayDungeonReplyConsumer : RabbitMqConsumerBase, IHostedService
{
    private const string ConsumerName = nameof(PlayDungeonReplyConsumer);

    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.PlayDungeon;
    private const QueuesEnum Queue = QueuesEnum.PlayDungeonReply;

    private readonly ILogger<PlayDungeonReplyConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PlayDungeonReplyConsumer(
        ILogger<PlayDungeonReplyConsumer> logger,
        RabbitMqConnectionManager connectionManager,
        IServiceScopeFactory serviceScopeFactory
    ) : base(connectionManager.ConsumerConnection, ExchangeType, Exchange, Queue)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consumer {ConsumerName} started", ConsumerName);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        base.Dispose();

        _logger.LogInformation("Consumer {ConsumerName} stopped", ConsumerName);
        return Task.CompletedTask;
    }

    protected override async Task OnEventReceived(object? sender, BasicDeliverEventArgs @event)
    {
        if (Channel?.IsClosed ?? true)
            base.CreateChannel();

        var messageByteArray = @event.Body.ToArray();
        var messageUtf8String = Encoding.UTF8.GetString(messageByteArray);

        var sagaInfo = SagaInfo.ExtractSagaInfo(@event.BasicProperties);

        try
        {
            var playDungeonReplyDto = JsonSerializer.Deserialize<PlayDungeonReplyDto>(messageUtf8String);

            var parentContext = RabbitMqTracingUtil.ExtractParentContext(@event.BasicProperties);
            Baggage.Current = parentContext.Baggage;
            using var activity = AppConfig.DungeonEntranceSource.StartActivity(ActivityKind.Consumer, parentContext.ActivityContext);
            RabbitMqTracingUtil.AddActivityTags(activity, Queue.ToString(), playDungeonReplyDto?.PlayDungeonEvent.ToString());

            if (playDungeonReplyDto == null)
                throw new Exception("Byte array could not be parsed to its respective DTO");

            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDungeonService>();

            await service.ProcessPlayDungeonReply(playDungeonReplyDto, sagaInfo, @event.BasicProperties);

            LogInformation(sagaInfo, "Message was consumed successfully");
        }
        catch (Exception ex)
        {
            LogError(sagaInfo, ex);
        }
        finally
        {
            Channel?.BasicAck(@event.DeliveryTag, false);
        }
    }

    protected override void LogInformation(SagaInfo? sagaInfo, string message)
    {
        _logger.LogInformation(
            "[{SagaName} #{SagaCorrelationId}] [{ConsumerName}] {ConsumerMessage}",
            sagaInfo?.SagaName,
            sagaInfo?.SagaCorrelationId,
            ConsumerName,
            message
        );
    }

    protected override void LogError(SagaInfo? sagaInfo, Exception ex)
    {
        _logger.LogError(
            ex,
            "[{SagaName} #{SagaCorrelationId}] [{ConsumerName}] Erro to consume. Message: {ConsumerMessage}",
            sagaInfo?.SagaName,
            sagaInfo?.SagaCorrelationId,
            ConsumerName,
            string.IsNullOrEmpty(ex.Message) ? "Unknown error" : ex.Message
        );
    }
}
