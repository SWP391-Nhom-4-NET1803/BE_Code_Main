using Core.HttpModels.ObjectModels.BookingModels;
using Core.HttpModels.ObjectModels.SlotModels;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.BookingService
{
    public class BookingService: IBookingService
    {
        private UnitOfWork _unitOfWork;

        private bool disposedValue;

        public enum Weeekdays
        {
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday
        }

        public BookingService(DentalClinicPlatformContext context)
        {

            this._unitOfWork = new UnitOfWork(context);
        }

        public bool CreateOneTimeAppointment(BookingModel bookInfo, out string message) 
        {
            var customerInfo = _unitOfWork._context.Customers.Include(x => x.User).Where(x => x.CustomerId == bookInfo.CustomerId).FirstOrDefault();

            var dentistInfo = _unitOfWork._context.ClinicStaffs.Include(x => x.User).Where(x => x.StaffId == bookInfo.DentistId).FirstOrDefault();

            var dentalClinicInfo = _unitOfWork._context.Clinics.Where(x => x.ClinicId == bookInfo.ClinicId).FirstOrDefault();

            var slotInfo = _unitOfWork._context.ScheduledSlots.Include(x => x.Slot).Where(x => x.ScheduleSlotId == bookInfo.timeSlotId && x.ClinicId == bookInfo.ClinicId).FirstOrDefault();

            if (customerInfo == null)
            {
                message = "Customer information not found!";
                return false;
            }

            if (dentistInfo == null)
            {
                message = "Dentist information not found!";
                return false;
            }

            if (dentalClinicInfo == null)
            {
                message = "Clinic information not found!";
                return false;
            }

            if (slotInfo == null)
            {
                message = "Slot information not found!";
                return false;
            }

            var newBook = new Booking()
            {
                BookingType = "Khám t?ng quát",
                AppointmentDate = bookInfo.appointmentDate,
                ClinicId = dentalClinicInfo.ClinicId,
                CustomerId = customerInfo.CustomerId,
                DentistId = dentistInfo.StaffId,
                CreationDate = DateTime.Now,
                ScheduleSlotId = slotInfo.ScheduleSlotId,
                Status = false
            };

            _unitOfWork._context.Bookings.Add(newBook);

            message = "Suceed";
            return true;
        }

        public bool CreateOneTimeAppointment(BookingRegistrationModel bookInfo, out string message)
        {
            var customerInfo = _unitOfWork._context.Customers.Include(x => x.User).Where(x => x.CustomerId == bookInfo.CustomerId).FirstOrDefault();

            var dentistInfo = _unitOfWork._context.ClinicStaffs.Include(x => x.User).Where(x => x.StaffId == bookInfo.DentistId && x.ClinicId == bookInfo.ClinicId).FirstOrDefault();

            var dentalClinicInfo = _unitOfWork._context.Clinics.Where(x => x.ClinicId == bookInfo.ClinicId).FirstOrDefault();

            var slotInfo = _unitOfWork._context.ScheduledSlots.Include(x => x.Slot).Where(x => x.ScheduleSlotId == bookInfo.TimeSlotId && x.ClinicId == bookInfo.ClinicId).FirstOrDefault();

            if (dentalClinicInfo == null)
            {
                message = $"No clinic found for id {bookInfo.ClinicId}.";
                return false;
            }

            if (customerInfo == null)
            {
                message = $"No customer with CustomerId {bookInfo.CustomerId} exist.";
                return false;
            }

            if (dentistInfo == null)
            {
                message = $"No dentist with ClinicStaffId {bookInfo.DentistId} exist for clinic {dentalClinicInfo.Name} with Id {bookInfo.ClinicId}.";
                return false;
            }


            if (slotInfo == null)
            {
                message = $"Slot information not found with provided ID {bookInfo.TimeSlotId} for clinc {dentalClinicInfo.Name} with Id {bookInfo.ClinicId}";
                return false;
            }

            var slotBookCount = _unitOfWork._context.Bookings.Where(x => x.DentistId == bookInfo.DentistId && x.AppointmentDate == bookInfo.AppointmentDate && x.ScheduleSlotId == bookInfo.TimeSlotId).Count();

            if (slotInfo.MaxAppointments <= slotBookCount)
            {
                message = "The current slot is fully booked!";
                return false;
            }

            var newBook = new Booking()
            {
                BookingType = "Khám tổng quát",
                AppointmentDate = bookInfo.AppointmentDate,
                ClinicId = dentalClinicInfo.ClinicId,
                CustomerId = customerInfo.CustomerId,
                DentistId = dentistInfo.StaffId,
                CreationDate = DateTime.Now,
                ScheduleSlotId = slotInfo.ScheduleSlotId,
                Status = false
            };

            _unitOfWork.BookingRepository.Add(newBook);

            message = "Suceed";
            return true;
        }

        public bool CreateSimpleAppointment(BookingModel bookInfo, IEnumerable<ClinicService> services, out string message)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ScheduledSlot> GetAllSlotForDay(DateOnly date, int clinicId)
        {
            return _unitOfWork._context.ScheduledSlots.Where(x => x.ClinicId == clinicId && x.DateOfWeek == (int)date.DayOfWeek).ToList();
        }

        public IEnumerable<ScheduledSlot> GetFreeSlotOnDay(DateOnly date, int clinicId)
        {
            return _unitOfWork._context.ScheduledSlots
                .Include(x => x.Bookings.Where(y => y.AppointmentDate == date))
                .Where(x => x.ClinicId == clinicId && x.DateOfWeek == (int)date.DayOfWeek && x.Bookings.Count < x.MaxAppointments)
                .ToList();
        }

        public IEnumerable<ScheduledSlot> GetSlotForWeekDay(int weekday, int clinicId)
        {
            return _unitOfWork._context.ScheduledSlots.Include(x => x.Bookings.Where(y => y.AppointmentDate >= DateOnly.FromDateTime(DateTime.Now)))
                .Where(x => x.ClinicId == clinicId && x.DateOfWeek == weekday && x.Bookings.Count < x.MaxAppointments)
                .ToList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<Booking> getClinicBooking(int clinicId, bool onlyFuture)
        {
            return _unitOfWork.BookingRepository.getClinicBooking(clinicId, onlyFuture);
        }

        public bool CreateClinicSlot(ClinicSlotInfoModel slot, out string message)
        {
            var SlotInfo = _unitOfWork._context.Slots.Where(x => x.SlotId == slot.SlotId).FirstOrDefault();

            var ClinicInfo = _unitOfWork.ClinicRepository.GetById(slot.ClinicId);

            if (SlotInfo == null)
            {
                message = $"Unknown selected based slot for id {slot.SlotId}";
                return false;
            }

            if (ClinicInfo == null)
            {
                message = $"Unknown clinic for id {slot.ClinicId}";
                return false;
            }

            var ExistSlotInfo = _unitOfWork._context.ScheduledSlots
                .Where(x => x.ClinicId == slot.ClinicId && x.DateOfWeek == slot.Weekday && x.SlotId == slot.SlotId)
                .FirstOrDefault();

            if (ExistSlotInfo != null) 
            {
                message = $"Slot existed with Id {ExistSlotInfo.ScheduleSlotId}";
                return false;
            }

            // Getting byte instead of int.
            byte[] intBytes = BitConverter.GetBytes(slot.Weekday);


            for (var x = 0; x < intBytes.Count(); x++) { Console.WriteLine($"{x}: {intBytes[x]}"); }
            
            if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);

            ScheduledSlot newClinicSlot = new ScheduledSlot()
            {
                ClinicId = slot.ClinicId,
                SlotId = slot.SlotId,
                DateOfWeek = (byte)slot.Weekday,
                MaxAppointments = slot.MaxAppointment
            };

            _unitOfWork.ClinicRepository.CreateClinicSlot(newClinicSlot);
            message = "Success";
            return true;
        }

        public bool CreateMultipleClinicSlot(IEnumerable<ClinicSlotInfoModel> slots, out string message)
        {
            foreach (var slot in slots)
            {
                var SlotInfo = _unitOfWork._context.Slots.Where(x => x.SlotId == slot.SlotId).FirstOrDefault();

                var ClinicInfo = _unitOfWork.ClinicRepository.GetById(slot.ClinicId);

                if (SlotInfo == null)
                {
                    message = $"Unknown selected based slot for id {slot.SlotId}";
                    return false;
                }

                if (ClinicInfo == null)
                {
                    message = $"Unknown clinic for id {slot.ClinicId}";
                    return false;
                }

                var ExistSlotInfo = _unitOfWork._context.ScheduledSlots
                    .Where(x => x.ClinicId == slot.ClinicId && x.DateOfWeek == slot.Weekday && x.SlotId == slot.SlotId)
                    .FirstOrDefault();

                if (ExistSlotInfo != null)
                {
                    message = $"Slot existed with Id {ExistSlotInfo.ScheduleSlotId}";
                    return false;
                }

                // Getting byte instead of int.
                byte[] intBytes = BitConverter.GetBytes(slot.Weekday);

                if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);

                ScheduledSlot newClinicSlot = new ScheduledSlot()
                {
                    ClinicId = slot.ClinicId,
                    SlotId = slot.SlotId,
                    DateOfWeek = intBytes[0],
                    MaxAppointments = slot.MaxAppointment
                };
                _unitOfWork.ClinicRepository.CreateClinicSlot(newClinicSlot);
            }
            message = "Success";
            return true;
        }

        public IEnumerable<ScheduledSlot> GetClinicSlot(int clinicId)
        {
            return _unitOfWork._context.ScheduledSlots.Where(x => x.ClinicId == clinicId).Include(x => x.Slot).ToList();
        }
    }
}
