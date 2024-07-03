using ClinicPlatformObjects.PayementModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IPaymentService: IDisposable
    {
        PaymentInfoModel? CreateNewPayment(decimal amount, string transactId, string information, Guid appointmentInfo, string provider, DateTime expiration, out string message);

        PaymentInfoModel? SetPaymentStatusToCompleted(string paymentId, out string message);

        PaymentInfoModel? SetPaymentStatusToCanceled(string paymentId, out string message);

        PaymentInfoModel? GetPaymentWithId(int paymentId);

        IEnumerable<PaymentInfoModel> GetAllPaymentForDay(DateOnly date);

        PaymentInfoModel? GetPayment(string paymentId);

        PaymentInfoModel? GetPaymentForAppointment(Guid appointmentId);

        IEnumerable<PaymentInfoModel> GetPaymentOfCustomer(int customerId);
    }
}
