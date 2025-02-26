﻿using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IVaccineScheduleService
    {
        Task<List<VaccineScheduleResponse>> GetVaccineSchedulesAsync(Guid? stageId, DateTime? date, string status);
        Task<VaccineScheduleResponse> GetVaccineScheduleByIdAsync(Guid id);
    }
}
