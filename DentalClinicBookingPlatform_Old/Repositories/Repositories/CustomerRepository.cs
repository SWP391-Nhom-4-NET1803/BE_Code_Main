using PlatformRepository.Repositories;
using Repositories.Models;
using Repositories.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    internal class CustomerRepository : GenericRepository<Customer, int>, ICustomerRepository
    {
        public CustomerRepository(DentalClinicPlatformContext context) : base(context){}



    }
}
