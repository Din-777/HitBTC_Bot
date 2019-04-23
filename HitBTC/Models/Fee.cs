using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class Fee
    {
        [JsonProperty("takeLiquidityRate")]
        public string TakeLiquidityRate { get; set; }

        [JsonProperty("provideLiquidityRate")]
        public string ProvideLiquidityRate { get; set; }

    }
}
