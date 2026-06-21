using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using YAMCqrs.Core.Extensions;
using YAMCqrs.EventBus.Core.SubscribeEvents.Abstractions;
using YAMCqrs.EventBus.Provider.RabbitMq.Configuration;

namespace YAMCqrs.EventBus.Provider.RabbitMq.Implementation;

internal sealed partial class RabbitSubscriber : IHostedService
{
    private readonly ILogger<RabbitSubscriber> _logger;
    private readonly ISubscribeEventStore _subscribeEventStore;
    private readonly RabbitConfigurationOptions _options;
    private readonly string _cs;

    private ConnectionFactory? _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    [LoggerMessage(LogLevel.Warning, "Some subscriptions don't exist, recreating task in 1min")]
    private partial void LogSubscriptionsDontExist();
    [LoggerMessage(LogLevel.Critical, "Error consuming messages from Rabbit, Task is shutting down, ErrorMessage: {ErrorMessage}")]
    private partial void LogCriticalError(string errorMessage);
    [LoggerMessage(LogLevel.Debug, "Received message: {Message} from topic: {Topic}")]
    private partial void LogReceivedMessage(string message, string topic);



    public RabbitSubscriber(IOptions<RabbitConfigurationOptions> configuration, IConfiguration theConfiguration, ILogger<RabbitSubscriber> logger, ISubscribeEventStore subscribeEventStore)
    {
        _logger = logger;
        _subscribeEventStore = subscribeEventStore;
        _options = configuration.Value;
        _cs = theConfiguration.FindConnectionString(_options.ConnectionString);
        Connect().Wait();
    }

    public async Task Connect()
    {
        if (_channel is not null && _channel.IsOpen)
            return;

        if (!Uri.TryCreate(_cs, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalida connection string for RabbitMq.");

        _factory ??= new ConnectionFactory { Uri = uri, AutomaticRecoveryEnabled = true };

        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!await TryStartConnection())
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }
    }

    public async Task<bool> TryStartConnection()
    {
        bool isError = false;
        try
        {
            await Connect();
            var consumer = new AsyncEventingBasicConsumer(_channel!);

            await _channel!.QueueDeclareAsync(
            queue: _options.QueueGroupName,
            durable: true,        // Persistente ante reinicios de RabbitMQ
            exclusive: false,     // IMPORTANTE: Permitir múltiples consumidores
            autoDelete: false
            );

            foreach (var t in _options.Topics)
                await _channel.QueueBindAsync(_options.QueueGroupName, t.Exchange, t.RoutingKey);

            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                LogReceivedMessage(message, ea.Exchange);

                return _subscribeEventStore.StoreAsync(ea.Exchange, message, null, Core.ServiceBusProvider.RabbitMq, default);
            };

            await _channel!.BasicConsumeAsync(_options.QueueGroupName, autoAck: true, consumer: consumer);

            return true;
        }
        catch (OperationInterruptedException)
        {
            LogSubscriptionsDontExist();
            isError = true;

            return false;
        }
        catch (Exception ex)
        {
            LogCriticalError(ex.Message);
            throw;
        }
        finally
        {
            if (isError)
            {
                await Dispose();
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Dispose();
    }

    private async Task Dispose()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        if (_channel is not null)
            await _channel.DisposeAsync();
    }
}
