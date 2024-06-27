using ClinicPlatformDatabaseObject;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.UserModels.CustomerModel;
using ClinicPlatformObjects.UserModels.DentistModel;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.ModelMapper;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/dentist")]
    [ApiController]
    public class DentistController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IClinicService clinicService;
        private readonly IClinicServiceService clinicServiceService;
        private readonly IBookingService bookingService;
        private readonly IScheduleService scheduleService;

        public DentistController(IUserService userService, IClinicService clinicService, IClinicServiceService clinicServiceService, IBookingService bookingService, IScheduleService scheduleService)
        {
            this.userService = userService;
            this.clinicService = clinicService;
            this.clinicServiceService = clinicServiceService;
            this.bookingService = bookingService;
            this.scheduleService = scheduleService;
        }

        // ======================== Dentist  ==========================================

        [HttpGet]
        [Authorize(Roles = "Dentist")]
        public ActionResult<IHttpResponseModel<DentistInfoViewModel>> GetDentistInfo() 
        {
            UserInfoModel invoker = (HttpContext.Items["user"] as UserInfoModel)!;

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = "Success",
                Content = UserInfoMapper.ToDentistView(invoker)
            });
        }

        [HttpPut]
        [Authorize(Roles = "Dentist")]
        public ActionResult<IHttpResponseModel<DentistInfoViewModel>> UpdateDentistInfo(DentistUpdateModel updatedInfo)
        {
            UserInfoModel? invoker = (HttpContext.Items["user"] as UserInfoModel)!;

            if (invoker.Id != updatedInfo.Id)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = $"Update failed",
                    Detail = $"User Id and update info Id mismatch! UserId: {invoker.Id}, TargetUserId: {updatedInfo.Id}."
                });
            }

            invoker.Username = updatedInfo.Username ?? invoker.Email;
            invoker.Fullname = updatedInfo.Fullname ?? invoker.Fullname;
            invoker.Email = updatedInfo.Email ?? invoker.Email;
            invoker.Phone = updatedInfo.Phone ?? invoker.Phone;

            invoker = userService.UpdateUserInformation(invoker, out var message);

            if (invoker is null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = $"Update failed",
                    Detail = message
                });
            }
            else
            {
                return Ok(new HttpResponseModel
                {
                    StatusCode = 200,
                    Message = $"Updated successfully.",
                    Detail = $"Update information for invoker UserId {invoker.Id}",
                    Content = invoker
                });
            }
        }


        // ======================= Clinic Owner ========================================

        [HttpGet("staff")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<IHttpResponseModel<DentistInfoViewModel>> GetDentistInformation()
        {
            UserInfoModel invoker = (UserInfoModel)HttpContext.Items["user"]!;

            if (!invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel()
                {
                    StatusCode = 403,
                    Message = "Unauthorized",
                    Detail = "You don't have permission to access the resource"
                });
            }

            IEnumerable<DentistInfoViewModel> dentistList = from user in userService.GetUsers().Where(x => x.ClinicId == invoker.ClinicId && !x.IsRemoved) select UserInfoMapper.ToDentistView(user);

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

            UserInfoModel? dentist = userService.GetUserWithDentistId(dentistId);

            if (dentist == null || dentist.IsRemoved)
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
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 404,
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

        [HttpPost("staff/register")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> RegisterClinicStaff([FromBody] DentistRegistrationModel dentistInfo)
        {
            UserInfoModel invoker = (HttpContext.Items["user"] as UserInfoModel)!;

            if (!invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel()
                {
                    StatusCode = 403,
                    Message = "Unauthorized",
                    Detail = "You do not have permission to invoke this method.",
                });
            }

            UserInfoModel? user = UserInfoMapper.FromRegistration(dentistInfo);
            user.ClinicId = invoker.ClinicId;
            user.IsActive = true;
            user.IsOwner = false;
                
            user = userService.RegisterAccount(user, "Dentist", out var message);

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

        [HttpPut("staff/deactivate")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> DeactivateDentistAccount(int dentistId)
        {
            UserInfoModel invoker = (HttpContext.Items["user"] as UserInfoModel)!;

            UserInfoModel? target = userService.GetUserWithDentistId(dentistId);

            if (target == null || target.IsRemoved)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = $"Update failed",
                    Detail = $"Can not find invoker with provided Id."
                });
            }

            if (invoker.ClinicId != target.ClinicId || !invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Message = $"Forbidden",
                    Detail = $"You can not update the target resource!."
                });
            }
            if (!target.IsActive)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = $"Update failed",
                    Detail = "Dentist account is already actived!"
                });
            }

            target.IsActive = false;
            target = userService.UpdateUserInformation(target, out var message);

            if (target == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = $"Update failed",
                    Detail = message
                });
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = $"Updated successfully",
                Detail = "Deactivated dentist account"
            });
            
        }

        [HttpPut("staff/activate")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> ActivateDentistAccount(int dentistId)
        {
            UserInfoModel invoker = (UserInfoModel)HttpContext.Items["user"]!;

            UserInfoModel? target = userService.GetUserWithDentistId(dentistId);

            if (target == null || target.IsRemoved)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = $"Update failed",
                    Detail = $"Can not find invoker with provided Id."
                });
            }

            if (invoker.ClinicId == target.ClinicId && invoker.IsOwner)
            {
                if (target.IsActive)
                {
                    return BadRequest(new HttpResponseModel
                    {
                        StatusCode = 400,
                        Message = $"Update failed",
                        Detail = "Dentist account is already actived!"
                    });
                }

                target.IsActive = true;
                target = userService.UpdateUserInformation(target, out var message);

                if (target == null)
                {
                    return BadRequest(new HttpResponseModel
                    {
                        StatusCode = 400,
                        Message = $"Update failed",
                        Detail = message
                    });
                }

                return Ok(new HttpResponseModel
                {
                    StatusCode = 200,
                    Message = $"Updated successfully",
                    Detail = "Activated dentist account"
                });
            }
            else
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Message = $"Forbidden",
                    Detail = $"You can not update the target resource!."
                });
            }
        }

        [HttpDelete("staff")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> RemoveDentistAccount(int dentistId)
        {
            UserInfoModel invoker = (UserInfoModel)HttpContext.Items["user"]!;

            UserInfoModel? target = userService.GetUserWithDentistId(dentistId);

            if (target == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = $"Update failed",
                    Detail = $"Can not find invoker with provided Id."
                });
            }

            if (invoker.ClinicId != target.ClinicId || !invoker.IsOwner)
            {
                return Unauthorized(new HttpResponseModel
                {
                    StatusCode = 403,
                    Message = $"Unauthorized",
                    Detail = $"You do not have access to this resource."
                });

            }

            if (userService.DeleteUser(target.Id, out var message))
            {
                return Ok(new HttpResponseModel
                {
                    StatusCode = 200,
                    Message = $"Update success",
                    Detail = message
                });
            }
            else
            {
                return Ok(new HttpResponseModel
                {
                    StatusCode = 200,
                    Message = $"Update success",
                    Detail = message,
                });
            }
        }

    }
}
