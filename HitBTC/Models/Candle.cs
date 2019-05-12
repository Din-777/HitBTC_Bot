using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class Candle
    {
        [JsonProperty("open")]
        public float Open { get; set; }
        [JsonProperty("close")]
        public float Close { get; set; }
        [JsonProperty("min")]
        public float Min { get; set; }
        [JsonProperty("max")]
        public float Max { get; set; }
        [JsonProperty("volume")]
        public float Volume { get; set; }
        [JsonProperty("volumeQuote")]
        public float VolumeQuote { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
