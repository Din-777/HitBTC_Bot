using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using HitBTC;
using HitBTC.Models;
using Trading;

namespace ViewChart
{
	static class Program
	{
		static HitBTCSocketAPI HitBTC;
		static Screen.Screen Screen;

		static string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
		static string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";

		public static Trading.Trading Trading;
		public static string TradingDataFileName = "tr.dat";
		public static string Symbol = "BTCUSD";

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

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
			
			for (int i = 0; i < HitBTC.Symbols.Count(); i++)
			{
				string symbol = HitBTC.Symbols.ElementAt(i).Key;
				string baseCurrency = HitBTC.Symbols.ElementAt(i).Value.BaseCurrency;
				string quoteCurrency = HitBTC.Symbols.ElementAt(i).Value.QuoteCurrency;
				if (symbol.EndsWith("USD") || symbol.EndsWith("USDT"))
				{
					Trading.Add(symbol, period: Period.M5, treadingQuantity: 100.0m, stopPercent: 1.0m, closePercent: 1.0m);
				}
			}

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
			Trading.Load(TradingDataFileName);

			Task task = Task.Run(() => { Application.Run(new Form1(HitBTC, Trading)); });

			bool close = false;
			while (close != true)
			{
				Console.ReadLine();
				HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
				Thread.Sleep(200);
				Screen.MenuRun();
				HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			}




		}

		private static void HitBTCSocket_MessageReceived(string s, string symbol)
		{
			if (s == "updateCandles" && symbol != null)
			{
				if (Trading.SmaFast[symbol].IsPrimed() && Trading.SmaSlow[symbol].IsPrimed())
				{
					Trading.Run_6(symbol, HitBTC.d_Candle[symbol].Close);
					Screen.Print();
				}
			}
		}

		
	}
}
