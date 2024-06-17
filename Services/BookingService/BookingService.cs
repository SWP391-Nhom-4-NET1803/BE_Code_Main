using Core.HttpModels.ObjectModels.BookingModels;
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
                message = $"No clinic found for {bookInfo.ClinicId}.";
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
            return _unitOfWork._context.ScheduledSlots.Include(x => x.Bookings.Where(y => y.AppointmentDate == date))
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

        public IEnumerable<ScheduledSlot> GetSlotFreeInRange(DateOnly start, DateOnly end, int clinicId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Booking> getClinicBooking(int clinicId, bool onlyFuture)
        {
            return _unitOfWork.BookingRepository.getClinicBooking(clinicId, onlyFuture);
        }
    }
}
