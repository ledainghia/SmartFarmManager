using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class LivestockSale : EntityBase
{
    public int LivestockId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? SaleDate { get; set; }

    public double Revenue { get; set; }

    [StringLength(255)]
    public string? BuyerInfo { get; set; }

    [ForeignKey("LivestockId")]
    [InverseProperty("LivestockSales")]
    public virtual Livestock Livestock { get; set; } = null!;
}
