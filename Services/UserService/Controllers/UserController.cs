using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;
using UserService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService.Services.UserService _userService;

        public UserController(UserService.Services.UserService userService)
        {
            _userService = userService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users.Select(u => new { u.Id, u.Name, u.Email, u.Phone , u.Role }));
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetById(id);
            if (user == null) return NotFound();
            return Ok(new { user.Id, user.Name, user.Email,user.Phone,user.Role });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User newUser)
        {
            if (newUser == null)
                return BadRequest("Invalid user data");

            var existing = await _userService.GetByEmail(newUser.Email);
            if (existing != null)
                return BadRequest("Email already exists");

            // ✅ Gọi đúng hàm CreateUser có 3 tham số
            var createdUser = await _userService.CreateUser(
                newUser.Name,
                newUser.Email,
                newUser.Phone,
                newUser.Password
            );

            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id },
                new
                {
                    createdUser.Id,
                    createdUser.Name,
                    createdUser.Email,
                    createdUser.Phone,
                    createdUser.Role
                });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto updatedUser)
        {
            if (updatedUser == null)
                return BadRequest("User data is required.");

            var success = await _userService.UpdateUser(
                id,
                updatedUser.Name,
                updatedUser.Email,
                updatedUser.Phone,
                updatedUser.Role
            );

            if (!success)
                return NotFound($"User with ID {id} not found.");

            return Ok(new { message = "User updated successfully" });
        }
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid token");

            var user = await _userService.GetById(userId);
            if (user == null)
                return NotFound("User not found");

            return Ok(new { user.Id, user.Name, user.Email, user.Phone, user.Role });
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid token");

            var success = await _userService.UpdateProfile(
                userId,
                dto.Name,
                dto.Email,
                dto.Phone,
                dto.Password
            );

            if (!success)
                return NotFound($"User with ID {userId} not found.");

            return Ok(new { message = "Profile updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _userService.DeleteUser(id);
            if (!success) return NotFound();
            return Ok("User deleted successfully");
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/ban")]
        public async Task<IActionResult> BanUser(string id)
        {
            var success = await _userService.BanUser(id);
            if (!success) return NotFound("Account not found");
            return Ok("Banned account");
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/unban")]
        public async Task<IActionResult> UnbanUser(string id)
        {
            var success = await _userService.UnbanUser(id);
            if (!success) return NotFound("Account not found");
            return Ok("Unbanned account");
        }

    }
}
