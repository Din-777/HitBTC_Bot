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
            Console.CursorVisible = false;
        }

        private int ClosedOrdersCount = 99999999;
		private int StaticId = 99999999;

		static int column_1 = 0;               // Tickers
        static int column_2 = 60;              // Pending orders
        static int column_3 = column_2 + 50;   // Closed orders
        static int column_4 = column_3 + 20;   // 
        static int column_5 = column_4 + 0;    // Orders Profit
        static int column_6 = column_5 + 0;    // Trad balance / Dealing
        static int column_7 = column_6 + 16;   // Estim balance

		static int series_2 = 20;

		private void PrintPendingOrders(int column, int row, int count, List<PendingOrder> PendingOrders)
		{
			for (int i = 0; i < count; i++)
			{
				if (i < PendingOrders.Count)
				{
					var PendingOrder = PendingOrders[i];

					Console.SetCursorPosition(column_1, i + row);
					Console.Write("{0}  {1} {2} {3}  {4}  {5}", PendingOrder.Id.ToString().PadLeft(4, '0'),
															PendingOrder.Symbol.PadRight(8),
															PendingOrder.Side.PadRight(6),
															PendingOrder.OpenPrice.ToString().PadRight(10).Substring(0, 10),
															PendingOrder.ClosePrice.ToString().PadRight(10).Substring(0, 10),
															//PendingOrder.CurrProfitPercent.ToString().PadRight(10).Substring(0, 10));
															(Trading.SmaFast[PendingOrder.Symbol].LastAverage - Trading.SmaSlow[PendingOrder.Symbol].LastAverage).
																ToString().PadRight(10).Substring(0, 10) 
															);
				}
				else if (i > PendingOrders.Count)
					return;
			}
		}

		private void PrintClosedorders(int column, int row, int count, List<PendingOrder> ClosedOrders)
		{
			for (int i = 0; i < count; i++)
			{
				if (i < ClosedOrders.Count)
				{
					int j = Trading.ClosedOrders.Count - i - 1;
					var ClosedOrder = ClosedOrders[j];
					Console.SetCursorPosition(column_2, i + row);
					Console.Write("{0}  {1} {2} {3}  {4}  {5}", ClosedOrder.Id.ToString().PadLeft(4, '0'),
															ClosedOrder.Symbol.PadRight(8),
															ClosedOrder.Side.PadRight(6),
															ClosedOrder.OpenPrice.ToString().PadRight(10).Substring(0, 10),
															ClosedOrder.ClosePrice.ToString().PadRight(10).Substring(0, 10),
															//ClosedOrder.CurrProfitPercent.ToString().PadRight(10).Substring(0, 10));
															ClosedOrder.CurrProfitPercent.ToString().PadRight(10).Substring(0, 10));
				}
				else if (i > ClosedOrders.Count)
					return;
			}
		}

		public void PrintBalance(int column, int row, int count, Dictionary<string, Balance> Balances)
		{
			for (int i = 0; i < count; i++)
			{
				if (i < Balances.Count)
				{
					var Balance = Balances.ElementAt(i);
					Console.SetCursorPosition(column_1, i + row);
					Console.Write("{0}  {1}", Balance.Value.Currency.PadRight(6),
												Balance.Value.Available.ToString().PadRight(10).Substring(0, 10));
				}
				else if (i > Balances.Count)
					return;
			}
		}

		public void Print()
        {     
			var t = Trading.PendingOrders.SelectMany(kvp => kvp.Value).ToList();
			var tempPendingOrders = t.OrderByDescending(o => o.CurrProfitPercent).ToList();

			if (StaticId != PendingOrder.StaticId)
			{
				Console.SetCursorPosition(column_1, 0);
				Console.Write("Pending orders {0}  Id {1}", tempPendingOrders.Count, PendingOrder.StaticId);
				Console.SetCursorPosition(column_1, 1);
				Console.Write("Id    Sym      Side   Open        Close       Profit");

				Console.SetCursorPosition(column_2, 0);
				decimal sumPercent = Trading.ClosedOrders.Sum(x => x.Side == "sell" ? x.CurrProfitPercent : 0m);
				Console.Write("Closed orders {0}  {1}", Trading.ClosedOrders.Count , sumPercent);
				Console.SetCursorPosition(column_2, 1);
				Console.Write("Id    Sym      Side   Open        Close       Profit");

				Console.SetCursorPosition(column_1, series_2 + 2);
				Console.Write("Balance");

				PrintClosedorders(column: column_2, row: 2, count: series_2, ClosedOrders: Trading.ClosedOrders);
				PrintBalance(column: column_1, row: series_2 + 3, count: series_2, Trading.DemoBalance);

				StaticId = PendingOrder.StaticId;
			}

			PrintPendingOrders(column: column_1, row: 2, count: series_2, tempPendingOrders);

            ClosedOrdersCount = Trading.ClosedOrders.Count;
        }

        public void PrintBalance()
        {
            Trading.DemoBalance.OrderByDescending(o => o.Value);

            Console.SetCursorPosition(column_2, 1);
            Console.Write("{0}      {1}", "USD", Trading.DemoBalance["USD"].ToString().PadRight(10).Substring(0, 10));

            Console.SetCursorPosition(column_2, 2);
            Console.Write("{0}      {1}", "BTC", Trading.DemoBalance["BTC"].ToString().PadRight(10).Substring(0, 10));

            Console.SetCursorPosition(column_2, 3);
            Console.Write("{0}      {1}", "ETH", Trading.DemoBalance["ETH"].ToString().PadRight(10).Substring(0, 10));

            for (int i = 0; i < 30; i++)
            {
                if (i < Trading.DemoBalance.Count)
                {
                    Console.SetCursorPosition(column_2, i + 5);
                    Console.Write("{0}  {1}", Trading.DemoBalance.ElementAtOrDefault(i).Key.PadRight(7).Substring(0, 7),
                                              Trading.DemoBalance.ElementAtOrDefault(i).Value.ToString().PadRight(10, '0').Substring(0, 10));
                }
            }
        }
    }
}
