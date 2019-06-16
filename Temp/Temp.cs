using System;

using HitBTC;
using System.Diagnostics;


namespace Temp
{
	class Temp
	{
		static string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
		static string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";
		static Stopwatch sw;

		static HitBTCSocketAPI HitBTC;
		public static bool IsSymbol = false;

		static decimal Price;
		static void Main(string[] args)
		{
			var v = (5, 10);



			Console.ReadKey();
		}
		private static void HitBTCSocket_MessageReceived(string notification, string symbol)
		{			
			if (notification == "balance")
			{
				
			}
		}
	}
}
