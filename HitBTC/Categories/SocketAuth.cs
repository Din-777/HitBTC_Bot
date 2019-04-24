using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC.Models;
using Newtonsoft.Json;

namespace HitBTC.Categories
{
	class SocketAuth
	{
		[JsonProperty("method")]
		string method = "login";

		[JsonProperty("params")]
		ParamsAuth Params;


		public SocketAuth(string pKey, string sKey)
		{
			Params = new ParamsAuth { Algo = "BASIC", PKey = pKey, Skey = sKey };

		}

		[JsonProperty("id")]
		string id = "auth";


	}
}
