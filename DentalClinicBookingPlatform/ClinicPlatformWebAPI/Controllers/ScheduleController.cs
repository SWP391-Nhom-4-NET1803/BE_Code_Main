using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.SlotModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.ModelMapper;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/schedule")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IBookingService bookingService;
        private readonly IScheduleService scheduleService;
        private readonly IClinicService clinicService;

        public ScheduleController(IUserService userService, IBookingService bookingService, IScheduleService scheduleService, IClinicService clinicService)
        {
            this.userService = userService;
            this.bookingService = bookingService;
            this.scheduleService = scheduleService;
            this.clinicService = clinicService;
        }

        [HttpGet("{id}/slots")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicSlotInfoModel>>> GetClinicSlot(int id)
        {
            if (clinicService.GetClinicWithId(id) != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Success",
                    Content = scheduleService.GetAllClinicSlot(id)
                });
            }

            return BadRequest(new HttpResponseModel()
            {
                StatusCode = 400,
                Success = false,
                Message = $"There are no clinic with Id {id} exist."
            });
        }

        [HttpPost("slot/create")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> AddClinicSlot([FromBody] ClinicSlotRegistrationModel slotInfo)
        {

            UserInfoModel invoker = (UserInfoModel)HttpContext.Items["user"]!;

            ClinicInfoModel? clinic = clinicService.GetClinicWithId((int)slotInfo.ClinicId);

            if (clinic == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = $"Can not find clinic information for id {slotInfo.ClinicId}"
                });
            }

            if (!invoker.IsOwner || invoker.ClinicId != clinic.Id)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = $"User lacking priviledges."
                });
            }

            ClinicSlotInfoModel? slot = ClinicMapper.MapToSlotInfo(slotInfo);

            slot.Status = true;

            slot = scheduleService.AddNewClinicSlot(slot, out var message);

            if (slot != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Message = message,
                    Content = slot
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message
                });
            }
        }

        [HttpPut("slot/update")]
        [Authorize(Roles = "Dentist")]
        public ActionResult<HttpResponseModel> UpdateClinicSlot([FromBody] ClinicSlotUpdateModel slotInfo)
        {
            UserInfoModel invoker = (HttpContext.Items["user"] as UserInfoModel)!;

            ClinicSlotInfoModel? slot = scheduleService.GetClinicSlotById(slotInfo.slotId);

            if (!invoker.IsOwner)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "There are no slot with the given ID found."
                });
            }

            if (slot == null || slot.ClinicId != invoker.ClinicId)
            {
                return BadRequest(new HttpResponseModel()
                {
                   StatusCode = 400,
                   Message = "There are no slot with the given ID found."
                });
            }

            slot.MaxCheckup = slotInfo.MaxCheckup;
            slot.MaxTreatment = slotInfo.MaxTreatement;
            slot.Status = slotInfo.Status;

            slot = scheduleService.UpdateClinicSlot(slot, out var message);

            if (slot != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Message = message,
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message
                });
            }
        }

        [HttpPut("slot/{slotId}/enable")]
        public ActionResult<HttpResponseModel> EnableSlot(Guid slotId)
        {
            ClinicSlotInfoModel? slot = scheduleService.GetClinicSlotById(slotId);

            if (slot == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "There are no slot with the given ID found."
                });
            }

            slot.Status = true;
            slot = scheduleService.UpdateClinicSlot(slot, out var message);

            if (slot != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Message = message,
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message
                });
            }
        }

        [HttpPut("slot/{slotId}/disable")]
        public ActionResult<HttpResponseModel> DisableSlot(Guid slotId)
        {
            ClinicSlotInfoModel? slot = scheduleService.GetClinicSlotById(slotId);

            if (slot == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "There are no slot with the given ID found."
                });
            }

            slot.Status = false;

            slot = scheduleService.UpdateClinicSlot(slot, out var message);

            if (slot != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Message = message,
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message
                });
            }
        }

        [HttpDelete("slot/delete")]
        public ActionResult<HttpResponseModel> DeleteClinicSlot(Guid slotId)
        {
            if (scheduleService.DeleteClinicSlot(slotId))
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Message = "Success"
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Slot deletion failed",
                });
            }
        }
    }
}
