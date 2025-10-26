using System.Text.Json.Serialization;

namespace VehicleService.Models.DTOs
{
	public class VehicleDto
	{
		[JsonPropertyName("user_id")]
		public string? UserId { get; set; }

		[JsonPropertyName("company_id")]
		public string? CompanyId { get; set; }

		[JsonPropertyName("plate_number")]
		public string PlateNumber { get; set; } = string.Empty;

		[JsonPropertyName("model")]
		public string Model { get; set; } = string.Empty;

		[JsonPropertyName("batteryCapacity")]
		public double BatteryCapacity { get; set; } = 0;
	}
}
