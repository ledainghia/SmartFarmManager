using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class AuthenticationService:IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;

        public AuthenticationService(IUnitOfWork unitOfWork,IMapper mapper, IOptions<JwtSettings> jwtSettingsOptions)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettingsOptions.Value;
            _mapper = mapper;
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            // Tìm người dùng bằng email
            var user = await _unitOfWork.Users.GetUserByUsername(username);

            if (user == null)
            {             
                throw new Exception("Username not found.");
            }

            // Kiểm tra mật khẩu
            if (!user.PasswordHash.Equals(password))
            {
                throw new Exception("Incorrect password.");
            }

            // Đăng nhập thành công, trả về token
            return new LoginResult
            {
                Authenticated = true,
                Token = CreateJwtToken(user),
                RefreshToken = CreateJwtRefreshToken(user)
            };
        }

        #region create token

        public SecurityToken CreateJwtToken(User user)
        {
            var utcNow = DateTime.UtcNow;
            var authClaims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(ClaimTypes.Role, user.Roles.Select(x=>x.RoleName).FirstOrDefault()), //
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = utcNow.Add(TimeSpan.FromHours(1)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);

            return token;
        }

        private SecurityToken CreateJwtRefreshToken(User user)
        {
            var utcNow = DateTime.UtcNow;
            var authClaims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(ClaimTypes.Role, user.Roles.Select(r=>r.RoleName).FirstOrDefault()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = utcNow.Add(TimeSpan.FromDays(5)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);

            return token;
        }
        #endregion



    }
}
