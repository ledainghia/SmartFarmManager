using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class Farm : EntityBase
{

    [StringLength(100)]
    public string FarmName { get; set; } = null!;

    [StringLength(255)]
    public string? Location { get; set; }

    public int OwnerId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Farm")]
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    [InverseProperty("Farm")]
    public virtual ICollection<CameraSurveillance> CameraSurveillances { get; set; } = new List<CameraSurveillance>();

    [InverseProperty("Farm")]
    public virtual ICollection<FarmStaffAssignment> FarmStaffAssignments { get; set; } = new List<FarmStaffAssignment>();

    [InverseProperty("Farm")]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    [InverseProperty("Farm")]
    public virtual ICollection<IoTDevice> IoTdevices { get; set; } = new List<IoTDevice>();

    [InverseProperty("Farm")]
    public virtual ICollection<Livestock> Livestocks { get; set; } = new List<Livestock>();

    [ForeignKey("OwnerId")]
    [InverseProperty("Farms")]
    public virtual User Owner { get; set; } = null!;

    [InverseProperty("Farm")]
    public virtual ICollection<RevenueAndProfitReport> RevenueAndProfitReports { get; set; } = new List<RevenueAndProfitReport>();

    [InverseProperty("Farm")]
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
