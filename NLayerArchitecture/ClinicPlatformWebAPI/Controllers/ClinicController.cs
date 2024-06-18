using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.ClinicModels.Registration;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/clinic")]
    [ApiController]
    public class ClinicController : ControllerBase
    {
        private readonly IClinicService clinicService;
        private readonly IUserService userService;

        public ClinicController(IClinicService clinicService, IUserService userService)
        {
            this.clinicService = clinicService;
            this.userService = userService;
        }

        [HttpPost("register")]
        public ActionResult<HttpResponseModel> RegisterClinic([FromBody] ClinicRegistrationModel info)
        {
            UserInfoModel? userInfo = userService.GetUserInformation((int) info.OwnerId!);

            if (userInfo == null || userInfo.IsOwner == false)
            {
                {
                    return BadRequest(new HttpResponseModel()
                    {
                        StatusCode = 400,
                        Message = "Failed to create new clinic",
                        Detail = $"Error while getting user information: {(userInfo != null ? "User is not a clinic owner" : $"No user with {info.OwnerId} exis.")}"
                    });
                }
            }

            if (!clinicService.RegisterClinic(info, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed to create new clinic",
                    Detail = message
                });
            }

            ClinicInfoModel registeredClinic = clinicService.GetClinicWithOwnerId((int)info.OwnerId!)!;

            // TODO: Set Clinic Schedule Slot.

            userInfo.ClinicId = registeredClinic.Id;

            userService.UpdateUserInformation(userInfo, out _);
           
            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Detail = "Succesfully created new clinic"
            });
        }

    }
}
