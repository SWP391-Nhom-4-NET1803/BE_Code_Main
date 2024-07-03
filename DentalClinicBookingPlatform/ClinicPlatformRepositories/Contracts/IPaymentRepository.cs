using ClinicPlatformObjects.PayementModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    public interface IPaymentRepository: IDisposable
    {
        PaymentInfoModel? GetPayment(int Id);

        PaymentInfoModel? GetPaymentWithPaymentId(string paymentId);

        IEnumerable<PaymentInfoModel> GetAllPayment();

        IEnumerable<PaymentInfoModel> GetAllPaymentWithStatus(string status);

        IEnumerable<PaymentInfoModel> GetAllExpiredPayment();

        IEnumerable<PaymentInfoModel> GetAllPaymentOnDate(DateOnly date);

        PaymentInfoModel? GetPaymentForAppointment(Guid appointmentId);

        PaymentInfoModel? CreatePayment(PaymentInfoModel paymentInfo);

        PaymentInfoModel? UpdatePayment(PaymentInfoModel paymentInfo);
    }
}
