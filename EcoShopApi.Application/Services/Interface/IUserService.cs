using EcoShopApi.Application.DTO.UserDTO;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EcoShopApi.Application.Services.Interface;

public interface IUserService
{
    // Define user management related methods here
    Task<IReadOnlyList<UserAppDto>> GetAllUsersAsync();
    Task<UserAppDto?> GetUserByIdAsync(string userId);
    Task<bool> UserExistsAsync(string userName);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<bool> ChangeUsernameAsync(string userId, UpdateUsernameDto dto);
    Task<IdentityResult> CreateUserAsync(CreateUserDto dto);
    Task<IdentityResult> UpdateUserAsync(string id, UpdateUserDto dto);
    Task DeleteUserAsync(string userId);
}
