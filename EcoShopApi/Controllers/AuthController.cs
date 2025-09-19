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
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var user = await _authService.GetUserByNameAsync(loginDto.UserName);
                if (user == null || !await _authService.CheckPasswordAsync(user, loginDto.Password))
                    return Unauthorized(new { Message = "Invalid username or password" });

                // Generate both access and refresh tokens
                var tokens = await _authService.GenerateTokensAsync(user);

                return Ok(new UserDto
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Token = tokens.accessToken,
                    RefreshToken = tokens.refreshToken
                });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (await _authService.UserExistsAsync(registerDto.UserName))
                    return BadRequest(new { Message = "Username is already taken" });

                if (await _authService.EmailExistsAsync(registerDto.Email))
                    return BadRequest(new { Message = "Email is already registered" });

                var user = new AppUser
                {
                    UserName = registerDto.UserName,
                    Email = registerDto.Email,
                    DisplayName = registerDto.UserName
                };

                await _authService.CreateUserAsync(user, registerDto.Password);


                // Generate both access and refresh tokens
                var tokens = await _authService.GenerateTokensAsync(user);

                return Ok(new UserDto
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Token = tokens.accessToken,
                    RefreshToken = tokens.refreshToken
                });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }




        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshToken request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var tokens = await _authService.RefreshAccessTokenAsync(request.Token);

                if (tokens == null) return Unauthorized(new { Message = "Invalid or expired refresh token" });

                return Ok(new UserDto
                {
                    DisplayName = tokens.DisplayName,
                    Email = tokens.Email,
                    Token = tokens.Token,
                    RefreshToken = tokens.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

    }
}
