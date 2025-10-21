using UserService.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTOs
{
	public class UpdateRoleDto
	{
		[Required]
		public string UserId { get; set; } = string.Empty;

		[Required]
		public UserRole Role { get; set; }
	}
}
