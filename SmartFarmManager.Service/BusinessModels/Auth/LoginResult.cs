using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Auth
{
    public class LoginResult
    {
        public bool Authenticated { get; set; }
        public SecurityToken? Token { get; set; }

        public SecurityToken? RefreshToken { get; set; }
    }
}

