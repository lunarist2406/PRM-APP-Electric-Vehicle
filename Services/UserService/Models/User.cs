using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UserService.Models.Enums;

namespace UserService.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;


        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("role")]
        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; } = UserRole.Driver;

        // ⚡️ Cho phép ban / unban user
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // 🔁 Lưu refresh token
        [BsonElement("refreshToken")]
        public string? RefreshToken { get; set; }

        [BsonElement("refreshTokenExpiry")]
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
