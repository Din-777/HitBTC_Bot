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
        static int column_2 = 0;              // Pending orders
        static int column_3 = column_2 + 25;   // Closed orders
        static int column_4 = column_3 + 20;   // 
        static int column_5 = column_4 + 0;    // Orders Profit
        static int column_6 = column_5 + 0;    // Trad balance / Dealing
        static int column_7 = column_6 + 16;   // Estim balance

        public void Print()
        {               
            List<DemoBalance> tempDemoBalances = new List<DemoBalance>();

			if(StaticId != PendingOrder.StaticId)
			{
				Console.SetCursorPosition(column_3, 34);
				Console.Write("Pending orders {0}   {1} ", Trading.PendingOrders.Count, PendingOrder.StaticId);

				StaticId = PendingOrder.StaticId;

				Console.SetCursorPosition(column_3, 35);
				Console.Write("{0:0000.000}  {1}", Trading.PendingOrders["BTCUSD"].ElementAt(0).ClosePrice,
									Trading.PendingOrders["BTCUSD"].ElementAt(0).CurrProfitPercent );
			}

            if (ClosedOrdersCount != Trading.ClosedOrders.Count)
            {
                decimal sumPercent = Trading.ClosedOrders.Sum(x => x.Side == "sell" ? x.CurrProfitPercent : 0m);
                Console.SetCursorPosition(column_3, 0);
                Console.Write("Closed orders {0}    {1}", Trading.ClosedOrders.Count, sumPercent);
                Console.SetCursorPosition(column_3, 1);
                Console.Write("Id      Symbol     Side  Open        Close       Profit %    Created");

                Console.SetCursorPosition(column_2, 0);
                Console.Write("Balance");

                tempDemoBalances = HitBTC.Balance.Select(k => new DemoBalance { Currency = k.Key, Available = k.Value.Available })
                    .OrderByDescending(v => v.Available).ToList();


                Console.SetCursorPosition(column_2, 1);
                var tempDemoBalance = tempDemoBalances.Find(b => b.Currency == "USD");
                Console.Write("{0}  {1}", tempDemoBalance.Currency.PadRight(7).Substring(0, 7),
                                              tempDemoBalance.Available.ToString().PadRight(10, '0').Substring(0, 10));
                Console.SetCursorPosition(column_2, 2);
                tempDemoBalance = tempDemoBalances.Find(b => b.Currency == "BTC");
                Console.Write("{0}  {1}", tempDemoBalance.Currency.PadRight(7).Substring(0, 7),
                                              tempDemoBalance.Available.ToString().PadRight(10, '0').Substring(0, 10));
                Console.SetCursorPosition(column_2, 3);
                tempDemoBalance = tempDemoBalances.Find(b => b.Currency == "ETH");
                Console.Write("{0}  {1}", tempDemoBalance.Currency.PadRight(7).Substring(0, 7),
                                              tempDemoBalance.Available.ToString().PadRight(10, '0').Substring(0, 10));
            }

            for (int i = 0; i < 30; i++)
            {
                if (ClosedOrdersCount != Trading.ClosedOrders.Count)
                {
                    if (i < Trading.ClosedOrders.Count)
                    {
                        int j = Trading.ClosedOrders.Count - i - 1;
                        Console.SetCursorPosition(column_3, i + 2);

						Console.Write("{0:00000}   {1}  {2}  {3}  {4}  {5:0.000000}  {6:HH:mm:ss}",
												  Trading.ClosedOrders.ElementAtOrDefault(j).Id,
												  Trading.ClosedOrders.ElementAtOrDefault(j).Symbol.PadRight(9),
												  Trading.ClosedOrders.ElementAt(j).Side.PadRight(4),
												  Trading.ClosedOrders.ElementAt(j).OpenPrice.ToString().PadRight(10).Substring(0, 10),
												  Trading.ClosedOrders.ElementAt(j).ClosePrice.ToString().PadRight(10).Substring(0, 10),
												  Trading.ClosedOrders.ElementAt(j).CurrProfitPercent.ToString().PadRight(10).Substring(0, 10),
												  Trading.ClosedOrders.ElementAt(j).DateTime);
                    }                   

                    if (i < tempDemoBalances.Count)
                    {
                        Console.SetCursorPosition(column_2, i + 5);
                        Console.Write("{0}  {1}", tempDemoBalances.ElementAt(i).Currency.PadRight(7).Substring(0, 7),
                                                  tempDemoBalances.ElementAt(i).Available.ToString().PadRight(10, '0').Substring(0, 10));
                    }
                }                
            }
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

		public void PrintHitBTCBalance()
		{
			var tempDemoBalances = HitBTC.Balance.Select(k => new DemoBalance { Currency = k.Key, Available = k.Value.Available })
				   .OrderByDescending(v => v.Available).ToList();


			Console.SetCursorPosition(column_2, 1);
			var tempDemoBalance = tempDemoBalances.Find(b => b.Currency == "USD");
			Console.Write("{0}  {1}", tempDemoBalance.Currency.PadRight(7).Substring(0, 7),
										  tempDemoBalance.Available.ToString().PadRight(10, '0').Substring(0, 10));
			Console.SetCursorPosition(column_2, 2);
			tempDemoBalance = tempDemoBalances.Find(b => b.Currency == "BTC");
			Console.Write("{0}  {1}", tempDemoBalance.Currency.PadRight(7).Substring(0, 7),
										  tempDemoBalance.Available.ToString().PadRight(10, '0').Substring(0, 10));
			Console.SetCursorPosition(column_2, 3);
			tempDemoBalance = tempDemoBalances.Find(b => b.Currency == "ETH");
			Console.Write("{0}  {1}", tempDemoBalance.Currency.PadRight(7).Substring(0, 7),
										  tempDemoBalance.Available.ToString().PadRight(10, '0').Substring(0, 10));

			for (int i = 0; i < 30; i++)
			{
				if (i < tempDemoBalances.Count)
				{
					Console.SetCursorPosition(column_2, i + 5);
					Console.Write("{0}  {1}", tempDemoBalances.ElementAt(i).Currency.PadRight(7).Substring(0, 7),
											  tempDemoBalances.ElementAt(i).Available.ToString().PadRight(10, '0').Substring(0, 10));
				}
			}
		}
	}
}
