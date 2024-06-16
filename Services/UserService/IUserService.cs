using Core.HttpModels.ObjectModels.Others;
using Core.HttpModels.ObjectModels.RegistrationModels;
using Core.HttpModels.ObjectModels.UserModel;
using Repositories.Models;
using System.Linq.Expressions;

namespace Services.UserService
{
    public interface IUserService : IDisposable
    {
        // ============ Create

        /// <summary>
        ///  Create new User with Customer role.
        /// </summary>
        /// <param name="information"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Boolean CreateCustomer(UserRegistrationModel information, out string message);

        /// <summary>
        ///  Create new User with Clinic Staff role.
        /// </summary>
        /// <param name="information"></param>
        /// <param name="isOwner"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Boolean CreateClinicStaff(UserRegistrationModel information, Boolean isOwner, out string message);

        // ============ Read

        /// <summary>
        ///  Find generic User information based on user ID. (should Include both Customer and Clinic Info)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>An <see cref="User"/> object that included information about <see cref="Customer"/> and <see cref="ClinicStaff"/></returns>
        User? GetUserInfo(int userId); // Generic usage to get all information of an user

        /// <summary>
        ///  Return all user inside the User table. This method is used in edge cases, just try not to call it.
        /// </summary>
        /// <returns></returns>
        IEnumerable<User> GetAllUserInfo(); // This is really bad to use and you know why.

        /// <summary>
        ///  Finds Customer information based on user ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Return a <see cref="Customer"/> with <see cref="User"/> related infromation.</returns>
        Customer? GetCustomerInfoById(int userId);

        /// <summary>
        ///  Finds Clinic Staff information based on user ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Return a <see cref="Customer"/> with <see cref="User"/> related infromation.</returns>
        ClinicStaff? GetClinicStaffInfoById(int userId);

        /// <summary>
        ///  Finds Customer information based on user ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Return a <see cref="Customer"/> with <see cref="User"/> related infromation.</returns>
        Customer? GetFromCustomerId(int customerId);
        ClinicStaff? GetFromStaffId(int staffId);
        IEnumerable<ClinicStaff> GetAllClinicStaffInfo(int clinicId);
        IEnumerable<User> GetBySimpleFilter(Expression<Func<User, bool>> filter);

        // ============ Update
        Boolean ActivateUser(int userId, out string message);

        Boolean ChangePassword(PasswordResetModel target, out string message);

        Boolean UpdateUserInformation(UserInfoModel userNewInfo, out string message);

        // ============ "Delete"

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

        // ============ Utils ===================
        Boolean IsClinicOwner(int userId);

        Boolean IsClinicOwner(User? user);

        User? Authenticate(string username, string password);

        User? AuthenticateWithEmail(string email, string password);

        Boolean ExistUser(Expression<Func<User, bool>> filter);

        string CreatePassword(int length);
    }
}
