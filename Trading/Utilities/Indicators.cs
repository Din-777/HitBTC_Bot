﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading.Utilities
{
	[Serializable]
	public class Sma
	{
		int _period = 0;
		Queue<decimal> _queue;
		public int Period = 20;

		public Sma()
		{
			_period = Period;
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

	[Serializable]
	public class iEMA
	{
		public decimal LastValue = 0;
		private int tickcount;
		public int Period;
		private decimal dampen;
		private decimal emav;

		public iEMA(int pPeriods)
		{
			Period = pPeriods;
			dampen = 2 / ((decimal)1.0 + Period);
		}

		public decimal ReceiveTick(decimal Val)
		{
			if (LastValue != Val)
			{
				if (tickcount < Period)
					emav += Val;
				if (tickcount == Period)
					emav /= Period;
				if (tickcount > Period)
					emav = (dampen * (Val - emav)) + emav;

				if (tickcount <= (Period + 1))
				{
					// avoid overflow by stopping use of tickcount
					// when indicator is fully primed
					tickcount++;
				}

				LastValue = Val;
			}

			if (isPrimed())
				return emav;
			else
				return 0;
		}

		public decimal Value()
		{
			if (isPrimed())
				return emav;
			else
				return 0;
		}

		public bool isPrimed()
		{
			if (tickcount > Period)
				return true;			
			else 
				return false;
		}
	}

	[Serializable]
	public class iMACD
	{
		int pSlowEMA, pFastEMA, pSignalEMA;
		iEMA slowEMA, fastEMA, signalEMA;

		// restriction: pPFastEMA < pPSlowEMA
		public iMACD(int pPFastEMA, int pPSlowEMA, int pPSignalEMA)
		{
			pFastEMA = pPFastEMA;
			pSlowEMA = pPSlowEMA;
			pSignalEMA = pPSignalEMA;

			slowEMA = new iEMA(pSlowEMA);
			fastEMA = new iEMA(pFastEMA);
			signalEMA = new iEMA(pSignalEMA);
		}

		public void ReceiveTick(decimal Val)
		{
			slowEMA.ReceiveTick(Val);
			fastEMA.ReceiveTick(Val);

			if (slowEMA.isPrimed() && fastEMA.isPrimed())
			{
				signalEMA.ReceiveTick(fastEMA.Value() - slowEMA.Value());
			}
		}

		public void Value(out decimal MACD, out decimal signal, out decimal hist)
		{
			if (signalEMA.isPrimed())
			{
				MACD = fastEMA.Value() - slowEMA.Value();
				signal = signalEMA.Value();
				hist = MACD - signal;
			}
			else
			{
				MACD = 0;
				signal = 0;
				hist = 0;
			}
		}

		public bool isPrimed()
		{
			if (signalEMA.isPrimed())
				return true;
			else
				return false;
		}
	}
}