using EcoShopApi.Application.Common.DTO.UserDTO;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Application.Services.Interface
{
    public interface IUserService
    {
        // Define user management related methods here
        Task<IReadOnlyList<UserAppDto>> GetAllUsersAsync();
        Task<UserAppDto> GetUserByIdAsync(string userId);
        Task<bool> UserExistsAsync(string userName);
        Task<bool> EmailExistsAsync(string email);
        Task<IdentityResult> CreateUserAsync(CreateUserDto dto);
        Task<IdentityResult> UpdateUserAsync(string id, UpdateUserDto dto);
        Task DeleteUserAsync(string userId);
    }
}
