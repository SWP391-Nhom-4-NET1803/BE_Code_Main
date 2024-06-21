using ClinicPlatformBusinessObject;
using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformDTOs.UserModels;
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

        private bool disposedValue;

        public BookingService(IBookingRepository bookingRepository)
        {
            this.bookingRepository = bookingRepository;
        }

        // Updates

        public bool CancelBooking(Guid id, out string message)
        {
            BookingModel? bookInfo = bookingRepository.GetBooking(id);
            
            if (bookInfo == null)
            {
                message = $"No booking information for Id {id}";
                return false;
            }

            if (bookInfo.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
            {
                message = $"This booking is already finished!";
                return false;
            }

            if (bookInfo.Status == false)
            {
                message = $"This booking is already canceled!";
                return false;
            }

            bookInfo.Status = false;
            bookingRepository.UpdateBookingInfo(bookInfo);

            message = $"Successfully canceled appointment for booking {id}";
            return true;
        }

        public bool ChangeDate(Guid bookId, DateOnly newDate, out string message)
        {
            if (newDate <= DateOnly.FromDateTime(DateTime.Now))
            {
                message = "Can not change to a date in the past.";
                return false;
            }

            BookingModel? bookInfo;

            if ((bookInfo = bookingRepository.GetBooking(bookId)) == null)
            {
                message = $"Can not find information for booking {bookId}";
                return false;
            }

            bookInfo.AppointmentDate = newDate;
            message = $"Changed booking date to {newDate.ToString("d/M/yyyy")}.";
            return true;
        }

        public bool ChangeDentist(Guid bookId, int clinicDentist, out string message)
        {
            BookingModel? bookInfo;

            if ((bookInfo = bookingRepository.GetBooking(bookId)) == null)
            {
                message = $"Can not find information for booking {bookId}";
                return false;
            }

            message = $"Changed dentist for this booking.";
            return true;
        }

        public bool ChangeService(Guid bookId, Guid clinicServiceId, out string message)
        {
            BookingModel? bookInfo = bookingRepository.GetBooking(bookId);

            if (bookInfo == null)
            {
                message = $"Can not find information for booking {bookId}";
                return false;
            }

            bookInfo.SelectedService = clinicServiceId;

            bookingRepository.UpdateBookingInfo(bookInfo);

            message = "Updated successfully";
            return true;
        }

        public bool RemoveService(Guid bookId, out string message)
        {
            BookingModel? bookInfo = bookingRepository.GetBooking(bookId);

            if (bookInfo == null)
            {
                message = $"Can not find information for booking {bookId}";
                return false;
            }

            if (bookInfo.SelectedService != null)
            {
                message = $"This booking does not have any additional service.";
            }
            else
            {
                message = "Updated successfully";
            }
            bookInfo.SelectedService = null;
            bookingRepository.UpdateBookingInfo(bookInfo);

            return true;
        }

        public bool ChangeSlot(Guid bookId, Guid clinicSlotId, out string message)
        {
            throw new NotImplementedException();
        } 

        // Create

        public bool CreateNewBooking(BookingRegistrationModel bookInfo, out string message)
        {
            if (bookInfo.AppointmentDate < DateOnly.FromDateTime(DateTime.Now))
            {
                message = "Can not book an appointment in the past!";
                return false;
            }

            BookingModel booking = new BookingModel
            {
                ClinicId = bookInfo.ClinicId,
                CustomerId = bookInfo.CustomerId,
                DentistId = bookInfo.DentistId,
                SelectedService = bookInfo.ServiceId,
                AppointmentDate = bookInfo.AppointmentDate,
                CreationTime = DateTime.Now,
                Status = false,
                TimeSlotId = bookInfo.TimeSlotId,
                Type = bookInfo.AppointmentType,
            };

            if (bookInfo.AppointmentType == BookingRegistrationModel.ServiceBooking)
            {
                // booking.maxRecurring = bookInfo.MaxRecurring; // This should be used to set the time and span.
                booking.Type = bookInfo.AppointmentType;
            }

            if (!bookingRepository.CreateNewBooking(booking))
            {
                message = "Failed to create new booking";
                return false;
            }

            message = $"Created new booking for customer {bookInfo.CustomerId}";
            return true;
        }

        public bool CreateNewPeriodicBooking(BookingRegistrationModel bookInfo, out string message)
        {
            throw new NotImplementedException();
        }

        public bool CreateOneTimeBooking(BookingRegistrationModel bookInfo, out string message)
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

        public IEnumerable<BookingModel> GetAllBooking()
        {
            return bookingRepository.GetAll();
        }

        public IEnumerable<BookingModel> GetAllBookingOnDay(DateOnly date)
        {
            return bookingRepository.GetAll().Where(x => x.AppointmentDate == date);
        }

        public IEnumerable<BookingModel> GetAllClinicBooking(int clinicId, bool includeCancelled = false)
        {
            var clinicBooking = bookingRepository.GetAll().Where(x => x.ClinicId == clinicId);
            
            return includeCancelled ? clinicBooking : FilterBookList(clinicBooking);
        }

        public IEnumerable<BookingModel> GetAllCustomerBooking(int customerId, bool includeCancelled = false)
        {
            var result = bookingRepository.GetAll().Where(x => x.CustomerId == customerId);

            return includeCancelled ? FilterBookList(result, includeCancelledOrFinished: true) : result;
        }

        public IEnumerable<BookingModel> GetAllDentistBooking(int dentistId, bool onlyFuture = false)
        {
            var result = bookingRepository.GetAll().Where(x => x.CustomerId == dentistId);

            return onlyFuture ? FilterBookList(result, includeCancelledOrFinished: true) : result;
        }

        public BookingModel? GetBooking(Guid id)
        {
            return bookingRepository.GetBooking(id);
        }

        // This is just horrendous.
        public bool UpdateBookingInformation(BookingModel bookModel, out string message)
        {
            BookingModel? bookInfo;
            if ((bookInfo = bookingRepository.GetBooking(bookModel.Id)) == null)
            {
                message = $"No booking information found for Id {bookModel.Id}";
                return false;
            }

            var excludedProps = "SelectedService,CreationTime";
            bool stop = false;

            message = "";


            foreach (var property in bookModel.GetType().GetProperties())
            {
                if (property.GetValue(bookModel) == null && !excludedProps.Split(",").Any(x => x == property.Name))
                {
                    message += $"Missing required property:  {property.Name}";
                    stop = true;
                }
                
            }

            if (stop)
            {
                return false;
            }

            // Changing appointment date
            if (bookModel.AppointmentDate <= DateOnly.FromDateTime(DateTime.Now))
            {
                message = $"Can not update because the appointment date {bookModel.AppointmentDate} is in the past.";
                return false;
            }

            bookInfo.CustomerId = bookModel.CustomerId;
            bookInfo.DentistId = bookModel.DentistId;
            bookInfo.AppointmentDate = bookModel.AppointmentDate;
            bookInfo.TimeSlotId = bookModel.TimeSlotId;
            bookInfo.Status = bookModel.Status;

            message = $"Updated booking {bookModel.Id} information.";
            return bookingRepository.UpdateBookingInfo(bookModel);
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

        public IEnumerable<BookingModel> FilterBookList(IEnumerable<BookingModel> list, DateOnly? start = null, DateOnly? end = null, bool includeCancelledOrFinished = false, int? page_size = null, int? page = null)
        {
            if (!includeCancelledOrFinished)
            {
                list = list.Where(x => !x.Status);
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
    }
}
