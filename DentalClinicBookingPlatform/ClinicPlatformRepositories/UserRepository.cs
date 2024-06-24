using ClinicPlatformDatabaseObject;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformRepositories.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Numerics;

namespace ClinicPlatformRepositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DentalClinicPlatformContext context;
        private bool disposedValue;

        public UserRepository(DentalClinicPlatformContext context)
        {
            this.context = context; 
        }

        public UserInfoModel? AddUser(UserInfoModel userInfo)
        {
            User user = MapToUser(userInfo);

            user.CreationTime = DateTime.Now;

            context.Users.Add(user);
            context.SaveChanges();
            return MapToUserInfo(user);
        }

        public IEnumerable<UserInfoModel> GetAllUser(bool includeRemoved = true, bool includeInactive = true)
        {
            IEnumerable<User> userList = context.Users.Include(x => x.Customer).Include(x => x.Dentist);

            Console.WriteLine($"{userList.Count()}");

            if (!includeRemoved)
            {
                userList = userList.Where(x => !x.Removed);
            }

            if (!includeInactive)
            {
                userList = userList.Where(x => x.Active);
            }

            return from user in userList.ToList() select MapToUserInfo(user);
        } 

        public UserInfoModel? GetUser(int userId)
        {
            User? user = context.Users.Find(userId);

            if (user != null)
            {
                return MapToUserInfo(user);
            }

            return null;
        }

        public UserInfoModel? GetUserWithCustomerID(int customerId)
        {
            return GetAllUser().Where(x => x.CustomerId == customerId).FirstOrDefault();
        }

        public UserInfoModel? GetUserWithDentistID(int dentistId)
        {

            return GetAllUser().Where(x => x.DentistId == dentistId).FirstOrDefault();
        }

        public UserInfoModel? GetUserWithEmail(string email)
        {
            return GetAllUser().Where(x => x.Email == email).FirstOrDefault();
        }

        public UserInfoModel? GetUserWithUsername(string username)
        {
            var user = GetAllUser().Where(x => x.Username == username);

            if (user != null)
            {
                Console.WriteLine(GetAllUser().FirstOrDefault().Username);
            }

            return user.FirstOrDefault();
        }

        public void DeleteUser(int userId)
        {
            User? user = context.Users.Find(userId);

            if (user == null)
            {
                throw new InvalidOperationException($"The user with provided Id {userId} does not exist!");
            }

            user.Removed = true;

            context.Users.Update(user);
            context.SaveChanges();
        }

        public UserInfoModel? UpdateUser(UserInfoModel userInfo)
        {
            context.Users.Update(MapToUser(userInfo));
            context.SaveChanges();
            return userInfo;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    context.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        User MapToUser(UserInfoModel userInfo)
        {
            User user = new User()
            {
                Username = userInfo.Username,
                PasswordHash = userInfo.PasswordHash,
                Salt = userInfo.Salt,
                Email = userInfo.Email,
                Phone = userInfo.Phone,
                Fullname = userInfo.Fullname,
                Active = false,
                Removed = false,
                CreationTime = DateTime.UtcNow,
                Role = userInfo.Role,
            };

            if (userInfo.Role == "Admin")
            {
                user.Active = true;
            }
            else if (userInfo.Role == "Dentist")
            {
                user.Dentist = new Dentist()
                {
                    ClinicId = null,
                    IsOwner = userInfo.IsOwner ?? false,
                };
            }
            else if (userInfo.Role == "Customer")
            {
                user.Customer = new Customer()
                {
                    Birthdate = userInfo.Birthdate,
                    Sex = userInfo.Sex,
                    Insurance = userInfo.Insurance,
                };
            }

            return user;
        }

        UserInfoModel MapToUserInfo(User user)
        {
            UserInfoModel userInfo = new UserInfoModel()
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Fullname = user.Fullname!,
                Phone = user.Phone!,
                IsActive = user.Active,
                IsRemoved = user.Removed,
                JoinedDate = user.CreationTime,
                PasswordHash = user.PasswordHash,
                Salt = user.Salt,
                Role = user.Role,
            };

            if (user.Role == "Dentist")
            {
                userInfo.DentistId = user.Dentist?.Id;
                userInfo.ClinicId = user.Dentist?.ClinicId!;
                userInfo.IsOwner = user.Dentist?.IsOwner ?? false;
            }
            else if (user.Role == "Customer")
            {
                userInfo.CustomerId = user.Id;
                userInfo.Insurance = user.Customer?.Insurance!;
                userInfo.Sex = user.Customer?.Sex!;
                userInfo.Birthdate = user.Customer?.Birthdate!;
            }
            
            return userInfo;
        }
    }
}
