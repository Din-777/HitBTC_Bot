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

		float tradUSD = 1.0f;
		float tradBTC = 0.0001f;

		static void printScreen(Stack<float> stack, Balance balance)
		{
			Console.Clear();
			Console.Title = string.Format("BTC/USD {0:0000.00}", stack.Peek());

			Console.SetCursorPosition(30, 0);
			Console.WriteLine("Balance USD {0:00.000}", balance.estimatedUSD);

			Console.SetCursorPosition(38, 1);
			Console.WriteLine("BTC {0:0.0000}", balance.estimatedBTC);


			Console.SetCursorPosition(55, 0);
			Console.WriteLine("Estimated USD {0:00.000}", balance.estimatedUSD);

			Console.SetCursorPosition(65, 1);
			Console.WriteLine("BTC {0:0.0000}", balance.estimatedBTC);

			for (int i = 0; i < 20; i++)
			{
				Console.SetCursorPosition(0, i);
				Console.Write("{0:00.000}", stack.ElementAtOrDefault<float>(i));
			}

		}

		void trading()
		{

		}

		static void Main(string[] args)
		{
			Balance balance = new Balance();
			Ticker ticker = new Ticker();
			HBTC hitBtc = new HBTC();
			Stack<float> stack = new Stack<float>();

			balance.USD = 100.0f;
			balance.BTC = 0.001f;

			while (true)
			{
				string respons = hitBtc.Request(out ticker, Pair.BTCUSD);
				stack.Push(Convert.ToSingle(ticker.ask));

				printScreen(stack, balance);
				Thread.Sleep(2000);

			}

			Console.ReadKey();
		}
	}
}
