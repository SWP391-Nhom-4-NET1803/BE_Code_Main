using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.ClinicModels;
using ClinicPlatformObjects.ServiceModels;
using ClinicPlatformObjects.UserModels.CustomerModel;
using ClinicPlatformObjects.UserModels.DentistModel;
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

        [HttpGet("staff")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<IHttpResponseModel<DentistInfoViewModel>> GetDentistInformation()
        {
            UserInfoModel invoker = (UserInfoModel)HttpContext.Items["user"]!;

            IEnumerable<DentistInfoViewModel> dentistList = from user in userService.GetUsers().Where( x=> x.ClinicId == invoker.ClinicId) select UserInfoMapper.ToDentistView(user);

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Content = dentistList
            });
        }

        [HttpGet("staff/{dentistId}")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<IHttpResponseModel<DentistInfoViewModel>> GetDentistInformation(int dentistId)
        {
            UserInfoModel invoker = (UserInfoModel)HttpContext.Items["user"]!;

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

            if (invoker.ClinicId != dentist.ClinicId || !invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel()
                {
                    StatusCode = 403,
                    Message = "Unauthorized",
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

            if (clinicInfo == null || clinicInfo.Status == "removed")
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

            result = result.Where(x => x.Status != "removed");

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
        [AllowAnonymous]
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
            UserInfoModel? user = userService.UpdateUserInformation(userInfo, out message);

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
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> UpdateClinicInformation(int id, [FromBody] ClinicInfoUpdateModel clinicUpdateInfo)
        {
            UserInfoModel invoker = (UserInfoModel) HttpContext.Items["user"]!;

            if (!invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Message = "Unauthorized",
                    Detail = "This is clinic owner available only feature!"
                });
            }

            if (invoker.ClinicId != clinicUpdateInfo.Id)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Message = "Unauthorized",
                    Detail = "You can only change your clinic information!"
                });
            }

            ClinicInfoModel? clinicInfo = clinicService.GetClinicWithId(id);

            if (clinicInfo == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Bad Request",
                    Detail = "No clinic were found with provided Id",
                });
            }

            if (id != clinicInfo.Id)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Bad Request",
                    Detail = "clinic id provided is different from the updated information",
                });
            }

            var Invoker = (UserInfoModel)HttpContext.Items["user"]!;

            clinicInfo.Name = clinicUpdateInfo.Name;
            clinicInfo.Description = clinicUpdateInfo.Description;
            clinicInfo.Address = clinicUpdateInfo.Address;
            clinicInfo.Email = clinicUpdateInfo.Email;
            clinicInfo.Phone = clinicUpdateInfo.Phone;
            clinicInfo.OpenHour = clinicUpdateInfo.OpenHour;
            clinicInfo.CloseHour = clinicUpdateInfo.CloseHour;

            clinicInfo = clinicService.UpdateClinicInformation(clinicInfo, out var message);
                
            if (clinicInfo == null)
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
                    Content = clinicInfo
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
        [Authorize(Roles = "Dentist")]
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
        public ActionResult<HttpResponseModel> AddClinicService([FromBody] ClinicServiceRegistrationModel serviceInfo)
        {
            ClinicServiceInfoModel? service = ClinicMapper.MapToServiceInfo(serviceInfo);

            service = clinicServiceService.AddClinicService(service, out var message);

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
            var clinicServiceInfo = clinicService.UpdateClinicService(serviceInfo, out var message);

            if (clinicService == null)
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
                    Content = clinicServiceInfo
                });
            }
        }

        [HttpDelete("service/{clinicServiceId}/on")]
        public ActionResult<HttpResponseModel> EnableService(Guid clinicServiceId)
        {
            if (clinicService.EnableClinicService(clinicServiceId, out var message))
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

        [HttpPut("service/{clinicServiceId}/off")]
        public ActionResult<HttpResponseModel> DisableService(Guid clinicServiceId)
        {
            if (clinicService.DisableClinicService(clinicServiceId, out var message))
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
