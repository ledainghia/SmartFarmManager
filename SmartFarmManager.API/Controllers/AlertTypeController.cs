using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertTypeController : ControllerBase
    {
        private readonly IAlertTypeService _alertTypeService;

        public AlertTypeController(IAlertTypeService alertTypeService)
        {
            _alertTypeService = alertTypeService;
        }

        // Implement API endpoints for AlertType operations here
    }
}
