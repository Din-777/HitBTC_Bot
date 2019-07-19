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

			public decimal ClosePrice { get; set; }
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

		public class SafetyOrders
		{
			public Signal Signal = Signal.None;

			private decimal _OpenPrice = 0;
			public decimal OpenPrice
			{
				get => _OpenPrice;
				set
				{
					_OpenPrice = value;
					AverageBuyPrice = value;
					StopLosePrice = value - value.Percent(StopLosePercent);

					for(int i = 0; i < SafetyOrdersCountMax; i ++)
					{
						SafetyOrdersPrice.Add(OpenPrice - (OpenPrice.Percent(SafityOrtersPercent) * (i+1)));
					}
				}
			}
			private decimal _AverageBuyPrice = 0;
			public decimal AverageBuyPrice
			{
				get => _AverageBuyPrice;
				set
				{
					_AverageBuyPrice = value;
					TakeProfitPrice = value + value.Percent(TakeProfitPercent);
				}
			}

			public decimal TakeProfitPrice = 0;
			public decimal TakeProfitPercent = 0;

			public decimal TrailingTakeProfitPrice = 0;
			public decimal TrailingTakeProfitPercent = 0.3m;

			public decimal StopLosePrice = 0;
			public decimal StopLosePercent = 0;

			public int SafetyOrdersCount = 1;
			public int SafetyOrdersCountMax = 0;
			public decimal SafityOrtersPercent = 0;
			public List<decimal> SafetyOrdersPrice;

			public SafetyOrders(decimal openPrice, decimal takeProfitPercent = 1, decimal safatyOrdersPercent = 1, decimal stopLosePercent = 10, int safetyOrdersCountMax = 5)
			{
				SafetyOrdersCountMax = safetyOrdersCountMax;
				StopLosePercent = stopLosePercent;
				TakeProfitPercent = takeProfitPercent;
				SafityOrtersPercent = safatyOrdersPercent;
				SafetyOrdersPrice = new List<decimal>();
				OpenPrice = openPrice;
			}

			public Signal Update(Candle candle = null, decimal price = 0)
			{
				if(candle != null)
					price = candle.Close;

				Signal = Signal.None;

				if (price > TakeProfitPrice)
				{
					//Signal = Signal.Sell;
					TakeProfitPrice = price;
					TrailingTakeProfitPrice = TakeProfitPrice - TakeProfitPrice.Percent(TrailingTakeProfitPercent);
				}
				else if (price < TrailingTakeProfitPrice) Signal = Signal.Sell;
				else if (price < StopLosePrice) Signal = Signal.Sell;
				else if (SafetyOrdersPrice.Count > 0 && price < SafetyOrdersPrice.First())
				{
					SafetyOrdersPrice.RemoveAt(0);
					SafetyOrdersCount++;
					AverageBuyPrice = (AverageBuyPrice + price) / 2.0m;
					Signal = Signal.Buy;
				}

				return Signal;
			}
		}

		public class StopLoss
		{
			Signal Signal = Signal.None;

			private decimal _OpenPrice = 0;
			public decimal OpenPrice
			{
				get => _OpenPrice;
				set
				{
					_OpenPrice = value;
					if (Side == Side.Sell)
						StopLossPrice = _OpenPrice - _OpenPrice.Percent(StopLosePercent);
					else if (Side == Side.Buy)
						StopLossPrice = _OpenPrice + _OpenPrice.Percent(StopLosePercent);
				}
			}

			public Side Side = Side.Sell;
			public decimal StopLossPrice = 0;
			private decimal _StopLossPercent = 0;
			public decimal StopLosePercent
			{
				get { return _StopLossPercent; }
				set
				{
					_StopLossPercent = value;
					StopLossPrice = OpenPrice - OpenPrice.Percent(_StopLossPercent);
				}
			}

			public StopLoss(decimal openPrice, Side side = Side.Sell, decimal stopLossPercent = 10, decimal stopLossPrice = 0)
			{
				Side = side;
				StopLossPrice = stopLossPrice;
				if (StopLossPrice == 0) StopLosePercent = stopLossPercent;
				else _StopLossPercent = stopLossPercent;

				OpenPrice = openPrice;
			}

			public Signal Update(Candle candle = null, decimal price = 0)
			{
				Signal = Signal.None;

				if (candle != null)
					price = candle.Close;

				if (price < StopLossPrice)
					Signal = Signal.Sell;

				return Signal;
			}
		}

		public class TakeProfit
		{
			public Signal Signal = Signal.None;

			private decimal _OpenPrice = 0;
			public decimal OpenPrice
			{
				get => _OpenPrice;
				set => _OpenPrice = value;
			}

			private decimal _TakeProfitPrice;
			public decimal TakeProfitPrice
			{
				get => _TakeProfitPrice;
				set
				{
					_TakeProfitPrice = value;

					if (Trailing == true)
					{
						if (Side == Side.Sell)
							TrailingPrice = _TakeProfitPrice - _TakeProfitPrice.Percent(_TrailingPercent);
						else if (Side == Side.Buy)
							TrailingPrice = _TakeProfitPrice + _TakeProfitPrice.Percent(_TrailingPercent);
					}

				}
			}

			private decimal _TakeProfitPercent = 1;
			public decimal TakeProfitPercent
			{
				get => _TakeProfitPercent;
				set
				{
					_TakeProfitPercent = value;
					if (Side == Side.Sell)
						TakeProfitPrice = _OpenPrice + _OpenPrice.Percent(_TakeProfitPercent);
					else if (Side == Side.Buy)
						TakeProfitPrice = _OpenPrice - _OpenPrice.Percent(_TakeProfitPercent);
				}
			}

			public bool Trailing = false;
			public bool TrailingActive = false;
			public decimal TrailingPrice;

			private decimal _TrailingPercent;
			public decimal TrailingPercent
			{
				get => _TrailingPercent;
				set
				{
					_TrailingPercent = value;
					if (Side == Side.Sell)
						TrailingPrice = TakeProfitPrice - TakeProfitPrice.Percent(_TrailingPercent);
					else if (Side == Side.Buy)
						TrailingPrice = TakeProfitPrice + TakeProfitPrice.Percent(_TrailingPercent);
				}
			}

			public Side Side;

			public TakeProfit(decimal openPrice, Side side = Side.Sell, bool trailing = false, decimal takeProfitPercent = 1, decimal takeProfitPrice = 0, decimal trailingPercent = 1)
			{
				Side = side;
				Trailing = trailing;
				OpenPrice = OpenPrice;

				if (takeProfitPrice == 0)
					TakeProfitPercent = takeProfitPercent;
				else
					TakeProfitPrice = takeProfitPrice;

				if (trailing == true)
					TrailingPercent = trailingPercent;

			}

			public Signal Update(Candle candle = null, decimal price = 0)
			{
				if (candle != null)
					price = candle.Close;

				Signal = Signal.None;

				if (Side == Side.Sell)
				{
					if (Trailing == false)
					{
						if (price > TakeProfitPrice)
							Signal = Signal.Sell;
					}
					else if (Trailing == true)
					{
						if (price > TrailingPrice)
						{
							TrailingActive = true;
							TakeProfitPrice = price;
						}
						else if (TrailingActive == true)
						{
							if (price < TrailingPrice)
								Signal = Signal.Sell;
						}
					}
				}
				else if (Side == Side.Buy)
				{
					if (Trailing == false)
					{
						if (price < TakeProfitPrice)
							Signal = Signal.Buy;
					}
					else if (Trailing == true)
					{
						if (price < TakeProfitPrice)
						{
							TrailingActive = true;
							TakeProfitPrice = price;
						}
						else if (TrailingActive == true)
						{
							if (price > TrailingPrice)
								Signal = Signal.Buy;
						}
					}
				}

				return Signal;
			}
		}
	}

	public static class DecimalExtension
	{
		public static decimal Percent(this decimal number, decimal percent)
		{
			//return ((double) 80         *       25)/100;
			return (number * percent) / 100.0m;
		}
	}
}
