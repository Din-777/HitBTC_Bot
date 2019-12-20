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
		string TradingDataFileName;

		public Screen(ref HitBTCSocketAPI hitBTC, ref Trading.Trading trading, string sfn = "tr.dat")
		{
			HitBTC = hitBTC;
			Trading = trading;
			Console.CursorVisible = false;
			TradingDataFileName = sfn;

			//Console.SetBufferSize(127, 50);
			Console.SetWindowSize(127, 50);
		}

		private int ClosedOrdersCount = 99999999;
		private int StaticId = 99999999;

		static int column_1 = 0;               // Tickers
		static int column_2 = 30;              // Pending orders
		static int column_3 = column_2 + 50;   // Closed orders
		static int column_4 = column_3 + 20;   //

		static int series_2 = 20;

		private void PrintPendingOrders(int column, int row, int count, List<PendingOrder> PendingOrders)
		{
			decimal profit = PendingOrders.Sum(p => p.Side == "sell" ? p.CurrProfitInUSD : 0.0m);

			Console.SetCursorPosition(30, 0);
			Console.Write(profit.ToString().PadRight(10).Substring(0, 10));

			for (int i = 0; i < count; i++)
			{
				if (i < PendingOrders.Count)
				{
					var PendingOrder = PendingOrders[i];

					Console.SetCursorPosition(column_1, i + row);
					Console.Write("{0}  {1} {2} {3}  {4}  {5}", PendingOrder.Id.ToString().PadLeft(4, '0'),
															PendingOrder.Symbol.PadRight(9),
															PendingOrder.Side.PadRight(6),
															PendingOrder.OpenPrice.ToString().PadRight(10).Substring(0, 10),
															PendingOrder.ClosePrice.ToString().PadRight(10).Substring(0, 10),
															PendingOrder.CurrProfitInUSD.ToString().PadRight(10).Substring(0, 10)
															//(Trading.SmaFast[PendingOrder.Symbol].LastAverage - Trading.SmaSlow[PendingOrder.Symbol].LastAverage).
															//	ToString().PadRight(10).Substring(0, 10) 
															);
				}
				else if (i > PendingOrders.Count)
					return;
			}
		}

		private void PrintClosedOrders(int column, int row, int count, List<PendingOrder> ClosedOrders)
		{
			for (int i = 0; i < count; i++)
			{
				if (i < ClosedOrders.Count)
				{
					int j = Trading.ClosedOrders.Count - i - 1;
					var ClosedOrder = ClosedOrders[j];
					Console.SetCursorPosition(column_2, i + row);
					Console.Write("{0}  {1} {2} {3}  {4}  {5}", ClosedOrder.Id.ToString().PadLeft(4, '0'),
															ClosedOrder.Symbol.PadRight(9),
															ClosedOrder.Side.PadRight(6),
															ClosedOrder.OpenPrice.ToString().PadRight(10).Substring(0, 10),
															ClosedOrder.ClosePrice.ToString().PadRight(10).Substring(0, 10),
															//ClosedOrder.CurrProfitPercent.ToString().PadRight(10).Substring(0, 10));
															ClosedOrder.CurrProfitInUSD.ToString().PadRight(10).Substring(0, 10));
				}
				else if (i > ClosedOrders.Count)
					return;
			}
		}

		public void PrintBalance(int column, int row, int count, Dictionary<string, Balance> Balances)
		{
			Balances = Balances.OrderByDescending(pair => pair.Value.Available).ToDictionary(pair => pair.Key, pair => pair.Value);

			Console.SetCursorPosition(column, row);
			Console.Write("{0}  {1}", Balances["USD"].Currency.PadRight(8),
										Balances["USD"].Available.ToString().PadRight(10).Substring(0, 10));

			for (int i = 1; i < count; i++)
			{
				var Balance = Balances.ElementAt(i);
				Console.SetCursorPosition(column, i + row);
				Console.Write("{0}  {1}", Balance.Value.Currency.PadRight(8),
										Balance.Value.Available.ToString().PadRight(10).Substring(0, 10));
				
			}
		}

		public void Print()
		{
			Console.Clear();
			Console.SetCursorPosition(column_1, 0);
			Console.Write("Open orders: {0}", Trading.d_OrdersBuy.Count + Trading.d_OrdersSell.Count);
			Console.SetCursorPosition(column_2, 0);
			Console.Write("State: {0}", HitBTC.socket.State.ToString());
			Console.SetCursorPosition(column_1, 1);
			Console.Write("Buy orders");
			Console.SetCursorPosition(column_1, 2);
			Console.Write("Symbol     Distance %");

			for (int i = 0; i <  Trading.d_OrdersBuy.Count; i++)
			{
				Console.SetCursorPosition(column_1, i + 3);
				Console.Write("{0} {1}", Trading.d_OrdersBuy.ElementAt(i).Value.Symbol.PadRight(10),
										  Trading.d_OrdersBuy.ElementAt(i).Value.Distance.ToString().PadRight(10).Substring(0, 10));
			}


			Console.SetCursorPosition(column_2, 1);
			Console.Write("Sell orders");
			Console.SetCursorPosition(column_2, 2);
			Console.Write("Symbol     Distance %");

			for (int i = 0; i < Trading.d_OrdersSell.Count; i++)
			{
				Console.SetCursorPosition(column_2, i + 3);
				Console.Write("{0} {1}", Trading.d_OrdersSell.ElementAt(i).Value.Symbol.PadRight(10),
										  Trading.d_OrdersSell.ElementAt(i).Value.Distance.ToString().PadRight(10).Substring(0, 10));
			}

			PrintBalance(column: column_1, row: 30, count: 15, Balances: Trading.DemoBalance);
		}

		public async void PrintAsync()
		{
			await Task.Run(() => Print());
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

		public bool MenuRun()
		{
			bool close = false;
			var column = 22;
			Console.SetCursorPosition(column, series_2 + 4);
			Console.WriteLine("MENU:");
			Console.SetCursorPosition(column, series_2 + 5);
			Console.WriteLine("Continue          > 1");
			Console.SetCursorPosition(column, series_2 + 6);
			Console.WriteLine("Subtotal and save > 2");
			Console.SetCursorPosition(column, series_2 + 7);
			Console.WriteLine("Sell all/exit     > 3");
			Console.SetCursorPosition(column, series_2 + 8);
			Console.WriteLine("Save and exit     > 4");
			Console.SetCursorPosition(column, series_2 + 9);
			Console.WriteLine("Add Orders from Candles   > 5");
			Console.SetCursorPosition(column, series_2 + 10);
			Console.WriteLine("Delete Order by ID   > 6");

			Console.CursorVisible = true;
			Console.WriteLine();
			Console.SetCursorPosition(column, series_2 + 11);
			Console.Write("> ");

			string ansver = Console.ReadLine();

			switch (ansver)
			{
				case "1":
					Console.CursorVisible = false;
					Trading.Save(TradingDataFileName);
					break;
				case "2":
					Console.CursorVisible = false;
					Trading.Save(TradingDataFileName);
					Trading.SellAll("BTC");
					PrintBalance(column: column_1, row: series_2 + 5, count: series_2, Trading.DemoBalance);
					Console.ReadLine();
					Trading.Load(TradingDataFileName);
					break;
				case "3":
					Console.CursorVisible = false;
					Trading.SellAll("BTC");
					PrintBalance(column: column_1, row: series_2 + 5, count: series_2, Trading.DemoBalance);
					Trading.Save(TradingDataFileName);
					Console.ReadLine();
					close = true;
					break;
				case "4":
					Console.CursorVisible = false;
					Trading.Save(TradingDataFileName);
					Trading.SellAll("BTC");
					PrintBalance(column: column_1, row: series_2 + 5, count: series_2, Trading.DemoBalance);
					Console.ReadLine();
					close = true;
					break;
				case "5":
					Console.CursorVisible = false;
					Trading.Save(TradingDataFileName);

					OrderParametr op = Trading.OrdersParameters.First().Value;
					foreach (var c in HitBTC.Candles)
					{
						if (!Trading.PendingOrders.ContainsKey(c.Key))
						{

						}
					}

					Console.ReadLine();
					close = false;
					break;
				case "6":
					Console.CursorVisible = true;
					Console.SetCursorPosition(column, series_2 + 12);
					Console.Write("Inder Order id   > ");
					int id = Convert.ToInt32(Console.ReadLine());
					Console.CursorVisible = false;
					Trading.ClosedOrderById(id);
					close = false;
					break;

				default:
					break;
			}

			Console.CursorVisible = false;
			Console.Clear();
			return close;
		}
	}
}
