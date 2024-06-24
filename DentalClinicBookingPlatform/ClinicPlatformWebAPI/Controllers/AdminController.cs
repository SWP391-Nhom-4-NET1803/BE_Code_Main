using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.UserModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        public AdminController(IUserService userService, IClinicService clinicService, IBookingService bookingService, IPaymentService paymentService, IScheduleService scheduleService, IClinicServiceService clinicServiceService)
        {
            this.userService = userService;
            this.clinicService = clinicService;
            //this.tokenService = tokenService;
            this.bookingService = bookingService;
            this.paymentService = paymentService;
            this.scheduleService = scheduleService;
            this.clinicServiceService = clinicServiceService;
        }

        [HttpGet("users")]
        public ActionResult<IHttpResponseModel<IEnumerable<UserInfoModel>>> GetUsers()
        {
            IEnumerable<UserInfoModel> user = userService.GetUsers();

            var ResponseBody = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Get info success",
                Detail = $"Total records: {user.Count()}",
                Content = user
            };

            return Ok(ResponseBody);
        }

        [HttpGet("clinics")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> GetClinics([FromQuery] int page = 1)
        {
            return Ok(new HttpResponseModel()
            {
                Content = clinicService.GetAllClinic(20, page - 1)
            });
        }

        [HttpPut("clinic/verify/{clinicId}")]
        public ActionResult<IHttpResponseModel<IEnumerable<ClinicInfoModel>>> VerifyClinicStatus([FromQuery] int clinicId)
        {
            ClinicInfoModel? clinic = clinicService.GetClinicWithId(clinicId);

            HttpResponseModel response = new HttpResponseModel();

            if (clinic == null)
            {
                response.StatusCode = 400;
                response.Message = $"Can not find clinic with Id {clinicId}";
                return BadRequest(response);
            }
            else if (clinic.Status == "verified" || clinic.Status == "removed")
            {
                response.StatusCode = 400;
                response.Message = "Can not verify this clinic!";
                return BadRequest(response);
            }
            else
            {
                clinic.Status = "verified";

                clinicService.UpdateClinicInformation(clinic, out _);

                response.StatusCode = 200;
                response.Message = "Updated clinic status!";
                response.Detail = "Clinic information verified!";

                return Ok(response);
            }
        }

        [HttpGet("service/categories")]
        public ActionResult<IHttpResponseModel<IEnumerable<ServiceCategoryModel>>> GetAllCategory()
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = "Success",
                Content = clinicServiceService.GetAllCategory()
            });
        }

        [HttpPost("service/categories")]
        public ActionResult<IHttpResponseModel<IEnumerable<ServiceCategoryModel>>> AddCategory([FromBody] string categoryName)
        {
            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = "Success",
                Content = clinicServiceService.GetAllCategory()
            });
        }

    }
}
