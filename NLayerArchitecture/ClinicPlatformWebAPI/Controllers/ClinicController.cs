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

        [HttpPut("activate/{id}")]
        public ActionResult<HttpResponseModel> ActivateClinic(int id)
        {

            if (!clinicService.ActivateClinic(id, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Clinic activision failed",
                    Detail = message
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Detail = $"Clinic {id} has been activated."
            });
        }

        [HttpPut("deactivate/{id}")]
        public ActionResult<HttpResponseModel> InactivateClinic(int id)
        {

            if (!clinicService.InactivateClinic(id, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Clinic deactivation failed",
                    Detail = message
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Detail = $"Clinic {id} has been deactivated."
            });
        }

        [HttpDelete("{id}")]
        public ActionResult<HttpResponseModel> RemoveClinic(int id)
        {

            if (!clinicService.DeleteClinic(id))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Clinic removal failed",
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Detail = $"Clinic {id} has been removed."
            });
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

        [HttpPost("service/add-batch")]
        public ActionResult<HttpResponseModel> AddClinicServices([FromBody] IEnumerable<ClinicServiceInfoModel> serviceInfo)
        {
            if (!clinicService.AddClinicServices(serviceInfo, out var message))
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
                    Detail = message,
                });
            }
        }

        [HttpPut("service/update")]
        public ActionResult<HttpResponseModel> UpdateClinicService([FromBody] ClinicServiceInfoModel serviceInfo)
        {
            if (!clinicService.UpdateClinicService(serviceInfo, out var message))
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed while updating services",
                    Detail = message,
                });
            }
            else
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Service added sucessfully",
                    Detail = $"Updated service information!",
                });
            }
        }

        [HttpPut("service/update-batch")]
        public ActionResult<HttpResponseModel> UpdateClinicServices([FromBody] IEnumerable<ClinicServiceInfoModel> serviceInfo)
        {
            if (!clinicService.UpdateClinicServices(serviceInfo, out var message))
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
                    Detail = $"Updated information for {serviceInfo.Count()} services!",
                });
            }
        }

        [HttpDelete("service/delete")]
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
