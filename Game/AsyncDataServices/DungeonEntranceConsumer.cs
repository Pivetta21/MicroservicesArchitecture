using System.Text;
using System.Text.Json;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq;
using Common.RabbitMq.Enums;
using Game.Services.Interfaces;
using RabbitMQ.Client.Events;

namespace Game.AsyncDataServices;

public class DungeonEntranceConsumer : RabbitMqConsumerBase, IHostedService
{
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
        _logger.LogInformation("Consumer '{ConsumerName}' started", nameof(DungeonEntranceConsumer));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        base.Dispose();

        _logger.LogInformation("Consumer '{ConsumerName}' stopped", nameof(DungeonEntranceConsumer));
        return Task.CompletedTask;
    }

    protected override async Task OnEventReceived(object? sender, BasicDeliverEventArgs @event)
    {
        if (Channel?.IsClosed ?? true)
            base.CreateChannel();

        try
        {
            var message = Encoding.UTF8.GetString(@event.Body.ToArray());

            _logger.LogInformation("Message {@Body} is being processed", message);

            var dungeonEntranceDto = JsonSerializer.Deserialize<DungeonEntranceArmoryDto>(message);

            if (dungeonEntranceDto == null)
                throw new RabbitMqException($"Message {message} could not be parsed to its respective DTO");

            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDungeonEntranceService>();

            await service.ProcessDungeonEntrance(dungeonEntranceDto);

            _logger.LogInformation("Message {@Body} was consumed successfully", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error happened while consuming a message with the following correlation id {CorrelationId}",
                @event.BasicProperties.CorrelationId
            );
        }
        finally
        {
            Channel?.BasicAck(@event.DeliveryTag, false);
        }
    }
}
