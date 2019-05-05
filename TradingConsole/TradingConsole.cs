
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using HitBTC;
using HitBTC.Models;
using Trading;
using Screen;

namespace TradingConsole
{
	class TradingConsole
	{
		static HitBTCSocketAPI HitBTC;
		static Screen.Screen Screen;

		static string pKey = "";
		static string sKey = "";

		public static Trading.Trading Trading;

		static void Main(string[] args)
		{
			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC);
			Screen = new Screen.Screen(ref HitBTC, ref Trading.PendingOrders, ref Trading.ClosedOrders);

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			HitBTC.SocketAuth.Auth(pKey, sKey);
			HitBTC.SocketTrading.GetTradingBalance();

			Thread.Sleep(2000);

			HitBTC.SocketMarketData.GetSymbols();

			Trading.Add("BTCUSD", 1.0f, 0.01f, 0.3f );
			Trading.Add("ETHUSD", 1.0f, 0.01f, 0.3f);
			Trading.Add("ETCUSD", 1.0f, 0.01f, 0.3f);
			Trading.Add("LTCUSD", 1.0f, 0.01f, 0.3f);

			Console.ReadKey();
		}

		private static void HitBTCSocket_MessageReceived(string s)
		{
			if (s == "ticker")
			{
				Trading.Run(HitBTC.Ticker.Symbol);				

				Screen.Print();
			}
		}
	}

	public static class FloatExtension
	{
		public static float Percent(this float number, float percent)
		{
			//return ((double) 80         *       25)/100;
			return (number * percent) / 100.0f;
		}
	}
}
