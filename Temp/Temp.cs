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

namespace Temp
{
	public class iEMA
	{
		private int tickcount;
		private int periods;
		private decimal dampen;
		private decimal emav;

		public iEMA(int pPeriods)
		{
			periods = pPeriods;
			dampen = 2 / ((decimal)1.0 + periods);
		}

		public void ReceiveTick(decimal Val)
		{
			if (tickcount < periods)
				emav += Val;
			if (tickcount == periods)
				emav /= periods;
			if (tickcount > periods)
				emav = (dampen * (Val - emav)) + emav;

			if (tickcount <= (periods + 1))
			{
				// avoid overflow by stopping use of tickcount
				// when indicator is fully primed
				tickcount++;
			}
		}

		public decimal Value()
		{
			decimal v;

			if (isPrimed())
				v = emav;
			else
				v = 0;

			return v;
		}

		public bool isPrimed()
		{
			bool v = false;
			if (tickcount > periods)
			{
				v = true;
			}
			return v;
		}
	}

	public class SMA
	{
		int _period = 0;
		Queue<decimal> _queue;
		public int Period = 20;

		public SMA(int period)
		{
			_period = period;
			Period = _period;
			_queue = new Queue<decimal>(Period);
		}

		public decimal Compute(decimal x)
		{
			_period = Period;

			if (_queue.Count >= _period)
			{
				_queue.Dequeue();
			}

			_queue.Enqueue(x);
			return _queue.Average();
		}
	}

	class Temp
	{
		static HitBTCSocketAPI HitBTC;
		public static bool IsSymbol = false;

		static SMA Sma06;
		static SMA Sma11;
		static iEMA iEMA06;
		static iEMA iEMA11;


		static decimal Price;

		static void Main(string[] args)
		{
			HitBTC = new HitBTCSocketAPI();

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			HitBTC.SocketMarketData.GetSymbols();

			while (!IsSymbol) Thread.Sleep(100);

			//HitBTC.SocketMarketData.SubscribeTicker("BTCUSD");
			HitBTC.SocketMarketData.SubscribeCandles("BTCUSD", period: Period.M1, limit: 1);
			HitBTC.SocketMarketData.SubscribeTrades("BTCUSD", limit: 1);

			Sma06 = new SMA(10);
			Sma11 = new SMA(30);

			iEMA06 = new iEMA(10);
			iEMA11 = new iEMA(30);

			Console.ReadKey();
		}

		private static void HitBTCSocket_MessageReceived(string s)
		{			
			if (s == "updateTrades")
			{
				if (Price != HitBTC.Trade.Price)
				{
					decimal sma06 = Sma06.Compute(HitBTC.Trade.Price);
					decimal sma11 = Sma11.Compute(HitBTC.Trade.Price);


					iEMA06.ReceiveTick(HitBTC.Trade.Price);
					iEMA11.ReceiveTick(HitBTC.Trade.Price);
					decimal iema06 = iEMA06.Value();
					decimal iema11 = iEMA11.Value();

					Console.WriteLine("{0} - {1} = {2}   {3} - {4} = {5}",
						sma06.ToString().PadRight(10).Substring(0, 10),
						sma11.ToString().PadRight(10).Substring(0, 10),
					(sma06 - sma11).ToString().PadRight(10).Substring(0, 10),
						iema06.ToString().PadRight(10).Substring(0, 10),
						iema11.ToString().PadRight(10).Substring(0, 10),
					(iema06 - iema11).ToString().PadRight(10).Substring(0, 10));
				}
				Price = HitBTC.Trade.Price;
			}
			else if (s == "getSymbol")
			{
				IsSymbol = true;
			}

			
		}
	}
}
