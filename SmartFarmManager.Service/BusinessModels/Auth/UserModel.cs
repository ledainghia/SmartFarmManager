using SmartFarmManager.Service.BusinessModels.Cages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Auth
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public List<CageModel> Cages { get; set; } = new List<CageModel>();
        public List<TaskStatusCountModel> TasksCountByStatus { get; set; } = new List<TaskStatusCountModel>();
    }

    public class TaskStatusCountModel
    {
        public string Status { get; set; }  
        public int Count { get; set; }      
    }


}
