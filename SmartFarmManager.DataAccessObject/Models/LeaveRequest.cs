using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public class LeaveRequest:EntityBase
    {

        public Guid StaffFarmId { get; set; }
        public Guid UserTempId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; } = "Pending";


        public DateTime? ReviewedAt { get; set; }

        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual User StaffFarm { get; set; }

    }
}
