using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDTOs.SlotModels
{
    public class ClinicSlotRegistrationModel
    {
        public int slotId { get; set; }
        public int maxAppointment { get; set; } = 1;
    }
}
