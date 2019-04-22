using System;
using System.Security.Cryptography;
using RestSharp;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

using System.Threading;

namespace Temp
{
	class Program
	{
		static void Main(string[] args)
		{

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