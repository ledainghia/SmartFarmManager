using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraSurveillanceController : ControllerBase
    {
        private readonly ICameraSurveillanceService _cameraSurveillanceService;

        public CameraSurveillanceController(ICameraSurveillanceService cameraSurveillanceService)
        {
            _cameraSurveillanceService = cameraSurveillanceService;
        }

        // Implement API endpoints for CameraSurveillance operations here
    }
}
