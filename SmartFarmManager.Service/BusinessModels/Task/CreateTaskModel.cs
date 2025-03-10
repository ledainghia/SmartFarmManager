using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class CreateTaskModel
    {
        public Guid? TaskTypeId { get; set; }
        public Guid CageId { get; set; }
        public Guid AssignedToUserId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public List<int>? Session { get; set; }
    }
}
