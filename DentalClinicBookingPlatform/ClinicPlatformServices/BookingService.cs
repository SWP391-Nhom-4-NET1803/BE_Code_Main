using ClinicPlatformDatabaseObject;
using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.BookingModels;
using ClinicPlatformRepositories;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository bookingRepository;
        private readonly IClinicServiceRepository clinicServiceRepository;
        private readonly IUserRepository userRepository;
        private readonly IClinicRepository clinicRepository;
        private readonly IScheduleRepository scheduleRepository;

        private bool disposedValue;

        public BookingService(IBookingRepository bookingRepository, IUserRepository userRepository, IClinicRepository clinicRepository, IClinicServiceRepository serviceRepository, IScheduleRepository scheduleRepository)
        {
            this.bookingRepository = bookingRepository;
            this.userRepository = userRepository;
            this.clinicRepository = clinicRepository;
            this.clinicServiceRepository = serviceRepository;
            this.scheduleRepository = scheduleRepository;
        }

        // Updates

        public AppointmentInfoModel? CancelBooking(Guid id, out string message)
        {
            AppointmentInfoModel? bookInfo = bookingRepository.GetBooking(id);
            
            if (bookInfo == null)
            {
                message = $"No booking information for Id {id}";
                return null;
            }

            if (bookInfo.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
            {
                message = $"This appointment is already in the past!";
                return null;
            }

            if (bookInfo.Status == "booked")
            {
                bookInfo.Status = "canceled";
                bookingRepository.UpdateBookingInfo(bookInfo);

                message = $"Successfully canceled appointment for booking {id}";
                return bookInfo;
                
            }

            message = $"This appointment state can not be changed!";
            return null;
        }

        public AppointmentInfoModel? ChangeDate(Guid bookId, DateOnly newDate, out string message)
        {
            AppointmentInfoModel? bookInfo = bookingRepository.GetBooking(bookId);

            if (bookInfo == null)
            {
                message = $"Can not find information for booking {bookId}";
                return null;
            }

            if (bookInfo.Status == "finished" || bookInfo.Status == "canceled")
            {
                message = "Can not change an already finished or canceled appointment.";
                return null;
            }

            if (newDate <= DateOnly.FromDateTime(DateTime.Now))
            {
                message = "Can not change to a date in the past.";
                return null;
            }

            var AppointmentOnSlot = bookingRepository.GetAll().Where(x => x.AppointmentDate == newDate && x.ClinicSlotId == x.ClinicSlotId && x.DentistId == bookInfo.DentistId);
            var slotInfo = scheduleRepository.GetClinicSlot(bookInfo.ClinicSlotId);

            if (slotInfo == null)
            {
                message = "Can not find information about the selected slot";
                return null;
            }

            if (bookInfo.Type == "checkup" && AppointmentOnSlot.Where(x => x.Type == "checkup").Count() < slotInfo.MaxCheckup)
            {
                bookInfo.AppointmentDate = newDate;
                bookingRepository.UpdateBookingInfo(bookInfo);
                message = $"Changed booking date to {newDate.ToString("d/M/yyyy")}.";
                return bookInfo;
            }

            if (bookInfo.Type == "treatment" && AppointmentOnSlot.Where(x => x.Type == "treatment").Count() < slotInfo.MaxCheckup)
            {
                bookInfo.AppointmentDate = newDate;
                bookingRepository.UpdateBookingInfo(bookInfo);
                message = $"Changed booking date to {newDate.ToString("d/M/yyyy")}.";
                return bookInfo;
            }

            message = "Error while updating slot.";
            return null;
            
        }

        public AppointmentInfoModel? ChangeSlot(Guid bookId, Guid slotId, out string message)
        {
            AppointmentInfoModel? bookInfo = bookingRepository.GetBooking(bookId);

            if (bookInfo == null)
            {
                message = $"No booking information for Id {bookId}";
                return null;
            }

            if (bookInfo.Status == "finished" && bookInfo.Status == "canceled")
            {
                message = $"Can not change a finished or canceled appointment!";
                return null;
            }

            var AppointmentOnSlot = bookingRepository.GetAll().Where(x => x.AppointmentDate == bookInfo.AppointmentDate && x.ClinicSlotId == slotId && x.DentistId == bookInfo.DentistId);
            var slotInfo = scheduleRepository.GetClinicSlot(slotId);

            if (slotInfo == null)
            {
                message = "Can not find information about the selected slot";
                return null;
            }

            if (bookInfo.Type == "checkup" && AppointmentOnSlot.Where(x => x.Type == "checkup").Count() < slotInfo.MaxCheckup
                || bookInfo.Type == "treatment" && AppointmentOnSlot.Where(x => x.Type == "treatment").Count() < slotInfo.MaxCheckup)
            {
                bookInfo.ClinicSlotId = slotId;
                bookingRepository.UpdateBookingInfo(bookInfo);
                message = $"Changed appointment slot.";
                return bookInfo;
            }


            bookingRepository.UpdateBookingInfo(bookInfo);
            message = $"Successfully canceled appointment for booking {bookId}";
            return bookInfo;
            
        }

        public AppointmentInfoModel? ChangeAppointmentTime(out string message, Guid bookId, DateOnly? newDate = null, Guid? slotId = null)
        {

            AppointmentInfoModel? bookInfo = bookingRepository.GetBooking((Guid)bookId);

            if (bookInfo == null)
            {
                message = $"No booking information for Id {bookId}";
                return null;
            }

            if (bookInfo.Status == "finished" && bookInfo.Status == "canceled")
            {
                message = $"Can not change a finished or canceled appointment!";
                return null;
            }

            var AppointmentOnSlot = bookingRepository.GetAll().Where(x => x.AppointmentDate == (newDate??bookInfo.AppointmentDate) && x.ClinicSlotId == (slotId??bookInfo.ClinicSlotId) && x.DentistId == bookInfo.DentistId);
            var slotInfo = scheduleRepository.GetClinicSlot(slotId ?? bookInfo.ClinicSlotId);

            if (slotInfo == null)
            {
                message = "Can not find information about the selected slot";
                return null;
            }

            if (newDate != null)
            {
                if (bookInfo.Type == "checkup" && AppointmentOnSlot.Where(x => x.Type == "checkup").Count() < slotInfo.MaxCheckup)
                {
                    bookInfo.AppointmentDate = (DateOnly) newDate;
                }

                else if (bookInfo.Type == "treatment" && AppointmentOnSlot.Where(x => x.Type == "treatment").Count() < slotInfo.MaxCheckup)
                {
                    bookInfo.AppointmentDate = (DateOnly) newDate;
                }
            }

            if (slotId != null)
            {
                if (bookInfo.Type == "checkup" && AppointmentOnSlot.Where(x => x.Type == "checkup").Count() < slotInfo.MaxCheckup)
                {
                    bookInfo.ClinicSlotId = (Guid)slotInfo.ClinicSlotId!;
                }

                else if (bookInfo.Type == "treatment" && AppointmentOnSlot.Where(x => x.Type == "treatment").Count() < slotInfo.MaxCheckup)
                {
                    bookInfo.ClinicSlotId = (Guid)slotInfo.ClinicSlotId!;
                }
            }

            bookInfo = bookingRepository.UpdateBookingInfo(bookInfo);
            message = "Updated appointment time";
            return bookInfo;
        }

        public AppointmentInfoModel? ChangeDentist(Guid bookId, int dentistId, out string message)
        {
            AppointmentInfoModel? bookInfo;

            if ((bookInfo = bookingRepository.GetBooking(bookId)) == null)
            {
                message = $"Can not find information for booking {bookId}";
                return null;
            }

            UserInfoModel? dentist = userRepository.GetUserWithDentistID(dentistId);

            if (dentist == null) 
            {
                message = $"Can not find information for dentist {dentistId}";
                return null;
            }

            bookInfo.DentistId = dentistId;
            bookingRepository.UpdateBookingInfo(bookInfo);
            message = $"Changed dentist for this booking.";
            return bookInfo;
        }

        public AppointmentInfoModel? ChangeService(Guid bookId, Guid clinicServiceId, out string message)
        {
            AppointmentInfoModel? bookInfo = bookingRepository.GetBooking(bookId);

            BookedServiceInfoModel? bookedService = bookingRepository.GetBookingService(bookId);

            if (bookInfo == null)
            {
                message = $"Can not find information for booking {bookId}";
                return null;
            }

            ClinicServiceInfoModel? serviceInfo = clinicServiceRepository.GetClinicService(clinicServiceId);

            if (serviceInfo != null)
            {
                var service = new BookedServiceInfoModel
                {
                    AppointmentId = bookId,
                    Price = serviceInfo.Price,
                    ClinicServiceId = clinicServiceId,
                };

                bookInfo.AppointmentFee = serviceInfo.Price;

                if (bookedService == null)
                {
                    bookingRepository.AddBookingService(service);
                }
                else
                {
                    bookingRepository.UpdateBookingService(service);
                }
            }
            else
            {
                message = "Can not find service!";
                return null;
            }

            bookingRepository.UpdateBookingInfo(bookInfo);

            message = "Updated successfully";
            return bookInfo;
        }

        public AppointmentInfoModel? RemoveService(Guid bookId, out string message)
        {
            AppointmentInfoModel? bookInfo = bookingRepository.GetBooking(bookId);

            if (bookInfo == null)
            {
                message = $"Can not find information for booking {bookId}";
                return null;
            }

            BookedServiceInfoModel? bookedService = bookingRepository.GetBookingService(bookId);

            if (bookedService != null)
            {
                message = $"This booking does not have any additional service.";
                return bookInfo;
            }
            else
            {
                bookingRepository.DeleteBookingService(bookId);
                bookInfo.AppointmentFee = 0;
                bookingRepository.UpdateBookingInfo(bookInfo);
                message = "Updated successfully";
            }

            bookingRepository.UpdateBookingInfo(bookInfo);
            return bookInfo;
        }

        // Create
        public AppointmentInfoModel? CreateNewBooking(AppointmentRegistrationModel bookInfo, out string message)
        {

            var book = new AppointmentInfoModel()
            {
                Type = bookInfo.AppointmentType,
                Status = "booked",
                CyleCount = 0,
                OriginalAppoinment = bookInfo.OrginialAppointment,
                AppointmentDate = bookInfo.AppointmentDate,
                ClinicSlotId = bookInfo.TimeSlotId,
                CustomerId = bookInfo.CustomerId,
                DentistId = bookInfo.DentistId,
                ClinicId = bookInfo.ClinicId,
                CreationTime = DateTime.Now,
            };

            if (bookInfo.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
            {
                message = "Can not book an appointment in the past!";
                return null;
            }

            ClinicSlotInfoModel? slotInfo = scheduleRepository.GetClinicSlot(bookInfo.TimeSlotId);

            if (slotInfo == null)
            {
                message = "Can not find the slot for this appointment";
                return null;
            }

            ClinicServiceInfoModel? service = clinicServiceRepository.GetClinicService((Guid)bookInfo.ServiceId!);

            if (service == null)
            {
                message = "Service does not exist";
                return null;
            }

            if (service.ClinicId != bookInfo.ClinicId)
            {
                message = "This service does not belong to the chosen clinic";
                return null;
            }

            book = bookingRepository.CreateNewBooking(book);

            if (book != null)
            {
                message = $"Created new bookInfo for customer {bookInfo.CustomerId}";
                return book;
            }

            message = "Failed to create new bookInfo";
            return null;
        }

        public AppointmentInfoModel CreateNewPeriodicBooking(AppointmentRegistrationModel bookInfo, AppointmentSetting setting, out string message)
        {
            throw new NotImplementedException();
        }


        // Delete

        public bool DeleteBookingInformation(Guid bookId, out string messgae)
        {
            messgae = $"Deleteing information for {bookId}";
            return bookingRepository.DeleteBookingInfo(bookId);
        }

        // Read

        public IEnumerable<AppointmentInfoModel> GetAllBooking()
        {
            return bookingRepository.GetAll();
        }

        public IEnumerable<AppointmentInfoModel> GetAllBookingOnDay(DateOnly date)
        {
            return bookingRepository.GetAll().Where(x => x.AppointmentDate == date);
        }

        public IEnumerable<AppointmentInfoModel> GetAllClinicBooking(int clinicId, bool includeCancelled = false)
        {
            var clinicBooking = bookingRepository.GetAll().Where(x => x.ClinicId == clinicId);
            
            return includeCancelled ? clinicBooking : FilterBookList(clinicBooking);
        }

        public IEnumerable<AppointmentInfoModel> GetAllCustomerBooking(int customerId, bool includeCancelled = false)
        {
            var result = bookingRepository.GetAll().Where(x => x.CustomerId == customerId);

            return includeCancelled ? FilterBookList(result, includeCancelled: true) : result;
        }

        public IEnumerable<AppointmentInfoModel> GetAllDentistBooking(int dentistId, bool onlyFuture = false)
        {
            var result = bookingRepository.GetAll().Where(x => x.CustomerId == dentistId);

            return onlyFuture ? FilterBookList(result, includeCancelled: true) : result;
        }

        public AppointmentInfoModel? GetBooking(Guid id)
        {
            return bookingRepository.GetBooking(id);
        }

        public AppointmentInfoModel? updateBookingDate(Guid appointmentId, DateOnly newDate, out string message)
        {
            if (newDate < DateOnly.FromDateTime(DateTime.Now))
            {
                message = "Can not set the new appointnment date in the pass";
                return null;
            }

            var appointment = bookingRepository.GetBooking(appointmentId);

            if (appointment == null)
            {
                message = $"No appointment found with Id {appointmentId}";
                return null;
            }

            appointment.AppointmentDate = newDate;

            appointment = bookingRepository.UpdateBookingInfo(appointment);

            message = "Successfully updated booking information";
            return appointment;
        }

        public AppointmentInfoModel? updateBookingDentist(Guid appointmentId, int dentistId, out string message)
        {
            var appointment = bookingRepository.GetBooking(appointmentId);

            if (appointment == null)
            {
                message = $"No appointment found with Id {appointmentId}";
                return null;
            }

            var dentist = userRepository.GetUserWithDentistID(dentistId);

            if (dentist == null)
            {
                message = $"No dentist found with Id {dentistId}";
                return null;
            }

            if (dentist.ClinicId != appointment.ClinicId)
            {
                message = $"The dentist does not work in the clinic that has this appointment";
                return null;
            }
            
            var appointmentSlot = scheduleRepository.GetClinicSlot(appointment.ClinicSlotId);

            if (appointmentSlot == null)
            {
                message = $"Can not find clinic slot information";
                return null;
            }
            
            var count = GetAllDentistBooking(dentistId, true).Where(x => x.AppointmentDate == appointment.AppointmentDate && x.ClinicSlotId == appointment.ClinicSlotId && x.DentistId == dentistId);

            if ( (appointment.Type == "checkup" && count.Where(x => x.Type == "checkup").Count() < appointmentSlot.MaxCheckup) ||
                (appointment.Type == "treatment" && count.Where(x => x.Type == "treatment").Count() < appointmentSlot.MaxTreatment))
            {
                appointment.DentistId = dentistId;
                appointment = bookingRepository.UpdateBookingInfo(appointment);

                message = "Successfully updated booking information";
                return appointment;
            }

            message = "Can not change appointment assgined clinic staff.";
            return null;
        }

        public AppointmentInfoModel? updateBookingSlot(Guid appointmentId, Guid newSlot, out string message)
        {
            var appointment = bookingRepository.GetBooking(appointmentId);

            if (appointment == null)
            {
                message = $"No appointment found with Id {appointmentId}";
                return null;
            }

            var appointmentSlot = scheduleRepository.GetClinicSlot(newSlot);

            if (appointmentSlot == null)
            {
                message = $"Can not find clinic slot information";
                return null;
            }
            var count = GetAllDentistBooking(appointment.DentistId, true).Where(x => x.AppointmentDate == appointment.AppointmentDate && x.ClinicSlotId == appointment.ClinicSlotId);

            if ((appointment.Type == "checkup" && count.Where(x => x.Type == "checkup").Count() < appointmentSlot.MaxCheckup) ||
                (appointment.Type == "treatment" && count.Where(x => x.Type == "treatment").Count() < appointmentSlot.MaxTreatment))
            {
                appointment.ClinicSlotId = newSlot;
                appointment = bookingRepository.UpdateBookingInfo(appointment);

                message = "Successfully updated booking information";
                return appointment;
            }

            appointment = bookingRepository.UpdateBookingInfo(appointment);

            message = "Successfully updated booking information";
            return appointment;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    bookingRepository.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<AppointmentInfoModel> FilterBookList(IEnumerable<AppointmentInfoModel> list, DateOnly? start = null, DateOnly? end = null, bool includeCancelled = false, int? page_size = null, int? page = null)
        {
            if (!includeCancelled)
            {
                list = list.Where(x => x.Status == "canceled");
            }

            if (start != null)
            {
                list = list.Where(x => x.AppointmentDate >= start);
            }

            if (end != null)
            {
                list = list.Where(x => x.AppointmentDate <= end);
            }

            if (page_size != null && page != null)
            {
                return list.Skip((int)page * (int)page_size).Take((int)page_size);
            }

            return list;
        }

        public BookedServiceInfoModel? GetBookedService(Guid bookId)
        {
            return bookingRepository.GetBookingService(bookId);
        }
    }
}
