using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class Task : EntityBase
{

    [StringLength(100)]
    public string TaskName { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    public int? AssignedToUserId { get; set; }

    public int? FarmId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DueDate { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [StringLength(50)]
    public string TaskType { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CompletedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
    public int CreatedBy { get; set; }  // ID của người tạo
    public int? ModifiedBy { get; set; } // ID của người chỉnh sửa (có thể null)

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedAt { get; set; } // Thời gian chỉnh sửa

    [ForeignKey("AssignedToUserId")]
    [InverseProperty("Tasks")]
    public virtual User AssignedToUser { get; set; } = null!;

    [ForeignKey("FarmId")]
    [InverseProperty("Tasks")]
    public virtual Farm Farm { get; set; } = null!;

    [InverseProperty("Task")]
    public virtual ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
}
