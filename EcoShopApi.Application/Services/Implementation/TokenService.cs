using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

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
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),

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
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                ExpireAt = DateTime.UtcNow.AddDays(double.Parse(_cfg["Jwt:RefreshTokenDays"]!))
            };
        }


    }
}
