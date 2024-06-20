﻿using ClinicPlatformDTOs.SlotModels;
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

        private bool disposedValue;

        public ScheduleService(IScheduleRepository scheduleRepository)
        {
            this.scheduleRepository = scheduleRepository;
        }

        public bool RegisterClinicSlot(ClinicSlotRegistrationModel slotInfo, out string message)
        {
            return AddNewClinicSlot(new ClinicSlotInfoModel()
            {
                ClinicId = slotInfo.ClinicId,
                MaxAppointment = slotInfo.MaxAppointment,
                SlotId = slotInfo.SlotId,
                Weekday = slotInfo.Weekday
            }, out message);
        }

        public bool AddNewClinicSlot(ClinicSlotInfoModel slotInfo, out string message)
        {
            // Some shit happens when I dont do this. (State change tracking)
            slotInfo.ClinicSlotId = null;

            if (slotInfo.Weekday == null || slotInfo.ClinicId == null || slotInfo.SlotId == null)
            {
                message = $"Information missing: {(slotInfo.Weekday == null ? "Weekday, " : "")} {(slotInfo.SlotId == null ? "SlotId, " : "")} {(slotInfo.ClinicId == null ? "ClinicId, " : "")}";
                return false;
            }

            var allSlot = scheduleRepository.GetAllClinicSlot();
            var allBaseSlot = scheduleRepository.GetAllSlot();

            if (allSlot.Any(x => x.ClinicId == slotInfo.ClinicId && x.SlotId == slotInfo.SlotId && x.Weekday == (byte) slotInfo.Weekday!))
            {
                message = $"Clinic {slotInfo.ClinicId} already has created this slot.";
                return false;
            }

            if (allSlot.Any(x => x.ClinicId == slotInfo.ClinicId && x.SlotId == slotInfo.SlotId && x.Weekday == (byte)slotInfo.Weekday!))
            {
                message = $"Clinic {slotInfo.ClinicId} already has created this slot.";
                return false;
            }

            message = "";
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
            return scheduleRepository.AddSlot(slotInfo);
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

        public IEnumerable<ClinicSlotInfoModel> GetAllWithMaxAppointment(int clinicId, int max)
        {
            return scheduleRepository.GetAllClinicSlot().Where(x => x.ClinicId == clinicId && x.MaxAppointment <= max);
        }

        public ClinicSlotInfoModel? GetClinicSlotById(Guid slotId)
        {
            return scheduleRepository.GetClinicSlot(slotId);
        }

        public IEnumerable<ClinicSlotInfoModel> GetClinicSlotInRange(TimeOnly start, TimeOnly end)
        {
            return scheduleRepository.GetAllClinicSlot().Where(x => x.start < start && end < x.end);
        }

        public IEnumerable<ClinicSlotInfoModel> GetClinicSlotInRange(int clinicId, TimeOnly start, TimeOnly end)
        {
            return scheduleRepository.GetAllClinicSlot().Where(x => x.start < start && end < x.end && x.ClinicId == clinicId);
        }

        public bool UpdateClinicSlot(ClinicSlotInfoModel slotInfo, out string message)
        {
            if (slotInfo.ClinicSlotId == null)
            {
                message = $"Missing required information: {(slotInfo.ClinicSlotId == null ? "ClinicSlotId" : "")}.";
                return false;
            }

            var clinicSlot = scheduleRepository.GetClinicSlot((Guid) slotInfo.ClinicSlotId!);

            if (clinicSlot == null)
            {
                message = $"Slot information not found for Id {slotInfo.ClinicSlotId}.";
                return false;
            }

            if (slotInfo.SlotId != null)
            {
                var baseSlot = scheduleRepository.GetSlot((int)slotInfo.SlotId);

                if (baseSlot == null)
                {
                    message = "New slot Id is not found for {slotInfo.SlotId}";
                    return false;
                }
                clinicSlot.SlotId = clinicSlot.SlotId;
            }

            clinicSlot.MaxAppointment = slotInfo.MaxAppointment ?? slotInfo.MaxAppointment;
            clinicSlot.Weekday = slotInfo.Weekday ?? slotInfo.MaxAppointment;

            message = "Updated clinic slot!";
            return scheduleRepository.UpdateClinicSlot(clinicSlot!);
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
                message = $"The end time is before start time! ({slotInfo.StartTime} > {slotInfo.EndTime})";
                return false;
            }

            var clinicSlot = scheduleRepository.GetSlot((int)slotInfo.Id!);

            if (clinicSlot == null)
            {
                message = $"Slot information not found for Id {slotInfo.Id}.";
            }

            message = "Updated clinic slot!";
            return scheduleRepository.UpdateSlot(clinicSlot!);
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