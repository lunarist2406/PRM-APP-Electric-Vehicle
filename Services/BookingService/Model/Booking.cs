using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using BookingService.Models.DTOs;
using BookingService.Models.Enums;

namespace BookingService.Models
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("station_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StationId { get; set; } = string.Empty;

        [BsonElement("vehicle_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string VehicleId { get; set; } = string.Empty;

        [BsonElement("charging_point_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChargingPointId { get; set; } = string.Empty;

        [BsonElement("start_time")]
        public DateTime StartTime { get; set; }

        [BsonElement("end_time")]
        public DateTime EndTime { get; set; }

        [BsonElement("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [BsonElement("rate_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ChargingRateType RateType { get; set; } = ChargingRateType.Standard;

        [BsonElement("total_fee")]
        public double TotalFee { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonIgnore]
        public UserResponseDto? UserInfo { get; set; }

        [BsonIgnore]
        public StationResponseDto? StationInfo { get; set; }

        [BsonIgnore]
        public VehicleResponseDto? VehicleInfo { get; set; }

        [BsonIgnore]
        public ChargingPointResponseDto? ChargingPointInfo { get; set; }

        // 🧮 Tính tiền dựa theo loại sạc
        public void CalculateTotalFee()
        {
            if (EndTime <= StartTime)
            {
                TotalFee = 0;
                return;
            }

            var duration = EndTime - StartTime;
            double totalHours = Math.Ceiling(duration.TotalHours < 1 ? 1 : duration.TotalHours);

            TotalFee = totalHours * (int)RateType;
        }
    }
}
    