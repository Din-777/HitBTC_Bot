using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class IdObject
    {
        [JsonProperty("id")]
        public string Id { set; get; }
    }
}
