using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Amazon.DynamoDBv2.Model;

namespace Temp
{
	public class Dealing
	{
		public string tred { get; set; } // "buy" "sel"
		public float price { get; set; }
		public float amount { get; set; }

		public Dealing(string tred, float price)
		{
			this.tred = tred;
			this.price = price;
			this.amount = amount;
		}
	}

	public class Order
	{
		public string tred { get; set; } // "buy" "sel"
		public float openPrice { get; set; }
		public float closePrice { get; set; }
		public float amount { get; set; }

		public Order(string tred, float openPrice, float amount, float closePrice)
		{
			this.tred = tred;
			this.closePrice = closePrice;
			this.openPrice = openPrice;
			this.amount = amount;
		}
	}

	public class Orders : List<Order>
	{
		public float loss { get; set; }
		public float prof { get; set; }


		new public void Add(Order item)
		{
			base.Add(item);
		}

		new public void RemoveAt(int index)
		{
			base.RemoveAt(index);
		}

		private void lossProfUpdate()
		{
			
		}
	}

	public struct Balance
	{
		public float USD;
		public float BTC;

		public float estimatedUSD;
		public float estimatedBTC;

		public Stack<Dealing> Deals;
		public List<Dealing> OpenOrders;
	}


	class Program
	{
		static void Main(string[] args)
		{
			Balance balance = new Balance();
			balance.OpenOrders = new Dealings();

			balance.OpenOrders.Add(new Dealing("buy", 1.0f, 0.0f));
			balance.OpenOrders.Add(new Dealing("buy", 2.0f, 0.0f));
			balance.OpenOrders.Add(new Dealing("buy", 3.0f, 0.0f));
			balance.OpenOrders.Add(new Dealing("bay", 4.0f, 0.0f));
			balance.OpenOrders.RemoveAt(0);


			Console.ReadLine();
		}

		private static void OpenOrdersEvents()
		{
			Console.WriteLine("asdf");
		}
	}

	public static class FloatExtension
	{
		public static float Percent(this float number, float percent)
		{
			//return ((double) 80         *       25)/100;
			return (number * percent) / 100;
		}
	}
}