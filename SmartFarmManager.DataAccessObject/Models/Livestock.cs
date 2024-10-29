using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Table("Livestock")]
public partial class Livestock : EntityBase
{

    public int FarmId { get; set; }

    [StringLength(50)]
    public string Species { get; set; } = null!;

    public int Quantity { get; set; }

    [StringLength(50)]
    public string? GrowthCycle { get; set; }

    public double? Expenses { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("FarmId")]
    [InverseProperty("Livestocks")]
    public virtual Farm Farm { get; set; } = null!;

    [InverseProperty("Livestock")]
    public virtual ICollection<LivestockExpense> LivestockExpenses { get; set; } = new List<LivestockExpense>();

    [InverseProperty("Livestock")]
    public virtual ICollection<LivestockSale> LivestockSales { get; set; } = new List<LivestockSale>();
}
