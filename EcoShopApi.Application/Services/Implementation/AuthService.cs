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
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;


        public AuthService(
            UserManager<AppUser> userManager,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _tokenService = tokenService;
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

        public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(AppUser user)

        {
            var accessToken = _tokenService.CreateAccessToken(user);
            var refreshToken = _tokenService.CreateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await UpdateUserAsync(user);
            return (accessToken, refreshToken.Token);
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
