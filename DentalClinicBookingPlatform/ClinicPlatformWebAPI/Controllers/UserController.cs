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
        private IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPut("update")]
        [Authorize]
        public ActionResult<HttpResponseModel> UpdateUser(int id, [FromBody] UserInfoModel user)
        {
            if (id != user.Id)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Update user info failed",
                    Detail = "User Id does not match!"
                });
            }

            try
            {
                if (!userService.UpdateUserInformation(user, out var message))
                {
                    return BadRequest(new HttpResponseModel()
                    {
                        StatusCode = 400,
                        Message = "Update user info failed",
                        Detail = message,
                    });
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                if (userService.ExistUser(id))
                {
                    return NotFound(new HttpResponseModel()
                    {
                        StatusCode = 404,
                        Message = "Update user info failed",
                        Detail = $"User does not exist for dentistId {id}",
                    });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Update user success",
            });
        }


        [HttpPut("activate/{userId}")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> ActivateUserAccount(int userId, [FromQuery] string token)
        {

            if (!userService.ActivateUser(userId, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Activition failed",
                    Detail = message,
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Activation success",
                Detail = $"Activated user account for user {userId}",
            });
        }

        [HttpPut("inactivate/{userId}")]
        [Authorize]
        public ActionResult<HttpResponseModel> InactivateUserAccount(int userId)
        {

            if (!userService.InactivateUser(userId, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Deactivation failed",
                    Detail = message,
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Deactivation success",
            });
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
