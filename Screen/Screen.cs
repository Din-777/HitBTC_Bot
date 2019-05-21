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

        private int ClosedOrdersCount = 0;

        static int column_1 = 0;               // Tickers
        static int column_2 = 0;              // Pending orders
        static int column_3 = column_2 + 79;   // Closed orders
        static int column_4 = column_3 + 20;   // 
        static int column_5 = column_4 + 0;    // Orders Profit
        static int column_6 = column_5 + 0;    // Trad balance / Dealing
        static int column_7 = column_6 + 16;   // Estim balance

        public void Print()
        {
            Console.CursorVisible = false;

            var t = Trading.PendingOrders.SelectMany(kvp => kvp.Value).ToList();
            var tempPendingOrders = t.OrderByDescending(o => o.CurrProfitPercent).ToList();

            var tempDemoBalance = Trading.DemoBalance.OrderByDescending(o => o.Value);

            Console.SetCursorPosition(column_2, 0);
            Console.Write("Pending orders {0}   {1:0000} ", tempPendingOrders.Count, PendingOrder.StaticId);

            if (ClosedOrdersCount != Trading.ClosedOrders.Count)
            {
                Console.SetCursorPosition(column_2, 1);
                Console.Write("Id     Sym   Side  Close      Profit       ProfitSMA   MaxProfit   Type");

                decimal sumPercent = Trading.ClosedOrders.Sum(x => x.Side == "sell" ? x.CurrProfitPercent : 0m);
                Console.SetCursorPosition(column_3, 0);
                Console.Write("Closed orders {0}    {1}", Trading.ClosedOrders.Count, sumPercent);
                Console.SetCursorPosition(column_3, 1);
                Console.Write("Id     Sym   Side  Open      Close     Profit");

                Console.SetCursorPosition(column_2, 23);
                Console.Write("Balance");
            }

            for (int i = 0; i < 20; i++)
            {
                if (i < tempPendingOrders.Count)
                {
                    Console.SetCursorPosition(column_2, i + 2);   //Open order
                    PendingOrder pendingOrder = tempPendingOrders.ElementAtOrDefault(i);
                    Console.Write("{0:0000}  {1,5}  {2}  {3:0000.000}  {4,11:0.00000000}  {5,11:0.00000000}  {6:0.00000000}  {7} ",
                                                                                pendingOrder.Id,
                                                                                pendingOrder.Symbol.Substring(0, pendingOrder.Symbol.Length - 3),
                                                                                pendingOrder.Side.PadRight(4),
                                                                                pendingOrder.ClosePrice,
                                                                                pendingOrder.CurrProfitPercent,
                                                                                pendingOrder.CurrProfitPercentSma,
                                                                                pendingOrder.MaxProfitPercentSma,
                                                                                pendingOrder.Type.ToString().PadRight(9));
                }
                else
                {
                    Console.SetCursorPosition(column_2, i + 2);
                    Console.Write("\t\t\t\t\t\t\t\t\t\t\t");
                }

                if (ClosedOrdersCount != Trading.ClosedOrders.Count)
                {
                    if (i < Trading.ClosedOrders.Count)
                    {
                        int j = Trading.ClosedOrders.Count - i - 1;
                        Console.SetCursorPosition(column_3, i + 2);

                        Console.Write("{0:0000}  {1,5}  {2}  {3:0000.000}  {4:0000.000}  {5,10:0.00000000}",
                                                                                        Trading.ClosedOrders.ElementAtOrDefault(j).Id,
                                                                                        Trading.ClosedOrders.ElementAt(j).Symbol.Substring(0, Trading.ClosedOrders.ElementAtOrDefault(j).Symbol.Length - 3),
                                                                                        Trading.ClosedOrders.ElementAt(j).Side.PadRight(4),
                                                                                        Trading.ClosedOrders.ElementAt(j).OpenPrice,
                                                                                        Trading.ClosedOrders.ElementAt(j).ClosePrice,
                                                                                        Trading.ClosedOrders.ElementAt(j).CurrProfitPercent);
                    }

                    if (i < Trading.DemoBalance.Count)
                    {
                        Console.SetCursorPosition(column_2, i + 24);
                        Console.Write("{0,5}  {1:00.000000}", tempDemoBalance.ElementAt(i).Key,
                                                                tempDemoBalance.ElementAt(i).Value);
                    }
                }                
            }
            ClosedOrdersCount = Trading.ClosedOrders.Count;
        }

        public void PrintBalance()
        {
            var tempDemoBalance = Trading.DemoBalance.OrderByDescending(o => o.Value);

            Console.SetCursorPosition(column_2, 23);
            Console.Write("Balance");

            for (int i = 0; i < 20; i++)
            {
                if (i < Trading.DemoBalance.Count)
                {
                    Console.SetCursorPosition(column_2, i + 24);
                    Console.Write("{0,5}  {1:00.000000}", tempDemoBalance.ElementAtOrDefault(i).Key,
                                                            tempDemoBalance.ElementAtOrDefault(i).Value);
                }
            }
        }
    }
}
