using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookingService.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ChargingRateType
    {
        Standard = 30000,
        Fast = 50000,
        SuperFast = 80000
    }
}
