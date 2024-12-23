using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public class TemporaryCageAssignment:EntityBase
    {
        public Guid CageId { get; set; }

        public Guid OriginalStaffId { get; set; }

        public Guid TemporaryStaffId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Notes { get; set; }

        // Navigation Properties
        public virtual Cage Cage { get; set; }

        public virtual User OriginalStaff { get; set; }

        public virtual User TemporaryStaff { get; set; }
    }
}
