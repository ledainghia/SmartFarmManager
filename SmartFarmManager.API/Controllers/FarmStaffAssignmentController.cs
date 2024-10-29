using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FarmStaffAssignmentController : ControllerBase
    {
        private readonly IFarmStaffAssignmentService _farmStaffAssignmentService;

        public FarmStaffAssignmentController(IFarmStaffAssignmentService farmStaffAssignmentService)
        {
            _farmStaffAssignmentService = farmStaffAssignmentService;
        }

        // Implement API endpoints for FarmStaffAssignment operations here
    }
}
