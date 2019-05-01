using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Threading;

namespace Temp
{
	
	class Temp
	{
		static void Main(string[] args)
		{
			Dictionary<string, int> dict = new Dictionary<string, int>();

			dict.Add("a", 1);
			dict.Add("b", 2);
			dict.Add("a", 3);


			Console.ReadKey();
		}

		
	}
}
