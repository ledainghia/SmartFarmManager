using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class UserPermission : EntityBase
{
    public int UserId { get; set; }

    public int PermissionId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? GrantedAt { get; set; }

    [ForeignKey("PermissionId")]
    [InverseProperty("UserPermissions")]
    public virtual Permission Permission { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserPermissions")]
    public virtual User User { get; set; } = null!;
}
