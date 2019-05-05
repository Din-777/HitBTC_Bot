using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;
using HitBTC.Models;

namespace Trading
{
	public class PendingOrder : IComparable<PendingOrder>
	{
		public string Symbol;
		public string Side;
		public float Quantity;
		public float OpenPrice;
		public float StopPrice;
		public float ClosePrice;

		public bool Closed = false;

		private float stopPercent;
		private float closePercent;

		public float StopPercent
		{
			get { return stopPercent; }
			set
			{
				stopPercent = value;
				StopPrice = Side == "sell" ? OpenPrice - OpenPrice.Percent(stopPercent) : OpenPrice + OpenPrice.Percent(stopPercent);
			}
		}

		public float ClosePercent
		{
			get { return closePercent; }
			set
			{
				closePercent = value;
				ClosePrice = Side == "sell" ? OpenPrice + OpenPrice.Percent(closePercent) : OpenPrice - OpenPrice.Percent(closePercent);
			}
		}

		public DateTime CreatedAt;

		public float CurrProfitPercent;

		public float CalcCurrProfitPercent(Ticker ticker)
		{
			CurrProfitPercent = ((100.0f / (OpenPrice / (Side == "sell" ? ticker.Bid : ticker.Ask))) - 100.0f) * (Side == "sell" ? 1.0f : (-1.0f));
			return CurrProfitPercent;
		}

		public PendingOrder() { }

		public PendingOrder(string Symbol, string Side, float OpenPrice, float stopPercent, float closePercent)
		{

		}

		public int CompareTo(PendingOrder obj)
		{
			if (this.CurrProfitPercent > obj.CurrProfitPercent)
				return 1;
			if (this.CurrProfitPercent < obj.CurrProfitPercent)
				return -1;
			else
				return 0;
		}
	}

	public class Trading
	{
		public Ticker Ticker { get; set; }

		public float Quantity = 0.0f;

		private HitBTCSocketAPI HitBTC;

		public Dictionary<string, List<PendingOrder>> PendingOrders;
		public List<PendingOrder> ClosedOrders;

		private Dictionary<string, OrderOption> OrderOptions;

		public Trading(ref HitBTCSocketAPI hitBTC)
		{
			this.HitBTC = hitBTC;
			this.PendingOrders = new Dictionary<string, List<PendingOrder>>();
			this.ClosedOrders = new List<PendingOrder>();
			this.OrderOptions = new Dictionary<string, OrderOption>();
		}

		private class OrderOption
		{
			public float quantity = 0.00f;
			public float stopPercent = 0.01f;
			public float closePercent = 0.20f;

		}

		public void Add(string symbol, float quantity, float stopPercent, float closePercent)
		{
			OrderOptions.Add(symbol, new OrderOption { quantity = quantity, stopPercent = stopPercent, closePercent = closePercent });
			HitBTC.SocketMarketData.SubscribeTicker(symbol);
		}

