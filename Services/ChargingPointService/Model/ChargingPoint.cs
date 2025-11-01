using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ChargingPointService.Models.DTOs;

namespace ChargingPointService.Models
{
    public class ChargingPoint
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;


        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("stationId")]
        public string StationId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string PointName { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "online";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "available";

        [JsonPropertyName("create_at")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("update_at")]
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        // ⚡ Không lưu field này trong Mongo để tránh loop hoặc dữ liệu thừa
        [BsonIgnore]
        [JsonPropertyName("station_info")]
        public StationResponseDto? StationInfo { get; set; }
    }
}
