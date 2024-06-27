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

        public AppointmentInfoModel? ChangeAppointmentDate(Guid bookId, DateOnly newDate, out string message)
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

            var AppointmentOnSlot = bookingRepository.GetAll().Where(x => x.AppointmentDate == newDate && x.ClinicSlotId == bookInfo.ClinicSlotId && x.DentistId == bookInfo.DentistId);
            var slotInfo = scheduleRepository.GetClinicSlot(bookInfo.ClinicSlotId);

            if (slotInfo == null)
            {
                message = "Slot info not found";
                return null;
            }

            if (bookInfo.Type == "checkup" && AppointmentOnSlot.Where(x => x.Type == "checkup").Count() < slotInfo.MaxCheckup
                || bookInfo.Type == "treatment" && AppointmentOnSlot.Where(x => x.Type == "treatment").Count() < slotInfo.MaxCheckup)
            {
                bookInfo.AppointmentDate = (DateOnly)newDate;
            }

            else 
            {
                bookInfo.AppointmentDate = (DateOnly)newDate;
            }

            bookInfo = bookingRepository.UpdateBookingInfo(bookInfo);

            if (bookInfo != null)
            {
                message = "Successfully updated appointment date";
            }
            else
            {
                message = "Error";
            }
            
            return bookInfo;
        }

        public AppointmentInfoModel? ChangeAppointmentSlot(Guid bookId, Guid slotId, out string message)
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

            var slotInfo = scheduleRepository.GetClinicSlot(bookInfo.ClinicSlotId);

            if (slotInfo == null)
            {
                message = "Slot info not found";
                return null;
            }

            var AppointmentOnSlot = bookingRepository.GetAll().Where(x => x.ClinicSlotId == slotId && x.ClinicSlotId == bookInfo.ClinicSlotId && x.DentistId == bookInfo.DentistId);
            

            if (bookInfo.Type == "checkup" && AppointmentOnSlot.Where(x => x.Type == "checkup").Count() < slotInfo.MaxCheckup 
                || bookInfo.Type == "treatment" && AppointmentOnSlot.Where(x => x.Type == "treatment").Count() < slotInfo.MaxCheckup)
            {
                bookInfo.ClinicSlotId = slotId;
            }
            else
            {
                message = "This slot is already fully booked";
                return null;
            }

            bookInfo = bookingRepository.UpdateBookingInfo(bookInfo);

            if (bookInfo != null)
            {
                message = "Successfully updated appointment slot";
            }
            else
            {
                message = "Error";
            }

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

            var appointmentSlot = scheduleRepository.GetClinicSlot(bookInfo.ClinicSlotId);

            if (appointmentSlot == null)
            {
                message = $"Can not find clinic slot information";
                return null;
            }

            var count = GetAllDentistBooking(dentistId, true).Where(x => x.AppointmentDate == bookInfo.AppointmentDate && x.ClinicSlotId == bookInfo.ClinicSlotId && x.DentistId == dentistId);

            if ((bookInfo.Type == "checkup" && count.Where(x => x.Type == "checkup").Count() < appointmentSlot.MaxCheckup) ||
                (bookInfo.Type == "treatment" && count.Where(x => x.Type == "treatment").Count() < appointmentSlot.MaxTreatment))
            {
                bookInfo.DentistId = dentistId;
                bookInfo = bookingRepository.UpdateBookingInfo(bookInfo);

                message = "Successfully updated booking information";
                return bookInfo;
            }

            message = "Can not change bookRegistrationInfo assgined clinic staff.";
            return null;
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
        public AppointmentInfoModel? CreateNewBooking(AppointmentRegistrationModel bookRegistrationInfo, out string message)
        {

            var book = new AppointmentInfoModel()
            {
                Type = bookRegistrationInfo.AppointmentType,
                Status = bookRegistrationInfo.Status,
                CyleCount = 0,
                OriginalAppoinment = bookRegistrationInfo.OrginialAppointment,
                AppointmentDate = bookRegistrationInfo.AppointmentDate,
                ClinicSlotId = bookRegistrationInfo.TimeSlotId,
                CustomerId = bookRegistrationInfo.CustomerId,
                DentistId = bookRegistrationInfo.DentistId,
                ClinicId = bookRegistrationInfo.ClinicId,
                CreationTime = DateTime.Now,
            };

            if (bookRegistrationInfo.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
            {
                message = "Can not book an appointment in the past!";
                return null;
            }

            UserInfoModel? customer = userRepository.GetUserWithCustomerID(bookRegistrationInfo.CustomerId);

            if (customer == null)
            {
                message = "Can not find the customer information";
                return null;
            }

            UserInfoModel? dentist = userRepository.GetUserWithDentistID(bookRegistrationInfo.DentistId);

            if (dentist == null)
            {
                message = "Can not find the dentist information";
                return null;
            }

            ClinicInfoModel? clinic = clinicRepository.GetClinic(bookRegistrationInfo.ClinicId);

            if (clinic == null)
            {
                message = "Can not find the clinic information";
                return null;
            }

            ClinicSlotInfoModel? slotInfo = scheduleRepository.GetClinicSlot(bookRegistrationInfo.TimeSlotId);

            if (slotInfo == null)
            {
                message = "Can not find the clinic slot";
                return null;
            }

            if (slotInfo.ClinicId != clinic.Id)
            {
                message = "This slot does not belong to the chosen clinic";
                return null;
            }

            int count = GetAllBookingOnDay(bookRegistrationInfo.AppointmentDate).Where(x => x.ClinicId == bookRegistrationInfo.ClinicId && x.DentistId  == dentist.DentistId && x.Type == bookRegistrationInfo.AppointmentType).Count();

            if (bookRegistrationInfo.AppointmentType == "treatement" && count >= slotInfo.MaxTreatment || bookRegistrationInfo.AppointmentType == "checkup" && count >= slotInfo.MaxCheckup )
            {
                message = "This slot is fully booked and unavailable for this date";
                return null;
            }

            ClinicServiceInfoModel? service = clinicServiceRepository.GetClinicService((Guid)bookRegistrationInfo.ServiceId!);

            if (service == null)
            {
                message = "Service does not exist";
                return null;
            }

            if (service.ClinicId != clinic.Id)
            {
                message = "This service does not belong to the chosen clinic";
                return null;
            }
            
            book.AppointmentFee = service.Price;

            book = bookingRepository.CreateNewBooking(book);

            if (book != null)
            {
                var bookedService = new BookedServiceInfoModel
                {
                    AppointmentId = book.Id,
                    ClinicServiceId = bookRegistrationInfo.ServiceId,
                    Name = service.Name,
                    Price = service.Price,
                };

                bookedService = bookingRepository.AddBookingService(bookedService);

                book.SelectedService = bookedService.ClinicServiceId;

                message = $"Created new bookRegistrationInfo for customer {bookRegistrationInfo.CustomerId}";
                return book;
            }

            message = "Failed to create new bookRegistrationInfo";
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

        public IEnumerable<AppointmentInfoModel> GetAllBookingOnDay(DateOnly date, bool includeCanceled = false)
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

        public bool DentistIsAvailableOn(DateOnly date, Guid slotId, int dentistId, out string message)
        {
            UserInfoModel? dentist = userRepository.GetUserWithDentistID(dentistId);

            if (dentist == null)
            {
                message = $"Can not find dentist with Id {dentistId}";
                return false;
            }

            ClinicSlotInfoModel? clinicSlot = scheduleRepository.GetClinicSlot(slotId);

            if (clinicSlot == null || clinicSlot.ClinicId != dentist.ClinicId)
            {
                message = $"Unavailable slot with Id {slotId}!";
                return false;
            }

            IEnumerable<AppointmentInfoModel> appointments = GetAllDentistBooking(dentistId).Where(x => x.AppointmentDate == date && x.Status != "canceled");

            if (clinicSlot.MaxCheckup > appointments.Where(x => x.Type == "checkup").Count() || clinicSlot.MaxTreatment > appointments.Where(x => x.Type == "treatment").Count()) 
            {
                message = "Available!";
                return true;
            }

            message = "Unavailable!";
            return false;
        }

        public bool DentistIsFreeForCheckupOn(DateOnly date, Guid slotId, int dentistId, out string message)
        {
            UserInfoModel? dentist = userRepository.GetUserWithDentistID(dentistId);

            if (dentist == null)
            {
                message = $"Unavailable dentist with Id {dentistId}";
                return false;
            }

            ClinicSlotInfoModel? clinicSlot = scheduleRepository.GetClinicSlot(slotId);

            if (clinicSlot == null || clinicSlot.ClinicId != dentist.ClinicId)
            {
                message = $"Unavailable slot with Id {slotId}!";
                return false;
            }

            IEnumerable<AppointmentInfoModel> appointments = GetAllDentistBooking(dentistId).Where(x => x.AppointmentDate == date && x.Status != "canceled");

            if (clinicSlot.MaxCheckup > appointments.Where(x => x.Type == "checkup").Count())
            {
                message = "Available!";
                return true;
            }

            message = "Unavailable!";
            return false;
        }

        public bool DentistIsFreeForTreatmentOn(DateOnly date, Guid slotId, int dentistId, out string message)
        {
            UserInfoModel? dentist = userRepository.GetUserWithDentistID(dentistId);

            if (dentist == null)
            {
                message = $"Unavailable dentist with Id {dentistId}";
                return false;
            }

            ClinicSlotInfoModel? clinicSlot = scheduleRepository.GetClinicSlot(slotId);

            if (clinicSlot == null || clinicSlot.ClinicId != dentist.ClinicId)
            {
                message = $"Unavailable slot with Id {slotId}!";
                return false;
            }

            IEnumerable<AppointmentInfoModel> appointments = GetAllDentistBooking(dentistId).Where(x => x.AppointmentDate == date && x.Status != "canceled");

            if (clinicSlot.MaxTreatment > appointments.Where(x => x.Type == "treatment").Count())
            {
                message = "Available!";
                return true;
            }

            message = "Unavailable!";
            return false;
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
