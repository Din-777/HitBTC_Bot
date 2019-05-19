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
			decimal quantityIncrement = 0.002m;
			decimal usd = 1.0m;
			decimal price = 4.0m;

			decimal quantityBuy = (usd / price) - ((usd / price) % quantityIncrement);


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
