namespace VehicleService.Models.DTOs
{
	public class VehicleDto
	{
		public string? UserId { get; set; }
		public string? CompanyId { get; set; }
		public string PlateNumber { get; set; } = string.Empty;
		public string Model { get; set; } = string.Empty;
		public double BatteryCapacity { get; set; } = 0;
	}
}
