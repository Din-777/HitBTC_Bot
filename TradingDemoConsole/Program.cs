using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using HitBTC;

namespace TradingDemoConsole
{
	class Program
	{
		public class Order
		{
			public string tred { get; set; } // "buy" "sel"
			public float openPrice { get; set; }
			public float closePrice { get; set; }
			public float amount { get; set; }

			private float stopLossPrice;

			private float stopLossPercent;

			public float StopLossPrice { get; set; }

			public float StopLossPercent
			{
				get { return stopLossPercent; }
				set
				{
					stopLossPercent = value;
					stopLossPrice = tred == "buy" ? openPrice + openPrice.Percent(stopLossPercent) : openPrice - openPrice.Percent(stopLossPercent);
				}
			}

			public Order() { }

			public Order(string tred, float openPrice, float amount, float closePrice, float stopLossPercent = 0.0f)
			{
				this.tred = tred;
				this.closePrice = closePrice;
				this.openPrice = openPrice;
				this.amount = amount;
				this.stopLossPercent = stopLossPercent;
				this.stopLossPrice = tred == "buy" ? openPrice + openPrice.Percent(stopLossPercent) : openPrice - openPrice.Percent(stopLossPercent);
			}
		}

		public class Orders : List<Order>
		{
			public float loss { get; set; }
			public float prof { get; set; }

			public Orders() { }

			new public void Add(Order item)
			{
				base.Add(item);
			}

			new public void RemoveAt(int index)
			{
				base.RemoveAt(index);
			}

		}

		public class Dealing
		{
			public string tred { get; set; } // "buy" "sel"
			public float price { get; set; }
			public float amount { get; set; }
			public float profit { get; set; }

			public Dealing() { }

			public Dealing(string tred, float price, float amount = 0.0f, float profit = 0.0f)
			{
				this.tred = tred;
				this.price = price;
				this.amount = amount;
				this.profit = profit;
			}

		}

		public class Balance
		{
			public float USD;
			public float BTC;

			public float loss { get; set; }
			public float prof { get; set; }

			public float estimatedUSD;
			public float estimatedBTC;

			public Stack<Dealing> Deals;
			public Orders Orders;

			public void Update(Ticker ticker)
			{
				estimatedUSD = USD + (BTC * ticker.bid);
				estimatedBTC = BTC + (USD / ticker.ask);

				float tempProf = 0.0f;

				foreach (Order order in Orders)
				{
					if (order.tred == "sel") tempProf += (ticker.bid - order.openPrice) * order.amount;
					if (order.tred == "buy") tempProf += (order.openPrice - ticker.ask) * order.amount;
				}

				prof = tempProf;
			}
		}

		public class Trading
		{
			private static Ticker oldTicker;

			public float Fee { get; set; }
			public float StopLossPercent { get; set; }

			public float TradUSD { get; set; }
			public float TradBTC { get; set; }

			public Balance balance { get; set; }
			public Ticker ticker { get; set; }

			public Trading() { }

			public void trading_3(ref Balance balance, Ticker ticker)
			{
				this.ticker = ticker;

				if (oldTicker == null)
				{
					buyBTC(ref balance, TradBTC);

					balance.Orders.Add(new Order
					{
						tred = "sel",
						openPrice = ticker.ask,
						amount = TradBTC * 1,
						closePrice = ticker.ask + ticker.ask.Percent(Fee),
						StopLossPercent = StopLossPercent
					});

					selBTC(ref balance, TradBTC);
					balance.Orders.Add(new Order
					{
						tred = "buy",
						openPrice = ticker.bid,
						amount = TradBTC * 1,
						closePrice = ticker.bid - ticker.bid.Percent(Fee),
						StopLossPercent = StopLossPercent
					});
				}
				else
				{
					for (int i = 0; i < balance.Orders.Count; i++)
					{
						if (balance.Orders[i].tred == "sel")
						{
							if (ticker.bid > balance.Orders[i].closePrice)
							{
								selBTC(ref balance, balance.Orders[i].amount);
								balance.Orders.RemoveAt(i);
							}
							if (ticker.bid < balance.Orders[i].StopLossPrice)
							{
								//selBTC(ref balance, balance.Orders[i].amount);
								balance.Orders.RemoveAt(i);
							}

						}
						else
						{
							if (ticker.ask < balance.Orders[i].closePrice)
							{
								buyBTC(ref balance, balance.Orders[i].amount);
								balance.Orders.RemoveAt(i);
							}
							if (ticker.ask > balance.Orders[i].StopLossPrice)
							{
								//buyBTC(ref balance, balance.Orders[i].amount);
								balance.Orders.RemoveAt(i);
							}
						}
					}

					if (!balance.Orders.Any(t => t.tred == "sel"))
					{
						//selBTC(ref balance, TradBTC);

						balance.Orders.Add(new Order
						{
							tred = "sel",
							openPrice = ticker.ask,
							amount = TradBTC * 1,
							closePrice = ticker.ask + ticker.ask.Percent(Fee),
							StopLossPercent = StopLossPercent
						});
					}

					if (!balance.Orders.Any(t => t.tred == "buy"))
					{
						//buyBTC(ref balance, TradBTC);

						balance.Orders.Add(new Order
						{
							tred = "buy",
							openPrice = ticker.bid,
							amount = TradBTC * 1,
							closePrice = ticker.bid - ticker.bid.Percent(Fee),
							StopLossPercent = StopLossPercent
						});
					}
				}

				oldTicker = ticker;
			}

