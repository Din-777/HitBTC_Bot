using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;

namespace Temp2
{
	class Temp2
	{
		static HitBTCSocketAPI hitBTCSocketApi;

		static void Main(string[] args)
		{
			hitBTCSocketApi = new HitBTCSocketAPI();
			hitBTCSocketApi.Opened += HitBTCSocketApi_Opened;

			hitBTCSocketApi.SubscribeTicker("BTCUSD");
			hitBTCSocketApi.MessageReceived += HitBTCSocketApi_MessageReceived;

			Console.ReadKey();
		}

		private static void HitBTCSocketApi_MessageReceived(string s)
		{
			if(hitBTCSocketApi.Ticker != null)
				Console.WriteLine("Ask {0}	Bid {1}", hitBTCSocketApi.Ticker.Ask, hitBTCSocketApi.Ticker.Bid);
		}

		private static void HitBTCSocketApi_Opened(string s)
		{
			Console.WriteLine("SocketApi_Opened");
		}
	}
}
