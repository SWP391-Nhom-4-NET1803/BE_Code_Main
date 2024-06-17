using AutoMapper;
using ClinicPlatformBusinessObject;
using ClinicPlatformDAOs;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformRepositories.Contracts;

namespace ClinicPlatformRepositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDAO userDAO;
        private readonly IMapper _mapper;
        private bool disposedValue;

        public UserRepository(IMapper mapper)
        {
            _mapper = mapper;
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
                UserInfoModel mapped = new UserInfoModel()
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
            return null;
        }

        public UserInfoModel? AddUser(UserInfoModel userInfo)
        {
            User user = new User()
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
                target.Email = userInfo.Email;
                target.PhoneNumber = userInfo.Phone;
                target.Status = userInfo.Status?? false;
                target.CreationDate = DateTime.Now;

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
                return _mapper.Map<Customer, CustomerInfoModel>(result);
            }

            return null;
        }

        public ClinicStaffInfoModel? GetStaffInfo(int staffId)
        {
            var result = userDAO.GetStaffByStaffId(staffId);

            if (result != null)
            {
                return _mapper.Map<ClinicStaff, ClinicStaffInfoModel>(result);
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

        
    }
}
