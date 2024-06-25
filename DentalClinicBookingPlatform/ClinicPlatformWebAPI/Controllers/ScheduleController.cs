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
                Message = "Failed getting clinic info",
                Detail = $"There are no clinic with Id {id} exist."
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
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Clinic not found.",
                    Detail = $"Can not find clinic information for id {slotInfo.ClinicId}"
                });
            }

            if (!invoker.IsOwner || invoker.ClinicId != clinic.Id)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed adding clinic slot.",
                    Detail = $"User lacking priviledges"
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
        public ActionResult<HttpResponseModel> UpdateClinicSlot([FromBody] ClinicSlotUpdateModel slotInfo)
        {
            ClinicSlotInfoModel? slot = scheduleService.GetClinicSlotById(slotInfo.slotId);

            if (slot == null)
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

        [HttpPut("slot/enable")]
        public ActionResult<HttpResponseModel> EndableSlot([FromQuery] Guid slotId)
        {
            ClinicSlotInfoModel? slot = scheduleService.GetClinicSlotById(slotId);

            if (slot == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
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

        [HttpPut("slot/disable")]
        public ActionResult<HttpResponseModel> DisableSlot([FromQuery] Guid slotId)
        {
            ClinicSlotInfoModel? slot = scheduleService.GetClinicSlotById(slotId);

            if (slot == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
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

        [HttpDelete("slot/delete")]
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
