using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SubscriptionService.Model
{
    public class VehicleSubscription
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("vehicle_id")]
        public string VehicleId { get; set; } = string.Empty;

        [BsonElement("subscription_id")]
        public string SubscriptionId { get; set; } = string.Empty;

        [BsonElement("auto_renew")]
        public bool AutoRenew { get; set; } = false;

        [BsonElement("payment_status")]
        public string PaymentStatus { get; set; } = "pending";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

