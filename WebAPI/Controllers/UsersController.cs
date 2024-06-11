using Core.HttpModels;
using Core.HttpModels.ObjectModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Models;
using Services.EmailSerivce;
using Services.JwtManager;
using Services.TokenManager;
using System.Net;
using System.Security.Claims;
using System.Text;
using WebAPI.Helper.AuthorizationPolicy;

namespace WebAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UnitOfWork _unitOfWork;

        public UsersController(IConfiguration config, DentalClinicPlatformContext context)
        {
            _config = config;
            _unitOfWork = new UnitOfWork(context);
        }

        /// <summary>
        ///     <para>Thực hiện thay đổi password của người dùng một cách tự động sau đó gửi email thông báo mật khẩu mới</para>
        /// </summary>
        /// <param name="target">Email của người dùng cần thay đổi mật khẩu</param>
        /// <returns>Kết quả</returns>
        [HttpPost]
        [Route("request-reset")]
        [AllowAnonymous]
        public async  Task<ActionResult> RequestResetPassword([FromBody] PasswordResetModel target)
        {
            User? user = _unitOfWork.UserRepository.GetUserWithEmail(target.Email);

            if (user == null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Email này chưa được sử dụng để đăng kí tài khoản trong hệ thống.",
                    Detail = $"Địa chỉ email {target.Email} chưa được sử dụng, hãy đăng kí tài khoản và thử lại sau."
                });
            }

            string newPassword = CreatePassword(24);

            user.Password = newPassword;

            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();

            string UserFullname = user.Fullname ?? $"người dùng {user.UserId}";

            string subject = $"Khôi phục mật khẩu cho tài khoản {user.Username}";

            string body = $"Xin chào, <b>{UserFullname}</b>!<br/>" +
                $"Chúng tôi đã nhận được yêu cầu thay đổi mật khẩu tài khoản của bạn, hãy sử dụng mật khẩu <b>{newPassword}</b> cho lần đăng nhập kế tiếp và thay đổi mật khẩu của bạn.<br/>";

            var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

            await emailService.SendMailGoogleSmtp(target.Email, subject, body);

            return Ok(new HttpResponseModel() { StatusCode = 202, Message = "Yêu cầu được chấp thuận." });
        }

        /// <summary>
        ///  Thực hiện thay đổi mật khẩu của người dùng dựa trên mật khẩu mới nhập của họ.
        /// </summary>
        /// <param name="target">Email của người dùng cần thay đổi mật khẩu cũng như mật khẩu mới</param>
        /// <returns>Kết quả của việc thay đổi nói trên</returns>
        [HttpPost("reset-password")]
        [JwtTokenAuthorization]
        public ActionResult ResetPassword([FromBody] PasswordResetModel target)
        {
            // Searching for user that invoked the reset password prompt in order to validate user existance
            // and send confirmation emails after changing their password.
            User? user = _unitOfWork.UserRepository.GetUserWithEmail(target.Email);

            if (user == null)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Email này chưa được sử dụng để đăng kí tài khoản trong hệ thống." });
            }

            if (target.PasswordReset == null || target.PasswordReset.Length < 10)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Mật khẩu mới không hợp lệ" });
            }

            user.Password = target.PasswordReset;

            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();


            string emailBody = $"<p></p> <p>Mật khẩu của bạn đã được thay đổi, hãy sử dụng mật khẩu mới cho lần đăng nhập kế tiếp. Trong trường hợp bạn không phải là người thực hiện việc thay đổi mật khẩu, hãy đổi lại mật khẩu ở mục <b>Quên mật khẩu</b></p>";

            // Sending confirmation email to user.
            var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

            emailService.SendMailGoogleSmtp(target: target.Email, subject: $"Thay đổi mật khẩu cho tài khoản ${user.Username}", body: emailBody);

            return Ok(new HttpResponseModel() { StatusCode = 202, Message = "Accepted" });
        }

        /// <summary>
        ///  Get user detailed information based on the valdated token.
        /// </summary>
        /// <returns>A User Info Model thats contains basic user information</returns>
        [HttpGet("info")]
        [JwtTokenAuthorization]
        public ActionResult<UserInfoModel> GetUser()    
        {
            var user = (User) HttpContext.Items["user"]!;

            if (user == null)
            {
                return Ok(new HttpResponseModel() { StatusCode = 401, Message = "Token is expired, invalid or user does not exist." });
            }

            UserInfoModel userInfo = new()
            {
                Id = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Fullname = user.Fullname ?? null,
                JoinedDate = user.CreationDate,
                Phone = user.PhoneNumber ?? null,
                Role = _unitOfWork.RoleRepository.GetById(user.RoleId)!.RoleName,
                Status = user.Status ? "verified" : "unverified",
            };

            return Ok(userInfo);
        }

        /// <summary>
        ///  Get user detailed information based on the valdated token.
        /// </summary>
        /// <returns>A User Info Model thats contains basic user information</returns>
        [HttpGet("info-free{id}")]
        public ActionResult<UserInfoModel> GetUserFree(int id)
        {
            var user = _unitOfWork.UserRepository.GetById(id);

            if (user == null)
            {
                return Ok(new HttpResponseModel() { StatusCode = 401, Message = "Token is expired, invalid or user does not exist." });
            }

            return Ok(user);
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCustomer([FromBody] UserRegistrationModel requestObject)
        {
            try
            {
                // Validate information
                if (requestObject.Email.IsNullOrEmpty())
                {
                    return Ok(new HttpResponseModel() { StatusCode = 400, Message = "No email were given" });
                }

                if (requestObject.Username.IsNullOrEmpty())
                {
                    return Ok(new HttpResponseModel() { StatusCode = 400, Message = "No email were given" });
                }

                if (requestObject.Password.IsNullOrEmpty())
                {
                    return Ok(new HttpResponseModel() { StatusCode = 400, Message = "No password were given" });
                }

                // Check for user availability before register them in the database.
                if (!_unitOfWork.UserRepository.CheckAvailability(requestObject.Username!, requestObject.Email!, out var responseMessage))
                {
                    return Ok(new HttpResponseModel()
                    {
                        StatusCode = 400,
                        Message = "Không thể thực hiện yêu cầu tạo mới người dùng.",
                        Detail = responseMessage
                    });
                }


                User newUser = new()
                {
                    Username = requestObject.Username!,
                    Password = requestObject.Password!,
                    Email = requestObject.Email!,
                    Status = true,
                    RoleId = 3,
                };
                _unitOfWork.UserRepository.Add(newUser);
                _unitOfWork.Save();

                var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

                string body = $"Xin chào người dùng! <br/>" +
                    $"Chúng tôi đã nhận được yêu cầu tạo tài khoản cho email {requestObject.Email}, cảm ơn bạn đã đăng kí dịch vụ của chúng tôi. <br/>" +
                    $"Vui lòng xác thực tài khoản thông qua cổng xác thực của chúng tôi tại [Tạo trang xác thực bên phía front-end call tới api xác thực phía backend]";

                if (!await emailService.SendMailGoogleSmtp(requestObject.Email!, "Xác nhận yêu cầu tạo tài khoản người dùng", body))
                {
                    throw new Exception("Không thể gửi email cho người dùng");
                }

                return Ok(new HttpResponseModel() { StatusCode = 202, Message = "Yêu cầu tạo mới người dùng đang được xử lí." });
            }
            catch (Exception ex)
            {
                return Ok(new HttpResponseModel() { StatusCode = 500, Message = "Internal Server Error", Detail = ex.Message });
            }
        }

        [HttpPut]
        [Route("put")]
        [JwtTokenAuthorization]
        public IActionResult PutUser(UserInfoModel UpdatedInfo)
        {
            try
            {
                if (_unitOfWork.UserRepository.ExistUser(UpdatedInfo.Id, out var OldInfo))
                {

                    if (((User) HttpContext.Items["user"]!).UserId != UpdatedInfo.Id)
                    {
                        throw new Exception("You are not authorized!");
                    }

                    OldInfo!.Fullname = UpdatedInfo.Fullname ?? OldInfo.Fullname;
                    OldInfo.Email = UpdatedInfo.Email ?? OldInfo.Email;
                    OldInfo.PhoneNumber = UpdatedInfo.Phone ?? OldInfo.PhoneNumber;

                    if (_unitOfWork.RoleRepository.GetById(OldInfo.RoleId)!.RoleName == "customer")
                    {
                        var customerInfo = _unitOfWork.UserRepository.GetCustomerInfo(UpdatedInfo.Id);
                        customerInfo!.Insurance = UpdatedInfo.Insurance ?? customerInfo.Insurance;
                        customerInfo.BirthDate = UpdatedInfo.Birthdate ?? customerInfo.BirthDate;
                    }

                    if (_unitOfWork.RoleRepository.GetById(OldInfo.RoleId)!.RoleName == "dentist" || _unitOfWork.RoleRepository.GetById(OldInfo.RoleId)!.RoleName == "clinic owner")
                    {
                        var staffInfo = OldInfo.ClinicStaffs.First();
                        staffInfo.ClinicId = UpdatedInfo.Clinic ?? staffInfo.ClinicId;
                        staffInfo.IsOwner = UpdatedInfo.IsOwner ?? staffInfo.IsOwner;
                    }

                    _unitOfWork.UserRepository.Update(OldInfo);
                    _unitOfWork.Save();

                    return Ok(new HttpResponseModel() { StatusCode = 200, Message = "User information updated!" });
                };
                throw new DbUpdateException("The user does not exist in the database!");
            }
            catch (Exception ex)
            {
            return Ok(new HttpResponseModel() { StatusCode = 400, Message = "Error occured while trying to update user information!" });//, Detail = ex.Message });
            }
        }

        // ================================================== UNFINISHED ====================================================

        [HttpDelete("{id}")]
        [JwtTokenAuthorization]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                if (_unitOfWork.UserRepository.ExistUser(id, out var OldInfo))
                {
                    var invoker = (User) HttpContext.Items["user"]!;

                    if (invoker.UserId != id && invoker.RoleId != 1)
                    {
                        throw new Exception("You are not authorized!");
                    }

                    _unitOfWork.UserRepository.Delete(OldInfo!);
                    _unitOfWork.Save();

                    return Ok(new HttpResponseModel() { StatusCode = 200, Message = "User information updated!" });
                };
                throw new DbUpdateException("The user does not exist in the database!");
            }
            catch (Exception ex)
            {
                return Ok(new HttpResponseModel() { StatusCode = 400, Message = "Error occured while trying to update user information!", Detail = ex.Message });
            }
        }

        private string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            var rng = new Random();
            while (0 < length--)
            {
                res.Append(valid[rng.Next(valid.Length)]);
            }

            return res.ToString();
        }
    }
}
