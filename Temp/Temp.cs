using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Threading;

using HitBTC;
using HitBTC.Models;
using Trading.Utilities;
using System.Windows.Forms;

namespace Temp
{
	[Serializable]
	public class EMA
	{
		Queue<decimal> Queue;
		public int Period = 0;
		private decimal Average = 0;

		public EMA(int period)
		{
			Period = period;
			Queue = new Queue<decimal>(0);
		}

		public decimal Compute(decimal value)
		{
			if (Queue.Count >= Period)
			{
				Queue.Dequeue();
			}
			else if (Queue.Count < Period)
				Average = value;

			Average = (Average + value) / 2;

			Queue.Enqueue(Average);
			Average = Queue.Average();

			if (isPrimed())
				return Average;
			else
				return 0;
		}

		public decimal Value
		{
			get
			{
				if (isPrimed())
					return Average;
				else
					return 0;
			}
			
		}

		public bool isPrimed()
		{
			if (Queue.Count >= Period)
				return true;
			else
				return false;
		}
	}

	class Temp
	{
		static HitBTCSocketAPI HitBTC;
		public static bool IsSymbol = false;

		static EMA Ema06;
		static EMA Ema11;

		static decimal Price;

		static void Main(string[] args)
		{
			HitBTC = new HitBTCSocketAPI();
			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
			HitBTC.SocketMarketData.GetSymbols();

			while (!IsSymbol) Thread.Sleep(100);			

			HitBTC.SocketMarketData.SubscribeCandles("BTCUSD", period: Period.M5, limit: 10);
			HitBTC.SocketMarketData.SubscribeTrades("BTCUSD", limit: 1);

			Ema06 = new EMA(06);
			Ema11 = new EMA(11);

			Console.ReadKey();

		}

		static int TradesCount = 0;
		private static void HitBTCSocket_MessageReceived(string s)
		{			
			if (s == "updateTrades")
			{
				if (Price != HitBTC.Trade.Price)
				{
					decimal sma06 = Ema06.Compute(HitBTC.Trade.Price);
					decimal sma11 = Ema11.Compute(HitBTC.Trade.Price);


					Console.WriteLine("{0}   {1} - {2} = {3}",
						HitBTC.Trade.Price.ToString().PadRight(10).Substring(0, 10),
						sma06.ToString().PadRight(10).Substring(0, 10),
						sma11.ToString().PadRight(10).Substring(0, 10),
					(sma06 - sma11).ToString().PadRight(10).Substring(0, 10));
				}
				Price = HitBTC.Trade.Price;

				TradesCount += 1;
			}
			else if (s == "getSymbol")
			{
				IsSymbol = true;				
			}
		}
	}
}
