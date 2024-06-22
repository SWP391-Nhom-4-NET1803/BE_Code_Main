using ClinicPlatformBusinessObject;
using ClinicPlatformDAOs.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDAOs
{
    public class PaymentDAO: IFilterQuery<Payment>, IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public PaymentDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public PaymentDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public Payment AddPayment(Payment Payment)
        {
            _context.Add(Payment);
            this.SaveChanges();

            return Payment;
        }

        public Payment? GetPayment(int paymentId)
        {
            return _context.Payments.Where(x => x.Id == paymentId).FirstOrDefault();
        }

        public IEnumerable<Payment> GetAllPayment()
        {
            return _context.Payments.ToList();
        }

        public Payment? UpdatePayment(Payment payment)
        {
            Payment? ServiceInfo = GetPayment(payment.Id);

            if (ServiceInfo != null)
            {
                _context.Payments.Update(payment);
                SaveChanges();

                return payment;
            }

            return null;
        }

        public bool DeletePayment(int paymentId)
        {
            Payment? Payment = GetPayment(paymentId);

            if (Payment != null)
            {
                _context.Payments.Remove(Payment);
                this.SaveChanges();

                return true;
            }

            return false;
        }

        public void SaveChanges()
        {
            this._context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IEnumerable<Payment> Filter(Expression<Func<Payment, bool>> filter, Func<IQueryable<Payment>, IOrderedQueryable<Payment>>? orderBy, string includeProperties = "", int? pageSize = null, int? pageIndex = null)
        {
            IQueryable<Payment> query = _context.Payments;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Implementing pagination
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                // Ensure the pageIndex and pageSize are valid
                int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
                int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Assuming a default pageSize of 10 if an invalid value is passed

                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }

            return query.ToList();
        }
    }
}
