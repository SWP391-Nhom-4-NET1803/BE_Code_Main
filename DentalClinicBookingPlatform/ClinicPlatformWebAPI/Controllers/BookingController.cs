using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformDTOs.SlotModels;
using ClinicPlatformObjects.BookingModels;
using ClinicPlatformObjects.UserModels.DentistModel;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.ModelMapper;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/booking")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService bookingService;
        private readonly IUserService userService;
        private readonly IClinicService clinicService;
        private readonly IScheduleService scheduleService;
        private readonly IClinicServiceService clinicServiceServivce;

        public BookingController(IBookingService bookingService, IUserService userService, IClinicService clinicService, IScheduleService scheduleService, IClinicServiceService clinicServiceService)
        {
            this.bookingService = bookingService;
            this.userService = userService;
            this.clinicService = clinicService;
            this.scheduleService = scheduleService;
            this.clinicServiceServivce = clinicServiceService; 
        }

        [HttpGet("clinic/{id}")]
        public ActionResult<IHttpResponseModel<IEnumerable<AppointmentViewModel>>> GetClinicAppointments(int id, DateOnly? from_date, DateOnly? to_date, TimeOnly? from_time, TimeOnly? to_time, bool requestOldItems = true, int? page_size = null, int? page_index = null)
        {
            if (clinicService.GetClinicWithId(id) == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = $"Unknown clinic with Id {id}"
                });
            }

            var result = bookingService.GetAllClinicBooking(id);
            result = bookingService.FilterBookList(result, from_date, to_date, requestOldItems, page_size, page_index);

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Content = from item in result select ConvertToBookingView(item)
            });
        }

        [HttpGet("staff/{id}")]
        public ActionResult<IHttpResponseModel<IEnumerable<AppointmentViewModel>>> GetClinicStaffAppointments(int id, DateOnly? from_date, DateOnly? to_date, TimeOnly? from_time, TimeOnly? to_time, bool requestOldItems = true, int? page_size = null, int? page_index = null)
        {
            if (userService.GetUserWithDentistId(id) == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = $"Unknown clinic staff with staff Id {id}"
                });
            }

            var result = bookingService.GetAllDentistBooking(id);

            result = bookingService.FilterBookList(result, from_date, to_date, requestOldItems, page_size, page_index);

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Content = from item in result select ConvertToBookingView(item)
            });
        }

        [HttpGet("customer/{id}")]
        public ActionResult<IHttpResponseModel<IEnumerable<AppointmentViewModel>>> GetCustomerAppointments(int id, DateOnly? from_date, DateOnly? to_date, TimeOnly? from_time, TimeOnly? to_time, bool requestOldItems = true, int? page_size = null, int? page_index = null)
        {
            if (userService.GetUserWithCustomerId(id) == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = $"Unknown customer with customer Id {id}"
                });
            }

            var result = bookingService.GetAllCustomerBooking(id);
            result = bookingService.FilterBookList(result, from_date, to_date, requestOldItems, page_size, page_index);

            return Ok(new HttpResponseModel()
            {
                StatusCode = 200,
                Message = "Success",
                Content = from item in result select ConvertToBookingView(item)
            });
        }


        [HttpPost("staff/create-schedule")]
        public ActionResult<HttpResponseModel> CreateNewSchedule([FromBody] AppointmentRegistrationModel bookInfo, [FromQuery] AppointmentSetting setting)
        {
            AppointmentInfoModel? appointment = bookingService.CreateNewBooking(bookInfo, out var message);

            if (appointment != null)
            {
                DateOnly originalDate = appointment.AppointmentDate;

                var TotalDate = setting.RepeatYear * 365 + setting.RepeatMonth * 30 + setting.RepeatWeek * 7 + setting.RepeatDay;

                for (int i = 0; i < setting.MaxRecurring; i++)
                {
                    originalDate = originalDate.AddDays(TotalDate);
                    var tempt = AppointmentMapper.MapToAppointment(bookInfo);
                    tempt.OriginalAppoinment = appointment.Id;

                    bookingService.CreateNewBooking(bookInfo, out message);
                }

                return Ok(new HttpResponseModel()
                {
                    StatusCode = 200,
                    Message = "Success",
                    Detail = "Created schedule!"
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = message
                });
            }
        }

        [HttpPost("customer/book")]
        public ActionResult<HttpResponseModel> CreateNewCustomerAppointment([FromBody] AppointmentRegistrationModel bookInfo)
        {
            bookInfo.Status = "pending";
            AppointmentInfoModel? appointment = bookingService.CreateNewBooking(bookInfo, out var message);

            if (appointment != null)
            {
                return Created(nameof(CreateNewCustomerAppointment), new HttpResponseModel()
                {
                    StatusCode = 201,
                    Message = "Success",
                    Detail = $"Created a booking for customer {appointment.CustomerId}!",
                    Content = appointment
                });
            }
            else
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = message
                });
            }
        }

        [HttpGet("available/{clinicId}/service")]
        public ActionResult<IHttpResponseModel<IEnumerable<DentistInfoViewModel>>> GetClinicService(int clinicId)
        {
            if (clinicService.GetClinicWithId(clinicId) == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = "Not found",
                    Detail = $"No clinic with Id {clinicId} was found.",
                });
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = "Success",
                Content = clinicServiceServivce.GetAllClinicService(clinicId).Where(x => x.Available && !x.Removed)
            });
        }

        [HttpGet("available/{clinicId}/dentist")]
        public ActionResult<IHttpResponseModel<IEnumerable<DentistInfoViewModel>>> GetFreeSlotOn(int clinicId)
        {
            if (clinicService.GetClinicWithId(clinicId) == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = "Not found",
                    Detail = $"No clinic with Id {clinicId} was found.",
                });
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = "Success",
                Content = from dentist in userService.GetAllUserWithClinicId(clinicId).Where(x => x.IsActive && !x.IsRemoved) select UserInfoMapper.ToDentistView(dentist)
            });
        }

        [HttpGet("available/weekly")]
        public ActionResult<IHttpResponseModel<Dictionary<int, List<ClinicSlotInfoModel>>>> GetDate(int clinicId)
        {
            if (clinicService.GetClinicWithId(clinicId) == null)
            {
                return BadRequest(new HttpResponseModel
                {
                    StatusCode = 400,
                    Message = "Not found",
                    Detail = $"No clinic with Id {clinicId} was found.",
                });
            }

            var availableSlot = scheduleService.GetAllClinicSlot(clinicId).Where(x => x.Status).OrderBy(x => x.Weekday).ThenBy(x => x.StartTime);

            // Good ol dictionary...
            Dictionary<int, List<ClinicSlotInfoModel>> clinicSlotByWeekday = new Dictionary<int, List<ClinicSlotInfoModel>>();

            foreach (var slot in availableSlot)
            {

                if (!clinicSlotByWeekday.ContainsKey(slot.Weekday))
                {
                    clinicSlotByWeekday.Add(slot.Weekday, new List<ClinicSlotInfoModel>() { slot });
                }
                else
                {
                    clinicSlotByWeekday[slot.Weekday].Add(slot);
                }
                        
            }

            return Ok(new HttpResponseModel
            {
                StatusCode = 200,
                Message = "Success",
                Detail = $"Found {availableSlot.Count()} available slot.",
                Content = clinicSlotByWeekday
            });
        }

        private AppointmentViewModel ConvertToBookingView(AppointmentInfoModel bookModel)
        {

            var clinicInfo = clinicService.GetClinicWithId(bookModel.ClinicId!)!;
            var customerInfo = userService.GetUserWithCustomerId(bookModel.CustomerId!)!;
            var dentistInfo = userService.GetUserWithDentistId(bookModel.DentistId!)!;
            var clinicSlotInfo = scheduleService.GetClinicSlotById(bookModel.ClinicSlotId!)!;

            BookedServiceInfoModel? service = bookingService.GetBookedService(bookModel.Id);
            ClinicServiceInfoModel? serviceInfo = null;

            if (service == null)
            {

            }
            else
            {
                serviceInfo = clinicServiceServivce.GetClinicService(service.ClinicServiceId);
            }

            return new AppointmentViewModel()
            {
                BookId = bookModel.Id,
                ClinicName = clinicInfo.Name!,
                ClinicAddress = clinicInfo.Address!,
                ClinicPhone = clinicInfo.Phone!,
                appointmentType = bookModel.Type!,
                CustomerFullName = customerInfo.Fullname!,
                DentistFullname = dentistInfo.Fullname!,
                AppointmentDate = (DateOnly)bookModel.AppointmentDate!,
                AppointmentTime = (TimeOnly)clinicSlotInfo.StartTime!,
                ExpectedEndTime = (TimeOnly)clinicSlotInfo.EndTime!,
                SelectedServiceName = serviceInfo?.Name ?? "No service"
            };
        }
    }
}
