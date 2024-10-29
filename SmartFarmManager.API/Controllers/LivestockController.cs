using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivestockController : ControllerBase
    {
        private readonly ILivestockService _livestockService;

        public LivestockController(ILivestockService livestockService)
        {
            _livestockService = livestockService;
        }

        // Implement API endpoints for Livestock operations here
    }
}
