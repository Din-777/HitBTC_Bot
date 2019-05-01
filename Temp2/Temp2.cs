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
		static HitBTCSocketAPI hitBTCSocketApi;

		static string pKey = "h";
		static string sKey = "B";

		//static string pKey = "p";
		//static string sKey = "M";

		static void Main(string[] args)
		{
			hitBTCSocketApi = new HitBTCSocketAPI();
			hitBTCSocketApi.Opened += HitBTCSocketApi_Opened;
			

			hitBTCSocketApi.SocketAuth.Auth(pKey, sKey);
			hitBTCSocketApi.MessageReceived += HitBTCSocketApi_MessageReceived;

			hitBTCSocketApi.SocketTrading.GetTradingBalance();

			Thread.Sleep(2000);

			hitBTCSocketApi.SocketMarketData.SubscribeTicker("BTCUSD");
			hitBTCSocketApi.SocketMarketData.SubscribeTicker("ETHUSD");
			//hitBTCSocketApi.SocketMarketData.UnSubscribeTicker("BTCUSD");

			//hitBTCSocketApi.SocketTrading.PlaceNewOrder("BTCUSD", "sell", 0.00001f);
			//hitBTCSocketApi.SocketTrading.PlaceNewOrder("ETHUSD", "buy", 0.0001f);
			//hitBTCSocketApi.SocketTrading.PlaceNewOrder("ETHUSD", "sell", 0.0001f);

			hitBTCSocketApi.SocketTrading.SubscribeReports();




			Console.ReadKey();
		}

		private static void HitBTCSocketApi_MessageReceived(string s)
		{
			if(s == "ticker")
				//Console.WriteLine("Ask {0}	Bid {1}", hitBTCSocketApi.Ticker.Ask, hitBTCSocketApi.Ticker.Bid);


			if (s == "balance")
			{
				Console.SetCursorPosition(30, 0);
				Console.WriteLine("Balance BTC = {0}", hitBTCSocketApi.Balance["BTC"].Available);
			}

		}

		private static void HitBTCSocketApi_Opened(string s)
		{
			Console.WriteLine("SocketApi_Opened");
		}
	}
}
