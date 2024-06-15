using Core.HttpModels;
using Core.HttpModels.ObjectModels.Others;
using Core.HttpModels.ObjectModels.RegistrationModels;
using Core.HttpModels.ObjectModels.UserModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services.UserService
{
    public class UserService: IUserService
    {
        private UnitOfWork _unitOfWork;
        private bool disposedValue;

        public UserService(DentalClinicPlatformContext context) 
        {
            this._unitOfWork = new UnitOfWork(context);
        }

        public bool CreateClinicStaff(UserRegistrationModel information, bool isOwner, out string message)
        {
            try
            {
                // Validate information
                if (information.Email.IsNullOrEmpty())
                {
                    message = "userEmail is required!";
                    return false;
                }

                if (!CheckValidUsername(information.Username))
                {
                    message = "username length should be between 7 and 30 and contain no white space or special character!";
                    return false;
                }

                if (!CheckValidPassword(information.Password))
                {
                    message = "password should be between 8 and 30 characters long. Containing at least one uppercase, lowercase, and digit!";
                    return false;
                }

                // Check for user availability before register them in the database.
                if (!_unitOfWork.UserRepository.CheckAvailability(information.Username!, information.Email!, out var innerMessage))
                {
                    message = innerMessage;
                    return false;
                }

                User newUser = new()
                {
                    Username = information.Username!,
                    Fullname = information.Username,
                    Password = information.Password!,
                    Email = information.Email!,
                    Status = true,
                    RoleId = 2,
                };
                _unitOfWork.UserRepository.Add(newUser);

                _unitOfWork._context.ClinicStaffs.Add(new ClinicStaff() { User = newUser, IsOwner = isOwner, ClinicId = isOwner ? null : information.Clinic });

                message = "Valid user information! Successfully created new user.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public bool CreateCustomer(UserRegistrationModel information, out string message)
        {
            try
            {
                // Validate information
                if (information.Email.IsNullOrEmpty())
                {
                    message = "userEmail is required!";
                    return false;
                }

                if (!CheckValidUsername(information.Username))
                {
                    message = "username length should be between 7 and 30 and contain no white space or special character!";
                    return false;
                }

                if (!CheckValidPassword(information.Password))
                {
                    message = "password should be between 8 and 30 characters long. Containing at least one uppercase, lowercase, and digit!";
                    return false;
                }

                // Check for user availability before register them in the database.
                if (!_unitOfWork.UserRepository.CheckAvailability(information.Username!, information.Email!, out var innerMessage))
                {
                    message = innerMessage;
                    return false;
                }


                User newUser = new()
                {
                    Username = information.Username!,
                    Fullname = information.Username,
                    Password = information.Password!,
                    Email = information.Email!,
                    Status = true,
                    RoleId = 3,
                };
                _unitOfWork.UserRepository.Add(newUser);

                _unitOfWork._context.Customers.Add(new Customer() { User = newUser });

                message = "Valid user information! Successfully created new user.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public IEnumerable<ClinicStaff> GetAllClinicStaffInfo(int clinicId)
        {
            return _unitOfWork._context.ClinicStaffs.Where(x => x.ClinicId == clinicId).ToList();
        }

        public IEnumerable<User> GetAllUserInfo()
        {
            return _unitOfWork.UserRepository.GetAll();
        }

        public User? GetUserInfo(int userId)
        {
            return _unitOfWork._context.Users.Include(x => x.Customers.FirstOrDefault()).Include(x=>x.ClinicStaffs.FirstOrDefault()).Where(x => x.UserId == userId).FirstOrDefault();
        }

        public ClinicStaff? GetClinicStaffInfoById(int userId)
        {
            return _unitOfWork._context.ClinicStaffs.Include(x => x.User).Include(x => x.Clinic).Where(x => x.UserId == userId).ToList().FirstOrDefault();
        }

        public Customer? GetCustomerInfoById(int userId)
        {
            
            return _unitOfWork._context.Customers.Include(x => x.User).Where(x => x.UserId == userId).ToList().FirstOrDefault();
        }

        public Customer? GetFromCustomerId(int customerId)
        {
            return _unitOfWork._context.Customers.Include(x => x.User).Where(x=> x.CustomerId == customerId).ToList().FirstOrDefault();
        }

        public ClinicStaff? GetFromStaffId(int staffId)
        {
            return _unitOfWork._context.ClinicStaffs.Include(x => x.User).Where(x => x.StaffId == staffId).ToList().FirstOrDefault();
        }

        public bool ChangePassword(PasswordResetModel target, out string message)
        {
            if (target.Email.IsNullOrEmpty())
            {
                message = "No target email was provided for this operation!";
                return false;
            }

            if (target.PasswordReset.IsNullOrEmpty())
            {
                message = "No password was provided for this operation";
                return false;
            }

            User? user = _unitOfWork.UserRepository.GetUserWithEmail(target.Email);

            if (user == null)
            {
                message = "No user found with provided email!";
                return false;
            }

            if (!Regex.Match(target.PasswordReset!, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[a-zA-Z\\d]{8,}$").Success)
            {
                message = "password is not long enough (at least 10 characters, including uppercase, lowercase and digits) ";
                return false;
            }

            user.Password = target.PasswordReset!;
            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();

            message = "Password was changed successfully!";
            return true;
        }

        public bool ActivateUser(int userId, out string message)
        {
            var user = _unitOfWork.UserRepository.GetById(userId);

            if (user == null)
            {
                message = "No user found with provided information";
                return false;
            }

            user.Status = true;

            message = "Update user status successfully!";
            return true;
        }
        public bool InactivateUser(int userId, out string message)
        {
            var user = _unitOfWork.UserRepository.GetById(userId);
                    
            if (user == null)
            {
                message = "No user found with provided information";
                return false;
            }
                
            user.Status = false;

            message = "Update user status successfully!";
            return true;
        }
        public bool RemoveUser(int userId, out string message)
        {
            try
            {
                var user = _unitOfWork.UserRepository.GetById(userId) ?? throw new Exception("No user found with provided information");

                string randomInfo = Guid.NewGuid().ToString();

                user.Email = randomInfo;
                user.Username = randomInfo;
                user.Password = randomInfo;
                user.Fullname = null;
                user.PhoneNumber = null;
                user.Status = false;

                if (user.RoleId == 3)
                {
                    var customer = _unitOfWork.UserRepository.GetCustomerInfo(userId)!;
                    customer.Insurance = null;
                    customer.BirthDate = null;
                    customer.Sex = null;

                    _unitOfWork._context.Customers.Update(customer);
                }

                if (user.RoleId == 2)
                {
                    var staff = _unitOfWork.UserRepository.GetStaffInfo(userId)!;
                    staff.IsOwner = false;
                    staff.ClinicId = null;

                    _unitOfWork._context.ClinicStaffs.Update(staff);
                }

                _unitOfWork.UserRepository.Update(user);

                message = "Removed user successfully!";
                return true;

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
        public bool UpdateUserInformation(UserInfoModel userNewInfo, out string message)
        {
            try
            {
                if (_unitOfWork.UserRepository.ExistUser(userNewInfo.Id, out var OldInfo))
                {
                    OldInfo!.Username = userNewInfo.Username.IsNullOrEmpty() ? OldInfo.Username! : userNewInfo.Username!;
                    OldInfo.Fullname = userNewInfo.Fullname.IsNullOrEmpty() ? OldInfo.Fullname : userNewInfo.Fullname;
                    OldInfo.Email = userNewInfo.Email.IsNullOrEmpty() ? OldInfo.Email : userNewInfo.Email!;
                    OldInfo.PhoneNumber = userNewInfo.Phone.IsNullOrEmpty() ? OldInfo.PhoneNumber : userNewInfo.Phone;
                    _unitOfWork.UserRepository.Update(OldInfo);

                    if (OldInfo.RoleId == 3)
                    {
                        var customerInfo = _unitOfWork.UserRepository.GetCustomerInfo(userNewInfo.Id)!;
                        customerInfo.Sex = userNewInfo.Sex ?? customerInfo.Sex;
                        customerInfo.Insurance = userNewInfo.Insurance ?? customerInfo.Insurance;
                        customerInfo.BirthDate = userNewInfo.Birthdate ?? customerInfo.BirthDate;
                        _unitOfWork._context.Customers.Update(customerInfo);
                    }

                    if (OldInfo.RoleId == 2)
                    {
                        var staffInfo = _unitOfWork.UserRepository.GetStaffInfo(userNewInfo.Id)!;
                        staffInfo.ClinicId = userNewInfo.Clinic ?? staffInfo.ClinicId;
                        staffInfo.IsOwner = userNewInfo.IsOwner ?? staffInfo.IsOwner;
                        _unitOfWork._context.ClinicStaffs.Update(staffInfo);
                    }
                }

                message = "succeed";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
        public bool IsClinicOwner(int userId)
        {
            var clinicStaff = _unitOfWork._context.ClinicStaffs.Where(x => x.UserId == userId).First();

            if (clinicStaff == null || !clinicStaff.IsOwner)
            {
                return false;
            }

            return true;
        }

        public bool IsClinicOwner(User? user)
        {

            if (user == null)
            {
                return false;
            }

            var clinicStaff = _unitOfWork._context.ClinicStaffs.Where(x => x.UserId == user.UserId).First();

            if (clinicStaff == null || !clinicStaff.IsOwner)
            {
                return false;
            }

            return true;
        }

        public User? Authenticate(string username, string password)
        {
            return _unitOfWork.UserRepository.Authenticate(username, password);
        }

        public User? AuthenticateWithEmail(string email, string password)
        {
            return _unitOfWork._context.Users.Where(x => x.Email == email && x.Password == password).First();
        }

        public IEnumerable<User> GetBySimpleFilter(Expression<Func<User, bool>> filter)
        {
            return _unitOfWork._context.Users.Where(filter).ToList();
        }

        public bool ExistUser(Expression<Func<User, bool>> filter)
        {
            return _unitOfWork._context.Users.Where(filter).Any();
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

        public string CreatePassword(int length)
        {
            if (length < 8)
            {
                throw new Exception("Password must be at least 8 character!");
            }

            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            var rng = new Random();
            
            while (!Regex.Match(res.ToString(), "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[a-zA-Z\\d]{8,30}$").Success)
            
            for (int i = 0; i < length; i++)
            {
                res.Append(valid[rng.Next(valid.Length)]);
            }

            return res.ToString();
        }

        // ================================== Tested and ready to deploy ==============================================

        // ==================================== Finished and untested  =================================================

        // ===========================================  Unfinished =====================================================


        // =================================== Generated ==================================================

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
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