		public void Run(string symbol)
		{
			Ticker = HitBTC.Ticker;

			if (!PendingOrders.ContainsKey(symbol))
			{
				OrderAdd(symbol, "sell", Ticker);
				OrderAdd(symbol, "buy", Ticker);
			}
			else
			{
				foreach (var pO in PendingOrders[symbol])
				{
					pO.CalcCurrProfitPercent(Ticker);
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if (PendingOrders[symbol][i].Side == "sell")
					{
						if (Ticker.Bid >= PendingOrders[symbol][i].ClosePrice)
						{
							//if (HitBTC.Balance[symbol.Substring(0, 3)].Available > PendingOrders[symbol][i].Quantity)
							//{
								Sell(symbol, PendingOrders[symbol][i].Quantity);
								HitBTC.SocketTrading.GetTradingBalance();

								ClosedOrders.Add(PendingOrders[symbol].ElementAtOrDefault(i));

								PendingOrders[symbol].RemoveAt(i);

								OrderAdd(symbol, "buy", Ticker);
							//}
							//else PendingOrders[symbol].ElementAtOrDefault(i).Closed = true;

						}
						else if (Ticker.Bid <= PendingOrders[symbol][i].StopPrice)
						{
							PendingOrders[symbol].RemoveAt(i);
						}
						else PendingOrders[symbol].ElementAtOrDefault(i).Closed = false;
					}
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if (PendingOrders[symbol][i].Side == "buy")
					{
						if (Ticker.Ask <= PendingOrders[symbol][i].ClosePrice)
						{
							//if (HitBTC.Balance[symbol.Substring(3)].Available > PendingOrders[symbol][i].Quantity)
							//{
								Buy(symbol, PendingOrders[symbol][i].Quantity);
								HitBTC.SocketTrading.GetTradingBalance();

								ClosedOrders.Add(PendingOrders[symbol].ElementAtOrDefault(i));

								PendingOrders[symbol].RemoveAt(i);

								OrderAdd(symbol, "sel", Ticker);
							//}
							//else PendingOrders[symbol].ElementAtOrDefault(i).Closed = true;
						}
						else if (Ticker.Ask >= PendingOrders[symbol][i].StopPrice)
						{
							PendingOrders[symbol].RemoveAt(i);
						}
						else PendingOrders[symbol].ElementAtOrDefault(i).Closed = false;
					}
				}

				if (!PendingOrders[symbol].Any(t => t.Side == "sell"))
					OrderAdd(symbol, "sell", Ticker);

				if (!PendingOrders[symbol].Any(t => t.Side == "buy"))
					OrderAdd(symbol, "buy", Ticker);
			}
		}

		public void OrderAdd(string symbol, string side, Ticker ticker)
		{
			if (side == "sell")
			{
				if (PendingOrders.ContainsKey(symbol))
				{
					PendingOrders[symbol].Add(new PendingOrder
					{
						Side = "sell",
						Symbol = symbol,
						OpenPrice = ticker.Bid,
						Quantity = OrderOptions[symbol].quantity * OrderOptions[symbol].quantity,
						StopPercent = OrderOptions[symbol].stopPercent,
						ClosePercent = OrderOptions[symbol].closePercent
					});
				}
				else
				{
					PendingOrders.Add(Ticker.Symbol, new List<PendingOrder>());
					PendingOrders[Ticker.Symbol].Add(new PendingOrder
					{
						Side = "sell",
						Symbol = symbol,
						OpenPrice = ticker.Bid,
						Quantity = OrderOptions[symbol].quantity,
						StopPercent = OrderOptions[symbol].stopPercent,
						ClosePercent = OrderOptions[symbol].closePercent
					});
				}
			}

			else if (side == "buy")
			{
				if (PendingOrders.ContainsKey(Ticker.Symbol))
				{
					PendingOrders[ticker.Symbol].Add(new PendingOrder
					{
						Side = "buy",
						Symbol = symbol,
						OpenPrice = ticker.Bid,
						Quantity = OrderOptions[symbol].quantity,
						StopPercent = OrderOptions[symbol].stopPercent,
						ClosePercent = OrderOptions[symbol].closePercent
					});
				}
				else
				{
					PendingOrders.Add(Ticker.Symbol, new List<PendingOrder>());
					PendingOrders[Ticker.Symbol].Add(new PendingOrder
					{
						Side = "buy",
						Symbol = symbol,
						OpenPrice = ticker.Bid,
						Quantity = OrderOptions[symbol].quantity,
						StopPercent = OrderOptions[symbol].stopPercent,
						ClosePercent = OrderOptions[symbol].closePercent
					});
				}
			}
		}


		public bool Sell(string symbol, float quantity)
		{
			//HitBTC.SocketTrading.PlaceNewOrder(symbol, "sell", quantity);
			return false;
		}

		public bool Buy(string symbol, float quantity)
		{
			//HitBTC.SocketTrading.PlaceNewOrder(symbol, "buy", quantity);
			return false;
		}

		
	}

	public static class FloatExtension
	{
		public static float Percent(this float number, float percent)
		{
			//return ((double) 80         *       25)/100;
			return (number * percent) / 100.0f;
		}
	}
}
