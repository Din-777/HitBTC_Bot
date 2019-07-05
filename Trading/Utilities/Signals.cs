using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HitBTC.Models;

namespace Trading.Utilities
{
	public class Signals
	{
		public class SignalRSI: RSI
		{
			public struct State
			{
				public bool UP70;
				public bool UP50;
				public bool LO50;
				public bool UP30;
				public bool LO30;
			}

			public State CurrState;
			public State PrevState;
			public List<decimal> l_RSI_Value;

			private DateTime DateTime;

			public SignalRSI(int rsiPepiod = 14): base(rsiPepiod)
			{
				l_RSI_Value = new List<decimal>();
			}

			public State Update(Candle candle)
			{
				if (DateTime == null)
				{
					DateTime = candle.TimeStamp;
					l_RSI_Value.Add(0);
				}

				if(candle.TimeStamp > DateTime)
				{
					l_RSI_Value.Add(base.NextValue(candle.Close));
					PrevState = CurrState;
				}
				else
				{
					l_RSI_Value[l_RSI_Value.Count - 1] = base.Value(candle.Close);
				}

				decimal _rsiVal = l_RSI_Value[l_RSI_Value.Count - 1];
				State _state = new State();

				if (_rsiVal >= 70)      _state.UP70 = true;
				else if (_rsiVal >= 50) _state.UP50 = true;
				else if (_rsiVal >= 30) _state.UP30 = true;
				else if (_rsiVal < 30)  _state.LO30 = true;

				if (candle.TimeStamp > DateTime)
				{
					DateTime = candle.TimeStamp;					
				}

				CurrState = _state;
				return _state;
			}
		}

		public class SignalBB : BB
		{
			public struct State
			{
				public bool PUpUpL;
				public bool PUpMdL;
				public bool PLoMdL;
				public bool PUpLoL;
				public bool PLoLoL;
			}

			public State CurrState;
			public State PrevState;
			public List<decimal> l_BB_UpperLine;
			public List<decimal> l_BB_MiddleLine;
			public List<decimal> l_BB_LowerLine;

			private DateTime DateTime;

			public SignalBB(int bbPeriod = 20) : base(bbPeriod)
			{
				l_BB_UpperLine = new List<decimal>();
				l_BB_MiddleLine = new List<decimal>();
				l_BB_LowerLine = new List<decimal>();
			}

			public State Update(Candle candle)
			{
				var price = candle.Close;
				(decimal Upper, decimal Middle, decimal Lower) bbVal;

				if (DateTime == null)
				{
					DateTime = candle.TimeStamp;
					l_BB_UpperLine.Add(0);
					l_BB_MiddleLine.Add(0);
					l_BB_LowerLine.Add(0);
				}

				if (candle.TimeStamp > DateTime)
				{
					bbVal = base.NextValue(candle.Close);
					l_BB_UpperLine.Add(bbVal.Upper);
					l_BB_MiddleLine.Add(bbVal.Middle);
					l_BB_LowerLine.Add(bbVal.Lower);
					PrevState = CurrState;
				}
				else
				{
					bbVal = base.Value(candle.Close);
					var lastNomber = l_BB_UpperLine.Count - 1;
					l_BB_UpperLine[lastNomber] = bbVal.Upper;
					l_BB_MiddleLine[lastNomber] = bbVal.Middle;
					l_BB_LowerLine[lastNomber] = bbVal.Lower;

				}

				State _state = new State();

				if (price > bbVal.Upper) _state.PUpUpL= true;
				else if (price >  bbVal.Middle) _state.PUpMdL = true;
				else if (price >= bbVal.Lower)  _state.PUpLoL = true;
				else if (price <  bbVal.Lower)  _state.PLoLoL = true;

				if (candle.TimeStamp > DateTime)
				{
					DateTime = candle.TimeStamp;
				}

				CurrState = _state;
				return _state;
			}
		}
	}
}
