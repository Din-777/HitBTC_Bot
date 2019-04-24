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

		public async void SubscribeTicker(string symbol)
		{
			var s = new Categories.SubscribeTicker(symbol);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));

			socket.MessageReceived += Socket_MessageReceived;
		}
		
		public async void UnSubscribeTicker(string symbol)
		{
			var s = new Categories.UnSubscribeTicker(symbol);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));

			socket.MessageReceived += Socket_MessageReceived;
		}

		static async void ConnectAsync(WebSocket socket)
		{
			await Task.Run(() => socket.Open());
		}

		public async void Auth(string pKey, string sKey)
		{
			var s = new Categories.SocketAuth(pKey, sKey);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}

		public async void GetTradingBalance()
		{
			var s = new Categories.GetTradingBalance();
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}

		public HitBTCSocketAPI()
		{
			string uri = "wss://api.hitbtc.com/api/2/ws";
			socket = new WebSocket(uri);

			ConnectAsync(socket);

			while (socket.State != WebSocketState.Open) { Thread.Sleep(100); }

			socket.Opened += Socket_Opened;
			socket.DataReceived += Socket_DataReceived;
			socket.MessageReceived += Socket_MessageReceived;
		}
		
		
		internal void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			var jo = JObject.Parse(e.Message);

			var Params = jo["params"];
			var id = (string)jo["id"];
			var method = (string)jo["method"];
			var result = jo["result"];

			if (id == "balance")
			{
				List<Balance> ListBalance = JsonConvert.DeserializeObject<List<Balance>>(result.ToString());
				Balance = ListBalance.ToDictionary(b => b.Currency);
			}			


			if (method == "ticker" && Params != null)
			{
				Ticker = JsonConvert.DeserializeObject<Ticker>(Params.ToString());
			}

			if (MessageReceived != null) MessageReceived(e.Message);
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
