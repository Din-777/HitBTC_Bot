using System;
using System.Text;
using Newtonsoft.Json;

namespace HitBTC.Models
{
	public class Ticker
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        /// <summary>
        /// Last price
        /// </summary>
        [JsonProperty("last")]
        public decimal Last { get; set; }

        /// <summary>
        /// Highest buy order
        /// </summary>
        [JsonProperty("bid")]
        public decimal Bid { get; set; }

        /// <summary>
        /// Lowest sell order
        /// </summary>
        [JsonProperty("ask")]
        public decimal Ask { get; set; }

        /// <summary>
        /// Highest trade price per last 24h + last incomplete minute
        /// </summary>
        [JsonProperty("high")]
        public decimal High { get; set; }

        /// <summary>
        /// Lowest trade price per last 24h + last incomplete minute
        /// </summary>
        [JsonProperty("low")]
        public decimal Low { get; set; }

        /// <summary>
        /// Volume per last 24h + last incomplete minute
        /// </summary>
        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        /// <summary>
        /// Price in which instrument open
        /// </summary>
        [JsonProperty("open")]
        public decimal Open { get; set; }

        /// <summary>
        /// Volume in second currency per last 24h + last incomplete minute
        /// </summary>
        [JsonProperty("volumeQuoute")]
        public decimal VolumeQuoute { get; set; }

        /// <summary>
        /// Server time in UNIX timestamp format
        /// </summary>
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

    }
}