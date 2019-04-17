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
			public string Side { get; set; } // "buy" "sel"
			public float OpenPrice { get; set; }
			public float ClosePrice { get; set; }
			public float Amount { get; set; }
			public float Profit { get; set; }

			public float CalcProfit(float price)
			{
				return Amount * (Side == "sel" ? price - OpenPrice : OpenPrice - price);
			}

			private float stopLossPrice;

			private float stopLossPercent;

			public float StopLossPrice { get; set; }

			public float StopLossPercent
			{
				get { return stopLossPercent; }
				set
				{
					stopLossPercent = value;
					stopLossPrice = Side == "buy" ? OpenPrice + OpenPrice.Percent(stopLossPercent) : OpenPrice - OpenPrice.Percent(stopLossPercent);
					StopLossPrice = stopLossPrice;
				}
			}

			public Order() { }

			public Order(string tred, float openPrice, float amount, float closePrice, float stopLossPercent = 0.0f)
			{
				this.Side = tred;
				this.ClosePrice = closePrice;
				this.OpenPrice = openPrice;
				this.Amount = amount;
				this.stopLossPercent = stopLossPercent;
				this.stopLossPrice = tred == "buy" ? openPrice + openPrice.Percent(stopLossPercent) :
														openPrice - openPrice.Percent(stopLossPercent);

				this.StopLossPrice = this.stopLossPrice;
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
			public string Side { get; set; } // "buy" "sel"
			public float Price { get; set; }
			public float Amount { get; set; }
			public float Profit { get; set; }

			public Dealing() { }

			public Dealing(string side, float price, float amount = 0.0f, float profit = 0.0f)
			{
				this.Side = side;
				this.Price = price;
				this.Amount = amount;
				this.Profit = profit;
			}

		}

		public class Balance
		{
			public float USD;
			public float BTC;

			public float Loss { get; set; }
			public float Prof { get; set; }

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
					if (order.Side == "sel") order.Profit = (ticker.bid - order.OpenPrice) * order.Amount;
					if (order.Side == "buy") order.Profit = (order.OpenPrice - ticker.ask) * order.Amount;

					tempProf += order.Profit;
				}

				Prof = tempProf;
			}
		}

		public class Trading
		{
			private static Ticker oldTicker;

			public float Fee { get; set; }
			public float StopLossPercent { get; set; }
			public float StopLossPrice { get; set; }

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
						Side = "sel",
						OpenPrice = ticker.ask,
						Amount = TradBTC * 1,
						ClosePrice = ticker.ask + ticker.ask.Percent(Fee),
						StopLossPercent = StopLossPercent
					});

					selBTC(ref balance, TradBTC);
					balance.Orders.Add(new Order
					{
						Side = "buy",
						OpenPrice = ticker.bid,
						Amount = TradBTC * 1,
						ClosePrice = ticker.bid - ticker.bid.Percent(Fee),
						StopLossPercent = StopLossPercent
					});
				}
				else
				{
					for (int i = 0; i < balance.Orders.Count; i++)
					{
						if (balance.Orders[i].Side == "sel")
						{
							if (ticker.bid > balance.Orders[i].ClosePrice)
							{
								if (selBTC(ref balance, balance.Orders[i].Amount, balance.Orders[i].CalcProfit(ticker.bid)))
									balance.Orders.RemoveAt(i);
							}
							else if (ticker.bid < balance.Orders[i].StopLossPrice)
							{
								//selBTC(ref balance, balance.Orders[i].amount);
								balance.Orders.RemoveAt(i);
							}
						}
					}

					for (int i = 0; i < balance.Orders.Count; i++)
					{
						if (balance.Orders[i].Side == "buy")
						{
							if (ticker.ask < balance.Orders[i].ClosePrice)
							{
								if(buyBTC(ref balance, balance.Orders[i].Amount, balance.Orders[i].CalcProfit(ticker.ask)))
									balance.Orders.RemoveAt(i);
							}
							else if (ticker.ask > balance.Orders[i].StopLossPrice)
							{
								//buyBTC(ref balance, balance.Orders[i].amount);
								balance.Orders.RemoveAt(i);
							}
						}
					}
				}

				if (!balance.Orders.Any(t => t.Side == "sel"))
				{
					//selBTC(ref balance, TradBTC);

					balance.Orders.Add(new Order
					{
						Side = "sel",
						OpenPrice = ticker.ask,
						Amount = TradBTC * 1,
						ClosePrice = ticker.ask + ticker.ask.Percent(Fee),
						StopLossPercent = StopLossPercent
					});
				}

				if (!balance.Orders.Any(t => t.Side == "buy"))
				{
					//buyBTC(ref balance, TradBTC);

					balance.Orders.Add(new Order
					{
						Side = "buy",
						OpenPrice = ticker.bid,
						Amount = TradBTC * 1,
						ClosePrice = ticker.bid - ticker.bid.Percent(Fee),
						StopLossPercent = StopLossPercent
					});
				}

				oldTicker = ticker;
			}

			public bool buyBTC(ref Balance balance, float amount, float profit = 0.0f)
			{
				if ( (balance.USD - amount * ticker.ask) > 0.0f)
				{
					balance.BTC += amount;
					balance.USD -= amount * ticker.ask;
					balance.Deals.Push(new Dealing("buy", ticker.ask, amount, profit));
					return true;
				}
				else return false;
				
			}

			public bool selBTC(ref Balance balance, float amount, float profit = 0.0f)
			{
				if ((balance.BTC - amount) > 0.0f)
				{
					balance.BTC -= amount;
					balance.USD += amount * ticker.bid;
					balance.Deals.Push(new Dealing("sel", ticker.bid, amount, profit));
					return true;
				}
				else return false;
			}

		}

		static void printScreen(Balance balance, Stack<float> prices, Ticker ticker)
		{
			int column_1 = 0;               // Prices
			int column_2 = 12;              // Dealings
			int column_3 = column_2 + 14;   // Dealings Profit
			int column_4 = column_3 + 15;   // Open orders
			int column_5 = column_4 + 14;   // Orders Profit
			int column_6 = column_5 + 15;   // Trad balance
			int column_7 = column_6 + 16;   // Estim balance

			Console.CursorVisible = false;

			// Сортировка OpenOrder в порядке удаления от текущей цены
			var tempOrders = (from Order in balance.Orders
							  let l = new
							  {
								  Side = Order.Side,
								  ClosePrice = Order.ClosePrice,
								  Amount = Order.Amount,
								  Diff = Math.Abs(Order.ClosePrice - prices.Peek()),
								  Profit = Order.Profit
							  }
							  orderby l.Diff descending
							  select new Order { Side = l.Side, Profit = l.Profit, ClosePrice = l.ClosePrice }).ToList<Order>();


			Console.Clear();
			Console.Title = string.Format("BTC/USD {0:0000.000}     Initial estimated USD {1:000.000000}",
				prices.Peek(), 100.0f + (0.01f * prices.Last<float>()));


			Console.SetCursorPosition(column_1, 0);
			Console.Write("Prices {0}", prices.Count);

			Console.SetCursorPosition(column_2, 0);
			Console.Write("Dealings {0}", balance.Deals.Count);

			Console.SetCursorPosition(column_3, 0);
			Console.Write("Profit");

			Console.SetCursorPosition(column_4, 0);
			Console.Write("Open order {0}", balance.Orders.Count);

			Console.SetCursorPosition(column_5, 0);
			Console.Write("Profit");

			Console.SetCursorPosition(column_6, 0);
			Console.WriteLine("Trad balance");

			Console.SetCursorPosition(column_7, 0);
			Console.WriteLine("Estim balance");

			Console.SetCursorPosition(column_6, 1);
			Console.WriteLine("USD {0:000.000000}", balance.USD);
			Console.SetCursorPosition(column_6, 2);
			Console.WriteLine("BTC {0:0.00000000}", balance.BTC);

			Console.SetCursorPosition(column_7, 1);
			Console.WriteLine("USD {0:000.000000}", balance.estimatedUSD);
			Console.SetCursorPosition(column_7, 2);
			Console.WriteLine("BTC {0:0.00000000}", balance.estimatedBTC);

			Console.SetCursorPosition(column_6, 4);
			Console.WriteLine("Prof in orders USD {0:0.00000000}", balance.Prof);

			tempOrders.Reverse();

			for (int i = 0; i < 20; i++)
			{
				Console.SetCursorPosition(column_1, i + 1); //Price
				Console.Write("{0:0000.000}", prices.ElementAtOrDefault<float>(i));

				if (i < balance.Deals.Count)
				{
					Console.SetCursorPosition(column_2, i + 1);   //Dealings
					Console.Write(balance.Deals.ElementAtOrDefault<Dealing>(i).Side + " {0:0000.000}", balance.Deals.ElementAtOrDefault<Dealing>(i).Price);

					Console.SetCursorPosition(column_3, i + 1);   //Profit
					Console.Write("{0:0.00000000}", balance.Deals.ElementAtOrDefault<Dealing>(i).Profit);
				}

				if (i < tempOrders.Count)
				{
					Console.SetCursorPosition(column_4, i + 1);   //Open order
					Console.Write(tempOrders.ElementAtOrDefault<Order>(i).Side + " {0:0000.000}", tempOrders.ElementAtOrDefault<Order>(i).ClosePrice);

					Console.SetCursorPosition(column_5, i + 1);   //Order Profit
					Console.Write("{0:0.00000000}", tempOrders.ElementAtOrDefault<Order>(i).Profit);
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
			float stopLossPercent = 1.0f;

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
