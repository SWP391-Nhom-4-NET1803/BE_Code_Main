using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/clinic")]
    [ApiController]
    public class ClinicController : ControllerBase
    {
        private readonly IClinicService clinicService;
        private readonly IUserService userService;
        private readonly IScheduleService scheduleService;

        public ClinicController(IClinicService clinicService, IUserService userService, IScheduleService scheduleService)
        {
            this.clinicService = clinicService;
            this.userService = userService;
            this.scheduleService = scheduleService;
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

        [HttpGet("search")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> SearchClinic([FromQuery] string? name=null, [FromQuery] TimeOnly? open=null, [FromQuery] TimeOnly? close=null , string? service = null, int page_size=10, int page = 1)
        {
            IEnumerable<ClinicInfoModel> result = clinicService.GetAll();;

            if (service != null)
            {
                int? real = clinicService.GetClinicServiceWithName(service)?.ServiceId;

                if (real != null)
                {
                    Console.WriteLine("real");
                    result = clinicService.GetClinicHasService((int) real);
                }
            }

            if (name != null)
            {
                result = result
                    .Where(x => !x.Name.IsNullOrEmpty() && x.Name!.StartsWith(name, true, CultureInfo.InvariantCulture));
            }

            if (open != null)
            {
                result = result
                    .Where(x => x.OpenHour >= open);
            }

            if (close != null)
            {
                result = result
                    .Where(x => x.CloseHour <= close);
            }

            return Ok(new HttpResponseModel()
            {
                Message = "Success",
                Content = result.Skip(Math.Abs(page - 1) * page_size).Take(page_size)
            });
        }

        [HttpPost("register")]
        public ActionResult<HttpResponseModel> RegisterClinic([FromBody] ClinicRegistrationModel info)
        {
            UserInfoModel? userInfo = userService.GetUserInformation((int) info.OwnerId!);

            if (userInfo == null || userInfo.IsOwner == false)
            {
                string errorReason = "Error while getting user information: ";

                errorReason += (userInfo != null ? $"No user with {info.OwnerId} exis. " : "");
                errorReason += (!userInfo?.IsOwner ?? false ? "User is not a clinic owner" : "");

                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed to create new clinic",
                    Detail = $"{errorReason}"
                });
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
            
            if (!clinicService.DeleteClinic(id)) // Please consider using status or flags instead.
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
                    Detail = $"{message}",
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

        [HttpGet("{id}/schedule")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicSlotInfoModel>>> GetClinicSlot(int id)
        {
            if (clinicService.GetClinicWithId(id) != null)
            {
                return Ok( new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Success",
                    Content = scheduleService.GetAllClinicSlot(id)
                });
            }

            return BadRequest(new HttpResponseModel()
            {
                StatusCode = 400,
                Message = "Failed getting clinic info",
                Detail = $"There are no clinic with Id {id} exist."
            });
        }

        [HttpPost("schedule")]
        public ActionResult<HttpResponseModel> AddClinicSlot([FromBody] ClinicSlotRegistrationModel slotInfo)
        {
            if (clinicService.GetClinicWithId((int)slotInfo.ClinicId) == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed adding clinic slot.",
                    Detail = $"Can not find clinic information for id {slotInfo.ClinicId}"
                });
            }

            if (scheduleService.RegisterClinicSlot(slotInfo, out var message))
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Success",
                    Detail = message,
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed adding clinic slot.",
                    Detail = message
                });
            }
        }

        [HttpPut("schedule")]
        public ActionResult<HttpResponseModel> UpdateClinicSlot([FromBody] ClinicSlotInfoModel slotInfo)
        {
            if (scheduleService.UpdateClinicSlot(slotInfo, out var message))
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Success",
                    Detail = message,
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed updating slot info.",
                    Detail = message
                });
            }
        }

        [HttpDelete("schedule")]
        public ActionResult<HttpResponseModel> DeleteClinicSlot(Guid slotId)
        {
            if (scheduleService.DeleteClinicSlot(slotId))
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Success"
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Slot deletion failed",
                });
            }
        }

    }
}
