using Newtonsoft.Json;
using System;

namespace HitBTC.Models
{
	[Serializable]
	public class Balance
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("available")]
        public decimal Available { get; set; }

        [JsonProperty("reserved")]
        public decimal Reserved { get; set; }

    }
}
