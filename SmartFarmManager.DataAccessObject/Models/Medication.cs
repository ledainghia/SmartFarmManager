﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class Medication : EntityBase
{
    public string Name { get; set; } // Tên thuốc

    public string UsageInstructions { get; set; } // Hướng dẫn sử dụng

    public decimal? Price { get; set; } // Giá của thuốc

    public int? DoseWeight { get; set; } // Khối lượng liều (tính theo mg)

    public int? Weight { get; set; } // Khối lượng thuốc (tính theo mg)

    public int? DoseQuantity { get; set; } // Số lượng liều (tính theo mg)

    public decimal? PricePerDose { get; set; }

    public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
    public virtual ICollection<StandardPrescriptionMedication> StandardPrescriptionMedications { get; set; } = new List<StandardPrescriptionMedication>();
}