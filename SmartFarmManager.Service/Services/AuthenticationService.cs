using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Auth;
using SmartFarmManager.Service.Helpers;
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
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public AuthenticationService(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings, IMapper mapper, JwtSecurityTokenHandler tokenHandler)
        {
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
            _mapper = mapper;
            _tokenHandler = tokenHandler;
        }

        public async Task<LoginResult> Login(string username, string password)
        {
            var user = await _unitOfWork.Users.FindByCondition(x => x.Username == username, false, x => x.Role).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception("Username not found.");
            }
            if (!user.PasswordHash.Equals(SecurityUtil.Hash(password)))
            {
                throw new Exception("Incorrect password.");
            }
            return new LoginResult
            {
                Authenticated = true,
                Token = CreateJwtToken(user),
                RefreshToken = CreateJwtRefreshToken(user)
            };
        }

        public async System.Threading.Tasks.Task Logout(Guid userId)
        {
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception("User not found.");
            }
            user.DeviceId = null;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
        }

        public SecurityToken CreateJwtToken(User user)
        {
            var utcNow = DateTimeUtils.GetServerTimeInVietnamTime();
            var authClaims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(ClaimTypes.Role, user.Role.RoleName), //
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(ClaimTypes.Name, user.FullName),
    };

            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = utcNow.Add(TimeSpan.FromMinutes(1)),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);

            return token;
        }


        private SecurityToken CreateJwtRefreshToken(User user)
        {
            var utcNow = DateTimeUtils.GetServerTimeInVietnamTime();
            var authClaims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(ClaimTypes.Role, user.Role.RoleName),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(ClaimTypes.Name, user.FullName),
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
        public async Task<TokenResult> RefreshTokenAsync(string refreshToken)
        {
            // Validate Refresh Token
            var principal = GetPrincipalFromExpiredToken(refreshToken);
            if (principal == null)
            {
                return null;
            }

            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // Check if user exists
            var user = await _unitOfWork.Users.FindByCondition(u => u.Id == userId,false, x => x.Role).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }

            // Generate new Access Token
            var newAccessToken = CreateJwtToken(user);
            return new TokenResult
            {
                AccessToken = newAccessToken,
                RefreshToken = refreshToken // Keep the same refresh token
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true // Ignore expiration for validation
            };

            try
            {
                var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is JwtSecurityToken jwtSecurityToken &&
                    jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true, // Kiểm tra token hết hạn
                    ClockSkew = TimeSpan.Zero // Không cho phép chênh lệch thời gian
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                // Kiểm tra token có đúng kiểu JwtSecurityToken và sử dụng cùng thuật toán
                if (securityToken is JwtSecurityToken jwtSecurityToken &&
                    jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true; // Token hợp lệ
                }

                return false; // Token không hợp lệ
            }
            catch
            {
                return false; // Token không hợp lệ hoặc hết hạn
            }
        }


    }
}
