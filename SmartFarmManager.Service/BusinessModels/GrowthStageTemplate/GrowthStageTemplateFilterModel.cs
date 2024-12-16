﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.GrowthStageTemplate
{
    public class GrowthStageTemplateFilterModel
    {
        public Guid? TemplateId { get; set; }
        public string? StageName { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

}