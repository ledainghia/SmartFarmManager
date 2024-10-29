using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivestockExpenseController : ControllerBase
    {
        private readonly ILivestockExpenseService _livestockExpenseService;

        public LivestockExpenseController(ILivestockExpenseService livestockExpenseService)
        {
            _livestockExpenseService = livestockExpenseService;
        }

        // Implement API endpoints for LivestockExpense operations here
    }
}
