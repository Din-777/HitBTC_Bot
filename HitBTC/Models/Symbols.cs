using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class Symbol
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("baseCurrency")]
        public string BaseCurrency { get; set; }

        [JsonProperty("quoteCurrency")]
        public float QuoteCurrency { get; set; }

        [JsonProperty("quantityIncrement")]
        public float QuantityIncrement { get; set; }

        [JsonProperty("tickSize")]
        public float TickSize { get; set; }

        [JsonProperty("takeLiquidityRate")]
        public float TakeLiquidityRate { get; set; }

        [JsonProperty("provideLiquidityRate")]
        public float ProvideLiquidityRate { get; set; }

        [JsonProperty("feeCurrency")]
        public string FeeCurrency { get; set; }
    }
}
