using EcoShopApi.Application.Interfaces;
using EcoShopApi.Application.DTO.UserDTO;
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
namespace EcoShopApi.Application.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IReadOnlyList<UserAppDto>> GetAllUsersAsync()
        {
            // load all users
            var users = await _userManager.Users.ToListAsync();

            var result = new List<UserAppDto>(users.Count);
            foreach (var u in users)
            {
                // get roles for each user (async)
                var roles = await _userManager.GetRolesAsync(u);

                result.Add(new UserAppDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? u.DisplayName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    IsActive = true, // you can customize using a flag on AppUser if you have it
                    Roles = roles?.ToList() ?? new List<string>()
                });
            }

            return result;
        }
        public async Task<UserAppDto?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserAppDto
            {
                Id = user.Id,
                UserName = user.UserName ?? user.DisplayName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                IsActive = true,
                Roles = roles?.ToList() ?? new List<string>()
            };
        }
        public async Task<bool> UserExistsAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName) != null;
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }


        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }

        public Task<IdentityResult> CreateUserAsync(CreateUserDto dto)
        {
            // Implementation for creating a user based on CreateUserDto and returning IdentityResult

            var user = new AppUser
            {
                Email = dto.Email,
                DisplayName = dto.FullName,

            };
            return _userManager.CreateAsync(user, dto.Password);
        }

        public Task<IdentityResult> UpdateUserAsync(string id, UpdateUserDto dtol)
        {
            // Implementation for updating a user based on UpdateUserDto and returning IdentityResult
            var user = _userManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.DisplayName = dtol.DisplayName;
            user.Email = dtol.Email;

            return _userManager.UpdateAsync(user);

        }

        public Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangeUsernameAsync(string userId, UpdateUsernameDto dto)
        {
            throw new NotImplementedException();
        }
    }

}
