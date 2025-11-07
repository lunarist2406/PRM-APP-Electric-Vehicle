using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace PaymentService.Models
{
	public class BillingItem
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; } = null!;

		[BsonElement("PaymentId")]
		public string PaymentId { get; set; } = null!; // liên kết Payment

		[BsonElement("VehicleId")]
		public string VehicleId { get; set; } = null!;

		[BsonElement("SubscriptionId")]
		public string SubscriptionId { get; set; } = null!;

		[BsonElement("TotalKwh")]
		public decimal TotalKwh { get; set; }

		[BsonElement("KwhPrice")]
		public decimal KwhPrice { get; set; }

		[BsonElement("KwhAmount")]
		public decimal KwhAmount { get; set; }

		[BsonElement("BaseAmount")]
		public decimal BaseAmount { get; set; }

		[BsonElement("DiscountPercent")]
		public decimal DiscountPercent { get; set; }

		[BsonElement("DiscountAmount")]
		public decimal DiscountAmount { get; set; }

		[BsonElement("Subtotal")]
		public decimal Subtotal { get; set; }

		[BsonElement("TotalAmount")]
		public decimal TotalAmount { get; set; }
	}
}