			public void buyBTC(ref Balance balance, float amount)
			{
				balance.BTC += amount;
				balance.USD -= amount * ticker.ask;
				balance.Deals.Push(new Dealing("buy", ticker.ask, 0.0f));
			}

			public void selBTC(ref Balance balance, float amount)
			{
				balance.BTC -= amount;
				balance.USD += amount * ticker.bid;
				balance.Deals.Push(new Dealing("sel", ticker.bid, 0.0f));
			}

		}

		static void printScreen(Balance balance, Stack<float> prices, Ticker ticker)
		{
			int column_1 = 0;				// Prices
			int column_2 = 12;				// Dealings
			int column_3 = column_2 + 15;	// Open orders
			int column_4 = column_3 + 14;	// Orders profit
			int column_5 = column_4 + 14;	// Trad balance
			int column_6 = column_5 + 16;	// Estim balance

			Console.CursorVisible = false;

			// Сортировка OpenOrder в порядке удаления от текущей цены
			var tempOrders = (from Order in balance.Orders
							  let l = new
							  {
								  Tred = Order.tred,
								  Price = Order.closePrice,
								  Amount = Order.amount,
								  Diff = Math.Abs(Order.closePrice - prices.Peek()),
								  Profit = Order.tred == "buy" ? (ticker.ask - Order.openPrice) * Order.amount : 
																	(Order.openPrice - ticker.bid) * Order.amount
							  }
							  orderby l.Diff descending
							  select new Dealing(l.Tred, l.Price, l.Amount, l.Profit)).ToList<Dealing>();


			Console.Clear();
			Console.Title = string.Format("BTC/USD {0:0000.000}     Initial estimated USD {1:000.000000}",
				prices.Peek(), 100.0f + (0.01f * prices.Last<float>()));


			Console.SetCursorPosition(column_1, 0);
			Console.Write("Prices {0}", prices.Count);

			Console.SetCursorPosition(column_2, 0);
			Console.Write("Dealings {0}", balance.Deals.Count);

			Console.SetCursorPosition(column_3, 0);
			Console.Write("Open order {0}", balance.Orders.Count);

			Console.SetCursorPosition(column_4, 0);
			Console.Write("Profit");

			Console.SetCursorPosition(column_5, 0);
			Console.WriteLine("Trad balance");

			Console.SetCursorPosition(column_6, 0);
			Console.WriteLine("Estim balance");

			Console.SetCursorPosition(column_5, 1);
			Console.WriteLine("USD {0:000.000000}", balance.USD);
			Console.SetCursorPosition(column_5, 2);
			Console.WriteLine("BTC {0:0.00000000}", balance.BTC);

			Console.SetCursorPosition(column_6, 1);
			Console.WriteLine("USD {0:000.000000}", balance.estimatedUSD);
			Console.SetCursorPosition(column_6, 2);
			Console.WriteLine("BTC {0:0.00000000}", balance.estimatedBTC);

			Console.SetCursorPosition(column_5, 4);
			Console.WriteLine("Prof/Loss in orders USD {0:000.000000}", balance.prof);

			tempOrders.Reverse();

			for (int i = 0; i < 20; i++)
			{
				Console.SetCursorPosition(column_1, i + 1); //Price
				Console.Write("{0:0000.000}", prices.ElementAtOrDefault<float>(i));

				if (i < balance.Deals.Count)
				{
					Console.SetCursorPosition(column_2, i + 1);   //Dealings
					Console.Write(balance.Deals.ElementAtOrDefault<Dealing>(i).tred + " {0:0000.000}", balance.Deals.ElementAtOrDefault<Dealing>(i).price);
				}

				if (i < tempOrders.Count)
				{
					Console.SetCursorPosition(column_3, i + 1);   //Open order
					Console.Write(tempOrders.ElementAtOrDefault<Dealing>(i).tred + " {0:0000.000}", tempOrders.ElementAtOrDefault<Dealing>(i).price);

					Console.SetCursorPosition(column_4, i + 1);   //Order Profit
					Console.Write("{0:0.00000000}", tempOrders.ElementAtOrDefault<Dealing>(i).profit);
				}
			}
		}


		static void Main(string[] args)
		{
			Balance balance = new Balance();
			balance.Deals = new Stack<Dealing>();
			balance.Orders = new Orders();
			Ticker ticker = new Ticker();
			HBTC hitBtc = new HBTC();
			Stack<float> prices = new Stack<float>();
			Trading trading = new Trading();

			balance.USD = 100.0f;
			balance.BTC = 0.01f;

			float fee = 0.001f;
			float stopLossPercent = 0.001f;

			float tradUSD = 1.0f;
			float tradBTC = 0.001f;

			trading.Fee = fee;
			trading.StopLossPercent = stopLossPercent;
			trading.TradUSD = tradUSD;
			trading.TradBTC = tradBTC;

			float price = 0.0f;
			float lastPrice = 0.0f;

			while (true)
			{
				string respons = hitBtc.Request(out ticker, Pair.BTCUSD);

				price = Convert.ToSingle(ticker.last);

				if (price != lastPrice) prices.Push(price);

				trading.trading_3(ref balance, ticker);

				balance.Update(ticker);

				printScreen(balance, prices, ticker);
				Thread.Sleep(500);

				lastPrice = price;
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
