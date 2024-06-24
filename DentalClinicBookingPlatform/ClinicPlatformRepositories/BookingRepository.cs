using ClinicPlatformDatabaseObject;
using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformObjects.BookingModels;
using ClinicPlatformRepositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatformRepositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DentalClinicPlatformContext context;
        private bool disposedValue;

        public BookingRepository(DentalClinicPlatformContext context)
        {
            this.context = context;
        }

        public AppointmentInfoModel CreateNewBooking(AppointmentInfoModel booking)
        {
            var appointment = MapBookingModelToBooking(booking);
            context.Appointments.Add(appointment);
            context.SaveChanges();

            return MapBookingToBookingModel(appointment);
        }

        public AppointmentInfoModel? GetBooking(Guid id)
        {
            var result = context.Appointments.Find(id);
            return result == null ? null : MapBookingToBookingModel(result);
        }

        public IEnumerable<AppointmentInfoModel> GetAll()
        {
            return from item in context.Appointments select MapBookingToBookingModel(item);
        }

        public BookedServiceInfoModel? AddBookingService(BookedServiceInfoModel bookedService)
        {
            var service = ToBookedService(bookedService);
            context.BookedServices.Add(service);
            context.SaveChanges();

            return ToBookedServiceModel(service);
        }

        public BookedServiceInfoModel? GetBookingService(Guid appointmentId)
        {
            var bookedService = context.BookedServices.Find(appointmentId);

            if (bookedService != null)
            {
                return ToBookedServiceModel(bookedService);
            }

            return null;
        }

        public BookedServiceInfoModel? UpdateBookingService(BookedServiceInfoModel service)
        {
            var serviceInfo = context.BookedServices.Find(service.AppointmentId);

            if (serviceInfo != null)
            {

                serviceInfo.ServiceId = service.ClinicServiceId;
                serviceInfo.Price = service.Price;

                context.BookedServices.Update(serviceInfo);
                context.SaveChanges();

                return ToBookedServiceModel(serviceInfo);
            }

            return null;
        }

        public AppointmentInfoModel? UpdateBookingInfo(AppointmentInfoModel booking)
        {
            var info = context.Appointments.Find(booking.Id);

            if (info != null)
            {
                info.DentistId = booking.DentistId;
                info.AppointmentType = booking.Type;
                info.DentistNote = booking.Note;
                info.SlotId = booking.ClinicSlotId;
                info.Date = booking.AppointmentDate;
                info.Status = booking.Status;
                info.CycleCount = booking.CyleCount;
                info.PriceFinal = booking.AppointmentFee;

                context.Appointments.Update(info);
                context.SaveChanges();

                return MapBookingToBookingModel(info);
            }

            return null;
        }

        public bool DeleteBookingInfo(Guid bookId)
        {
            var appointment = context.Appointments.Find(bookId);

            if (appointment != null)
            {
                context.Appointments.Remove(appointment);
                context.SaveChanges();
                return true;
            }

            return false;
        }

        public void DeleteBookingService(Guid appointmentId)
        {
            var service = context.BookedServices.Find(appointmentId);

            if (service != null)
            context.BookedServices.Remove(service);
            context.SaveChanges();
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

        private Appointment MapBookingModelToBooking(AppointmentInfoModel book)
        {
            return new Appointment()
            {
                Id = book.Id,
                Date = book.AppointmentDate,
                AppointmentType = book.Type ?? "checkup",
                ClinicId = book.ClinicId,
                CustomerId = book.CustomerId,
                DentistId = book.DentistId,
                SlotId = book.ClinicSlotId,
                Status = book.Status,
                CycleCount = book.CyleCount,
                DentistNote = book.Note,
                OriginalAppointment = book.OriginalAppoinment,
                PriceFinal = book.AppointmentFee,
                
            };
        }

        private static AppointmentInfoModel MapBookingToBookingModel(Appointment appointment)
        {
            return new AppointmentInfoModel
            {
                Id = appointment.Id,
                AppointmentDate = appointment.Date,
                Type = appointment.AppointmentType,
                CreationTime = appointment.CreationTime,
                ClinicId = appointment.ClinicId,
                CustomerId = appointment.CustomerId,
                DentistId = appointment.DentistId,
                ClinicSlotId = appointment.SlotId,
                Status = appointment.Status,
                Note = appointment.DentistNote,
                CyleCount = appointment.CycleCount,
                OriginalAppoinment = appointment.OriginalAppointment!,
                AppointmentFee = appointment.PriceFinal,
                
            };
        }

        private static BookedServiceInfoModel ToBookedServiceModel(BookedService bookedService)
        {
            return new BookedServiceInfoModel()
            {
                AppointmentId = bookedService.AppointmentId,
                ClinicServiceId = bookedService.ServiceId,
                Price = bookedService.Price,
            };
        }

        private static BookedService ToBookedService(BookedServiceInfoModel bookedServiceInfo)
        {
            return new BookedService()
            {
                AppointmentId = bookedServiceInfo.AppointmentId,
                ServiceId = bookedServiceInfo.ClinicServiceId,
                Price = bookedServiceInfo.Price,
            };
        }
    }
}
