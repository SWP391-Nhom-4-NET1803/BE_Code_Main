using Core.Exception;
using Core.HttpModels;
using Core.NewFolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    [JwtTokenAuthorization]
    public class UsersController : ControllerBase
    {
        private readonly DentalClinicPlatformContext _context;
        private readonly IConfiguration _config;
        private readonly UnitOfWork _unitOfWork;

        public UsersController(IConfiguration config, DentalClinicPlatformContext context)
        {
            _context = context;
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
        public ActionResult RequestResetPassword([FromBody] PasswordResetModel target)
        {
            User? user = _unitOfWork.GetUserWithEmail(target.Email);

            if (user == null)
            {
                return BadRequest(new HttpErrorResponse()
                {
                    statusCode = 400,
                    message = "Email này chưa được sử dụng để đăng kí tài khoản trong hệ thống.",
                    errorDetail = $"Địa chỉ email {target.Email} chưa được sử dụng, hãy đăng kí tài khoản và thử lại sau."
                });
            }

            string UserFullname = user.Fullname != null ? user.Fullname : $"người dùng {user.UserId}";

            var emailService = _context.GetService<IEmailService>();

            string newPassword = CreatePassword(24);

            user.Password = newPassword;

            _unitOfWork.UserRepository.Update(user);

            string subject = $"Khôi phục mật khẩu cho tài khoản {user.Username}";

            string body = $"Xin chào, <b>{UserFullname}</b>!<br/>" +
                $"Chúng tôi đã nhận được yêu cầu thay đổi mật khẩu tài khoản của bạn, hãy sử dụng mật khẩu <b>{newPassword}</b> cho lần đăng nhập kế tiếp và thay đổi mật khẩu của bạn.<br/>";


            var configuration = emailService.CreateConfiguration(_config.GetValue<string>("EmailService:Email")!, _config.GetValue<string>("EmailService:Password")!, target.Email, subject, body);

            emailService.SendMailGoogleSmtp(configuration);

            return Ok(new HttpValidResponse() { statusCode = 202, message = "Yêu cầu được chấp thuận." });
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
            User? user = _unitOfWork.GetUserWithEmail(target.Email);

            if (user == null)
            {
                return BadRequest(new HttpErrorResponse() { statusCode = 400, message = "Email này chưa được sử dụng để đăng kí tài khoản trong hệ thống." });
            }

            if (target.PasswordReset == null || target.PasswordReset.Length < 10)
            {
                return BadRequest(new HttpErrorResponse() { statusCode = 400, message = "Mật khẩu mới không hợp lệ" });
            }

            user.Password = target.PasswordReset;
            _unitOfWork.Save();

            // Sending confirmation email to user.
            var emailService = _context.GetService<IEmailService>();

            string UserFullname = user.Fullname != null ? user.Fullname : "Người dùng";

            string emailBody = $"Mật khẩu của bạn đã được thay đổi. nếu bạn không thực hiện thay đổi này, vui lòng bấm vào mục \"quên mật khẩu\" đề thực hiện thay đổi lại mật khẩu.";

            emailService.SendMailGoogleSmtp(target: target.Email, subject: $"Thay đổi mật khẩu cho tài khoản ${user.Username}", body: emailBody);

            return Ok(new HttpErrorResponse() { statusCode = 202, message = "Accepted" });
        }

        /// <summary>
        ///  Get user detailed information based on the valdated token.
        /// </summary>
        /// <returns>A User Info Model thats contains basic user information</returns>
        [HttpGet("info")]
        [JwtTokenAuthorization]
        public ActionResult<UserInfoModel> GetUser()
        {
            var token = Request.Headers.Authorization.ToString().Split(' ').Last();

            var service = _context.GetService<IJwtTokenService>();

            var claims = service.GetPrincipalsFromToken(token);

            var user = _unitOfWork.UserRepository.GetById(int.Parse(claims.Claims.First(claim => claim.Type == "id").Value));

            if (user == null)
            {
                return BadRequest(new HttpErrorResponse() { statusCode = 404, message = "Token không hợp lệ hoặc người dùng không tồn !" });
            }

            UserInfoModel userInfo = new UserInfoModel()
            {
                Id = user.UserId,
                Username = user.Username ?? null,
                Email = user.Email ?? null,
                Fullname = user.Fullname ?? null,
                JoinedDate = user.CreationDate,
                Phone = user.PhoneNumber ?? null,
                Role = claims.Claims.First(claim => claim.Type == ClaimTypes.Role).Value,
                Status = claims.Claims.First(claim => claim.Type == "status").Value,
            };

            return Ok(userInfo);
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCustomer([FromBody] UserRegistrationModel requestObject)
        {
            // Check for user availability before register them in the database.
            if (!_unitOfWork.CheckAvailability(requestObject.Username, requestObject.Email, out var responseMessage))
            {
                return BadRequest(new HttpErrorResponse()
                {
                    statusCode = 400,
                    message = "Không thể thực hiện yêu cầu tạo mới người dùng.",
                    errorDetail = responseMessage
                });
            }
            try
            {
                User newUser = new User()
                {
                    Username = requestObject.Username,
                    Password = requestObject.Password,
                    Email = requestObject.Email,
                    Status = true,
                    RoleId = 3,
                };
                _unitOfWork.UserRepository.Add(newUser);
                _unitOfWork.Save();

                var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

                string body = $"Xin chào người dùng! <br/>" +
                    $"Chúng tôi đã nhận được yêu cầu tạo tài khoản cho email {newUser.Email}, cảm ơn bạn đã đăng kí dịch vụ của chúng tôi. <br/>" +
                    $"Vui lòng xác thực tài khoản thông qua cổng xác thực của chúng tôi tại [Tạo trang xác thực bên phía front-end call tới api xác thực phía backend]";

                if (!await emailService.SendMailGoogleSmtp(requestObject.Email, "Xác nhận yêu cầu tạo tài khoản người dùng", body))
                {
                    throw new Exception("Can't send email to user.");
                }

                return Ok(new HttpValidResponse() { statusCode = 202, message = "Yêu cầu tạo mới người dùng đang được xử lí." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new HttpErrorResponse() { statusCode = 500, message = "Lỗi hệ thống trong lúc xử lí yêu cầu", errorDetail = ex.Message })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    ContentType = "application/json",
                };
            }
        }

        [HttpPut]
        [Route("{id}")]
        [JwtTokenAuthorization]
        public IActionResult PutUser(UserInfoModel UpdatedInfo)
        {
            try
            {
                if (_unitOfWork.UserExists(UpdatedInfo.Id, out var OldInfo))
                {
                    OldInfo!.Fullname = UpdatedInfo.Fullname;
                    OldInfo.Email = UpdatedInfo.Email;
                    OldInfo.PhoneNumber = UpdatedInfo.Phone ?? OldInfo.PhoneNumber;
                    //OldInfo.Role = _unitOfWork.GetRoleByName(UpdatedInfo.Role)?.RoleId ?? OldInfo.Role;
                    //OldInfo.Status = _unitOfWork.GetStatusByName(UpdatedInfo.Status)?.StatusId ?? OldInfo.Status;

                    _unitOfWork.UserRepository.Update(OldInfo);
                    _unitOfWork.Save();

                    return NoContent();
                };
                throw new DbUpdateException("Không tồn tại người dùng trong hệ thống");
            }
            catch (Exception ex)
            {
                return BadRequest(new HttpErrorResponse() { statusCode = 404, message = "Lỗi xảy ra khi cố gắng cập nhật thông tin người dùng.", errorDetail = ex.Message });
            }
        }

        // ================================================== UNFINISHED ====================================================

        [HttpDelete("{id}")]
        [JwtTokenAuthorization(Roles: "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
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
