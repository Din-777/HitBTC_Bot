using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using HitBTC;

namespace TradingOfflineConsole
{
	class Program
	{

		public class Dealing
		{
			public string tred { get; set; } // "buy" "sel"
			public float price { get; set; }
			public float amount { get; set; }

			public Dealing(string tred, float price, float amount)
			{
				this.tred = tred;
				this.price = price;
				this.amount = amount;
			}

		}

		public struct Balance
		{
			public float USD;
			public float BTC;

			public float estimatedUSD;
			public float estimatedBTC;

			public Stack<Dealing> Deals;
			public List<Dealing> OpenOrders;
		}

		public class Trading
		{
			private static Ticker oldTicker;

			public float Fee { get; set; }
			public float TradUSD { get; set; }
			public float TradBTC { get; set; }

			public Balance balance { get; set; }
			public Ticker ticker { get; set; }

			public Trading()
			{

			}

			public void trading_3(ref Balance balance, Ticker ticker)
			{
				this.ticker = ticker;

				if (oldTicker == null)
				{
					buyBTC(ref balance, TradBTC * 6);

					balance.OpenOrders.Add(new Dealing("sel", ticker.bid + ticker.bid.Percent(Fee * 3), TradBTC * 1));
					balance.OpenOrders.Add(new Dealing("sel", ticker.bid + ticker.bid.Percent(Fee * 2), TradBTC * 2));
					balance.OpenOrders.Add(new Dealing("sel", ticker.bid + ticker.bid.Percent(Fee * 1), TradBTC * 3));

					selBTC(ref balance, TradBTC);
					balance.OpenOrders.Add(new Dealing("buy", ticker.ask - ticker.ask.Percent(Fee * 3), TradBTC * 1));
					balance.OpenOrders.Add(new Dealing("buy", ticker.ask - ticker.ask.Percent(Fee * 2), TradBTC * 2));
					balance.OpenOrders.Add(new Dealing("buy", ticker.ask - ticker.ask.Percent(Fee * 1), TradBTC * 3));
				}
				else
				{
					for (int i = 0; i < balance.OpenOrders.Count; i++)
					{
						if (balance.OpenOrders[i].tred == "sel")
						{
							if (ticker.bid >= balance.OpenOrders[i].price)
							{
								selBTC(ref balance, balance.OpenOrders[i].amount);
								balance.OpenOrders.RemoveAt(i);

								//balance.OpenOrders.Add(new Dealing("buy", ticker.ask - ticker.ask.Percent(Fee), TradBTC / 3));
							}
						}
						else
						{
							if (ticker.ask < balance.OpenOrders[i].price)
							{
								buyBTC(ref balance, TradBTC * 6);
								balance.OpenOrders.RemoveAt(i);

								balance.OpenOrders.Add(new Dealing("sel", ticker.bid + ticker.bid.Percent(Fee * 3), TradBTC * 1));
								balance.OpenOrders.Add(new Dealing("sel", ticker.bid + ticker.bid.Percent(Fee * 2), TradBTC * 2));
								balance.OpenOrders.Add(new Dealing("sel", ticker.bid + ticker.bid.Percent(Fee * 1), TradBTC * 3));
							}
						}
					}
				}

				oldTicker = ticker;
			}

			public void buyBTC(ref Balance balance, float sum)
			{
				balance.BTC += sum;
				balance.USD -= sum * ticker.ask;
				balance.Deals.Push(new Dealing("buy", ticker.ask, 0.0f));
			}

			public void selBTC(ref Balance balance, float sum)
			{
				balance.BTC -= sum;
				balance.USD += sum * ticker.bid;
				balance.Deals.Push(new Dealing("sel", ticker.bid, 0.0f));
			}

			public void calcEstimete(ref Balance balance, Ticker ticker)
			{
				balance.estimatedUSD = balance.USD + (balance.BTC * ticker.bid);
				balance.estimatedBTC = balance.BTC + (balance.USD / ticker.ask);
			}
		}


