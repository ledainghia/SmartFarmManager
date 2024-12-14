using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Auth
{
    public class UserProfileModel : UserModel
    {
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
