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

        public ClinicStaffInfoModel? GetClinicStaffInformationWithUserId(int userId);

        public CustomerInfoModel? GetCustomerInformationWithUserID(int userId);

        UserInfoModel? GetUserInformationWithEmail(string email);

        bool RegisterCustomerAccount(UserRegistrationModel information, out string message);

        bool RegisterClinicStaffAccount(UserRegistrationModel information, out string message);

        bool RegisterAccount(UserRegistrationModel information, RoleModel.Roles role, out string message, bool IsAdmin = false);

        bool UpdateUserInformation(UserInfoModel information, out string message);

        bool DeleteUser(int id, out string message);


        // Other business logics
        bool CheckLogin(string username, string password, out UserInfoModel? user);
        
        bool LoginAsClinicStaff(string username, string password, out ClinicStaffInfoModel? info, out string message);

        bool LoginAsCustomer(string username, string password, out CustomerInfoModel? info, out string message);

        bool LoginAsAdmin(string username, string password, out UserInfoModel? info, out string message);

        bool ExistUser(int id);

        bool UpdatePasswordForUserWithId(int userId, string oldPassword, string newPassword, out string message);

        bool ActivateUser(int id, out string message);

        bool InactivateUser(int id,out string message);

        // Utils
        bool CheckAccountAvailability(string username, string email, out string message);
    }
}
