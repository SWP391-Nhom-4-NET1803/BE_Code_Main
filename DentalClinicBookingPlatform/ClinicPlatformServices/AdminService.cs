using ClinicPlatformObjects.ReportModels;
using ClinicPlatformRepositories;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository userRepository;
        private readonly IClinicRepository clinicRepository;
        private readonly IBookingRepository bookingRepository;
        private readonly IClinicServiceRepository clinicServiceRepository;
        private readonly IScheduleRepository scheduleRepository;
        private readonly IPaymentRepository paymentRepository;
        private bool disposedValue;

        public AdminService(IUserRepository userRepository, IClinicRepository clinicRepository, IBookingRepository bookingRepository, IClinicServiceRepository clinicServiceRepository, IScheduleRepository scheduleRepository, IPaymentRepository paymentRepository)
        {
            this.userRepository = userRepository;
            this.clinicRepository = clinicRepository;
            this.bookingRepository = bookingRepository;
            this.clinicServiceRepository = clinicServiceRepository;
            this.scheduleRepository = scheduleRepository;
            this.paymentRepository = paymentRepository;
        }

        public IEnumerable<AdminSumarizedInfo> GetReportInDateRange(DateOnly start, DateOnly end)
        {
            List<AdminSumarizedInfo> reportList = new List<AdminSumarizedInfo>();

            for (var date = start; date <= end; date = date.AddDays(1))
            {
                reportList.Add(GetSummaryReportOnDate(date));
            }

            return reportList;
        }

        public AdminSumarizedInfo GetSummaryReportOnDate(DateOnly date)
        {
            AdminSumarizedInfo report = new AdminSumarizedInfo();

            // Report date
            report.reportFor = date;

            // Appointment related items

            var allTodayBooking = bookingRepository.GetAllBookingInDate(date);

            report.TodayTotal = allTodayBooking.Count();

            report.TodayBook = allTodayBooking.Where(x => x.Status == "booked").Count();

            report.TodayFinished = allTodayBooking.Where(x => x.Status == "finished").Count();

            report.TodayCanceled = allTodayBooking.Where(x => x.Status == "canceled").Count();

            report.TodayPending = allTodayBooking.Where(x => x.Status == "pending").Count();

            // User related items
            
            report.TodayUserCreation = userRepository.GetUserWithCreationDate(date).Count();
            report.TodayUserUnactivated = userRepository.GetUnactivatedUser().Count();
            report.TodayUserDeleted = userRepository.GetRemovedUsers().Count();

            report.TodayMoneyEarned = 0; // PaymentService not done.


            // Others

            int maxAppointmentClinicId = -1;
            int maxAppointmentByClinic = 0;
            int currentMaxAppointmentByClinic = 0;

            var appointmentGroupByClinic = allTodayBooking.GroupBy(x => x.ClinicId).OrderBy(x => x.Key);

            foreach (var item in appointmentGroupByClinic)
            {
                currentMaxAppointmentByClinic = item.ToList().Count();

                if (currentMaxAppointmentByClinic > maxAppointmentByClinic)
                {
                    maxAppointmentClinicId = item.Key;
                    maxAppointmentByClinic = currentMaxAppointmentByClinic;
                }
            }

            report.MostBookedClinic = maxAppointmentClinicId != -1 ? clinicRepository.GetClinic(maxAppointmentClinicId) : null;

            Guid? maxAppointmentByServiceId = null;
            int maxAppointmentByService = 0;
            int currentMaxAppointmentByService = 0;

            var appointmentServices = allTodayBooking.Select(x => bookingRepository.GetBookingService(x.Id)).ToList();

            // Because database inconsistency, this line will temporarily ignore nulls.
            appointmentServices.Remove(null);

            var ServiceListGroupById = appointmentServices.GroupBy(x => x!.ClinicServiceId);

            foreach (var item in ServiceListGroupById)
            {
                currentMaxAppointmentByService = item.ToList().Count();

                if (currentMaxAppointmentByService > maxAppointmentByService)
                {
                    maxAppointmentByServiceId = item.Key;
                    maxAppointmentByService = currentMaxAppointmentByService;
                }
            }

            report.MostPopularService = maxAppointmentByServiceId != null ? clinicServiceRepository.GetClinicService((Guid)maxAppointmentByServiceId!) : null;

            report.MostBusyTime = null; // Unimplemented

            return report;
        }

        public AdminSumarizedInfo GetTodaySummaryReport()
        {
            return GetSummaryReportOnDate(DateOnly.FromDateTime(DateTime.UtcNow));
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
    }
}
