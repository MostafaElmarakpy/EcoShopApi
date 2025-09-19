using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Services.Implementation
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _cfg;

        public TokenService(IConfiguration cfg) => _cfg = cfg;

        
        public string CreateAccessToken(AppUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };
            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_cfg["Jwt:AccessTokenMinutes"]!)),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken CreateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var plainToken = Convert.ToBase64String(randomBytes);
            var hash = HashToken(plainToken); 
            return new RefreshToken
            {
                Token = hash,
                ExpireAt = DateTime.UtcNow.AddDays(double.Parse(_cfg["Jwt:RefreshTokenDays"]!)),
                IsRevoked = false
            };
        }

        public bool ValidateRefreshToken(RefreshToken storedToken, string providedToken) 
        {
            var providedHash = HashToken(providedToken);
            return storedToken.Token == providedHash && !storedToken.IsExpired && !storedToken.IsRevoked;
        }

        private string HashToken(string token) 
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}