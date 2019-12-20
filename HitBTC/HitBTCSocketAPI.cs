using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocket4Net;
using HitBTC.Models;
using Newtonsoft.Json.Linq;
using HitBTC.Categories;
using System.Collections.Concurrent;

namespace HitBTC
{
	public class HitBTCSocketAPI
	{
		public WebSocket socket;

		public delegate void SocketHandler(string Notification, string Symbol);
		public event SocketHandler Opened;
		public event SocketHandler Closed;
		public delegate void MessegeHandler(string Notification, string Symbol);
		public event MessegeHandler MessageReceived;		

		public bool Authorized = false;
		public Error Error;
		public Ticker Ticker;
		public ConcurrentDictionary<string, Ticker> d_Tickers = new ConcurrentDictionary<string, Ticker>();
		public List<Ticker> L_Tickers = new List<Ticker>();
		public Dictionary<string, Balance> Balance;
		public SocketOrederResult NewOrederResult;
		public Stack<SocketOrederResult> stackPlaceNewOrderResults;
		public Dictionary<string, Symbol> Symbols;
		public Dictionary<string, List<Candle>> Candles;
		public SocketTrade Trade;
		public Dictionary<string, List<SocketTrade>> Trades;
		public Dictionary<string, SocketTrade> d_Trades = new Dictionary<string, SocketTrade>();
		public Dictionary<string, Candle> d_Candle = new Dictionary<string, Candle>();
		public List<ParamsActiveOrders> ActiveOrders;

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
		public void SocketConnect(WebSocket socket)
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

		public TimeSpan TimeZone = TimeSpan.FromHours(5);

		public void ReceiveMessages(bool received)
		{
			if(received)
				socket.DataReceived += Socket_DataReceived;
			else if(!received)
				socket.DataReceived -= Socket_DataReceived;
		}

		public HitBTCSocketAPI()
		{
			string uri = "wss://api.hitbtc.com/api/2/ws";
			socket = new WebSocket(uri);

			Error = null;

			SocketAuth = new SocketAuth(ref socket);
			SocketTrading = new SocketTrading(ref socket);
			SocketMarketData = new SocketMarketData(ref socket);
			stackPlaceNewOrderResults = new Stack<SocketOrederResult>();
			Candles = new Dictionary<string, List<Candle>>();
			Trades = new Dictionary<string, List<SocketTrade>>();

			while (socket.State != WebSocketState.Connecting)
			{
				ConnectAsync(socket);
				Thread.Sleep(5000);
			}
			//Console.Clear();

			socket.Opened += Socket_Opened;
			socket.Closed += Socket_Closed;
			socket.DataReceived += Socket_DataReceived;
			socket.MessageReceived += Socket_MessageReceived;
		}

