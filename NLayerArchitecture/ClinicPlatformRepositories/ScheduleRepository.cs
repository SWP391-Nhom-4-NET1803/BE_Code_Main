using ClinicPlatformBusinessObject;
using ClinicPlatformDAOs;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformRepositories.Contracts;

namespace ClinicPlatformRepositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ScheduledSlotDAO clinicSlotDAO;
        private readonly SlotDAO slotDAO;
        private bool disposedValue;

        public ScheduleRepository()
        {
            this.clinicSlotDAO = new ScheduledSlotDAO();
            this.slotDAO = new SlotDAO();
        }

        public bool AddSlot(SlotInfoModel slot)
        {
            var slotInfo = MapSlotModelToSlot(slot);
            if (slotDAO.AddSlot(slotInfo) != null)
            {
                return true;
            }

            return false;

        }

        public SlotInfoModel? GetSlot(int slotId)
        {
            var result = slotDAO.GetSlot(slotId);

            if (result == null)
            {
                return null;
            }

            return MapSlotToSlotModel(result);
        }

        public IEnumerable<SlotInfoModel> GetAllSlot() 
        {
            return from slot in slotDAO.GetAllSlot()
                   select new SlotInfoModel
                   {
                       Id = slot.SlotId,
                       StartTime = slot.StartTime,
                       EndTime = slot.EndTime,
                   };
        }

        public bool UpdateSlot(SlotInfoModel slot)
        {
            if (slot.Id != null)
            {
                var slotInfo = slotDAO.GetSlot((int)slot.Id);
                if (slotInfo != null)
                {
                    slotInfo.StartTime = slot.StartTime ?? slotInfo.StartTime;
                    slotInfo.EndTime = slot.EndTime ?? slotInfo.EndTime;
                    slotDAO.UpdateSlot(slotInfo);

                    return true;
                }
            }

            return false;
        }

        public bool DeleteSlot(int slotId)
        {
            slotDAO.DeleteSlot(slotId);

            return true;
        }

        public bool AddClinicSlot(ClinicSlotInfoModel slot)
        {
            return clinicSlotDAO.AddScheduledSlot(MapClinicSlotModelToScheduleSlot(slot)) != null;
        }

        public ClinicSlotInfoModel? GetClinicSlot(Guid slotId)
        {
            var result = clinicSlotDAO.GetScheduledSlot(slotId);
            return result != null ? MapScheduleSlotToClinicSlotModel(result) : null; 
        }

        public IEnumerable<ClinicSlotInfoModel> GetAllClinicSlot()
        {
            return from clinicSlot in clinicSlotDAO.GetAllScheduledSlot()
                   join baseSlot in slotDAO.GetAllSlot() on clinicSlot.SlotId equals baseSlot.SlotId
                   select new ClinicSlotInfoModel
                   {
                       ClinicSlotId = clinicSlot.ScheduleSlotId,
                       SlotId = clinicSlot.SlotId,
                       Weekday = clinicSlot.DateOfWeek,
                       ClinicId = clinicSlot.ClinicId,
                       MaxAppointment = clinicSlot.MaxAppointments,
                       start = baseSlot.StartTime,
                       end = baseSlot.EndTime,
                   };
        }

        public bool UpdateClinicSlot(ClinicSlotInfoModel slot)
        {
            if (slot.ClinicSlotId != null)
            {
                var slotInfo = clinicSlotDAO.GetScheduledSlot((Guid) slot.ClinicSlotId);
                if (slotInfo != null)
                {
                    slotInfo.DateOfWeek = (byte?) slot.Weekday ?? slotInfo.DateOfWeek;
                    slotInfo.SlotId = slot.SlotId ?? slotInfo.SlotId;
                    slotInfo.MaxAppointments = slot.MaxAppointment ?? slotInfo.MaxAppointments;

                    clinicSlotDAO.UpdateScheduledSlot(slotInfo);
                    return true;
                }
            }

            return false;
        }

        public bool DeleteClinicSlot(Guid slotId)
        {
            clinicSlotDAO.DeleteScheduledSlot(slotId);

            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    slotDAO.Dispose();
                    clinicSlotDAO.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Mappers
        private ClinicSlotInfoModel MapScheduleSlotToClinicSlotModel(ScheduledSlot ScheduleSlot)
        {
            return new ClinicSlotInfoModel()
            {
                ClinicSlotId = ScheduleSlot.ScheduleSlotId,
                ClinicId = ScheduleSlot.ClinicId,
                SlotId = ScheduleSlot.SlotId,
                Weekday = ScheduleSlot.DateOfWeek,
            };
        }

        private SlotInfoModel MapSlotToSlotModel(Slot slot)
        {
            return new SlotInfoModel()
            {
                Id = slot.SlotId,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
            };
        }

        private ScheduledSlot MapClinicSlotModelToScheduleSlot(ClinicSlotInfoModel slotInfo)
        {
            return new ScheduledSlot()
            {
                ScheduleSlotId = (Guid)slotInfo.ClinicSlotId!,
                SlotId = (int)slotInfo.SlotId!,
                ClinicId = slotInfo.ClinicId ?? 0,
                DateOfWeek = (byte)slotInfo.Weekday!,
                MaxAppointments = (int)slotInfo.MaxAppointment!,
            };
        }

        private Slot MapSlotModelToSlot(SlotInfoModel slotInfo)
        {
            return new Slot()
            {
                SlotId = (int)slotInfo.Id!,
                StartTime = (TimeOnly)slotInfo.StartTime!,
                EndTime = (TimeOnly)slotInfo.EndTime!,
            };
        }
    }
}
