using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicPlatformServices.Contracts;
using ClinicPlatformServices;
using AutoMapper;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService userService;


        public UserController(IMapper mapper)
        {
            userService = new UserService(mapper);
        }

        [HttpGet]
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
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<UserInfoModel>> GetUser(int id)
        {
            var user = userService.GetUserInformation(id);

            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Get user info sucess",
                Content = user
            };

            if (user == null)
            {
                ResponseBody.StatusCode = 404;
                ResponseBody.Message = "Get user info failed";
                ResponseBody.Detail = $"Unknown user with id {id}";
                return NotFound(ResponseBody);
            }

            return Ok(ResponseBody);
        }

        [HttpPut("{id}")]
        public ActionResult<HttpResponseModel> PutUser(int id,[FromBody] UserInfoModel user)
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

            if (!userService.RegisterCustomerAccount(userInfo, out var message))
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

            if (!userService.RegisterClinicStaffAccount(userInfo, out var message))
            {
                ResponseBody.StatusCode = 400;
                ResponseBody.Message = "Update failed";
                ResponseBody.Detail = message;

                return BadRequest(ResponseBody);
            }

            return Ok(ResponseBody) ;
        }

        [HttpPost("register/clinic-owner")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> RegisterClinicOwner([FromBody] UserRegistrationModel userInfo)
        {
            userInfo.ClinicOwner = true;
            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "OK",
                Detail = "User created successfully!",
            };

            if (!userService.RegisterClinicStaffAccount(userInfo, out var message))
            {
                ResponseBody.StatusCode = 400;
                ResponseBody.Message = "Update failed";
                ResponseBody.Detail = message;

                return BadRequest(ResponseBody);
            }

            return Ok(ResponseBody);
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
