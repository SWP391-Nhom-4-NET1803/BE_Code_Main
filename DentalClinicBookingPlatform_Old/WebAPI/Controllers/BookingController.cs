using AutoMapper;
using Core.HttpModels;
using Core.HttpModels.ObjectModels.BookingModels;
using Core.HttpModels.ObjectModels.RoleModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Repositories.Models;
using Services.BookingService;
using Services.ClinicsService;
using Services.UserService;
using WebAPI.Helper.AuthorizationPolicy;
using static System.Net.Mime.MediaTypeNames;

namespace WebAPI.Controllers
{
    [Route("api/booking")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingController(DentalClinicPlatformContext context, IMapper mapper)
        {
            _unitOfWork = new UnitOfWork(context);
            _mapper = mapper;
        }

        [HttpPost("book")]
        //[JwtTokenAuthorization(RoleModel.Roles.Customer, RoleModel.Roles.ClinicStaff)]
        public ActionResult<HttpResponseModel> CreateNewBooking([FromBody] BookingModel booking)
        {
            var bookingService = HttpContext.RequestServices.GetService<IBookingService>()!;

            var HttpClinicService = HttpContext.RequestServices.GetService<IClinicsService>()!;

            List<ClinicService> service = [];

            if (booking.SelectedService != null)
            {
                service.Add(HttpClinicService.GetServiceInfo((Guid)booking.SelectedService));

            }

            if(!bookingService.CreateSimpleAppointment(booking, service, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode=400, Message="Error while making request", Detail=message});
            }

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Request processed successfully"});

        }

        [HttpPost("general-book")]
        //[JwtTokenAuthorization(RoleModel.Roles.Customer, RoleModel.Roles.ClinicStaff)]
        public ActionResult<HttpResponseModel> CreateNewGeneralBooking([FromBody] BookingRegistrationModel booking)
        {
            var bookingService = HttpContext.RequestServices.GetService<IBookingService>()!;

            if (!bookingService.CreateOneTimeAppointment(booking, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Failed", Detail = message });
            }

            _unitOfWork.Save();
            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Request processed successfully" });

        }

        [HttpGet("schedule/staff")]
        //[JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<IHttpResponseModel<IEnumerable<BookingModel>>> GetStaffBooking([FromQuery] int staffId)
        {

            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            /*var invoker = (User?) HttpContext.Items["user"]!;

            ClinicStaff? invokerInfo = userService.GetClinicStaffInfoById(invoker.UserId);*/

            ClinicStaff? targetInfo = userService.GetFromStaffId(staffId);


            /*if (invokerInfo == null)
            {
                return Unauthorized(new HttpResponseModel() { StatusCode=403, Message="Unauthorized", Detail="User is unauthorized for this action"});
            }

            if (targetInfo == null)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Failed", Detail = "Staff does not exist" });
            }

            if (invokerInfo.ClinicId == targetInfo.ClinicId && (invokerInfo.StaffId == targetInfo.StaffId || invokerInfo.IsOwner))*/
            {
                var bookingList = _unitOfWork.BookingRepository.getBookingForClinicStaff(staffId);

                var mappedList = new List<BookingModel>();

                foreach (var booking in bookingList)
                {
                    mappedList.Add(_mapper.Map<Booking, BookingModel>(booking));
                }

                return Ok(new HttpResponseModel() { StatusCode=200, Content=mappedList});
            }

            return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Failed", Detail = "Don't try to seek for something that you shouldn't know about." });
        }

        [HttpGet("schedule/customer")]
        //[JwtTokenAuthorization(RoleModel.Roles.Customer)]
        public ActionResult<IHttpResponseModel<IEnumerable<BookingModel>>> GetCustomerBooking([FromQuery] int customerId)
        {
            var bookingList = _unitOfWork.BookingRepository.getBookingForCustomer(customerId);

            var mappedList = new List<BookingModel>();

            foreach (var booking in bookingList)
            {
                mappedList.Add(_mapper.Map<Booking, BookingModel>(booking));
            }
            return Ok(new HttpResponseModel() { StatusCode=200, Message="Succeed", Content=mappedList});
        }

    }
}
