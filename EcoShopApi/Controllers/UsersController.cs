using EcoShopApi.Application.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EcoShopApi.Domain.Entities;
using System.Threading.Tasks;
using EcoShopApi.Application.Services.Implementation;
using EcoShopApi.Application.DTO.UserDTO;

namespace EcoShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var result = await _userService.CreateUserAsync(dto);
                if (result.Succeeded)
                {
                    return CreatedAtAction(nameof(GetUserById), new { id = dto.Email }, dto);
                }
                return BadRequest(result.Errors);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            if (id != dto.Email)
            {
                return BadRequest("User ID mismatch");
            }
            try
            {
                var result = await _userService.UpdateUserAsync(id, dto);
                if (result.Succeeded)
                {
                    return NoContent();
                }
                return BadRequest(result.Errors);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromBody] string id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
    }
}
