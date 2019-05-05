using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;
using System.Threading;
using HitBTC.Models;

namespace Temp2
{
	class Temp2
	{
		static HitBTCSocketAPI hitBTC;

		static string pKey = "p";
		static string sKey = "M";

		static void Main(string[] args)
		{
			hitBTC = new HitBTCSocketAPI();
			hitBTC.Opened += HitBTCSocketApi_Opened;
			

			hitBTC.SocketAuth.Auth(pKey, sKey);
			hitBTC.MessageReceived += HitBTCSocketApi_MessageReceived;

			hitBTC.SocketTrading.GetTradingBalance();

			Thread.Sleep(2000);

			//hitBTC.SocketTrading.SubscribeReports();

			//hitBTCSocketApi.SocketMarketData.SubscribeTicker("BTCUSD");
			//hitBTC.SocketMarketData.SubscribeTicker("ETHUSD");
			//hitBTCSocketApi.SocketMarketData.UnSubscribeTicker("BTCUSD");

			//hitBTCSocketApi.SocketTrading.PlaceNewOrder("BTCUSD", "sell", 0.00001f);
			hitBTC.SocketTrading.PlaceNewOrder("ETHUSD", "sell", 0.0001f);
			//hitBTCSocketApi.SocketTrading.PlaceNewOrder("ETHUSD", "sell", 0.0001f);

			


			Console.ReadKey();
		}

		private static void HitBTCSocketApi_MessageReceived(string s)
		{
			if(s == "ticker")
				//Console.WriteLine("Ask {0}	Bid {1}", hitBTCSocketApi.Ticker.Ask, hitBTCSocketApi.Ticker.Bid);


			if (s == "balance")
			{
				Console.SetCursorPosition(30, 0);
				Console.WriteLine("Balance BTC = {0}", hitBTC.Balance["BTC"].Available);
			}

		}

		private static void HitBTCSocketApi_Opened(string s)
		{
			Console.WriteLine("SocketApi_Opened");
		}
	}
}
