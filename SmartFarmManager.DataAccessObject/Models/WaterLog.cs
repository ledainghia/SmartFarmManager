﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class WaterLog : EntityBase
{

    public Guid FarmId { get; set; }

    public double Data { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public double? FirstIndexData { get; set; }

    public double? LastIndexData { get; set; }

    public virtual Farm Farm { get; set; }
}