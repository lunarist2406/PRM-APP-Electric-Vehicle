using System.ComponentModel.DataAnnotations;

namespace CompanyService.Models.DTOs
{
    public class RegisterCompanyDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; } = null!;
    }
}
