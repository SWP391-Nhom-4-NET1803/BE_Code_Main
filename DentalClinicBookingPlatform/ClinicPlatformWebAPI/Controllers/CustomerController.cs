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
using ClinicPlatformObjects.UserModels.CustomerModel;

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
            UserInfoModel customer = (UserInfoModel?) HttpContext.Items["user"]!;

            if (customer == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "User not found",
                });

            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = false,
                Message = "Success",
                Content = UserInfoMapper.ToCustomerView(customer)
            });
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> RegisterCustomer([FromBody] CustomerRegistrationModel userInfo)
        {
            UserInfoModel? user = UserInfoMapper.FromRegistration(userInfo);
            user.IsActive = true;

            user = userService.RegisterAccount(user, "Customer", out var message);
            if (user == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message,
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 201,
                Success = true,
                Message = "User created successfully!",
            });
        }

        [HttpPut]
        [Authorize(Roles = "Customer")]
        public ActionResult<IHttpResponseModel<CustomerInfoViewModel>> UpdateCustomerIndo([FromBody] CustomerUpdateModel updatedInfo)
        {
            UserInfoModel? customer = (UserInfoModel?)HttpContext.Items["user"]!;

            customer.Insurance = updatedInfo.Insurance ?? customer.Insurance;
            customer.Email = updatedInfo.Email ?? customer.Email;
            customer.Fullname = updatedInfo.Fullname ?? customer.Fullname;
            customer.Phone = updatedInfo.Phone ?? customer.Phone;
            customer.Birthdate = updatedInfo.Birthdate ?? customer.Birthdate;
            customer.Sex = updatedInfo.Sex ?? customer.Sex;

            var result = userService.UpdateUserInformation(customer, out var message);

            if (result.userInfo == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = result.statusCode,
                    Success = false,
                    Message = message
                });
            }
            else
            {
                return Ok(new HttpResponseModel
                {
                    StatusCode = result.statusCode,
                    Success = true,
                    Message = "Updated information for user",
                    Content = UserInfoMapper.ToCustomerView(result.userInfo)
                });
            }
        }

        [HttpPut("activate")]
        [AllowAnonymous]
        public ActionResult<HttpResponseModel> ActivateUserAccount([FromQuery] int userId)
        {

            if (!userService.ActivateUser(userId, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message,
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = $"Activated user account for user {userId}",
            });
        }

        [HttpPut("deactivate")]
        [Authorize(Roles = "Customer,Admin")]
        public ActionResult<HttpResponseModel> InactivateUserAccount()
        {
            UserInfoModel user = (UserInfoModel)HttpContext.Items["user"]!;

            if (!userService.InactivateUser(user.Id, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = true,
                    Message = message,
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = "Deactivation success",
            });
        }

        [HttpDelete]
        [Authorize(Roles = "Customer")]
        public ActionResult<HttpResponseModel> DeleteUser()
        {
            UserInfoModel user = (UserInfoModel) HttpContext.Items["user"]!; 

            if (!userService.DeleteUser(user.Id, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = true,
                    Message = message,
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = "Delete user info success",
            });
        }
    }
}
