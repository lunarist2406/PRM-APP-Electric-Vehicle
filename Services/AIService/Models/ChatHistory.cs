namespace AIService.Models
{
	public class ChatHistory
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string UserId { get; set; } = string.Empty;
		public string UserMessage { get; set; } = string.Empty;
		public string AiResponse { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
