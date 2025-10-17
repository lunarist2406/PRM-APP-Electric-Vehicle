using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;
using UserService.Models.DTOs;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users.Select(u => new { u.Id, u.Name, u.Email, u.Role }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetById(id);
            if (user == null) return NotFound();
            return Ok(new { user.Id, user.Name, user.Email, user.Role });
        }

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
                newUser.Password
            );

            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id },
                new
                {
                    createdUser.Id,
                    createdUser.Name,
                    createdUser.Email,
                    createdUser.Role
                });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto updatedUser)
        {
            if (updatedUser == null)
                return BadRequest("User data is required.");

            var success = await _userService.UpdateUser(
                id,
                updatedUser.Name,
                updatedUser.Email,
                updatedUser.Role
            );

            if (!success)
                return NotFound($"User with ID {id} not found.");

            return Ok(new { message = "User updated successfully" });
        }
        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateProfileDto dto)
        {
            if (dto == null)
                return BadRequest("Profile data is required.");

            var success = await _userService.UpdateProfile(
                id,
                dto.Name,
                dto.Email,
                dto.Password
            );

            if (!success)
                return NotFound($"User with ID {id} not found.");

            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _userService.DeleteUser(id);
            if (!success) return NotFound();
            return Ok("User deleted successfully");
        }
    }
}
