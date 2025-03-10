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
        public string? ImageUrl { get; set; }
        public List<CageModel> Cages { get; set; } = new List<CageModel>();
        public TaskStatusCountModel TasksCountByStatus { get; set; }
    }

    public class TaskStatusCountModel
    {
        public int Pending { get; set; }
        public int InProgress { get; set; }
        public int Done { get; set; }
        public int Overdue { get; set; }
        public int Cancelled { get; set; }
    }


}
