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
}
