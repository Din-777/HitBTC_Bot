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

		public Ticker Ticker;
		public Dictionary<string, Balance> Balance;
		

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
			var jo = JObject.Parse(e.Message);
			string str = null;

			var Params = jo["params"];
			var id = (string)jo["id"];
			var method = (string)jo["method"];
			var result = jo["result"];

			if (id == "balance")
			{
				List<Balance> ListBalance = JsonConvert.DeserializeObject<List<Balance>>(result.ToString());
				Balance = ListBalance.ToDictionary(b => b.Currency);

				str = "balance";
			}			
			
			if (method == "ticker" && Params != null)
			{
				Ticker = JsonConvert.DeserializeObject<Ticker>(Params.ToString());

				str = "ticker";
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
