﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class VaccineTemplate : EntityBase
{

    public Guid TemplateId { get; set; }

    public string VaccineName { get; set; }

    public string ApplicationMethod { get; set; }

    public int? ApplicationAge { get; set; }
    public int Session { get; set; }

    public virtual AnimalTemplate Template { get; set; }
}