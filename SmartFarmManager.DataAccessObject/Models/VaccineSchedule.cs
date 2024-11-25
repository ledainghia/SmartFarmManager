﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class VaccineSchedule
{
    public Guid ScheduleId { get; set; }

    public Guid VaccineId { get; set; }

    public Guid StageId { get; set; }

    public DateOnly? Date { get; set; }

    public int? Quantity { get; set; }

    public string Status { get; set; }

    public virtual GrowthStage Stage { get; set; }

    public virtual Vaccine Vaccine { get; set; }

    public virtual ICollection<VaccineScheduleLog> VaccineScheduleLogs { get; set; } = new List<VaccineScheduleLog>();
}