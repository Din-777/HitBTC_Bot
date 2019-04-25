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
				clientOrderId = Utils.GenerateId()
			};
		}
	}

	public class SocketTrading
	{
		WebSocket socket;

		public SocketTrading(ref WebSocket socket)
		{
			this.socket = socket;
		}

		public async void GetTradingBalance()
		{
			var s = new Categories.GetTradingBalance();
			var jsonStr = JsonConvert.SerializeObject(s);
			await Task.Run(() => socket.Send(jsonStr));
		}
	}
}
