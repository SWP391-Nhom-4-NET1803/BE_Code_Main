using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.ClinicModels
{
    public class ClinicServiceInfoModel
    {
        public Guid ClinicServiceId { get; set; }
        public string? Description { get; set; } = null;
        public float? Price { get; set; } = null;
        public int ClinicId { get; set; }
        public int ServiceId { get; set; }
        public string? Name { get; set; } = null;
    }
}
