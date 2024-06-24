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
        SlotInfoModel? AddSlot(SlotInfoModel slot);
        SlotInfoModel? GetSlot(int slotId);
        IEnumerable<SlotInfoModel> GetAllSlot();
        SlotInfoModel? UpdateSlot(SlotInfoModel slot);
        bool DeleteSlot(int slotId);

        // Schedule Slots
        ClinicSlotInfoModel? AddClinicSlot(ClinicSlotInfoModel slot);
        ClinicSlotInfoModel? GetClinicSlot(Guid slotId);
        IEnumerable<ClinicSlotInfoModel> GetAllClinicSlot();
        ClinicSlotInfoModel? UpdateClinicSlot(ClinicSlotInfoModel slot);
        bool DeleteClinicSlot(Guid slotId);
    }
}
