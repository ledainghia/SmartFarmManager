﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FarmingBatch
{
    public class CreateFarmingBatchModel
    {
        public Guid TemplateId { get; set; }
        public Guid CageId { get; set; }
        public string Name { get; set; }
        public int CleaningFrequency { get; set; }
        public int Quantity { get; set; }
        public DateTime? EstimatedTimeStart { get; set; }    
    }

}
