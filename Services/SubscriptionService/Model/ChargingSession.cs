using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SubscriptionService.Model
{
    public class ChargingSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("booking_id")]
        public int? BookingId { get; set; }

        [BsonElement("spot_id")]
        public int SpotId { get; set; }

        [BsonElement("vehicle_subscription_id")]
        public string VehicleSubscriptionId { get; set; } = string.Empty;

        [BsonElement("station_id")]
        public string StationId { get; set; } = string.Empty;

        [BsonElement("start_time")]
        public DateTime StartTime { get; set; }

        [BsonElement("end_time")]
        public DateTime? EndTime { get; set; }

        [BsonElement("duration_minutes")]
        public int DurationMinutes { get; set; } = 0;

        [BsonElement("kwh_used")]
        public decimal KwhUsed { get; set; }

        [BsonElement("actual_kwh")]
        public decimal? ActualKwh { get; set; }

        [BsonElement("battery_needed_kwh")]
        public decimal BatteryNeededKwh { get; set; }

        [BsonElement("cost")]
        public decimal Cost { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "ongoing";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

