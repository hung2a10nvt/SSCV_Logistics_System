namespace SSCV.Api.Models
{
    public class TelemetryDto
    {
        public int VehicleId { get; set; }
        public double Longitude { get; set; }
        public double Latiton { get; set; }
        public decimal Temperature { get; set; }
        public double Speed { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
