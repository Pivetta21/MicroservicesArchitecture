using Common.RabbitMq.Enums;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;
using Armory.Services.Interfaces;
using Common.DTOs.PlayDungeon;

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
            var playDungeonReplyDto = JsonSerializer.Deserialize<PlayDungeonReplyDto>(message);

            if (playDungeonReplyDto == null)
                throw new Exception("Message could not be parsed to its respective DTO");

            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IDungeonService>();

            await service.ProcessPlayDungeonReply(playDungeonReplyDto);

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
