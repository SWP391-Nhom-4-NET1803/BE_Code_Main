using ClinicPlatformDTOs.SlotModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IScheduleService: IDisposable
    {
        IEnumerable<ClinicSlotInfoModel> GetAllSlot();

        ClinicSlotInfoModel? GetClinicSlotById(Guid slotId);

        IEnumerable<ClinicSlotInfoModel> GetAllClinicSlot(int clinicId);

        IEnumerable<ClinicSlotInfoModel> GetAllWithMaxCheckup(int clinicId, int max);
        public IEnumerable<ClinicSlotInfoModel> GetAllWithMaxTreatment(int clinicId, int max);

        IEnumerable<ClinicSlotInfoModel> GetClinicSlotInRange(TimeOnly start, TimeOnly end);

        IEnumerable<ClinicSlotInfoModel> GetClinicSlotInRange(int clinicId, TimeOnly start, TimeOnly end);

        ClinicSlotInfoModel? AddNewClinicSlot(ClinicSlotInfoModel slotInfo, out string message);

        ClinicSlotInfoModel? UpdateClinicSlot(ClinicSlotInfoModel slotInfo, out string message);

        public List<ClinicSlotInfoModel>? AvailableSlotOnDate(DateTime date, int dentistId, bool forTreatment, out string message);
        bool DeleteClinicSlot(Guid slotId);

        // Just dont, It's useless.
        bool AddNewSlot(SlotInfoModel slotInfo, out string message);

        SlotInfoModel? TryAddNewSlot(SlotInfoModel slotInfo, out string message);

        bool UpdateSlot(SlotInfoModel slotInfo, out string message);

        bool DeleteSlot(int slotId);
    }
}
