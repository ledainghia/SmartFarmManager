using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Index("PermissionName", Name = "UQ__Permissi__0FFDA357F2115A31", IsUnique = true)]
public partial class Permission : EntityBase
{

    [StringLength(50)]
    public string PermissionName { get; set; } = null!;

    [StringLength(255)]
    public string? RoleType { get; set; }

    [InverseProperty("Permission")]
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
