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

		public class BlaBla
		{
			public decimal profitPercent = 0.0m;
			public decimal stopLossPercent = 0.0m;
			public decimal profit = 0.0m;
		}

		static void Main(string[] args)
		{
			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC);
			Screen = new Screen.Screen(ref HitBTC, ref Trading);			

			HitBTC.SocketMarketData.GetSymbols();
			HitBTC.SocketMarketData.SubscribeCandles("BTCUSD", Period.M1, 100);
			HitBTC.SocketMarketData.SubscribeTicker("BTCUSD");
			//HitBTC.SocketMarketData.SubscribeCandles("ETHUSD", Period.M3, 500);
			//HitBTC.SocketMarketData.SubscribeCandles("LTCUSD", Period.M3, 500);
			//HitBTC.SocketMarketData.SubscribeCandles("ETCUSD", Period.M3, 500);

			Thread.Sleep(5000);

			Trading.DemoBalance.Add("USD", new Balance { Currency = "USD", Available = 10.0m });
			Trading.DemoBalance.Add("BTC", new Balance { Currency = "BTC", Available = 00.0m });
			//Trading.DemoBalance.Add("ETH", 0.00f);
			//Trading.DemoBalance.Add("LTC", 0.00f);
			//Trading.DemoBalance.Add("ETC", 0.00f);

			Trading.Add("BTCUSD", 20.0m, 2.0m, 1.0m, 0.5m);
			//Trading.Add("ETHUSD", 20.0f, 5.0f, 0.5f, 0.5f);
			//Trading.Add("LTCUSD", 20.0f, 5.0f, 0.5f, 0.5f);
			//Trading.Add("ETCUSD", 20.0f, 5.0f, 0.5f, 0.5f);

			var Tickers = (from candles in HitBTC.Candles
						   from candel in candles.Value
						   select new Ticker
						   {
							   Ask = candel.Open,
							   Bid = candel.Open * 0.999m,
							   Symbol = candles.Key
						   }).ToList();


			List<BlaBla> blaBlas = new List<BlaBla>();


			for (decimal _profitPercent = 0.25m; _profitPercent <= 2.0m; _profitPercent += 0.01m)
			{
				for (decimal _stopLossPercent = 0.01m; _stopLossPercent <= 2.0m; _stopLossPercent += 0.01m)
				{
					Trading = new Trading.Trading(ref HitBTC);

					Trading.DemoBalance.Add("USD", new Balance { Currency = "USD", Available = 10.0m });
					Trading.DemoBalance.Add("BTC", new Balance { Currency = "BTC", Available = 00.0m });

					Trading.Add("BTCUSD", 20.0m, 2.0m, _stopLossPercent, _profitPercent);

					foreach (var t in Tickers)
					{
						Trading.Run_3(t);
					}

					for (int i = 0; i < Trading.DemoBalance.Keys.Count; i++)
					{
						if (Trading.DemoBalance.ElementAt(i).Key != "USD")
							if (Trading.DemoBalance.ElementAt(i).Value.Available > 0.0m)
							{
								string symbol = Trading.DemoBalance.ElementAt(i).Key.Insert(3, "USD");
								Trading.Sell(symbol,
									HitBTC.d_Tickers[symbol].Bid,
									Trading.DemoBalance.ElementAt(i).Value.Available);
							}
					}


					blaBlas.Add(new BlaBla
						{ profitPercent = _profitPercent, stopLossPercent = _stopLossPercent, profit = Trading.DemoBalance["USD"].Available - 10.0m });

				}
			}

			var bestBlaBla = blaBlas.Aggregate((i1, i2) => i1.profit > i2.profit ? i1 : i2);

			Console.SetCursorPosition(0,0);
			Console.WriteLine("{0}  {1}  {2}", bestBlaBla.profit, bestBlaBla.profitPercent, bestBlaBla.stopLossPercent);

			for (int i = 0; i < Trading.DemoBalance.Count; i++)
			{
				Console.SetCursorPosition(45, 24 + i);
				Console.WriteLine("{0}  {1:00.000000}", Trading.DemoBalance.ElementAtOrDefault(i).Key, Trading.DemoBalance.ElementAt(i).Value.Available);
			}

			Console.ReadKey();
		}
	}

	public static class DecimalExtension
	{
		public static decimal Percent(this decimal number, decimal percent)
		{
			//return ((double) 80         *       25)/100;
			return (number * percent) / 100.0m;
		}
	}
}
