namespace SSCV.Api.Models
{
    public class CreateVehicleRequest
    {
        public string LicensePlate { get; set; } = null!;
        public string VehicleType { get; set; } = string.Empty;
    }
}
