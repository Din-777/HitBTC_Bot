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

		public bool Authorized = false;
		public Error Error;
		public Ticker Ticker;
		public Dictionary<string, Stack<Ticker>> Tickers = new Dictionary<string, Stack<Ticker>>();
		public Dictionary<string, Balance> Balance;
		public Stack<SocketOrederResult> stackPlaceNewOrderResults;

		public Stack<ParamsActiveOrders> ActiveOrders;

		static async void ConnectAsync(WebSocket socket)
		{
			await Task.Run(() => socket.Open());
		}
		
		public SocketTrading SocketTrading;
		public SocketMarketData SocketMarketData;
		public SocketAuth SocketAuth;

		public HitBTCSocketAPI()
		{
			string uri = "wss://api.hitbtc.com/api/2/ws";
			socket = new WebSocket(uri);

			Error = null;

			SocketAuth = new SocketAuth(ref socket);
			SocketTrading = new SocketTrading(ref socket);
			SocketMarketData = new SocketMarketData(ref socket);

			ConnectAsync(socket);

			while (socket.State != WebSocketState.Open) { Thread.Sleep(100); }

			socket.Opened += Socket_Opened;
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
				else if (id == "placeNewOrder")
				{
					stackPlaceNewOrderResults.Push(JsonConvert.DeserializeObject<SocketOrederResult>(result.ToString()));
					str = "placeNewOrder";
				}
				else if (method == "activeOrders")
				{
					this.ActiveOrders = JsonConvert.DeserializeObject<Stack<ParamsActiveOrders>>(Params.ToString());
				}
				else if (method == "ticker" && Params != null)
				{
					Ticker = JsonConvert.DeserializeObject<Ticker>(Params.ToString());
					if (Tickers.ContainsKey(Ticker.Symbol))
					{
						Tickers[Ticker.Symbol].Push(Ticker);
					}
					else
					{
						Stack<Ticker> st = new Stack<Ticker>();
						st.Push(Ticker);
						Tickers.Add(Ticker.Symbol, st);
					}

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
