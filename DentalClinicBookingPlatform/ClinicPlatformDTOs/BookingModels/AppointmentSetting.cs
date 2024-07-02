using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.BookingModels
{
    public class AppointmentSetting
    {
        public int MaxRecurring { get; set; } = 0;
        public int TimeSpan { get; set; } = 0;
        public string RepeatType { get; set; } = string.Empty;

        public const string Daily = "daily";
        public const string Weekly = "weekly";
        public const string Monthly = "monthly";
        public const string Yearly = "yearly";
    }
}
