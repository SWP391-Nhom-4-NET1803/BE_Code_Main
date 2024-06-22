using ClinicPlatformBusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDAOs
{
    public class CustomerDAO: IDisposable
    {
        private readonly DentalClinicPlatformContext _context;
        private bool disposedValue;

        public CustomerDAO()
        {
            _context = new DentalClinicPlatformContext();
        }

        public CustomerDAO(DentalClinicPlatformContext context)
        {
            _context = context;
        }

        public Customer AddCustomer(Customer customer)
        {
            _context.Customers.Add(customer);

            return customer;
        }

        public Customer? GetCustomer(int customerId)
        {
            return _context.Customers
                .Where(x => x.Id == customerId)
                .FirstOrDefault();
        }

        public IEnumerable<Customer> GetAll()
        {
            return _context.Customers.ToList();
        }

        public Customer? UpdateCustomer(Customer customer)
        {
            if (GetCustomer(customer.Id) != null)
            {
                _context.Update(customer);

                return customer;
            }

            return null;
        }

        public bool DeleteCustomer(int customerId)
        {
            Customer? customer = GetCustomer(customerId);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                return true;
            }

            return false;
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
    }
}
