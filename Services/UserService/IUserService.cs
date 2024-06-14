using Core.HttpModels.ObjectModels.Others;
using Core.HttpModels.ObjectModels.RegistrationModels;
using Core.HttpModels.ObjectModels.UserModel;
using Repositories.Models;
using System.Linq.Expressions;

namespace Services.UserService
{
    public interface IUserService : IDisposable
    {
        // Create
        Boolean createCustomer(UserRegistrationModel information, out string message);
        Boolean createClinicStaff(UserRegistrationModel information, Boolean isOwner, out string message);

        // Read
        IEnumerable<User> getAllUserInfo(); // This is really bad to use and you know why.
        Customer? getCustomerInfoById(int customerId);
        ClinicStaff? getClinicStaffInfo(int staffId);
        IEnumerable<ClinicStaff> getAllClinicStaffInfo(int clinicId);
        IEnumerable<User> SimpleFilter(Expression<Func<User, bool>> filter);

        // Update
        Boolean changePassword(PasswordResetModel target, out string message);

        Boolean updateUserInformation(UserInfoModel userNewInfo, out string message);

        // "Delete"

        /// <summary>
        ///     Temporarily "remove" the user by disabling user status. (set the status to false)
        /// </summary>
        /// <param name="userId">the user identity number</param>
        /// <returns>true if success, else false</returns>
        Boolean InactivateUser(int userId, out string message);

        /// <summary>
        ///     This will totally remove the user (including the customer and clinicStaff account if any)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Boolean RemoveUser(int userId, out string message);

        // Utils
        Boolean IsClinicOwner(int userId);

        User? Authenticate(string username, string password);

        User? AuthenticateWithEmail(string email, string password);

        Boolean ExistUser(Expression<Func<User, bool>> filter);

        string CreatePassword(int length);
    }
}
