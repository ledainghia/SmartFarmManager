using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Index("RoleName", Name = "UQ__Roles__8A2B61603525D2EC", IsUnique = true)]
public partial class Role : EntityBase
{

    [StringLength(50)]
    public string RoleName { get; set; } = null!;

    [ForeignKey("RoleId")]
    [InverseProperty("Roles")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
