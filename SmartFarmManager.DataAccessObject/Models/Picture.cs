﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class Picture : EntityBase
{

    public Guid RecordId { get; set; }

    public string Image { get; set; }

    public DateTime? DateCaptured { get; set; }

    public virtual MedicalSymptom Record { get; set; }
}