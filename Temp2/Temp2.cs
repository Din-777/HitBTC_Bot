using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;

namespace Temp2
{
	class Temp2
	{
		static void Main(string[] args)
		{
			var hitBTCSocketApi = new HitBTCSocketAPI();
			hitBTCSocketApi.Opened += HitBTCSocketApi_Opened;



			Console.ReadKey();
		}

		private static void HitBTCSocketApi_Opened()
		{
			Console.WriteLine("Socket_Opened");
		}
	}
}
