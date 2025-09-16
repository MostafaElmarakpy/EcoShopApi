using EcoShopApi.Application.Common.DTO.UserDTO;
using EcoShopApi.Application.Common.Interfaces;
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EcoShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _authService.UserExistsAsync(registerDto.UserName);
            if (exists)
            {
                return BadRequest(new { Message = "Username is already taken" });
            }

            if (await _authService.EmailExistsAsync(registerDto.Email))
            {
                return BadRequest(new { Message = "Email is already registered" });
            }
            var user = new AppUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                DisplayName = registerDto.UserName
            };

            await _authService.CreateUserAsync(user, registerDto.Password);
            var accessToken = _tokenService.CreateAccessToken(user);

            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = accessToken
            });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _authService.GetUserByNameAsync(loginDto.UserName);
            if (user == null || !await _authService.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }
            var accessToken = _tokenService.CreateAccessToken(user);
            var refreshToken = _tokenService.CreateRefreshToken();
            //save refresh token to db
            user.RefreshTokens.Add(refreshToken);
            await _authService.CreateUserAsync(user, loginDto.Password);
            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = accessToken
            });

        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshToken request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _authService.GetUserByNameAsync(request.User.UserName);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid user" });
            }
            var existingToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.Token);
            if (existingToken == null)
            {
                return Unauthorized(new { Message = "Invalid or expired refresh token" });
            }
            // Revoke the old refresh token
            existingToken.ExpireAt = DateTime.UtcNow;
            // Generate new tokens
            var newAccessToken = _tokenService.CreateAccessToken(user);
            var newRefreshToken = _tokenService.CreateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _authService.CreateUserAsync(user, request.Token); // Update user with new refresh token
            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = newAccessToken
            });
        }
    
    }
}
