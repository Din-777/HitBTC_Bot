
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

			Trading.DemoBalance.Add("USD", 10.0f);
			Trading.DemoBalance.Add("BTC", 0.00f);
			Trading.DemoBalance.Add("ETH", 0.00f);
			Trading.DemoBalance.Add("LTC", 0.00f);
			Trading.DemoBalance.Add("ETC", 0.00f);			
			//Trading.DemoBalance.Add("BCN", 0.00f);
			//Trading.DemoBalance.Add("ETN", 0.00f);
			//Trading.DemoBalance.Add("LSK", 0.00f);
			//Trading.DemoBalance.Add("PPC", 0.00f);


			Trading.Add("BTCUSD", 20.0f, 10.0f, 2.0f, 0.5f);
			Trading.Add("ETHUSD", 20.0f, 10.0f, 2.0f, 0.5f);
			Trading.Add("LTCUSD", 20.0f, 10.0f, 2.0f, 0.5f);
			Trading.Add("ETCUSD", 20.0f, 10.0f, 2.0f, 0.5f);			
			//Trading.Add("BCNUSD", 1.0f, 0.3f, 0.3f);
			//Trading.Add("ETNUSD", 1.0f, 0.1f, 0.2f);
			//Trading.Add("LSKUSD", 1.0f, 0.1f, 0.2f);
			//Trading.Add("PPCUSD", 1.0f, 0.1f, 0.2f);

			//Trading.Load("tr.dat");

			Console.ReadLine();

			HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;	

			Trading.Save("tr.dat");

			for(int i = 0; i < Trading.DemoBalance.Keys.Count; i++)
			{
				if(Trading.DemoBalance.ElementAt(i).Key != "USD")
					if(Trading.DemoBalance.ElementAt(i).Value > 0.0f)
						Trading.Sell(Trading.DemoBalance.ElementAt(i).Key.Insert(3, "USD"),
							HitBTC.d_Tickers[Trading.DemoBalance.ElementAt(i).Key.Insert(3, "USD")].Bid,
							Trading.DemoBalance.ElementAt(i).Value);
			}

			for (int i = 0; i < Trading.DemoBalance.Count; i++)
			{
				Console.SetCursorPosition(45, 24 + i);
				Console.WriteLine("{0}  {1:00.000000}", Trading.DemoBalance.ElementAtOrDefault(i).Key, Trading.DemoBalance.ElementAt(i).Value);
			}

			Console.ReadLine();
		}

		private static void HitBTCSocket_MessageReceived(string s)
		{
			if (s == "ticker")
			{
				Trading.Run_3();
				Screen.Print();
			}
		}
	}
}
