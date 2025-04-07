﻿using SmartFarmManager.Service.BusinessModels.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResult> Login(string username, string password);
        Task<TokenResult> RefreshTokenAsync(string refreshToken);

        bool ValidateToken(string token);
        Task Logout(Guid userId);
    }
}
