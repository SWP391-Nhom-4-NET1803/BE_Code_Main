using Microsoft.EntityFrameworkCore;
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
    public class UserRepository: GenericRepository<User, int>, IUserRepository
    {
        public UserRepository(DentalClinicPlatformContext context) :base(context) { }

        public Customer? GetCustomerInfo(int id)
        {
            return context.Customers.Include(x => x.User).First(x => x.UserId == id);
        }

        public ClinicStaff? GetStaffInfo(int id)
        {
            return context.ClinicStaffs.Include(x => x.User).FirstOrDefault(x => x.UserId == id);
        }

        public User? Authenticate(string username, string password)
        {
            return dbSet.Where((user) => (user.Username == username && user.Password == password)).FirstOrDefault();
        }

        public bool CheckAvailability(string username, string email, out string message)
        {
            List<User> ExistanceList =dbSet.Where((user) => (user.Username == username || user.Email == email)).ToList(); ;

            foreach (User user in ExistanceList)
            {
                if (user.Username.Equals(username))
                {
                    message = "Username is used by another account.";
                    return false;
                }

                if (user.Email.Equals(email))
                {
                    message = "Email is used by another account.";
                    return false;
                }
            }

            message = "Account is available for creation";
            return true;
        }

        public User? GetUserWithEmail(string email)
        {
            return dbSet.FirstOrDefault(x => x.Email == email);
        }

        public IEnumerable<ClinicStaff> GetAllClinicStaff(int clinic_id)
        {
            return context.ClinicStaffs.Where(staff => staff.ClinicId == clinic_id).Include(x => x.User).ToList();
        }

        public IEnumerable<ClinicStaff> GetAllClinicStaff(string clinic_name)
        {
            return context.ClinicStaffs.Where(x => x.Clinic!.Name == clinic_name).Include(x => x.Clinic).ToList();
        }
        public bool ExistUser(int id, out User? info)
        {
            info = this.GetById(id);

            if (info == null)
            {
                return false;
            }

            return true;
        }
    }
}
