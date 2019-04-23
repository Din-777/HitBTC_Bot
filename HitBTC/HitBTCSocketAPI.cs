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

namespace HitBTC
{
	public class HitBTCSocketAPI
	{
		internal WebSocket socket;

		public delegate void SocketHandler(string s);
		public event SocketHandler Opened;
		public event SocketHandler MessageReceived;

		public Ticker Ticker;

		public async void SubscribeTicker(string symbol)
		{
			var s = new SubscribeTicker(symbol);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));

			socket.MessageReceived += Socket_MessageReceived;
		}

		static async void ConnectAsync(WebSocket socket)
		{
			await Task.Run(() => socket.Open());
		}

		public HitBTCSocketAPI()
		{
			string uri = "wss://api.hitbtc.com/api/2/ws";
			socket = new WebSocket(uri);

			ConnectAsync(socket);

			while (socket.State != WebSocketState.Open) { }

			socket.Opened += Socket_Opened;
		}

		public virtual void Socket_Opened(object sender, EventArgs e)
		{
			if (Opened != null) Opened(e.ToString());
		}

		internal void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			Ticker = JsonConvert.DeserializeObject<NotificationTicker>(e.Message).Ticker;

			if (MessageReceived != null) MessageReceived(e.Message);
		}
	}
}
