﻿
using System.Linq;

using System.Threading;
using HitBTC;
using HitBTC.Models;
using System.IO;
using System;
using System.Collections.Generic;
using CsvHelper;

namespace Temp
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

			Trading.DemoBalance = HitBTC.Balance;
			Trading.DemoBalance["USD"].Available = 100.0m;

			foreach (var symbol in HitBTC.Symbols)
			{
				if(symbol.Value.QuoteCurrency == "USD" || symbol.Value.QuoteCurrency == "USDT")
					HitBTC.SocketMarketData.SubscribeTicker(symbol.Key);
				Thread.Sleep(10);
			}			

			Timer timer = new Timer(new TimerCallback(BotStep), null, 0, 1000*60*5);
			Trading.DateTimeStartCurr = DateTime.Now;

			Console.ReadLine();
		}

		

		private static void HitBTCSocket_MessageReceived(string s, string symbol)
		{
			
		}
	}
}