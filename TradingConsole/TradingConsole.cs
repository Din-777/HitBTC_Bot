
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

		static string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
		static string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";

		public static Trading.Trading Trading;

		static void Main(string[] args)
		{
			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC);
			Screen = new Screen.Screen(ref HitBTC, ref Trading);

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			HitBTC.SocketAuth.Auth(pKey, sKey);
			HitBTC.SocketTrading.GetTradingBalance();
			HitBTC.SocketMarketData.GetSymbols();

			Thread.Sleep(2000);		

			Trading.DemoBalance.Add("USD", 20.0f);
			Trading.DemoBalance.Add("BTC", 0.001f);
			Trading.DemoBalance.Add("ETH", 0.01f);
			Trading.DemoBalance.Add("ETC", 0.10f);
			Trading.DemoBalance.Add("LTC", 0.01f);


			Trading.Add("BTCUSD", 1.0f, 0.03f, 0.22f);
			Trading.Add("ETHUSD", 1.0f, 0.03f, 0.22f);
			Trading.Add("ETCUSD", 1.0f, 0.03f, 0.22f);
			Trading.Add("LTCUSD", 1.0f, 0.03f, 0.22f);

			Trading.Load("tr.dat");


			Console.ReadLine();

			Trading.Save("tr.dat");
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
