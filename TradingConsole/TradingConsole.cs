
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
			
			HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
			Trading.DemoBalance = HitBTC.Balance;
			if(Trading.Demo == true)
				Trading.DemoBalance["BTC"].Available = 0.01m;


			for (int i = 0; i < HitBTC.Symbols.Count(); i++)
			{
				string symbol = HitBTC.Symbols.ElementAt(i).Key;
				string baseCurrency = HitBTC.Symbols.ElementAt(i).Value.BaseCurrency;
				string quoteCurrency = HitBTC.Symbols.ElementAt(i).Value.QuoteCurrency;
				if (symbol.EndsWith("BTC") || symbol.EndsWith("BTC"))
				{
					Trading.Add(symbol: symbol, period: Period.M5, tradingQuantityInPercent: 10.0m, stopPercent: 10.0m, closePercent: 1.0m,
							SmaPeriodFast: 20, SmaPeriodSlow: 5);
				}
			}

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
			Trading.Load(TradingDataFileName);

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

		private static void HitBTCSocket_MessageReceived(string s, string symbol)
		{
			if(s == "updateCandles" && symbol != null)
			{
				if (Trading.SmaFast[symbol].IsPrimed() && Trading.SmaSlow[symbol].IsPrimed())
				{
					if (HitBTC.d_Candle[symbol].VolumeQuote > 0.5m)
					{
						Trading.Run_7_RSI(symbol, HitBTC.d_Candle[symbol].Close);
						Screen.Print();
					}
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
