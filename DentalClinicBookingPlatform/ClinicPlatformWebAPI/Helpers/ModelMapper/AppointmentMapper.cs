using ClinicPlatformDTOs.BookingModels;

namespace ClinicPlatformWebAPI.Helpers.ModelMapper
{
    public class AppointmentMapper
    {
        public static AppointmentInfoModel MapToAppointment(AppointmentRegistrationModel registration)
        {
            return new AppointmentInfoModel
            {
                AppointmentDate = registration.AppointmentDate,
                AppointmentFee = 0,
                ClinicId = registration.ClinicId,
                ClinicSlotId = registration.TimeSlotId,
                CustomerId = registration.CustomerId,
                DentistId = registration.DentistId,
                CreationTime = DateTime.Now,
                CyleCount = registration.MaxRecurring,
                OriginalAppoinment = registration.OrginialAppointment,
                Status = registration.Status,
                Type = registration.AppointmentType
            };
        }
    }
}
