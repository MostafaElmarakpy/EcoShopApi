using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using EcoShopApi.Application.DTO.AuthDTO;

namespace EcoShopApi.Application.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;


        public AuthService(
             UserManager<AppUser> userManager,
             IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AppUser> GetUserByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("Username cannot be null or empty.", nameof(userName));

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                throw new InvalidOperationException($"User with username '{userName}' not found.");
            return user;
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName) != null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }




        public async Task UpdateUserAsync(AppUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new Exception($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }


        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public Task CreateUserAsync(AppUser user, string password)
        {
            var result = _userManager.CreateAsync(user, password);
            if (!result.Result.Succeeded)
            {
                throw new Exception($"User creation failed: {string.Join(", ", result.Result.Errors.Select(e => e.Description))}");
            }
            return result;
        }

        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            //get by email by user manager and add validation
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException($"User with email '{email}' not found.");
            return user;
        }

        public async Task<string> GenerateJwtTokenAsync(AppUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add user roles to claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenMinutes"] ?? "15")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber =  new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<string> GenerateAndStoreRefreshTokenAsync(string userId)
        {
            var refreshToken = await GenerateRefreshTokenAsync();
            await StoreRefreshTokenAsync(userId, refreshToken);
            return refreshToken;
        }
        public async Task StoreRefreshTokenAsync(string userId, string refreshToken)
        {
            // In a real-world application, you would store the refresh tokens in a database or cache.
            // Here, we simply simulate the storage process.
            await Task.CompletedTask;
        }
        public async Task<bool> LogoutAsync(string userId, string RefreshToken)
        {
            // In a real-world application, you would remove the refresh token from the database or cache.
            // Here, we simply simulate the removal process.
            await Task.CompletedTask;
            return true;

        }
    }
}
