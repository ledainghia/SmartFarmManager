using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class InventoryTransaction : EntityBase
{

    public int InventoryId { get; set; }

    [StringLength(50)]
    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Date { get; set; }

    [StringLength(255)]
    public string? Purpose { get; set; }

    [ForeignKey("InventoryId")]
    [InverseProperty("InventoryTransactions")]
    public virtual Inventory Inventory { get; set; } = null!;
}
