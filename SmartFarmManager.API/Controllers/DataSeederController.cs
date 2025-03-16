using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.DataAccessObject.Models;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSeederController : ControllerBase
    {
        private readonly SmartFarmContext _context;

        public DataSeederController(SmartFarmContext context)
        {
            _context = context;
        }

        [HttpPost("seed/TempleteChicken")]
        public IActionResult SeedDataTempleteChicken()
        {
            try
            {
                // Kiểm tra nếu đã có dữ liệu thì không thêm nữa
                if (_context.AnimalTemplates.Any())
                {
                    return BadRequest("Dữ liệu đã tồn tại trong hệ thống.");
                }

                // 1. Thêm loại gà (ChickenTemplate)
                var chickenTemplate = new AnimalTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = "Gà nuôi thịt",
                    Species = "Gà thịt công nghiệp",
                    Status = "Active",
                    Notes = "Gà nuôi theo mô hình công nghiệp, có các giai đoạn phát triển cụ thể"
                };
                _context.AnimalTemplates.Add(chickenTemplate);
                _context.SaveChanges();

                // 2. Thêm các giai đoạn phát triển (GrowthStageTemplate)
                var growthStages = new List<GrowthStageTemplate>
                {
                    new GrowthStageTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, StageName = "Gà con", WeightAnimal = 0.2m, AgeStart = 1, AgeEnd = 7, Notes = "Giai đoạn gà mới nở, cần được úm cẩn thận" },
                    new GrowthStageTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, StageName = "Gà tơ", WeightAnimal = 0.8m, AgeStart = 8, AgeEnd = 21, Notes = "Giai đoạn phát triển nhanh, thay lông tơ bằng lông cứng" },
                    new GrowthStageTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, StageName = "Gà trưởng thành", WeightAnimal = 2.5m, AgeStart = 22, AgeEnd = 90, Notes = "Giai đoạn gà sắp xuất chuồng, đạt trọng lượng tối đa" }
                };

                _context.GrowthStageTemplates.AddRange(growthStages);
                _context.SaveChanges();

                // 3. Thêm thức ăn theo từng giai đoạn (FoodTemplate)
                var foodTemplates = new List<FoodTemplate>
                {
                    new FoodTemplate { Id = Guid.NewGuid(), StageTemplateId = growthStages[0].Id, FoodType = "Dạng mảnh", WeightBasedOnBodyMass = 0.2m },
                    new FoodTemplate { Id = Guid.NewGuid(), StageTemplateId = growthStages[1].Id, FoodType = "Dạng viên", WeightBasedOnBodyMass = 0.5m },
                    new FoodTemplate { Id = Guid.NewGuid(), StageTemplateId = growthStages[2].Id, FoodType = "Dạng viên", WeightBasedOnBodyMass = 0.8m }
                };
                _context.FoodTemplates.AddRange(foodTemplates);
                _context.SaveChanges();

                // 4. Thêm lịch vaccine (VaccineTemplate)
                var vaccineTemplates = new List<VaccineTemplate>
                {
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "Marek", ApplicationMethod = "Tiêm dưới da", ApplicationAge = 1, Session = 1 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "IB - H120", ApplicationMethod = "Nhỏ mắt, nhỏ mũi", ApplicationAge = 1, Session = 1 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "ND IB H120 Lasota", ApplicationMethod = "Nhỏ mắt, nhỏ mũi", ApplicationAge = 3, Session = 1 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "Đậu gà", ApplicationMethod = "Tiêm chủng dưới da cánh", ApplicationAge = 7, Session = 1 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "Gumboro int", ApplicationMethod = "Nhỏ mắt", ApplicationAge = 7, Session = 1 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "Gumboro plus", ApplicationMethod = "Pha nước uống", ApplicationAge = 14, Session = 3 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "Cúm", ApplicationMethod = "Tiêm dưới da", ApplicationAge = 17, Session = 3 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "ND - HB1", ApplicationMethod = "Nhỏ mắt, nhỏ mũi", ApplicationAge = 21, Session = 3 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "ND - M", ApplicationMethod = "Tiêm dưới da", ApplicationAge = 50, Session = 3 },
                    new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chickenTemplate.Id, VaccineName = "Avac-Clone Entero", ApplicationMethod = "Pha nước uống", ApplicationAge = 70, Session = 3 }
                };

                // 5. Thêm vào danh sách vaccine (Vaccine)
                var vaccines = new List<Vaccine>
                {
                    new Vaccine { Id = Guid.NewGuid(), Name = "Marek", Method = "Tiêm dưới da", Price = 500, AgeStart = 1, AgeEnd = 1 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "IB - H120", Method = "Nhỏ mắt, nhỏ mũi", Price = 700, AgeStart = 1, AgeEnd = 1 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "ND IB H120 Lasota", Method = "Nhỏ mắt, nhỏ mũi", Price = 200, AgeStart = 3, AgeEnd = 5 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Đậu gà", Method = "Tiêm chủng dưới da cánh", Price = 300, AgeStart = 7, AgeEnd = 7 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Gumboro int", Method = "Nhỏ mắt", Price = 250, AgeStart = 7, AgeEnd = 7 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Gumboro plus", Method = "Pha nước uống", Price = 350, AgeStart = 14, AgeEnd = 14 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Cúm", Method = "Tiêm dưới da", Price = 400, AgeStart = 17, AgeEnd = 17 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "ND - HB1", Method = "Nhỏ mắt, nhỏ mũi", Price = 220, AgeStart = 21, AgeEnd = 21 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "ND - M", Method = "Tiêm dưới da", Price = 450, AgeStart = 50, AgeEnd = 50 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Avac-Clone Entero", Method = "Pha nước uống", Price = 380, AgeStart = 70, AgeEnd = 70 }
                };

                _context.VaccineTemplates.AddRange(vaccineTemplates);
                _context.SaveChanges();

                return Ok("Dữ liệu đã được nhập vào thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi nhập dữ liệu: {ex.Message}");
            }
        }
    }
}
