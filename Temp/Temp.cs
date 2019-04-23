using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Threading;

namespace Temp
{
	public class Params
	{
		[JsonProperty("symbol")]
		string symbol = "BTCUSD";
	}

	public class Method
	{
		[JsonProperty("method")]
		string method = "subscribeTicker";

		[JsonProperty("params")]
		Params Params = new Params();

		[JsonProperty("id")]
		string id = "123";	
	}

	public class Ticker
	{
		public string ask;
		public float bid;
		public float last;
		public float open;
		public float low;
		public float high;
		public float volume;
		public float volumeQuote;
		public DateTime timestamp;
		public string symbol;
	}

	public class Error
	{
		public int code;
		public string message;
		public string description;
	}

	public class NotificationTicker
	{
		[JsonProperty("jsonrpc")]
		public string Jsonrpc;

		[JsonProperty("result")]
		public bool Result;

		[JsonProperty("method")]
		public string Method;

		[JsonProperty("params")]
		public Ticker Ticker;

		[JsonProperty("error")]
		public Error Error;
	}

	class Temp
	{
		static void Main(string[] args)
		{
			string uri = "wss://api.hitbtc.com/api/2/ws";

			Method request = new Method();			

			WebSocket socket = new WebSocket(uri);
			socket.Open();

			Thread.Sleep(2000);

			socket.Opened += Socket_Opened;
			socket.DataReceived += Socket_DataReceived;
			socket.MessageReceived += Socket_MessageReceived;
			socket.Closed += Socket_Closed;

			var jsonStr = JsonConvert.SerializeObject(request);
			socket.Send(jsonStr);

			
			Console.ReadKey();
		}

		private static void Socket_Closed(object sender, EventArgs e)
		{
			Console.WriteLine("Socket_Closed");
		}

		private static void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
		{
			var x = JsonConvert.DeserializeObject<NotificationTicker>(e.Message);

			if(x.Result) Console.WriteLine("Socket_MessageRequest OK");
			else if(x.Method == "ticker") Console.WriteLine("ask {0}	bid {1}", x.Ticker.ask, x.Ticker.bid);

		}

		private static void Socket_DataReceived(object sender, DataReceivedEventArgs e)
		{
			Console.WriteLine("Socket_DataReceived {0}", e.Data);
		}

		private static void Socket_Opened(object sender, EventArgs e)
		{
			Console.WriteLine("Socket_Opened");
		}
	}
}
