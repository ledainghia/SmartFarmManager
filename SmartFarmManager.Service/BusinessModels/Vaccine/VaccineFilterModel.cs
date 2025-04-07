﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Vaccine
{
    public class VaccineFilterModel
    {
        public string? Name { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
