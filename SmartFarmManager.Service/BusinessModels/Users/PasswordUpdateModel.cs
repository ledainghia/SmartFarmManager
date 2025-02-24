using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Users
{
    public class PasswordUpdateModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
