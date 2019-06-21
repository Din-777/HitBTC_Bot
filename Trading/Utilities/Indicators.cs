using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading.Utilities
{
	[Serializable]
	public class SMA
	{
		public List<decimal> Queue;
		public int Period = 0;
		public decimal LastAverage = 0;

		public SMA(int period)
		{
			Period = period;
			Queue = new List<decimal>();
		}

		public decimal NextAverage(decimal value)
		{
			if (Queue.Count >= Period)
				Queue.RemoveRange(0, Queue.Count-Period);

			Queue.Add(value);
			LastAverage = Queue.Average();
			//Queue.RemoveAt(Queue.Count - 1);
			//Queue.Add(LastAverage);
			//LastAverage = Queue.Average();

			return LastAverage;
		}

		public decimal Average(decimal value)
		{
			Queue.Add(value);
			var average = Queue.Average();
			LastAverage = average;
			Queue.RemoveAt(Queue.Count - 1);

			return average;
		}

		public bool IsPrimed()
		{
			if (Queue.Count >= Period)
				return true;
			else
				return false;
		}

		public void Clear()
		{
			Queue.Clear();
		}
	}

	[Serializable]
	public class EMA
	{
		public int Period = 0;
		public decimal LastAverage = 0;
		public decimal Alpha = 0;

		public EMA(int period)
		{
			Period = period;
			Alpha = 2.0m / (period + 1.0m);
		}

		public decimal NextAverage(decimal value)
		{
			LastAverage = LastAverage == 0 ? value : (value - LastAverage) * Alpha + LastAverage;

			return LastAverage;
		}

		public decimal Average(decimal value)
		{
			var average = LastAverage == 0 ? value : (value - LastAverage) * Alpha + LastAverage;

			return average;
		}

		public bool IsPrimed()
		{
			if (LastAverage == 0) return false;
			else if (LastAverage != 0) return true;
			else return false;
		}

		public void Clear()
		{
			LastAverage = 0;
		}
	}

	[Serializable]
	public class iMACD
	{
		int pSlowEMA, pFastEMA, pSignalEMA;
		SMA slowEMA, fastEMA, signalEMA;

		// restriction: pPFastEMA < pPSlowEMA
		public iMACD(int pPFastEMA, int pPSlowEMA, int pPSignalEMA)
		{
			pFastEMA = pPFastEMA;
			pSlowEMA = pPSlowEMA;
			pSignalEMA = pPSignalEMA;

			slowEMA = new SMA(pSlowEMA);
			fastEMA = new SMA(pFastEMA);
			signalEMA = new SMA(pSignalEMA);
		}

		public void ReceiveTick(decimal Val)
		{
			slowEMA.NextAverage(Val);
			fastEMA.NextAverage(Val);

			if (slowEMA.IsPrimed() && fastEMA.IsPrimed())
			{
				signalEMA.NextAverage( fastEMA.LastAverage - slowEMA.LastAverage );
			}
		}

		public void Value(out decimal MACD, out decimal signal, out decimal hist)
		{
			if (signalEMA.IsPrimed())
			{
				MACD = fastEMA.LastAverage - slowEMA.LastAverage;
				signal = signalEMA.LastAverage;
				hist = MACD - signal;
			}
			else
			{
				MACD = 0;
				signal = 0;
				hist = 0;
			}
		}

		public decimal Value()
		{
			if (signalEMA.IsPrimed())
				return signalEMA.LastAverage;
			else
				return 0;
		}

		public bool isPrimed()
		{
			if (signalEMA.IsPrimed())
				return true;
			else
				return false;
		}
	}

	public class Revers
	{
		private bool LastState = false;
		public bool ReversNow = false;

		public bool IsRevers(decimal val)
		{
			if (val > 0 && LastState)
				ReversNow = false;
			else if (val > 0 && !LastState)
				ReversNow = true;
			else if (val < 0 && LastState)
				ReversNow = true;
			else if (val < 0 && !LastState)
				ReversNow = false;
			else if (val == 0)
				return false;

			if (val != 0) LastState = val > 0 ? true : false;
			return ReversNow;
		}
	}

}
