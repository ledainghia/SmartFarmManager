using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class RevenueAndProfitReport : EntityBase
{


    public int FarmId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EndDate { get; set; }

    public double TotalRevenue { get; set; }

    public double TotalExpenses { get; set; }

    public double TotalProfit { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReportGeneratedAt { get; set; }

    [ForeignKey("FarmId")]
    [InverseProperty("RevenueAndProfitReports")]
    public virtual Farm Farm { get; set; } = null!;
}
