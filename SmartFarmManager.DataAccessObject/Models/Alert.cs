using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class Alert : EntityBase
{
    public int DeviceId { get; set; }

    public int AlertTypeId { get; set; }

    public int FarmId { get; set; }

    [StringLength(255)]
    public string? Message { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? AcknowledgedAt { get; set; }

    [ForeignKey("AlertTypeId")]
    [InverseProperty("Alerts")]
    public virtual AlertType AlertType { get; set; } = null!;

    [InverseProperty("Alert")]
    public virtual ICollection<AlertUser> AlertUsers { get; set; } = new List<AlertUser>();

    [ForeignKey("DeviceId")]
    [InverseProperty("Alerts")]
    public virtual IoTDevice Device { get; set; } = null!;

    [ForeignKey("FarmId")]
    [InverseProperty("Alerts")]
    public virtual Farm Farm { get; set; } = null!;
}
