using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;

using HitBTC;
using HitBTC.Models;
using Trading;
using Trading.Utilities;
using Screen;
using System.Threading;

namespace Temp2
{
	class Program
	{
		static void Main()
		{
			Queue<int> Queue = new Queue<int>();
			Queue.Enqueue(1); Queue.Enqueue(2); Queue.Enqueue(3); Queue.Enqueue(4);

			double i = 0;

			foreach(var q in Queue)
			{
				i += Math.Sqrt(q);
			}

			Console.ReadLine();
		}
	}
}