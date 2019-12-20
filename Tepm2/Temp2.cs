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
using System.Collections.Concurrent;

namespace Temp2
{
	class Program
	{
		static void Main()
		{
            ConcurrentDictionary<int, int> elements = new ConcurrentDictionary<int, int>();
            elements.TryAdd(0,5);
            elements.TryAdd(1,10);
            elements.TryAdd(2,15);
            elements.TryAdd(3,20);

            Thread th1 = new Thread(
                () =>
                {
                    Thread.Sleep(1000);
                    foreach (var item in elements.Values)
                    {
                         Console.WriteLine("Item {0}", item.ToString());
                         Thread.Sleep(1000);
                    }

                    Thread.Sleep(1000);
                    Console.WriteLine();
                    foreach (var item in elements.Values)
                    {
                        Console.WriteLine("Item {0}", item.ToString());
                        Thread.Sleep(1000);
                    }
                }
                );

            Thread th2 = new Thread(
                () =>
                {
                    Thread.Sleep(1200);
                    elements[0] = 30;
                    Console.WriteLine("Изменена запись в коллекции");
                }
                );

            Thread th3 = new Thread(
               () =>
               {
                   Thread.Sleep(1200);
                   elements.TryAdd(4,25);
                   Console.WriteLine("Добавлена запись в коллекцию");
               }
               );

            th2.Start();
            th1.Start();
            th3.Start();

            Console.ReadLine();
		}

        static void Factorial(int x)
        {
            int result = 1;

            for (int i = 1; i <= x; i++)
            {
                result *= i;
            }
            Console.WriteLine($"Выполняется задача {Task.CurrentId}");
            Console.WriteLine($"Факториал числа {x} равен {result}");
            Thread.Sleep(3000);
        }
    }
}