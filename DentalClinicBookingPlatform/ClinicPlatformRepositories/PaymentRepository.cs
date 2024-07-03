using ClinicPlatformDatabaseObject;
using ClinicPlatformObjects.PayementModels;
using ClinicPlatformRepositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly DentalClinicPlatformContext context;
        private bool disposedValue;

        public PaymentRepository(DentalClinicPlatformContext context)
        {
            this.context = context;
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public PaymentInfoModel? CreatePayment(PaymentInfoModel paymentInfo)
        {
            var payment = MapToPayment(paymentInfo);

            context.Payments.Add(payment);
            context.SaveChanges();

            return MapToPaymentInfo(payment);
        }

        public IEnumerable<PaymentInfoModel> GetAllPayment()
        {
            return context.Payments.Select(x => MapToPaymentInfo(x)).ToList();
        }

        public IEnumerable<PaymentInfoModel> GetAllPaymentWithStatus(string status)
        {
            return context.Payments
                .Where(x => x.Status == status)
                .Select(x => MapToPaymentInfo(x)).ToList();
        }

        public IEnumerable<PaymentInfoModel> GetAllExpiredPayment()
        {
            return context.Payments
                .Where(x => x.Status != "completed" && x.ExpirationTime <= DateTime.Now)
                .Select(x => MapToPaymentInfo(x)).ToList();
        }

        public IEnumerable<PaymentInfoModel> GetAllPaymentOnDate(DateOnly date)
        {
            return context.Payments
                .Where(x => DateOnly.FromDateTime(x.CreationTime) == date)
                .Select(x => MapToPaymentInfo(x)).ToList();
        }

        public PaymentInfoModel? GetPayment(int Id)
        {
            var result = context.Payments.Find(Id);

            return result != null ? MapToPaymentInfo(result) : null;
        }

        public PaymentInfoModel? UpdatePayment(PaymentInfoModel paymentInfo)
        {
            var target = context.Payments.Find(paymentInfo.Id);

            if (target == null)
            {
                return null;
            }

            target.Info = paymentInfo.Info;
            target.Amount = paymentInfo.Amount;
            target.Status = paymentInfo.Status;

            context.Payments.Update(target);
            context.SaveChanges();

            return MapToPaymentInfo(target);
        }

        private PaymentInfoModel MapToPaymentInfo(Payment payment)
        {
            return new PaymentInfoModel
            {
                Id = payment.Id,
                TransactId = payment.TransactionId,
                Amount = payment.Amount,
                Info = payment.Info,
                Provider = payment.Provider,
                CreatedTime = payment.CreationTime,
                AppointmentId = payment.AppointmentId,
                Expiration = payment.ExpirationTime,
                Status = payment.Status,
            };
        }

        private Payment MapToPayment(PaymentInfoModel payment)
        {
            return new Payment
            {
                Id = payment.Id,
                TransactionId = payment.TransactId,
                Amount = payment.Amount,
                AppointmentId = payment.AppointmentId,
                CreationTime = payment.CreatedTime,
                ExpirationTime = payment.Expiration,
                Info = payment.Info,
                Provider = payment.Provider,
                Status = payment.Status,
            };
        }

        public PaymentInfoModel? GetPaymentWithPaymentId(string paymentId)
        {
            var result = context.Payments.Where(x => x.TransactionId == paymentId).FirstOrDefault();

            return result == null ? null : MapToPaymentInfo(result);
        }

        public PaymentInfoModel? GetPaymentForAppointment(Guid appointmentId)
        {
            var target = context.Payments.Where(x => x.AppointmentId == appointmentId).FirstOrDefault();

            return target == null ? null : MapToPaymentInfo(target);
        }
    }
}
