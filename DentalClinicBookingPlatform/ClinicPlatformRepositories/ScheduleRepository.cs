using ClinicPlatformDatabaseObject;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformRepositories.Contracts;

namespace ClinicPlatformRepositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly DentalClinicPlatformContext context;
        private bool disposedValue;

        public ScheduleRepository(DentalClinicPlatformContext context)
        {
            this.context = context;
        }

        public SlotInfoModel AddSlot(SlotInfoModel slot)
        {
            Slot slotInfo = MapToSlot(slot);
            context.Slots.Add(slotInfo);
            context.SaveChanges();

            return MapSlotToSlotModel(slotInfo);
        }

        public SlotInfoModel? GetSlot(int slotId)
        {
            var result = context.Slots.Find(slotId);

            if (result == null)
            {
                return null;
            }

            return MapSlotToSlotModel(result);
        }

        public IEnumerable<SlotInfoModel> GetAllSlot() 
        {
            return from slot in context.Slots
                   select new SlotInfoModel
                   {
                       Id = slot.Id,
                       StartTime = slot.Start,
                       EndTime = slot.End,
                   };
        }

        public SlotInfoModel? UpdateSlot(SlotInfoModel slot)
        {
            if (slot.Id != null)
            {
                var slotInfo = context.Slots.Find((int)slot.Id);
                if (slotInfo != null)
                {
                    slotInfo.Start = slot.StartTime ?? slotInfo.Start;
                    slotInfo.End = slot.EndTime ?? slotInfo.End;

                    context.Slots.Update(slotInfo);
                    return MapSlotToSlotModel(slotInfo);
                }
            }

            return null;
        }

        public bool DeleteSlot(int slotId)
        {
            Slot? slot = context.Slots.Find(slotId);

            if (slot == null)
            {
                return false;
            }

            context.Slots.Remove(slot);
            return true;
        }

        public ClinicSlotInfoModel AddClinicSlot(ClinicSlotInfoModel slot)
        {
            slot.ClinicSlotId = null;

            ClinicSlot slotInfo = MapToClinicSlot(slot);

            context.ClinicSlots.Add(slotInfo);
            context.SaveChanges();

            slotInfo.Time = context.Slots.Find(slotInfo.TimeId)!;

            return MapToClinicSlotModel(slotInfo);
        }

        public ClinicSlotInfoModel? GetClinicSlot(Guid slotId)
        {
            var result = context.ClinicSlots.Find(slotId);

            if (result != null)
            {
                result.Time = context.Slots.Find(result.TimeId)!;

                return MapToClinicSlotModel(result); 
            }
            return null;
            
        }

        public IEnumerable<ClinicSlotInfoModel> GetAllClinicSlot()
        {
            return from clinicSlot in context.ClinicSlots
                   join baseSlot in context.Slots on clinicSlot.TimeId equals baseSlot.Id
                   select new ClinicSlotInfoModel
                   {
                       ClinicSlotId = clinicSlot.SlotId,
                       SlotId = clinicSlot.TimeId,
                       Weekday = clinicSlot.Weekday,
                       ClinicId = clinicSlot.ClinicId,
                       MaxCheckup = clinicSlot.MaxCheckup,
                       MaxTreatment = clinicSlot.MaxTreatment,
                       StartTime = baseSlot.Start,
                       EndTime = baseSlot.End,
                       Status = clinicSlot.Status,
                   };
        }

        public ClinicSlotInfoModel? UpdateClinicSlot(ClinicSlotInfoModel slot)
        {
            var slotInfo = context.ClinicSlots.Find(slot.ClinicSlotId);

            if (slotInfo != null)
            {
                slotInfo.ClinicId = slot.ClinicId;
                slotInfo.Weekday = (byte) slot.Weekday;
                slotInfo.MaxCheckup = slot.MaxCheckup;
                slotInfo.MaxTreatment = slot.MaxTreatment;
                slotInfo.Status = slot.Status;
                slotInfo.TimeId = slot.SlotId;

                context.ClinicSlots.Update(slotInfo);
                context.SaveChanges();
          
                return MapToClinicSlotModel(slotInfo);
            }

            return null;
        }

        public bool DeleteClinicSlot(Guid slotId)
        {
            var clinicSlot = context.ClinicSlots.Find(slotId);

            if (clinicSlot != null)
            {
                context.ClinicSlots.Remove(clinicSlot);

                return true;

            }

            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    context.Dispose();
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
        private ClinicSlotInfoModel MapToClinicSlotModel(ClinicSlot ScheduleSlot)
        {
            return new ClinicSlotInfoModel()
            {
                ClinicSlotId = ScheduleSlot.SlotId,
                ClinicId = ScheduleSlot.ClinicId,
                SlotId = ScheduleSlot.TimeId,
                Weekday = ScheduleSlot.Weekday,
                MaxTreatment = ScheduleSlot.MaxTreatment,
                MaxCheckup = ScheduleSlot.MaxCheckup,
                StartTime = ScheduleSlot.Time.Start,
                EndTime = ScheduleSlot.Time.End,
                Status = ScheduleSlot.Status,
            };
        }

        private SlotInfoModel MapSlotToSlotModel(Slot slot)
        {
            return new SlotInfoModel()
            {
                Id = slot.Id,
                StartTime = slot.Start,
                EndTime = slot.End,
            };
        }

        private ClinicSlot MapToClinicSlot(ClinicSlotInfoModel slotInfo)
        {
            return new ClinicSlot()
            {
                SlotId = slotInfo.ClinicSlotId ?? Guid.NewGuid(),
                TimeId = slotInfo.SlotId,
                ClinicId = slotInfo.ClinicId,
                Weekday = (byte)slotInfo.Weekday,
                MaxCheckup = slotInfo.MaxCheckup,
                MaxTreatment = slotInfo.MaxTreatment,
                Status = slotInfo.Status,
            };
        }

        private Slot MapToSlot(SlotInfoModel slotInfo)
        {
            return new Slot()
            {
                Id = (int)slotInfo.Id!,
                Start = (TimeOnly)slotInfo.StartTime!,
                End = (TimeOnly)slotInfo.EndTime!,
            };
        }
    }
}
