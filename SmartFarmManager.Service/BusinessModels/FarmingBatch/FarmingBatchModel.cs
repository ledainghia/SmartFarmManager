using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels.Cages;
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
        public string Name { get; set; }
        public string Species { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompleteAt { get; set; }
        public string Status { get; set; }
        public int CleaningFrequency { get; set; }
        public int? Quantity { get; set; }
        public CageModel Cage { get; set; }
        public AnimalTemplateItemModel Template { get; set; }
    }
}
