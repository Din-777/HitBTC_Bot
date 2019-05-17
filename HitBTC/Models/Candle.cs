using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class Candle
    {
        [JsonProperty("open")]
        public decimal Open { get; set; }
        [JsonProperty("close")]
        public decimal Close { get; set; }
        [JsonProperty("min")]
        public decimal Min { get; set; }
        [JsonProperty("max")]
        public decimal Max { get; set; }
        [JsonProperty("volume")]
        public decimal Volume { get; set; }
        [JsonProperty("volumeQuote")]
        public decimal VolumeQuote { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
