﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineSchedule
{
    public class VaccineScheduleFilterKeySearchModel
    {
        public string? KeySearch { get; set; }
        public Guid? VaccineId { get; set; }
        public Guid? StageId { get; set; }
        public DateTime? Date { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
