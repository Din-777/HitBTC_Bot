using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HitBTC.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocket4Net;

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

	public class SocketMarketData
	{
		WebSocket socket;

		public SocketMarketData(ref WebSocket socket)
		{
			this.socket = socket;
		}


		public async void SubscribeTicker(string symbol)
		{
			var s = new Categories.SubscribeTicker(symbol);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}

		public async void UnSubscribeTicker(string symbol)
		{
			var s = new Categories.UnSubscribeTicker(symbol);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}
	}
}