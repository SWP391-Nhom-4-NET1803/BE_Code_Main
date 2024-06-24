using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.SlotModels
{
    public class ClinicSlotInfoModel
    {
        public Guid? ClinicSlotId { get; set; }
        public int ClinicId { get; set; }
        public int MaxCheckup { get; set; }
        public int MaxTreatment { get; set; }
        public int Weekday { get; set; }
        public int SlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool Status { get; set; }

        public enum Weekdays
        {
            Sunday = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6,
        }
    }
}
