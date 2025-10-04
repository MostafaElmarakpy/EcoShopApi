using EcoShopApi.Application.Common.DTO.UserDTO;
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
        Task<IdentityResult> CreateUserAsync(AppUser user, string password);
        Task UpdateUserAsync(AppUser user);
        Task<string> GenerateJwtTokenAsync(AppUser user);
        Task<string> GenerateRefreshTokenAsync();

        //RefreshAccessTokenAsync
        Task<UserDto?> RefreshAccessTokenAsync(string refreshToken);

    }


}
