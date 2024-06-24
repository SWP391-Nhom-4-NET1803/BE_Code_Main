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

        [HttpGet]
        [Authorize(Roles = "Admin")]
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
    }
}
