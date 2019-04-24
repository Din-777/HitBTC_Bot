using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;
using System.Threading;

namespace Temp2
{
	class Temp2
	{
		static HitBTCSocketAPI hitBTCSocketApi;

		static string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
		static string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";

		static void Main(string[] args)
		{
			hitBTCSocketApi = new HitBTCSocketAPI();
			hitBTCSocketApi.Opened += HitBTCSocketApi_Opened;			

			hitBTCSocketApi.Auth(pKey, sKey);

			hitBTCSocketApi.GetTradingBalance();

			hitBTCSocketApi.SubscribeTicker("BTCUSD");
			hitBTCSocketApi.MessageReceived += HitBTCSocketApi_MessageReceived;

			Thread.Sleep(10000);

			hitBTCSocketApi.UnSubscribeTicker("BTCUSD");

			Console.ReadKey();
		}

		private static void HitBTCSocketApi_MessageReceived(string s)
		{
			if(hitBTCSocketApi.Ticker != null)
				Console.WriteLine("Ask {0}	Bid {1}", hitBTCSocketApi.Ticker.Ask, hitBTCSocketApi.Ticker.Bid);


			if (hitBTCSocketApi.Balance != null)
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
