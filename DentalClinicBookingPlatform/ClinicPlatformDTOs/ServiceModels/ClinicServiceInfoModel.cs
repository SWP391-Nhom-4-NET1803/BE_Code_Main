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
        public string? Name { get; set; } = null;
        public string? CustomName { get; set; } = null;
        public string? Description { get; set; } = null;
        public long? Price { get; set; } = null;
        public int? ClinicId { get; set; }
        public int? CategoryId { get; set; }
        public bool Available { get; set; } = false;
        public bool Removed { get; set; } = false;
        public bool TreatAtFirstAppointment { get; set; } = false;
    }
}
