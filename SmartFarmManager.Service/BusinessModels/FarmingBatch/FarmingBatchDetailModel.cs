using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FarmingBatch
{
    public class FarmingBatchDetailModel
    {
        public Guid Id { get; set; }
        public string FarmingBatchCode { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompleteAt { get; set; }
        public DateTime? EstimatedTimeStart { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int CleaningFrequency { get; set; }
        public int DeadQuantity { get; set; }
        public int? Quantity { get; set; }
        public Guid FarmId { get; set; }
        public Guid CageId { get; set; }
        public string CageName { get; set; }
        public List<AnimalSaleDetaiInFarmingBatchlModel> AnimalSales { get; set; }
        public List<GrowthStageDetailInFarmingBactchModel> GrowthStages { get; set; }
        public List<MedicalSymptomInFarmingBatchModel> MedicalSymptoms { get; set; }
        public string TemplateName { get; set; }
    }

    public class AnimalSaleDetaiInFarmingBatchlModel
    {
        public Guid Id { get; set; }
        public DateTime SaleDate { get; set; }
        public double Total { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public Guid StaffId { get; set; }
        public Guid SaleTypeId { get; set; }
    }
    public class GrowthStageDetailInFarmingBactchModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? AgeStartDate { get; set; }
        public DateTime? AgeEndDate { get; set; }
        public decimal? WeightAnimal { get; set; }
        public int? Quantity { get; set; }
        public string FoodType { get; set; }
        public string Status { get; set; }
        public int? AffectQuantity { get; set; }
        public int? DeadQuantity { get; set; }
        public decimal? RecommendedWeightPerSession { get; set; }
        public decimal? WeightBasedOnBodyMass { get; set; }
    }
    public class MedicalSymptomInFarmingBatchModel
    {
        public Guid Id { get; set; }
        public string Diagnosis { get; set; }
        public string Status { get; set; }
        public bool IsEmergency { get; set; }
        public string Notes { get; set; }
        public int? AffectedQuantity { get; set; }
        public int? QuantityInCage { get; set; }
    }
}
