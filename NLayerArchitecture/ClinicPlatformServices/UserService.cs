using ClinicPlatformDTOs.RoleModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformRepositories;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace ClinicPlatformServices
{
    public class UserService: IUserService
    {
        private readonly IUserRepository userRepository;
        private bool disposedValue;

        public UserService()
        {
            userRepository = new UserRepository();
        }

        public UserService(IUserRepository repository)
        {
            userRepository = repository;
        }

        public IEnumerable<UserInfoModel> GetUsers()
        {
            return userRepository.GetAll();
        }

        public UserInfoModel? GetWithUsernameAndPassword(string username, string password)
        {
            return userRepository.GetAll().Where(x => x.Username == username && x.Password == password).FirstOrDefault();
        }

        public bool DeleteUser(int id, out string message)
        {
            try 
            {
                userRepository.DeleteUser(id);
                message = "Delete user information Sucessfully";
                return true;
            }

            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public bool CheckLogin(string username, string password, out UserInfoModel? user)
        {
            user = GetWithUsernameAndPassword(username, password);

            if (user != null) 
            {
                return true;
            }

            return false;
        }

        public ClinicStaffInfoModel? GetClinicStaffInformation(int staffId)
        {
            return userRepository.GetStaffInfo(staffId);
        }

        public CustomerInfoModel? GetCustomerInformation(int customerId)
        {
            return userRepository.GetCustomerInfo(customerId);
        }

        public ClinicStaffInfoModel? GetClinicStaffInformationWithUserId(int userId)
        {
            var tempt = userRepository.GetUser(userId);

            if (tempt == null)
            {
                return null;
            }

            return userRepository.MapUserModelIntoStaffModel(tempt);
        }

        public CustomerInfoModel? GetCustomerInformationWithUserID(int userId)
        {
            var tempt = userRepository.GetUser(userId);

            if (tempt == null)
            {
                return null;
            }

            return userRepository.MapUserModelIntoCustomerModel(tempt);
        }

        public UserInfoModel? GetUserInformation(int userId)
        {
           return userRepository.GetUser(userId);
        }

        public UserInfoModel? GetUserInformationWithEmail(string email)
        {
            var SearchResult = userRepository.GetAll().Where(user => user.Email == email).ToList();

            return SearchResult.FirstOrDefault();
        }

        public bool LoginAsAdmin(string username, string password, out UserInfoModel? info, out string message)
        {
            info = GetWithUsernameAndPassword(username, password);

            if (info == null || info.Role != 1)
            {
                message = "Invalid Username or Password.";
                return false;
            }

            message = $"Username and Password matched to user with Id {info.Id}";
            return true;
        }

        public bool LoginAsClinicStaff(string username, string password, out ClinicStaffInfoModel? info, out string message)
        {
            var result = GetWithUsernameAndPassword(username, password);

            if (result == null || result.Role != 2)
            {
                message = "Invalid Username or Password.";
                info = null;
                return false;
            }

            info = userRepository.MapUserModelIntoStaffModel(result!);
            message = $"Username and Password matched to user with Id {info.Id}";
            return true;
        }

        public bool LoginAsCustomer(string username, string password, out CustomerInfoModel? info, out string message)
        {
            var result = GetWithUsernameAndPassword(username, password);

            if (result == null || result.Role != 3)
            { 
                message = "Invalid Username or Password.";
                info = null;
                return false;
            }

            info = userRepository.MapUserModelIntoCustomerModel(result!);
            message = $"Username and Password matched to user with Id {info.Id}";
            return true;
        }

        /// <summary>
        ///     Basically three method in one but with extra sauce!
        /// </summary>
        /// <param name="information"></param>
        /// <param name="role">User actual Role, will be override if IsAdmin is true</param>
        /// <param name="message"></param>
        /// <param name="IsAdmin">Granting the registing user an admin if true regardless of their role</param>
        /// <returns><see cref="true"/> if register sucessfully or <see cref="false"/> if failed.</returns>
        public bool RegisterAccount(UserRegistrationModel information, RoleModel.Roles role, out string message, bool IsAdmin = false)
        {

            if (!CheckAccountAvailability(information.Username, information.Email, out message))
            {
                return false;
            }

            if (!CheckValidUsername(information.Username))
            {
                message = "Username should contain at least 8 character starting with a alphabet character and containing at least one uppercase, lowercase or number with maxium of 30 character.";
                return false;
            }

            if (!CheckValidPassword(information.Password))
            {
                message = "Pass should contain at least 8 character and containing at least one uppercase, lowercase and number with maxium of 30 character.";
                return false;
            }

            if (role == RoleModel.Roles.Admin || IsAdmin)
            {
                UserInfoModel registerInfo = new UserInfoModel()
                {
                    Username = information.Username,
                    Password = information.Password,
                    Email = information.Email,
                    Role = 1,
                };

                UserInfoModel? newUser = userRepository.AddUser(registerInfo);

                if (newUser == null)
                {
                    message = "Failed while creating admin accoont";
                    return false;
                }

                message = "Created admin account successfully";
                return true;
            }
            else if (role == RoleModel.Roles.Customer)
            {
                if (!RegisterCustomerAccount(information, out message))
                {
                    return false;
                }

                return true;
            }
            else if (role == RoleModel.Roles.ClinicStaff)
            {
                if (!RegisterClinicStaffAccount(information, out message))
                {
                    return false;
                }
                return true;
            }
            else
            {
                message = "Unknown user role provided";
                return false;
            }
        }

        public bool RegisterClinicStaffAccount(UserRegistrationModel information, out string message)
        {
            try
            {
                UserInfoModel registerInfo = new UserInfoModel()
                {
                    Username = information.Username,
                    Password = information.Password,
                    Email = information.Email,
                    Fullname = information.Username,
                    Role = 2,
                    ClinicId = information.Clinic,
                    IsOwner = information.ClinicOwner
                };

                var result = userRepository.AddUser(registerInfo);

                if (result == null)
                {
                    message = "Failed";
                    return false;
                }
                message = "Success";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public bool RegisterCustomerAccount(UserRegistrationModel information, out string message)
        {
            try
            {
                UserInfoModel registerInfo = new UserInfoModel()
                {
                    Username = information.Username,
                    Password = information.Password,
                    Email = information.Email,
                    Fullname = information.Username,
                    Role = 3,
                };

                var result = userRepository.AddUser(registerInfo);

                if (result == null)
                {
                    message = "Failed";
                    return false;
                }
                message = "Success";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public bool UpdateUserInformation(UserInfoModel information, out string message)
        {

            if (userRepository.UpdateUser(information) != null)
            {
                message = "Update user information successfully";
                return true;
            }

            message = "Update user information failed";
            return false;
        }

        public bool UpdatePasswordForUserWithId(int userId, string oldPassword, string newPassword, out string message)
        {
            if (!CheckValidPassword(newPassword))
            {
                message = "New password must have minium length of 8 characters and maximum of 30 starting with an alphabet character and required to contains one uppercase, lowercase and one digit.";
                return false;
            }

            UserInfoModel? userInfo = GetUserInformation(userId);


            if (userInfo != null)
            {
                if (userInfo.Password != oldPassword)
                {
                    message = "Invalid user id or password provided.";
                    return false;
                }

                userInfo.Password = newPassword;

                if (!UpdateUserInformation(userInfo, out message))
                {
                    return false;
                }

                message = "Update user password successfully";
                return true;
            }

            message = $"User does not exist for Id {userId}";
            return false;
        }

        public bool CheckAccountAvailability(string? username, string? email, out string message)
        {
            if (username == null || email == null)
            {
                message = $"Missing required information {(username.IsNullOrEmpty() ? "username" : "")}{(email.IsNullOrEmpty() ? ", email" : "")}.";
                return false;
            }

            var AllUserInfo = userRepository.GetAll();

            if (AllUserInfo.Where(x => x.Username == username).Any())
            {
                message = "Username is taken by another account.";
                return false;
            }

            if (AllUserInfo.Where(x => x.Email == email).Any())
            {
                message = "Email is used by another account.";
                return false;
            }

            message = "Available! You are good to go.";
            return true;
        }

        public bool ExistUser(int id)
        {
            return userRepository.GetUser(id) == null;

        }

        public bool ActivateUser(int id, out string message)
        {
            UserInfoModel? user = GetUserInformation(id);

            if (user==null)
            {
                message = $"User does not exist for Id {id}";
                return false;
            }

            if (user.Status == true)
            {
                message = "User account already activated";
                return false;
            }

            user.Status = true;
            if (!UpdateUserInformation(user, out message))
            {
                return false;
            }
            message = "Success";
            return true;       
        }

        public bool InactivateUser(int id, out string message)
        {
            UserInfoModel? user = GetUserInformation(id);

            if (user == null)
            {
                message = $"User does not exist for Id {id}";
                return false;
            }

            if (user.Status == false)
            {
                message = "User account already inactivated";
                return false;
            }

            user.Status = false;
            if (!UpdateUserInformation(user, out message))
            {
                return false;
            }
            message = "Success";
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    userRepository.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        // Helper Functions
        public bool CheckValidUsername(string? username)
        {

            if (username.IsNullOrEmpty())
            {
                return false;
            }

            return Regex.Match(username!, "^[A-Za-z][A-Za-z0-9_]{7,29}$").Success;
        }

        public bool CheckValidPassword(string? password)
        {
            if (password.IsNullOrEmpty())
            {
                return false;
            }

            return Regex.Match(password!, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[a-zA-Z\\d]{8,30}$").Success;
        }

        // Mappers
        
    }
}
