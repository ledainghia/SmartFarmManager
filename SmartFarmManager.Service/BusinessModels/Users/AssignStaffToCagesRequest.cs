using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Users
{
    public class AssignStaffToCagesRequest
    {
        public Guid StaffId { get; set; }
        public List<Guid> CageIds { get; set; }
    }

}
