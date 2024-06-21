using ClinicPlatformDTOs.BookingModels;
using ClinicPlatformDTOs.ClinicModels;
using ClinicPlatformServices.Contracts;
using ClinicPlatformWebAPI.Helpers.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

        public BookingController(IBookingService bookingService, IUserService userService, IClinicService clinicService, IScheduleService scheduleService)
        {
            this.bookingService = bookingService;
            this.userService = userService;
            this.clinicService = clinicService;
            this.scheduleService = scheduleService;
        }

        [HttpGet("clinic/{id}")]
        public ActionResult<IHttpResponseModel<IEnumerable<BookingViewModel>>> GetClinicAppointments(int id, DateOnly? from_date, DateOnly? to_date, TimeOnly? from_time, TimeOnly? to_time, bool requestOldItems = true, int? page_size = null, int? page_index = null)
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

        [HttpGet("customer/{id}")]
        public ActionResult<IHttpResponseModel<IEnumerable<BookingViewModel>>> GetCustomerAppointments(int id, DateOnly? from_date, DateOnly? to_date, TimeOnly? from_time, TimeOnly? to_time, bool requestOldItems = true, int? page_size = null, int? page_index = null)
        {
            if (userService.GetCustomerInformation(id) == null)
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

        [HttpGet("staff/{id}")]
        public ActionResult<IHttpResponseModel<IEnumerable<BookingViewModel>>> GetClinicStaffAppointments(int id, DateOnly? from_date, DateOnly? to_date, TimeOnly? from_time, TimeOnly? to_time, bool requestOldItems = true, int? page_size = null, int? page_index = null)
        {
            if (userService.GetClinicStaffInformation(id) == null)
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



        [HttpPost("customer/book")]
        public ActionResult<HttpResponseModel> CreateNewCustomerAppointment([FromBody] BookingRegistrationModel bookInfo)
        {
            // Validate request information (I really want to find a better way)
            var clinicInfo = clinicService.GetClinicWithId(bookInfo.ClinicId);

            if (clinicInfo == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = "The selected clinic does not exist."
                });
            }

            var dentistInfo = userService.GetClinicStaffInformation(bookInfo.DentistId);

            if (dentistInfo == null)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = "The selected dentist does not exist."
                });
            }
            
            if (dentistInfo.ClinicId != clinicInfo.Id)
            {
                return BadRequest(new HttpResponseModel()
                {
                    StatusCode = 400,
                    Message = "Failed",
                    Detail = "Can not book an appointment because the chosen dentist does nto work in the selected clinic."
                });
            }

            if (bookInfo.AppointmentType == BookingRegistrationModel.GeneralBooking)
            {
                bookInfo.ServiceId = null; // General Check-up does not apply services
                bookInfo.IsRecurring = false;
                // bookInfo.RecurringDuration = 0;
            }

            if (bookInfo.AppointmentType == BookingRegistrationModel.ServiceBooking)
            {

            }
            
            if (bookInfo.ServiceId != null)
            {
                var serviceInfo = clinicService.GetClinicServiceWithId((Guid) bookInfo.ServiceId);

                if (serviceInfo == null || serviceInfo.ClinicId != clinicInfo.Id)
                {
                    return BadRequest(new HttpResponseModel()
                    {
                        StatusCode = 400,
                        Message = "Failed",
                        Detail = "The selected service does not exist for the current clinic"
                    });
                }
            }

            if (bookingService.CreateNewBooking(bookInfo, out var message))
            {
                return Created(nameof(CreateNewCustomerAppointment), new HttpResponseModel()
                {
                    StatusCode = 201,
                    Message = "Success"
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

        private BookingViewModel ConvertToBookingView(BookingModel bookModel)
        {

            var clinicInfo = clinicService.GetClinicWithId((int)bookModel.ClinicId!)!;
            var customerInfo = userService.GetCustomerInformation((int)bookModel.CustomerId!)!;
            var dentistInfo = userService.GetClinicStaffInformation((int)bookModel.DentistId!)!;
            var clinicSlotInfo = scheduleService.GetClinicSlotById((Guid)bookModel.TimeSlotId!)!;
            ClinicServiceInfoModel? serviceInfo = null;

            if (bookModel.SelectedService != null)
            {
                serviceInfo = clinicService.GetClinicServiceWithId((Guid)bookModel.SelectedService!)!;
            }
            
            Console.WriteLine(clinicInfo);
            Console.WriteLine(customerInfo);
            Console.WriteLine(dentistInfo);
            Console.WriteLine(serviceInfo);
            Console.WriteLine(clinicSlotInfo.EndTime);

            return new BookingViewModel()
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
