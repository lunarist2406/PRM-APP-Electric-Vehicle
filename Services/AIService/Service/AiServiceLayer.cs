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

                if (IsStationQuery(request.Message))
                {
                    var stations = await _stationClient.GetStationsAsync(userToken);

                    if (stations != null && stations.Any())
                    {
                        // Chuyển dữ liệu trạm sang dạng text để AI đọc
                        var stationText = string.Join("\n", stations.Select(s =>
                            $"- {s.Name} | {s.Address} | ID: {s.Id}"
                        ));

                        aiInput = $"Dưới đây là danh sách các trạm sạc:\n{stationText}\n\nNgười dùng hỏi: {request.Message}\nHãy trả lời tự nhiên nhất:";
                    }
                }

                // Gọi AI
                responseDto.Reply = await GetGeminiResponseAsync(aiInput);

                // Nếu có trạm, set gợi ý
                var firstStation = await _stationClient.GetStationsAsync(userToken);
                if (firstStation != null && firstStation.Any())
                {
                    responseDto.SuggestedStationId = firstStation.First().Id;
                    responseDto.StationName = firstStation.First().Name;
                    responseDto.Address = firstStation.First().Address;
                    responseDto.Stations = firstStation.Select(s => new StationResponseDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Address = s.Address,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude
                    }).ToList();
                }

                // Lưu chat
                await _chatRepo.AddChatAsync(new ChatHistory
                {
                    UserId = request.UserId,
                    UserMessage = request.Message,
                    AiResponse = responseDto.Reply
                });

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AskGeminiAsync");
                responseDto.Reply = "❌ Có lỗi xảy ra khi xử lý yêu cầu";
                return responseDto;
            }
        }


        #region Helpers

        private bool IsStationQuery(string message)
        {
            message = message.ToLower();
            return message.Contains("trạm sạc")
                || message.Contains("station")
                || message.Contains("charging")
                || message.Contains("trạm");
        }

        private async Task AttachStationInfoAsync(AiResponseDto responseDto, string userToken, string userMessage)
        {
            var stations = await _stationClient.GetStationsAsync(userToken);

            if (stations != null && stations.Any())
            {
                // Lọc theo từ khóa người dùng
                var keyword = userMessage.ToLower();
                var matchedStations = stations
                    .Where(s => s.Name.ToLower().Contains(keyword) || s.Address.ToLower().Contains(keyword))
                    .ToList();

                if (!matchedStations.Any())
                {
                    responseDto.Reply = "Không tìm thấy trạm sạc nào phù hợp với yêu cầu của bạn.";
                    return;
                }

                // Gán danh sách trạm trả về JSON
                responseDto.Stations = matchedStations.Select(s => new StationResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Address = s.Address,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude
                }).ToList();

                // Trạm đầu tiên làm gợi ý
                var firstStation = matchedStations.First();
                responseDto.SuggestedStationId = firstStation.Id;
                responseDto.StationName = firstStation.Name;
                responseDto.Address = firstStation.Address;

                responseDto.Reply = $"Mình tìm thấy {matchedStations.Count} trạm sạc phù hợp với yêu cầu của bạn:";
            }
            else
            {
                responseDto.Reply = "Không tìm thấy trạm sạc nào trong hệ thống.";
            }
        }

        private async Task<string> GetGeminiResponseAsync(string userMessage)
        {
            var payload = new
            {
                model = "models/gemini-2.5-flash",
                contents = new[] { new { parts = new[] { new { text = userMessage } } } }
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("X-Goog-Api-Key", _apiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var candidates = doc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() > 0)
            {
                var parts = candidates[0].GetProperty("content").GetProperty("parts");
                if (parts.GetArrayLength() > 0)
                {
                    return parts[0].GetProperty("text").GetString() ?? "No response from AI";
                }
            }

            return "No response from AI";
        }

        #endregion

        #region Chat History Methods

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
        {
            return await _chatRepo.GetUserChatsAsync(userId);
        }

        #endregion
    }
}
