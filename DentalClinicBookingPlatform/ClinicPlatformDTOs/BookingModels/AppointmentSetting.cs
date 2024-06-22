using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class AppointmentSetting
    {
        public bool IsRecurring { get; set; } = false;
        public int MaxRecurring { get; set; } = 0;
        public int RepeatYear { get; set; }
        public int RepeatMonth { get; set; }
        public int RepeatWeek { get; set; }
        public int RepeatDay { get; set; }
        public int DateOfWeek { get; set; }
    }
}
