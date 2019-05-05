using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;
using HitBTC.Models;

namespace TradingConsole
{
	public class Trading
	{
		public Ticker Ticker { get; set; }

		public float Quantity = 0.0f;

		private HitBTCSocketAPI HitBTC;

		private Dictionary<string, List<PendingOrder>> PendingOrders;
		private Dictionary<string, OrderOption> OrderOptions;
		private List<PendingOrder> ClosedgOrders;

		public Trading(ref HitBTCSocketAPI hitBTC, ref Dictionary<string, List<PendingOrder>> pendingOrders, ref List<PendingOrder> closedgOrders)
		{
			this.HitBTC = hitBTC;
			this.PendingOrders = pendingOrders;
			ClosedgOrders = closedgOrders;
			OrderOptions = new Dictionary<string, OrderOption>();
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
							ClosedgOrders.Add(PendingOrders[symbol].ElementAtOrDefault(i));

							PendingOrders[symbol].RemoveAt(i);

							OrderAdd(symbol, "buy", Ticker);
						}
						else if (Ticker.Bid <= PendingOrders[symbol][i].StopPrice)
						{
							PendingOrders[symbol].RemoveAt(i);
						}
					}
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if (PendingOrders[symbol][i].Side == "buy")
					{
						if (Ticker.Ask <= PendingOrders[symbol][i].ClosePrice)
						{
							ClosedgOrders.Add(PendingOrders[symbol].ElementAtOrDefault(i));

							PendingOrders[symbol].RemoveAt(i);

							OrderAdd(symbol, "sell", Ticker);
						}
						else if (Ticker.Ask >= PendingOrders[symbol][i].StopPrice)
						{
							PendingOrders[symbol].RemoveAt(i);
						}
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

		public bool buyBTC(ref Balance balance, float amount, float profit = 0.0f)
		{
			return false;
		}

		public bool selBTC(ref Balance balance, float amount, float profit = 0.0f)
		{
			return false;
		}

		
	}
}
