using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HitBTC.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HitBTC.Categories
{
	public class NotificationTicker
	{
		[JsonProperty("jsonrpc")]
		public string JsonRpc { get; private set; }

		[JsonProperty("method")]
		public string Method { get; private set; }		

		[JsonProperty("params")]
		public Ticker Ticker { get; private set; }

		[JsonProperty("id")]
		public string Id { get; private set; }
	}
	
	public class SubscribeTicker
	{
		[JsonProperty("method")]
		string Method = "subscribeTicker";

		[JsonProperty("params")]
		public ParamsTicker param = new ParamsTicker { Symbol = "symbol" };

		public SubscribeTicker(string symbol)
		{
			param.Symbol = symbol;
		}

		[JsonProperty("id")]
		string id = "ticker";
	}

	public class UnSubscribeTicker
	{
		[JsonProperty("method")]
		string Method = "unsubscribeTicker";

		[JsonProperty("params")]
		public ParamsTicker param = new ParamsTicker { Symbol = "symbol" };

		public UnSubscribeTicker(string symbol)
		{
			param.Symbol = symbol;
		}

		[JsonProperty("id")]
		string id = "unsubscribeTicker";
	}
}