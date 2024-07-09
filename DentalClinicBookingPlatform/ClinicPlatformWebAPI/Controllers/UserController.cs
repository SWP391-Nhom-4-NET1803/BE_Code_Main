using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.MiscModels;
using ClinicPlatformObjects.TokenModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.ModelMapper;
using ClinicPlatformWebAPI.Helpers.Models;
using ClinicPlatformWebAPI.Services.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ITokenService tokenService;
        string PasswordResetPage;

        public UserController(IConfiguration configuration, IUserService userService, ITokenService tokenService)
        {
            this.userService = userService;
            this.tokenService = tokenService;
            this.PasswordResetPage = configuration.GetValue<string>("Frontend:PasswordResetPage")!;
        }

        [HttpPut("password/change")]
        [Authorize]
        public ActionResult<HttpResponseModel> UpdateUserPassword([FromBody] PasswordResetModel resetInfo)
        {
            if (!userService.UpdatePasswordForUserWithId((int)resetInfo.Id!, resetInfo.NewPassword, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = "Update sucessfully",
            });

        }

        [HttpPost("password/reset")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> ResetPassword([FromBody] PasswordResetModel resetInfo)
        {
            TokenInfoModel? tokenInfo = tokenService.MatchTokenValue(resetInfo.TokenValue, out var message);

            if (tokenInfo == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message,
                });
            }

            UserInfoModel user = userService.GetUserWithUserId(tokenInfo.UserId)!;

            

            if (!userService.UpdatePasswordForUserWithId(user.Id, resetInfo.NewPassword, out message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message,
                });
            }

            tokenService.MarkTokenAsUsed(tokenInfo.Id);

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = message,
            });
        }

        [HttpPost("password/request")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> RequestResetPassword([FromQuery] string email)
        {
            UserInfoModel? user = userService.GetUserWithEmail(email);

            if (user == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "This email is not used to register an account"
                });
            }

            var token = tokenService.CreateUserPasswordResetToken(user.Id, out var message)!;

            var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

            emailService.SendMailGoogleSmtp(email, "User Password Reset Request", $"Your password reset token is {token.Value}. Use it at {PasswordResetPage} to reset your password!");

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = message,
            });
        }
    }
}
