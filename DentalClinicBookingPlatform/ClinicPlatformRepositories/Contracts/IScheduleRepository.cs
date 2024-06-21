using ClinicPlatformBusinessObject;
using ClinicPlatformDTOs.SlotModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    public interface IScheduleRepository: IDisposable
    {

        // Slots
        bool AddSlot(SlotInfoModel slot);
        SlotInfoModel? GetSlot(int slotId);
        IEnumerable<SlotInfoModel> GetAllSlot();
        bool UpdateSlot(SlotInfoModel slot);
        bool DeleteSlot(int slotId);

        // Schedule Slots
        bool AddClinicSlot(ClinicSlotInfoModel slot);
        ClinicSlotInfoModel? GetClinicSlot(Guid slotId);
        IEnumerable<ClinicSlotInfoModel> GetAllClinicSlot();
        bool UpdateClinicSlot(ClinicSlotInfoModel slot);
        bool DeleteClinicSlot(Guid slotId);
    }
}
