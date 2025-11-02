using System;

namespace StationService.DTOs
{
	public class StationResponseDto
	{
		public string Id { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Address { get; set; } = null!;
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public int PowerCapacity { get; set; }
		public double PricePerKwh { get; set; }
		public string Status { get; set; } = "offline";
		public DateTime CreatedAt { get; set; }
	}
}
