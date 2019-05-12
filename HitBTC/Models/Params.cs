using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HitBTC.Models
{
	public class Params
	{
		public string symbol;
	}

	public class ParamsNull
	{
	}

	public class ParamsTicker
	{
		[JsonProperty("symbol")]
		public string Symbol;		
	}
	
	public class ParamsAuth
	{
		[JsonProperty("algo")]
		public string Algo { get; internal set; }

		[JsonProperty("pKey")]
		public string PKey { get; internal set; }

		[JsonProperty("sKey")]
		public string Skey { get; internal set; }
	}

	public class ParamsBalance
	{
	}

	public class ParamsPlaceNewOrder
	{
		public string clientOrderId;	// Required parameter.Uniqueness must be guaranteed within a single trading day, including all active orders.
		public string symbol;			// Trading symbol
		public string side;				// Trade side. Accepted values: sell, buy
		public string type;				// Optional parameter: limit, market, stopLimit, stopMarket. Default value: limit
		public string timeInForce;		// Optional parameter. GTC, IOC, FOK, Day. GTD Default value: GTC
		public float quantity;			//Order quantity
		public float price;				//Order price.Required for limit types
		public float stopPrice;			// Required for stop-limit orders
		public DateTime expireTime;		// Required for timeInForce = GTD
		public bool strictValidate;		// Price and quantity will be checked for the incrementation within tick size and quantity step.
										// See symbol's tickSize and quantityIncrement
		public bool postOnly;			// A post-only order is an order that does not remove liquidity.
										// If your post-only order causes a match with a pre-existing order as a taker, then order will be cancelled.
	}

	public class ParamsActiveOrders
	{
		public string id;
		public string clientOrderId;
		public string symbol;
		public string side;
		public string status;
		public string type;
		public string timeInForce;
		public float quantity;
		public float price;
		public float cumQuantity;
		public bool postOnly;
		public DateTime createdAt;
		public DateTime updatedAt;
		public string  reportType;
	}

	public class ParamsSubscribeCandles
	{
		public string symbol;
		public Period period;
		public int limit;
	}
}
