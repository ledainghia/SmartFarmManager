using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Users
{
    public class UserPasswordRequest
    {
        public Guid UserId { get; set; }
        public string Password { get; set; }
    }
}
