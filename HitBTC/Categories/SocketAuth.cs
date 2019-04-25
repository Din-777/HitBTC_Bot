using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC.Models;
using Newtonsoft.Json;
using WebSocket4Net;

namespace HitBTC.Categories
{
	public class SocketAuth
	{
		[JsonProperty("method")]
		string method = "login";

		[JsonProperty("params")]
		ParamsAuth Params;

		WebSocket socket;

		public SocketAuth(ref WebSocket socket)
		{
			this.socket = socket;
		}

		SocketAuth(string pKey, string sKey)
		{
			Params = new ParamsAuth { Algo = "BASIC", PKey = pKey, Skey = sKey };
		}

		[JsonProperty("id")]
		string id = "auth";

		public async void Auth(string pKey, string sKey)
		{
			var s = new Categories.SocketAuth(pKey, sKey);
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}
	}
}
