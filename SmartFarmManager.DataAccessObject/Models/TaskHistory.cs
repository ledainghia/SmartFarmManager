using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Table("TaskHistory")]
public partial class TaskHistory : EntityBase
{

    public int TaskId { get; set; }

    [StringLength(50)]
    public string? StatusBefore { get; set; }

    [StringLength(50)]
    public string StatusAfter { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? ChangedAt { get; set; }

    [ForeignKey("TaskId")]
    [InverseProperty("TaskHistories")]
    public virtual Task Task { get; set; } = null!;
}
