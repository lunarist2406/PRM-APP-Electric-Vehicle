using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AIService.Models.DTOs;
using AIService.Services;

namespace AIService.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly AiServiceLayer _aiService;

        public AiController(AiServiceLayer aiService)
        {
            _aiService = aiService;
        }

        // API chat: chỉ nhận message, tự thêm userId từ JWT
        [HttpPost("chat")]
        [Authorize]
        public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request)
        {
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdFromToken))
                return Unauthorized("❌ Token không hợp lệ hoặc hết hạn");

            var aiRequest = new AiRequestDto
            {
                UserId = userIdFromToken,
                Message = request.Message
            };

            var response = await _aiService.AskGeminiAsync(aiRequest);
            return Ok(response);
        }


        // API xóa lịch sử chat của user hiện tại
        [HttpDelete("chat-history")]
        [Authorize]
        public async Task<IActionResult> DeleteMyChatHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("❌ Token không hợp lệ hoặc hết hạn");

            await _aiService.DeleteUserChatHistoryAsync(userId);
            return NoContent();
        }
    }
}
