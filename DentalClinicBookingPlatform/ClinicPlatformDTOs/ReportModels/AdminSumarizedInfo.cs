using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformObjects.BookingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformObjects.ReportModels
{
    public class AdminSumarizedInfo
    {
        // Summary information
        public DateOnly reportFor {  get; set; }

        // Booking Informations
        public int TodayBook {  get; set; }
        public int TodayCanceled{ get; set; }
        public int TodayPending { get; set; }
        public int TodayFinished { get; set; }
        public int TodayTotal { get; set; }

        // Money Informations
        public decimal TodayMoneyEarned {  get; set; }

        // User Informations
        public int TodayUserCreation {  get; set; }
        public int TodayUserUnactivated { get; set; }
        public int TodayUserDeleted { get; set; }

        // Other Informations
        public TimeOnly? MostBusyTime { get; set; }
        public AppointmentInfoModel? MostExpensiveAppointment { get; set; } = null!;
        public BookedServiceInfoModel? ExpensiveAppointmentService { get; set; } = null!;
        public ClinicInfoModel? MostBookedClinic { get; set; } = null!;
        public ClinicServiceInfoModel? MostPopularService { get; set; } = null!;

    }
}
