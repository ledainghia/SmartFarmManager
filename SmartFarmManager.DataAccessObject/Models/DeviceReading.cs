using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class DeviceReading : EntityBase
{
    public int DeviceId { get; set; }

    [StringLength(50)]
    public string ReadingType { get; set; } = null!;

    public double Value { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Timestamp { get; set; }

    [ForeignKey("DeviceId")]
    [InverseProperty("DeviceReadings")]
    public virtual IoTDevice Device { get; set; } = null!;
}
