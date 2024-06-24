using ClinicPlatformServices.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicPlatformWebAPI.Controllers
{
    [Route("api/schedule")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IBookingService bookingService;
        private readonly IScheduleService scheduleService;

        public ScheduleController(IUserService userService, IBookingService bookingService, IScheduleService scheduleService)
        {
            this.userService = userService;
            this.bookingService = bookingService;
            this.scheduleService = scheduleService;
        }

    }
}
