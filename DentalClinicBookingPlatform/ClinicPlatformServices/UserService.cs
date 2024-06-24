using ClinicPlatformDTOs.UserModels;
using ClinicPlatformRepositories;
using System.Security.Cryptography;
using ClinicPlatformRepositories.Contracts;
using ClinicPlatformServices.Contracts;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.RegularExpressions;

namespace ClinicPlatformServices
{
    public class UserService: IUserService
    {
        private readonly IUserRepository userRepository;
        private bool disposedValue;

        public UserService(IUserRepository repository)
        {
            userRepository = repository;
        }

        public IEnumerable<UserInfoModel> GetUsers()
        {
            return userRepository.GetAllUser();
        }

        public UserInfoModel? GetUserWithUserId(int userId)
        {
            return userRepository.GetUser(userId);
        }

        public UserInfoModel? GetUserWithCustomerId(int customerId)
        {
            return userRepository.GetUserWithCustomerID(customerId);
        }

        public UserInfoModel? GetDentistWithDentistId(int dentistId)
        {
            return userRepository.GetUserWithDentistID(dentistId);
        }

        public UserInfoModel? GetUserWithEmail(string email)
        {
            return userRepository.GetUserWithEmail(email);
        }

        public IEnumerable<UserInfoModel> GetAllUserOfRole(string role)
        {
            return from user in userRepository.GetAllUser() where user.Role == role select user;
        }

        public UserInfoModel? GetWithUsernameAndPassword(string username, string password)
        {
            var user = userRepository.GetUserWithUsername(username);

            if (user == null)
            {
                user = userRepository.GetUserWithEmail(username);

                if (user == null)
                {
                    return null;
                }
            }

            if (HashPassword(password, user.Salt) == user.PasswordHash)
            {
                return user;
            }

            return null;
        }

        public IEnumerable<UserInfoModel> GetRemovedUser()
        {
            return userRepository.GetAllUser().Where(x => x.IsRemoved == true);
        }

        public IEnumerable<UserInfoModel> GetUserRegisteredOn(DateOnly date)
        {
            return userRepository.GetAllUser().Where(x => DateOnly.FromDateTime((DateTime)x.JoinedDate!) == date);
        }

