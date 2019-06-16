
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using HitBTC;
using HitBTC.Models;
using Trading;
using Trading.Utilities;
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
			while (HitBTC.MessageType != "getSymbol") { Thread.Sleep(1); }

			HitBTC.SocketTrading.GetTradingBalance();
			while (HitBTC.MessageType != "balance") { Thread.Sleep(1); }
			
			HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
			Trading.DemoBalance = HitBTC.Balance;
			Trading.DemoBalance["USD"].Available = 10.0m;
			//Trading.DemoBalance.Add("USD", new Balance { Currency = "USD", Available = 10.0m });
			//Trading.DemoBalance.Add("BTC", new Balance { Currency = "BTC", Available = 00.0m });
			//Trading.DemoBalance.Add("ETH", new Balance { Currency = "ETH", Available = 00.0m });

			HitBTC.SocketMarketData.SubscribeTicker("BTCUSD");
			HitBTC.SocketMarketData.SubscribeTicker("ETHUSD");
			//HitBTC.SocketMarketData.SubscribeCandles("BTCUSD", period: Period.M1, 1000);
			//HitBTC.SocketMarketData.SubscribeCandles("ETHUSD", period: Period.M1, 1000);

			string[] treadingBaseCurrency = {  "BTC", "ETH", "ETC", "LTC", "XRP", "ZEC", "TRX", "EOS", "NEO", "ADA",
												"XLM", "XMR", "BTG", "ZIL", "DOGE"};

			for (int i = 0; i < HitBTC.Symbols.Count(); i++)
			{
				string symbol = HitBTC.Symbols.ElementAt(i).Key;
				string baseCurrency = HitBTC.Symbols.ElementAt(i).Value.BaseCurrency;
				string quoteCurrency = HitBTC.Symbols.ElementAt(i).Value.QuoteCurrency;
				//string symbol = treadingBaseCurrency[i] + "USD";
				if (quoteCurrency == "USD" ) // || quoteCurrency == "BTC" || quoteCurrency == "ETH")
				{
					//if (!Trading.DemoBalance.ContainsKey(baseCurrency))
					//	Trading.DemoBalance.Add(baseCurrency, 0);
					Trading.Add(symbol, startingQuantity: 0.1m, treadingQuantity: 0.1m, stopPercent: 1.0m, closePercent: 0.5m);
				}
			}

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
			Trading.Load("tr.dat");

			bool close = false;
			while (close != true)
			{
				Console.ReadLine();
				HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
				Console.SetCursorPosition(0, 40);
				Console.WriteLine("Continue       > 1");
				Console.WriteLine("Subtotal       > 2");
				Console.WriteLine("Sell all/exit  > 3");
				Console.WriteLine("Save and exit  > 4");
				Console.CursorVisible = true;
				Console.WriteLine();
				Console.Write("> ");

				string ansver = Console.ReadLine();

				switch (ansver)
				{
					case "1":
						HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
						break;
					case "2":
						Console.CursorVisible = false;
						Trading.Save("tr.dat");
						SubtotalBalanse();
						Screen.PrintBalance(column: 20, row: 23, count: 20, Trading.DemoBalance);
						Console.ReadLine();
						Trading.Load("tr.dat");
						HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
						break;
					case "3":
						Console.CursorVisible = false;
						SubtotalBalanse();
						Screen.PrintBalance(column: 20, row: 23, count: 20, Trading.DemoBalance);
						Trading.Save("tr.dat");
						Console.ReadLine();
						close = true;
						break;
					case "4":
						Console.CursorVisible = false;
						Trading.Save("tr.dat");
						SubtotalBalanse();
						Screen.PrintBalance(column: 20, row: 23, count: 20, Trading.DemoBalance);
						Console.ReadLine();
						close = true;
						break;

					default:
						HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
						break;
				}

				Console.CursorVisible = false;
				Console.Clear();
			}
		}

		private static void SubtotalBalanse()
		{
			for (int i = 0; i < Trading.DemoBalance.Keys.Count; i++)
			{
				string baseCurrency = Trading.DemoBalance.ElementAt(i).Key;
				string quoteCurrency = "USD";
				string symbol = String.Concat(baseCurrency, quoteCurrency);

				if (Trading.DemoBalance.ElementAt(i).Key != "USD")
					if (Trading.DemoBalance.ElementAt(i).Value.Available > 0.0m)
						if (HitBTC.d_Tickers.ContainsKey(symbol))
							Trading.Sell(symbol, HitBTC.d_Tickers[symbol].Bid, Trading.DemoBalance.ElementAt(i).Value.Available);
						else if (HitBTC.d_Tickers.ContainsKey(String.Concat(baseCurrency, "BTC")))
						{
							quoteCurrency = "BTC";
							symbol = String.Concat(baseCurrency, quoteCurrency);
							Trading.Sell(symbol, HitBTC.d_Tickers[symbol].Bid, Trading.DemoBalance.ElementAt(i).Value.Available);
						}
						else if (HitBTC.d_Tickers.ContainsKey(String.Concat(baseCurrency, "ETH")))
						{
							quoteCurrency = "ETH";
							symbol = String.Concat(baseCurrency, quoteCurrency);
							Trading.Sell(symbol, HitBTC.d_Tickers[symbol].Bid, Trading.DemoBalance.ElementAt(i).Value.Available);
						}
			}

			if (Trading.DemoBalance["BTC"].Available > 0.0m)
				Trading.Sell("BTCUSD", HitBTC.d_Tickers["BTCUSD"].Bid, Trading.DemoBalance["BTC"].Available);
			if (Trading.DemoBalance["ETH"].Available > 0.0m)
				Trading.Sell("ETHUSD", HitBTC.d_Tickers["ETHUSD"].Bid, Trading.DemoBalance["ETH"].Available);


		}

		static Dictionary<string, DateTime> d_DateTimes = new Dictionary<string, DateTime>();
		private static void HitBTCSocket_MessageReceived(string s, string symbol)
		{
			if (s == "getSymbol")
			{
				IsSymbol = true;
			}
			else if(s == "updateCandles" && symbol != null)
			{
				var candle = HitBTC.d_Candle[symbol];

				if (!d_DateTimes.ContainsKey(symbol))
					d_DateTimes.Add(symbol, HitBTC.Candles[symbol].Last().TimeStamp);

				if (d_DateTimes[symbol] == candle.TimeStamp)
				{
					Trading.Run_4(symbol);
					Screen.Print();
				}
				else
				{
					Trading.SmaSlow[symbol].NextAverage(HitBTC.d_Candle[symbol].Close);
					Trading.SmaFast[symbol].NextAverage(HitBTC.d_Candle[symbol].Close);
					d_DateTimes[symbol] = HitBTC.Candles[symbol].Last().TimeStamp;
				}
			}
			else if (s == "snapshotCandles" && symbol != null)
			{				
				if(!d_DateTimes.ContainsKey(symbol))
					d_DateTimes.Add(symbol, new DateTime());
				d_DateTimes[symbol] = HitBTC.Candles[symbol].Last().TimeStamp;

				if (!Trading.SmaFast.ContainsKey(symbol))
					Trading.SmaFast.Add(symbol, new SMA(4));
				if (!Trading.SmaSlow.ContainsKey(symbol))
					Trading.SmaSlow.Add(symbol, new SMA(7));

				foreach (var candle in HitBTC.Candles[symbol])
				{
					Trading.SmaSlow[symbol].NextAverage(candle.Close);
					Trading.SmaFast[symbol].NextAverage(candle.Close);
				}
			}
		}
	}
}
