using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacySystem.Core.Entities
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; }
        public string? Error { get; set; }
        public string? TraceId { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
    }
}
