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

		public interface IStrategy
		{
			Signal Update(Candle candle);
			void Clear();
			int abc { get; set; }
		}

		public List<IStrategy> l_Stratery = new List<IStrategy>();
		public Signal Update(Candle candle)
		{
			Signal signal = Signal.None;
			int isig = 0;

			foreach(var s in l_Stratery)
			{
				isig += (int)s.Update(candle);
			}

			if (isig > 0) return signal = Signal.Buy;
			else if (isig < 0) return signal = Signal.Sell;
			else return signal = Signal.None;
		}

		public void SaveCsv(string fileNeme = "strategies.csv")
		{
			using (var writer = new StreamWriter("strategies.csv"))
			using (var csv = new CsvWriter(writer))
			{
				csv.WriteRecords(l_Stratery);
			}
		}

		public class SellAtStopClosePrice : IStrategy
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

		public class SimpleRSI : Signals.SignalRSI, IStrategy
		{
			public int abc { get; set; }
			public Signal Signal = Signal.None;

			public SimpleRSI(int rsiPeriod = 14) : base(rsiPepiod: rsiPeriod)
			{
			}

			public new Signal Update(Candle candle)
			{
				State _state = base.Update(candle: candle);

				if (!base.IsPrimed()) return Signal = Signal.None;

				if (base.PrevState.LO30 && _state.UP30)
					Signal = Signal.Buy;
				else if (base.PrevState.UP70 && _state.UP50)
					Signal = Signal.Sell;
				else Signal = Signal.None;

				return Signal;
			}

			public new void Clear ()
			{

			}
		}

		public class adsf
		{
			public decimal a { get; set; }
		}
	}
}
