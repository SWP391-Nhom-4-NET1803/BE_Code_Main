using AutoMapper;
using Azure;
using Core.HttpModels;
using Core.HttpModels.ObjectModels.BookingModels;
using Core.HttpModels.ObjectModels.ClinicModels;
using Core.HttpModels.ObjectModels.RegistrationModels;
using Core.HttpModels.ObjectModels.RoleModels;
using Core.HttpModels.ObjectModels.SlotModels;
using Core.HttpModels.ObjectModels.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        [JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public async Task<IActionResult> RegisterClinic([FromBody] ClinicRegistrationModel requestObject)
        {
            var clinicsService = HttpContext.RequestServices.GetService<IClinicsService>()!;

            var invoker = (User?) HttpContext.Items["user"];
            
            if (invoker == null)
            {
                return Unauthorized(new HttpResponseModel()
                {
                    StatusCode = 401,
                    Message = "Unauthorized",
                });
            }

            requestObject.OwnerId = invoker.UserId;

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
        [AllowAnonymous]
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
            // Check for invoker 
            User invoker = (User)HttpContext.Items["user"]!;

            var userService = HttpContext.RequestServices.GetService<UserService>()!;

            ClinicStaff? owner = userService.GetClinicStaffInfoById(invoker.UserId); // Getting the invoker information (just for the clinic ID)

            if (owner == null || !owner.IsOwner)
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

        [HttpGet("clinic-schedule")]
        [JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<IHttpResponseModel<List<BookingModel>>> GetClinicBooking([FromQuery] int clinicId)
        {
            // Check for invoker 
            User invoker = (User)HttpContext.Items["user"]!;

            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            ClinicStaff? invokerInfo = userService.GetClinicStaffInfoById(invoker.UserId); // Getting the invoker information (just for the clinic ID)

            if (invokerInfo == null || !invokerInfo.IsOwner || invokerInfo.ClinicId != clinicId)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 401, Message = "Unauthorized", Detail = "You don't have access for this command." });
            }

            // Doing more items
            var clinicService = HttpContext.RequestServices.GetService<IClinicsService>()!;
            var bookingService = HttpContext.RequestServices.GetService<IBookingService>()!;

            if (clinicService.GetClinicInformation(clinicId) == null)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 404, Message = "Failed", Detail = "No clinic found with provided id!" });
            }

            var appointments = bookingService.getClinicBooking(clinicId, true);

            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Succed", Content = appointments });
        }

        [HttpGet("staffs")]
        //[JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<IHttpResponseModel<List<ClinicStaffInfoModel>>> GetAllStaffInfo(int clinicId)
        {
            // Check for invoker 
            User invoker = (User)HttpContext.Items["user"]!;

            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            ClinicStaff? owner = userService.GetClinicStaffInfoById(invoker.UserId); // Getting the invoker information (just for the clinic ID)

            if (owner == null || !owner.IsOwner)
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 401, Message = "Unauthorized", Detail = "You don't have access for this command." });
            }

            var staff = userService.GetAllClinicStaffInfo(clinicId);

            if (staff == null)
            {
                return NotFound(new HttpResponseModel() { StatusCode = 404, Message = "User info not found" });
            }

            var staffInfo = _mapper.Map<IEnumerable<ClinicStaffInfoModel>>(staff);


            return Ok(new HttpResponseModel() { StatusCode = 200, Message = "Succeed",Content = staffInfo });
        }

        [HttpGet("staff")]
        [JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<IHttpResponseModel<ClinicStaffInfoModel>> GetStaffInfo([FromQuery] int staffId)
        {
            var userService = HttpContext.RequestServices.GetService<IUserService>()!;

            // validating invoker
            var invoker = (User)HttpContext.Items["user"]!;

            var invokerInfo = userService.GetClinicStaffInfoById(invoker.UserId);

            if (invokerInfo == null || !invokerInfo.IsOwner)
            {
                return Unauthorized(new HttpResponseModel()
                {
                    StatusCode = 401,
                    Message = "Unauthorized"
                });
            }

            ClinicStaff? staff = userService.GetClinicStaffInfoById(staffId);

            if (staff == null || staff.ClinicId != invokerInfo.ClinicId)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = "User Information not found or is not accessible"
                });
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

        [HttpPost("slots/create")]
        //[JwtTokenAuthorization(RoleModel.Roles.ClinicStaff)]
        public ActionResult<HttpResponseModel> CreateNewClinicSlot([FromBody]ClinicSlotInfoModel slot)
        {
            var bookingService = HttpContext.RequestServices.GetService<IBookingService>()!;

            if (!bookingService.CreateClinicSlot(slot, out var message))
            {
                return BadRequest(new HttpResponseModel() { StatusCode = 400, Message = "Failed", Detail = message });
            }
            _unitOfWork.Save();

            return Ok(new HttpResponseModel() {StatusCode=200, Message="success" });
        }

        [HttpPost("slots/all")]
        //[JwtTokenAuthorization(RoleModel.Roles.Customer, RoleModel.Roles.ClinicStaff)]
        public ActionResult<HttpResponseModel> GetAllClinicSlots([FromQuery] int clinicId)
        {
            var bookingService = HttpContext.RequestServices.GetService<IBookingService>()!;

            var slots = bookingService.GetClinicSlot(clinicId);

            var mappedSlot = new List<ClinicSlotInfoModel>();

            foreach (var slot in slots)
            {
                mappedSlot.Add(_mapper.Map<ScheduledSlot, ClinicSlotInfoModel>(slot));
            }
            return (new HttpResponseModel() { StatusCode = 200, Message = "success", Content = mappedSlot });
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
        [JwtTokenAuthorization(RoleModel.Roles.ClinicStaff, RoleModel.Roles.Admin)]
        public ActionResult<IHttpResponseModel<ClinicInformationModel>> Put(int id,[FromBody]ClinicRegistrationModel info)
        {

            var invoker = (User?) HttpContext.Items["user"]!;

            var userService = HttpContext.RequestServices.GetService<IUserService>()!;
            var clinicsService = HttpContext.RequestServices.GetService<IClinicsService>()!;

            if (invoker.RoleId == 3)
            {
                return Unauthorized(new HttpResponseModel() { StatusCode = 401, Message = "Unauthorized"});
            }

            if (invoker.RoleId == 2)
            {
                ClinicStaff? staffInfo = userService.GetClinicStaffInfoById(invoker.UserId);

                if (staffInfo == null || staffInfo.ClinicId != id || !staffInfo.IsOwner) 
                {
                    return Unauthorized(new HttpResponseModel() { StatusCode = 401, Message = "Unauthorized" });
                }
            }

            var clinicInfo = clinicsService.GetClinicInformation(id);
            if (clinicInfo == null)
            {
                return NotFound(new HttpResponseModel() { StatusCode = 404, Message = "Clinic not found", Detail = "No clinic found with provided ID" });
            }
            else
            {
                var open = info.OpenHour.IsNullOrEmpty() ? clinicInfo.OpenHour : TimeOnly.Parse(info.OpenHour);
                var close = info.CloseHour.IsNullOrEmpty() ? clinicInfo.CloseHour : TimeOnly.Parse(info.CloseHour);
                Clinic newInfo = new Clinic()
                {
                    ClinicId = clinicInfo.ClinicId,
                    Name = info.Name ?? clinicInfo.Address,
                    Email = info.Email ?? clinicInfo.Address,
                    Address = info.Address ?? clinicInfo.Address,
                    OpenHour =  open,
                    CloseHour = close,
                    Description = clinicInfo.Description ?? clinicInfo.Description,
                };
                clinicsService.UpdateClinicInformation(newInfo, out var message);
                _unitOfWork.Save();
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
