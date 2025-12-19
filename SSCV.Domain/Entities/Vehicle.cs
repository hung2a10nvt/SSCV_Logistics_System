using System;
using System.Collections.Generic;
using System.Text;

namespace SSCV.Domain.Entities
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public ICollection<TelemetryRecord> TelemetryRecords { get; set; } = new List<TelemetryRecord>();
    }
}
