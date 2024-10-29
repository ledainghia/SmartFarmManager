using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class FarmStaffAssignment : EntityBase
{

    public int FarmId { get; set; }

    public int FarmStaffId { get; set; }

    [ForeignKey("FarmId")]
    [InverseProperty("FarmStaffAssignments")]
    public virtual Farm Farm { get; set; } = null!;

    [ForeignKey("FarmStaffId")]
    [InverseProperty("FarmStaffAssignments")]
    public virtual User FarmStaff { get; set; } = null!;
}
