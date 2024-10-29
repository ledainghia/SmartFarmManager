using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Table("IoTDevices")]
public partial class IoTDevice : EntityBase
{

    [StringLength(50)]
    public string DeviceType { get; set; } = null!;

    public int FarmId { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? InstallDate { get; set; }

    [InverseProperty("Device")]
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    [InverseProperty("Device")]
    public virtual ICollection<DeviceReading> DeviceReadings { get; set; } = new List<DeviceReading>();

    [ForeignKey("FarmId")]
    [InverseProperty("IoTdevices")]
    public virtual Farm Farm { get; set; } = null!;
}
