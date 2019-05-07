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
		public List<Ticker> L_Tickers = new List<Ticker>();
		public Dictionary<string, Ticker> D_Tickers = new Dictionary<string, Ticker>();
		public Dictionary<string, Balance> Balance;
		public SocketOrederResult NewOrederResult;
		public Stack<SocketOrederResult> stackPlaceNewOrderResults;
		public Dictionary<string, Symbol> Symbols;

		public Stack<ParamsActiveOrders> ActiveOrders;

		static async void ConnectAsync(WebSocket socket)
		{
			try { await Task.Run(() => socket.Open()); }
			catch { Console.WriteLine("......................"); }						
		}
		public void SocketDisconect()
		{
			socket.Close();
		}
		public void SocketConect()
		{
			socket.Open();
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

			while (socket.State != WebSocketState.Open)
			{				
				ConnectAsync(socket);
				Thread.Sleep(1000);
			}


			socket.Opened += Socket_Opened;
			socket.Closed += Socket_Closed;
			socket.DataReceived += Socket_DataReceived;
			socket.MessageReceived += Socket_MessageReceived;
		}

		private void Socket_Closed(object sender, EventArgs e)
		{
			if (Closed != null) Closed(e.ToString());
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
				else if (id == "placeNewOrder")
				{
					NewOrederResult = JsonConvert.DeserializeObject<SocketOrederResult>(result.ToString());
					stackPlaceNewOrderResults.Push(NewOrederResult);
					str = "placeNewOrder";
				}
				else if (id == "getSymbol")
				{
					Symbols = (JsonConvert.DeserializeObject<List<Symbol>>(result.ToString())).ToDictionary(t=> t.Id);
					str = "getSymbol";
				}
				else if (method == "activeOrders")
				{
					this.ActiveOrders = JsonConvert.DeserializeObject<Stack<ParamsActiveOrders>>(Params.ToString());
				}
				else if (method == "ticker" && Params != null)
				{
					Ticker = JsonConvert.DeserializeObject<Ticker>(Params.ToString());
					if (D_Tickers.ContainsKey(Ticker.Symbol))
					{
						D_Tickers[Ticker.Symbol] = Ticker;
					}
					else
					{
						D_Tickers.Add(Ticker.Symbol, Ticker);
					}

					L_Tickers.Add(Ticker);
					if (L_Tickers.Count > 20) L_Tickers.RemoveAt(0);

					str = "ticker";
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
	}
}
