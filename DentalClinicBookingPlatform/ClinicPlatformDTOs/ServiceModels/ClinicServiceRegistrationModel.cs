using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.ServiceModels
{
    public class ClinicServiceRegistrationModel
    {
        public int ServiceCategory { get; set; }
        public string? ServiceName { get; set; } = null!;
        public string ServiceDescription { get; set; } = null!;
        public int servicePrice { get; set; }
        public int clinicId { get; set; }
    }
}
