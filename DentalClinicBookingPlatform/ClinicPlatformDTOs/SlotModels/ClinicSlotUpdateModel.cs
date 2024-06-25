using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.SlotModels
{
    public class ClinicSlotUpdateModel
    {
        public Guid slotId { get; set; }
        public int MaxTreatement {  get; set; }
        public int MaxCheckup {get; set;}
        public bool Status {  get; set; }
    }
}
