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

        [HttpGet("{id}")]
        public ActionResult<IHttpResponseModel<ClinicInfoModel>> GetClinicInformation(int id)
        {
            ClinicInfoModel? clinicInfo = clinicService.GetClinicWithId(id);

            if (clinicInfo == null)
            {
                return NotFound(new HttpResponseModel()
                {
                    StatusCode = 404,
                    Message = $"Notfound",
                    Detail = $"Clinic not found for Id {id}",
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Clinic found",
                Content = clinicInfo
            });
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

            userInfo.ClinicId = registeredClinic.Id;

            if (!userService.UpdateUserInformation(userInfo, out message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed to update user info",
                    Detail = message
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Detail = "Succesfully created new clinic",
                Content = registeredClinic
            });
        }

        [HttpPut("{id}")]
        public ActionResult<HttpResponseModel> UpdateClinicInformation(int id, [FromBody] ClinicInfoModel clinicInfo)
        {
            if (id != clinicInfo.Id)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Bad Request",
                    Detail = "clinic id provided is different from the updated information",
                });
            }

            if (!clinicService.UpdateClinicInformation(clinicInfo, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Bad Request",
                    Detail = message,
                });
            }
            else
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Updated sucessfully",
                    Detail = $"Information updated for clinic {clinicInfo.Id}",
                    Content = clinicService.GetClinicWithId(clinicInfo.Id)
                });
            }
        }

        [HttpPost("service/add")]
        public ActionResult<HttpResponseModel> AddClinicService([FromBody] ClinicServiceInfoModel serviceInfo)
        {
            if (!clinicService.AddClinicService(serviceInfo, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Bad Request",
                    Detail = message,
                });
            }
            else
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Service added sucessfully",
                    Detail = $"Information updated for clinic {serviceInfo.ClinicId}",
                });
            }
        }

        [HttpDelete("deactivate")]
        public ActionResult<HttpResponseModel> InactivateClinic()
        {
            try

            return Ok(new HttpResponseModel()
            {

            })
        }



        [HttpDelete("service/deactivate")]
        public ActionResult<HttpResponseModel> InactivateService(Guid clinicServiceId)
        {
            if (clinicService.DeleteClinicServices(clinicServiceId, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed to remove clinic service",
                    Detail = message
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Service removed",
            });
        }
    }
}
