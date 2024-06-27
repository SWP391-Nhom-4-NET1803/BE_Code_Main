using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.BookingModels
{
    public class BookedServiceInfoModel
    {
        public Guid AppointmentId { get; set; }

        public int Price { get; set; }

        public string Name { get; set; } = null!;

        public Guid ClinicServiceId { get; set; }

        public int ServiceCategoryId { get; set; }
    }
}
