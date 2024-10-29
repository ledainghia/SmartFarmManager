using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class AlertUser : EntityBase
{

    public int AlertId { get; set; }

    public int UserId { get; set; }

    [ForeignKey("AlertId")]
    [InverseProperty("AlertUsers")]
    public virtual Alert Alert { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("AlertUsers")]
    public virtual User User { get; set; } = null!;
}
