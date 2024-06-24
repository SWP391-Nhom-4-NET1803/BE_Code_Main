using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.ModelMapper;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IClinicServiceService clinicServiceService;
        private readonly IUserService userService;
        private readonly IScheduleService scheduleService;

        public ClinicController(IClinicService clinicService, IUserService userService, IScheduleService scheduleService, IClinicServiceService clinicServiceService)
        {
            this.clinicService = clinicService;
            this.userService = userService;
            this.scheduleService = scheduleService;
            this.clinicServiceService = clinicServiceService;
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

            UserInfoModel? user = userService.RegisterAccount(UserInfoMapper.FromRegistration(userInfo), "Dentist", out var message);

            if (user == null)
            {
                ResponseBody.StatusCode = 400;
                ResponseBody.Message = "Register account failed";
                ResponseBody.Detail = message;

                return BadRequest(ResponseBody);
            }

            return Ok(ResponseBody);
        }

        [HttpPost("staff/register")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> RegisterClinicStaff([FromBody] UserRegistrationModel userInfo)
        {

            UserInfoModel? user = userService.RegisterAccount(UserInfoMapper.FromRegistration(userInfo), "Dentist", out var message);

            if (user==null)
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

        [HttpGet("staff/{dentistId}")]
        public ActionResult<IHttpResponseModel<DentistInfoViewModel>> GetDentistInformation(int dentistId)
        {
            UserInfoModel? dentist = userService.GetDentistWithDentistId(dentistId);

            if (dentist == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "User not found",
                    Detail = $"User does not exist for dentistId {dentistId}!"
                });

            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Content = UserInfoMapper.ToDentistView(dentist)
            });
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
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> SearchClinic([FromQuery] string? name=null, [FromQuery] TimeOnly? open=null, [FromQuery] TimeOnly? close=null, int page_size=100, int page = 1)
        {
            IEnumerable<ClinicInfoModel> result;

            result = clinicService.GetAllClinic(page_size, page-1);

            if (name != null)
            {
                result = result.Where(x => !x.Name.IsNullOrEmpty() && x.Name!.Contains(name, StringComparison.OrdinalIgnoreCase));
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
                StatusCode = 200,
                Message = "Success",
                Content = result
            });
        }

        [HttpPost("register")]
        public ActionResult<HttpResponseModel> RegisterClinic([FromBody] ClinicRegistrationModel info)
        {

            UserInfoModel? userInfo = new UserInfoModel()
            {
                Username = info.OwnerUserName,
                PasswordHash = info.OwnerPassword,
                Fullname = info.OwnerFullName,
                Email = info.OwnerEmail,
                IsOwner = true
            };
                
            userInfo = userService.RegisterAccount(userInfo, "Dentist", out var message);

            if (userInfo == null || userInfo.IsOwner == false)
            {
                string errorReason = "Error while getting user information: ";

                errorReason += (userInfo == null ? message : "");
                errorReason += (!userInfo?.IsOwner ?? false  ? "User is not a clinic owner" : "");

                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed to create new clinic",
                    Detail = $"{errorReason}"
                });
            }

            ClinicInfoModel? clinic = ClinicMapper.MapToClinicInfo(info);

            clinic.OwnerId = userInfo.Id;

            clinic = clinicService.RegisterClinic(clinic, out message);

            if (clinic == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed to create new clinic",
                    Detail = message
                });
            }

            userInfo.ClinicId = clinic.Id;
            UserInfoModel user = userService.UpdateUserInformation(userInfo, out message);

            if (user == null)
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
                Content = clinic
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
        [Authorize(Roles = "Dentist")]
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
            ClinicServiceInfoModel? service = clinicServiceService.AddClinicService(serviceInfo, out var message);

            if (service == null)
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
            if (!clinicServiceService.AddClinicServices(serviceInfo, out var message))
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

        [HttpDelete("service/delete")]
        public ActionResult<HttpResponseModel> RemoveService(Guid clinicServiceId)
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

        [HttpGet("{id}/slots")]
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

        [HttpPost("slot")]
        [Authorize(Roles = "Dentist")]
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

            ClinicSlotInfoModel? slot = scheduleService.AddNewClinicSlot(ClinicMapper.MapToSlotInfo(slotInfo), out var message);

            if (slot != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Success",
                    Detail = message,
                    Content = slot
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

        [HttpPut("slot/update")]
        public ActionResult<HttpResponseModel> UpdateClinicSlot([FromBody] ClinicSlotInfoModel slotInfo)
        {
            ClinicSlotInfoModel? slot = scheduleService.UpdateClinicSlot(slotInfo, out var message);

            if (slot != null)
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