		public string MessageType = null;
		internal void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			MessageType = null;
			string symbol = null;
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
					MessageType = "auth";
					symbol = e.Message;
				}
				else if (id == "balance")
				{
					List<Balance> ListBalance = JsonConvert.DeserializeObject<List<Balance>>(result.ToString());
					Balance = ListBalance.ToDictionary(b => b.Currency);

					MessageType = "balance";
				}
				else if (id == "subscribeReports")
				{
					MessageType = "subscribeReports";
				}
				else if (id == "subscribeCandles")
				{
					MessageType = "subscribeCandles";
				}
				else if (id == "unsubscribeCandles")
				{
					MessageType = "unsubscribeCandles";
				}
				else if (id == "subscribeTrades")
				{
					MessageType = "subscribeTrades";
				}
				else if (id == "unsubscribeTrades")
				{
					MessageType = "unsubscribeTrades";
				}
				else if (id == "placeNewOrder")
				{
					NewOrederResult = JsonConvert.DeserializeObject<SocketOrederResult>(result.ToString());
					stackPlaceNewOrderResults.Push(NewOrederResult);
					MessageType = "placeNewOrder";
				}
				else if (id == "getSymbol")
				{
					Symbols = (JsonConvert.DeserializeObject<List<Symbol>>(result.ToString())).ToDictionary(t => t.Id);
					MessageType = "getSymbol";
				}
				else if (method == "activeOrders" && Params != null)
				{
					this.ActiveOrders = JsonConvert.DeserializeObject<List<ParamsActiveOrders>>(Params.ToString());
					MessageType = "activeOrders";
				}
				else if (method == "report" && Params != null)
				{
					MessageType = "report";
				}
				else if (method == "ticker" && Params != null)
				{
					try { Ticker = JsonConvert.DeserializeObject<Ticker>(Params.ToString()); }
					catch { Ticker = null; }
					finally
					{
						if (Ticker == null)
						{
							MessageType = "null";							
						}
						else
						{
							L_Tickers.Add(Ticker);
							if (L_Tickers.Count > 20) L_Tickers.RemoveAt(0);

							if (!d_Tickers.ContainsKey(Ticker.Symbol))
								d_Tickers.TryAdd(Ticker.Symbol, new Ticker());

							d_Tickers[Ticker.Symbol] = Ticker;

							symbol = Ticker.Symbol;
							MessageType = "subscribeTicker";
						}
					}
				}
				else if (method == "snapshotCandles" && Params != null)
				{ 
					var Data = jObject["params"]["data"];
					symbol = (string)jObject["params"]["symbol"];

					List<Candle> lCandle = JsonConvert.DeserializeObject<List<Candle>>(Data.ToString());
					lCandle.ForEach(c => c.TimeStamp = c.TimeStamp + TimeZone);

					if (!Candles.ContainsKey(symbol))
						Candles.Add(symbol, new List<Candle>());

					if (!d_Candle.ContainsKey(symbol))
						d_Candle.Add(symbol, new Candle());

					d_Candle[symbol] = lCandle.Last();
					Candles[symbol] = lCandle; 
					MessageType = "snapshotCandles";
				}
				else if (method == "updateCandles" && Params != null)
				{
					var Data = jObject["params"]["data"];
					symbol = (string)jObject["params"]["symbol"];

					Candle candle = JsonConvert.DeserializeObject<Candle>(Data[0].ToString());

					candle.TimeStamp = candle.TimeStamp + TimeZone;

					if (!Candles.ContainsKey(symbol))
						Candles.Add(symbol, new List<Candle>());

					if (Candles[symbol].Last().TimeStamp == candle.TimeStamp)
					{
						Candles[symbol][Candles[symbol].Count - 1] = candle;
					}
					else
						Candles[symbol].Add(candle);

					d_Candle[symbol] = candle;

					MessageType = "updateCandles";
				}
				else if (method == "snapshotTrades" && Params != null)
				{
					var Data = jObject["params"]["data"];
					symbol = (string)jObject["params"]["symbol"];

					List<SocketTrade> ListSocketTradeData = JsonConvert.DeserializeObject<List<SocketTrade>>(Data.ToString());
					ListSocketTradeData.ForEach(c => c.TimeStamp = c.TimeStamp + TimeZone);
					if (!Trades.ContainsKey(symbol))
						Trades.Add(symbol, new List<SocketTrade>());

					Trades[symbol] = ListSocketTradeData;
					if(!d_Trades.ContainsKey(symbol))
						d_Trades.Add(symbol, new SocketTrade());
					if (Trades[symbol].Count > 0)
					{
						d_Trades[symbol] = Trades[symbol].Last();
						MessageType = "snapshotTrades";
					}
				}
				else if (method == "updateTrades" && Params != null)
				{
					var Data = jObject["params"]["data"];
					symbol = (string)jObject["params"]["symbol"];
					Trade = JsonConvert.DeserializeObject<SocketTrade>(Data[0].ToString());
					Trade.TimeStamp = Trade.TimeStamp + TimeZone;
					Trade.Symbol = symbol;
					Trades[symbol].Add(Trade);
					d_Trades[symbol] = Trade;

					MessageType = "updateTrades";
				}
			}
			else
			{
				this.Error = JsonConvert.DeserializeObject<Error>(Error.ToString());
				this.Error.Id = id;
			}

			MessageReceived?.Invoke(MessageType, symbol);
		}

		internal void Socket_DataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine("Socket_DataReceived");
		}

		internal void Socket_Opened(object sender, EventArgs e)
		{
			Opened?.Invoke(e.ToString(), "");
		}

		private void Socket_Closed(object sender, EventArgs e)
		{
			Closed?.Invoke(e.ToString(), "");
		}
	}
}
