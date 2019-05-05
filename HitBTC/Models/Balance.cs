using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class Balance
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("available")]
        public float Available { get; set; }

        [JsonProperty("reserved")]
        public float Reserved { get; set; }

    }
}
