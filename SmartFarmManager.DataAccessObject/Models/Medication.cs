﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class Medication : EntityBase
{
    public string Name { get; set; }

    public string UsageInstructions { get; set; }

    public decimal? Price { get; set; }

    public int? DoseQuantity { get; set; }

    public decimal? PricePerDose { get; set; }

    public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
}