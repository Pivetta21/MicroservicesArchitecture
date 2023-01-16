using Armory.Services.Interfaces;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq.Enums;
using Common.RabbitMq;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;

namespace Armory.AsyncDataServices;

public class DungeonEntranceConsumer : RabbitMqConsumerBase, IHostedService
{
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
        _logger.LogInformation("Consumer '{}' started", nameof(DungeonEntranceConsumer));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        base.Dispose();

        _logger.LogInformation("Consumer '{}' stopped", nameof(DungeonEntranceConsumer));
        return Task.CompletedTask;
    }

    protected override async Task OnEventReceived(object? sender, BasicDeliverEventArgs @event)
    {
        if (Channel?.IsClosed ?? true)
            base.CreateChannel();

        try
        {
            var message = Encoding.UTF8.GetString(@event.Body.ToArray());

            _logger.LogInformation("Message {} is being processed", message);

            var dungeonEntranceDto = JsonSerializer.Deserialize<DungeonEntranceGameDto>(message);

            if (dungeonEntranceDto == null)
                throw new RabbitMqException($"Message {message} could not be parsed to its respective DTO");

            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDungeonEntranceService>();

            await service.ProcessDungeonEntrance(dungeonEntranceDto);

            _logger.LogInformation("Message {} was consumed successfully", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error happened while consuming a message with the following correlation id {}",
                @event.BasicProperties.CorrelationId
            );
        }
        finally
        {
            Channel?.BasicAck(@event.DeliveryTag, false);
        }
    }
}
