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
	class Temp
	{
		static void Main(string[] args)
		{
			decimal quantityIncrement = 0.1m;
			decimal quantity = 0.09999999999999m;
			decimal usd = 1.0m;
			decimal ask = 0.123456789m;
            decimal bid = 1234.567890m;

			Console.WriteLine("{0}", ask.ToString().Substring(0, 6));
			Console.WriteLine("{0}", bid.ToString().Substring(0, 6));

			quantity -= quantity % quantityIncrement;


			Console.ReadKey();

			decimal quantityBuy = (usd / ask) - ((usd / ask) % quantityIncrement);
            decimal quantitySel = (usd / bid) - ((usd / bid) % quantityIncrement);


            //string symbol = "DATHUSD";
            //string baseCurrency = symbol.Substring(0, symbol.Length - 3);
            //string quoteCurrency = symbol.Substring(symbol.Length - 3, 3);

            string baseCurrency = "BTC";
			string quoteCurrency = "USD";
			string symbol = String.Concat(baseCurrency, quoteCurrency);


			Console.ReadKey();
		}		
	}
}
