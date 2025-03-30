using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.SensorDataLog;
using SmartFarmManager.Service.BusinessModels.Webhook;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Shared;

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
                // Danh sách TaskType với ID, Tên và Mức độ ưu tiên
                var taskTypes = new List<TaskType>
                {
                    new TaskType { Id = Guid.Parse("F1124086-2B16-4D3F-B522-0AE43EB528AC"), TaskTypeName = "Bán vật nuôi", PriorityNum = 10 },
                    new TaskType { Id = Guid.Parse("468E7FDE-6DD4-48BD-9358-6168D2840FBD"), TaskTypeName = "Cho uống thuốc", PriorityNum = 3 },
                    new TaskType { Id = Guid.Parse("51C09974-49C9-4484-8620-63D0FAA3876B"), TaskTypeName = "Cân", PriorityNum = 6 },
                    new TaskType { Id = Guid.Parse("CC9E8354-F0DF-481B-ADBB-694F810E68EA"), TaskTypeName = "Thu hoạch", PriorityNum = 7 },
                    new TaskType { Id = Guid.Parse("6C5EF3C3-449B-431D-8996-76942057F936"), TaskTypeName = "Nhập vật nuôi", PriorityNum = 9 },
                    new TaskType { Id = Guid.Parse("88F4EDE7-3AFC-45B1-83CC-77D2EBE43B64"), TaskTypeName = "Phun thuốc sát trùng", PriorityNum = 5 },
                    new TaskType { Id = Guid.Parse("5772F17E-088C-458D-8808-7CACB5E5103E"), TaskTypeName = "Cho ăn", PriorityNum = 1 },
                    new TaskType { Id = Guid.Parse("E01A648B-FCF3-4933-BBBA-80A1B71C8EF2"), TaskTypeName = "Nhập thức ăn", PriorityNum = 11 },
                    new TaskType { Id = Guid.Parse("646143A0-EB0A-42BF-A308-99896E3C6476"), TaskTypeName = "Dọn chuồng", PriorityNum = 4 },
                    new TaskType { Id = Guid.Parse("6AD005A1-1764-4836-A70F-B589A095231C"), TaskTypeName = "Sửa chữa chuồng trại", PriorityNum = 8 },
                    new TaskType { Id = Guid.Parse("A426BF57-E6F8-4FC5-BCE4-BE39BBFE9930"), TaskTypeName = "Bán trứng", PriorityNum = 12 },
                    new TaskType { Id = Guid.Parse("23D4BEF2-D68C-4F8D-B9AA-D4AFB9FCCF4E"), TaskTypeName = "Tiêm vắc xin", PriorityNum = 2 }
                };

                _context.TaskTypes.AddRange(taskTypes);
                _context.SaveChanges();
                // Kiểm tra nếu đã có dữ liệu thì không thêm nữa
                if (_context.AnimalTemplates.Any())
                {
                    return BadRequest("Dữ liệu đã tồn tại trong hệ thống.");
                }

                // 1. Thêm loại gà (ChickenTemplate)
                var chickenTemplates = new List<AnimalTemplate>
                {
                    new AnimalTemplate { Id = Guid.NewGuid(), Name = "Gà nuôi thịt - Cobb-500", Species = "Cobb-500", Status = "Active", Notes = "Gà thịt phát triển nhanh" },
                    new AnimalTemplate { Id = Guid.NewGuid(), Name = "Gà nuôi thịt - Cobb-700", Species = "Cobb-700", Status = "Active", Notes = "Gà thịt phát triển nhanh" },
                    new AnimalTemplate { Id = Guid.NewGuid(), Name = "Gà nuôi thịt - Ross-308", Species = "Ross-308", Status = "Active", Notes = "Gà thịt năng suất cao" },
                    new AnimalTemplate { Id = Guid.NewGuid(), Name = "Gà nuôi thịt - Ross-708", Species = "Ross-708", Status = "Active", Notes = "Gà thịt năng suất cao" }
                };
                _context.AnimalTemplates.AddRange(chickenTemplates);
                _context.SaveChanges();

                // 2. Thêm các giai đoạn phát triển (GrowthStageTemplate)
                var growthStages = new List<GrowthStageTemplate>();
                foreach (var chicken in chickenTemplates)
                {
                    growthStages.Add(new GrowthStageTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, StageName = "Gà con", WeightAnimal = 0.2m, AgeStart = 1, AgeEnd = 7, Notes = "Giai đoạn đầu phát triển" });
                    growthStages.Add(new GrowthStageTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, StageName = "Gà tơ", WeightAnimal = 0.9m, AgeStart = 8, AgeEnd = 21, Notes = "Giai đoạn phát triển nhanh" });
                    growthStages.Add(new GrowthStageTemplate
                    {
                        Id = Guid.NewGuid(),
                        TemplateId = chicken.Id,
                        StageName = "Gà trưởng thành",
                        WeightAnimal = 3.0m,
                        AgeStart = 22,
                        AgeEnd = (chicken.Species.Contains("Cobb") ? 50 : 60),
                        Notes = "Giai đoạn trưởng thành"
                    });
                }

                _context.GrowthStageTemplates.AddRange(growthStages);
                _context.SaveChanges();

                // 3. Thêm thức ăn theo từng giai đoạn (FoodTemplate)
                var foodTemplates = growthStages.Select(stage => new FoodTemplate
                {
                    Id = Guid.NewGuid(),
                    StageTemplateId = stage.Id,
                    FoodType = stage.StageName == "Gà con" ? "Dạng mảnh" : "Dạng viên",
                    WeightBasedOnBodyMass = stage.StageName == "Gà con" ? 0.17m :
                                            stage.StageName == "Gà tơ" ? 0.13m : 0.07m
                }).ToList();
                _context.FoodTemplates.AddRange(foodTemplates);
                _context.SaveChanges();

                // Danh sách TaskDailyTemplate để thêm vào
                var taskDailyTemplates = new List<TaskDailyTemplate>();
                Guid taskTypeId = Guid.Parse("5772F17E-088C-458D-8808-7CACB5E5103E"); // TaskType cho "Cho ăn"

                foreach (var stage in growthStages)
                {
                    List<int> sessionNumbers = stage.StageName == "Gà con" ? new List<int> { 1, 2, 3, 4 } : new List<int> { 1, 2, 4 };

                    foreach (var session in sessionNumbers)
                    {
                        taskDailyTemplates.Add(new TaskDailyTemplate
                        {
                            Id = Guid.NewGuid(),
                            GrowthStageTemplateId = stage.Id,
                            TaskTypeId = taskTypeId,
                            TaskName = "Cho ăn",
                            Description = $"Giai đoạn {stage.StageName} - Bữa {session}",
                            Session = session
                        });
                    }
                }

                _context.TaskDailyTemplates.AddRange(taskDailyTemplates);
                _context.SaveChanges();
                // 4. Thêm lịch vaccine (VaccineTemplate)
                var vaccineTemplates = new List<VaccineTemplate>();
                foreach (var chicken in chickenTemplates)
                {
                    vaccineTemplates.AddRange(new List<VaccineTemplate>
                    {
                        new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "Marek", ApplicationMethod = "Tiêm dưới da", ApplicationAge = 1, Session = 1 },
                        new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "ND IB H120 Lasota", ApplicationMethod = "Nhỏ mắt, nhỏ mũi", ApplicationAge = 3, Session = 1 },
                        new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "Đậu gà", ApplicationMethod = "Tiêm dưới da cánh", ApplicationAge = 7, Session = 1 },
                        new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "Gumboro int", ApplicationMethod = "Nhỏ mắt", ApplicationAge = 7, Session = 1 },
                        new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "Gumboro plus", ApplicationMethod = "Pha nước uống", ApplicationAge = 10, Session = 3 },
                        new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "Cúm", ApplicationMethod = "Tiêm dưới da", ApplicationAge = 15, Session = 3 },
                        new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "ND - HB1", ApplicationMethod = "Nhỏ mắt, nhỏ mũi", ApplicationAge = 21, Session = 3 },
                        new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "ND - M", ApplicationMethod = "Tiêm dưới da", ApplicationAge = 50, Session = 3 }
                    });

                    //if (chicken.Species.Contains("Ross")) // Chỉ áp dụng cho Ross-308 & Ross-708
                    //{
                    //    vaccineTemplates.Add(new VaccineTemplate { Id = Guid.NewGuid(), TemplateId = chicken.Id, VaccineName = "Avac-Clone Entero", ApplicationMethod = "Pha nước uống", ApplicationAge = 70, Session = 3 });
                    //}
                }


                _context.VaccineTemplates.AddRange(vaccineTemplates);
                _context.SaveChanges();
                // 5. Thêm vào danh sách vaccine (Vaccine)
                var vaccines = new List<Vaccine>
                {
                    new Vaccine { Id = Guid.NewGuid(), Name = "Marek", Method = "Tiêm dưới da", Price = 500, AgeStart = 1, AgeEnd = 1 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "ND IB H120 Lasota", Method = "Nhỏ mắt, nhỏ mũi", Price = 200, AgeStart = 3, AgeEnd = 5 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Đậu gà", Method = "Tiêm chủng dưới da cánh", Price = 300, AgeStart = 7, AgeEnd = 7 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Gumboro int", Method = "Nhỏ mắt", Price = 250, AgeStart = 7, AgeEnd = 7 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Gumboro plus", Method = "Pha nước uống", Price = 350, AgeStart = 1, AgeEnd = 14 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Cúm", Method = "Tiêm dưới da", Price = 400, AgeStart = 15, AgeEnd = 17 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "ND - HB1", Method = "Nhỏ mắt, nhỏ mũi", Price = 220, AgeStart = 21, AgeEnd = 21 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "ND - M", Method = "Tiêm dưới da", Price = 450, AgeStart = 50, AgeEnd = 50 },
                    new Vaccine { Id = Guid.NewGuid(), Name = "Avac-Clone Entero", Method = "Pha nước uống", Price = 380, AgeStart = 70, AgeEnd = 70 }
                };

                _context.Vaccines.AddRange(vaccines);
                _context.SaveChanges();

                // 1. Thêm triệu chứng
                var symptoms = new List<Symptom>
                {
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Xù lông" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Ủ rũ, kém hoạt động" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Giảm ăn, bỏ ăn" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Ho, khò khè, khó thở" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Thở gấp" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Mào, tích sưng to, tím tái" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Mắt sưng, viêm giác mạc" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Da khô, bong vảy" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Xuất hiện mụn đậu trên mào, mặt, chân" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Mụn loét, có vảy cứng" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Khớp chân sưng, đi lại khó khăn" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Tiêu chảy phân xanh" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Tiêu chảy phân trắng" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Tiêu chảy phân đỏ (có máu)" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Tiêu chảy phân loãng, có mùi hôi" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Tiêu chảy mãn tính" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Lắc đầu liên tục" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Há miệng thở" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Bụng căng tròn, phình to" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Bụng chướng to" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Xương mềm, biến dạng" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Gà chậm lớn, còi cọc" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Lông xơ xác, gãy lông" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Vỏ trứng mỏng, mềm" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Đẻ trứng kém, vỏ trứng méo mó" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Sưng phù vùng hậu môn" },
                    new Symptom { Id = Guid.NewGuid(), SymptomName = "Phù mặt" }
                };
                _context.Symptoms.AddRange(symptoms);
                _context.SaveChanges();

                // 2. Thêm bệnh
                var diseases = new List<Disease>
                {
                    new Disease { Id = Guid.NewGuid(), Name = "Dịch tả gà", Description = "Bệnh do virus Newcastle gây tổn thương hô hấp, thần kinh và tiêu hóa ở gà" },
                    new Disease { Id = Guid.NewGuid(), Name = "Viêm khí quản truyền nhiễm", Description = "Bệnh do virus IB gây ho, khò khè, khó thở, chảy nước mũi" },
                    new Disease { Id = Guid.NewGuid(), Name = "Gumboro", Description = "Bệnh do virus IBD làm giảm miễn dịch ở gà con, gây tiêu chảy và xù lông" },
                    new Disease { Id = Guid.NewGuid(), Name = "Cúm gia cầm", Description = "Bệnh truyền nhiễm nguy hiểm, gây viêm hô hấp, mào tím tái, tiêu chảy" },
                    new Disease { Id = Guid.NewGuid(), Name = "Đậu gà", Description = "Bệnh do virus gây mụn đậu trên da, miệng và đường hô hấp" },
                    new Disease { Id = Guid.NewGuid(), Name = "Viêm não tủy gia cầm", Description = "Bệnh do virus AE gây bại liệt, run rẩy, giảm ăn" },
                    new Disease { Id = Guid.NewGuid(), Name = "Viêm khớp do Reovirus", Description = "Bệnh gây viêm khớp, sưng chân, khó đi lại" },
                    new Disease { Id = Guid.NewGuid(), Name = "Nhiễm trùng Adenovirus", Description = "Bệnh gây viêm gan, suy giảm miễn dịch" },
                    new Disease { Id = Guid.NewGuid(), Name = "Bệnh Marek", Description = "Bệnh do virus gây bại liệt, mù mắt, hình thành khối u" },
                    new Disease { Id = Guid.NewGuid(), Name = "Lymphoid leukosis", Description = "Bệnh ung thư máu ở gà, gây còi cọc, sụt cân" },
                    new Disease { Id = Guid.NewGuid(), Name = "Tụ huyết trùng", Description = "Bệnh do vi khuẩn gây sưng tích, mào tím tái, tiêu chảy phân xanh" },
                    new Disease { Id = Guid.NewGuid(), Name = "Thương hàn", Description = "Bệnh do Salmonella gây tiêu chảy phân trắng, viêm khớp chân" },
                    new Disease { Id = Guid.NewGuid(), Name = "E. coli", Description = "Bệnh do vi khuẩn đường ruột gây tiêu chảy, sưng phù bụng" },
                    new Disease { Id = Guid.NewGuid(), Name = "Bệnh CRD", Description = "Bệnh do Mycoplasma gây ho, khó thở, chảy nước mũi kéo dài" },
                    new Disease { Id = Guid.NewGuid(), Name = "Viêm ruột hoại tử", Description = "Bệnh do vi khuẩn Clostridium gây tiêu chảy phân đen, có mùi hôi" },
                    new Disease { Id = Guid.NewGuid(), Name = "Ngộ độc thịt", Description = "Bệnh do vi khuẩn Botulism gây liệt cơ, khó thở" },
                    new Disease { Id = Guid.NewGuid(), Name = "Nhiễm nấm", Description = "Bệnh do nấm gây viêm niêm mạc, tiêu chảy" },
                    new Disease { Id = Guid.NewGuid(), Name = "Cầu trùng", Description = "Bệnh ký sinh trùng gây tiêu chảy phân đỏ, sụt cân" },
                    new Disease { Id = Guid.NewGuid(), Name = "Giun khí quản", Description = "Ký sinh trùng trong khí quản làm gà khó thở, há miệng thở" },
                    new Disease { Id = Guid.NewGuid(), Name = "Gout nội tạng", Description = "Do rối loạn chuyển hóa, gây sưng khớp, chậm lớn" },
                    new Disease { Id = Guid.NewGuid(), Name = "Hội chứng báng nước", Description = "Do rối loạn tuần hoàn, gây bụng phình to, khó thở" },
                    new Disease { Id = Guid.NewGuid(), Name = "Thiếu Vitamin A", Description = "Gây khô mắt, giảm miễn dịch" },
                    new Disease { Id = Guid.NewGuid(), Name = "Thiếu Vitamin D", Description = "Gây loãng xương, mềm xương, giảm tỷ lệ đẻ" },
                    new Disease { Id = Guid.NewGuid(), Name = "Thiếu Canxi & Photpho", Description = "Xương mềm, bại liệt" },
                    new Disease { Id = Guid.NewGuid(), Name = "Thiếu Protein", Description = "Chậm lớn, lông xơ xác" }
                };
                _context.Diseases.AddRange(diseases);
                _context.SaveChanges();


                // Danh sách SaleType với ID, Tên và Mô tả
                var saleTypes = new List<SaleType>
                {
                    new SaleType { Id = Guid.Parse("439f849a-785f-43c5-9176-49b1da6e5080"), StageTypeName = "EggSale", Discription = "Giai đoạn bán trứng gà" },
                    new SaleType { Id = Guid.Parse("50aa6910-b1b1-48ae-beed-f345c24ddb63"), StageTypeName = "MeatSale", Discription = "Giai đoạn bán thịt gà" }
                };

                _context.SaleTypes.AddRange(saleTypes);
                _context.SaveChanges();


                var roles = new List<Role>
                {
                    new Role { Id = Guid.Parse("3c1ef196-e428-4951-83e1-b640a08d3bfb"), RoleName = "Admin" },
                    new Role { Id = Guid.Parse("544865bf-c00d-4e6f-9d05-f32f9d9cc468"), RoleName = "Admin Farm" },
                    new Role { Id = Guid.Parse("70702de9-89bc-48e5-861e-f4c1a5ac01d8"), RoleName = "Staff" },
                    new Role { Id = Guid.Parse("63f38a5f-6a4c-4006-9e20-73a89c1d3940"), RoleName = "Staff Farm" },
                    new Role { Id = Guid.Parse("b833e6cd-6c06-4daa-aa6e-b5ed5f64dda0"), RoleName = "Vet" },
                    new Role { Id = Guid.Parse("E8C551C1-F509-4191-91CE-764370E86278"), RoleName = "GOD" }
                };
                _context.Roles.AddRange(roles);
                _context.SaveChanges();

                var users = new List<User>
                {
                    new User { Id = Guid.Parse("babc4332-d7d8-457b-af12-765a992c4314"), Username = "admin", PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5", FullName = "Admin User", Email = "admin@farm.com", PhoneNumber = "0123456789", Address = "Admin Address", IsActive = true, CreatedAt = DateTime.UtcNow, RoleId = Guid.Parse("3c1ef196-e428-4951-83e1-b640a08d3bfb") }, // Admin
                    new User { Id = Guid.Parse("8dac47e4-58b6-43ef-aac8-c9c4315bd4e0"), Username = "staff", PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5", FullName = "Farm Staff", Email = "staff@farm.com", PhoneNumber = "0987654321", Address = "Staff Address", IsActive = true, CreatedAt = DateTime.UtcNow, RoleId = Guid.Parse("70702de9-89bc-48e5-861e-f4c1a5ac01d8") }, // Staff
                    new User { Id = Guid.Parse("ad28b5ad-e1f4-4bf8-b7de-00859235a3f8"), Username = "vet_farm", PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5", FullName = "Veterinary Doctor", Email = "vet@farm.com", PhoneNumber = "0123987654", Address = "Vet Address", IsActive = true, CreatedAt = DateTime.UtcNow, RoleId = Guid.Parse("b833e6cd-6c06-4daa-aa6e-b5ed5f64dda0") }, // Vet
                    new User { Id = Guid.Parse("a406f2a5-f7f7-4701-9c21-339b16c06f76"), Username = "admin_farm", PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5", FullName = "Farm Admin", Email = "adminfarm@farm.com", PhoneNumber = "0123567890", Address = "Farm Admin Address", IsActive = true, CreatedAt = DateTime.UtcNow, RoleId = Guid.Parse("544865bf-c00d-4e6f-9d05-f32f9d9cc468") }, // Admin Farm
                    new User { Id = Guid.Parse("b8a28787-9d97-4849-949a-56ebfc6d5de0"), Username = "staff_farm_1", PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5", FullName = "Farm Staff 1", Email = "stafffarm1@farm.com", PhoneNumber = "0987123456", Address = "Staff Farm Address 1", IsActive = true, CreatedAt = DateTime.UtcNow, RoleId = Guid.Parse("63f38a5f-6a4c-4006-9e20-73a89c1d3940") }, // Staff Farm 1
                    new User { Id = Guid.Parse("54da1a44-d865-4d41-bf65-f8fc3e939d25"), Username = "staff_farm_2", PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5", FullName = "Farm Staff 2", Email = "stafffarm2@farm.com", PhoneNumber = "0978123456", Address = "Staff Farm Address 2", IsActive = true, CreatedAt = DateTime.UtcNow, RoleId = Guid.Parse("63f38a5f-6a4c-4006-9e20-73a89c1d3940") }, // Staff Farm 2
                    new User { Id = Guid.NewGuid(), Username = "GOD_farm", PasswordHash = "5994471abb01112afcc18159f6cc74b4f511b99806da59b3caf5a9c173cacfc5", FullName = "GOD", Email = "GOD@farm.com", PhoneNumber = "09781245634", Address = "GOD Address", IsActive = true, CreatedAt = DateTime.UtcNow, RoleId = Guid.Parse("E8C551C1-F509-4191-91CE-764370E86278") } // God
                
                };

                _context.Users.AddRange(users);
                _context.SaveChanges();



                var notificationTypes = new List<DataAccessObject.Models.NotificationType>
                {
                    new DataAccessObject.Models.NotificationType { Id = Guid.Parse("800bd31c-4fcb-4d4d-a462-55c4a70c1e7d"), NotiTypeName = "Alert" },
                    new DataAccessObject.Models.NotificationType { Id = Guid.Parse("4245669d-361d-4c8b-bc76-cfbd1961505b"), NotiTypeName = "MedicalSymptom" },
                    new DataAccessObject.Models.NotificationType { Id = Guid.Parse("05904def-aedc-421a-86cb-93226c2e08ad"), NotiTypeName = "Task" },
                    new DataAccessObject.Models.NotificationType { Id = Guid.NewGuid(), NotiTypeName = "FarmingBatchSchedule" }
                };

                _context.NotificationTypes.AddRange(notificationTypes);
                _context.SaveChanges();

                var farm = new Farm
                {
                    Id = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"),
                    FarmCode = "FA001",
                    Name = "Trang trại gà",
                    Address = "123Abc",
                    PhoneNumber = "0734654304",
                    Email = "farm001@gmail.com",
                    Area = 0.1123,
                    CreatedDate = DateTime.Parse("2024-12-14T10:37:27.9430000"),
                    IsDeleted = false,
                    Macaddress = "192.2.2.3.12"
                };

                _context.Farms.Add(farm);
                _context.SaveChanges();

                var farmAdminEntry = new FarmAdmin
                {
                    Id = Guid.NewGuid(),
                    FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"),
                    AdminId = Guid.Parse("a406f2a5-f7f7-4701-9c21-339b16c06f76"),
                    AssignedDate = DateTime.UtcNow
                };

                _context.FarmAdmins.Add(farmAdminEntry);
                _context.SaveChanges();

                // Thêm dữ liệu thức ăn
                var foodStacks = new List<FoodStack>
                {
                    new FoodStack
                    {
                        Id = Guid.NewGuid(),
                        FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"),
                        FoodType = "Dạng mảnh",
                        Quantity = 1000m,
                        CostPerKg = 15000m,
                        CurrentStock = 1000m
                    },
                    new FoodStack
                    {
                        Id = Guid.NewGuid(),
                        FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"),
                        FoodType = "Dạng viên",
                        Quantity = 2000m,
                        CostPerKg = 17000m,
                        CurrentStock = 2000m
                    }
                };

                _context.FoodStacks.AddRange(foodStacks);
                _context.SaveChanges();


                var cages = new List<Cage>
                {
                    new Cage { Id = Guid.Parse("f37f0727-435d-4d80-9c29-ae2f41b49c9d"), PenCode = "Pen_01", FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"), Name = "Cage 1", Area = 10, Location = "Location 1", Capacity = 500, BoardCode = "Board_01", BoardStatus = true, CreatedDate = DateTime.Parse("2024-12-16T03:01:38.5500000"), CameraUrl = "http://camera_1.example.com", ChannelId = 1, IsSolationCage = false },
                    new Cage { Id = Guid.Parse("80b4c7dd-31d6-4169-9c51-9f4ff368fd41"), PenCode = "Pen_02", FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"), Name = "Cage 2", Area = 11.5, Location = "Location 2", Capacity = 1000, BoardCode = "Board_02", BoardStatus = true, CreatedDate = DateTime.Parse("2024-12-16T03:01:38.5500000"), CameraUrl = "http://camera_2.example.com", ChannelId = 2, IsSolationCage = false },
                    new Cage { Id = Guid.Parse("03eb1376-9197-4e77-bc17-4e4bf595e387"), PenCode = "Pen_03", FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"), Name = "Cage 3", Area = 13, Location = "Location 3", Capacity = 1000, BoardCode = "Board_03", BoardStatus = true, CreatedDate = DateTime.Parse("2024-12-16T03:01:38.5500000"), CameraUrl = "http://camera_3.example.com", ChannelId = 3, IsSolationCage = false },
                    new Cage { Id = Guid.Parse("35050ff8-7822-49d8-8d3d-b1bd51f11891"), PenCode = "Pen_04", FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"), Name = "Cage 4", Area = 14.5, Location = "Location 4", Capacity = 1000, BoardCode = "Board_04", BoardStatus = true, CreatedDate = DateTime.Parse("2024-12-16T03:01:38.5500000"), CameraUrl = "http://camera_4.example.com", ChannelId = 4, IsSolationCage = false },
                    new Cage { Id = Guid.Parse("fcaceb35-b3a0-4e7e-b479-19059aa7006c"), PenCode = "Pen_05", FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"), Name = "Cage 5", Area = 16, Location = "Location 5", Capacity = 1000, BoardCode = "Board_05", BoardStatus = true, CreatedDate = DateTime.Parse("2024-12-16T03:01:38.5500000"), CameraUrl = "http://camera_5.example.com", ChannelId = 5, IsSolationCage = false },
                    new Cage { Id = Guid.Parse("a8cb5774-62a5-41e7-b854-cc2cfd2d33e5"), PenCode = "Pen_06", FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4"), Name = "Cage 6", Area = 10, Location = "Location 6", Capacity = 500, BoardCode = "Board_06", BoardStatus = true, CreatedDate = DateTime.Parse("2024-12-16T03:01:38.5500000"), CameraUrl = "http://camera_6.example.com", ChannelId = 1, IsSolationCage = true },

                };

                _context.Cages.AddRange(cages);
                _context.SaveChanges();


                var cageStaffEntries = new List<CageStaff>
                {
                    new CageStaff { Id = Guid.NewGuid(), CageId = cages[0].Id, StaffFarmId = Guid.Parse("b8a28787-9d97-4849-949a-56ebfc6d5de0"), AssignedDate = DateTime.UtcNow },
                    new CageStaff { Id = Guid.NewGuid(), CageId = cages[1].Id, StaffFarmId = Guid.Parse("b8a28787-9d97-4849-949a-56ebfc6d5de0"), AssignedDate = DateTime.UtcNow },
                    new CageStaff { Id = Guid.NewGuid(), CageId = cages[2].Id, StaffFarmId = Guid.Parse("b8a28787-9d97-4849-949a-56ebfc6d5de0"), AssignedDate = DateTime.UtcNow },
                    new CageStaff { Id = Guid.NewGuid(), CageId = cages[3].Id, StaffFarmId = Guid.Parse("54da1a44-d865-4d41-bf65-f8fc3e939d25"), AssignedDate = DateTime.UtcNow },
                    new CageStaff { Id = Guid.NewGuid(), CageId = cages[4].Id, StaffFarmId = Guid.Parse("54da1a44-d865-4d41-bf65-f8fc3e939d25"), AssignedDate = DateTime.UtcNow },
                    new CageStaff { Id = Guid.NewGuid(), CageId = cages[5].Id, StaffFarmId = Guid.Parse("54da1a44-d865-4d41-bf65-f8fc3e939d25"), AssignedDate = DateTime.UtcNow }
                };

                _context.CageStaffs.AddRange(cageStaffEntries);
                _context.SaveChanges();

                // Tạo bản ghi FarmConfig
                var farmConfig = new FarmConfig
                {
                    Id = Guid.NewGuid(),
                    FarmId = Guid.Parse("7B0AD5A5-CA3E-45B1-9519-D42135D5BEA4"),
                    MaxCagesPerStaff = 5,
                    MaxFarmingBatchesPerCage = 5,
                    LastTimeUpdated = DateTimeUtils.GetServerTimeInVietnamTime(),
                    TimeDifferenceInMinutes = 0
                };
                _context.FarmConfigs.Add(farmConfig);
                _context.SaveChanges();
                return Ok("Dữ liệu đã được nhập vào thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi nhập dữ liệu: {ex.Message}");
            }
        }

        [HttpPost("seed/FarmingBatch")]
        public IActionResult SeedFarmingBatch()
        {
            try
            {
                // Kiểm tra nếu dữ liệu đã có
                if (_context.FarmingBatchs.Any(fb => fb.Name == "Gà nuôi thịt - Cobb-500"))
                    return BadRequest("Dữ liệu FarmingBatch đã tồn tại.");

                // Lấy Template "Gà nuôi thịt - Cobb-500"
                var template = _context.AnimalTemplates.FirstOrDefault(t => t.Name == "Gà nuôi thịt - Cobb-500");
                if (template == null)
                    return BadRequest("Không tìm thấy mẫu chăn nuôi Gà nuôi thịt - Cobb-500.");

                // Lấy danh sách chuồng áp dụng
                var cageIds = new List<Guid>
                {
                    Guid.Parse("f37f0727-435d-4d80-9c29-ae2f41b49c9d"),
                    Guid.Parse("80b4c7dd-31d6-4169-9c51-9f4ff368fd41")
                };

                var startDate = DateTime.UtcNow.AddDays(-50);
                var endDate = DateTime.UtcNow;

                var farmingBatches = new List<FarmingBatch>();

                foreach (var cageId in cageIds)
                {
                    var farmingBatch = new FarmingBatch
                    {
                        Id = Guid.NewGuid(),
                        TemplateId = template.Id,
                        CageId = cageId,
                        FarmingBatchCode = $"FB-{cageId.ToString().Substring(0, 8)}",
                        Name = "Gà nuôi thịt - Cobb-500",
                        StartDate = startDate,
                        EstimatedTimeStart = startDate,
                        EndDate = endDate,
                        Status = "Active",
                        CleaningFrequency = 2,
                        Quantity = 200,
                        DeadQuantity = 0,
                        FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4")
                    };

                    farmingBatches.Add(farmingBatch);
                }

                _context.FarmingBatchs.AddRange(farmingBatches);
                _context.SaveChanges();

                // Lấy danh sách GrowthStage từ template
                var growthStageTemplates = _context.GrowthStageTemplates
                    .Where(g => g.TemplateId == template.Id)
                    .ToList();

                var growthStages = new List<GrowthStage>();
                var taskDailies = new List<TaskDaily>();
                var vaccineSchedules = new List<VaccineSchedule>();

                foreach (var batch in farmingBatches)
                {
                    foreach (var stageTemplate in growthStageTemplates)
                    {
                        var stageStartDate = startDate.AddDays(stageTemplate.AgeStart.Value);
                        var stageEndDate = startDate.AddDays(stageTemplate.AgeEnd.Value);

                        var growthStage = new GrowthStage
                        {
                            Id = Guid.NewGuid(),
                            FarmingBatchId = batch.Id,
                            Name = stageTemplate.StageName,
                            WeightAnimal = stageTemplate.WeightAnimal,
                            WeightAnimalExpect = stageTemplate.WeightAnimal,
                            Quantity = 200,
                            AgeStart = stageTemplate.AgeStart,
                            AgeEnd = stageTemplate.AgeEnd,
                            FoodType = _context.FoodTemplates.FirstOrDefault(f => f.StageTemplateId == stageTemplate.Id)?.FoodType ?? "Không xác định",
                            AgeStartDate = stageStartDate,
                            AgeEndDate = stageEndDate,
                            Status = stageTemplate.StageName == "Gà trưởng thành" ? "Active" : "Completed",
                            DeadQuantity = 0,
                            AffectedQuantity = 0,
                            WeightBasedOnBodyMass = _context.FoodTemplates.FirstOrDefault(f => f.StageTemplateId == stageTemplate.Id)?.WeightBasedOnBodyMass,
                            RecommendedWeightPerSession = 200 * stageTemplate.WeightAnimal
                        };

                        growthStage.RecommendedWeightPerSession = growthStage.WeightAnimal * growthStage.WeightBasedOnBodyMass;

                        growthStages.Add(growthStage);

                        // Xác định số cữ ăn mỗi ngày
                        List<int> sessionNumbers = growthStage.Name == "Gà con" ? new List<int> { 1, 2, 3, 4 } : new List<int> { 1, 2, 4 };

                        // Tạo TaskDaily từ TaskDailyTemplate
                        var taskDailyTemplates = _context.TaskDailyTemplates.Where(t => t.GrowthStageTemplateId == stageTemplate.Id).ToList();
                        foreach (var taskTemplate in taskDailyTemplates)
                        {

                            taskDailies.Add(new TaskDaily
                            {
                                Id = Guid.NewGuid(),
                                GrowthStageId = growthStage.Id,
                                TaskTypeId = taskTemplate.TaskTypeId,
                                TaskName = taskTemplate.TaskName,
                                Description = taskTemplate.Description,
                                Session = taskTemplate.Session,
                                StartAt = stageStartDate,
                                EndAt = stageEndDate
                            });

                        }



                        // Tạo VaccineSchedule
                        var vaccineTemplates = _context.VaccineTemplates
                            .Where(v => v.TemplateId == template.Id && v.ApplicationAge >= growthStage.AgeStart && v.ApplicationAge <= growthStage.AgeEnd).ToList();
                        foreach (var vaccine in vaccineTemplates)
                        {
                            var vaccineData = _context.Vaccines.FirstOrDefault(v => v.Name == vaccine.VaccineName);
                            if (vaccineData != null)
                            {
                                var vaccineSchedule = new VaccineSchedule
                                {
                                    Id = Guid.NewGuid(),
                                    StageId = growthStage.Id,
                                    VaccineId = vaccineData.Id,
                                    Date = startDate.AddDays(vaccine.ApplicationAge ?? 0),
                                    Quantity = 200,
                                    ApplicationAge = vaccine.ApplicationAge,
                                    ToltalPrice = 200 * (decimal)vaccineData.Price,
                                    Session = vaccine.Session,
                                    Status = VaccineScheduleStatusEnum.Completed
                                };

                                vaccineSchedules.Add(vaccineSchedule);
                            }
                        }

                    }

                }
                UpdateVaccineScheduleStatus(vaccineSchedules);
                _context.GrowthStages.AddRange(growthStages);
                _context.TaskDailies.AddRange(taskDailies);
                _context.VaccineSchedules.AddRange(vaccineSchedules);

                _context.SaveChanges();

                return Ok("Dữ liệu FarmingBatch, GrowthStage, TaskDaily, DailyFoodUsageLog, VaccineSchedule đã được nhập thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi nhập dữ liệu: {ex.Message}");
            }
        }

        private void UpdateVaccineScheduleStatus(List<VaccineSchedule> vaccineSchedules)
        {
            DateTime vietnamNow = DateTimeUtils.GetServerTimeInVietnamTime(); // Lấy giờ thực tế theo múi giờ Việt Nam
            int currentSession = SessionTime.GetCurrentSession(vietnamNow.TimeOfDay); // Lấy session hiện tại từ giờ

            foreach (var schedule in vaccineSchedules)
            {
                if (!schedule.Date.HasValue)
                    continue;

                var scheduleDate = schedule.Date.Value.Date;

                // Nếu ngày thực tế > ngày lịch → chắc chắn là upcoming
                if (vietnamNow.Date < scheduleDate)
                {
                    schedule.Status = VaccineScheduleStatusEnum.Upcoming;
                }
                // Nếu cùng ngày thì so session
                else if (vietnamNow.Date == scheduleDate)
                {
                    if (currentSession >= schedule.Session)
                    {
                        schedule.Status = VaccineScheduleStatusEnum.Upcoming;
                    }
                }
                // Còn lại (ngày nhỏ hơn) thì giữ nguyên (ví dụ vẫn "Complete")
            }
        }

        [HttpPost("seed/Tasks")]
        public IActionResult SeedTasks()
        {
            try
            {
                if (_context.Tasks.Any())
                    return BadRequest("Dữ liệu Tasks đã tồn tại.");

                var farmingBatches = _context.FarmingBatchs.ToList();
                var taskTypes = _context.TaskTypes.ToList();

                var tasks = new List<DataAccessObject.Models.Task>();

                foreach (var batch in farmingBatches)
                {
                    var growthStages = _context.GrowthStages.Where(g => g.FarmingBatchId == batch.Id).ToList();

                    // Tìm nhân viên được phân công cho chuồng này
                    var assignedStaffs = _context.CageStaffs
                        .Where(cs => cs.CageId == batch.CageId)
                        .Select(cs => cs.StaffFarmId)
                        .ToList();

                    if (!assignedStaffs.Any())
                        continue; // Không có nhân viên, bỏ qua chuồng này

                    var assignedUserId = assignedStaffs.First(); // Chọn ngẫu nhiên 1 nhân viên Staff Farm

                    foreach (var stage in growthStages)
                    {
                        var stageStartDate = stage.AgeStartDate.Value;
                        var stageEndDate = stage.AgeEndDate.Value;

                        // Task "Cho ăn"
                        List<int> sessionNumbers = stage.Name == "Gà con" ? new List<int> { 1, 2, 3, 4 } : new List<int> { 1, 2, 4 };
                        var feedingTaskType = taskTypes.FirstOrDefault(t => t.TaskTypeName == "Cho ăn");
                        var dateNow = DateTimeUtils.GetServerTimeInVietnamTime();
                        if (dateNow.Date == stageEndDate.Date)
                        {
                            stageEndDate = stageEndDate.AddDays(1);
                        }
                        for (DateTime date = stageStartDate; date <= stageEndDate; date = date.AddDays(1))
                        {
                            foreach (var session in sessionNumbers)
                            {
                                tasks.Add(new DataAccessObject.Models.Task
                                {
                                    Id = Guid.NewGuid(),
                                    TaskTypeId = feedingTaskType.Id,
                                    CageId = batch.CageId,
                                    AssignedToUserId = assignedUserId,
                                    TaskName = "Cho ăn",
                                    PriorityNum = feedingTaskType.PriorityNum.Value,
                                    Description = $"Cho gà ăn",
                                    DueDate = date,
                                    Status = "Done",
                                    Session = session,
                                    IsTreatmentTask = false,
                                    CreatedAt = date.AddDays(-1)
                                });
                            }
                        }

                        // Task "Dọn chuồng" - Mỗi `CleaningFrequency` ngày
                        var cleaningTaskType = taskTypes.FirstOrDefault(t => t.TaskTypeName == "Dọn chuồng");
                        for (DateTime date = stageStartDate.AddDays(batch.CleaningFrequency); date <= stageEndDate; date = date.AddDays(batch.CleaningFrequency))
                        {
                            tasks.Add(new DataAccessObject.Models.Task
                            {
                                Id = Guid.NewGuid(),
                                TaskTypeId = cleaningTaskType.Id,
                                CageId = batch.CageId,
                                AssignedToUserId = assignedUserId,
                                TaskName = "Dọn chuồng",
                                PriorityNum = cleaningTaskType.PriorityNum.Value,
                                Description = $"Dọn chuồng",
                                DueDate = date,
                                Status = "Done",
                                Session = 1,
                                IsTreatmentTask = false,
                                CreatedAt = date.AddDays(-1)
                            });
                        }

                        // Task "Cân" - Ngày cuối mỗi giai đoạn
                        var weighingTaskType = taskTypes.FirstOrDefault(t => t.TaskTypeName == "Cân");
                        tasks.Add(new DataAccessObject.Models.Task
                        {
                            Id = Guid.NewGuid(),
                            TaskTypeId = weighingTaskType.Id,
                            CageId = batch.CageId,
                            AssignedToUserId = assignedUserId,
                            TaskName = "Cân",
                            PriorityNum = weighingTaskType.PriorityNum.Value,
                            Description = $"Cân gà vào cuối giai đoạn {stage.Name}",
                            DueDate = stageEndDate,
                            Session = 1,
                            IsTreatmentTask = false,
                            Status = "Done",
                            CreatedAt = stageEndDate.AddDays(-1)
                        });

                        // Task "Tiêm vắc xin" - Dựa trên VaccineSchedule
                        var vaccineSchedules = _context.VaccineSchedules.Where(vs => vs.StageId == stage.Id).ToList();
                        foreach (var vaccineSchedule in vaccineSchedules)
                        {
                            var vaccinationTaskType = taskTypes.FirstOrDefault(t => t.TaskTypeName == "Tiêm vắc xin");
                            var vaccine = _context.Vaccines.FirstOrDefault(v => v.Id == vaccineSchedule.VaccineId);

                            tasks.Add(new DataAccessObject.Models.Task
                            {
                                Id = Guid.NewGuid(),
                                TaskTypeId = vaccinationTaskType.Id,
                                CageId = batch.CageId,
                                AssignedToUserId = assignedUserId,
                                TaskName = "Tiêm vắc xin",
                                PriorityNum = vaccinationTaskType.PriorityNum.Value,
                                Description = $"Tiêm vắc xin {vaccine.Name}",
                                DueDate = vaccineSchedule.Date.Value,
                                Status = "Done",
                                Session = vaccineSchedule.Session,
                                CreatedAt = vaccineSchedule.Date.Value.AddDays(-1)
                            });
                        }
                    }
                }

                _context.Tasks.AddRange(tasks);
                _context.SaveChanges();

                return Ok("Dữ liệu Tasks đã được nhập thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi nhập dữ liệu Tasks: {ex.Message}");
            }
        }

        [HttpPost("seed/Logs")]
        public IActionResult SeedLogs()
        {
            try
            {
                if (_context.DailyFoodUsageLogs.Any() || _context.VaccineScheduleLogs.Any())
                    return BadRequest("Dữ liệu Logs đã tồn tại.");

                var farmingBatches = _context.FarmingBatchs.ToList();
                var tasks = _context.Tasks.ToList();

                var foodLogs = new List<DailyFoodUsageLog>();
                var vaccineLogs = new List<VaccineScheduleLog>();

                foreach (var batch in farmingBatches)
                {
                    var growthStages = _context.GrowthStages.Where(g => g.FarmingBatchId == batch.Id).ToList();

                    foreach (var stage in growthStages)
                    {
                        var stageStartDate = stage.AgeStartDate.Value;
                        var stageEndDate = stage.AgeEndDate.Value;

                        // Tạo DailyFoodUsageLog (Cho ăn)
                        List<int> sessionNumbers = stage.Name == "Gà con" ? new List<int> { 1, 2, 3, 4 } : new List<int> { 1, 2, 4 };
                        var dateNow = DateTimeUtils.GetServerTimeInVietnamTime();
                        if (dateNow.Date == stageEndDate.Date)
                        {
                            stageEndDate = stageEndDate.AddDays(-1);
                        }
                        for (DateTime date = stageStartDate; date <= stageEndDate; date = date.AddDays(1))
                        {
                            foreach (var session in sessionNumbers)
                            {
                                // Tìm Task tương ứng
                                var relatedTask = tasks.FirstOrDefault(t =>
                                    t.CageId == batch.CageId &&
                                    t.TaskName == "Cho ăn" &&
                                    t.DueDate.Value.Date == date.Date &&
                                    t.Session == session);

                                foodLogs.Add(new DailyFoodUsageLog
                                {
                                    Id = Guid.NewGuid(),
                                    StageId = stage.Id,
                                    RecommendedWeight = stage.Quantity * stage.RecommendedWeightPerSession,
                                    ActualWeight = stage.Quantity * stage.RecommendedWeightPerSession,
                                    Notes = "Ghi nhận lượng thức ăn tiêu thụ",
                                    LogTime = date,
                                    UnitPrice = 15000, // Giá đại diện
                                    Photo = "food_log.jpg",
                                    TaskId = relatedTask?.Id // Gán TaskId nếu tìm thấy Task
                                });
                            }
                        }

                        // Tạo VaccineScheduleLog (Tiêm vắc xin)
                        var vaccineSchedules = _context.VaccineSchedules.Where(vs => vs.StageId == stage.Id && vs.ApplicationAge >= stage.AgeStart &&
                vs.ApplicationAge <= stage.AgeEnd).ToList();

                        foreach (var vaccineSchedule in vaccineSchedules)
                        {
                            // Tìm Task tương ứng
                            var relatedTask = tasks.FirstOrDefault(t =>
                                t.CageId == batch.CageId &&
                                t.TaskName == "Tiêm vắc xin" &&
                                t.DueDate.Value.Date == vaccineSchedule.Date.Value.Date &&
                                t.Session == vaccineSchedule.Session);

                            vaccineLogs.Add(new VaccineScheduleLog
                            {
                                Id = Guid.NewGuid(),
                                ScheduleId = vaccineSchedule.Id,
                                Date = DateOnly.FromDateTime(vaccineSchedule.Date.Value),
                                Notes = "Ghi nhận lịch tiêm vaccine",
                                Photo = "vaccine_log.jpg",
                                TaskId = relatedTask?.Id // Gán TaskId nếu tìm thấy Task
                            });
                        }
                    }
                }

                _context.DailyFoodUsageLogs.AddRange(foodLogs);
                _context.VaccineScheduleLogs.AddRange(vaccineLogs);
                _context.SaveChanges();

                return Ok("Dữ liệu Logs đã được nhập thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi nhập dữ liệu Logs: {ex.Message}");
            }
        }


        [HttpGet("tasks/status/update")]
        public IActionResult GetAndUpdateTaskStatus()
        {
            try
            {
                DateTime vietnamNow = DateTimeUtils.GetServerTimeInVietnamTime();
                int currentSession = SessionTime.GetCurrentSession(vietnamNow.TimeOfDay);

                // Lấy danh sách task cần xử lý
                var tasks = _context.Tasks
                    .Where(t => t.DueDate >= vietnamNow.Date) // Chỉ lấy task hôm nay trở đi
                    .ToList();

                var foodLogs = new List<DailyFoodUsageLog>();
                var vaccineLogs = new List<VaccineScheduleLog>();

                foreach (var task in tasks)
                {
                    // Xác định trạng thái task
                    if (task.DueDate.Value.Date == vietnamNow.Date)
                    {
                        if (task.Session < currentSession)
                        {
                            task.Status = "Done";
                            GenerateLogForTask(task, foodLogs, vaccineLogs);
                        }
                        else if (task.Session == currentSession)
                        {
                            task.Status = "InProgress";
                        }
                        else
                        {
                            task.Status = "Pending";
                        }
                    }
                    else // Task có DueDate > ngày hiện tại
                    {
                        task.Status = "Pending";
                    }
                }

                // Lưu thay đổi Task
                _context.SaveChanges();

                // Thêm log nếu cần
                _context.DailyFoodUsageLogs.AddRange(foodLogs);
                _context.VaccineScheduleLogs.AddRange(vaccineLogs);
                _context.SaveChanges();

                return Ok(new { Message = "Tasks updated successfully", UpdatedTasks = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi cập nhật Task: {ex.Message}");
            }
        }

        private void GenerateLogForTask(DataAccessObject.Models.Task task, List<DailyFoodUsageLog> foodLogs, List<VaccineScheduleLog> vaccineLogs)
        {
            if (task.TaskName == "Cho ăn")
            {
                var growthStage = _context.GrowthStages.FirstOrDefault(gs => gs.FarmingBatch.CageId == task.CageId && gs.AgeStartDate <= task.DueDate && gs.AgeEndDate >= task.DueDate);
                if (growthStage != null)
                {
                    foodLogs.Add(new DailyFoodUsageLog
                    {
                        Id = Guid.NewGuid(),
                        StageId = growthStage.Id,
                        RecommendedWeight = growthStage.Quantity * growthStage.RecommendedWeightPerSession,
                        ActualWeight = growthStage.Quantity * growthStage.RecommendedWeightPerSession,
                        Notes = "Ghi nhận lượng thức ăn tiêu thụ",
                        LogTime = task.DueDate,
                        UnitPrice = 15000,
                        Photo = "food_log.jpg",
                        TaskId = task.Id
                    });
                }
            }
            else if (task.TaskName == "Tiêm vắc xin")
            {
                var vaccineSchedule = _context.VaccineSchedules.FirstOrDefault(vs => vs.Stage.FarmingBatch.CageId == task.CageId && vs.Date == task.DueDate);
                if (vaccineSchedule != null)
                {
                    vaccineLogs.Add(new VaccineScheduleLog
                    {
                        Id = Guid.NewGuid(),
                        ScheduleId = vaccineSchedule.Id,
                        Date = DateOnly.FromDateTime(task.DueDate.Value),
                        Notes = "Ghi nhận lịch tiêm vaccine",
                        Photo = "vaccine_log.jpg",
                        TaskId = task.Id
                    });
                }
            }
        }


        [HttpPost("seed/Standard")]
        public IActionResult SeedStandards()
        {
            try
            {
                var medications = new List<Medication>
                {
                // ✅ Kháng sinh & Thuốc điều trị đặc hiệu
                new Medication { Id = Guid.NewGuid(), Name = "Enrofloxacin", UsageInstructions = "Pha vào nước uống, dùng trong 5-7 ngày", Price = 50000, DoseWeight = 50, Weight = 500, DoseQuantity = 10, PricePerDose = 5000 },
                new Medication { Id = Guid.NewGuid(), Name = "Doxycycline", UsageInstructions = "Trộn vào thức ăn hoặc nước uống, dùng trong 5-7 ngày", Price = 40000, DoseWeight = 100, Weight = 1000, DoseQuantity = 10, PricePerDose = 4000 },
                new Medication { Id = Guid.NewGuid(), Name = "Tylosin", UsageInstructions = "Pha vào nước uống, dùng trong 5 ngày", Price = 60000, DoseWeight = 25, Weight = 250, DoseQuantity = 10, PricePerDose = 6000 },
                new Medication { Id = Guid.NewGuid(), Name = "Oxytetracycline", UsageInstructions = "Tiêm bắp hoặc pha nước uống, dùng trong 5 ngày", Price = 55000, DoseWeight = 50, Weight = 500, DoseQuantity = 10, PricePerDose = 5500 },
                new Medication { Id = Guid.NewGuid(), Name = "Amprolium", UsageInstructions = "Pha vào nước uống, dùng trong 5 ngày", Price = 30000, DoseWeight = 100, Weight = 500, DoseQuantity = 5, PricePerDose = 6000 },
                new Medication { Id = Guid.NewGuid(), Name = "Toltrazuril", UsageInstructions = "Pha vào nước uống, dùng trong 2 ngày", Price = 80000, DoseWeight = 20, Weight = 200, DoseQuantity = 10, PricePerDose = 8000 },
                new Medication { Id = Guid.NewGuid(), Name = "Florfenicol", UsageInstructions = "Pha vào nước uống hoặc tiêm bắp, dùng trong 5 ngày", Price = 70000, DoseWeight = 30, Weight = 300, DoseQuantity = 10, PricePerDose = 7000 },
                new Medication { Id = Guid.NewGuid(), Name = "Ivermectin", UsageInstructions = "Tiêm dưới da hoặc uống, dùng trong 2 ngày", Price = 90000, DoseWeight = 10, Weight = 100, DoseQuantity = 10, PricePerDose = 9000 },
                new Medication { Id = Guid.NewGuid(), Name = "Penicillin", UsageInstructions = "Tiêm bắp hoặc pha nước uống, dùng trong 5 ngày", Price = 45000, DoseWeight = 50, Weight = 500, DoseQuantity = 10, PricePerDose = 4500 },
                new Medication { Id = Guid.NewGuid(), Name = "Amoxicillin", UsageInstructions = "Pha vào nước uống, dùng trong 5-7 ngày", Price = 50000, DoseWeight = 50, Weight = 500, DoseQuantity = 10, PricePerDose = 5000 },
                new Medication { Id = Guid.NewGuid(), Name = "Lincomycin", UsageInstructions = "Tiêm bắp hoặc pha nước uống, dùng trong 5 ngày", Price = 65000, DoseWeight = 25, Weight = 250, DoseQuantity = 10, PricePerDose = 6500 },
                new Medication { Id = Guid.NewGuid(), Name = "Chloramphenicol", UsageInstructions = "Pha vào nước uống hoặc trộn thức ăn, dùng trong 5 ngày", Price = 48000, DoseWeight = 50, Weight = 500, DoseQuantity = 10, PricePerDose = 4800 },
                new Medication { Id = Guid.NewGuid(), Name = "Nystatin", UsageInstructions = "Pha vào nước uống, dùng trong 5 ngày", Price = 35000, DoseWeight = 50, Weight = 500, DoseQuantity = 10, PricePerDose = 3500 },
                new Medication { Id = Guid.NewGuid(), Name = "Fluconazole", UsageInstructions = "Pha vào nước uống, dùng trong 5 ngày", Price = 40000, DoseWeight = 50, Weight = 500, DoseQuantity = 10, PricePerDose = 4000 },

                // ✅ Vitamin & Chất điện giải
                new Medication { Id = Guid.NewGuid(), Name = "Vitamin A", UsageInstructions = "Bổ sung hàng ngày qua nước uống", Price = 20000, DoseWeight = 10, Weight = 100, DoseQuantity = 10, PricePerDose = 2000 },
                new Medication { Id = Guid.NewGuid(), Name = "Vitamin D", UsageInstructions = "Bổ sung hàng ngày, giúp hấp thụ canxi", Price = 22000, DoseWeight = 10, Weight = 100, DoseQuantity = 10, PricePerDose = 2200 },
                new Medication { Id = Guid.NewGuid(), Name = "Vitamin B-complex", UsageInstructions = "Giúp giảm stress, tăng chuyển hóa", Price = 25000, DoseWeight = 10, Weight = 100, DoseQuantity = 10, PricePerDose = 2500 },
                new Medication { Id = Guid.NewGuid(), Name = "Vitamin C", UsageInstructions = "Tăng cường miễn dịch, hỗ trợ phục hồi", Price = 23000, DoseWeight = 10, Weight = 100, DoseQuantity = 10, PricePerDose = 2300 },
                new Medication { Id = Guid.NewGuid(), Name = "Electrolyte", UsageInstructions = "Bù nước, giúp gà hồi phục nhanh", Price = 30000, DoseWeight = 20, Weight = 200, DoseQuantity = 10, PricePerDose = 3000 },
                new Medication { Id = Guid.NewGuid(), Name = "Multivitamin tổng hợp", UsageInstructions = "Hỗ trợ miễn dịch, tăng đề kháng", Price = 27000, DoseWeight = 10, Weight = 100, DoseQuantity = 10, PricePerDose = 2700 }
                };


                _context.Medications.AddRange(medications);
                _context.SaveChanges();

                // 📌 2. Danh sách bệnh + phác đồ điều trị
                var diseases = _context.Diseases.ToList();
                var prescriptions = new List<StandardPrescription>();
                var random = new Random();
                foreach (var disease in diseases)
                {
                    var prescription = new StandardPrescription
                    {
                        Id = Guid.NewGuid(),
                        DiseaseId = disease.Id,
                        Notes = "Tuân thủ hướng dẫn điều trị, bổ sung vitamin và điện giải",
                        RecommendDay = random.Next(3, 6)
                    };

                    prescriptions.Add(prescription);

                }
                _context.StandardPrescriptions.AddRange(prescriptions);
                _context.SaveChanges();

                // 📌 3. Gán thuốc + Vitamin & Điện giải vào phác đồ điều trị
                var prescriptionMedications = new List<StandardPrescriptionMedication>();

                foreach (var prescription in prescriptions)
                {
                    var diseaseName = _context.Diseases.First(d => d.Id == prescription.DiseaseId).Name;
                    var medicationsForDisease = new List<(string Medication, int Morning, int Noon, int Afternoon, int Evening)>();

                    switch (diseaseName)
                    {
                        case "Dịch tả gà":
                            medicationsForDisease.Add(("Enrofloxacin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Doxycycline", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            medicationsForDisease.Add(("Multivitamin tổng hợp", 1, 0, 1, 0));
                            break;

                        case "Viêm khí quản truyền nhiễm":
                            medicationsForDisease.Add(("Tylosin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Oxytetracycline", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Vitamin A", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Cầu trùng":
                            medicationsForDisease.Add(("Amprolium", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Toltrazuril", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Thiếu Vitamin A":
                            medicationsForDisease.Add(("Vitamin A", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Multivitamin tổng hợp", 1, 0, 1, 0));
                            break;

                        case "Thiếu Vitamin D":
                            medicationsForDisease.Add(("Vitamin D", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Multivitamin tổng hợp", 1, 0, 1, 0));
                            break;

                        case "Tụ huyết trùng":
                            medicationsForDisease.Add(("Florfenicol", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Gumboro":
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            medicationsForDisease.Add(("Multivitamin tổng hợp", 1, 0, 1, 0));
                            break;

                        case "Cúm gia cầm":
                            medicationsForDisease.Add(("Enrofloxacin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Florfenicol", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            medicationsForDisease.Add(("Vitamin C", 1, 0, 1, 0));
                            break;

                        case "Đậu gà":
                            medicationsForDisease.Add(("Oxytetracycline", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Vitamin A", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Viêm ruột hoại tử":
                            medicationsForDisease.Add(("Amoxicillin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Lincomycin", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Ngộ độc thịt":
                            medicationsForDisease.Add(("Penicillin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Nhiễm nấm":
                            medicationsForDisease.Add(("Nystatin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Fluconazole", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Giun khí quản":
                            medicationsForDisease.Add(("Ivermectin", 1, 0, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Lymphoid leukosis":
                            medicationsForDisease.Add(("Multivitamin tổng hợp", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Thương hàn":
                            medicationsForDisease.Add(("Chloramphenicol", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "E. coli":
                            medicationsForDisease.Add(("Enrofloxacin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Florfenicol", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Bệnh CRD":
                            medicationsForDisease.Add(("Tylosin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Doxycycline", 1, 1, 0, 1));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Gout nội tạng":
                            medicationsForDisease.Add(("Vitamin D", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;

                        case "Hội chứng báng nước":
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            medicationsForDisease.Add(("Multivitamin tổng hợp", 1, 0, 1, 0));
                            break;

                        case "Thiếu Canxi & Photpho":
                            medicationsForDisease.Add(("Vitamin D", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Multivitamin tổng hợp", 1, 0, 1, 0));
                            break;

                        case "Thiếu Protein":
                            medicationsForDisease.Add(("Multivitamin tổng hợp", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Vitamin B-complex", 1, 1, 0, 1));
                            break;

                        default:
                            medicationsForDisease.Add(("Enrofloxacin", 1, 0, 1, 0));
                            medicationsForDisease.Add(("Electrolyte", 1, 1, 1, 1));
                            break;
                    }

                    foreach (var med in medicationsForDisease)
                    {
                        var medication = _context.Medications.FirstOrDefault(m => m.Name == med.Medication);

                        if (medication != null)
                        {
                            prescriptionMedications.Add(new StandardPrescriptionMedication
                            {
                                Id = Guid.NewGuid(),
                                PrescriptionId = prescription.Id,
                                MedicationId = medication.Id,
                                Morning = med.Morning,
                                Noon = med.Noon,
                                Afternoon = med.Afternoon,
                                Evening = med.Evening
                            });
                        }
                    }
                }

                _context.StandardPrescriptionMedications.AddRange(prescriptionMedications);
                _context.SaveChanges();
                return Ok("Dữ liệu Standards đã được nhập thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi nhập dữ liệu Logs: {ex.Message}");
            }
        }

        [HttpPost("seed/SensorData")]
        public IActionResult SeedSensorData()
        {
            try
            {
                // Kiểm tra nếu đã có dữ liệu thì không thêm nữa
                if (_context.Sensors.Any())
                {
                    return BadRequest("Dữ liệu cảm biến đã tồn tại trong hệ thống.");
                }

                // Danh sách các loại cảm biến (SensorType)
                var sensorTypes = new List<SensorType>
        {
            new SensorType { Id = Guid.NewGuid(), Name = "Cảm biến nhiệt độ", Description = "Cảm biến đo nhiệt độ", FieldName = "Temperature", Unit = "°C", DefaultPinCode = 1 },
            new SensorType { Id = Guid.NewGuid(), Name = "Cảm biến H2S", Description = "Cảm biến đo nồng độ H2S", FieldName = "H2S", Unit = "%", DefaultPinCode = 2 },
            new SensorType { Id = Guid.NewGuid(), Name = "Cảm biến NH3", Description = "Cảm biến đo nồng độ NH3", FieldName = "NH3", Unit = "%", DefaultPinCode = 3 },
            new SensorType { Id = Guid.NewGuid(), Name = "Cảm biến độ ẩm", Description = "Cảm biến đo độ ẩm", FieldName = "Humidity", Unit = "%", DefaultPinCode = 4 }
        };

                _context.SensorTypes.AddRange(sensorTypes);
                _context.SaveChanges();

                // Lấy danh sách các chuồng (Cage)
                var cages = _context.Cages.ToList();

                foreach (var cage in cages)
                {
                    foreach (var sensorType in sensorTypes)
                    {
                        // Tạo dữ liệu cảm biến cho mỗi chuồng
                        var sensor = new Sensor
                        {
                            Id = Guid.NewGuid(),
                            SensorTypeId = sensorType.Id,
                            CageId = cage.Id,
                            SensorCode = $"Sensor_{cage.PenCode}_{sensorType.FieldName}",
                            Name = sensorType.Name,
                            PinCode = sensorType.DefaultPinCode, // Gán PinCode theo mặc định của SensorType
                            Status = true, // Giả sử cảm biến luôn hoạt động
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow,
                            IsDeleted = false,
                            NodeId = 1 // Gán NodeId mặc định (có thể thay đổi nếu có yêu cầu thêm về NodeId)
                        };

                        _context.Sensors.Add(sensor);
                    }
                }

                _context.SaveChanges();

                return Ok("Dữ liệu cảm biến đã được nhập vào thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi nhập dữ liệu cảm biến: {ex.Message}");
            }
        }
        [HttpPost("seed/ElectricityData")]
        public IActionResult SeedElectricityData()
        {
            try
            {
                // Kiểm tra nếu đã có dữ liệu điện thì không thêm nữa
                if (_context.ElectricityLogs.Any())
                {
                    return BadRequest("Dữ liệu điện đã tồn tại trong hệ thống.");
                }

                // Lấy danh sách các farm
                var farms = _context.Farms.ToList();

                // Tạo dữ liệu điện giả cho mỗi farm
                foreach (var farm in farms)
                {
                    var fakeElectricData = new ElectricDataOfFarmModel
                    {
                        FarmCode = farm.FarmCode,
                        Data = new List<ElectricRecordModel>(), // Danh sách các record điện cho từng giờ
                        CreatedDate = DateTime.UtcNow.Date  // Tạo dữ liệu cho ngày hôm nay
                    };

                    // Tạo dữ liệu điện giả cho 24 giờ trong ngày
                    var random = new Random();
                    for (int hour = 0; hour < 24; hour++)
                    {
                        var beginTime = DateTime.UtcNow.Date.AddHours(hour);  // Thời gian bắt đầu của mỗi giờ
                        var endTime = beginTime.AddHours(1);  // Thời gian kết thúc sau 1 giờ

                        // Tạo record cho mỗi giờ
                        var electricityRecord = new ElectricRecordModel
                        {
                            BeginTime = beginTime,  // Thời gian bắt đầu của mỗi giờ
                            EndTime = endTime,      // Thời gian kết thúc của mỗi giờ
                            Value = Math.Round(random.NextDouble() * 300, 2),  // Giá trị random cho điện tiêu thụ (từ 0 - 500)
                            Date = DateTime.UtcNow.Date.AddHours(hour)  // Ngày và giờ ghi nhận (cùng ngày)
                        };

                        fakeElectricData.Data.Add(electricityRecord); // Thêm vào dữ liệu cho farm
                    }


                    // Lưu dữ liệu vào cơ sở dữ liệu
                    var electricityLog = new ElectricityLog
                    {
                        Id = Guid.NewGuid(),
                        FarmId = farm.Id,
                        Data = JsonConvert.SerializeObject(fakeElectricData.Data),  // Lưu dữ liệu dưới dạng JSON
                        TotalConsumption = (decimal)fakeElectricData.Data.Sum(record => record.Value),
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };

                    _context.ElectricityLogs.Add(electricityLog);
                }

                _context.SaveChanges();

                return Ok("Dữ liệu điện đã được nhập vào thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi nhập dữ liệu điện: {ex.Message}");
            }
        }

        [HttpPost("seed/WaterData")]
        public IActionResult SeedWaterData()
        {
            try
            {
                // Kiểm tra nếu đã có dữ liệu nước thì không thêm nữa
                if (_context.WaterLogs.Any())
                {
                    return BadRequest("Dữ liệu nước đã tồn tại trong hệ thống.");
                }

                // Lấy danh sách các farm
                var farms = _context.Farms.ToList();

                // Tạo dữ liệu nước giả cho mỗi farm
                foreach (var farm in farms)
                {
                    var fakeWaterData = new WaterDataOfFarmModel
                    {
                        FarmCode = farm.FarmCode,
                        Data = new List<WaterRecordModel>(), // Danh sách các record nước cho từng giờ
                        CreatedDate = DateTime.UtcNow.Date  // Tạo dữ liệu cho ngày hôm nay
                    };

                    // Tạo dữ liệu nước giả cho 24 giờ trong ngày
                    var random = new Random();
                    for (int hour = 0; hour < 24; hour++)
                    {
                        var beginTime = DateTime.UtcNow.Date.AddHours(hour);  // Thời gian bắt đầu của mỗi giờ
                        var endTime = beginTime.AddHours(1);  // Thời gian kết thúc sau 1 giờ

                        // Tạo record cho mỗi giờ
                        var waterRecord = new WaterRecordModel
                        {
                            BeginTime = beginTime,  // Thời gian bắt đầu của mỗi giờ
                            EndTime = endTime,      // Thời gian kết thúc của mỗi giờ
                            Value = Math.Round(random.NextDouble() * 300, 2),  // Giá trị random cho nước tiêu thụ (từ 0 - 1000)
                            Date = DateTime.UtcNow.Date.AddHours(hour)  // Ngày và giờ ghi nhận (cùng ngày)
                        };

                        fakeWaterData.Data.Add(waterRecord); // Thêm vào dữ liệu cho farm
                    }

                    // Lưu dữ liệu vào cơ sở dữ liệu
                    var waterLog = new WaterLog
                    {
                        Id = Guid.NewGuid(),
                        FarmId = farm.Id,
                        Data = JsonConvert.SerializeObject(fakeWaterData.Data),  // Lưu dữ liệu dưới dạng JSON
                        TotalConsumption = (decimal)fakeWaterData.Data.Sum(record => record.Value),
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    };

                    _context.WaterLogs.Add(waterLog);
                }

                _context.SaveChanges();

                return Ok("Dữ liệu nước đã được nhập vào thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi nhập dữ liệu nước: {ex.Message}");
            }
        }

        [HttpPost("seed/SensorDataLogs")]
        public IActionResult SeedSensorDataLogs()
        {
            try
            {
                // Kiểm tra nếu đã có dữ liệu cảm biến thì không thêm nữa
                if (_context.SensorDataLogs.Any())
                {
                    return BadRequest("Dữ liệu cảm biến đã tồn tại trong hệ thống.");
                }

                // Lấy danh sách các cảm biến (Sensor) đã tạo ở bước trước
                var sensors = _context.Sensors.Include(s => s.SensorType).ToList();

                // Tạo dữ liệu cảm biến giả cho mỗi cảm biến
                foreach (var sensor in sensors)
                {
                    // Tạo dữ liệu cảm biến giả cho mỗi cảm biến trong từng chuồng
                    var fakeSensorData = new SensorDataLog
                    {
                        Id = Guid.NewGuid(),
                        SensorId = sensor.Id,
                        Data = GenerateFakeSensorData(sensor.SensorType),  // Dữ liệu cảm biến giả (tạo cho mỗi sensor)
                        CreatedDate = DateTime.UtcNow,
                        IsWarning = false // Mặc định là không có cảnh báo
                    };

                    _context.SensorDataLogs.Add(fakeSensorData);
                }

                _context.SaveChanges();

                return Ok("Dữ liệu cảm biến đã được nhập vào thành công!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi nhập dữ liệu cảm biến: {ex.Message}");
            }
        }

        private string GenerateFakeSensorData(SensorType sensorType)
        {
            var random = new Random();
            var sensorRecords = new List<SensorRecordModel>();

            // Tạo dữ liệu cảm biến giả cho 24 giờ trong ngày
            for (int hour = 0; hour < 24; hour++)
            {
                var beginTime = DateTime.UtcNow.Date.AddHours(hour);  // Thời gian bắt đầu của mỗi giờ
                var endTime = beginTime.AddHours(1);  // Thời gian kết thúc sau 1 giờ

                double sensorValue = 0;

                // Cập nhật giá trị cảm biến tùy theo loại cảm biến
                switch (sensorType.Name)
                {
                    case "Cảm biến nhiệt độ":
                        // Nhiệt độ trong khoảng 18°C - 30°C
                        sensorValue = Math.Round(random.NextDouble() * (30 - 18) + 18, 2);
                        break;

                    case "Cảm biến H2S":
                        // Nồng độ H2S trong khoảng 0% - 1%
                        sensorValue = Math.Round(random.NextDouble() * (1 - 0) + 0, 2);
                        break;

                    case "Cảm biến NH3":
                        // Nồng độ NH3 trong khoảng 0.001% - 0.02%
                        sensorValue = Math.Round(random.NextDouble() * (0.02 - 0.001) + 0.001, 5);
                        break;

                    case "Cảm biến độ ẩm":
                        // Độ ẩm trong khoảng 50% - 70%
                        sensorValue = Math.Round(random.NextDouble() * (70 - 50) + 50, 2);
                        break;

                    default:
                        sensorValue = 0;
                        break;
                }

                // Tạo record cho mỗi giờ
                var sensorRecord = new SensorRecordModel
                {
                    BeginTime = beginTime,  // Thời gian bắt đầu của mỗi giờ
                    EndTime = endTime,      // Thời gian kết thúc của mỗi giờ
                    Value = sensorValue,    // Giá trị của cảm biến
                    Date = DateTime.UtcNow.Date.AddHours(hour)  // Ngày và giờ ghi nhận (cùng ngày)
                };

                sensorRecords.Add(sensorRecord); // Thêm record vào danh sách
            }

            // Chuyển đổi list các SensorRecordModel thành JSON
            return JsonConvert.SerializeObject(sensorRecords);
        }

        [HttpPost("update/tasks-to-done-and-log-by-date")]
        public IActionResult UpdateTasksAndLogByDate()
        {
            try
            {
                // Lấy ngày Việt Nam hiện tại
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var vietnamToday = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).Date;

                var updatedCount = 0;
                var foodLogs = new List<DailyFoodUsageLog>();

                // Lấy các Cage có FarmingBatch đang Completed
                var cageIds = _context.Cages
                    .Where(c => c.FarmingBatches.Any(fb => fb.Status == FarmingBatchStatusEnum.Completed))
                    .Select(c => c.Id)
                    .ToList();

                // Lấy tất cả Task chưa hoàn thành có DueDate <= hôm nay và nằm trong cage hợp lệ
                var tasks = _context.Tasks
                    .Where(t =>
                        t.Status != TaskStatusEnum.Done &&
                        t.DueDate.HasValue &&
                        t.DueDate.Value.Date <= vietnamToday &&
                        cageIds.Contains(t.CageId))
                    .ToList();

                foreach (var task in tasks)
                {
                    task.Status = TaskStatusEnum.Done;
                    task.CompletedAt = DateTimeUtils.GetServerTimeInVietnamTime();
                    updatedCount++;

                    // Nếu là task "Cho ăn" thì tạo log
                    if (task.TaskName == "Cho ăn")
                    {
                        var batch = _context.FarmingBatchs.FirstOrDefault(b =>
                            b.CageId == task.CageId &&
                            b.Status == FarmingBatchStatusEnum.Completed);

                        if (batch != null)
                        {
                            var stage = _context.GrowthStages.FirstOrDefault(s =>
                                s.FarmingBatchId == batch.Id &&
                                s.AgeStartDate <= task.DueDate &&
                                s.AgeEndDate >= task.DueDate);

                            if (stage != null)
                            {
                                foodLogs.Add(new DailyFoodUsageLog
                                {
                                    Id = Guid.NewGuid(),
                                    StageId = stage.Id,
                                    RecommendedWeight = stage.Quantity * stage.RecommendedWeightPerSession,
                                    ActualWeight = stage.Quantity * stage.RecommendedWeightPerSession,
                                    Notes = "Ghi nhận cho ăn tự động (theo ngày)",
                                    LogTime = task.DueDate,
                                    UnitPrice = 15000,
                                    Photo = "log_food_auto.jpg",
                                    TaskId = task.Id
                                });
                            }
                        }
                    }
                }

                _context.Tasks.UpdateRange(tasks);
                if (foodLogs.Any())
                {
                    _context.DailyFoodUsageLogs.AddRange(foodLogs);
                }

                _context.SaveChanges();

                return Ok($"✅ Đã cập nhật {updatedCount} task thành 'Done', thêm {foodLogs.Count} log cho ăn.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Lỗi khi cập nhật dữ liệu: {ex.Message}");
            }
        }

        [HttpPost("newFarmingBatch")]
        public IActionResult CreateNewFarmingBatch([FromQuery] DateTime dateEstimated)
        {
            try
            {
                var template = _context.AnimalTemplates.FirstOrDefault(t => t.Name == "Gà nuôi thịt - Cobb-500");
                if (template == null)
                    return BadRequest("Không tìm thấy mẫu chăn nuôi Gà nuôi thịt - Cobb-500.");

                var farmingBatch = new FarmingBatch
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    CageId = Guid.Parse("f37f0727-435d-4d80-9c29-ae2f41b49c9d"),
                    FarmingBatchCode = $"FB-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    Name = "Gà nuôi thịt - Cobb-500",
                    StartDate = null,
                    EstimatedTimeStart = dateEstimated,
                    EndDate = null,
                    Status = FarmingBatchStatusEnum.Planning,
                    CleaningFrequency = 2,
                    Quantity = 200,
                    DeadQuantity = 0,
                    FarmId = Guid.Parse("7b0ad5a5-ca3e-45b1-9519-d42135d5bea4")
                };

                _context.FarmingBatchs.Add(farmingBatch);
                _context.SaveChanges();

                var growthStageTemplates = _context.GrowthStageTemplates
                    .Where(g => g.TemplateId == template.Id)
                    .ToList();

                var growthStages = new List<GrowthStage>();
                var taskDailies = new List<TaskDaily>();
                var vaccineSchedules = new List<VaccineSchedule>();

                foreach (var stageTemplate in growthStageTemplates)
                {

                    var foodTemplate = _context.FoodTemplates.FirstOrDefault(f => f.StageTemplateId == stageTemplate.Id);

                    var growthStage = new GrowthStage
                    {
                        Id = Guid.NewGuid(),
                        FarmingBatchId = farmingBatch.Id,
                        Name = stageTemplate.StageName,
                        WeightAnimal = stageTemplate.WeightAnimal,
                        WeightAnimalExpect = stageTemplate.WeightAnimal,
                        Quantity = 200,
                        AgeStart = stageTemplate.AgeStart,
                        AgeEnd = stageTemplate.AgeEnd,
                        FoodType = foodTemplate?.FoodType ?? "Không xác định",
                        AgeStartDate = null,
                        AgeEndDate = null,
                        Status = GrowthStageStatusEnum.Planning,
                        DeadQuantity = 0,
                        AffectedQuantity = 0,
                        WeightBasedOnBodyMass = foodTemplate?.WeightBasedOnBodyMass ?? 0,
                        RecommendedWeightPerSession = stageTemplate.WeightAnimal * (foodTemplate?.WeightBasedOnBodyMass ?? 0)
                    };

                    growthStages.Add(growthStage);

                    var taskDailyTemplates = _context.TaskDailyTemplates.Where(t => t.GrowthStageTemplateId == stageTemplate.Id).ToList();

                    foreach (var taskTemplate in taskDailyTemplates)
                    {
                        taskDailies.Add(new TaskDaily
                        {
                            Id = Guid.NewGuid(),
                            GrowthStageId = growthStage.Id,
                            TaskTypeId = taskTemplate.TaskTypeId,
                            TaskName = taskTemplate.TaskName,
                            Description = taskTemplate.Description,
                            Session = taskTemplate.Session,
                            StartAt = null,
                            EndAt = null
                        });
                    }

                    var vaccineTemplates = _context.VaccineTemplates
                        .Where(v => v.TemplateId == template.Id &&
                                    v.ApplicationAge >= growthStage.AgeStart &&
                                    v.ApplicationAge <= growthStage.AgeEnd)
                        .ToList();

                    foreach (var vaccine in vaccineTemplates)
                    {
                        var vaccineData = _context.Vaccines.FirstOrDefault(v => v.Name == vaccine.VaccineName);
                        if (vaccineData != null)
                        {
                            vaccineSchedules.Add(new VaccineSchedule
                            {
                                Id = Guid.NewGuid(),
                                StageId = growthStage.Id,
                                VaccineId = vaccineData.Id,
                                Date = null,
                                Quantity = 200,
                                ApplicationAge = vaccine.ApplicationAge,
                                ToltalPrice = 200 * (decimal)vaccineData.Price,
                                Session = vaccine.Session,
                                Status = VaccineScheduleStatusEnum.Upcoming,
                            });
                        }
                    }
                }

                _context.GrowthStages.AddRange(growthStages);
                _context.TaskDailies.AddRange(taskDailies);
                _context.VaccineSchedules.AddRange(vaccineSchedules);
                _context.SaveChanges();

                return Ok("Đã tạo vụ nuôi mới");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Lỗi khi cập nhật dữ liệu: {ex.Message}");
            }
        }

        [HttpPost("newPrescription")]
        public async Task<IActionResult> CreateNewPrescription()
        {
            try
            {
                // ID thực tế của disease cần test (ví dụ: Dịch tả gà)
                var diseaseId = _context.Diseases.FirstOrDefault(d => d.Name == "Dịch tả gà")?.Id;
                if (diseaseId == null) return BadRequest("Không tìm thấy bệnh");

                // Lấy StandardPrescription mẫu
                var standardPrescription = _context.StandardPrescriptions
                    .Where(sp => sp.DiseaseId == diseaseId)
                    .FirstOrDefault();

                if (standardPrescription == null) return BadRequest("Không tìm thấy đơn thuốc mẫu");

                var farmingBatch = await _context.FarmingBatchs.Where(fb => fb.CageId == Guid.Parse("F37F0727-435D-4D80-9C29-AE2F41B49C9D")).FirstOrDefaultAsync();
                // Tạo MedicalSymptom
                var symptom = new MedicalSymptom
                {
                    Id = Guid.NewGuid(),
                    FarmingBatchId = farmingBatch.Id, // farming batch cụ thể
                    Diagnosis = "Ủ rũ, kém hoạt động, Giảm ăn, bỏ ăn",
                    Status = MedicalSymptomStatuseEnum.Prescribed,
                    AffectedQuantity = 20,
                    QuantityInCage = 200,
                    IsEmergency = false,
                    Notes = "Phát hiện nghi nhiễm dịch tả",
                    CreateAt = DateTimeUtils.GetServerTimeInVietnamTime().AddDays(-10),
                    DiseaseId = diseaseId
                };

                // Tạo Prescription từ mẫu
                var prescription = new Prescription
                {
                    Id = Guid.NewGuid(),
                    MedicalSymtomId = symptom.Id,
                    CageId = Guid.Parse("F37F0727-435D-4D80-9C29-AE2F41B49C9D"),
                    PrescribedDate = DateTimeUtils.GetServerTimeInVietnamTime().AddDays(-10),
                    EndDate = DateTimeUtils.GetServerTimeInVietnamTime().AddDays(-10 + standardPrescription.RecommendDay),
                    Notes = "Đơn thuốc từ mẫu chuẩn",
                    QuantityAnimal = 20,
                    RemainingQuantity = 20,
                    Status = PrescriptionStatusEnum.Completed,
                    DaysToTake = standardPrescription.RecommendDay,
                    Price = 0 // sẽ tính bên dưới
                };

                // Tạo danh sách PrescriptionMedications từ StandardPrescriptionMedications
                var stdMeds = _context.StandardPrescriptionMedications
                    .Where(m => m.PrescriptionId == standardPrescription.Id)
                    .ToList();

                var prescriptionMeds = stdMeds.Select(m => new PrescriptionMedication
                {
                    Id = Guid.NewGuid(),
                    PrescriptionId = prescription.Id,
                    MedicationId = m.MedicationId,
                    Morning = m.Morning,
                    Noon = m.Noon,
                    Afternoon = m.Afternoon,
                    Evening = m.Evening
                }).ToList();

                // Tính tổng giá đơn thuốc (nếu cần)
                var medicationPrices = _context.Medications
                    .Where(m => stdMeds.Select(x => x.MedicationId).Contains(m.Id))
                    .ToDictionary(m => m.Id, m => m.PricePerDose ?? 0);

                prescription.Price = prescriptionMeds.Sum(pm => (medicationPrices.ContainsKey(pm.MedicationId) ? medicationPrices[pm.MedicationId] : 0) * prescription.DaysToTake ?? 1);

                // Add vào DB
                _context.MedicalSymptoms.Add(symptom);
                _context.Prescriptions.Add(prescription);
                _context.PrescriptionMedications.AddRange(prescriptionMeds);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Lỗi khi cập nhật dữ liệu: {ex.Message}");
            }
        }
    }
}
