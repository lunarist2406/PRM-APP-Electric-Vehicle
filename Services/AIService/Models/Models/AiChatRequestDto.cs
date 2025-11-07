namespace AIService.Models.DTOs
{
    // DTO cho client gửi lên, chỉ chứa message
    public class AiChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
    }
}
