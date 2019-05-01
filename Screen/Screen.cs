using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;

namespace Screen
{
	public class Screen
	{
		HitBTCSocketAPI HitBTC;

		public Screen(ref HitBTCSocketAPI hitBTC, ref List<PendingOrder> PendingOrders)
		{
			this.HitBTC = hitBTC;
		}

		public void Print()
		{
			int column_1 = 0;               // 
			int column_2 = 12;              // 
			int column_3 = column_2 + 14;   // Dealings Profit
			int column_4 = column_3 + 15;   // Open orders
			int column_5 = column_4 + 14;   // Orders Profit
			int column_6 = column_5 + 15;   // Trad balance / Dealing
			int column_7 = column_6 + 16;   // Estim balance

			Console.CursorVisible = false;

			// Сортировка OpenOrder в порядке удаления от текущей цены
			var tempOrders = (from Order in balance.Orders
							  let l = new
							  {
								  Side = Order.Side,
								  OpenPrice = Order.OpenPrice,
								  Amount = Order.Amount,
								  StopLossPercent = Order.StopLossPercent,
								  ProfitPercent = Order.ProfitPercent,
								  ClosePrice = Order.ClosePrice,
								  Diff = Math.Abs(Order.ClosePrice - prices.Peek()),
								  CurrProfit = Order.CurrProfit,
								  CurrProfitPercent = Order.CurrProfitPercent
							  }
							  orderby l.Diff descending
							  select new Order
							  {
								  Side = l.Side,
								  OpenPrice = l.OpenPrice,
								  Amount = l.Amount,
								  StopLossPercent = l.StopLossPercent,
								  ProfitPercent = l.ProfitPercent,
								  ClosePrice = l.ClosePrice,
								  CurrProfit = l.CurrProfit,
								  CurrProfitPercent = l.CurrProfitPercent
							  }).ToList<Order>();


			Console.Clear();
			Console.Title = string.Format("BTC/USD {0:0000.000}     Initial estimated USD {1:000.000000}",
				prices.Peek(), 100.0f + (0.01f * prices.Last<float>()));


			Console.SetCursorPosition(column_1, 0);
			Console.Write("Prices {0}", prices.Count);

			Console.SetCursorPosition(column_6, 7);
			Console.Write("Dealings {0}", balance.Deals.Count);

			Console.SetCursorPosition(column_7, 7);
			Console.Write("Profit");

			Console.SetCursorPosition(column_2, 0);
			Console.Write("Open order {0}", balance.Orders.Count);

			Console.SetCursorPosition(column_3, 0);
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


			float profitDeals = 0.0f;
			foreach (Dealing d in balance.Deals)
				profitDeals += d.Profit;
			Console.SetCursorPosition(column_6, 5);
			Console.WriteLine("Prof in dealin USD {0:0.00000000}", profitDeals);


			tempOrders.Reverse();

			for (int i = 0; i < 20; i++)
			{
				Console.SetCursorPosition(column_1, i + 1); //Price
				Console.Write("{0:0000.000}", prices.ElementAtOrDefault<float>(i));

				if (i < balance.Deals.Count)
				{
					Console.SetCursorPosition(column_6, i + 8);   //Dealings
					Console.Write(balance.Deals.ElementAtOrDefault<Dealing>(i).Side + " {0:0000.000}", balance.Deals.ElementAtOrDefault<Dealing>(i).Price);

					Console.SetCursorPosition(column_7, i + 8);   //Profit
					Console.Write("{0:0.00000000}", balance.Deals.ElementAtOrDefault<Dealing>(i).Profit);
				}

				if (i < tempOrders.Count)
				{
					Console.SetCursorPosition(column_2, i + 1);   //Open order
					Console.Write(tempOrders.ElementAtOrDefault<Order>(i).Side + " {0:0000.000}", tempOrders.ElementAtOrDefault<Order>(i).ClosePrice);

					Console.SetCursorPosition(column_3, i + 1);   //Order Profit
					Console.Write("{0:0.00000000}", tempOrders.ElementAtOrDefault<Order>(i).CurrProfit);

					Console.SetCursorPosition(column_3 + 12, i + 1);   //Order Profit Percent
					Console.Write("{0:0.00000000}%", tempOrders.ElementAtOrDefault<Order>(i).CurrProfitPercent);

				}
			}

			if (balance.BTC == BTC)
			{
				if (beep)
				{
					Console.Beep(400, 100);
					beep = false;

					Console.SetCursorPosition(0, 22);
					Console.CursorVisible = true;
					Console.Write("waiting...");
					Console.ReadKey();
					Console.CursorVisible = false;
				}
			}
			else beep = true;
		}
	}
    }
}
