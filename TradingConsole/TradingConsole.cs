
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
            HitBTC.SocketMarketData.GetSymbols();
            HitBTC.SocketTrading.GetTradingBalance();			

			Thread.Sleep(5000);

			Trading.DemoBalance.Add("USD", 100.0m);

			foreach(var symbol in HitBTC.Symbols)
			{
				string baseCurrency = symbol.Key.Substring(0, symbol.Key.Length - 3);
				string quoteCurrency = symbol.Key.Substring(symbol.Key.Length - 3, 3);

				if (quoteCurrency == "USD" || quoteCurrency == "BTC" || quoteCurrency == "ETH")
				{
					if (!Trading.DemoBalance.ContainsKey(baseCurrency))
						Trading.DemoBalance.Add(baseCurrency, 0);
					HitBTC.SocketMarketData.SubscribeTicker(symbol.Key);
					Trading.Add(symbol.Key, 20.0m, 10.0m, 2.0m, 0.5m);
				}
			}

			/*for(int i = 0; i < 300; i ++)
			{
				string baseCurrency = HitBTC.Symbols.ElementAt(i).Key.Substring(0, HitBTC.Symbols.ElementAt(i).Key.Length - 3);
				string quoteCurrency = HitBTC.Symbols.ElementAt(i).Key.Substring(HitBTC.Symbols.ElementAt(i).Key.Length - 3, 3);

				if (quoteCurrency == "USD" || quoteCurrency == "BTC" || quoteCurrency == "ETH")
				{
					if (!Trading.DemoBalance.ContainsKey(baseCurrency))
						Trading.DemoBalance.Add(baseCurrency, 0);
					HitBTC.SocketMarketData.SubscribeTicker(HitBTC.Symbols.ElementAt(i).Key);
					Trading.Add(HitBTC.Symbols.ElementAt(i).Key, 20.0m, 10.0m, 2.0m, 0.5m);
				}
			}*/

			//Trading.Load("tr.dat");

			Console.ReadLine();

			HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;	

			Trading.Save("tr.dat");

			for (int i = 0; i < Trading.DemoBalance.Keys.Count; i++)
			{
				string baseCurrency = Trading.DemoBalance.ElementAt(i).Key;
				string quoteCurrency = "USD";
				string symbol = String.Concat(baseCurrency, quoteCurrency);

				if (Trading.DemoBalance.ElementAt(i).Key != "USD")
					if (Trading.DemoBalance.ElementAt(i).Value > 0.0m)
						if (HitBTC.d_Tickers.ContainsKey(symbol))
							Trading.Sell(symbol, HitBTC.d_Tickers[symbol].Bid, Trading.DemoBalance.ElementAt(i).Value);
						else if(HitBTC.d_Tickers.ContainsKey(String.Concat(baseCurrency, "BTC")))
						{
							quoteCurrency = "BTC";
							symbol = String.Concat(baseCurrency, quoteCurrency);
							Trading.Sell(symbol, HitBTC.d_Tickers[symbol].Bid, Trading.DemoBalance.ElementAt(i).Value);
						}
                        else if (HitBTC.d_Tickers.ContainsKey(String.Concat(baseCurrency, "ETH")))
                        {
                            quoteCurrency = "ETH";
                            symbol = String.Concat(baseCurrency, quoteCurrency);
                            Trading.Sell(symbol, HitBTC.d_Tickers[symbol].Bid, Trading.DemoBalance.ElementAt(i).Value);
                        }
            }

			if (Trading.DemoBalance["BTC"] > 0.0m)
				Trading.Sell("BTCUSD", HitBTC.d_Tickers["BTCUSD"].Bid, Trading.DemoBalance["BTC"]);
            if (Trading.DemoBalance["ETH"] > 0.0m)
                Trading.Sell("ETHUSD", HitBTC.d_Tickers["BTCUSD"].Bid, Trading.DemoBalance["ETH"]);

            Screen.PrintBalance();

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
