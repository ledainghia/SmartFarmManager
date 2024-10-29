using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivestockSaleController : ControllerBase
    {
        private readonly ILivestockSaleService _livestockSaleService;

        public LivestockSaleController(ILivestockSaleService livestockSaleService)
        {
            _livestockSaleService = livestockSaleService;
        }

        // Implement API endpoints for LivestockSale operations here
    }
}
