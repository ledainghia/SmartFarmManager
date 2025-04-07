﻿using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.GrowthStage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FarmingBatch
{
    public class FarmingBatchModel
    {
        public Guid Id { get; set; }
        public string FarmingbatchCode { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompleteAt { get; set; }
        public DateTime? EstimatedTimeStart { get; set; }
        public DateTime? EndDate { get; set; }

        public string Status { get; set; }
        public int CleaningFrequency { get; set; }
        public int? Quantity { get; set; }
        public int? DeadQuantity { get; set; }
        public GrowthStageDetailModel GrowthStageDetails { get; set; }
        public CageModel Cage { get; set; }
        public AnimalTemplateItemModel Template { get; set; }
    }
}
