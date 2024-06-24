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
using ClinicPlatformObjects.MiscModels;
using ClinicPlatformWebAPI.Helpers.ModelMapper;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private IUserService userService;

        public CustomerController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet]
        [Authorize(Roles="Customer")]
        public ActionResult<IHttpResponseModel<CustomerInfoViewModel>> GetCustomerInformationint()
        {
            UserInfoModel? customer = (UserInfoModel?) HttpContext.Items["user"];

            if (customer == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "User not found",
                });

            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Content = UserInfoMapper.ToCustomerView(customer)
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> RegisterCustomer([FromBody] UserRegistrationModel userInfo)
        {
            UserInfoModel? user = userService.RegisterAccount(UserInfoMapper.FromRegistration(userInfo), "Customer", out var message);
            if (user == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = message,
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "OK",
                Detail = "User created successfully!",
            });
        }

        [HttpDelete("delete")]
        [Authorize]
        public ActionResult<HttpResponseModel> DeleteUser()
        {
            UserInfoModel? user = (UserInfoModel?) HttpContext.Items["user"]; 

            if (!userService.DeleteUser(user.Id, out var message))
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
