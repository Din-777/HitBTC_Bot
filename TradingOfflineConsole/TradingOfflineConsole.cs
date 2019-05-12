using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using HitBTC;
using HitBTC.Models;
using Trading;
using Screen;

namespace TradingOfflineConsole
{
	class TradingOfflineConsole
	{
		static HitBTCSocketAPI HitBTC;
		static Screen.Screen Screen;
		static Trading.Trading Trading;

		static void Main(string[] args)
		{
			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC);
			Screen = new Screen.Screen(ref HitBTC, ref Trading);			

			HitBTC.SocketMarketData.GetSymbols();
			HitBTC.SocketMarketData.SubscribeCandles("BTCUSD", Period.M3, 500);
			//HitBTC.SocketMarketData.SubscribeCandles("ETHUSD", Period.M3, 500);
			//HitBTC.SocketMarketData.SubscribeCandles("LTCUSD", Period.M3, 500);
			//HitBTC.SocketMarketData.SubscribeCandles("ETCUSD", Period.M3, 500);

			Thread.Sleep(5000);

			Trading.DemoBalance.Add("USD", 10.0f);
			Trading.DemoBalance.Add("BTC", 0.00f);
			//Trading.DemoBalance.Add("ETH", 0.00f);
			//Trading.DemoBalance.Add("LTC", 0.00f);
			//Trading.DemoBalance.Add("ETC", 0.00f);

			Trading.Add("BTCUSD", 20.0f, 5.0f, 0.5f, 0.5f);
			//Trading.Add("ETHUSD", 20.0f, 5.0f, 0.5f, 0.5f);
			//Trading.Add("LTCUSD", 20.0f, 5.0f, 0.5f, 0.5f);
			//Trading.Add("ETCUSD", 20.0f, 5.0f, 0.5f, 0.5f);

			var Tickers = (from candles in HitBTC.Candles
						   from candel in candles.Value
						   select new Ticker
						   {
							   Ask = candel.Open,
							   Bid = candel.Open * 0.999f,
							   Symbol = candles.Key
						   }).ToList();

			foreach (var t in Tickers)
			{
				Trading.Run_3(t);
			}

			for (int i = 0; i < Trading.DemoBalance.Keys.Count; i++)
			{
				if (Trading.DemoBalance.ElementAt(i).Key != "USD")
					if (Trading.DemoBalance.ElementAt(i).Value > 0.0f)
					{
						string symbol = Trading.DemoBalance.ElementAt(i).Key.Insert(3, "USD");
						Trading.Sell(symbol,
							HitBTC.d_Tickers[symbol].Bid,
							Trading.DemoBalance.ElementAt(i).Value);
					}
			}

			for (int i = 0; i < Trading.DemoBalance.Count; i++)
			{
				Console.SetCursorPosition(45, 24 + i);
				Console.WriteLine("{0}  {1:00.000000}", Trading.DemoBalance.ElementAtOrDefault(i).Key, Trading.DemoBalance.ElementAt(i).Value);
			}

			Console.ReadKey();
		}
	}

	public static class FloatExtension
	{
		public static float Percent(this float number, float percent)
		{
			//return ((double) 80         *       25)/100;
			return (number * percent) / 100.0f;
		}
	}
}
