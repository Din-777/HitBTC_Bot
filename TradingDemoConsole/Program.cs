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
		public struct Balance
		{
			public float USD;
			public float BTC;

			public float estimatedUSD;
			public float estimatedBTC;
		}

		public class Trading
		{
			private static Ticker oldTicker;

			public float TradUSD { get; set; }
			public float TradBTC { get; set; }


			public void trading(ref Balance balance, Ticker ticker)
			{
				//buy BTC
				balance.BTC += TradBTC;
				balance.USD -= TradBTC * ticker.ask;


				//sel BTC
				balance.BTC -= TradBTC;
				balance.USD += TradBTC * ticker.bid;

				oldTicker = ticker;
			}
		}

		static void printScreen(Balance balance, Stack<float> stack)
		{
			Console.Clear();
			Console.Title = string.Format("BTC/USD {0:0000.000}", stack.Peek());

			Console.SetCursorPosition(30, 0);
			Console.WriteLine("Balance USD {0:000.000}", balance.USD);

			Console.SetCursorPosition(38, 1);
			Console.WriteLine("BTC {0:0.00000}", balance.BTC);


			Console.SetCursorPosition(55, 0);
			Console.WriteLine("Estimated USD {0:000.000000}", balance.estimatedUSD);

			Console.SetCursorPosition(65, 1);
			Console.WriteLine("BTC {0:0.00000000}", balance.estimatedBTC);

			for (int i = 0; i < 20; i++)
			{
				Console.SetCursorPosition(0, i);
				Console.Write("{0:0000.000}", stack.ElementAtOrDefault<float>(i));
			}

		}

		static void calcEstimete(ref Balance b, Ticker t)
		{
			b.estimatedUSD = b.USD + (b.BTC * t.ask);
			b.estimatedBTC = b.BTC + (b.USD / t.bid);
		}

		

		static void Main(string[] args)
		{
			Balance balance = new Balance();
			Ticker ticker = new Ticker();
			HBTC hitBtc = new HBTC();
			Stack<float> stack = new Stack<float>();
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
				stack.Push(Convert.ToSingle(ticker.ask));

				trading.trading(ref balance, ticker);

				calcEstimete(ref balance, ticker);

				printScreen(balance, stack);
				Thread.Sleep(2000);

			}

			Console.ReadKey();
		}
	}
}
