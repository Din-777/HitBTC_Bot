using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Expressions;
using CsvHelper.Configuration.Attributes;
using HitBTC.Models;

namespace Trading.Utilities
{
	public class Strategies
	{
		public enum Signal
		{
			Buy = 1,
			Sell = -1,
			None = 0
		}

		public SellAtStopClosePrice SellAtLoseClosePrice;
		public SimpleRSI SimpleRsi;
		public BB SimpleBB;
		public int IntSignal;

		public int Update(Candle candle)
		{
			Signal signal = Signal.None;
			int isig = 0;

			if (SellAtLoseClosePrice != null)
				isig += (int)SellAtLoseClosePrice.Update(candle);
			if (SimpleRsi != null)
				isig += (int)SimpleRsi.Update(candle);
			if (SimpleBB != null)
				isig += (int)SimpleBB.Update(candle);

			if (isig > 1) isig = 1;
			else if (isig < -1) isig = -1;
			else isig = 0;

			IntSignal = isig;
			return isig;
		}

		public void Clear()
		{
			if (SellAtLoseClosePrice != null)
				SellAtLoseClosePrice.Clear();
			if (SimpleRsi != null)
				SimpleRsi.Clear();
		}

		public class SellAtStopClosePrice
		{
			public int abc { get; set; }
			private decimal _stopPercent;
			private decimal _closePercent;

			[Name("Identifier")]
			public decimal ClosePrice { get; set; }
			[Name("Iden")]
			public decimal StopPrice { get; set; }
			public decimal OpenPrice { get; set; }
			public Signal Signal = Signal.None;

			public decimal StopPercent
			{
				get { return _stopPercent; }
				set
				{
					_stopPercent = value;
					StopPrice = OpenPrice - OpenPrice.Percent(_stopPercent);
				}
			}

			public decimal ClosePercent
			{
				get { return _closePercent; }
				set
				{
					_closePercent = value;
					ClosePrice = OpenPrice + OpenPrice.Percent(_closePercent);
				}
			}

			public SellAtStopClosePrice(decimal openPrice=0, decimal stopPercent = 0, decimal closePercent = 0, decimal stopPrice = 0, decimal closePrice = 0)
			{
				StopPercent = stopPercent;
				ClosePercent = closePercent;
				if (stopPrice  != 0) StopPrice  = stopPrice;
				if (closePrice != 0) ClosePrice = closePrice;
			}

			public Signal Update(Candle candle)
			{
				if(ClosePrice == 0) return Signal = Signal.None;
				decimal price = candle.Close;
				if (price >= ClosePrice) Signal = Signal.Sell;
				else if (price <= StopPrice) Signal = Signal.Sell;
				else Signal = Signal.None;

				return Signal;
			}

			public void Clear()
			{
				_stopPercent = 0;
				_closePercent = 0;
				ClosePrice = 0;
				StopPrice = 0;
				OpenPrice = 0;
				Signal = Signal.None;
			}
		}

		public class SimpleRSI : Signals.SignalRSI
		{
			public Signal Signal = Signal.None;

			public SimpleRSI(int rsiPeriod = 14) : base(rsiPepiod: rsiPeriod)
			{
			}

			public new Signal Update(Candle candle)
			{
				base.Update(candle: candle);

				if (!base.IsPrimed()) return Signal = Signal.None;

				if (base.PrevState.UP70 && base.CurrState.UP50)
					Signal = Signal.Sell;
				else if (base.PrevState.LO30 && base.CurrState.UP30)
					Signal = Signal.Buy;
				else Signal = Signal.None;

				return Signal;
			}

			public new void Clear ()
			{
				base.Clear();
			}
		}

		public class BB : Signals.SignalBB
		{
			public Signal Signal = Signal.None;

			public BB(int bbPeriod = 14) : base(bbPeriod: bbPeriod)
			{
			}

			public new Signal Update(Candle candle)
			{
				base.Update(candle: candle);

				if (!base.IsPrimed()) return Signal = Signal.None;

				if (base.PrevState.PUpUpL && base.CurrState.PUpMdL)
					Signal = Signal.Sell;
				else if (base.PrevState.PLoLoL&& base.CurrState.PUpLoL)
					Signal = Signal.Buy;
				else Signal = Signal.None;

				return Signal;
			}

			public new void Clear()
			{
				base.Clear();
			}
		}
	}
}
