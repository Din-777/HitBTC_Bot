using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;

using System.Threading;
using HitBTC;
using HitBTC.Models;
using Trading;
using Trading.Utilities;
using Screen;

namespace Temp2
{
	class Program
	{
		static void Main()
		{
			Strategies s = new Strategies();
			s.l_Stratery.Add(new Strategies.SellAtStopClosePrice(closePrice: 10));
			s.l_Stratery.Add(new Strategies.SimpleRSI(rsiPeriod: 14));

			Candle candle = new Candle { Close = 11, TimeStamp = DateTime.Now };
			int signal = s.Update(candle);

			Console.ReadLine();
		}
	}
}