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
		Dictionary<string, List<PendingOrder>>  PendingOrders;
		List<PendingOrder> ClosedOrders;

		public Screen(ref HitBTCSocketAPI hitBTC, ref Dictionary<string, List<PendingOrder>> pendingOrders, ref List<PendingOrder> closedOrders)
		{
			HitBTC = hitBTC;
			PendingOrders = pendingOrders;
			ClosedOrders = closedOrders;
		}

		public void Print(HitBTCSocketAPI hitBTC, Dictionary<string, List<PendingOrder>> pendingOrders, List<PendingOrder> closedOrders)
		{
			int column_1 = 0;               // Tickers
			int column_2 = 30;              // Pending orders
			int column_3 = column_2 + 45;   // Closed orders
			int column_4 = column_3 + 20;   // 
			int column_5 = column_4 + 0;   // Orders Profit
			int column_6 = column_5 + 0;   // Trad balance / Dealing
			int column_7 = column_6 + 16;   // Estim balance

			Console.CursorVisible = false;

			

			//var t = pendingOrders.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
			//var tempPendingOrders = t.SelectMany(kvp => kvp.Value).ToList();

			var t = pendingOrders.SelectMany(kvp => kvp.Value).ToList();
			var tempPendingOrders = t.OrderByDescending(o => o.CurrProfitPercent).ToList();

			Console.Clear();

			Console.SetCursorPosition(column_1, 0);
			Console.Write("Tickers");
			Console.SetCursorPosition(column_1, 1);
			Console.Write("Symbol  Ask       Bid");

			Console.SetCursorPosition(column_2, 0);
			Console.Write("Pending orders {0}", tempPendingOrders.Count);
			Console.SetCursorPosition(column_2, 1);
			Console.Write("Symbol  Side	Close     Profit");

			Console.SetCursorPosition(column_3, 0);
			Console.Write("Closed orders {0}", closedOrders.Count);
			Console.SetCursorPosition(column_3, 1);
			Console.Write("Symbol  Open      Close     Profit");

			for (int i = 0; i < 20; i++)
			{
				if (i < hitBTC.L_Tickers.Count)
				{					
					Console.SetCursorPosition(column_1, i + 2); //Price

					int j = hitBTC.L_Tickers.Count - i - 1;
					Console.Write("{0}  {1:0000.000}  {2:0000.000}", hitBTC.L_Tickers.ElementAtOrDefault(j).Symbol, 
						hitBTC.L_Tickers.ElementAtOrDefault(j).Ask, 
						hitBTC.L_Tickers.ElementAtOrDefault(j).Bid);
				}

				if (i < tempPendingOrders.Count)
				{
					Console.SetCursorPosition(column_2, i + 2);   //Open order
					Console.Write("{0}  {1}	{2:0000.000}  {3:0.00000000}%", tempPendingOrders.ElementAtOrDefault(i).Symbol, 
																				tempPendingOrders.ElementAtOrDefault(i).Side, 
																				tempPendingOrders.ElementAtOrDefault(i).ClosePrice,
																				tempPendingOrders.ElementAtOrDefault(i).CurrProfitPercent);
				}

				if (i < closedOrders.Count)
				{
					int j = closedOrders.Count - i - 1;
					Console.SetCursorPosition(column_3, i + 2); //Price
					Console.Write("{0}  {1:0000.000}  {2:0000.000}  {3:0.00000000}%", closedOrders.ElementAtOrDefault(j).Symbol,
																					closedOrders.ElementAtOrDefault(j).OpenPrice,
																					closedOrders.ElementAtOrDefault(j).ClosePrice,
																					closedOrders.ElementAtOrDefault(j).CurrProfitPercent);
				}
			}
		}
	}
}
