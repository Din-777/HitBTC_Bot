using System;

using System.Text;
using System.Net;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;


using HitBTC;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net.Http;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;

namespace Temp
{
	
	class Program
	{		
		


		static void Main(string[] args)
		{
			List<Balance> balance = new List<Balance>();
			

			Console.ReadLine();
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