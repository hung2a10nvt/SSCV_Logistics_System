using System;
using System.Collections.Generic;
using System.Text;

namespace SSCV.Domain.Entities
{
    public class AlertSystem
    {
        public long AlertSystemId { get; set; }
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime Timestamp { get; set; }

    }
}
