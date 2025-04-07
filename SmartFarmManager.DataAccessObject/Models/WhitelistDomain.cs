using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class WhitelistDomain: EntityBase
    {
        public string Domain { get; set; } // Tên domain được phép
        public string ApiKey { get; set; } // API Key được cấp cho domain này
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true; // Đánh dấu domain có đang hoạt động không
    }
}
