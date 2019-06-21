
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
			
			HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
			Trading.DemoBalance = HitBTC.Balance;
			Trading.DemoBalance["USD"].Available = 10000.0m;

			string[] treadingBaseCurrency = {  "BTC", "ETH", "ETC", "LTC", "XRP", "ZEC", "TRX", "EOS", "NEO", "ADA",
												"XLM", "XMR", "BTG", "ZIL", "DOGE"};

			for (int i = 0; i < HitBTC.Symbols.Count(); i++)
			{
				string symbol = HitBTC.Symbols.ElementAt(i).Key;
				string baseCurrency = HitBTC.Symbols.ElementAt(i).Value.BaseCurrency;
				string quoteCurrency = HitBTC.Symbols.ElementAt(i).Value.QuoteCurrency;
				if (symbol.EndsWith("USD") || symbol.EndsWith("USDT"))
				{
					Trading.Add(symbol, period: Period.H1, treadingQuantity: 100.0m, stopPercent: 5.0m, closePercent: 0.5m);
				}
			}

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
			Trading.Load(TradingDataFileName);

			bool close = false;
			while (close != true)
			{
				Console.ReadLine();
				HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
				Console.SetCursorPosition(0, 40);
				Console.WriteLine("Continue          > 1");
				Console.WriteLine("Subtotal and save > 2");
				Console.WriteLine("Sell all/exit     > 3");
				Console.WriteLine("Save and exit     > 4");

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
						Trading.Save(TradingDataFileName);
						SubtotalBalanse();
						Screen.PrintBalance(column: 20, row: 23, count: 20, Trading.DemoBalance);
						Console.ReadLine();
						Trading.Load(TradingDataFileName);
						HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
						break;
					case "3":
						Console.CursorVisible = false;
						SubtotalBalanse();
						Screen.PrintBalance(column: 20, row: 23, count: 20, Trading.DemoBalance);
						Trading.Save(TradingDataFileName);
						Console.ReadLine();
						close = true;
						break;
					case "4":
						Console.CursorVisible = false;
						Trading.Save(TradingDataFileName);
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
						if (HitBTC.d_Candle.ContainsKey(symbol))
							Trading.Sell(symbol, HitBTC.d_Candle[symbol].Close, Trading.DemoBalance.ElementAt(i).Value.Available);
						else if (HitBTC.d_Candle.ContainsKey(String.Concat(baseCurrency, "BTC")))
						{
							quoteCurrency = "BTC";
							symbol = String.Concat(baseCurrency, quoteCurrency);
							Trading.Sell(symbol, HitBTC.d_Candle[symbol].Close, Trading.DemoBalance.ElementAt(i).Value.Available);
						}
						else if (HitBTC.d_Candle.ContainsKey(String.Concat(baseCurrency, "ETH")))
						{
							quoteCurrency = "ETH";
							symbol = String.Concat(baseCurrency, quoteCurrency);
							Trading.Sell(symbol, HitBTC.d_Candle[symbol].Close, Trading.DemoBalance.ElementAt(i).Value.Available);
						}
			}

			if (Trading.DemoBalance["BTC"].Available > 0.0m)
				Trading.Sell("BTCUSD", HitBTC.d_Candle["BTCUSD"].Close, Trading.DemoBalance["BTC"].Available);
			if (Trading.DemoBalance["ETH"].Available > 0.0m)
				Trading.Sell("ETHUSD", HitBTC.d_Candle["ETHUSD"].Close, Trading.DemoBalance["ETH"].Available);
		}

		private static void HitBTCSocket_MessageReceived(string s, string symbol)
		{
			if(s == "updateCandles" && symbol != null)
			{
				if (Trading.SmaFast[symbol].IsPrimed() && Trading.SmaSlow[symbol].IsPrimed())
				{
					Trading.Run_6(symbol, HitBTC.d_Candle[symbol].Close);
					Screen.Print();
				}
			}
		}

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

		#region Unmanaged

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
