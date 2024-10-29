using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceReadingController : ControllerBase
    {
        private readonly IDeviceReadingService _deviceReadingService;

        public DeviceReadingController(IDeviceReadingService deviceReadingService)
        {
            _deviceReadingService = deviceReadingService;
        }

        // Implement API endpoints for DeviceReading operations here
    }
}
