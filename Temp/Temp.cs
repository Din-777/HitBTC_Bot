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

namespace Temp
{
	class Temp
	{
		static void Main(string[] args)
		{
			HitBTCSocketAPI HitBTC = new HitBTCSocketAPI();

			HitBTC.SocketMarketData.SubscribeCandles("BTCUSD", Period.M1, 10);
			HitBTC.SocketMarketData.SubscribeCandles("ETHUSD", Period.M1, 10);
			HitBTC.SocketMarketData.SubscribeCandles("LTCUSD", Period.M1, 10);
			HitBTC.SocketMarketData.SubscribeCandles("ETCUSD", Period.M1, 10);

			Console.ReadKey();
		}		
	}
}
