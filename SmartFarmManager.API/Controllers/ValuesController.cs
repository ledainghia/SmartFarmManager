using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ValuesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1️⃣ API tìm user có Role = "Admin"
        [HttpGet("admin")]
        public async Task<IActionResult> GetAdmin()
        {
            var user = await _unitOfWork.Users
                .FindByCondition(u => u.Role.RoleName == "Admin")
                .FirstOrDefaultAsync();

            return user != null ? Ok(user) : NotFound("Admin not found");
        }

        // 2️⃣ API tìm user có Role = "Admin Farm"
        [HttpGet("admin-farm")]
        public async Task<IActionResult> GetAdminFarm()
        {
            var user = await _unitOfWork.Users
                .FindByCondition(u => u.Role.RoleName == "Admin Farm")
                .FirstOrDefaultAsync();

            return user != null ? Ok(user) : NotFound("Admin Farm not found");
        }

        // 3️⃣ API tìm user có Role = "Staff"
        [HttpGet("staff")]
        public async Task<IActionResult> GetStaff()
        {
            var user = await _unitOfWork.Users
                .FindByCondition(u => u.Role.RoleName == "Staff")
                .FirstOrDefaultAsync();

            return user != null ? Ok(user) : NotFound("Staff not found");
        }

        // 4️⃣ API tìm user có Role = "Staff Farm"
        [HttpGet("staff-farm")]
        public async Task<IActionResult> GetStaffFarm()
        {
            var user = await _unitOfWork.Users
                .FindByCondition(u => u.Role.RoleName == "Staff Farm")
                .FirstOrDefaultAsync();

            return user != null ? Ok(user) : NotFound("Staff Farm not found");
        }

        // 5️⃣ API tìm user có Role = "Vet"
        [HttpGet("vet")]
        public async Task<IActionResult> GetVet()
        {
            var user = await _unitOfWork.Users
                .FindByCondition(u => u.Role.RoleName == "Vet")
                .FirstOrDefaultAsync();

            return user != null ? Ok(user) : NotFound("Vet not found");
        }
    }
}
