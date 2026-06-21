using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using YAMCqrs.Core.Extensions;
using YAMCqrs.EventBus.Provider.RabbitMq.Abstractions;
using YAMCqrs.EventBus.Provider.RabbitMq.Configuration;

namespace YAMCqrs.EventBus.Provider.RabbitMq.Implementation;

internal sealed class RabbitPublisher : IRabbitEventBusPublisher, IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitPublisher(IOptions<RabbitConfigurationOptions> configuration, IConfiguration theConfiguration)
    {
        var cs = theConfiguration.FindConnectionString(configuration.Value.ConnectionString);
        if (string.IsNullOrWhiteSpace(cs))
            throw new ArgumentException("RabbitMQ connection string is empty");

        // If it's a full amqp(s) URI, use it directly
        if (Uri.TryCreate(cs, UriKind.Absolute, out var uri) && (uri.Scheme == "amqp" || uri.Scheme == "amqps"))
        {
            _factory = new ConnectionFactory { Uri = uri, AutomaticRecoveryEnabled = true };
            return;
        }

        throw new ArgumentException("RabbitMQ connection string is invalid URI");
    }

    public async Task PublishAsync(string topic, string jsonValue, Dictionary<string, string>? headers, CancellationToken cancellationToken)
    {
        await Connect();

        string? routingKey = null;

        headers?.TryGetValue(ConfigurationConst.RouteRoutingKey, out routingKey);

        var body = Encoding.UTF8.GetBytes(jsonValue);

        await _channel!.BasicPublishAsync(topic, routingKey ?? topic, body, cancellationToken: cancellationToken);
    }



    private async Task Connect()
    {
        if (_channel is not null && _channel.IsOpen)
            return;

        await _lock.WaitAsync();
        try
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }
        catch (Exception ex)
        {
            // Minimal logging so you can see the real error while debugging
            Debug.WriteLine($"RabbitPublisher.Connect error: {ex}");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }


    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
        if (_lock != null) _lock.Dispose();
    }
}
