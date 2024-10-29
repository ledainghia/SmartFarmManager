using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertUserController : ControllerBase
    {
        private readonly IAlertUserService _alertUserService;

        public AlertUserController(IAlertUserService alertUserService)
        {
            _alertUserService = alertUserService;
        }

        // Implement API endpoints for AlertUser operations here
    }
}
