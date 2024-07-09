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

        [HttpGet("{id}")]
        public ActionResult<IHttpResponseModel<ClinicInfoModel>> GetClinicInformation(int id)
        {
            ClinicInfoModel? clinicInfo = clinicService.GetClinicWithId(id);

            if (clinicInfo == null || clinicInfo.Status == "removed")
            {
                return NotFound(new HttpResponseModel
                {
                    StatusCode = 404,
                    Success = false,
                    Message = $"Clinic not found for Id {id}",
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = "Clinic found",
                Content = clinicInfo
            });
        }

        [HttpGet("search")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> SearchClinic([FromQuery] string? name=null, [FromQuery] TimeOnly? open=null, [FromQuery] TimeOnly? close=null, int page_size=100, int page = 1)
        {
            IEnumerable<ClinicInfoModel> result;

            result = clinicService.GetAllClinic(page_size, page-1);

            result = result.Where(x => x.Status == "verified");

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
                Success = true,
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

            if (userInfo == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message
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
                    Success = false,
                    Message =  message
                });
            }

            userInfo.ClinicId = clinic.Id;
            UserInfoModel? user = userService.UpdateUserInformation(userInfo, out message);

            if (user == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message =  message
                });
            }
            
            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = "Succesfully created new clinic",
                Content = clinic
            });
        }

        [HttpPut]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> UpdateClinicInformation([FromBody] ClinicInfoUpdateModel clinicUpdateInfo)
        {
            UserInfoModel invoker = (UserInfoModel) HttpContext.Items["user"]!;

            if (!invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Success = false,
                    Message = "This is clinic owner available only feature!"
                });
            }

            if (invoker.ClinicId != clinicUpdateInfo.Id)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Success = false,
                    Message = "You can only change your clinic information!"
                });
            }

            ClinicInfoModel? clinicInfo = clinicService.GetClinicWithId(clinicUpdateInfo.Id);

            if (clinicInfo == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "No clinic were found with provided Id"
                });
            }

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
                    Success = false,
                    Message = message,
                });
            }
            else
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Message = $"Information updated for clinic {clinicInfo.Id}",
                    Content = clinicInfo
                });
            }
        }

        [HttpPut("activate/{id}")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> ActivateClinic(int id)
        {
            UserInfoModel invoker = (HttpContext.Items["user"] as UserInfoModel)!;

            if (!invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Success=false,
                    Message = "Unauthorized",
                });
            }

            var clinicInfo = clinicService.ActivateClinic(id, out var message);

            if (clinicInfo == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = false,
                Message = $"Clinic {id} has been activated."
            });
        }

        [HttpPut("deactivate/{id}")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> InactivateClinic(int id)
        {
            UserInfoModel invoker = (HttpContext.Items["user"] as UserInfoModel)!;

            if (!invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Success = false,
                    Message = "Unauthorized",
                });
            }

            var clinicInfo = clinicService.InactivateClinic(id, out var message);

            if (clinicInfo == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message,
                    Content = clinicInfo
                });
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = $"Clinic {id} has been deactivated.",
                Content = clinicInfo
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> RemoveClinic(int id)
        {
            UserInfoModel invoker = (HttpContext.Items["user"] as UserInfoModel)!;

            if (!invoker.IsOwner || invoker.ClinicId != id)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Success = false,
                    Message = "Unauthorized",
                });
            }

            if (!clinicService.DeleteClinic(id)) // Please consider using status or flags instead.
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Clinic removal failed",
                });
            }

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = $"Clinic {id} has been removed."
            });
        }
    }
}
