using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.ReportModels;
using ClinicPlatformObjects.ServiceModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private IUserService userService;
        private IClinicService clinicService;
        //private ITokenService tokenService;
        private IBookingService bookingService;
        private IPaymentService paymentService;
        private IScheduleService scheduleService;
        private IClinicServiceService clinicServiceService;
        private IAdminService adminService;

        public AdminController(IUserService userService, IClinicService clinicService, IBookingService bookingService, IScheduleService scheduleService, IClinicServiceService clinicServiceService, IAdminService adminService, IPaymentService paymentService)
        {
            this.userService = userService;
            this.clinicService = clinicService;
            //this.tokenService = tokenService;
            this.bookingService = bookingService;
            this.paymentService = paymentService;
            this.scheduleService = scheduleService;
            this.clinicServiceService = clinicServiceService;
            this.adminService = adminService;
        }


        /// <summary>
        ///  I WAS WRONG WHEN I CREATED THIS ENDPOINT! 
        ///  DON'T USE THIS WHILE I'M FINDING ANOTHER WAY TO FIX THIS DANGEROUS ONE.
        ///  THIS WILL RETURN "ALL" USER INFORMATION, INCLUDING PASSWORD.
        /// </summary>
        /// <returns>
        ///  All system user informations
        /// </returns>
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
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> GetClinics([FromQuery] int page_size = int.MaxValue, [FromQuery] int page = 1)
        {
            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = $"Showing page {page}.",
                Content = clinicService.GetAllClinic(page_size, page - 1)
            });
        }

        [HttpGet("customer")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetCustomer([FromQuery] int pageSize = int.MaxValue, [FromQuery] int page = 1)
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
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetClinicOwners([FromQuery] int pageSize = int.MaxValue, [FromQuery] int page = 1)
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
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetDentists(int clinicId, [FromQuery] int pageSize = int.MaxValue, [FromQuery] int page = 1)
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

        [HttpGet("verified-clinic")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicRegistrationModel>>> GetRegisteredClinics([FromQuery] string name = "", [FromQuery] int pageSize = int.MaxValue, [FromQuery] int page = 1)
        {

            var registeredClinic = clinicService.GetVerifiedClinics();

            Console.WriteLine(registeredClinic.First().OwnerName);

            if (registeredClinic == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "There is no registered clinic"
                });
            }
            else
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Content = registeredClinic.Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).Skip(pageSize * (page - 1)).Take(pageSize),
                    Message = "Success"
                });
            }
        }

        [HttpGet("unverified-clinic")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicRegistrationModel>>> GetUnregisteredClinics([FromQuery] string name = "", [FromQuery] int pageSize = int.MaxValue, [FromQuery] int page = 1)
        {
            var unregisteredClinic = clinicService.GetUnverifiedClinics();

            if (unregisteredClinic == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "There is no unregistered clinic"
                });
            }
            else
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Success = true,
                    Content = unregisteredClinic.Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).Skip(pageSize*(page-1)).Take(pageSize),
                    Message = "Success"
                });
            }
        }

        [HttpPut("clinic/unverify/{clinicId}")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> UnverifyClinicStatus(int clinicId)
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
            else if (clinic.Status == "unverified" || clinic.Status == "removed")
            {
                response.StatusCode = 400;
                response.Success = false;
                response.Message = "Can not unverify this clinic!";
                return BadRequest(response);
            }
            else
            {
                clinic.Status = "unverified";
                clinicService.UpdateClinicInformation(clinic, out _);

                response.StatusCode = 200;
                response.Message = "Updated clinic status!";
                response.Success = true;
                return Ok(response);
            }
        }

    }
}
