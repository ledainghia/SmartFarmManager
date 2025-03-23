using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using System.Diagnostics;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;


        public DatabaseController(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        [HttpDelete("ClearAndRecreate")]
        public async Task<IActionResult> ClearAndRecreateDatabase()
        {
            try
            {
                // Bước 1: Xóa cơ sở dữ liệu hiện tại
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                await DropDatabaseAsync(connectionString);

                // Bước 2: Tạo lại cơ sở dữ liệu với tên mới (farm4)
                await CreateDatabaseAsync(connectionString);

                // Bước 3: Thực hiện Migration lại
                await ApplyMigrationsAsync();

                return Ok("Database has been cleared, recreated and migrations applied successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while clearing, recreating the database, and applying migrations: {ex.Message}");
            }
        }

        // Bước 1: Xóa cơ sở dữ liệu
        private async System.Threading.Tasks.Task DropDatabaseAsync(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SmartFarmContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new SmartFarmContext(optionsBuilder.Options))
            {
                await context.Database.EnsureDeletedAsync(); // Xóa cơ sở dữ liệu hiện tại
            }
        }

        // Bước 2: Tạo lại cơ sở dữ liệu với tên mới
        private async System.Threading.Tasks.Task CreateDatabaseAsync(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SmartFarmContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using (var context = new SmartFarmContext(optionsBuilder.Options))
            {
                await context.Database.EnsureCreatedAsync(); // Tạo lại cơ sở dữ liệu
            }
        }

        // Bước 3: Áp dụng Migration
        private async System.Threading.Tasks.Task ApplyMigrationsAsync()
        {
            // Dùng migration của dự án thông qua EF Core command-line tool
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "ef database update", // Áp dụng migration
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                using (var reader = process.StandardOutput)
                {
                    var output = await reader.ReadToEndAsync();
                    Console.WriteLine(output); // Để debug thông tin
                }
            }
        }
    }
}
