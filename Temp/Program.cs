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

		public Order() { }

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
		
		public Orders(ref int ticker)
		{
			
		}

		new public void Add(Order item)
		{
			base.Add(item);
		}

		new public void RemoveAt(int index)
		{
			base.RemoveAt(index);
		}

		public void lossProfUpdate(int x)
		{
			foreach(Order o in this)
			{
				if (o.tred == "sel")
				{
					prof += (x - o.openPrice) * o.amount;
					
				}
				if (o.tred == "buy")
				{
					prof += (o.openPrice - x) * o.amount; 
				}
			}	
		}
	}

	public class Balance
	{
		public float USD;
		public float BTC;

		public float estimatedUSD;
		public float estimatedBTC;

		public Stack<Dealing> Deals;
		public Orders Orders;

		public void Update(int x)
		{
			Orders.lossProfUpdate(x);
		}
	}


	class Program
	{
		static void Main(string[] args)
		{
			int x = 1;
			Balance balance = new Balance();
			balance.Orders = new Orders(ref x);
			balance.Deals = new Stack<Dealing>();

			balance.Orders.Add(new Order { tred = "buy", openPrice = 0.0f, amount = 0.0f, closePrice = 0.0f });
			balance.Orders.Add(new Order { tred = "buy", openPrice = 0.0f, amount = 0.0f, closePrice = 0.0f });
			balance.Orders.Add(new Order { tred = "buy", openPrice = 0.0f, amount = 0.0f, closePrice = 0.0f });
			balance.Orders.Add(new Order { tred = "buy", openPrice = 0.0f, amount = 0.0f, closePrice = 0.0f });
			balance.Orders.RemoveAt(0);

			balance.Update(x);

			//Console.ReadLine();
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