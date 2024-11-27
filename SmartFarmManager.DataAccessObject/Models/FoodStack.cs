﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class FoodStack : EntityBase
{    

    public Guid FarmId { get; set; }

    public string NameFood { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? CostPerKg { get; set; }

    public decimal? CurrentStock { get; set; }

    public virtual Farm Farm { get; set; }

    public virtual ICollection<StockLog> StockLogs { get; set; } = new List<StockLog>();
}