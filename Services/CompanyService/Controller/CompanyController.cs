using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyService.Models;
using CompanyService.Services;
using CompanyService.Models.DTOs;
using System.Security.Claims;

namespace CompanyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyDataService _companyService;

        public CompaniesController(CompanyDataService companyService)
        {
            _companyService = companyService;
        }

        // 🔐 Lấy tất cả company (chỉ user có token mới được xem)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            Console.WriteLine($"✅ Request by user {userId}");

            var companies = await _companyService.GetAllCompanies();
            return Ok(companies);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var company = await _companyService.GetCompanyById(id);
            if (company == null)
                return NotFound(new { message = "Company not found" });

            return Ok(company);
        }

        // ➕ Tạo mới company
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RegisterCompanyDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { message = "User not found" });

            var company = new Company
            {
                UserId = userId,
                Name = dto.Name,
                Address = dto.Address,
                ContactEmail = dto.ContactEmail,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _companyService.CreateCompany(company);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }

        // 🔁 Cập nhật company
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] RegisterCompanyDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { message = "User not found" });

            var existing = await _companyService.GetCompanyById(id);
            if (existing == null)
                return NotFound(new { message = "Company not found" });

            // Chỉ cho phép chủ sở hữu sửa
            if (existing.UserId != userId)
                return Forbid("You are not allowed to update this company");

            existing.Name = dto.Name;
            existing.Address = dto.Address;
            existing.ContactEmail = dto.ContactEmail;
            existing.UpdatedAt = DateTime.UtcNow;

            var success = await _companyService.UpdateCompany(id, existing);

            if (!success)
                return BadRequest(new { message = "Update failed" });

            return Ok(existing);
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var existing = await _companyService.GetCompanyById(id);
            if (existing == null)
                return NotFound(new { message = "Company not found" });

            var success = await _companyService.DeleteCompany(id);
            if (!success)
                return BadRequest(new { message = "Delete failed" });

            return Ok(new { message = "Company deleted successfully" });
        }
    }
}
