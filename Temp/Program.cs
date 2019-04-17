using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Amazon.DynamoDBv2.Model;

using HitBTC;

namespace Temp
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

		public void Update(TestTicker ticker)
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
		private static TestTicker oldTicker;

		public float Fee { get; set; }
		public float StopLossPercent { get; set; }

		public float TradUSD { get; set; }
		public float TradBTC { get; set; }

		public Balance balance { get; set; }
		public TestTicker ticker { get; set; }

		public Trading() { }

		public void trading_3(ref Balance balance, TestTicker ticker)
		{
			this.ticker = ticker;

			if (oldTicker != null)
			{
			}
			else
			{
				for (int i = 0; i < balance.Orders.Count; i++)
				{
					if (balance.Orders[i].Side == "sel")
					{
						if (ticker.bid > balance.Orders[i].ClosePrice)
						{
							selBTC(ref balance, balance.Orders[i].Amount, balance.Orders[i].CalcProfit(ticker.bid));
							balance.Orders.RemoveAt(i);
							if (i > 0) i -= 1;
						}
						if ((i < balance.Orders.Count) && (ticker.bid < balance.Orders[i].StopLossPrice))
						{
							//selBTC(ref balance, balance.Orders[i].amount);
							balance.Orders.RemoveAt(i);
							if (i > 0) i -= 1;
						}

					}
				}

				for (int i = 0; i < balance.Orders.Count; i++)
				{
					if (balance.Orders[i].Side == "buy")
					{
						if (ticker.ask < balance.Orders[i].ClosePrice)
						{
							buyBTC(ref balance, balance.Orders[i].Amount, balance.Orders[i].CalcProfit(ticker.ask));
							balance.Orders.RemoveAt(i);
							if (i > 0) i -= 1;
						}
						if (i < balance.Orders.Count && ticker.ask > balance.Orders[i].StopLossPrice)
						{
							//buyBTC(ref balance, balance.Orders[i].amount);
							balance.Orders.RemoveAt(i);
							if (i > 0) i -= 1;
						}
					}
				}
			}

			oldTicker = ticker;
		}

		public void buyBTC(ref Balance balance, float amount, float profit = 0.0f)
		{
			balance.BTC += amount;
			balance.USD -= amount * ticker.ask;
			balance.Deals.Push(new Dealing("buy", ticker.ask, profit));
		}

		public void selBTC(ref Balance balance, float amount, float profit = 0.0f)
		{
			balance.BTC -= amount;
			balance.USD += amount * ticker.bid;
			balance.Deals.Push(new Dealing("sel", ticker.bid, profit));
		}

	}

	public class TestTicker
	{
		public float ask { get; set; }
		public float bid { get; set; }

	}
	
	class Program
	{
		static void Main(string[] args)
		{
			Balance balance = new Balance();
			balance.Orders = new Orders();
			balance.Deals = new Stack<Dealing>();
			Trading trading = new Trading();

			balance.Orders.Add(new Order { Side = "buy", OpenPrice = 10.0f, Amount = 1.0f, ClosePrice = 09.0f, StopLossPrice = 11.0f });
			balance.Orders.Add(new Order { Side = "sel", OpenPrice = 10.0f, Amount = 1.0f, ClosePrice = 11.0f, StopLossPrice = 09.0f });
			//balance.Orders.Add(new Order { Side = "buy", OpenPrice = 0.0f, Amount = 0.0f, ClosePrice = 0.0f });

			balance.Update(new TestTicker { ask = 7, bid = 8 });

			trading.trading_3(ref balance, new TestTicker { ask = 7, bid = 8 });

			Console.ReadLine();
		}
	}

	public static class FloatExtension
	{
		public static float Percent(this float number, float percent)
		{
			//return ((double) 80         *       25)/100;
			return (number * percent) / 100;
		}
	}
}