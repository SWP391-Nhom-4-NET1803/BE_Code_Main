using ClinicPlatformBusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformDAOs.Contracts
{
    internal interface IUserDAO: IDisposable
    {
        public User AddUser(User user);
        public User? GetUser(int id);
        public IEnumerable<User> GetAllUsers();
        public User UpdateUser(User user);
        public void DeleteUser(int id);
    }
}
