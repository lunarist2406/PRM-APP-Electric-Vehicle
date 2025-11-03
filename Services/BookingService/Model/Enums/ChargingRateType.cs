using System.Text.Json.Serialization;

namespace BookingService.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChargingRateType
    {
        Standard,
        Fast,
        SuperFast
    }

    public static class ChargingRateTypeExtensions
    {
        /// <summary>
        /// Lấy giá mỗi giờ theo type
        /// </summary>
        public static int GetRatePerHour(this ChargingRateType type)
        {
            return type switch
            {
                ChargingRateType.Standard => 30000,
                ChargingRateType.Fast => 50000,
                ChargingRateType.SuperFast => 80000,
                _ => 0
            };
        }

        /// <summary>
        /// Tính tổng tiền dựa trên số giờ
        /// </summary>
        public static int CalculateTotal(this ChargingRateType type, int hours)
        {
            if (hours <= 0) return 0;
            return type.GetRatePerHour() * hours;
        }
    }
}
