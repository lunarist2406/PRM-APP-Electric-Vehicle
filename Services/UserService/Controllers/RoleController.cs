using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
// alias để phân biệt rõ class và namespace
using ServiceUser = UserService.Services.UserService;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly ServiceUser _userService;

        public RoleController(ServiceUser userService)
        {
            _userService = userService;
        }

        // PUT: api/role/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateRoleDto dto)
        {
            var success = await _userService.UpdateUserRole(dto.UserId, dto.Role);
            if (!success)
                return NotFound(new { message = "User not found or role update failed." });

            return Ok(new { message = "Role updated successfully." });
        }
    }
}
