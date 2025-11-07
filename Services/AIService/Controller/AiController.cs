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

        // Chat với AI
        [HttpPost("chat")]
        [Authorize]
        public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "❌ Token không hợp lệ hoặc hết hạn" });

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "❌ Message không được để trống" });

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var response = await _aiService.AskGeminiAsync(
                new AiRequestDto { UserId = userId, Message = request.Message },
                token
            );

            return Ok(response);
        }

        // Lấy toàn bộ lịch sử chat của user
        [HttpGet("chat-history")]
        [Authorize]
        public async Task<IActionResult> GetMyChatHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn" });

            try
            {
                var chats = await _aiService.GetAllUserChatsAsync(userId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy lịch sử chat", details = ex.Message });
            }
        }

        // Xóa tất cả chat của user
        [HttpDelete("chat-history")]
        [Authorize]
        public async Task<IActionResult> DeleteMyChatHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn" });

            try
            {
                await _aiService.DeleteUserChatHistoryAsync(userId);
                return Ok(new { message = "Đã xóa tất cả lịch sử chat" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa lịch sử chat", details = ex.Message });
            }
        }

        // Xóa 1 chat dựa trên chatId
        [HttpDelete("chat/{chatId}")]
        [Authorize]
        public async Task<IActionResult> DeleteSingleChat(string chatId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token không hợp lệ hoặc hết hạn" });

            try
            {
                await _aiService.DeleteSingleChatAsync(userId, chatId);
                return Ok(new { message = $"Đã xóa chat {chatId}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa chat", details = ex.Message });
            }
        }
    }
}
