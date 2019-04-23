using System.Collections.Generic;
using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class SocketBalance
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; }

        [JsonProperty("result")]
        public List<Balance> Result { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
