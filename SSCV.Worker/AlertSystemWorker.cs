using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; 
using SSCV.Infrastructure.Data;
using SSCV.Domain.Entities;   

namespace SSCV.Worker;

public class AlertSystemWorker : BackgroundService
{
    private readonly ILogger<AlertSystemWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider; 
    private const string AlertQueue = "vehicle_alerts";


    public AlertSystemWorker(
        ILogger<AlertSystemWorker> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider) 
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hostName = _configuration["RabbitMQ:Host"] ?? "localhost";
        var factory = new ConnectionFactory() { HostName = hostName };


        IConnection connection = null;
        IChannel channel = null;

        // Retry logic
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                _logger.LogInformation($"[AlertWorker] --> Connected to RabbitMQ at {hostName}");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[AlertWorker] RabbitMQ not ready: {ex.Message}. Retrying in 5s...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (channel == null) return;

        await channel.QueueDeclareAsync(queue: AlertQueue, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Deserialize message JSON -> Object
                var alertData = JsonSerializer.Deserialize<AlertSystem>(message);

                if (alertData != null)
                {
                    // save to db
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        alertData.AlertSystemId = 0;

                        dbContext.AlertSystems.Add(alertData);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"[AlertWorker] --> Saved Alert to DB (ID: {alertData.AlertSystemId})");
                    }

                    await SendTelegramMessage(alertData.Message);
                }

                // Ack 
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[AlertWorker] Error processing alert: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync(queue: AlertQueue, autoAck: false, consumer: consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task SendTelegramMessage(string message)
    {
        var TelegramBotToken = _configuration["Telegram:BotToken"];
        var TelegramChatId = _configuration["Telegram:ChatId"];
        try
        {

            var url = $"https://api.telegram.org/bot{TelegramBotToken}/sendMessage";

            using var httpClient = new HttpClient();
            var payload = new
            {
                chat_id = TelegramChatId,
                text = $"ALERT: {message}"
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
                _logger.LogInformation($"[Telegram] Sent message: {message}");
            else
                _logger.LogError($"[Telegram] Failed to send. Status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Telegram] Exception: {ex.Message}");
        }
    }
}