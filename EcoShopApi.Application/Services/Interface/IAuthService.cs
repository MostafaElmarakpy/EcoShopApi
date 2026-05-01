using EcoShopApi.Application.DTO.AuthDTO;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace EcoShopApi.Application.Services.Interface
{
    public interface IAuthService
    {

        Task<AppUser> GetUserByNameAsync(string userName);
        //get user by email
        Task<AppUser> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(string userName);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task CreateUserAsync(AppUser user, string password);
        Task UpdateUserAsync(AppUser user);
        Task<string> GenerateJwtTokenAsync(AppUser user);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> LogoutAsync(string userId, string RefreshToken);
        Task<RefreshTokenDto?> RefreshAccessTokenAsync(string refreshToken);

    }


}
