using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformObjects.ReportModels;
using ClinicPlatformObjects.ServiceModels;
using ClinicPlatformObjects.UserModels;
using ClinicPlatformObjects.UserModels.CustomerModel;
using ClinicPlatformObjects.UserModels.DentistModel;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.ModelMapper;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

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

        [HttpGet("users")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoViewModel>>> GetUsers([FromQuery] int page_size = int.MaxValue, [FromQuery] int page = 1)
        {
            IEnumerable<UserInfoModel> user = userService.GetUsers();

            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Success = true,
                Message = "Total records: {user.Count()}",
                Content = user.Select(x => UserInfoMapper.FromUserInfoToView(x)).Skip((page-1)*page_size).Take(page_size)
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
        public ActionResult<IHttpResponseModel<IEnumerable<CustomerInfoViewModel>>> GetCustomer([FromQuery]string name = "", [FromQuery] int pageSize = int.MaxValue, [FromQuery] int page = 1)
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = $"Showing page {page}.",
                Content = userService.GetAllUserOfRole("Customer").Skip(pageSize * (page - 1)).Take(pageSize).Select(x => UserInfoMapper.ToCustomerView(x))
            });;
        }

        [HttpGet("clinic-owners")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetClinicOwners([FromQuery]string name = "", [FromQuery] int pageSize = int.MaxValue, [FromQuery] int page = 1)
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = $"Showing page {page}.",
                Content = userService.GetAllUserOfRole("Dentist").Skip(pageSize * (page - 1)).Take(pageSize).Where(x => x.Fullname.Contains(name, StringComparison.OrdinalIgnoreCase) && x.IsOwner).Select(x => UserInfoMapper.ToDentistView(x))
            });
        }

        [HttpGet("dentist")]
        public ActionResult<IHttpResponseModel<IEnumerable<DentistInfoViewModel>>> GetDentists([FromQuery] int pageSize = int.MaxValue, [FromQuery] int page = 1)
        {
            List<DentistInfoViewModel> itemList = (List<DentistInfoViewModel>)userService.GetAllUserOfRole("Dentist").Skip(pageSize * (page - 1)).Take(pageSize).Select(x => UserInfoMapper.ToDentistView(x)).ToList();

            foreach (var dentist in itemList)
            {
                dentist.ClinicName = clinicService.GetClinicWithId((int)dentist.ClinicId).Name;
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Success = true,
                Message = $"Showing page {page}.",
                Content = itemList/*Where(x => x.Fullname.Contains(name, StringComparison.OrdinalIgnoreCase))*/
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
