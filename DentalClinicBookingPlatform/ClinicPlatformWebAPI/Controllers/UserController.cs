using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.MiscModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.ModelMapper;
using ClinicPlatformWebAPI.Helpers.Models;
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
        private readonly IAuthService authService;

        public UserController(IUserService userService, IAuthService authService)
        {
            this.userService = userService;
            this.authService = authService;
        }

        [HttpPut("password/change")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> UpdateUserPassword([FromBody] PasswordResetModel resetInfo)
        {
            if (!userService.UpdatePasswordForUserWithId(resetInfo.Id, resetInfo.NewPassword, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Update failed",
                    Detail = message
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Update sucessfully",
            });

        }

        [HttpPost("password/reset")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> ResetPassword([FromQuery] string email)
        {
            UserInfoModel? user = userService.GetUserWithEmail(email);

            if (user == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Invalid Request",
                    Detail = "This email is not used to register an account"
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Request Approved",
            });
        }
    }
}
