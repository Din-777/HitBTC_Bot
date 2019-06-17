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
		public static bool IsSymbol = false;

		static decimal Price;
		static void Main(string[] args)
		{
			string Symbol = "BTCUSD";
			HitBTC = new HitBTCSocketAPI();
			HitBTC.SocketMarketData.GetSymbols();

			Thread.Sleep(2000);

			foreach(var s in HitBTC.Symbols)
			{
				if(s.Key.EndsWith("USD"))
				{
					HitBTC.SocketMarketData.SubscribeCandles(s.Key, Period.M1, 1);
					Thread.Sleep(20);
					HitBTC.SocketMarketData.SubscribeTrades(s.Key, 1);
					Thread.Sleep(20);
					//HitBTC.SocketMarketData.SubscribeTicker(s.Key);
				}
			}						

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			Console.ReadKey();
		}
		private static void HitBTCSocket_MessageReceived(string notification, string symbol)
		{
		}
	}
}
