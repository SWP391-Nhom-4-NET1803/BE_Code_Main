using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.ReportModels;
using ClinicPlatformObjects.ServiceModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/admin")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private IUserService userService;
        private IClinicService clinicService;
        //private ITokenService tokenService;
        private IBookingService bookingService;
        //private IPaymentService paymentService;
        private IScheduleService scheduleService;
        private IClinicServiceService clinicServiceService;
        private IAdminService adminService;

        public AdminController(IUserService userService, IClinicService clinicService, IBookingService bookingService, IScheduleService scheduleService, IClinicServiceService clinicServiceService, IAdminService adminService)
        {
            this.userService = userService;
            this.clinicService = clinicService;
            //this.tokenService = tokenService;
            this.bookingService = bookingService;
            //this.paymentService = paymentService;
            this.scheduleService = scheduleService;
            this.clinicServiceService = clinicServiceService;
            this.adminService = adminService;
        }

        [HttpGet("users")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetUsers()
        {
            IEnumerable<UserInfoModel> user = userService.GetUsers();

            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = "Total records: {user.Count()}",
                Content = user
            };

            return Ok(ResponseBody);
        }

        [HttpGet("clinics")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> GetClinics([FromQuery] int page = 1)
        {
            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = $"Showing page {page}.",
                Content = clinicService.GetAllClinic(20, page - 1)
            });
        }

        [HttpGet("customer")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetCustomer([FromQuery] int pageSize = 20, [FromQuery] int page = 1)
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = $"Showing page {page}.",
                Content = userService.GetAllUserOfRole("Customer").Skip(pageSize * (page - 1)).Take(pageSize).ToList()
            });
        }

        [HttpGet("clinic-owners")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetClinicOwners([FromQuery] int pageSize = 20, [FromQuery] int page = 1)
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = $"Showing page {page}.",
                Content = userService.GetAllUserOfRole("Dentist").Where(x => x.IsOwner).Skip(pageSize*(page - 1)).Take(pageSize).ToList()
            });
        }

        [HttpGet("dentist")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetDentists(int clinicId, [FromQuery] int pageSize = 20, [FromQuery] int page = 1)
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = $"Showing page {page}.",
                Content = userService.GetAllUserOfRole("Dentist").Where(x => !x.IsOwner).Skip(pageSize * (page - 1)).Take(pageSize).ToList()
            });
        }

        [HttpPut("clinic/verify/{clinicId}")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> VerifyClinicStatus(int clinicId)
        {
            ClinicInfoModel? clinic = clinicService.GetClinicWithId(clinicId);

            HttpResponseModel response = new HttpResponseModel();

            if (clinic == null)
            {
                response.StatusCode = 400;
                response.Success = false;
                response.Message = $"Can not find clinic with Id {clinicId}";
                return BadRequest(response);
            }
            else if (clinic.Status == "verified" || clinic.Status == "removed")
            {
                response.StatusCode = 400;
                response.Success = false;
                response.Message = "Can not verify this clinic!";
                return BadRequest(response);
            }
            else
            {
                clinic.Status = "verified";
                clinicService.UpdateClinicInformation(clinic, out _);

                response.StatusCode = 200;
                response.Message = "Updated clinic status!";
                response.Success = true;
                return Ok(response);
            }
        }

        [HttpGet("service/categories")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicServiceCategoryModel>>> GetAllCategory()
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = "Success",
                Content = clinicServiceService.GetAllCategory()
            });
        }

        [HttpPost("service/categories")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicServiceCategoryModel>>> AddCategory([FromBody] ClinicServiceCategoryRegistrationModel category)
        {
            ClinicServiceCategoryModel clinicCategory = new ClinicServiceCategoryModel()
            {
                Name = category.Name,
            };

            if(clinicServiceService.AddServiceCategory(clinicCategory, out var message))
            {
                return Ok(new HttpResponseModel
                {
                    StatusCode = 200,
                    Success = true,
                    Message = message,
                    Content = clinicServiceService.GetAllCategory()
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = message,
                    Content = null,
                });
            }
        }

        [HttpGet("summary")]
        public ActionResult<HttpResponseModel> GetSummaryInfo([FromQuery]DateOnly? from,[FromQuery] DateOnly? to)
        {

            if (from == null || to == null)
            {
                return Ok(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Success",
                    Content = adminService.GetTodaySummaryReport(),
                });
            }

            if (from > to)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Success = false,
                    Message = $"Can not proccess request be cause {from.Value.ToString("dd/mm/yyyy")} is after {to.Value.ToString("dd/mm/yyyy")}"
                });
            }
            
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true, 
                Message = "Success",
                Content = adminService.GetReportInDateRange((DateOnly)from, (DateOnly)to)
            });
        }

    }
}
