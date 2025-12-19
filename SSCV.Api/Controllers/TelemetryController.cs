using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using SSCV.Api.Models;
using SSCV.Infrastructure.Data;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace SSCV.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : Controller
{
    private readonly ApplicationDbContext _context;
    public TelemetryController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpPost]
    public async Task<IActionResult> IngestData([FromBody] TelemetryDto data)
    {
        var factory = new ConnectionFactory() { HostName = "logistics-rabbitmq" };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Check if Vehicle exists 
        var vehicleExists = await _context.Vehicles.AnyAsync(v => v.VehicleId == data.VehicleId);
        if (!vehicleExists)
        {
            return BadRequest(new { error = "Vehicle not found", data.VehicleId });
        }

        await channel.QueueDeclareAsync(queue: "telemetry_queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var message = JsonSerializer.Serialize(data);
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(exchange: "",
                             routingKey: "telemetry_queue",
                             body: body);

        return Accepted(new { status = "Queued", data.VehicleId, timestamp = DateTime.UtcNow });
    }
}
