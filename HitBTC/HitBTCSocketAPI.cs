using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;
using HitBTC.Models;
using Newtonsoft.Json.Linq;
using HitBTC.Categories;

namespace HitBTC
{
	public class HitBTCSocketAPI
	{
		internal WebSocket socket;

		public delegate void SocketHandler(string s);
		public event SocketHandler Opened;
		public event SocketHandler MessageReceived;
		public event SocketHandler Closed;

		public bool Authorized = false;
		public Error Error;
		public Ticker Ticker;
		public Dictionary<string, Ticker> d_Tickers = new Dictionary<string, Ticker>();
		public List<Ticker> L_Tickers = new List<Ticker>();
		public Dictionary<string, Balance> Balance;
		public SocketOrederResult NewOrederResult;
		public Stack<SocketOrederResult> stackPlaceNewOrderResults;
		public Dictionary<string, Symbol> Symbols;
		public Dictionary<string, List<Candle>> Candles;
		public SocketTrade SocketTrade;
		public Dictionary<string, List<SocketTrade>> TradesData;

		public Stack<ParamsActiveOrders> ActiveOrders;

		public async void ConnectAsync(WebSocket socket)
		{
			try
			{
				await Task.Run(() => socket.Open());
			}
			catch
			{
				Console.SetCursorPosition(0, 0);
				Console.Clear();
				Console.WriteLine("Connecting...");
			}
		}
		public void SocketConnect()
		{
			while (socket.State != WebSocketState.Open)
			{
				ConnectAsync(socket);
				Thread.Sleep(5000);
			}
		}
		public void SocketDisconect()
		{
			socket.Close();
		}
		

		public SocketAuth SocketAuth;
		public SocketTrading SocketTrading;
		public SocketMarketData SocketMarketData;

		public HitBTCSocketAPI()
		{
			string uri = "wss://api.hitbtc.com/api/2/ws";
			socket = new WebSocket(uri);
			socket.AutoSendPingInterval = 10;
			socket.EnableAutoSendPing = true;

			Error = null;

			SocketAuth = new SocketAuth(ref socket);
			SocketTrading = new SocketTrading(ref socket);
			SocketMarketData = new SocketMarketData(ref socket);
			stackPlaceNewOrderResults = new Stack<SocketOrederResult>();
			Candles = new Dictionary<string, List<Candle>>();
			TradesData = new Dictionary<string, List<SocketTrade>>();

			while (socket.State != WebSocketState.Connecting)
			{				
				ConnectAsync(socket);
				Thread.Sleep(5000);
			}
			Console.Clear();

			socket.Opened += Socket_Opened;
			socket.Closed += Socket_Closed;
			socket.DataReceived += Socket_DataReceived;
			socket.MessageReceived += Socket_MessageReceived;
		}
				
		internal void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			string str = null;
			this.Error = null;

			var jObject = JObject.Parse(e.Message);

			var Params = jObject["params"];	
			var Error = jObject["error"];
			var id = (string)jObject["id"];
			var method = (string)jObject["method"];
			var result = jObject["result"];

			if (Error == null)
			{
				if (id == "auth")
				{
					this.Authorized = true;
					str = "auth";
				}
				else if (id == "balance")
				{
					List<Balance> ListBalance = JsonConvert.DeserializeObject<List<Balance>>(result.ToString());
					Balance = ListBalance.ToDictionary(b => b.Currency);

					str = "balance";
				}
				else if (id == "subscribeReports")
				{
					str = "subscribeReports";
				}
				else if (id == "subscribeCandles")
				{
					str = "subscribeCandles";
				}
				else if (id == "unsubscribeCandles")
				{
					str = "unsubscribeCandles";
				}
				else if (id == "subscribeTrades")
				{
					str = "subscribeTrades";
				}
				else if (id == "unsubscribeTrades")
				{
					str = "unsubscribeTrades";
				}
				else if (id == "placeNewOrder")
				{
					NewOrederResult = JsonConvert.DeserializeObject<SocketOrederResult>(result.ToString());
					stackPlaceNewOrderResults.Push(NewOrederResult);
					str = "placeNewOrder";
				}
				else if (id == "getSymbol")
				{
					Symbols = (JsonConvert.DeserializeObject<List<Symbol>>(result.ToString())).ToDictionary(t => t.Id);
					str = "getSymbol";
				}
				else if (method == "activeOrders")
				{
					this.ActiveOrders = JsonConvert.DeserializeObject<Stack<ParamsActiveOrders>>(Params.ToString());
				}
				else if (method == "ticker" && Params != null)
				{
					Ticker = JsonConvert.DeserializeObject<Ticker>(Params.ToString());

					L_Tickers.Add(Ticker);
					if (L_Tickers.Count > 20) L_Tickers.RemoveAt(0);

					if (!d_Tickers.ContainsKey(Ticker.Symbol))
						d_Tickers.Add(Ticker.Symbol, new Ticker());

					d_Tickers[Ticker.Symbol] = (Ticker);

					str = "ticker";
				}
				else if (method == "snapshotCandles" && Params != null)
				{
					var Data = jObject["params"]["data"];
					string symbol = (string)jObject["params"]["symbol"];

					List<Candle> lCandle = JsonConvert.DeserializeObject<List<Candle>>(Data.ToString());

					if (!Candles.ContainsKey(symbol))
						Candles.Add(symbol, new List<Candle>());

					Candles[symbol] = lCandle;

					SocketMarketData.UnSubscribeCandles(symbol);

					str = "snapshotCandles";
				}
				else if (method == "snapshotTrades" && Params != null)
				{
					var Data = jObject["params"]["data"];
					string symbol = (string)jObject["params"]["symbol"];

					List<SocketTrade> ListSocketTradeData = JsonConvert.DeserializeObject<List<SocketTrade>>(Data.ToString());
					if (!TradesData.ContainsKey(symbol))
						TradesData.Add(symbol, new List<SocketTrade>());

					TradesData[symbol] = ListSocketTradeData;
					str = "snapshotCandles";
				}
				else if (method == "updateTrades" && Params != null)
				{
					var Data = jObject["params"]["data"];
					string symbol = (string)jObject["params"]["symbol"];
					SocketTrade = JsonConvert.DeserializeObject<SocketTrade>(Data[0].ToString());
					TradesData[symbol].Add(SocketTrade);

					str = "updateTrades";
				}
			}
			else
			{
				this.Error = JsonConvert.DeserializeObject<Error>(Error.ToString());
				this.Error.Id = id;				
			}

			if (MessageReceived != null) MessageReceived(str);
		}

		internal void Socket_DataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine("Socket_DataReceived");
		}

		internal void Socket_Opened(object sender, EventArgs e)
		{
			if (Opened != null) Opened(e.ToString());
		}

		private void Socket_Closed(object sender, EventArgs e)
		{
			if (Closed != null) Closed(e.ToString());
		}
	}
}
