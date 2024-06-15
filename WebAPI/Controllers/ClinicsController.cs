using AutoMapper;
using Azure;
using Core.HttpModels;
using Core.HttpModels.ObjectModels.BookingModels;
using Core.HttpModels.ObjectModels.ClinicModels;
using Core.HttpModels.ObjectModels.RegistrationModels;
using Core.HttpModels.ObjectModels.RoleModels;
using Core.HttpModels.ObjectModels.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Models;
using Services.BookingService;
using Services.ClinicsService;
using Services.EmailSerivce;
using Services.UserService;
using System.Net;
using System.Security.Claims;
using WebAPI.Helper.AuthorizationPolicy;

namespace WebAPI.Controllers
{
    [Route("api/clinic")]
    [ApiController]
    public class ClinicsController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ClinicsController(IMapper mapper, DentalClinicPlatformContext context)
        {
            _unitOfWork = new UnitOfWork(context);
            _mapper = mapper;
        }

        [HttpPost]
        [Route("register-clinic")]
        //[JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public async Task<IActionResult> RegisterClinic([FromBody] ClinicRegistrationModel requestObject)
        {
            var clinicsService = HttpContext.RequestServices.GetService<IClinicsService>()!;

            if (clinicsService.CreateClinic(requestObject, out var message))
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Request accepted",
                    Detail = message
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Unable to process request",
                    Detail = message
                });
            }

            // ! DO NOT REMOVE THE FOLLOWING PIECE OF CODE!

            /*// Validate user authorization information
            var user = (User?) HttpContext.Items["user"];

            if (user == null)
            {
                return Ok(new HttpResponseModel() { StatusCode = 401, Message = "You are unauthorized"});
            }

            //Get owner info info
            ClinicStaff staff = _unitOfWork.UserRepository.GetStaffInfo(user.UserId)!;

            // Check if the user is a clinic owner or not, whether the owner already created a clinic before
            if (staff == null || !staff.IsOwner)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 401,
                    Message = "You are unauthorized!",
                    Detail = "You are not a clinic owner."
                });
            }

            if (staff.ClinicId != null)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Unable to process request",
                    Detail = "You already created a clinic."
                });
            }

            if (!_unitOfWork.ClinicRepository.CheckClinicAvailability(requestObject.Name!, out var responseMessage))
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Unable to process request",
                    Detail = responseMessage
                });
            }
            try
            {
                Clinic newClinic = new Clinic()
                {
                    Name = requestObject.Name!,
                    Address = requestObject.Address!,
                    Phone = requestObject.Phone!,
                    Email = requestObject.Email!,
                    OpenHour = TimeOnly.Parse(requestObject.OpenHour!),
                    CloseHour = TimeOnly.Parse(requestObject.CloseHour!),
                    Status = true,
                    Owner = _unitOfWork.UserRepository.GetById(staff.UserId)!
                };

                // Add clinic services
                foreach (var serviceId in requestObject.ClinicServices!)
                {
                    var service = await _unitOfWork._context.Services.FindAsync(serviceId);
                    if (service != null)
                    {
                        newClinic.ClinicServices.Add(new ClinicService
                        {
                            ServiceId = service.ServiceId,
                            Clinic = newClinic,
                            Service = service
                        });
                    }
                    else
                    {
                        return Ok(new HttpResponseModel()
                        {
                            StatusCode = 400,
                            Message = "Unable to process request",
                            Detail = $"Service with ID {serviceId} does not exist"
                        });
                    }
                }
                
                // Add the clinic to the owner inside ClinicStaff table.
                staff.Clinic = newClinic;
                _unitOfWork.clinicStaffRepository.Update(staff);
                _unitOfWork.ClinicRepository.Add(newClinic);
                _unitOfWork.Save();

                var clinicsService = HttpContext.RequestServices.GetService<IClinicsService>()!;

                if (clinicsService.CreateClinic(requestObject, out var message))
                {
                    return Ok(new HttpResponseModel()
                    {
                        StatusCode = 200,
                        Message = "Request accepted",
                        Detail = message
                    });
                }
                else 
                {
                    return BadRequest(new HttpResponseModel()
                    {
                        StatusCode = 400,
                        Message = "Unable to process request",
                        Detail = message
                    });
                }
                
            }
            catch (Exception e)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 500,
                    Message = "Internal Server Error",
                    Detail = e.Message,
                    Content = e.InnerException?.Message
                });
            }*/
        }

        [HttpPost]
        [Route("register-owner")]
        //[AllowAnonymous]
        public ActionResult<IHttpResponseModel<object>> RegisterClinicOwner([FromBody] UserRegistrationModel requestObject)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            if (!userService.CreateClinicStaff(requestObject, true, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 202, Message = "Can not create owner account", Detail = message });
            }

            _unitOfWork.Save();

            //
            /*var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

            string emailSubject = "Xác nhận yêu cầu tạo tài khoản người dùng";

            string emailBody =
                $"<p>Xin chào người dùng {requestObject.Username}! </p>" +
                $"Chúng tôi đã nhận được yêu cầu tạo tài khoản cho email {requestObject.Email}, cảm ơn bạn đã đăng kí dịch vụ của chúng tôi. </p>" +
                $"<p>Vui lòng xác thực tài khoản thông qua cổng xác thực của chúng tôi tại <a href=\"http://localhost:5173/user/auth\"></a></p>";

            await emailService.SendMailGoogleSmtp(requestObject.Email!, subject: emailSubject, body: emailBody);*/

            return Ok(new HttpResponseModel() { StatusCode = 202, Message = "Yêu cầu tạo mới người dùng đang được xử lí." });
        }

        [HttpPost]
        [Route("register-dentist")]
        //[JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<IHttpResponseModel<object>> RegisterDentist([FromBody] UserRegistrationModel requestObject)
        {

            User invoker = (User)HttpContext.Items["user"]!;

            var userService = HttpContext.RequestServices.GetService<UserService>()!;

            ClinicStaff owner = userService.GetClinicStaffInfoById(invoker.UserId)!; // Getting the clinic owner information (just get the clinic ID)

            if (!owner.IsOwner)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 401, Message = "Unauthorized", Detail = "You don't have access for this command." });
            }

            requestObject.Clinic = owner.ClinicId; // Append the clinic Id from the owner to the new clinic staff.

            if (!userService.CreateClinicStaff(requestObject, false, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Failed", Detail = message });
            }

            _unitOfWork.Save();

            /*var emailService = HttpContext.RequestServices.GetService<IEmailService>()!;

            string emailSubject = "Xác nhận yêu cầu tạo tài khoản người dùng";

            string emailBody =
                $"<p>Xin chào người dùng {requestObject.Username}! </p>" +
                $"Chúng tôi đã nhận được yêu cầu tạo tài khoản cho email {requestObject.Email} bởi chủ phòng khám. Hãy kiểm tra lại thông tin nếu bạn nhận được thông báo này. </p>" +
                $"<p>Vui lòng xác thực tài khoản thông qua cổng xác thực của chúng tôi tại <a href=\"http://localhost:5173/user/auth\"></a></p>";

            await emailService.SendMailGoogleSmtp(requestObject.Email!, subject: emailSubject, body: emailBody);
            await emailService.SendMailGoogleSmtp(invoker.Email, subject: emailSubject, body: emailBody);*/

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "User account creation is succeed" });
        }

        [HttpGet("clinic-book")]
        //[JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<IHttpResponseModel<List<BookingModel>>> GetClinicBooking([FromQuery] int clinicId)
        {
            var clinicService = HttpContext.RequestServices.GetService<IClinicsService>()!;
            var bookingService = HttpContext.RequestServices.GetService<IBookingService>()!;

            if (clinicService.GetClinicInformation(clinicId) == null)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 404, Message = "Failed", Detail = "No clinic found with provided id!" });
            }

            var appointments = bookingService.getClinicBooking(clinicId, true);

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Succed", Content = appointments });
        }

        [HttpGet("{id}/staffs")]
        //[JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<IHttpResponseModel<List<ClinicStaffInfoModel>>> GetAllStaffInfo([FromQuery] int id)
        {
            var user = (User)HttpContext.Items["user"]!;

            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            var staff = userService.GetAllClinicStaffInfo(id);

            if (staff == null)
            {
                return NotFound(new HttpResponseModel() { StatusCode = 404, Message = "User info not found" });
            }

            var staffInfo = _mapper.Map<IEnumerable<ClinicStaffInfoModel>>(staff);


            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Succeed",Content = staffInfo });
        }

        [HttpGet("{id}/staff")]
        //[JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<IHttpResponseModel<ClinicStaffInfoModel>> GetStaffInfo([FromQuery] int staffId)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            // validating invoker
            var invoker = (User)HttpContext.Items["user"]!;

            if (!userService.IsClinicOwner(invoker))
            {

            }


            

            var staff = userService.GetFromStaffId(staffId);

            if (staff == null)
            {
                return NotFound(new HttpResponseModel() { StatusCode = 404, Message = "User info not found" });
            }

            var staffInfo = _mapper.Map<ClinicStaff, ClinicStaffInfoModel>(staff);

            HttpResponseModel response = new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Succeed",
                Content = staffInfo
            };

            return Ok(response);
        }

        // =================================== Simple REST API =====================================

        [HttpGet("{id}")]
        [AllowAnonymous]
        public ActionResult<IHttpResponseModel<ClinicInformationModel>> Get(int id)
        {
            var clinicsService = HttpContext.RequestServices.GetService<IClinicsService>()!;

            var clinicInfo = clinicsService.GetClinicInformation(id);

            if (clinicInfo == null)
            {
                return NotFound(new HttpResponseModel() { StatusCode = 404, Message = "Clinic not found", Detail = "No clinic found with provided ID" });
            }

            var response = new HttpResponseModel() { StatusCode = 200, Message = "Clinic found", Content = _mapper.Map<Clinic, ClinicInformationModel>(clinicInfo) };

            return Ok(response);
        }

        [HttpPost]
        public ActionResult<IHttpResponseModel<ClinicInformationModel>> Post(int id)
        {
            var clinicsService = HttpContext.RequestServices.GetService<IClinicsService>()!;

            var clinicInfo = clinicsService.GetClinicInformation(id);

            if (clinicInfo == null)
            {
                return NotFound(new HttpResponseModel() { StatusCode = 404, Message = "Clinic not found", Detail = "No clinic found with provided ID" });
            }

            var response = new HttpResponseModel() { StatusCode = 200, Message = "Clinic found", Content = _mapper.Map<Clinic, ClinicInformationModel>(clinicInfo) };

            return Ok(response);
        }

        [HttpPut("{id}")]
        public ActionResult<IHttpResponseModel<ClinicInformationModel>> Put(int id)
        {
            var clinicsService = HttpContext.RequestServices.GetService<IClinicsService>()!;

            var clinicInfo = clinicsService.GetClinicInformation(id);

            if (clinicInfo == null)
            {
                return NotFound(new HttpResponseModel() { StatusCode = 404, Message = "Clinic not found", Detail = "No clinic found with provided ID" });
            }

            var response = new HttpResponseModel() { StatusCode = 200, Message = "Clinic found", Content = _mapper.Map<Clinic, ClinicInformationModel>(clinicInfo) };

            return Ok(response);
        }

        

        [HttpDelete("{id}")]
        public ActionResult<IHttpResponseModel<object>> DeleteClinic(int id)
        {
            return Ok(new HttpResponseModel() { Detail="This method is not implemented" });
        }
    }
}