        public bool DeleteUser(int id, out string message)
        {
            try 
            {
                userRepository.DeleteUser(id);
                message = "Delete user sucessfully";
                return true;
            }

            catch (Exception ex)
            {
                message = ex.InnerException?.Message ?? ex.Message;
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

        /// <summary>
        ///     Basically three method in one but with extra sauce!
        /// </summary>
        /// <param name="information"></param>
        /// <param name="role">User actual Role, will be override if IsAdmin is true</param>
        /// <param name="message"></param>
        /// <param name="IsAdmin">Granting the registing user an admin if true regardless of their role</param>
        /// <returns><see cref="true"/> if register sucessfully or <see cref="false"/> if failed.</returns>
        public UserInfoModel? RegisterAccount(UserInfoModel information, string role, out string message, bool IsAdmin = false)
        {

            if (!CheckAccountAvailability(information.Username, information.Email, out message))
            {
                return null;
            }

            if (!CheckValidUsername(information.Username))
            {
                message = "Username should contain at least 8 character starting with a alphabet character and containing at least one uppercase, lowercase or number with maxium of 30 character.";
                return null;
            }

            if (!CheckValidPassword(information.PasswordHash))
            {
                message = "Pass should contain at least 8 character and containing at least one uppercase, lowercase and number with maxium of 30 character.";
                return null;
            }

            information.Salt = CreateSalt(64);
            information.PasswordHash = HashPassword(information.PasswordHash, information.Salt);
            information.Role = role;

            if (role == "admin" || IsAdmin)
            {
                information.Role = "Admin";
                UserInfoModel? newUser = userRepository.AddUser(information);

                if (newUser == null)
                {
                    message = "Failed while creating admin account";
                    return null;
                }

                message = "Created admin account successfully";
                return newUser;
            }
            else if (role == "Customer")
            {
                UserInfoModel? newUser = userRepository.AddUser(information);

                if (newUser == null)
                {
                    message = "Failed while creating Customer account";
                    return null;
                }

                message = "Created Customer account successfully";
                return newUser;
            }
            else if (role == "Dentist")
            {
                UserInfoModel? newUser = userRepository.AddUser(information);

                if (newUser == null)
                {
                    message = "Failed while creating Dentist account";
                    return null;
                }

                message = "Created Dentist account successfully";
                return newUser;
            }
            else
            {
                message = "Unknown user role provided";
                return null;
            }
        }

        public UserInfoModel UpdateUserInformation(UserInfoModel information, out string message)
        {
            UserInfoModel user = userRepository.UpdateUser(information);
            if (user != null)
            {
                message = "Update user information successfully";
                return user;
            }

            message = "Update user information failed";
            return null ;
        }

        public bool UpdatePasswordForUserWithId(int userId, string newPassword, out string message)
        {
            if (!CheckValidPassword(newPassword))
            {
                message = "New password must have minium length of 8 characters and maximum of 30 starting with an alphabet character and required to contains one uppercase, lowercase and one digit.";
                return false;
            }

            UserInfoModel? userInfo = GetUserWithUserId(userId);


            if (userInfo != null)
            {
                userInfo.PasswordHash = HashPassword(newPassword, userInfo.Salt);

                return UpdateUserInformation(userInfo, out message) != null;
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

            var AllUserInfo = userRepository.GetAllUser();

            if (AllUserInfo.Where(x => x.Username == username).Any())
            {
                message = "Username is taken by another account.";
                return false;
            }

            if (AllUserInfo.Where(x => x.Email == email && !x.IsRemoved).Any())
            {
                message = "Email is used by another account.";
                return false;
            }

            message = "Available! You are good to go.";
            return true;
        }

        public bool ExistUser(int id)
        {
            return userRepository.GetUser(id) != null;

        }

        public bool ActivateUser(int id, out string message)
        {
            UserInfoModel? user = GetUserWithUserId(id);

            if (user==null)
            {
                message = $"User does not exist for Id {id}.";
                return false;
            }

            if (user.IsActive == true)
            {
                message = "User account already activated.";
                return false;
            }

            user.IsActive = true;
            if (userRepository.UpdateUser(user) == null)
            {
                message = "Failed while updating user information.";
                return false;
            }
            message = "Success";
            return true;       
        }

        public bool InactivateUser(int id, out string message)
        {
            UserInfoModel? user = GetUserWithUserId(id);

            if (user == null)
            {
                message = $"User does not exist for Id {id}";
                return false;
            }

            if (user.IsActive == false)
            {
                message = "User account already inactivated";
                return false;
            }

            user.IsActive = false;
            if (userRepository.UpdateUser(user) == null)
            {
                message = "Failed while updating user information.";
                return false;
            }
            message = "Success";
            return true;
        }

        public bool RemoveUser(int userId, out string message)
        {
            UserInfoModel? user = userRepository.GetUser(userId);

            if (user == null)
            {
                message = "Can not find user to remove";
                return false;
            }

            user.IsRemoved = true;

            userRepository.UpdateUser(user);

            message = $"Removed user {userId}!";
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
        string HashPassword(string password, string salt)
        {
            SHA512 hashFunction = SHA512.Create();

            var passwordByte = Encoding.UTF8.GetBytes(password + salt);

            var hashedPassword = hashFunction.ComputeHash(passwordByte);

            return Convert.ToBase64String(hashedPassword);
        }

        string CreateSalt(int size)
        {
            byte[] result = new byte[size];

            using (var RNG = RandomNumberGenerator.Create())
            {
                RNG.GetBytes(result);
            }

            return Convert.ToBase64String(result);
        }

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
    }
}
