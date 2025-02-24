using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class VaccineScheduleService :IVaccineScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VaccineScheduleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<VaccineScheduleResponse>> GetVaccineSchedulesAsync(
    Guid? stageId, DateTime? date, string status)
        {
            var query = _unitOfWork.VaccineSchedules
                .FindAll()
                .Include(vs => vs.Vaccine) // Join đến Vaccine để lấy tên
                .AsQueryable();

            // 1️⃣ Lọc theo StageId nếu có
            if (stageId.HasValue)
            {
                query = query.Where(vs => vs.StageId == stageId.Value);
            }

            // 2️⃣ Lọc theo Date nếu có
            if (date.HasValue)
            {
                query = query.Where(vs => vs.Date.Value.Date == date.Value.Date);
            }

            // 3️⃣ Lọc theo Status nếu có
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(vs => vs.Status == status);
            }

            // 4️⃣ Thực thi truy vấn và ánh xạ vào response
            var result = await query.Select(vs => new VaccineScheduleResponse
            {
                VaccineScheduleId = vs.Id,
                StageId = vs.StageId,
                VaccineId = vs.VaccineId,
                VaccineName = vs.Vaccine.Name, // Lấy tên vaccine
                Date = vs.Date,
                Quantity = vs.Quantity,
                ApplicationAge = vs.ApplicationAge,
                TotalPrice = vs.ToltalPrice,
                Session = vs.Session,
                Status = vs.Status
            }).ToListAsync();

            return result;
        }

    }
}
