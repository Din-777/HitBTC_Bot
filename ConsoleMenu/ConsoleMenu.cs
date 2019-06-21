using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;


namespace ConsoleMenu
{
	public class Menu
	{
		HitBTCSocketAPI HitBTC;
		Trading.Trading Trading;

		public void MenuRun()
		{
			bool close = false;
			while (close != true)
			{
				Console.ReadLine();

				Console.SetCursorPosition(0, 40);
				Console.WriteLine("Continue          > 1");
				Console.WriteLine("Subtotal and save > 2");
				Console.WriteLine("Sell all/exit     > 3");
				Console.WriteLine("Save and exit     > 4");
				Console.WriteLine("Add Orders from Candles   > 5");

				Console.CursorVisible = true;
				Console.WriteLine();
				Console.Write("> ");

				string ansver = Console.ReadLine();

				switch (ansver)
				{
					case "1":
						break;
					case "2":
						Console.CursorVisible = false;
						Trading.Save(TradingDataFileName);
						Trading.SellAll();
						Screen.PrintBalance(column: 20, row: 23, count: 20, Trading.DemoBalance);
						Console.ReadLine();
						Trading.Load(TradingDataFileName);
						break;
					case "3":
						Console.CursorVisible = false;
						Trading.SellAll();
						Screen.PrintBalance(column: 20, row: 23, count: 20, Trading.DemoBalance);
						Trading.Save(TradingDataFileName);
						Console.ReadLine();
						close = true;
						break;
					case "4":
						Console.CursorVisible = false;
						Trading.Save(TradingDataFileName);
						Trading.SellAll();
						Screen.PrintBalance(column: 20, row: 23, count: 20, Trading.DemoBalance);
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


					default:
						break;
				}

				Console.CursorVisible = false;
				Console.Clear();
			}
		}
    }
}
