using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Index("AlertTypeName", Name = "UQ__AlertTyp__AB2F036A77E854D4", IsUnique = true)]
public partial class AlertType : EntityBase
{

    [StringLength(50)]
    public string AlertTypeName { get; set; } = null!;

    [InverseProperty("AlertType")]
    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
