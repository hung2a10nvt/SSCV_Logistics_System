using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace SSCV.Api.Services;

public interface IRabbitMQProducer
{
    Task SendMessageAsync<T>(T message);
}

public class RabbitMQProducer : IRabbitMQProducer, IDisposable
{
    private readonly string _hostName;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "telemetry_queue";

    public RabbitMQProducer(IConfiguration configuration)
    {
        _hostName = configuration["RabbitMQ:Host"] ?? "localhost";

        InitializeRabbitMQ().Wait();
    }

    private async Task InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory { HostName = _hostName };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task SendMessageAsync<T>(T message)
    {
        if (_connection == null || !_connection.IsOpen)
        {
            await InitializeRabbitMQ();
        }

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel!.BasicPublishAsync(exchange: "", routingKey: QueueName, body: body);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
    }
}