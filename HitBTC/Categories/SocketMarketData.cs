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
	
	public class SubscribeCandles
	{
		[JsonProperty("method")]
		string Method = "subscribeCandles";

		[JsonProperty("params")]
		public ParamsSubscribeCandles param = new ParamsSubscribeCandles();

		public SubscribeCandles(string symbol, Period period, int limit)
		{
			param.symbol = symbol;
			param.period = period;
			param.limit = limit;
		}

		[JsonProperty("id")]
		string id = "subscribeCandles";
	}

	public class UnSubscribeCandles
	{
		[JsonProperty("method")]
		string Method = "unsubscribeCandles";

		[JsonProperty("params")]
		public Params param = new Params { symbol = "symbol" };

		public UnSubscribeCandles(string symbol)
		{
			param.symbol = symbol;
		}

		[JsonProperty("id")]
		string id = "unsubscribeCandles";
	}

	public class SubscribeTrades
	{
		[JsonProperty("method")]
		string Method = "subscribeTrades";

		[JsonProperty("params")]
		public ParamsSubscribeTrades param = new ParamsSubscribeTrades();

		public SubscribeTrades(string symbol, int limit)
		{
			param.symbol = symbol;
			param.limit = limit;
		}

		[JsonProperty("id")]
		string id = "subscribeTrades";
	}

	public class UnsubscribeTrades
	{
		[JsonProperty("method")]
		string Method = "unsubscribeTrades";

		[JsonProperty("params")]
		public Params param = new Params { symbol = "symbol" };

		public UnsubscribeTrades(string symbol)
		{
			param.symbol = symbol;
		}

		[JsonProperty("id")]
		string id = "unsubscribeTrades";
	}
	
	public class GetSymmols
	{
		[JsonProperty("method")]
		string Method = "getSymbols";

		[JsonProperty("params")]
		public ParamsNull param = new ParamsNull();

		[JsonProperty("id")]
		string id = "getSymbol";
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

		public async void SubscribeCandles(string symbol, Period period, int limit)
		{
			var s = new Categories.SubscribeCandles(symbol, period, limit);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}

		public async void UnSubscribeCandles(string symbol)
		{
			var s = new Categories.UnSubscribeCandles(symbol);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}

		public async void SubscribeTrades(string symbol, int limit)
		{
			var s = new Categories.SubscribeTrades(symbol, limit);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}

		public async void UnsubscribeTrades(string symbol)
		{
			var s = new Categories.UnsubscribeTrades(symbol);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}

		public async void GetSymbols()
		{
			var s = new Categories.GetSymmols();
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}
	}
}