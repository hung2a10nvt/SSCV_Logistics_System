using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSCV.Infrastructure.Data;
using SSCV.Api.Models;
using SSCV.Domain.Entities;
namespace SSCV.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VehiclesController(ApplicationDbContext context) 
        { 
            _context = context; 
        }
        // Post - Create new vehicle
        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
        {
            var newVehicle = new Vehicle
            {
                LicensePlate = request.LicensePlate,
                VehicleType = request.VehicleType
            };
            _context.Vehicles.Add(newVehicle);
            await _context.SaveChangesAsync();

            // Return
            return CreatedAtAction(nameof(GetVehicleById), new { id = newVehicle.VehicleId }, newVehicle);
        }
        // Get - Return created vehicle
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return Ok(vehicle);
        }
    }
}
