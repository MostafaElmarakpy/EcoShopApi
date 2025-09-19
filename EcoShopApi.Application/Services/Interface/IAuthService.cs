using EcoShopApi.Application.Common.DTO.UserDTO;
using EcoShopApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace EcoShopApi.Application.Services.Interface
{
    public interface IAuthService
    {
        Task<AppUser> GetUserByNameAsync(string userName);
        Task<bool> UserExistsAsync(string userName);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task CreateUserAsync(AppUser user, string password);
        Task UpdateUserAsync(AppUser user);
        // RefreshAccessTokenAsync
        Task<UserDto?> RefreshAccessTokenAsync(string refreshToken);

        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(AppUser user);

    }


}
