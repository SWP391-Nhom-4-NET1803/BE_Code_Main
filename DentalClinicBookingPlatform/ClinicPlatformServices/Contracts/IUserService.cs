using ClinicPlatformDTOs.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicPlatformServices.Contracts
{
    public interface IUserService: IDisposable
    {
        IEnumerable<UserInfoModel> GetUsers();
        UserInfoModel? GetUserWithUserId(int userId);

        UserInfoModel? GetUserWithCustomerId(int customerId);

        UserInfoModel? GetUserWithEmail(string email);

        UserInfoModel? GetDentistWithDentistId(int dentistId);

        IEnumerable<UserInfoModel> GetRemovedUser();

        IEnumerable<UserInfoModel> GetAllUserOfRole(string role);

        IEnumerable<UserInfoModel> GetUserRegisteredOn(DateOnly date);

        UserInfoModel? RegisterAccount(UserInfoModel information, string role, out string message, bool IsAdmin = false);

        bool DeleteUser(int id, out string message);

        bool CheckLogin(string username, string password, out UserInfoModel? user);

        UserInfoModel? UpdateUserInformation(UserInfoModel information, out string message);

        bool ExistUser(int id);

        bool UpdatePasswordForUserWithId(int userId, string newPassword, out string message);

        bool ActivateUser(int id, out string message);

        bool InactivateUser(int id,out string message);

        // Utils
        bool CheckAccountAvailability(string username, string email, out string message);
    }
}
