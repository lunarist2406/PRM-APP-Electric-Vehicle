using AIService.Models;
using AIService.Models.DTOs;
using AIService.External;
using AIService.Repositories;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

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

            _apiKey = config["GOOGLE_API_KEY"]
                      ?? throw new ArgumentNullException("GOOGLE_API_KEY missing");
        }

        public async Task<AiResponseDto> AskGeminiAsync(AiRequestDto request)
        {
            var payload = new
            {
                model = "models/gemini-2.5-flash",  // ✅ model mới
                input = new[]
                {
                    new
                    {
                        content = new[]
                        {
                            new { type = "text", text = request.Message }
                        }
                    }
                },
                temperature = 0.7,
                candidateCount = 1
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent")
            {
                Content = JsonContent.Create(payload)
            };

            httpRequest.Headers.Add("X-Goog-Api-Key", _apiKey);

            var aiResponse = await _httpClient.SendAsync(httpRequest);
            var content = await aiResponse.Content.ReadAsStringAsync();

            if (!aiResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error {Status}: {Content}", aiResponse.StatusCode, content);
                throw new HttpRequestException($"Gemini API returned {aiResponse.StatusCode}");
            }

            // Parse JSON response
            string aiText = "No response from AI";
            try
            {
                using var doc = JsonDocument.Parse(content);
                var candidates = doc.RootElement.GetProperty("candidates");
                if (candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    var contents = firstCandidate.GetProperty("content");
                    if (contents.GetArrayLength() > 0)
                    {
                        aiText = contents[0].GetProperty("text").GetString() ?? aiText;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini response");
            }

            var result = new AiResponseDto
            {
                Reply = aiText
            };

            // Lưu chat
            var chat = new ChatHistory
            {
                UserId = request.UserId,
                UserMessage = request.Message,
                AiResponse = result.Reply
            };
            await _chatRepo.AddChatAsync(chat);

            return result;
        }

        public async Task DeleteUserChatHistoryAsync(string userId)
        {
            await _chatRepo.DeleteUserChatsAsync(userId);
        }
    }
}
