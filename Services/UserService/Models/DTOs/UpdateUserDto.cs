namespace UserService.Models.DTOs
{
	public class UpdateUserDto
	{
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
	}
}
