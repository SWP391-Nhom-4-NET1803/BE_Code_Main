using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicPlatformServices.Contracts;
using ClinicPlatformServices;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using ClinicPlatformDTOs.RoleModels;
using ClinicPlatformDTOs.AuthenticationModels;

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

        [HttpGet]
        [Authorize(Roles ="2")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetUsers()
        {
            IEnumerable<UserInfoModel> user = userService.GetUsers();

            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Get info success",
                Detail = $"Total records: {user.Count()}",
                Content = user
            };

            return Ok(ResponseBody);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "3")]
        public ActionResult<IHttpResponseModel<UserInfoModel>> GetUser(int id)
        {
            var user = userService.GetUserInformation(id);

            if (user != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Failed",
                    Content = user
                });
            }
            
            return NotFound(new HttpResponseModel()
            {
                StatusCode = 404,
                Message = "Failed",
                Detail = $"Unknown user with id {id}"
            });
            
        }

        [HttpGet("customer/{userId}")]
        public ActionResult<IHttpResponseModel<CustomerInfoModel>> GetCustomerInformation(int userId)
        {
            CustomerInfoModel? customer = userService.GetCustomerInformationWithUserID(userId);

            if (customer == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = $"User does not exist for id {userId}!"
                });

            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Content = customer
            });
        }

        [HttpGet("staff/{userId}")]
        public ActionResult<IHttpResponseModel<ClinicStaffInfoModel>> GetClinicStaffInformation(int userId)
        {
            ClinicStaffInfoModel? clinicStaff = userService.GetClinicStaffInformationWithUserId(userId);

            if (clinicStaff == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = $"User does not exist for id {userId}!"
                });

            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Content = clinicStaff
            });
        }

        [HttpPost("register/customer")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> RegisterCustomer([FromBody] UserRegistrationModel userInfo)
        {

            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "OK",
                Detail = "User created successfully!",
            };

            if (!userService.RegisterAccount(userInfo, RoleModel.Roles.Customer, out var message))
            {
                ResponseBody.StatusCode = 400;
                ResponseBody.Message = "Register account failed";
                ResponseBody.Detail = message;

                return BadRequest(ResponseBody);
            }

            return Ok(ResponseBody);
        }

        [HttpPost("register/clinic-staff")]
        public ActionResult<HttpResponseModel> RegisterClinicStaff([FromBody] UserRegistrationModel userInfo)
        {

            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "OK",
                Detail = "User created successfully!",
            };

            if (!userService.RegisterAccount(userInfo, RoleModel.Roles.ClinicStaff, out var message))
            {
                ResponseBody.StatusCode = 400;
                ResponseBody.Message = "Update failed";
                ResponseBody.Detail = message;

                return BadRequest(ResponseBody);
            }

            return Ok(ResponseBody);
        }

        [HttpPost("register/clinic-owner")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> RegisterClinicOwner([FromBody] UserRegistrationModel userInfo)
        {
            userInfo.ClinicOwner = true;
            userInfo.Clinic = null;
            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "OK",
                Detail = "User created successfully!",
            };

            if (!userService.RegisterAccount(userInfo, RoleModel.Roles.ClinicStaff, out var message))
            {
                ResponseBody.StatusCode = 400;
                ResponseBody.Message = "Register account failed";
                ResponseBody.Detail = message;

                return BadRequest(ResponseBody);
            }

            return Ok(ResponseBody);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<AuthenticationTokenModel>> Login([FromBody] UserAuthenticationRequestModel loginInfo)
        {
            bool isValidUser = userService.CheckLogin(loginInfo.Username, loginInfo.Password, out var user);

            if (isValidUser)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Login Successfully",
                    Content = authService.GenerateTokens(user!)
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Login Failed",
                    Detail = "Invalid user or username"
                });
            }

        }

        [HttpPost("login-google")]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<AuthenticationTokenModel>> LoginGoogle([FromBody] UserAuthenticationRequestModel loginInfo)
        {
            bool isValidUser = userService.CheckLogin(loginInfo.Username, loginInfo.Password, out var user);

            if (isValidUser)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Login Successfully",
                    Content = authService.GenerateTokens(user!)
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Login Failed",
                    Detail = "Invalid user or username"
                });
            }

        }


        [HttpPut("{id}")]
        public ActionResult<HttpResponseModel> UpdateUser(int id,[FromBody] UserInfoModel user)
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
                if(!userService.UpdateUserInformation(user, out var message))
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
                        Detail = $"User does not exist for id {id}",
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

        [HttpPut("activate/{id}")]
        public ActionResult<HttpResponseModel> ActivateUserAccount(int id)
        {

            if (!userService.ActivateUser(id, out var message))
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
                Detail = $"Activated user account for user {id}",
            });
        }

        [HttpPut("inactivate/{id}")]
        public ActionResult<HttpResponseModel> InactivateUserAccount(int id)
        {

            if (!userService.InactivateUser(id, out var message))
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
            if (!userService.UpdatePasswordForUserWithId(resetInfo.userId, resetInfo.OldPassword, resetInfo.NewPassword, out var message))
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
            if (userService.GetUserInformationWithEmail(email) == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Invalid Request",
                    Detail = "This email is not used to register an account"
                });
            }

            // TODO: Send email.

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Request Approved",
            });
        }

        [HttpDelete("{id}")]
        public ActionResult<HttpResponseModel> DeleteUser(int id)
        {
            var user = userService.GetUserInformation(id);
            if (user == null)
            {
                return NotFound(new HttpResponseModel()
                {
                    StatusCode = 404,
                    Message = "Delete user info failed",
                    Detail = $"User does not exist for id {id}"
                });
            }

            if (!userService.InactivateUser(id, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Delete user failed",
                    Detail = message,
                });
            }


            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Delete user info success",
            });
        }
    }
}
