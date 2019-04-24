using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HitBTC.Models;
using Newtonsoft.Json;

namespace HitBTC.Categories
{
	class GetTradingBalance
	{
		[JsonProperty("method")]
		string Method = "getTradingBalance";

		[JsonProperty("params")]
		ParamsBalance Params = new ParamsBalance();

		[JsonProperty("id")]
		string Id = "balance";
	}

	class PlaceNewOrder
	{
		[JsonProperty("method")]
		string Method = "newOrder";

		[JsonProperty("params")]
		ParamsPlaceNewOrder Params;

		[JsonProperty("id")]
		string id = "placeNewOrder";

		public PlaceNewOrder()
		{
			Params = new ParamsPlaceNewOrder
			{
				clientOrderId = GenerateId()
			};
		}

		private static string GenerateId() => Guid.NewGuid().ToString()
									.TrimEnd('=')
									.Replace("+", "")
									.Replace(@"\", "")
									.Replace(@"/", "")
									.Replace("-", "");
	}
}