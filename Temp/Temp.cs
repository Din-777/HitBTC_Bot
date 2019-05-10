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
			Dictionary<int, List<int>> keyValuePairs = new Dictionary<int, List<int>>();

			keyValuePairs.Add(0, new List<int>(1));
			keyValuePairs[0].Add(1);
			keyValuePairs[0].Add(1);
			keyValuePairs[0].Add(2);
			keyValuePairs[0].Add(3);
			keyValuePairs[0].Add(1);
			keyValuePairs[0].Add(2);
			keyValuePairs[0].Add(1);

			keyValuePairs.Add(1, new List<int>(1));
			keyValuePairs[1].Add(1);
			keyValuePairs[1].Add(1);
			keyValuePairs[1].Add(2);
			keyValuePairs[1].Add(3);
			keyValuePairs[1].Add(1);			
			keyValuePairs[1].Add(1);
			keyValuePairs[1].Add(2);

			for (int i = 0; i < keyValuePairs.Keys.Count; i++)
			{
				for (int j = 0; j < keyValuePairs[i].Count; j++)
				{
					if(keyValuePairs[i][j]== 1)
					{
						keyValuePairs[i].RemoveAt(j);
						//keyValuePairs[i].Add(1);
						j -= 1;
					}
				}
			}

			string a = "123";
			string b = a.Insert(3, "456");

			Console.ReadKey();
		}

		
	}
}
