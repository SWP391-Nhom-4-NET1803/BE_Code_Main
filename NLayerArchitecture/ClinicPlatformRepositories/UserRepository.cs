using ClinicPlatformBusinessObject;
using ClinicPlatformDAOs;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformRepositories.Contracts;
using System.Globalization;
using System.Numerics;

namespace ClinicPlatformRepositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDAO userDAO;
        private bool disposedValue;

        public UserRepository()
        {
            userDAO = new UserDAO();
            
        }

        public IEnumerable<UserInfoModel> GetAll()
        {
            var mapped = from result in userDAO.GetAllUsers()
                         select new UserInfoModel
                         {
                             Id = result.UserId,
                             Username = result.Username,
                             Password = result.Password,
                             Email = result.Email,
                             Fullname = result.Fullname,
                             Phone = result.PhoneNumber,
                             Status = result.Status,
                             Role = result.RoleId,
                             RoleName = result.Role.RoleName,

                             CustomerId = result.Customer?.CustomerId,
                             Sex = result.Customer?.Sex,
                             Insurance = result.Customer?.Insurance,
                             Birthdate = result.Customer?.BirthDate,

                             ClinicStaffId = result.ClinicStaff?.StaffId,
                             ClinicId = result.ClinicStaff?.ClinicId,
                             IsOwner = result.ClinicStaff?.IsOwner ?? false,

                         };

            return mapped;
        }

        public UserInfoModel? GetUser(int userId)
        {
            User? result = userDAO.GetUser(userId);

            if (result != null)
            {
                UserInfoModel mapped = new()
                {
                    Id = result.UserId,
                    Username = result.Username, 
                    Password = result.Password,
                    Email = result.Email,
                    Fullname = result.Fullname,
                    Phone = result.PhoneNumber,
                    Status = result.Status,
                    Role = result.RoleId,
                    RoleName = result.Role.RoleName,
                    JoinedDate = result.CreationDate,

                    CustomerId = result.Customer?.CustomerId,
                    Sex = result.Customer?.Sex,
                    Insurance = result.Customer?.Insurance,
                    Birthdate = result.Customer?.BirthDate,
                    
                    ClinicStaffId = result.ClinicStaff?.StaffId,
                    ClinicId = result.ClinicStaff?.ClinicId,
                    IsOwner = result.ClinicStaff?.IsOwner ?? false,
                };

                return mapped;
            }
            return null;
        }

        public UserInfoModel? AddUser(UserInfoModel userInfo)
        {
            User user = new()
            {
                Username = userInfo.Username!,
                Password = userInfo.Password!,
                Fullname = userInfo.Fullname,
                Email = userInfo.Email,
                PhoneNumber = userInfo.Phone,
                Status = false,
                CreationDate = DateTime.Now,
            };

            if (userInfo.Role == 1 || userInfo.RoleName == "Admin") 
            {
                user.RoleId = 1;
            }
            else if (userInfo.Role == 2 || userInfo.RoleName == "ClinicStaff")
            {

                user.RoleId = 2;
                user.ClinicStaff = new ClinicStaff()
                {
                    ClinicId = userInfo.ClinicId,
                    IsOwner = userInfo.IsOwner
                };
            }
            else if (userInfo.Role == 3 || userInfo.RoleName == "Customer")
            {
                user.RoleId = 3;
                user.Customer = new Customer()
                {
                    Insurance = userInfo.Insurance,
                    Sex = userInfo.Sex,
                    BirthDate = userInfo.Birthdate,
                };
            }
            else
            {
                return null;
            }

            userDAO.AddUser(user);

            return userInfo;
        }

        public UserInfoModel? UpdateUser(UserInfoModel userInfo)
        {
            User? target = userDAO.GetUser(userInfo.Id);

            if (target != null)
            {

                target.Username = userInfo.Username?? target.Username;
                target.Password = userInfo.Password?? target.Password;
                target.Fullname = userInfo.Fullname?? target.Password;
                target.Email = userInfo.Email?? target.Email;
                target.PhoneNumber = userInfo.Phone ?? target.PhoneNumber;
                target.Status = userInfo.Status?? false;

                if (target.RoleId == 2)
                {
                    target.ClinicStaff!.ClinicId = userInfo.ClinicId;
                    target.ClinicStaff!.IsOwner = userInfo.IsOwner;
                }

                if (target.RoleId == 3)
                {
                    target.Customer!.BirthDate = userInfo.Birthdate ?? target.Customer.BirthDate;
                    target.Customer!.Insurance = userInfo.Insurance ?? target.Customer.Insurance;
                    target.Customer!.Sex = userInfo.Sex ?? target.Customer.Sex;
                }

                userDAO.UpdateUser(target);
                SaveChanges();

                return userInfo;
            }

            return null;
        }

        public void DeleteUser(int userId)
        {
            userDAO.DeleteUser(userId);
        }

        public CustomerInfoModel? GetCustomerInfo(int customerId)
        {
            var result = userDAO.GetCustomerByCustomerId(customerId);

            if (result != null) 
            {
                return MapCustomerToCustomerModel(result);
            }

            return null;
        }

        public ClinicStaffInfoModel? GetStaffInfo(int staffId)
        {
            var result = userDAO.GetStaffByStaffId(staffId);

            if (result != null)
            {
                return MapClinicStaffToClinicStaffModel(result);
            }

            return null;
        }

        public void SaveChanges()
        {
            userDAO.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    userDAO.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private CustomerInfoModel MapCustomerToCustomerModel(Customer user)
        {
            return new CustomerInfoModel()
            {
                Id = user.UserId,
                CustomerId = user.CustomerId,
                Username = user.User.Username,
                Password = user.User.Password,
                Fullname = user.User.Fullname,
                Email = user.User.Email,
                Phone = user.User.PhoneNumber,
                Birthdate = user.BirthDate,
                Insurance = user.Insurance,
                Sex = user.Sex,
                Role = user.User.RoleId,
                JoinedDate = user.User.CreationDate,
                Status = user.User.Status
            };
        }

        private ClinicStaffInfoModel MapClinicStaffToClinicStaffModel(ClinicStaff user)
        {
            return new ClinicStaffInfoModel()
            {
                Id = user.UserId,
                StaffId = user.StaffId,
                Username = user.User.Username,
                Password = user.User.Password,
                Fullname = user.User.Fullname,
                Email = user.User.Email,
                Phone = user.User.PhoneNumber,
                IsOwner = user.IsOwner,
                ClinicId = user.ClinicId,
                ClinicName = user.Clinic?.Name,
                Role = user.User.RoleId,
                JoinedDate = user.User.CreationDate,
                Status = user.User.Status
            };
        }


        public UserInfoModel MapCustomerModelIntoUserModel(CustomerInfoModel customer)
        {
            return new UserInfoModel()
            {
                Id = customer.Id,
                CustomerId = customer.CustomerId,
                Username = customer.Username,
                Password = customer.Password,
                Role = customer.Role,
                Fullname = customer.Fullname,
                Email = customer.Email,
                Phone = customer.Phone,
                Insurance = customer.Insurance,
                Birthdate = customer.Birthdate,
                Sex = customer.Sex,
                JoinedDate = customer.JoinedDate,
                Status = customer.Status,
            };
        }

        public UserInfoModel MapStaffModelIntoUserModel(ClinicStaffInfoModel staff)
        {
            return new UserInfoModel()
            {
                Id = staff.Id,
                ClinicStaffId = staff.StaffId,
                Username = staff.Username,
                Password = staff.Password,
                Role = staff.Role,
                Fullname = staff.Fullname,
                Email = staff.Email,
                Phone = staff.Phone,
                ClinicId = staff.ClinicId,
                IsOwner = staff.IsOwner ?? false,
                JoinedDate = staff.JoinedDate,
                Status = staff.Status,
            };
        }

        public CustomerInfoModel MapUserModelIntoCustomerModel(UserInfoModel customer)
        {
            return new CustomerInfoModel()
            {
                Id = customer.Id,
                CustomerId = customer.CustomerId,
                Username = customer.Username,
                Password = customer.Password,
                Role = customer.Role,
                Fullname = customer.Fullname,
                Email = customer.Email,
                Phone = customer.Phone,
                Insurance = customer.Insurance,
                Birthdate = customer.Birthdate,
                Sex = customer.Sex,
                JoinedDate = customer.JoinedDate,
                Status = customer.Status,
            };
        }

        public ClinicStaffInfoModel MapUserModelIntoStaffModel(UserInfoModel staff)
        {
            return new ClinicStaffInfoModel()
            {
                Id = staff.Id,
                StaffId = staff.ClinicStaffId ?? 0,
                Username = staff.Username,
                Password = staff.Password,
                Role = staff.Role,
                Fullname = staff.Fullname,
                Email = staff.Email,
                Phone = staff.Phone,
                ClinicId = staff.ClinicId,
                IsOwner = staff.IsOwner,
                JoinedDate = staff.JoinedDate,
                Status = staff.Status,
            };
        }
    }
}
