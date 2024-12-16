using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.GrowthStageTemplate
{
    public class GrowthStageTemplateDetailResponseModel
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string StageName { get; set; }
        public decimal? WeightAnimal { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public string? Notes { get; set; }
        public List<FoodTemplateResponse> FoodTemplates { get; set; } = new List<FoodTemplateResponse>();
        public List<TaskDailyTemplateResponse> TaskDailyTemplates { get; set; } = new List<TaskDailyTemplateResponse>();
    }


    public class FoodTemplateResponse
    {
        public Guid Id { get; set; }
        public string FoodName { get; set; }
        public decimal? RecommendedWeightPerDay { get; set; }
        public int Session { get; set; }
        public decimal? WeightBasedOnBodyMass { get; set; }
    }

    public class TaskDailyTemplateResponse
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; }
        public string? Description { get; set; }
        public int Session { get; set; }
        public TaskTypeResponse? TaskType { get; set; }
    }

    public class TaskTypeResponse
    {
        public Guid Id { get; set; }
        public string TaskTypeName { get; set; }
        public int? PriorityNum { get; set; }
    }
}
