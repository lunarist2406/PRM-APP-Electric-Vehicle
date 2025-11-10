using AIService.Models;
using AIService.Models.DTOs;
using AIService.External;
using AIService.Repositories;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIService.Services
{
    public class AiServiceLayer
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiServiceLayer> _logger;
        private readonly string _apiKey;
        private readonly StationClient _stationClient;
        private readonly ChatHistoryRepository _chatRepo;

        public AiServiceLayer(
            HttpClient httpClient,
            ILogger<AiServiceLayer> logger,
            IConfiguration config,
            StationClient stationClient,
            ChatHistoryRepository chatRepo)
        {
            _httpClient = httpClient;
            _logger = logger;
            _stationClient = stationClient;
            _chatRepo = chatRepo;
            _apiKey = config["GOOGLE_API_KEY"] ?? throw new ArgumentNullException("GOOGLE_API_KEY missing");
        }

        public async Task<AiResponseDto> AskGeminiAsync(AiRequestDto request, string userToken)
        {
            var responseDto = new AiResponseDto();

            try
            {
                string aiInput = request.Message;
                List<StationResponseDto> stations = new();

                // Nếu câu hỏi liên quan trạm sạc
                if (IsStationQuery(request.Message))
                {
                    try
                    {
                        stations = await _stationClient.GetStationsAsync(userToken) ?? new List<StationResponseDto>();

                        if (stations.Any())
                        {
                            string stationText = string.Join("\n", stations.Select(s =>
                                $"- {s.Name} | {s.Address} | ID: {s.Id}"
                            ));

                            aiInput = $"Dưới đây là danh sách các trạm sạc:\n{stationText}\n\nNgười dùng hỏi: {request.Message}\nHãy trả lời tự nhiên nhất:";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Không thể lấy danh sách trạm sạc.");
                    }
                }

                // Gọi Gemini API (có retry)
                responseDto.Reply = await GetGeminiResponseAsync(aiInput);

                // Nếu có danh sách trạm sạc, trả thêm gợi ý
                if (stations.Any())
                {
                    var first = stations.First();
                    responseDto.SuggestedStationId = first.Id;
                    responseDto.StationName = first.Name;
                    responseDto.Address = first.Address;
                    responseDto.Stations = stations;
                }

                // Lưu lịch sử chat
                await _chatRepo.AddChatAsync(new ChatHistory
                {
                    UserId = request.UserId,
                    UserMessage = request.Message,
                    AiResponse = responseDto.Reply,
                    CreatedAt = DateTime.UtcNow
                });

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔥 Error in AskGeminiAsync - Message: {Message}", request.Message);
                responseDto.Reply = "⚠️ Xin lỗi, hiện tại hệ thống AI đang bận hoặc quá tải. Vui lòng thử lại sau nhé!";
                return responseDto;
            }
        }

        #region Helpers

        private bool IsStationQuery(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;

            message = message.ToLower();
            return message.Contains("trạm sạc")
                || message.Contains("station")
                || message.Contains("charging")
                || message.Contains("trạm");
        }

        private async Task<string> GetGeminiResponseAsync(string userMessage)
        {
            var payload = new
            {
                model = "models/gemini-2.5-flash",
                contents = new[]
                {
                    new { parts = new[] { new { text = userMessage } } }
                }
            };

            const int maxRetries = 3;
            const int baseDelayMs = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent"
                    )
                    {
                        Content = JsonContent.Create(payload)
                    };
                    request.Headers.Add("X-Goog-Api-Key", _apiKey);

                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return ParseGeminiResponse(content);
                    }

                    // Retry khi lỗi tạm thời
                    if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        int delay = baseDelayMs * attempt;
                        _logger.LogWarning("⚠️ Gemini API quá tải (HTTP {StatusCode}). Thử lại lần {Attempt} sau {Delay}ms.",
                            response.StatusCode, attempt, delay);
                        await Task.Delay(delay);
                        continue;
                    }

                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Lỗi mạng khi gọi Gemini API (attempt {Attempt}/{Max})", attempt, maxRetries);
                    if (attempt == maxRetries)
                        throw;

                    await Task.Delay(baseDelayMs * attempt);
                }
            }

            return "Hiện tại AI đang bận, hãy thử lại sau ít phút nhé.";
        }

        private string ParseGeminiResponse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var content = candidates[0].GetProperty("content");
                if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                {
                    return parts[0].GetProperty("text").GetString() ?? "No response from AI";
                }
            }

            return "No response from AI";
        }

        #endregion

        #region Chat History

        public async Task DeleteUserChatHistoryAsync(string userId)
            => await _chatRepo.DeleteUserChatsAsync(userId);

        public async Task DeleteSingleChatAsync(string userId, string chatId)
        {
            var userChats = await _chatRepo.GetUserChatsAsync(userId);
            if (!userChats.Any(c => c.Id == chatId))
                throw new Exception("Chat không tồn tại hoặc không thuộc user");

            await _chatRepo.DeleteChatByIdAsync(chatId);
        }

        public async Task<List<ChatHistory>> GetAllUserChatsAsync(string userId)
            => await _chatRepo.GetUserChatsAsync(userId);

        #endregion
    }
}
