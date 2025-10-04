using EcoShopApi.Application.Common.DTO.UserDTO;
using EcoShopApi.Application.Common.Interfaces;
using EcoShopApi.Application.Services.Interface;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                //var user = await _userManager.FindByEmailAsync(loginDto.Email);
                var user = await _authService.GetUserByEmailAsync(loginDto.Email);
                if (user == null) return BadRequest(new { Message = "User not found" });

                var result = await _authService.CheckPasswordAsync(user, loginDto.Password);
                if (!result) return BadRequest(new { Message = "Password is incorrect" });

                //generate token
                return Ok(new UserDto
                {
                    DisplayName = user.UserName,
                    Email = user.Email,
                    Token = await _authService.GenerateJwtTokenAsync(user),
                });

            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }


        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _authService.UserExistsAsync(registerDto.UserName))
                return BadRequest(new { Message = "Username is already registered" });

            if (await _authService.EmailExistsAsync(registerDto.Email))
                return BadRequest(new { Message = "Email is already registered" });

            var user = new AppUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                EmailConfirmed = true,
                DisplayName = registerDto.UserName ?? registerDto.UserName
            };

            var result = await _authService.CreateUserAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new UserDto
            {
                DisplayName = user.UserName,
                Email = user.Email,
                Token = await _authService.GenerateJwtTokenAsync(user)
            });
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshToken request)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var tokens = await _authService.RefreshAccessTokenAsync(request.Token);
                //var tokens = await _authService.GenerateRefreshTokenAsync(request.Token);

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
