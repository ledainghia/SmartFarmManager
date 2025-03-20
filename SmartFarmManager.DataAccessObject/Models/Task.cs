﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class Task : EntityBase
{
    public Guid? TaskTypeId { get; set; }

    public Guid CageId { get; set; }

    public Guid AssignedToUserId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public string TaskName { get; set; }

    public int PriorityNum { get; set; }

    public string Description { get; set; }

    public DateTime? DueDate { get; set; }

    public string Status { get; set; }
    public int Session { get; set; }
    public bool IsWarning { get;set; } 
    public DateTime? CompletedAt { get; set; }
    public DateTime? CreatedAt { get; set; }

    public bool IsTreatmentTask { get; set; }
    public Guid? PrescriptionId { get; set; }

    public virtual User AssignedToUser { get; set; }

    public virtual Cage Cage { get; set; }

    public virtual ICollection<StatusLog> StatusLogs { get; set; } = new List<StatusLog>();

    public virtual TaskType TaskType { get; set; }
}