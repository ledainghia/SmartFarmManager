using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Staff
{
    public class StaffPendingTasksModel
    {
        public Guid StaffId { get; set; }
        public Guid? CageId { get; set; }
        public string CageName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int PendingTaskCount { get; set; }
    }

}
