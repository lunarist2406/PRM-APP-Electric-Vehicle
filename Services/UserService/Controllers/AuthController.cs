using Microsoft.AspNetCore.Mvc;
using UserService.Models.DTOs;
using UserService.Services;
using UserService.Utils;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService.Services.UserService _userService;

        private readonly JwtService _jwtService;

        public AuthController(UserService.Services.UserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var existing = await _userService.GetByEmail(dto.Email);
            if (existing != null) return BadRequest("Email already exists");

            var user = await _userService.CreateUser(dto.Name, dto.Email, dto.Password, dto.Role);
            return Ok(new { user.Id, user.Name, user.Email, user.Role });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var valid = await _userService.ValidateUser(dto.Email, dto.Password);
            if (!valid) return Unauthorized("Email or password incorrect");

            var user = await _userService.GetByEmail(dto.Email);
            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }

    }
}
