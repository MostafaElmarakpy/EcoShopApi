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
using EcoShopApi.Application.Common.DTO.UserDTO;

namespace EcoShopApi.Application.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;


        public AuthService(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
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

        public async Task<IdentityResult> CreateUserAsync(AppUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        public async Task<AppUser> GetUserByEmailAsync(string email)
        {

            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _userManager.FindByEmailAsync(email);

            //get by email by user manager and add validation
            //if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            //var user = await _userManager.FindByEmailAsync(email);
            //if (user == null)
            //    throw new InvalidOperationException($"User with email '{email}' not found.");
            //return user;
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

        public Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Task.FromResult(Convert.ToBase64String(randomNumber));
        }
        public async Task<UserDto?> RefreshAccessTokenAsync(string refreshToken) // دالة جديدة للـ Refresh
        {
            var user = await GetUserByRefreshTokenAsync(refreshToken);
            if (user == null) return null;

            var existingToken = user.RefreshTokens.FirstOrDefault(t => _tokenService.ValidateRefreshToken(t, refreshToken)); // استخدم Validate مع هاشتج
            if (existingToken == null || existingToken.IsExpired || existingToken.IsRevoked) return null;

            existingToken.IsRevoked = true; // Revoke بشكل صحيح
            var newAccessToken = _tokenService.CreateAccessToken(user);
            var newRefreshToken = _tokenService.CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await UpdateUserAsync(user);

            return new UserDto
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }
        private async Task<AppUser> GetUserByRefreshTokenAsync(string refreshToken) // دالة جديدة للبحث
        {
            return await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => _tokenService.ValidateRefreshToken(t, refreshToken)));
        }
    }
}
