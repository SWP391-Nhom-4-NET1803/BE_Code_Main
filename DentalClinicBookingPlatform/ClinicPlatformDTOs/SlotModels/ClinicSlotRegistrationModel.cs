using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.SlotModels
{
    public class ClinicSlotRegistrationModel
    {
        public int ClinicId { get; set; }
        public int SlotId { get; set; }
        public int MaxAppointment { get; set; } = 1;

        public int Weekday { get; set; }
    }
}
