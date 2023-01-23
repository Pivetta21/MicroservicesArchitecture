using System.Text;
using System.Text.Json;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq.Enums;
using Game.Services.Interfaces;
using RabbitMQ.Client.Events;

namespace Game.AsyncDataServices;

public class DungeonEntranceConsumer : RabbitMqConsumerBase, IHostedService
{
    private const string ConsumerName = nameof(DungeonEntranceConsumer);

    private const ExchangeTypeEnum ExchangeType = ExchangeTypeEnum.Direct;
    private const ExchangesEnum Exchange = ExchangesEnum.DungeonEntrance;
    private const QueuesEnum Queue = QueuesEnum.DungeonEntranceArmory;

    private readonly ILogger<DungeonEntranceConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DungeonEntranceConsumer(
        RabbitMqConnectionManager connectionManager,
        ILogger<DungeonEntranceConsumer> logger,
        IServiceScopeFactory serviceScopeFactory
    ) : base(connectionManager.ConsumerConnection, ExchangeType, Exchange, Queue)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Consumer '{ConsumerName}' started", ConsumerName);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        base.Dispose();

        _logger.LogInformation("Consumer '{ConsumerName}' stopped", ConsumerName);
        return Task.CompletedTask;
    }

    protected override async Task OnEventReceived(object? sender, BasicDeliverEventArgs @event)
    {
        if (Channel?.IsClosed ?? true)
            base.CreateChannel();

        var messageCorrelationId = @event.BasicProperties.CorrelationId;

        _logger.LogInformation(
            "Message {MessageCorrelationId} is being processed by {ConsumerName}",
            messageCorrelationId,
            ConsumerName
        );

        try
        {
            var message = Encoding.UTF8.GetString(@event.Body.ToArray());
            var dungeonEntranceDto = JsonSerializer.Deserialize<DungeonEntranceArmoryDto>(message);

            if (dungeonEntranceDto == null)
                throw new Exception("Message could not be parsed to its respective DTO");

            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDungeonEntranceService>();

            await service.ProcessDungeonEntrance(dungeonEntranceDto);

            _logger.LogInformation(
                "Message {MessageCorrelationId} was consumed successfully by {ConsumerName}",
                messageCorrelationId,
                ConsumerName
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error happened while message {MessageCorrelationId} was being consumed by {ConsumerName}",
                messageCorrelationId,
                ConsumerName
            );
        }
        finally
        {
            Channel?.BasicAck(@event.DeliveryTag, false);
        }
    }
}
