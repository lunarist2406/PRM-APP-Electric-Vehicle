using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace PaymentService.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("UserId")]
        public string UserId { get; set; } = null!;

        [BsonElement("VehicleId")]
        public string VehicleId { get; set; } = null!;

        [BsonElement("SubscriptionId")]
        public string? SubscriptionId { get; set; } // để tìm payment trong VehicleService

        [BsonElement("OrderId")]
        public string OrderId { get; set; } = null!; // dùng để map với VNPay order

        [BsonElement("Amount")]
        public decimal Amount { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("PaymentUrl")]
        public string? PaymentUrl { get; set; } // link redirect VNPay
    }
}
