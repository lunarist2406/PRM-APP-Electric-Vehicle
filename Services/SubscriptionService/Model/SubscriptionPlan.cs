using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SubscriptionService.Model
{
    public class SubscriptionPlan
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("billing_cycle")]
        public string BillingCycle { get; set; } = string.Empty;

        [BsonElement("kwh_price")]
        public decimal KwhPrice { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("isCompany")]
        public bool IsCompany { get; set; } = false;

        [BsonElement("discount")]
        public string? Discount { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

