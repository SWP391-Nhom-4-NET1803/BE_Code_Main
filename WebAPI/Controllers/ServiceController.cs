using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using PlatformRepository.Repositories;
using Repositories.Models;
using System;

namespace WebAPI.Controllers
{
    [Route("api/service")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly DentalClinicPlatformContext _DbContext;
        private readonly GenericRepository<Service, int> _serviceRepository;

        public ServiceController(DentalClinicPlatformContext context)
        {
            _DbContext = context;
            _serviceRepository = new GenericRepository<Service, int>(_DbContext);
        }

        [HttpGet]
        [Route("get-all")]
        public IActionResult GetAllServices()
        {
            try
            {
                var services = _serviceRepository.GetAll();
                if (services == null)
                {
                    return NotFound("No services found.");
                }
                return Ok(services);
            }
            catch (Exception ex)
            {
                return BadRequest("An error happen while processing your request" + ex.Message);
            }
        }
    }
}
