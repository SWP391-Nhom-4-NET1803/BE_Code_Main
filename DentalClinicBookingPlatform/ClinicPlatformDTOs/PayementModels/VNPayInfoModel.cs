using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.PayementModels
{
    public class VNPayInfoModel
    {
        public string amount { get; set; } = string.Empty;
        public string info { get; set; } = string.Empty;
        public string order_info { get; set; } = string.Empty;
    }
}
