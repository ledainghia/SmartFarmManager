using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

[Index("Username", Name = "UQ__Users__536C85E4FE039EB6", IsUnique = true)]
[Index("Email", Name = "UQ__Users__A9D1053466D7C567", IsUnique = true)]
public partial class User : EntityBase
{

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(100)]
    public string? FullName { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(15)]
    public string? PhoneNumber { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<AlertUser> AlertUsers { get; set; } = new List<AlertUser>();

    [InverseProperty("FarmStaff")]
    public virtual ICollection<FarmStaffAssignment> FarmStaffAssignments { get; set; } = new List<FarmStaffAssignment>();

    [InverseProperty("Owner")]
    public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("AssignedToUser")]
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();

    [InverseProperty("User")]
    public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
