using Armory.Services.Interfaces;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq.Enums;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;

namespace Armory.AsyncDataServices;

public class DungeonEntranceConsumer : RabbitMqConsumerBase, IHostedService
{
    private const string ConsumerName = nameof(DungeonEntranceConsumer);

    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.DungeonEntrance;
    private const QueuesEnum Queue = QueuesEnum.DungeonEntranceGame;

    private readonly ILogger<DungeonEntranceConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DungeonEntranceConsumer(
        ILogger<DungeonEntranceConsumer> logger,
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
            var dungeonEntranceDto = JsonSerializer.Deserialize<DungeonEntranceGameDto>(messageUtf8String);

            if (dungeonEntranceDto == null)
                throw new Exception("Byte array could not be parsed to its respective DTO");

            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDungeonEntranceService>();

            await service.ProcessDungeonEntrance(dungeonEntranceDto);

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
