using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformObjects.PayementModels;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository paymentRepository;
        private readonly IBookingRepository bookingRepository;
        private readonly IUserRepository userRepository;
        private bool disposedValue;

        public PaymentService(IPaymentRepository paymentRepository, IBookingRepository bookingRepository, IUserRepository userRepository)
        {
            this.paymentRepository = paymentRepository;
            this.bookingRepository = bookingRepository;
            this.userRepository = userRepository;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    paymentRepository.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public PaymentInfoModel? CreateNewPayment(decimal amount,string transactId, string information, Guid appointmentId, string provider, DateTime expiration, out string message)
        {
            AppointmentInfoModel? bookingInfo = bookingRepository.GetBooking(appointmentId);

            if (bookingInfo == null)
            {
                message = "Can not find appointment information";
                return null;
            }

            PaymentInfoModel? pastPayment = GetLatestPaymentForAppointment(appointmentId);

            if (pastPayment != null) 
            {
                if (pastPayment.Status == "completed")
                {
                    message = "All payment for this appointment is already completed";
                    return null;
                }
                else if (pastPayment.Status == "pending" && pastPayment.Expiration >= DateTime.UtcNow)
                {
                    message = $"Using existing pending payment for {appointmentId}";
                    return pastPayment;
                }
            }

            PaymentInfoModel? newPayment = new PaymentInfoModel
            {
                Amount = amount,
                TransactId = transactId,
                Info = information,
                CreatedTime = DateTime.UtcNow,
                Expiration = expiration,
                Provider = provider,
                AppointmentId = appointmentId,
                Status = "pending",
            };

            newPayment = paymentRepository.CreatePayment(newPayment);

            if (newPayment != null)
            {
                message = $"Successfully created payment for appointment {appointmentId}";
            }
            else
            {
                message = "Payment creation failed";
            }

            return newPayment;
        }

        public PaymentInfoModel? SetPaymentStatusToCompleted(string paymentId, out string message)
        {
            var target = paymentRepository.GetPaymentWithPaymentId(paymentId);

            if (target == null)
            {
                message = $"Can not find target payment with id {paymentId}";
                return null;
            }

            target.Status = "completed";
            paymentRepository.UpdatePayment(target);

            message = $"Successfully updated payment {paymentId} status to completed";
            return target;
        }

        public PaymentInfoModel? SetPaymentStatusToCanceled(string paymentId, out string message)
        {
            var target = paymentRepository.GetPaymentWithPaymentId(paymentId);

            if (target == null)
            {
                message = $"Can not find target payment with id {paymentId}";
                return null;
            }

            target.Status = "completed";
            paymentRepository.UpdatePayment(target);

            message = $"Successfully updated payment {paymentId} status to completed";
            return target;
        }

        public IEnumerable<PaymentInfoModel> GetAllPaymentForDay(DateOnly date)
        {
            return paymentRepository.GetAllPaymentOnDate(date);
        }

        public PaymentInfoModel? GetPayment(string paymentId)
        {
            return paymentRepository.GetPaymentWithPaymentId(paymentId);
        }

        public IEnumerable<PaymentInfoModel> GetPaymentsForAppointment(Guid appointmentId)
        {
            return paymentRepository.GetPaymentForAppointment(appointmentId);
        }

        public PaymentInfoModel? GetLatestPaymentForAppointment(Guid appointmentId)
        {
            return GetPaymentsForAppointment(appointmentId).OrderByDescending(x => x.CreatedTime).FirstOrDefault();
        }

        public IEnumerable<PaymentInfoModel> GetAllClinicAppointmentPayment(int clinicId)
        {
            var appointments = bookingRepository.GetAllClinicBooking(clinicId);

            List<PaymentInfoModel> appointmentPayments = new List<PaymentInfoModel>();

            foreach(var appointment in appointments)
            {
                var payment = GetLatestPaymentForAppointment(appointment.Id);

                if (payment != null)
                {
                    appointmentPayments.Add(payment);
                }
            }

            return appointmentPayments;
        }

        public IEnumerable<PaymentInfoModel> GetPaymentOfCustomer(int customerId)
        {
            List<PaymentInfoModel> paymentList = new List<PaymentInfoModel>();
            
            var bookingInfo = bookingRepository.GetUserBooking(customerId);

            foreach (var appointment in bookingInfo)
            {
                var payment = GetLatestPaymentForAppointment(appointment.Id);

                if (payment != null)
                {
                    paymentList.Add(payment);
                }
            }

            return paymentList;
        }

        public PaymentInfoModel? GetPaymentWithId(int paymentId)
        {
            return paymentRepository.GetPayment(paymentId);
        }
    }
}
