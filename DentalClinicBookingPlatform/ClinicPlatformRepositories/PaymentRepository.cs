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
        private bool disposedValue;

        public PaymentInfoModel AddPayment(PaymentInfoModel paymentInfo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PaymentInfoModel> GetAllPayment()
        {
            throw new NotImplementedException();
        }

        public PaymentInfoModel? GetPayment(int paymentId)
        {
            throw new NotImplementedException();
        }

        public PaymentInfoModel UpdatePayment(PaymentInfoModel paymentInfo)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PaymentRepository()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
