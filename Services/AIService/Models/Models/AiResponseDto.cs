namespace AIService.Models.DTOs
{
    public class AiResponseDto
    {
        public string Reply { get; set; } = string.Empty;
        public string? SuggestedStationId { get; set; }
        public string? StationName { get; set; }
        public string? Address { get; set; }
    }
}
