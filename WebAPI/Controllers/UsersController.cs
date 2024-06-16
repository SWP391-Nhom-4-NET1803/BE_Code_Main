using AutoMapper;
using Core.HttpModels;
using Core.HttpModels.ObjectModels.Others;
using Core.HttpModels.ObjectModels.RegistrationModels;
using Core.HttpModels.ObjectModels.RoleModels;
using Core.HttpModels.ObjectModels.UserModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Repositories;
using Repositories.Models;
using Services.UserService;
using WebAPI.Helper.AuthorizationPolicy;

namespace WebAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsersController(IConfiguration config, DentalClinicPlatformContext context, IMapper mapper)
        {
            _unitOfWork = new UnitOfWork(context);
            _mapper = mapper;
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestObject"></param>
        /// <returns></returns>
        [HttpPost("register")]
        //[AllowAnonymous]
        public async Task<IActionResult> RegisterCustomer([FromBody] UserRegistrationModel requestObject)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            if (!userService.CreateCustomer(requestObject, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Failed", Detail = message });
            }

            _unitOfWork.Save();

            /*var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

            string emailSubject = "Xác nhận yêu cầu tạo tài khoản người dùng";

            string emailBody = 
                $"<p>Xin chào người dùng {requestObject.Username}! </p>" +
                $"Chúng tôi đã nhận được yêu cầu tạo tài khoản cho email {requestObject.Email}, cảm ơn bạn đã đăng kí dịch vụ của chúng tôi. </p>" +
                $"<p>Vui lòng xác thực tài khoản thông qua cổng xác thực của chúng tôi tại <a href=\"http://localhost:5173/user/auth\"></a></p>";

            await emailService.SendMailGoogleSmtp(requestObject.Email!, subject: emailSubject, body: emailBody);*/

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Success" });
        }

        /// <summary>
        ///     <para>Change user password by generating a random password</para>
        /// </summary>
        /// <param name="target">User email (and unused optional password)</param>
        /// <returns> The result (either failed or succeed)</returns>
        [HttpPost("password-reset-request")]
        //[AllowAnonymous]
        public ActionResult<IHttpResponseModel<object>> RequestResetPassword([FromBody] PasswordResetModel target)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            // Setting temporary password for the request.
            string newPassword = userService.CreatePassword(8);
            target.PasswordReset = newPassword;

            if (!userService.ChangePassword(target, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Error while processing request", Detail = message });
            }

            _unitOfWork.Save();

            var targetUser = _unitOfWork.UserRepository.GetUserWithEmail(target.Email)!;

            /*string emailSubject = $"Khôi phục mật khẩu cho tài khoản {targetUser.Username}";

            string emailBody = 
                $"<p>Xin chào <b>{targetUser.Username}</b>!</p>" +
                $"<p>Chúng tôi đã nhận được yêu cầu khôi phục mật khẩu của bạn</p>" +
                $"<p>hãy sử dụng mật khẩu <b>{newPassword}</b> cho lần đăng nhập kế tiếp và chỉnh sửa mật khẩu của bạn trong phần <a href=\"http://localhost:5173/user/account\">Tài khoản</a>.</p>";

            // Sending new password in the email.
            var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;
            await emailService.SendMailGoogleSmtp(target.Email, emailSubject, emailBody);*/

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Success" });
        }



        // ================================== Tested and ready to deploy ==============================================

        /// <summary>
        ///  Change user password based on their new input.
        /// </summary>
        /// <param name="target">Email của người dùng cần thay đổi mật khẩu cũng như mật khẩu mới</param>
        /// <returns>Kết quả của việc thay đổi nói trên</returns>
        [HttpPut("password-reset")]
        //[JwtTokenAuthorization]
        public ActionResult ResetPassword([FromBody] PasswordResetModel target)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            if (!userService.ChangePassword(target, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Failed", Detail = message });
            }

            _unitOfWork.Save();

            /*var targetUser = _unitOfWork.UserRepository.GetUserWithEmail(target.Email)!;

            string emailSubject = $"Thay đổi mật khẩu cho tài khoản {targetUser.Username}";

            string emailBody = 
                $"<p>Xin chào {targetUser.Username}!</p> " +
                $"<p>Mật khẩu đăng nhập của bạn đã được thay đổi.</p>" +
                $"<p>Trong trường hợp bạn không phải là người thực hiện việc thay đổi mật khẩu, hãy đổi lại mật khẩu ở mục <b>Quên mật khẩu</b> tại trang đăng nhập.</p>";

            // Sending confirmation email to user.
            var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;
            emailService.SendMailGoogleSmtp(target: target.Email, subject: emailSubject, body: emailBody);*/

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Accepted" });
        }

        [HttpGet("{id}")]
        //[JwtTokenAuthorization]
        public ActionResult<CustomerInfoModel> GetUserInfo(int id)
        {
            var user = (User)HttpContext.Items["user"]!;

            if (user == null)
            {
                return Unauthorized(new HttpResponseModel()
                {
                    StatusCode = 401,
                    Message = "Unauthorized"
                });
            }
            else
            {
                var userService = HttpContext.RequestServices.GetService<IUserService>()!;

                var userInfo = userService.GetUserInfo(user.UserId)!;

                var mappedUserInfo = _mapper.Map<User, UserInfoModel>(userInfo);

                HttpResponseModel response = new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Success",
                    Content = mappedUserInfo
                };

                return Ok(response);
            }


        }

        [HttpPut]
        //[JwtTokenAuthorization]
        public IActionResult PutUser(UserInfoModel UpdatedInfo)
        {

            var user = (User)HttpContext.Items["user"]!;

            // Validating if the request invoker is an admin or the one who have authority over the account
            if (user.UserId != UpdatedInfo.Id && !(user.RoleId == 1))
            {
                return Unauthorized(new HttpResponseModel() { StatusCode = 401, Message = "Unauthorized", Detail = "User is unauthorized!" });
            }

            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            if (!userService.UpdateUserInformation(UpdatedInfo, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Failed", Detail = message });
            }

            _unitOfWork.Save();

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Success", Detail = "User information updateds" });
        }

        [HttpPut("inactivate/{id}")]
        //[JwtTokenAuthorization]
        public IActionResult UnactivateUser(int id)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            if (!userService.InactivateUser(id, out var message))
            {
                return BadRequest(new HttpResponseModel() {StatusCode = 400, Message = "Failed", Detail = message });
            }
            else
            {
                return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Success" });
            }
        }

        /// <summary>
        ///  Get user detailed information based on the validated token.
        ///     
        ///  Required user to logged (as customer role) in if 
        /// </summary>
        /// <returns>A User Info Model thats contains basic user information</returns>
        [HttpGet("customer")]
        [JwtTokenAuthorization(RoleModel.Roles.Customer)]
        public ActionResult<CustomerInfoModel> GetCustomer(int id)
        {
            var user = (User)HttpContext.Items["user"]!;

            if (user == null)
            {
                return Unauthorized(new HttpResponseModel()
                {
                    StatusCode = 401,
                    Message = "Unauthorized"
                });
            }
            else
            {
                var userService = HttpContext.RequestServices.GetService<IUserService>()!;

                var customer = userService.GetCustomerInfoById(user.UserId)!;

                var customerInfo = _mapper.Map<Customer, CustomerInfoModel>(customer);

                HttpResponseModel response = new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Success",
                    Content = customerInfo
                };

                return Ok(response);
            }
        }

        [HttpDelete("{id}")]
        //[JwtTokenAuthorization]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                if (_unitOfWork.UserRepository.ExistUser(id, out var OldInfo))
                {

                    var invoker = (User)HttpContext.Items["user"]!;

                    if (invoker.UserId != id && invoker.RoleId != 1)
                    {
                        throw new Exception("You are not authorized!");
                    }

                    HttpContext.RequestServices.GetService<IUserService>()!.RemoveUser(id, out var message);
                    _unitOfWork.Save();

                    return Ok(new HttpResponseModel() { StatusCode = 200, Message = "User information removed!" });
                };
                throw new DbUpdateException("The user does not exist in the database!");
            }
            catch (Exception ex)
            {
                return Ok(new HttpResponseModel() { StatusCode = 400, Message = "Error occured while trying to update user information!", Detail = ex.Message });
            }
        }

        // ==================================== Finished and untested  =================================================

        // ===========================================  Unfinished =====================================================
    }
}
