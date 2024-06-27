using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly IBookingRepository bookingRepository;
        private readonly IUserRepository userRepository;
        private readonly IClinicRepository clinicRepository;

        private bool disposedValue;

        public ScheduleService(IScheduleRepository scheduleRepository, IBookingRepository bookingRepository, IUserRepository userRepository, IClinicRepository clinicRepository)
        {
            this.scheduleRepository = scheduleRepository;
            this.bookingRepository = bookingRepository;
            this.userRepository = userRepository;
            this.clinicRepository = clinicRepository;
        }

        public ClinicSlotInfoModel? AddNewClinicSlot(ClinicSlotInfoModel slotInfo, out string message)
        {
            slotInfo.ClinicSlotId = null;

            if (slotInfo.MaxCheckup < 0 || slotInfo.MaxTreatment < 0)
            {
                message = "Maximum checkup or maximum treatment can not be smaller than 0";
                return null;
            }
            
            var allSlot = scheduleRepository.GetAllClinicSlot();
            var allBaseSlot = scheduleRepository.GetAllSlot();

            if (allSlot.Any(x => x.ClinicId == slotInfo.ClinicId && x.SlotId == slotInfo.SlotId && x.Weekday == slotInfo.Weekday!))
            {
                message = $"Clinic {slotInfo.ClinicId} already has created this slot.";
                return null;
            }

            if (allSlot.Any(x => x.ClinicId == slotInfo.ClinicId && x.SlotId == slotInfo.SlotId && x.Weekday == slotInfo.Weekday!))
            {
                message = $"Clinic {slotInfo.ClinicId} already has created this slot.";
                return null;
            }

            message = "Created slot successfully.";
            return scheduleRepository.AddClinicSlot(slotInfo);
        }

        public bool AddNewSlot(SlotInfoModel slotInfo, out string message)
        {
            var allBaseSlot = scheduleRepository.GetAllSlot();

            if (allBaseSlot.Any(x => x.StartTime == x.StartTime && x.EndTime == slotInfo.EndTime))
            {
                message = "This slot is already defined!";
                return false;
            }

            message = "Successfully added new base slot definition";
            return scheduleRepository.AddSlot(slotInfo) != null;
        }

        public SlotInfoModel? TryAddNewSlot(SlotInfoModel slotInfo, out string message)
        {
            var allBaseSlot = scheduleRepository.GetAllSlot();

            var tempt = allBaseSlot.Where(x => x.StartTime == x.StartTime && x.EndTime == slotInfo.EndTime).FirstOrDefault();
            if (tempt != null)
            {
                message = "This slot is already defined!";
                return tempt;
            }

            message = "Successfully added new slot";
            return null;
        }

        public bool DeleteClinicSlot(Guid slotId)
        {
            return scheduleRepository.DeleteClinicSlot(slotId);
        }

        public bool DeleteSlot(int slotId)
        {
            return scheduleRepository.DeleteSlot(slotId);
        }

        public IEnumerable<ClinicSlotInfoModel> GetAllClinicSlot(int clinicId)
        {
            return scheduleRepository.GetAllClinicSlot().Where(x => x.ClinicId == clinicId);
        }

        public IEnumerable<ClinicSlotInfoModel> GetAllSlot()
        {
            return scheduleRepository.GetAllClinicSlot();
        }

        public IEnumerable<ClinicSlotInfoModel> GetAllWithMaxCheckup(int clinicId, int max)
        {
            return scheduleRepository.GetAllClinicSlot().Where(x => x.ClinicId == clinicId && x.MaxCheckup <= max);
        }

        public IEnumerable<ClinicSlotInfoModel> GetAllWithMaxTreatment(int clinicId, int max)
        {
            return scheduleRepository.GetAllClinicSlot().Where(x => x.ClinicId == clinicId && x.MaxTreatment <= max);
        }

        public ClinicSlotInfoModel? GetClinicSlotById(Guid slotId)
        {
            return scheduleRepository.GetClinicSlot(slotId);
        }

        public IEnumerable<ClinicSlotInfoModel> GetClinicSlotInRange(TimeOnly start, TimeOnly end)
        {
            return scheduleRepository.GetAllClinicSlot().Where(x => x.StartTime < start && end < x.EndTime);
        }

        public IEnumerable<ClinicSlotInfoModel> GetClinicSlotInRange(int clinicId, TimeOnly start, TimeOnly end)
        {
            return scheduleRepository.GetAllClinicSlot().Where(x => x.StartTime < start && end < x.EndTime && x.ClinicId == clinicId);
        }

        public ClinicSlotInfoModel? UpdateClinicSlot(ClinicSlotInfoModel slotInfo, out string message)
        {
            if (slotInfo.ClinicSlotId == null)
            {
                message = $"Missing required information: {(slotInfo.ClinicSlotId == null ? "ClinicSlotId" : "")}.";
                return null;
            }

            var clinic = clinicRepository.GetClinic(slotInfo.ClinicId);

            if (clinic == null)
            {
                message = $"No information found for clinic with Id {slotInfo.ClinicId}";
            }

            var clinicSlot = scheduleRepository.GetClinicSlot((Guid) slotInfo.ClinicSlotId!);

            if (clinicSlot == null)
            {
                message = $"Slot information not found for Id {slotInfo.ClinicSlotId}.";
                return null;
            }

            var baseSlot = scheduleRepository.GetSlot((int)slotInfo.SlotId);

            if (baseSlot == null)
            {
                message = "New slot Id is not found for {slotInfo.SlotId}";
                return null;
            }

            clinicSlot.SlotId = clinicSlot.SlotId;
            clinicSlot.MaxCheckup = slotInfo.MaxCheckup;
            clinicSlot.MaxTreatment = slotInfo.MaxTreatment;
            clinicSlot.Weekday = slotInfo.Weekday;
            clinicSlot.Status = slotInfo.Status;

            message = "Updated clinic slot!";
            return scheduleRepository.UpdateClinicSlot(clinicSlot!);
        }

        public List<ClinicSlotInfoModel>? AvailableSlotOnDate(DateTime date, int dentistId, bool forTreatment, out string message) 
        {
            var dentist = userRepository.GetUserWithDentistID(dentistId);

            if (dentist == null)
            {
                message = "Can not find dentist information";
                return null;
            }


            var allBooking = bookingRepository.GetAll().Where(x => x.AppointmentDate == DateOnly.FromDateTime(date) && x.DentistId == dentistId && x.Status != "canceled");

            var allClinicSlot = scheduleRepository.GetAllClinicSlot().Where(x => x.ClinicId == dentist.ClinicId && x.Weekday == ((int)date.DayOfWeek)).ToList();

            for ( var i = allClinicSlot.Count() - 1; i >= 0 ; i--)
            {
                ClinicSlotInfoModel temptSlot = allClinicSlot.ElementAt(i);

                if (forTreatment && temptSlot.MaxTreatment <= allBooking.Where(x => x.ClinicSlotId == temptSlot.ClinicSlotId && x.Type == "treatment").Count())
                {
                    allClinicSlot.Remove(temptSlot);
                }
                else if (temptSlot.MaxCheckup <= allBooking.Where(x => x.ClinicSlotId == temptSlot.ClinicSlotId && x.Type == "checkup").Count())
                {
                    allClinicSlot.Remove(temptSlot);
                }
            }

            message = $"On {date.ToString("dddd")} {date.Month} {date.Year}, dentist {dentist.Fullname} has {allClinicSlot.Count()} available slots.";

            return allClinicSlot;
        }

        public bool UpdateSlot(SlotInfoModel slotInfo, out string message)
        {
            if (slotInfo.Id == null || slotInfo.StartTime == null || slotInfo.EndTime == null)
            {
                message = $"Missing required information: {(slotInfo.Id == null ? "Id" : "")} {(slotInfo.StartTime == null ? "StartTime" : "")} {(slotInfo.EndTime == null ? "EndTime" : "")}.";
                return false;
            }

            if (slotInfo.EndTime < slotInfo.StartTime)
            {
                message = $"The EndTime time is before StartTime! ({slotInfo.StartTime} > {slotInfo.EndTime})";
                return false;
            }

            var clinicSlot = scheduleRepository.GetSlot((int)slotInfo.Id!);

            if (clinicSlot == null)
            {
                message = $"Slot information not found for Id {slotInfo.Id}.";
            }

            message = "Updated clinic slot!";
            return scheduleRepository.UpdateSlot(clinicSlot!) != null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    scheduleRepository.Dispose();
                }

                disposedValue = true;
            }
        }
      
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
