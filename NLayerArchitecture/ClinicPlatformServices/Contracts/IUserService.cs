using ClinicPlatformDTOs.RoleModels;
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
        UserInfoModel? GetUserInformation(int userId);

        CustomerInfoModel? GetCustomerInformation(int customerId);

        ClinicStaffInfoModel? GetClinicStaffInformation(int staffId);

        bool RegisterCustomerAccount(UserRegistrationModel information, out string message);

        bool RegisterClinicStaffAccount(UserRegistrationModel information, out string message);

        bool RegisterAccount(UserRegistrationModel information, RoleModel.Roles role, out string message, bool IsAdmin = false);

        bool UpdateUserInformation(UserInfoModel information, out string message);

        bool DeleteUser(int id, out string message);


        // Other business logics
        bool checkLogin(string username, string password, out UserInfoModel? user);
        
        bool loginAsClinicStaff(string username, string password, out ClinicStaffInfoModel? info, out string message);

        bool loginAsCustomer(string username, string password, out CustomerInfoModel? info, out string message);

        bool loginAsAdmin(string username, string password, out UserInfoModel? info, out string message);

        bool ExistUser(int id);

        bool ActivateUser(int id, out string message);

        bool InactivateUser(int id,out string message);

        // Utils
        bool checkAccountAvailability(string username, string email, out string message);
    }
}
