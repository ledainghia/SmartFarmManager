using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Auth
{
    public class TokenResult
    {
        public SecurityToken? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

}
