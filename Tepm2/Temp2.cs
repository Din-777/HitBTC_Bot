using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;

using HitBTC;
using HitBTC.Models;
using Trading;
using Trading.Utilities;
using Screen;
using System.Threading;

namespace Temp2
{
	class Program
	{
		static HitBTCSocketAPI HitBTC;
		static Screen.Screen Screen;

		static string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
		static string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";

		public static Trading.Trading Trading;
		public static string TradingDataFileName = "tr.dat";

		static void Main()
		{
			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC);
			Screen = new Screen.Screen(ref HitBTC, ref Trading);

			HitBTC.SocketAuth.Auth(pKey, sKey);

			HitBTC.SocketMarketData.GetSymbols();
			while (HitBTC.MessageType != "getSymbol") { Thread.Sleep(1); }

			HitBTC.SocketTrading.GetTradingBalance();
			while (HitBTC.MessageType != "balance") { Thread.Sleep(1); }

			Trading.DemoBalance = HitBTC.Balance;
			Trading.DemoBalance["USD"].Available = 100.0m;
			Trading.DemoBalance["BTC"].Available = 100.0m;

			HitBTC.ReceiveMessages(false);

			var symbol = "BTCUSD";
			Trading.Add(symbol: "BTCUSD", period: Period.M15, tradingQuantityInPercent: 10, stopPercent: 10, closePercent: 10);
			Trading.PendingOrderAdd(symbol, "buy", 100);

			Trading._Buy(Trading.PendingOrders["BTCUSD"], price: 100);

			Trading.Run_6(symbol, price: 110);

			for (int i = 50; i < 200; i++)
				Trading.Run_6(symbol, price: 100.0m * Convert.ToDecimal(Math.Sin(i)));

			Trading.Save("1");

			Console.ReadLine();
		}
	}
}