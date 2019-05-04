
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HitBTC;
using HitBTC.Models;
using System.Threading;

namespace TradingConsole
{
	public class PendingOrder: IComparable<PendingOrder>
	{
		public string Symbol;
		public string Side;
		public float Quantity;
		public float OpenPrice;
		public float StopPrice;
		public float ClosePrice;

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
		private static Ticker oldTicker;

		public float StopPercent { get; set; }
		public float ClosePercent { get; set; }

		public float StopPrice { get; set; }
		public float ClosePrice { get; set; }

		public Ticker Ticker { get; set; }

		public float Quantity = 0.0f;

		private HitBTCSocketAPI HitBTC;

		private Dictionary<string, List<PendingOrder>> PendingOrders;
		private List<PendingOrder> ClosedgOrders;

		public Trading(ref HitBTCSocketAPI hitBTC, ref Dictionary<string, List<PendingOrder>> pendingOrders, ref List<PendingOrder> closedgOrders)
		{
			this.HitBTC = hitBTC;
			this.PendingOrders = pendingOrders;
			ClosedgOrders = closedgOrders;
			this.StopPercent = 0.01f;
			this.ClosePercent = 0.2f;
		}

		public void trading_0(float stopPercent, float closePercent, float quantity)
		{
			this.StopPercent = stopPercent;
			this.ClosePercent = closePercent;
			this.Ticker = HitBTC.Ticker;
			this.Quantity = quantity;
			

			if (!PendingOrders.ContainsKey(Ticker.Symbol))
			{
				OrderAdd("sell", Ticker);
				OrderAdd("buy", Ticker);
			}
			else
			{
				foreach (var pO in PendingOrders[HitBTC.Ticker.Symbol])
				{
					pO.CalcCurrProfitPercent(HitBTC.Ticker);
				}

				for (int i = 0; i < PendingOrders[Ticker.Symbol].Count; i++)
				{
					if (PendingOrders[Ticker.Symbol][i].Side == "sell")
					{
						if (Ticker.Bid >= PendingOrders[Ticker.Symbol][i].ClosePrice)
						{
							ClosedgOrders.Add(PendingOrders[Ticker.Symbol].ElementAtOrDefault(i));

							PendingOrders[Ticker.Symbol].RemoveAt(i);

							OrderAdd("buy", Ticker);
						}
						else if (Ticker.Bid <= PendingOrders[Ticker.Symbol][i].StopPrice)
						{
							PendingOrders[Ticker.Symbol].RemoveAt(i);
						}
					}
				}

				for (int i = 0; i < PendingOrders[Ticker.Symbol].Count; i++)
				{
					if (PendingOrders[Ticker.Symbol][i].Side == "buy")
					{
						if (Ticker.Ask <= PendingOrders[Ticker.Symbol][i].ClosePrice)
						{
							ClosedgOrders.Add(PendingOrders[Ticker.Symbol].ElementAtOrDefault(i));

							PendingOrders[Ticker.Symbol].RemoveAt(i);

							OrderAdd("sell", Ticker);
						}
						else if (Ticker.Ask >= PendingOrders[Ticker.Symbol][i].StopPrice)
						{
							PendingOrders[Ticker.Symbol].RemoveAt(i);
						}
					}
				}

				if (!PendingOrders[Ticker.Symbol].Any(t => t.Side == "sell"))
				{
					OrderAdd("sell", Ticker);
				}

				if (!PendingOrders[Ticker.Symbol].Any(t => t.Side == "buy"))
				{
					OrderAdd("buy", Ticker);
				}
			}


			oldTicker = Ticker;
		}

		public void OrderAdd(string side, Ticker ticker)
		{
			if (side == "sell")
			{
				if (PendingOrders.ContainsKey(Ticker.Symbol))
				{
					PendingOrders[ticker.Symbol].Add(new PendingOrder
					{
						Symbol = ticker.Symbol,
						Side = "sell",
						Quantity = Quantity,
						OpenPrice = ticker.Bid,
						ClosePercent = ClosePercent,
						StopPercent = StopPercent
					});
				}
				else
				{
					PendingOrders.Add(Ticker.Symbol, new List<PendingOrder>());
					PendingOrders[Ticker.Symbol].Add(new PendingOrder{
														Symbol = ticker.Symbol,
														Side = "sell",
														Quantity = Quantity,
														OpenPrice = ticker.Bid,
														ClosePercent = ClosePercent,
														StopPercent = StopPercent });
				}
			}

			else if (side == "buy")
			{
				if (PendingOrders.ContainsKey(Ticker.Symbol))
				{
					PendingOrders[ticker.Symbol].Add(new PendingOrder
					{
						Symbol = ticker.Symbol,
						Side = "buy",
						Quantity = Quantity,
						OpenPrice = ticker.Bid,
						ClosePercent = ClosePercent,
						StopPercent = StopPercent
					});
				}
				else
				{
					PendingOrders.Add(Ticker.Symbol, new List<PendingOrder>());
					PendingOrders[Ticker.Symbol].Add(new PendingOrder
					{
						Symbol = ticker.Symbol,
						Side = "buy",
						Quantity = Quantity,
						OpenPrice = ticker.Bid,
						ClosePercent = ClosePercent,
						StopPercent = StopPercent
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


	class TradingConsole
	{
		static HitBTCSocketAPI hitBTC;
		static Screen screen;

		static string pKey = "";
		static string sKey = "";

		public static Dictionary<string, List<PendingOrder>> PendingOrders;
		public static List<PendingOrder> ClosedOrders;

		public static Trading Trading;

		static void Main(string[] args)
		{
			hitBTC = new HitBTCSocketAPI();

			PendingOrders = new Dictionary<string, List<PendingOrder>>();
			ClosedOrders = new List<PendingOrder>();

			screen = new Screen(ref hitBTC, ref PendingOrders, ref ClosedOrders);

			Trading = new Trading(ref hitBTC, ref PendingOrders, ref ClosedOrders);

			hitBTC.SocketAuth.Auth(pKey, sKey);
			hitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			hitBTC.SocketTrading.GetTradingBalance();

			Thread.Sleep(2000);

			hitBTC.SocketMarketData.GetSymbols();

			hitBTC.SocketMarketData.SubscribeTicker("BTCUSD");
			hitBTC.SocketMarketData.SubscribeTicker("ETHUSD");
			hitBTC.SocketMarketData.SubscribeTicker("ETCUSD");
			hitBTC.SocketMarketData.SubscribeTicker("LTCUSD");
			hitBTC.SocketMarketData.SubscribeTicker("REPUSD");


			Console.ReadKey();
		}

		private static void HitBTCSocket_MessageReceived(string s)
		{
			if (s == "ticker")
			{
				Trading.trading_0(0.01f, 0.2f, 1.0f);

				screen.Print(hitBTC, PendingOrders, ClosedOrders);
			}
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
