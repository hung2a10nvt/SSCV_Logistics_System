using System;
using System.Collections.Generic;
using System.Text;

namespace SSCV.Domain.Entities
{
    public class TelemetryRecord
    {
        public long RecordId { get; set; }
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public double Speed { get; set; }
        public double Longitude { get; set; }
        public double Latitude {  get; set; }
        public decimal Temperature { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
    }
}
