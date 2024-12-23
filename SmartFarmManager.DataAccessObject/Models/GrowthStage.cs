﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class GrowthStage : EntityBase
{

    public Guid FarmingBatchId { get; set; }


    public string Name { get; set; }

    public decimal? WeightAnimal { get; set; }

    public int? Quantity { get; set; }
    public int? AgeStart { get; set; }

    public int? AgeEnd { get; set; }

    public DateTime? AgeStartDate { get; set; }

    public DateTime? AgeEndDate { get; set; }
    public string Status { get; set; }

    public decimal? RecommendedWeightPerSession { get; set; }

    public decimal? WeightBasedOnBodyMass { get; set; }

    public virtual ICollection<DailyFoodUsageLog> DailyFoodUsageLogs { get; set; } = new List<DailyFoodUsageLog>();
    public ICollection<TaskDaily> TaskDailies { get; set; } = new List<TaskDaily>();
    public virtual FarmingBatch FarmingBatch { get; set; }

    public virtual ICollection<VaccineSchedule> VaccineSchedules { get; set; } = new List<VaccineSchedule>();
}