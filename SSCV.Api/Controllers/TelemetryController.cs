using Microsoft.AspNetCore.Mvc;
using SSCV.Api.Models;
using SSCV.Api.Services;

namespace SSCV.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : Controller
{
    private readonly IRabbitMQProducer _mqProducer;

    public TelemetryController(IRabbitMQProducer mqProducer)
    {
        _mqProducer = mqProducer;
    }

    [HttpPost]
    public async Task<IActionResult> IngestData([FromBody] TelemetryDto data)
    {
        await _mqProducer.SendMessageAsync(data);

        return Accepted(new { status = "Queued", data.VehicleId, timestamp = DateTime.UtcNow });
    }
}