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


			Console.ReadKey();
		}		
	}
}
