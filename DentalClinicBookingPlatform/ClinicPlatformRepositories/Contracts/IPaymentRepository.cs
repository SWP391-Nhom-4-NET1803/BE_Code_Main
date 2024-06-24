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
        PaymentInfoModel? GetPayment(int paymentId);

        IEnumerable<PaymentInfoModel> GetAllPayment();

        PaymentInfoModel AddPayment(PaymentInfoModel paymentInfo);

        PaymentInfoModel UpdatePayment(PaymentInfoModel paymentInfo);
    }
}
