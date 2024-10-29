using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class LivestockExpense : EntityBase
{

    public int LivestockId { get; set; }

    [StringLength(50)]
    public string ExpenseType { get; set; } = null!;

    public double Amount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpenseDate { get; set; }

    [ForeignKey("LivestockId")]
    [InverseProperty("LivestockExpenses")]
    public virtual Livestock Livestock { get; set; } = null!;
}
