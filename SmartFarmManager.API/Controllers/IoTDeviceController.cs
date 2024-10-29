using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IoTDeviceController : ControllerBase
    {
        private readonly IIoTDeviceService _ioTDeviceService;

        public IoTDeviceController(IIoTDeviceService ioTDeviceService)
        {
            _ioTDeviceService = ioTDeviceService;
        }

        // Implement API endpoints for IoTDevice operations here
    }
}
