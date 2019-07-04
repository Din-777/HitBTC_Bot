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
					PrevState = CurrState;
					CurrState = _state;
				}

				return _state;
			}
		}
	}
}
