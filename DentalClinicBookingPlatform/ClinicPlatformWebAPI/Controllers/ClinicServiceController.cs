using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformObjects.ServiceModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.ModelMapper;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/services")]
    [ApiController]
    [Authorize(Roles = "Dentist")]
    public class ClinicServiceController : ControllerBase
    {
        private readonly IClinicService clinicService;
        private readonly IClinicServiceService clinicServiceService;
        private readonly IUserService userService;

        public ClinicServiceController(IClinicService clinicService, IUserService userService, IClinicServiceService clinicServiceService)
        {
            this.clinicService = clinicService;
            this.userService = userService;
            this.clinicServiceService = clinicServiceService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicServiceInfoModel>>> GetAllService([Required]int clinicId)
        {

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = "Success",
                Content = clinicServiceService.GetAllClinicService(clinicId)
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<ClinicServiceInfoModel>> GetService(Guid id)
        {
            ClinicServiceInfoModel? service = clinicServiceService.GetClinicService(id);

            if (service == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = $"Can not find clinic service {id}"
                });
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = "Success",
                Content = service
            });
        }

        [HttpPost]
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
                    Content = service
                });
            }
        }

        [HttpPost("add-batch")]
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

        [HttpPut]
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

        [HttpPut("{clinicServiceId}/activate")]
        public ActionResult<HttpResponseModel> EnableService(Guid clinicServiceId)
        {
            if (clinicService.EnableClinicService(clinicServiceId, out var message))
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Service activated",
                    Detail = message
                });
            }
            return BadRequest(new HttpResponseModel()
            {
                StatusCode = 400,
                Message = "Failed to activate service",
                Detail = message
            });
        }

        [HttpPut("{clinicServiceId}/deactivate")]
        public ActionResult<HttpResponseModel> DisableService(Guid clinicServiceId)
        {
            if (clinicService.DisableClinicService(clinicServiceId, out var message))
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Service disabled",
                    Detail = message
                });
            }
            return BadRequest(new HttpResponseModel()
            {
                StatusCode = 400,
                Message = "Failed to deactivate service",
                Detail = message
            });

        }
    }
}
