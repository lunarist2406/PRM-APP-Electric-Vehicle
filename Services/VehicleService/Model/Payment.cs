using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VehicleService.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("vehicle_id")]
        public string VehicleId { get; set; } = string.Empty;

        [BsonElement("subscription_id")]
        public string SubscriptionId { get; set; } = string.Empty;

        [BsonElement("kwh")]
        public decimal Kwh { get; set; } = 0;

        [BsonElement("base_amount")]
        public decimal BaseAmount { get; set; } = 0;

        [BsonElement("kwh_amount")]
        public decimal KwhAmount { get; set; } = 0;

        [BsonElement("total_amount")]
        public decimal TotalAmount { get; set; } = 0;

        [BsonElement("discount_amount")]
        public decimal DiscountAmount { get; set; } = 0;

        [BsonElement("subtotal")]
        public decimal Subtotal { get; set; } = 0; // Before discount

        [BsonElement("billing_period_start")]
        public DateTime BillingPeriodStart { get; set; }

        [BsonElement("billing_period_end")]
        public DateTime BillingPeriodEnd { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "pending"; // pending, paid, overdue

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

