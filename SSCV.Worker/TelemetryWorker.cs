using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SSCV.Domain.Entities;
using SSCV.Infrastructure.Data;
using System.Text;
using System.Text.Json;

namespace SSCV.Worker;

public class TelemetryWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration; 
    private const string TelemetryQueue = "telemetry_queue";
    private const string AlertQueue = "vehicle_alerts";

    public TelemetryWorker(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
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

                Console.WriteLine($"[Worker] --> Connect RabbitMQ succeeded at {hostName}");
                break; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Worker] RabbitMQ not ready, retrying after ({ex.Message})");
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (channel == null) return; 

        await channel.QueueDeclareAsync(queue: TelemetryQueue, durable: false, exclusive: false, autoDelete: false);
        await channel.QueueDeclareAsync(queue: AlertQueue, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var data = JsonSerializer.Deserialize<TelemetryRecord>(message);

                if (data != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    if (string.IsNullOrEmpty(data.LicensePlate))
                    {
                        // Find vehicles' info
                        var vehicle = await dbContext.Vehicles.FindAsync(data.VehicleId);

                        if (vehicle != null)
                        {
                            data.LicensePlate = vehicle.LicensePlate; 
                        }
                        else
                        {
                            // Can't find vehicle info, temporarily set to UNKNOWN...
                            data.LicensePlate = "UNKNOWN";
                            Console.WriteLine($"[Warning] Couldnt find VehicleId {data.VehicleId}");
                        }
                    }

                    dbContext.TelemetryRecords.Add(data);
                    await dbContext.SaveChangesAsync();
                    Console.WriteLine($"[Worker] Saved vehicle {data.VehicleId} data.");

                    // Logic 
                    bool isUnstable = false;
                    string alertReason = "";

                    if (data.Speed > 80)
                    {
                        isUnstable = true;
                        alertReason = $"Speed limit exceed! Speed: {data.Speed} km/h";
                    }
                    else if (data.Temperature > 30)
                    {
                        isUnstable = true;
                        alertReason = $"Temperature too high! Current temp: {data.Temperature}°C";
                    }

                    if (isUnstable)
                    {
                        var plate = data.LicensePlate ?? $"ID:{data.VehicleId}";
                        var alertEvent = new AlertSystem 
                        {
                            LicensePlate = plate,
                            Message = $"[{plate}] {alertReason}",
                            Timestamp = DateTime.UtcNow
                        };
                        var alertBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alertEvent));

                        await channel.BasicPublishAsync(
                            exchange: string.Empty,
                            routingKey: AlertQueue,
                            body: alertBody
                        );
                        Console.WriteLine($"[Worker] !!! ALERT SENT for Vehicle {data.VehicleId} !!!");
                    }
                }

                // Confirm processing
                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to process message: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync(queue: TelemetryQueue, autoAck: false, consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}