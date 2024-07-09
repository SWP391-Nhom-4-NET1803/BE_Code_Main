using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.PayementModels
{
    public class VNPayInfoModel
    {
        public string orderInfo { get; set; } = null!;
        public string? returnUrl { get; set; } = null!;
        public Guid appointmentId { get; set; }
    }
}
