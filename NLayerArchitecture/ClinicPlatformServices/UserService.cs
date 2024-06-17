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
        private readonly IUserRepository _userRepository;
        private bool disposedValue;

        public UserService()
        {
            _userRepository = new UserRepository();
        }

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IEnumerable<UserInfoModel> GetUsers()
        {
            return _userRepository.GetAll();
        }

        public UserInfoModel? GetWithUsernameAndPassword(string username, string password)
        {
            return _userRepository.GetAll().Where(x => x.Username == username && x.Password == password).FirstOrDefault();
        }

        public bool DeleteUser(int id, out string message)
        {
            try 
            {
                _userRepository.DeleteUser(id);
                message = "Delete user information Sucessfully";
                return true;
            }

            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public bool checkLogin(string username, string password, out UserInfoModel? user)
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
            return _userRepository.GetStaffInfo(staffId);
        }

        public CustomerInfoModel? GetCustomerInformation(int customerId)
        {
            return _userRepository.GetCustomerInfo(customerId);
        }

        public UserInfoModel? GetUserInformation(int userId)
        {
           return _userRepository.GetUser(userId);
        }

        public bool loginAsAdmin(string username, string password, out UserInfoModel? info, out string message)
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

        public bool loginAsClinicStaff(string username, string password, out ClinicStaffInfoModel? info, out string message)
        {
            var result = GetWithUsernameAndPassword(username, password);

            if (result == null || result.Role != 2)
            {
                message = "Invalid Username or Password.";
                info = null;
                return false;
            }

            info = _userRepository.MapUserModelIntoStaffModel(result!);
            message = $"Username and Password matched to user with Id {info.Id}";
            return true;
        }

        public bool loginAsCustomer(string username, string password, out CustomerInfoModel? info, out string message)
        {
            var result = GetWithUsernameAndPassword(username, password);

            if (result == null || result.Role != 3)
            { 
                message = "Invalid Username or Password.";
                info = null;
                return false;
            }

            info = _userRepository.MapUserModelIntoCustomerModel(result!);
            message = $"Username and Password matched to user with Id {info.Id}";
            return true;
        }

        /// <summary>
        ///  Basically three in one with extra sauce!
        /// </summary>
        /// <param name="information"></param>
        /// <param name="role"></param>
        /// <param name="message"></param>
        /// <param name="IsAdmin"></param>
        /// <returns></returns>
        public bool RegisterAccount(UserRegistrationModel information, RoleModel.Roles role, out string message, bool IsAdmin = false)
        {

            if (!checkAccountAvailability(information.Username, information.Email, out message))
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

                UserInfoModel? newUser = _userRepository.AddUser(registerInfo);

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

                var result = _userRepository.AddUser(registerInfo);

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

                var result = _userRepository.AddUser(registerInfo);

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

            if (_userRepository.UpdateUser(information) != null)
            {
                message = "Update user information successfully";
                return true;
            }

            message = "Update user information failed";
            return false;
        }

        public bool checkAccountAvailability(string? username, string? email, out string message)
        {
            if (username == null || email == null)
            {
                message = $"Missing required information {(username.IsNullOrEmpty() ? "username" : "")}{(email.IsNullOrEmpty() ? ", email" : "")}.";
                return false;
            }

            var AllUserInfo = _userRepository.GetAll();

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
            return _userRepository.GetUser(id) == null;

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
                    _userRepository.Dispose();
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
