using ClinicPlatformBusinessObject;
using ClinicPlatformDAOs;
using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformRepositories.Contracts;

namespace ClinicPlatformRepositories
{
    public class BookingRepository : IBookingRepository
    {
        private BookingDAO bookingDAO;
        private bool disposedValue;

        public BookingRepository()
        {
            bookingDAO = new BookingDAO();
        }

        public bool CreateNewBooking(BookingModel booking)
        {
            return bookingDAO.AddBooking(MapBookingModelToBooking(booking));
        }

        public BookingModel? GetBooking(Guid id)
        {
            var result = bookingDAO.GetBooking(id);
            return result == null ? null : MapBookingToBookingModel(result);
        }

        public IEnumerable<BookingModel> GetAll()
        {
            return from item in bookingDAO.GetAll() select MapBookingToBookingModel(item);
        }

        public bool UpdateBookingInfo(BookingModel booking)
        {
            return bookingDAO.UpdateBooking(MapBookingModelToBooking(booking));

        }

        public bool DeleteBookingInfo(Guid bookId)
        {
            return bookingDAO.DeleteBooking(bookId);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    bookingDAO.Dispose();
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

        private Booking MapBookingModelToBooking(BookingModel book)
        {
            return new Booking()
            {
                BookId = book.Id,
                AppointmentDate = book.AppointmentDate ?? DateOnly.FromDateTime(DateTime.Now),
                BookingServiceId = book.SelectedService ?? null,
                BookingType = book.Type ?? "khám tổng quát",
                CreationDate = book.CreationTime ?? DateTime.Now,
                ClinicId = book.ClinicId ?? 0,
                CustomerId = book.CustomerId ?? 0,
                DentistId = book.DentistId ?? 0,
                ScheduleSlotId = book.TimeSlotId ?? Guid.NewGuid(),
                Status = book.Status
            };
        }

        private BookingModel MapBookingToBookingModel(Booking book)
        {
            return new BookingModel()
            {
                Id = book.BookId,
                AppointmentDate = book.AppointmentDate,
                SelectedService = book.BookingServiceId,
                Type = book.BookingType,
                CreationTime = book.CreationDate,
                ClinicId = book.ClinicId,
                CustomerId = book.CustomerId,
                DentistId = book.DentistId,
                TimeSlotId = book.ScheduleSlotId,
                Status = book.Status
            };
        }
    }
}
