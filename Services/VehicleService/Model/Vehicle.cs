using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VehicleService.Models
{
    public class Vehicle
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.String)]
        public string? UserId { get; set; }

        [BsonElement("company_id")]
        [BsonRepresentation(BsonType.String)]
        public string? CompanyId { get; set; }

        [BsonElement("plate_number")]
        public string PlateNumber { get; set; } = string.Empty;

        [BsonElement("model")]
        public string Model { get; set; } = string.Empty;

        [BsonElement("batteryCapacity")]
        public double BatteryCapacity { get; set; } = 0;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