		static void printScreen(Balance balance, Stack<float> prices)
		{
			int column_1 = 0;   // Prices
			int column_2 = 14;  // Dealings
			int column_3 = 30;  // Open orders
			int column_4 = 47;  // Trad balance
			int column_5 = 65;  // Estim balance

			// Сортировка OpenOrder в порядке удаления от текущей цены
			balance.OpenOrders = (from openOperder in balance.OpenOrders
								  let l = new
								  {
									  Tred = openOperder.tred,
									  Price = openOperder.price,
									  Amount = openOperder.amount,
									  Diff = Math.Abs(openOperder.price - prices.Peek())
								  }
								  orderby l.Diff descending
								  select new Dealing(l.Tred, l.Price, l.Amount)).ToList<Dealing>();

			Console.Clear();
			Console.Title = string.Format("BTC/USD {0:0000.000}     Initial estimated USD {1:000.000000}",
				prices.Peek(), 100.0f + (0.01f * prices.Last<float>()));


			Console.SetCursorPosition(column_1, 0);
			Console.Write("Prices {0}", prices.Count);

			Console.SetCursorPosition(column_2, 0);
			Console.Write("Dealings {0}", balance.Deals.Count);

			Console.SetCursorPosition(column_3, 0);
			Console.Write("Open order {0}", balance.OpenOrders.Count);

			Console.SetCursorPosition(column_4, 0);
			Console.WriteLine("Trad balance");

			Console.SetCursorPosition(column_5, 0);
			Console.WriteLine("Estim balance");

			Console.SetCursorPosition(column_4, 1);
			Console.WriteLine("USD {0:000.000000}", balance.USD);
			Console.SetCursorPosition(column_4, 2);
			Console.WriteLine("BTC {0:0.00000000}", balance.BTC);

			Console.SetCursorPosition(column_5, 1);
			Console.WriteLine("USD {0:000.000000}", balance.estimatedUSD);
			Console.SetCursorPosition(column_5, 2);
			Console.WriteLine("BTC {0:0.00000000}", balance.estimatedBTC);

			List<Dealing> tempOpenDeals = new List<Dealing>(balance.OpenOrders);
			tempOpenDeals.Reverse();

			for (int i = 0; i < 20; i++)
			{
				Console.SetCursorPosition(column_1, i + 1); //Price
				Console.Write("{0:0000.000}", prices.ElementAtOrDefault<float>(i));

				if (i < balance.Deals.Count)
				{
					Console.SetCursorPosition(column_2, i + 1);   //Dealings
					Console.Write(balance.Deals.ElementAtOrDefault<Dealing>(i).tred + " {0:0000.000}", balance.Deals.ElementAtOrDefault<Dealing>(i).price);
				}

				if (i < tempOpenDeals.Count)
				{
					Console.SetCursorPosition(column_3, i + 1);   //Open order
					Console.Write(tempOpenDeals.ElementAtOrDefault<Dealing>(i).tred + " {0:0000.000}", tempOpenDeals.ElementAtOrDefault<Dealing>(i).price);
				}
			}
		}


		static void Main(string[] args)
		{
			Balance balance = new Balance();
			balance.Deals = new Stack<Dealing>();
			balance.OpenOrders = new List<Dealing>();
			Ticker ticker = new Ticker();
			HBTC hitBtc = new HBTC();
			Stack<float> prices = new Stack<float>();
			Trading trading = new Trading();

			hitBtc.Request(Pair.BTCUSD, Period.M1, 1000);
			Candle[] candles = hitBtc.candles;

			Ticker[] tickers = (from candel in candles
								select new Ticker
								{
									ask = candel.open,
									bid = candel.open - 0.02f,
									last = 0.0f,
									open = 0.0f,
									low = 0.0f,
									high = 0.0f,
									volume = 0.0f,
									volumeQuote = 0.0f,
									timestamp = new DateTime(),
									symbol = "?"
								}).ToArray<Ticker>();

			balance.USD = 100.0f;
			balance.BTC = 0.01f;

			float fee = 1.0f;
			float tradUSD = 5.0f;
			float tradBTC = 0.001f;

			trading.Fee = fee;
			trading.TradUSD = tradUSD;
			trading.TradBTC = tradBTC;

			foreach (Ticker t in tickers)
			{
				prices.Push(Convert.ToSingle(t.ask));

				trading.trading_3(ref balance, t);

				trading.calcEstimete(ref balance, t);

				printScreen(balance, prices);

				Thread.Sleep(10);
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
