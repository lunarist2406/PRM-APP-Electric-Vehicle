using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StationService.Models
{
    public class Station
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("address")]
        public string Address { get; set; } = null!;

        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("power_capacity")]
        public int PowerCapacity { get; set; }

        [BsonElement("price_per_kwh")]
        public double PricePerKwh { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "offline";

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
