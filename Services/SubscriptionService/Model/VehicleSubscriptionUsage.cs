using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SubscriptionService.Model
{
    public class VehicleSubscriptionUsage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("vehicle_subscription_id")]
        public string VehicleSubscriptionId { get; set; } = string.Empty;

        [BsonElement("used_sessions")]
        public int UsedSessions { get; set; } = 0;

        [BsonElement("used_kwh")]
        public decimal UsedKwh { get; set; } = 0;

        [BsonElement("total_amount")]
        public decimal TotalAmount { get; set; } = 0;

        [BsonElement("reset_date")]
        public DateTime ResetDate { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

