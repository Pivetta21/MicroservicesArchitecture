using System.Text;
using System.Text.Json;
using Common.DTOs.PlayDungeon;
using Common.RabbitMq.Enums;
using Game.Services.Interfaces;
using RabbitMQ.Client.Events;

namespace Game.AsyncDataServices;

public class PlayDungeonGameRequestConsumer : RabbitMqConsumerBase, IHostedService
{
    private const string ConsumerName = nameof(PlayDungeonGameRequestConsumer);

    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.PlayDungeon;
    private const QueuesEnum Queue = QueuesEnum.PlayDungeonGameRequest;

    private readonly ILogger<PlayDungeonGameRequestConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PlayDungeonGameRequestConsumer(
        ILogger<PlayDungeonGameRequestConsumer> logger,
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
        var messageCorrelationId = @event.BasicProperties.CorrelationId;

        var sagaInfo = JsonSerializer.Deserialize<SagaInfo>(messageUtf8String);

        try
        {
            var playDungeonGameDto = JsonSerializer.Deserialize<PlayDungeonGameDto>(messageUtf8String);

            if (playDungeonGameDto == null)
                throw new Exception("Byte array could not be parsed to its respective DTO");

            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDungeonService>();

            await service.ProcessPlayDungeonGameRequest(playDungeonGameDto);

            LogInformation(sagaInfo, $"Message {messageCorrelationId} was consumed successfully");
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
