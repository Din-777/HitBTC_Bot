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
		public Dictionary<string, Ticker> d_Tickers = new Dictionary<string, Ticker>();
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
		public Stack<ParamsActiveOrders> ActiveOrders;

		public async void ConnectAsync(WebSocket socket)
		{
			try
			{
				await Task.Run(() => socket.Open());
			}
			catch
			{
				//Console.SetCursorPosition(0, 0);
				//Console.Clear();
				//Console.WriteLine("Connecting...");
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

		internal void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			string str = null;
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
					try { Ticker = JsonConvert.DeserializeObject<Ticker>(Params.ToString()); }
					catch { Ticker = null; }
					finally
					{
						if (Ticker == null)
						{
							str = "null";							
						}
						else
						{
							L_Tickers.Add(Ticker);
							if (L_Tickers.Count > 20) L_Tickers.RemoveAt(0);

							if (!d_Tickers.ContainsKey(Ticker.Symbol))
								d_Tickers.Add(Ticker.Symbol, new Ticker());

							d_Tickers[Ticker.Symbol] = Ticker;

							str = "ticker";
						}
					}
				}
				else if (method == "snapshotCandles" && Params != null)
				{ 
					var Data = jObject["params"]["data"];
					symbol = (string)jObject["params"]["symbol"];

					List<Candle> lCandle = JsonConvert.DeserializeObject<List<Candle>>(Data.ToString());
					lCandle.ForEach(c => c.TimeStamp = c.TimeStamp + TimeSpan.FromHours(5));

					if (!Candles.ContainsKey(symbol))
						Candles.Add(symbol, new List<Candle>());

					if (!d_Candle.ContainsKey(symbol))
						d_Candle.Add(symbol, new Candle());

					d_Candle[symbol] = lCandle.Last();
					Candles[symbol] = lCandle; 
					str = "snapshotCandles";
				}
				else if (method == "updateCandles" && Params != null)
				{
					var Data = jObject["params"]["data"];
					symbol = (string)jObject["params"]["symbol"];

					Candle candle = JsonConvert.DeserializeObject<Candle>(Data[0].ToString());

					candle.TimeStamp = candle.TimeStamp + TimeSpan.FromHours(5);

					if (!Candles.ContainsKey(symbol))
						Candles.Add(symbol, new List<Candle>());

					if (Candles[symbol].Last().TimeStamp == candle.TimeStamp)
					{
						Candles[symbol].Last().Close = candle.Close;
						Candles[symbol].Last().Max = candle.Max;
						Candles[symbol].Last().Min = candle.Min;
						Candles[symbol].Last().Open = candle.Open;
						Candles[symbol].Last().TimeStamp = candle.TimeStamp;
						Candles[symbol].Last().Volume = candle.Volume;
						Candles[symbol].Last().VolumeQuote = candle.VolumeQuote;
					}
					else
						Candles[symbol].Add(candle);

					d_Candle[symbol] = candle;

					str = "updateCandles";
				}
				else if (method == "snapshotTrades" && Params != null)
				{
					var Data = jObject["params"]["data"];
					symbol = (string)jObject["params"]["symbol"];

					List<SocketTrade> ListSocketTradeData = JsonConvert.DeserializeObject<List<SocketTrade>>(Data.ToString());
					if (!Trades.ContainsKey(symbol))
						Trades.Add(symbol, new List<SocketTrade>());

					Trades[symbol] = ListSocketTradeData;
					d_Trades.Add(symbol, new SocketTrade());
					str = "snapshotTrades";
				}
				else if (method == "updateTrades" && Params != null)
				{
					var Data = jObject["params"]["data"];
					symbol = (string)jObject["params"]["symbol"];
					Trade = JsonConvert.DeserializeObject<SocketTrade>(Data[0].ToString());
					Trade.Symbol = symbol;
					Trades[symbol].Add(Trade);
					d_Trades[symbol] = Trade;

					str = "updateTrades";
				}
			}
			else
			{
				this.Error = JsonConvert.DeserializeObject<Error>(Error.ToString());
				this.Error.Id = id;
			}

			MessageReceived?.Invoke(str, symbol);
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
