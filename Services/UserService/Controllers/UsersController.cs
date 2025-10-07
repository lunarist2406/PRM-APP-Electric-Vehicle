using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Role = request.Role
            };

            var created = await _service.RegisterAsync(user);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _service.LoginAsync(request.Email, request.Password);
            if (user == null)
                return Unauthorized("Invalid credentials");
            return Ok(user);
        }
    }
}
