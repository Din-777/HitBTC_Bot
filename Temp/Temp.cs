using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Threading;

using HitBTC;
using HitBTC.Models;
using Trading.Utilities;
using System.Windows.Forms;

namespace Temp
{	
	class Temp
	{
		static HitBTCSocketAPI HitBTC;
		public static bool IsSymbol = false;


		static decimal Price;
		static void Main(string[] args)
		{
			HitBTC = new HitBTCSocketAPI();
			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
			HitBTC.SocketMarketData.GetSymbols();

			while (!IsSymbol) Thread.Sleep(100);			

			HitBTC.SocketMarketData.SubscribeCandles("BTCUSD", period: Period.M5, limit: 10);
			HitBTC.SocketMarketData.SubscribeTrades("BTCUSD", limit: 1);

			Console.ReadKey();

		}

		static int TradesCount = 0;
		private static void HitBTCSocket_MessageReceived(string s, string ss)
		{			
			if (s == "updateTrades")
			{
				if (Price != HitBTC.Trade.Price)
				{

				}
				Price = HitBTC.Trade.Price;

				TradesCount += 1;
			}
			else if (s == "getSymbol")
			{
				IsSymbol = true;				
			}
		}
	}
}
