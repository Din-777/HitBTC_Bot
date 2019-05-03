using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;
using HitBTC.Models;

namespace TradingConsole
{
	public class Screen
	{
		HitBTCSocketAPI HitBTC;
		List<PendingOrder> PendingOrders;
		List<PendingOrder> ClosedOrders;

		public Screen(ref HitBTCSocketAPI hitBTC, ref List<PendingOrder> pendingOrders, ref List<PendingOrder> closedOrders)
		{
			HitBTC = hitBTC;
			PendingOrders = pendingOrders;
			ClosedOrders = closedOrders;
		}

		public void Print(HitBTCSocketAPI hitBTC, List<Ticker> tickers, Dictionary<string, List<PendingOrder>> pendingOrders, List<PendingOrder> closedOrders)
		{
			int column_1 = 0;               // Tickers
			int column_2 = 30;              // Pending orders
			int column_3 = column_2 + 25;   // Pending orders Profit
			int column_4 = column_3 + 15;   // 
			int column_5 = column_4 + 0;   // Orders Profit
			int column_6 = column_5 + 0;   // Trad balance / Dealing
			int column_7 = column_6 + 16;   // Estim balance

			Console.CursorVisible = false;

			foreach(var pO in pendingOrders)
			{
				if(pO.Symbol == hitBTC.Ticker.Symbol)
					pO.CalcCurrProfitPercent(hitBTC.Ticker);
			}

			var tempPendingOrders = pendingOrders.OrderByDescending(t => t.CurrProfitPercent).ToList<PendingOrder>();
			
			Console.Clear();


			Console.SetCursorPosition(column_1, 0);
			Console.Write("Symbol  Ask       Bid");

			Console.SetCursorPosition(column_6, 0);
			Console.Write("Closed orders {0}", closedOrders.Count);

			Console.SetCursorPosition(column_5, 1);
			Console.Write("Symbol  Open      Close       Profit");

			Console.SetCursorPosition(column_2, 0);
			Console.Write("Pending orders {0}", pendingOrders.Count);

			Console.SetCursorPosition(column_3, 0);
			Console.Write("Profit");

			for (int i = 0; i < 20; i++)
			{
				if (i < tickers.Count)
				{
					int j = tickers.Count - i - 1;
					Console.SetCursorPosition(column_1, i + 1); //Price
					Console.Write("{0}  {1:0000.000}  {2:0000.000}", tickers.ElementAtOrDefault(j).Symbol, tickers.ElementAtOrDefault(j).Ask, tickers.ElementAtOrDefault(j).Bid);
				}

				if (i < tempPendingOrders.Count)
				{
					Console.SetCursorPosition(column_2, i + 1);   //Open order
					Console.Write(tempPendingOrders.ElementAtOrDefault(i).Side + "\t{0:0000.000}", tempPendingOrders.ElementAtOrDefault(i).ClosePrice);

					Console.SetCursorPosition(column_3, i + 1);   //Order Profit Percent
					Console.Write("{0:0.00000000}%", tempPendingOrders.ElementAtOrDefault(i).CurrProfitPercent);
				}

				if (i < closedOrders.Count)
				{
					int j = closedOrders.Count - i - 1;
					Console.SetCursorPosition(column_5, i + 2); //Price
					Console.Write("{0}  {1:0000.000}  {2:0000.000}  {3:0000.000}", closedOrders.ElementAtOrDefault(j).Symbol,
																					closedOrders.ElementAtOrDefault(j).OpenPrice,
																					closedOrders.ElementAtOrDefault(j).ClosePrice,
																					closedOrders.ElementAtOrDefault(j).CurrProfitPercent);
				}
			}
		}
	}
}
