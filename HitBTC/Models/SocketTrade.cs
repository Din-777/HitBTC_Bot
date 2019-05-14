using System.Collections.Generic;
using Newtonsoft.Json;

namespace HitBTC.Models
{
    public class SocketTrades
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc { get; set; }

        [JsonProperty("result")]
        public SocketTradeData Result { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class SocketTradeData
    {
        [JsonProperty("data")]
        public List<SocketTrade> Data { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
    }

    public class SocketTrade
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("price")]
        public float Price { get; set; }

        [JsonProperty("quantity")]
        public float Quantity { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}