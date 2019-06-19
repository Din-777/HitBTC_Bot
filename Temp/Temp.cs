using System;

using HitBTC;
using HitBTC.Models;
using System.Diagnostics;
using System.Threading;

namespace Temp
{
	class Temp
	{
		static string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
		static string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";
		static Stopwatch sw;

		static HitBTCSocketAPI HitBTC;

		static void Main(string[] args)
		{
			string Symbol = "BTCUSD";
			HitBTC = new HitBTCSocketAPI();

			HitBTC.SocketMarketData.SubscribeTrades("BTCUSD", 1000);

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			TimeSpan timeSpan = TimeSpan.FromHours(5);
			DateTime dt = DateTime.Now;

			dt = dt + timeSpan;

			Console.ReadKey();
		}

		static DateTime NextDateTime = DateTime.Now;

		private static void HitBTCSocket_MessageReceived(string notification, string symbol)
		{
			if (notification == "updateTrades" && symbol != null)
			{
				var trade = HitBTC.d_Trades[symbol];

				if(trade.TimeStamp > NextDateTime)
				{
					NextDateTime = NextDateTime + TimeSpan.FromMinutes(1);
					Console.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff"));
				}

			}
		}
	}
}
