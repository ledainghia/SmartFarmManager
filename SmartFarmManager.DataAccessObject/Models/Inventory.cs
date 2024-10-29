using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Table("Inventory")]
public partial class Inventory : EntityBase
{

    public int FarmId { get; set; }

    [StringLength(100)]
    public string ItemName { get; set; } = null!;

    public int Quantity { get; set; }

    [StringLength(50)]
    public string? Unit { get; set; }

    public double CostPerUnit { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpirationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("FarmId")]
    [InverseProperty("Inventories")]
    public virtual Farm Farm { get; set; } = null!;

    [InverseProperty("Inventory")]
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
}
