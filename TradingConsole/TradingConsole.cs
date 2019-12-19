
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using HitBTC;
using HitBTC.Models;
using Trading;
using Trading.Utilities;
using Screen;

using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace TradingConsole
{
	class TradingConsole
	{
		static HitBTCSocketAPI HitBTC;
		static Screen.Screen Screen;

		static string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
		static string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";

		public static Trading.Trading Trading;
		public static string TradingDataFileName = "tr.dat";

		static void Main(string[] args)
		{
			SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

			if (pKey == null || sKey == null)
			{
				using (var reader = new StreamReader("key.txt"))
				{
					pKey = reader.ReadLine().Substring(5);
					sKey = reader.ReadLine().Substring(5);
				}
			}

			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC);
			Screen = new Screen.Screen(ref HitBTC, ref Trading);

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			HitBTC.SocketAuth.Auth(pKey, sKey);

			HitBTC.SocketMarketData.GetSymbols();
			while (HitBTC.MessageType != "getSymbol") { Thread.Sleep(1); }

			HitBTC.SocketTrading.GetTradingBalance();
			while (HitBTC.MessageType != "balance") { Thread.Sleep(1); }
			
			//HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;

			Trading.DemoBalance = HitBTC.Balance;
			if(Trading.Demo == true)
				Trading.DemoBalance["USD"].Available = 100.0m;

			Trading.Load(TradingDataFileName);

			for (int i = 0; i < HitBTC.Symbols.Count(); i++)
			{
				string symbol = HitBTC.Symbols.ElementAt(i).Key;
				string baseCurrency = HitBTC.Symbols.ElementAt(i).Value.BaseCurrency;
				string quoteCurrency = HitBTC.Symbols.ElementAt(i).Value.QuoteCurrency;

				if (symbol.EndsWith("USD") || symbol.EndsWith("USDT"))
				{
					HitBTC.SocketMarketData.SubscribeTicker(symbol);
					Thread.Sleep(10);
				}
			}
			
			//HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			Timer timer = new Timer(new TimerCallback(BotStep), null, 0, 1000 * 60 * 5);
			Trading.DateTimeStartCurr = DateTime.Now;

			//Process.Start("https://hitbtc.com/exchange/BTC-to-USDT");

			bool close = false;
			while (close != true)
			{
				Console.ReadLine();
				HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
				Thread.Sleep(500);
				close = Screen.MenuRun();
				Console.Clear();
				HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
			}
		}

		private static void BotStep(object state)
		{
			Dictionary<string, Ticker> d_Tickers_sort =
				HitBTC.d_Tickers.Where(v => v.Value.VolumeQuoute > 10000m)
					.OrderByDescending(v => ((v.Value.Ask - v.Value.Bid) / v.Value.Bid) * v.Value.VolumeQuoute)
					.OrderByDescending(v => 100.0m / (v.Value.Bid / v.Value.Ask) - 100.0m)
					.Where(v => (100.0m / (v.Value.Bid / v.Value.Ask) - 100.0m) > 0.5m)
					.ToDictionary(v => v.Value.Symbol, v => v.Value);


			Trading.d_OrdersBuy.Clear();
			for (int i = 0; i < d_Tickers_sort.Count; i++)
			{
				string symbol = d_Tickers_sort.Values.ElementAt(i).Symbol;
				string baseCurrency = HitBTC.Symbols[symbol].BaseCurrency;

				if (Trading.DemoBalance["USD"].Available >= 10.0m && Trading.DemoBalance[baseCurrency].Available == 0)
				{
					Trading.d_OrdersBuy.Add(d_Tickers_sort.Values.ElementAt(i).Symbol,
						new Trading.OrderBuy
						{
							Symbol = symbol,
							BuyPrice = d_Tickers_sort[symbol].Bid,
							Quantity = 10.0m / d_Tickers_sort[symbol].Bid,
							Distance = 100.0m / (d_Tickers_sort[symbol].Bid / d_Tickers_sort[symbol].Ask) - 100.0m
						});
				}
			}

			//Trading.d_OrdersSell.Clear();
			foreach (var b in Trading.DemoBalance.Values)
			{
				if (b.Available > 0 && b.Currency != "USD" && 
					(Trading.d_OrdersSell.ContainsKey(b.Currency + "USD") == false || !Trading.d_OrdersSell.ContainsKey(b.Currency + "USDT") == false))
				{
					string symbol = HitBTC.Symbols.ContainsKey(b.Currency + "USD") ? b.Currency + "USD" : b.Currency + "USDT";
					Trading.d_OrdersSell.Add(symbol,
						new Trading.OrderSell
						{
							Symbol = symbol,
							BuyPrice = 0,
							SellPrice = HitBTC.d_Tickers[symbol].Ask,
							Quantity = b.Available
						});
				}
			}

			foreach (var v in Trading.d_OrdersSell.Values)
			{
				v.Distance = 100.0m / (HitBTC.d_Tickers[v.Symbol].Bid / v.SellPrice) - 100.0m;
			}

			Screen.Print();
			Trading.Save(TradingDataFileName);
		}

		private static void HitBTCSocket_MessageReceived(string s, string symbol)
		{
			if (s == "subscribeTicker")
			{
				if (Trading.d_OrdersBuy.ContainsKey(symbol))
				{
					if (Trading.d_OrdersBuy[symbol].BuyPrice >= HitBTC.d_Tickers[symbol].Ask)
					{
						Trading.DemoBalance["USD"].Available -= HitBTC.d_Tickers[symbol].Ask * Trading.d_OrdersBuy[symbol].Quantity;
						Trading.DemoBalance[HitBTC.Symbols[symbol].BaseCurrency].Available += Trading.d_OrdersBuy[symbol].Quantity;
						Trading.d_OrdersBuy.Remove(symbol);
					}
				}

				if (Trading.d_OrdersSell.ContainsKey(symbol))
				{
					if (Trading.d_OrdersSell[symbol].SellPrice <= HitBTC.d_Tickers[symbol].Bid)
					{
						Trading.DemoBalance["USD"].Available += HitBTC.d_Tickers[symbol].Bid * Trading.DemoBalance[HitBTC.Symbols[symbol].BaseCurrency].Available;
						Trading.DemoBalance[HitBTC.Symbols[symbol].BaseCurrency].Available = 0;
						Trading.d_OrdersSell.Remove(symbol);						
					}
				}
			}

			if (s == "auth")
			{
				//Console.WriteLine(symbol);
			}
		}

	#region Unmanaged
		private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
		{
			HitBTC.ReceiveMessages(false);
			HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;

			if(Trading.DataFileIsloaded)
			{
				Console.Clear();
				Console.Write("SAVE...");
				Trading.Save(TradingDataFileName);
				Console.Write("  OK");
				Thread.Sleep(1000);
			}
			return true;
		}

		// Declare the SetConsoleCtrlHandler function
		// as external and receiving a delegate.
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

		// A delegate type to be used as the handler routine
		// for SetConsoleCtrlHandler.
		private delegate bool HandlerRoutine(CtrlTypes CtrlType);

		// An enumerated type for the control messages
		// sent to the handler routine.
		private enum CtrlTypes
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

	#endregion

	}
}
