using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Table("CameraSurveillance")]
public partial class CameraSurveillance : EntityBase
{

    public int FarmId { get; set; }

    [StringLength(255)]
    public string? CameraLocation { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? InstallDate { get; set; }

    [ForeignKey("FarmId")]
    [InverseProperty("CameraSurveillances")]
    public virtual Farm Farm { get; set; } = null!;
}
