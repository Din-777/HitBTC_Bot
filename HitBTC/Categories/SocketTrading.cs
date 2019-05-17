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
		public string Method = "newOrder";

		[JsonProperty("params")]
		public ParamsPlaceNewOrder Params;

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

	class SubscribeReports
	{
		[JsonProperty("method")]
		string Method = "subscribeReports";

		[JsonProperty("params")]
		ParamsNull Params = new ParamsNull();

		[JsonProperty("id")]
		string Id = "subscribeReports";
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
			var gtb = new Categories.GetTradingBalance();
			var jsonGtb = JsonConvert.SerializeObject(gtb);
			await Task.Run(() => socket.Send(jsonGtb));
		}

		public async void PlaceNewOrder(string symbol, string side, decimal quantity, string clientOrderId = null, bool strictValidate = true)
		{
			OrderType orderType = OrderType.Market;

			if (clientOrderId == null)
				clientOrderId = Utils.GenerateId();

			var parameters = new ParamsPlaceNewOrder
			{
				clientOrderId = clientOrderId,
				symbol = symbol,
				side = side,
				type = "market",
				quantity = quantity,
				strictValidate = strictValidate
			};

			PlaceNewOrder pno = new Categories.PlaceNewOrder();
			pno.Params = parameters;

			var jsonPno = JsonConvert.SerializeObject(pno);
			await Task.Run(() => socket.Send(jsonPno));
		}

		public async void SubscribeReports()
		{
			var sr = new SubscribeReports();
			var jsonSr = JsonConvert.SerializeObject(sr);
			await Task.Run(() => socket.Send(jsonSr));
		}

	}
}
