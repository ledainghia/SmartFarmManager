﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.AnimalTemplate
{
    public class UpdateAnimalTemplateModel
    {
        public string? Name { get; set; }
        public string? Species { get; set; }
        public string? Notes { get; set; }
    }
}
