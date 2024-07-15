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
    [Route("api/service")]
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
                Success = true,
                Message = "Success",
                Content = clinicServiceService.GetAllClinicService(clinicId).Where(x => x.Available && !x.Removed)
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
                    Success = false,
                    Message = $"Can not find clinic service {id}"
                });
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
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
                    Message = $"{message}",
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
                    Message = message,
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
                    Message = $"Updated service information!",
                    Content = clinicServiceInfo
                });
            }
        }

        [HttpPut("{clinicServiceId}/activate")]
        public ActionResult<HttpResponseModel> EnableService(Guid clinicServiceId)
        {
            var clinicState = clinicService.EnableClinicService(clinicServiceId, out var message);


            if (clinicState != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Message = message
                });
            }
            return BadRequest(new HttpResponseModel()
            {
                StatusCode = 400,
                Success = false,
                Message = message
            });
        }

        [HttpPut("{clinicServiceId}/deactivate")]
        public ActionResult<HttpResponseModel> DisableService(Guid clinicServiceId)
        {
            var clinicState = clinicService.DisableClinicService(clinicServiceId, out var message);

            if (clinicState != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success= true,
                    Message = message
                });
            }
            return BadRequest(new HttpResponseModel()
            {
                StatusCode = 400,
                Success = false,
                Message = message
            });

        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicServiceCategoryModel>>> GetAllCategories()
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 0,
                Success = true,
                Message = "Success",
                Content = clinicServiceService.GetAllCategory()
            });
        }

        [HttpGet("category/{id}")]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<ClinicServiceCategoryModel>> GetCategory(int id)
        {
            var result = clinicServiceService.GetCategory(id);

            if (result == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 404,
                    Success = false,
                    Message = $"Can not find service category with Id {id}"
                });
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 0,
                Success = true,
                Message = "Found service category",
                Content = result
            });
        }
    }
}
