﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class SensorType : EntityBase
{

    public string Name { get; set; }

    public string Description { get; set; }

    public string FieldName { get; set; }

    public string Unit { get; set; }

    public int DefaultPinCode { get; set; }

    public virtual ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
}