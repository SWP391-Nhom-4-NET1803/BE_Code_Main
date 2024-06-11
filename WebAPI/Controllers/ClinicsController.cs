using Core.HttpModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Models;
using System.Net;
using System.Security.Claims;
using WebAPI.Helper.AuthorizationPolicy;

namespace WebAPI.Controllers
{
    [Route("api/clinic")]
    [ApiController]
    [AllowAnonymous]
    public class ClinicsController : ControllerBase
    {
        private readonly DentalClinicPlatformContext _DbContext;
        private readonly IConfiguration _configuration;
        private readonly UnitOfWork _unitOfWork;

        public ClinicsController(IConfiguration config, DentalClinicPlatformContext context)
        {
            _configuration = config;
            _DbContext = context;
            _unitOfWork = new UnitOfWork(_DbContext);
        }

        [HttpPost]
        [Route("register")]
        [JwtTokenAuthorization]
        public async Task<IActionResult> RegisterClinic([FromBody] ClinicRegistrationModel requestObject)
        {

            // Validate user authorization information
            var user = (User?) HttpContext.Items["user"];

            if (user == null)
            {
                return Ok(new HttpResponseModel() { StatusCode = 401, Message = "You are unauthorized"});
            }

            //Get staff info
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

            if (!_unitOfWork.ClinicRepository.CheckClinicAvailability(requestObject.Name, out var responseMessage))
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
                    Name = requestObject.Name,
                    Address = requestObject.Address,
                    Phone = requestObject.Phone,
                    Email = requestObject.Email,
                    OpenHour = TimeOnly.Parse(requestObject.OpenHour),
                    CloseHour = TimeOnly.Parse(requestObject.CloseHour),
                    Status = true,
                    Owner = _unitOfWork.UserRepository.GetById(staff.UserId)!
                };

                // Add clinic services
                foreach (var serviceId in requestObject.ClinicServices)
                {
                    var service = await _DbContext.Services.FindAsync(serviceId);
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
                staff.Clinic = newClinic;
                _unitOfWork.clinicStaffRepository.Update(staff);
                _unitOfWork.ClinicRepository.Add(newClinic);
                _unitOfWork.Save();

                return Ok(new HttpResponseModel()
                {
                    StatusCode = 202,
                    Message = "Request accepted",
                    Detail = $"Created clinic {newClinic.Name}."
                });
            }
            catch (DbUpdateException dbEx)
            {
                var innerExceptionMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 500,
                    Message = "Internal Server Error",
                    Detail = innerExceptionMessage
                });
            }
            catch (Exception e)
            {
                return Ok(new HttpResponseModel()
                {
                    StatusCode = 500,
                    Message = "Internal Server Error",
                    Detail = e.Message
                });
            }
        }
    }
}
