using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;
using HitBTC.Models;
using Trading;

namespace Screen
{
	public class Screen
	{
		HitBTCSocketAPI HitBTC;

		Trading.Trading Trading;

		public Screen(ref HitBTCSocketAPI hitBTC, ref Trading.Trading trading)
		{
			HitBTC = hitBTC;
			Trading = trading;
		}

		public void Print()
		{
			int column_1 = 0;               // Tickers
			int column_2 = 0;              // Pending orders
			int column_3 = column_2 + 78;   // Closed orders
			int column_4 = column_3 + 20;   // 
			int column_5 = column_4 + 0;    // Orders Profit
			int column_6 = column_5 + 0;    // Trad balance / Dealing
			int column_7 = column_6 + 16;   // Estim balance

			Console.CursorVisible = false;
			
			var t = Trading.PendingOrders.SelectMany(kvp => kvp.Value).ToList();
			var tempPendingOrders = t.OrderByDescending(o => o.CurrProfitPercent).ToList();

			/*Console.SetCursorPosition(column_1, 0);
			Console.Write("Tickers      ");
			Console.SetCursorPosition(column_1, 1);
			Console.Write("Sym Ask       Bid");*/

			Console.SetCursorPosition(column_2, 0);
			Console.Write("Pending orders {0}   {1:0000} ", tempPendingOrders.Count, PendingOrder.StaticId);
			Console.SetCursorPosition(column_2, 1);
			Console.Write("Id    Sym  Side  Close      Profit       ProfitSMA   MaxProfit   Type");

			Console.SetCursorPosition(column_3, 0);
			Console.Write("Closed orders {0}", Trading.ClosedOrders.Count);
			Console.SetCursorPosition(column_3, 1);
			Console.Write("Id    Sym  Side  Open      Close     Profit");

			Console.SetCursorPosition(column_2, 23);
			Console.Write("Balance");

			for (int i = 0; i < 20; i++)
			{
				/*if (i < HitBTC.L_Tickers.Count)
				{
					Console.SetCursorPosition(column_1, i + 2); //Price

					int j = HitBTC.L_Tickers.Count - i - 1;
					Console.Write("{0} {1:0000.000}  {2:0000.000}", HitBTC.L_Tickers.ElementAtOrDefault(j).Symbol.Substring(0, 3),
						HitBTC.L_Tickers.ElementAtOrDefault(j).Ask,
						HitBTC.L_Tickers.ElementAtOrDefault(j).Bid);
				}*/

				if (i < tempPendingOrders.Count)
				{
					Console.SetCursorPosition(column_2, i + 2);   //Open order
					Console.Write("{0:0000}  {1}  {2}  {3:0000.000}  {4,11:0.00000000}  {5,11:0.00000000}  {6:0.00000000}  {7} ", 
																				tempPendingOrders.ElementAtOrDefault(i).Id,
																				tempPendingOrders.ElementAtOrDefault(i).Symbol.Substring(0, 3),
																				tempPendingOrders.ElementAtOrDefault(i).Side.PadRight(4),
																				tempPendingOrders.ElementAtOrDefault(i).ClosePrice,
																				tempPendingOrders.ElementAtOrDefault(i).CurrProfitPercent,
																				tempPendingOrders.ElementAtOrDefault(i).CurrProfitPercentSma,
																				tempPendingOrders.ElementAtOrDefault(i).MaxProfitPercentSma,
																				tempPendingOrders.ElementAtOrDefault(i).Type.ToString().PadRight(9));
				}
				else
				{
					Console.SetCursorPosition(column_2, i + 2);
					Console.Write("\t\t\t\t\t\t\t\t\t\t\t");
				}

				if (i < Trading.ClosedOrders.Count)
				{
					int j = Trading.ClosedOrders.Count - i - 1;
					Console.SetCursorPosition(column_3, i + 2);
					Console.Write("{0:0000}  {1}  {2}  {3:0000.000}  {4:0000.000}  {5:0.00000000}%", Trading.ClosedOrders.ElementAtOrDefault(j).Id,
																					Trading.ClosedOrders.ElementAtOrDefault(j).Symbol.Substring(0, 3),
																					Trading.ClosedOrders.ElementAtOrDefault(j).Side.PadRight(4),
																					Trading.ClosedOrders.ElementAtOrDefault(j).OpenPrice,
																					Trading.ClosedOrders.ElementAtOrDefault(j).ClosePrice,
																					Trading.ClosedOrders.ElementAtOrDefault(j).CurrProfitPercent);
				}

				if(i < Trading.DemoBalance.Count)
				{
					Console.SetCursorPosition(column_2, i + 24);
					Console.Write("{0}  {1:00.000000}", Trading.DemoBalance.ElementAtOrDefault(i).Key,
														Trading.DemoBalance.ElementAtOrDefault(i).Value);
				}
			}
		}
	}
}
