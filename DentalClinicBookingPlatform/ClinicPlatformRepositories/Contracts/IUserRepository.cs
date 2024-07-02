using ClinicPlatformDTOs.UserModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformRepositories.Contracts
{
    public interface IUserRepository: IDisposable
    {
        IEnumerable<UserInfoModel> GetAllUser(bool includeRemoved = true, bool includeInactive = true);

        public IEnumerable<UserInfoModel> GetUserWithRole(string role);

        UserInfoModel? GetUser(int userId);

        UserInfoModel? GetUserWithUsername(string username);

        UserInfoModel? GetUserWithEmail(string email);

        UserInfoModel? GetUserWithCustomerID(int customerId);

        UserInfoModel? GetUserWithDentistID(int dentistId);

        UserInfoModel? AddUser(UserInfoModel user);

        UserInfoModel? UpdateUser(UserInfoModel user);

        void DeleteUser(int userId);
    }
}
