using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CompanyService.Models
{
	public class Company
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; }

		[BsonElement("user_id")]
		[BsonRepresentation(BsonType.ObjectId)] // nếu user_id là ObjectId từ MongoDB Users collection
		public string UserId { get; set; } = null!; // 🔥 Ai tạo công ty này

		[BsonElement("name")]
		public string Name { get; set; } = null!;

		[BsonElement("address")]
		public string Address { get; set; } = null!;

		[BsonElement("contact_email")]
		public string ContactEmail { get; set; } = null!;

		[BsonElement("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[BsonElement("updatedAt")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}
