﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class FarmingBatch : EntityBase
{

    public Guid TemplateId { get; set; }

    public Guid CageId { get; set; }
    public string FarmingBatchCode { get; set; }
    public string Name { get; set; }
    public DateTime? StartDate { get; set; } // Ngày vụ nuôi băt đầu
    public DateTime? CompleteAt { get; set; } // Ngày vụ nuôi hoàn thành
    public DateTime? EstimatedTimeStart { get; set; } // Thời gian dự kiến
    public DateTime? EndDate { get; set; } // Ngày kết thúc dự kiến
    public string Status { get; set; }

    public int CleaningFrequency { get; set; }
    public int DeadQuantity { get; set; } = 0;

    public int? Quantity { get; set; }
    public Guid FarmId { get; set; }

    public virtual ICollection<AnimalSale> AnimalSales { get; set; } = new List<AnimalSale>();

    public virtual Cage Cage { get; set; }

    public virtual ICollection<GrowthStage> GrowthStages { get; set; } = new List<GrowthStage>();

    public virtual ICollection<MedicalSymptom> MedicalSymptoms { get; set; } = new List<MedicalSymptom>();

    public virtual AnimalTemplate Template { get; set; }
}