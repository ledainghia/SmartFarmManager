using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.WhiteListDomain
{
    public class WhiteListDomainItemModel
    {     
            public string Domain { get; set; } 
            public string ApiKey { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsActive { get; set; }     
    }
}
