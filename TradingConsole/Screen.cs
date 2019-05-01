using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;

namespace TradingConsole
{
	public class Screen
	{
		HitBTCSocketAPI HitBTC;
		List<PendingOrder> PendingOrders;

		public Screen(ref HitBTCSocketAPI hitBTC, ref List<PendingOrder> pendingOrders)
		{
			this.HitBTC = hitBTC;
			PendingOrders = pendingOrders;
		}

		public void Print(HitBTCSocketAPI hitBTC, List<PendingOrder> PendingOrders)
		{
			int column_1 = 0;               // Tickers
			int column_2 = 22;              // Pending orders
			int column_3 = column_2 + 20;   // Pending orders Profit
			int column_4 = column_3 + 15;   // 
			int column_5 = column_4 + 14;   // Orders Profit
			int column_6 = column_5 + 15;   // Trad balance / Dealing
			int column_7 = column_6 + 16;   // Estim balance

			Console.CursorVisible = false;

			foreach(var p in PendingOrders)
			{
				p.CalcCurrProfitPercent(hitBTC.Ticker);
			}

			var tempPendingOrders = PendingOrders.OrderByDescending(t => t.CurrProfitPercent).ToList<PendingOrder>();

			// Сортировка OpenOrder в порядке удаления от текущей цены
			/*var tempPendingOrders = (from _PendingOrder in PendingOrders
									 let l = new
									 {
										 _Symbol = _PendingOrder.Symbol,
										 _Side = _PendingOrder.Side,
										 _OpenPrice = _PendingOrder.OpenPrice,
										 _Quantity = _PendingOrder.Quantity,
										 _StopPercent = _PendingOrder.StopPercent,
										 _ClosePercent = _PendingOrder.ClosePercent,
										 _StopPrice = _PendingOrder.StopPrice,
										 _ClosePrice = _PendingOrder.ClosePrice,
										 _CreatedAt = _PendingOrder.CreatedAt,
										 _CurrProfitPercent = _PendingOrder.CurrProfitPercent,
										 _Diff = Math.Abs(_PendingOrder.ClosePrice - hitBTC.Ticker.Last)
									 }
									 orderby l._Diff descending
									 select new PendingOrder
									 {
										 Symbol = l._Symbol,
										 Side = l._Side,
										 OpenPrice = l._OpenPrice,
										 Quantity = l._Quantity,
										 StopPercent = l._StopPercent,
										 ClosePercent = l._ClosePercent,
										 StopPrice = l._StopPrice,
										 ClosePrice = l._ClosePrice,
										 CreatedAt = l._CreatedAt,
										 CurrProfitPercent = l._CurrProfitPercent,
									 }).ToList<PendingOrder>(); */


			Console.Clear();


			Console.SetCursorPosition(column_1, 0);
			Console.Write("Ask       Bid");

			Console.SetCursorPosition(column_6, 7);
			Console.Write("Dealings {0}", 0);

			Console.SetCursorPosition(column_7, 7);
			Console.Write("Profit");

			Console.SetCursorPosition(column_2, 0);
			Console.Write("Pending orders {0}", PendingOrders.Count);

			Console.SetCursorPosition(column_3, 0);
			Console.Write("Profit");

			Console.SetCursorPosition(column_6, 0);
			Console.WriteLine("Trad balance");

			Console.SetCursorPosition(column_7, 0);
			Console.WriteLine("Estim balance");

			Console.SetCursorPosition(column_6, 1);	// Trad balance / Dealing
			Console.WriteLine("USD {0:000.000000}", HitBTC.Balance["USD"].Available);
			Console.SetCursorPosition(column_6, 2);
			Console.WriteLine("BTC {0:0.00000000}", HitBTC.Balance["BTC"].Available);

			Console.SetCursorPosition(column_7, 1);
			Console.WriteLine("USD {0:000.000000}",0);
			Console.SetCursorPosition(column_7, 2);
			Console.WriteLine("BTC {0:0.00000000}", 0);

			Console.SetCursorPosition(column_6, 4);
			Console.WriteLine("Prof in orders USD {0:0.00000000}", 0);

			Console.SetCursorPosition(column_6, 5);
			Console.WriteLine("Prof in dealin USD {0:0.00000000}", 0);


			//tempPendingOrders.Reverse();

			for (int i = 0; i < 20; i++)
			{
				if (i < HitBTC.Tickers["BTCUSD"].Count)
				{
					Console.SetCursorPosition(column_1, i + 1); //Price
					Console.Write("{0:0000.000}  {1:0000.000}", HitBTC.Tickers["BTCUSD"].ElementAtOrDefault(i).Ask, HitBTC.Tickers["BTCUSD"].ElementAtOrDefault(i).Bid);
				}

				if (i < tempPendingOrders.Count)
				{
					Console.SetCursorPosition(column_2, i + 1);   //Open order
					Console.Write(tempPendingOrders.ElementAtOrDefault(i).Side + "\t{0:0000.000}", tempPendingOrders.ElementAtOrDefault(i).ClosePrice);


					Console.SetCursorPosition(column_3, i + 1);   //Order Profit Percent
					Console.Write("{0:0.00000000}%", tempPendingOrders.ElementAtOrDefault(i).CurrProfitPercent);

				}
			}
		}
	}
}
