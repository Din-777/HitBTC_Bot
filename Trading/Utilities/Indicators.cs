using HitBTC.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

		public decimal NextValue(decimal value)
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

		public decimal Value(decimal value)
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
			LastAverage = 0;
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

		public decimal NextValue(decimal value)
		{
			LastAverage = LastAverage == 0 ? value : (value - LastAverage) * Alpha + LastAverage;

			return LastAverage;
		}

		public decimal Value(decimal value)
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
			slowEMA.NextValue(Val);
			fastEMA.NextValue(Val);

			if (slowEMA.IsPrimed() && fastEMA.IsPrimed())
			{
				signalEMA.NextValue( fastEMA.LastAverage - slowEMA.LastAverage );
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

	public class RSI
	{
		public int Period = 14;
		private int Counter = 0;
		public decimal LastRSI = 0;
		public decimal PrevValue = 0;

		public RSI(int period)
		{
			Period = period;
		}

		decimal AverageGain = 0;
		decimal AverageLoss = 0;

		public decimal NextValue(decimal value)
		{
			if (Counter == 0) PrevValue = value;
			decimal diff = value - PrevValue;

			if(Counter < Period)
			{
				if (diff >= 0)
					AverageGain = AverageGain + diff;
				else
					AverageLoss = AverageLoss - diff;
			}
			else if(Counter == Period)
			{
				AverageGain = AverageGain / Period;
				AverageLoss = AverageLoss / Period;
				decimal rs = AverageGain / AverageLoss;

				LastRSI = 100.0m - (100.0m / (1.0m + rs));
			}
			else if(Counter > Period)
			{
				if (diff >= 0)
				{
					AverageGain = ((AverageGain * (Period - 1)) + diff) / Period;
					AverageLoss = (AverageLoss * (Period - 1)) / Period;
				}
				else
				{
					AverageLoss = ((AverageLoss * (Period - 1)) - diff) / Period;
					AverageGain = (AverageGain * (Period - 1)) / Period;
				}

				decimal rs = AverageGain / AverageLoss;

				LastRSI = 100.0m - (100.0m / (1.0m + rs));
			}

			Counter += 1;
			PrevValue = value;
			return LastRSI;
		}

		public decimal Value(decimal value)
		{
			if (Counter == 0) PrevValue = value;
			decimal diff = value - PrevValue;
			decimal averageGain = 0;
			decimal averageLoss = 0;
			decimal rsi = 0;

			if (Counter < Period)
			{
				if (diff >= 0)
					averageGain = AverageGain + diff;
				else
					averageLoss = AverageLoss - diff;
			}
			else if (Counter == Period)
			{
				averageGain = AverageGain / Period;
				averageLoss = AverageLoss / Period;
				decimal rs = averageGain / averageLoss;

				rsi = 100.0m - (100.0m / (1.0m + rs));
			}
			else if (Counter > Period)
			{
				if (diff >= 0)
				{
					averageGain = ((AverageGain * (Period - 1)) + diff) / Period;
					averageLoss = (AverageLoss * (Period - 1)) / Period;
				}
				else
				{
					averageLoss = ((AverageLoss * (Period - 1)) - diff) / Period;
					averageGain = (AverageGain * (Period - 1)) / Period;
				}

				decimal rs = averageGain / averageLoss;

				rsi = 100.0m - (100.0m / (1.0m + rs));
			}

			return rsi;
		}

		public bool IsPrimed()
		{
			if (Counter > Period) return true;
			else return false;
		}

		public void Clear()
		{
			Counter = 0;
			LastRSI = 0;
			PrevValue = 0;
		}
	}

	public class BB
	{
		public int Period;
		public List<decimal> QueueTP;

		private (decimal Sma, decimal BU, decimal BD) LastBB = (0.0m , 0.0m, 0.0m);

		public BB(int period = 20)
		{
			Period = period;
			QueueTP = new List<decimal>();
		}

		public (decimal Sma, decimal BU, decimal BD) NextValue(decimal value)
		{
			if (QueueTP.Count > Period)
				QueueTP.RemoveRange(0, QueueTP.Count - Period);

			//var tp = (candle.Max + candle.Min + candle.Close) / 3.0m;
			var tp = value;
			QueueTP.Add(tp);
			var average = QueueTP.Average();
			var diff = 0.0;

			foreach (var q in QueueTP)
			{
				diff += Math.Pow(Convert.ToDouble(q - average), 2);
			}

			var StdDev = Convert.ToDecimal(Math.Sqrt(diff / (Period - 1.0)));

			LastBB.Sma = average;
			LastBB.BU = average + (2.0m * StdDev);
			LastBB.BD = average - (2.0m * StdDev);
			
			return LastBB;
		}

		public (decimal Sma, decimal BU, decimal BD) Value(decimal value)
		{
			(decimal Sma, decimal BU, decimal BD) bb = (0.0m, 0.0m, 0.0m);

			var lastTP = QueueTP.Last();

			var tp = value;
			QueueTP[QueueTP.Count - 1] = tp;
			var average = QueueTP.Average();
			var diff = 0.0;

			foreach (var q in QueueTP)
			{
				diff += Math.Pow(Convert.ToDouble(q - average), 2);
			}

			var StdDev = Convert.ToDecimal(Math.Sqrt(diff / (Period - 1.0)));

			bb.Sma = average;
			bb.BU = average + (2.0m * StdDev);
			bb.BD = average - (2.0m * StdDev);

			QueueTP[QueueTP.Count - 1] = lastTP;

			return bb;
		}

		public bool IsPrimed()
		{
			if (QueueTP.Count >= Period) return true;
			else return false;
		}

		public void Clear()
		{
			QueueTP.Clear();
			LastBB = (0.0m, 0.0m, 0.0m);
		}
	}

}
