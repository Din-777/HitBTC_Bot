using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using HitBTC;

namespace TradingDemoConsole
{
	class Program
	{
		public class Dealing
		{
			public string tred; // "buy" "sel"
			public float price;

			public Dealing(string tred, float price)
			{
				this.tred = tred;
				this.price = price;
			}

		}

		public struct Balance
		{
			public float USD;
			public float BTC;

			public float estimatedUSD;
			public float estimatedBTC;

			public Stack<Dealing> deals;
		}
				
		public class Trading
		{			
			public List<Dealing> OpenDeals;

			private static Ticker oldTicker;

			public float TradUSD { get; set; }
			public float TradBTC { get; set; }

			public Balance balance { get; set; }
			public Ticker ticker { get; set; }

			public Trading()
			{
				OpenDeals = new List<Dealing>();
			}

			public void trading_1(ref Balance balance, Ticker ticker)
			{
				this.ticker = ticker;

				if (oldTicker != null)
				{
					if(ticker.ask < oldTicker.ask) buyBTC(ref balance);

					if (ticker.bid > oldTicker.bid) selBTC(ref balance);
				}

				oldTicker = ticker;
			}

			private int h = 0;
			public void trading_2(ref Balance balance, Ticker ticker)
			{
				this.ticker = ticker;
				List<Dealing> tempOpenDeals = new List<Dealing>();

				if (oldTicker == null)
				{
					buyBTC(ref balance);
					OpenDeals.Add(new Dealing("buy", ticker.ask));
					balance.deals.Push(new Dealing("buy", ticker.ask));

					selBTC(ref balance);
					OpenDeals.Add(new Dealing("sel", ticker.bid));
					balance.deals.Push(new Dealing("sel", ticker.bid));
					h = 1;
				}
				else
				{	

					var v = from d in OpenDeals
							where d.tred == "buy"
							where d.price > ticker.bid
							select d;


					foreach (Dealing d in OpenDeals)
					{
						if(d.tred == "buy")
						{
							if (ticker.bid > d.price)
							{
								selBTC(ref balance);
								tempOpenDeals.Add(new Dealing("sel", ticker.bid));
								balance.deals.Push(new Dealing("sel", ticker.bid));
							}
						}
						if (d.tred == "sel")
						{
							if (ticker.ask < d.price)
							{
								buyBTC(ref balance);
								tempOpenDeals.Add(new Dealing("buy", ticker.ask));
								balance.deals.Push(new Dealing("buy", ticker.ask));
							}
						}
					}
				}

				OpenDeals = OpenDeals.Concat<Dealing>(tempOpenDeals).ToList<Dealing>();

				oldTicker = ticker;
			}

			public void buyBTC(ref Balance balance)
			{
				balance.BTC += TradBTC;
				balance.USD -= TradBTC * ticker.ask;
			}

			public void selBTC(ref Balance balance)
			{
				balance.BTC -= TradBTC;
				balance.USD += TradBTC * ticker.bid;
			}

			public void calcEstimete(ref Balance balance, Ticker ticker)
			{
				balance.estimatedUSD = balance.USD + (balance.BTC * ticker.bid);
				balance.estimatedBTC = balance.BTC + (balance.USD / ticker.ask);
			}
		}


		static void printScreen(Balance balance, Stack<float> prices)
		{
			Console.Clear();
			Console.Title = string.Format("BTC/USD {0:0000.000}     Initial estimated USD {1:000.000000}",
				prices.Peek(), 100.0f + (0.01f * prices.Last<float>()));

			Console.SetCursorPosition(45, 0);
			Console.WriteLine("Trading balance");
			Console.SetCursorPosition(65, 0);
			Console.WriteLine("Estimated balance");

			Console.SetCursorPosition(45, 1);
			Console.WriteLine("USD {0:000.000000}", balance.USD);
			Console.SetCursorPosition(45, 2);
			Console.WriteLine("BTC {0:0.00000000}", balance.BTC);
			
			Console.SetCursorPosition(65, 1);
			Console.WriteLine("USD {0:000.000000}", balance.estimatedUSD);
			Console.SetCursorPosition(65, 2);
			Console.WriteLine("BTC {0:0.00000000}", balance.estimatedBTC);

			Console.SetCursorPosition(0, 0);
			Console.Write("Prices");

			Console.SetCursorPosition(15, 0);
			Console.Write("Dealings");

			for (int i = 0; i < 20; i++)
			{
				Console.SetCursorPosition(0, i+1);
				Console.Write("{0:0000.000}", prices.ElementAtOrDefault<float>(i));

				if (i < balance.deals.Count)
				{
					Console.SetCursorPosition(15, i+1);
					Console.Write(balance.deals.ElementAtOrDefault<Dealing>(i).tred + " {0:0000.000}", balance.deals.ElementAtOrDefault<Dealing>(i).price);
				}				
			}


		}
				

		static void Main(string[] args)
		{
			Balance balance = new Balance();
			balance.deals = new Stack<Dealing>();
			Ticker ticker = new Ticker();
			HBTC hitBtc = new HBTC();
			Stack<float> prices = new Stack<float>();			
			Trading trading = new Trading();

			balance.USD = 100.0f;
			balance.BTC = 0.01f;

			float tradUSD = 1.0f;
			float tradBTC = 0.0001f;

			trading.TradUSD = tradUSD;
			trading.TradBTC = tradBTC;

			while (true)
			{
				string respons = hitBtc.Request(out ticker, Pair.BTCUSD);
				prices.Push(Convert.ToSingle(ticker.ask));

				trading.trading_2(ref balance, ticker);

				trading.calcEstimete(ref balance, ticker);

				printScreen(balance, prices);
				Thread.Sleep(2000);

			}

			Console.ReadKey();
		}
	}
}
