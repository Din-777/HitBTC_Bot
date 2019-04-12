using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp
{
	public class Dealing
	{
		public string tred; // "buy" "sel"
		public float price;

		public Dealing(string tred, float price)
		{
			this.tred = tred;
			this.price = price;
		}

	}

	public struct Balance
	{
		public List<Dealing> OpenOrders;
	}

	class Program
	{
		static void Main(string[] args)
		{
			Balance balance = new Balance();
			balance.OpenOrders = new List<Dealing>();

			balance.OpenOrders.Add(new Dealing("buy", 1.0f));
			balance.OpenOrders.Add(new Dealing("buy", 2.0f));
			balance.OpenOrders.Add(new Dealing("buy", 3.0f));
			balance.OpenOrders.Add(new Dealing("buy", 4.0f));
			balance.OpenOrders.Add(new Dealing("buy", 5.0f));

			bool res = balance.OpenOrders.Any(t => t.tred == "sel");

			float price = 3.0f;
			balance.OpenOrders = (  from openOperder in balance.OpenOrders
									let l = new
									{
										Tred = openOperder.tred,
										Price = openOperder.price,
										Diff = Math.Abs(openOperder.price - price)
									}
									orderby l.Diff
									select new Dealing(l.Tred, l.Price)).ToList<Dealing>();

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