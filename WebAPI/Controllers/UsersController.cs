using AutoMapper;
using Core.HttpModels;
using Core.HttpModels.ObjectModels.Others;
using Core.HttpModels.ObjectModels.RegistrationModels;
using Core.HttpModels.ObjectModels.UserModel;
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
using Services.UserService;
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
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsersController(IConfiguration config, DentalClinicPlatformContext context, IMapper mapper)
        {
            _unitOfWork = new UnitOfWork(context);
            _mapper = mapper;
        }

        /// <summary>
        ///     <para>Change user password by generating a random password</para>
        /// </summary>
        /// <param name="target">User email (and unused optional password)</param>
        /// <returns> The result (either failed or succeed)</returns>
        [HttpPost]
        [Route("request-reset")]
        //[AllowAnonymous]
        public async  Task<ActionResult> RequestResetPassword([FromBody] PasswordResetModel target)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            // Setting temporary password for the request.
            string newPassword = userService.CreatePassword(8);
            target.PasswordReset = newPassword;

            if (!userService.changePassword(target, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Error while processing request", Detail = message });
            }

            _unitOfWork.Save();

            var targetUser = _unitOfWork.UserRepository.GetUserWithEmail(target.Email)!;

            string emailSubject = $"Khôi phục mật khẩu cho tài khoản {targetUser.Username}";

            string emailBody = 
                $"<p>Xin chào <b>{targetUser.Username}</b>!</p>" +
                $"<p>Chúng tôi đã nhận được yêu cầu khôi phục mật khẩu của bạn</p>" +
                $"<p>hãy sử dụng mật khẩu <b>{newPassword}</b> cho lần đăng nhập kế tiếp và chỉnh sửa mật khẩu của bạn trong phần <a href=\"http://localhost:5173/user/account\">Tài khoản</a>.</p>";

            // Sending new password in the email.
            var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;
            await emailService.SendMailGoogleSmtp(target.Email, emailSubject, emailBody);

            return Ok(new HttpResponseModel() { StatusCode = 202, Message = "Yêu cầu được chấp thuận." });
        }

        /// <summary>
        ///  Thực hiện thay đổi mật khẩu của người dùng dựa trên mật khẩu mới nhập của họ.
        /// </summary>
        /// <param name="target">Email của người dùng cần thay đổi mật khẩu cũng như mật khẩu mới</param>
        /// <returns>Kết quả của việc thay đổi nói trên</returns>
        [HttpPost("reset-password")]
        //[JwtTokenAuthorization]
        public ActionResult ResetPassword([FromBody] PasswordResetModel target)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            if (!userService.changePassword(target, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Error while processing request", Detail = message});
            }

            _unitOfWork.Save();

            var targetUser = _unitOfWork.UserRepository.GetUserWithEmail(target.Email)!;

            string emailSubject = $"Thay đổi mật khẩu cho tài khoản {targetUser.Username}";

            string emailBody = 
                $"<p>Xin chào {targetUser.Username}!</p> " +
                $"<p>Mật khẩu đăng nhập của bạn đã được thay đổi.</p>" +
                $"<p>Trong trường hợp bạn không phải là người thực hiện việc thay đổi mật khẩu, hãy đổi lại mật khẩu ở mục <b>Quên mật khẩu</b> tại trang đăng nhập.</p>";

            // Sending confirmation email to user.
            var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;
            emailService.SendMailGoogleSmtp(target: target.Email, subject: emailSubject, body: emailBody);

            return Ok(new HttpResponseModel() { StatusCode = 202, Message = "Accepted" });
        }

        /// <summary>
        ///  Get user detailed information based on the valdated token.
        /// </summary>
        /// <returns>A User Info Model thats contains basic user information</returns>
        [HttpGet("info-customer")]
        //[JwtTokenAuthorization]
        public ActionResult<CustomerInfoModel> GetCustomer()    
        {
            var user = (User) HttpContext.Items["user"]!;

            if (user == null)
            {
                return Ok( new HttpResponseModel()
                {
                    StatusCode = 404,
                    Message = "NotFound"
                });
            }

            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            var customer = userService.getCustomerInfoById(user.UserId);
            var customerInfo = _mapper.Map<Customer,CustomerInfoModel>(customer);


            HttpResponseModel response = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Succeed",
                Content = customerInfo
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("register")]
        //[AllowAnonymous]
        public async Task<IActionResult> RegisterCustomer([FromBody] UserRegistrationModel requestObject)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            if (!userService.createCustomer(requestObject, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 202, Message = "Can not create customer account", Detail = message });
            }

            _unitOfWork.Save();

            var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

            string emailSubject = "Xác nhận yêu cầu tạo tài khoản người dùng";

            string emailBody = 
                $"<p>Xin chào người dùng {requestObject.Username}! </p>" +
                $"Chúng tôi đã nhận được yêu cầu tạo tài khoản cho email {requestObject.Email}, cảm ơn bạn đã đăng kí dịch vụ của chúng tôi. </p>" +
                $"<p>Vui lòng xác thực tài khoản thông qua cổng xác thực của chúng tôi tại <a href=\"http://localhost:5173/user/auth\"></a></p>";

            await emailService.SendMailGoogleSmtp(requestObject.Email!, subject: emailSubject, body: emailBody);

            return Ok(new HttpResponseModel() { StatusCode = 202, Message = "Yêu cầu tạo mới người dùng đang được xử lí." });
        }

        [HttpPut]
        [Route("update")]
        //[JwtTokenAuthorization]
        public IActionResult PutUser(UserInfoModel UpdatedInfo)
        {

            var user = (User) HttpContext.Items["user"]!;

            if (user.UserId != UpdatedInfo.Id && ! (user.RoleId==1) )
            {
                return Unauthorized( new HttpResponseModel() { StatusCode=401,  Message="Unauthorized access!" });
            }

            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            if (!userService.updateUserInformation(UpdatedInfo, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Error occured!", Detail = message });
            }

            _unitOfWork.Save();

             return Ok(new HttpResponseModel() { StatusCode = 200, Message = "User information updated!" });
        }

        // ================================================== UNFINISHED ====================================================

        [HttpDelete("{id}")]
        //[JwtTokenAuthorization]
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
    }
}
