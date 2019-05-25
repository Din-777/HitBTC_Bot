
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
        public static bool IsSymbol = false;		

		static void Main(string[] args)
		{
			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC);
			Screen = new Screen.Screen(ref HitBTC, ref Trading);

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			HitBTC.SocketAuth.Auth(pKey, sKey);
            HitBTC.SocketMarketData.GetSymbols();
            HitBTC.SocketTrading.GetTradingBalance();

            while (!IsSymbol) Thread.Sleep(100);

			Trading.DemoBalance.Add("USD", 10.0m);
            Trading.DemoBalance.Add("BTC", 0.0m);
            Trading.DemoBalance.Add("ETH", 0.0m);

            HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
            HitBTC.SocketMarketData.SubscribeTicker("BTCUSD");
            HitBTC.SocketMarketData.SubscribeTicker("ETHUSD");

            for (int i = 0; i < HitBTC.Symbols.Count; i ++)
			{
                string symbol = HitBTC.Symbols.ElementAt(i).Key;
                string baseCurrency = HitBTC.Symbols.ElementAt(i).Value.BaseCurrency;
				string quoteCurrency = HitBTC.Symbols.ElementAt(i).Value.QuoteCurrency;

				if (quoteCurrency == "USD" || quoteCurrency == "BTC" || quoteCurrency == "ETH")
				{
					if (!Trading.DemoBalance.ContainsKey(baseCurrency))
						Trading.DemoBalance.Add(baseCurrency, 0);
                    Trading.Add(symbol, startingQuantity: 0.2m, treadingQuantity: 0.2m, stopPercent: 1.0m, closePercent: 0.5m);
                }
			}

            Trading.Load("tr.dat");

            HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

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
                Trading.Sell("ETHUSD", HitBTC.d_Tickers["ETHUSD"].Bid, Trading.DemoBalance["ETH"]);

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

            if (s == "getSymbol")
            {
                IsSymbol = true;
            }
        }
	}
}
