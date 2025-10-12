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




        public async Task<AppUser> GetUserByEmailAsync(string email)
        {

            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _userManager.FindByEmailAsync(email);

        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName) != null;
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


        public  Task<string> GenerateJwtTokenAsync(AppUser user)
        {
            return Task.FromResult(_tokenService.CreateAccessToken(user));
        }

        public Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Task.FromResult(Convert.ToBase64String(randomNumber));
        }
        public async Task<UserDto?> RefreshAccessTokenAsync(string refreshToken) 
        {
            var user = await GetUserByRefreshTokenAsync(refreshToken);
            if (user == null) return null;

            var existingToken = user.RefreshTokens.FirstOrDefault(t => _tokenService.ValidateRefreshToken(t, refreshToken)); // استخدم Validate مع هاشتج
            if (existingToken == null || existingToken.IsExpired || existingToken.IsRevoked) return null;

            existingToken.IsRevoked = true; 
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
        
        private async Task UpdateUserAsync(AppUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) throw new Exception($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        private async Task<AppUser> GetUserByRefreshTokenAsync(string refreshToken) 
        {
            return await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => _tokenService.ValidateRefreshToken(t, refreshToken)));
        }
    }
}
