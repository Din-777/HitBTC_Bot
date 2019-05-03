
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
	public class PendingOrder
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
			CurrProfitPercent = ( (100.0f / (OpenPrice / (Side == "sell" ? ticker.Bid : ticker.Ask) ) ) - 100.0f) * (Side == "sell" ? 1.0f : (-1.0f));
			return CurrProfitPercent;
		}

		public PendingOrder() { }

		public PendingOrder(string Symbol, string Side, float OpenPrice, float stopPercent, float closePercent)
		{

		}
	}

	public class PendingOrders: List<PendingOrder>
	{


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
			this.ClosePercent = 0.1f;
		}

		public void trading_0(float stopPercent, float closePercent, float quantity)
		{
			this.StopPercent = stopPercent;
			this.ClosePercent = closePercent;
			this.Ticker = HitBTC.Ticker;

			foreach(var ticker in HitBTC.Tickers)
			{
				if (oldTicker == null)
				{
					OrderAdd("sell", ticker.Key, ticker.Value);
					OrderAdd("buy", ticker.Key, ticker.Value);
				}
				else
				{
					for (int i = 0; i < PendingOrders[ticker.Key].Count; i++)
					{
						if (PendingOrders[ticker.Key][i].Side == "sell")
						{
							if (PendingOrders[ticker.Key][i].Symbol == ticker.Key)
							{
								if (Ticker.Bid >= PendingOrders[ticker.Key][i].ClosePrice)
								{
									ClosedgOrders.Add(PendingOrders[ticker.Key].ElementAtOrDefault(i));

									PendingOrders[ticker.Key].RemoveAt(i);

									OrderAdd("buy", ticker.Key, ticker.Value);
								}
								else if (Ticker.Bid <= PendingOrders[ticker.Key][i].StopPrice)
								{
									PendingOrders[ticker.Key].RemoveAt(i);
								}

							}
						}
					}

					for (int i = 0; i < PendingOrders[ticker.Key].Count; i++)
					{
						if (PendingOrders[ticker.Key][i].Side == "buy")
						{
							if (PendingOrders[ticker.Key][i].Symbol == ticker.Key)
							{
								if (Ticker.Bid >= PendingOrders[ticker.Key][i].ClosePrice)
								{
									ClosedgOrders.Add(PendingOrders[ticker.Key].ElementAtOrDefault(i));

									PendingOrders[ticker.Key].RemoveAt(i);

									OrderAdd("sell", ticker.Key, ticker.Value);
								}
								else if (Ticker.Bid <= PendingOrders[ticker.Key][i].StopPrice)
								{
									PendingOrders[ticker.Key].RemoveAt(i);
								}

							}
						}
					}

					if (!PendingOrders[ticker.Key].Any(t => t.Side == "sell"))
					{
						OrderAdd("sell", ticker.Key, ticker.Value);
					}

					if (!PendingOrders[ticker.Key].Any(t => t.Side == "buy"))
					{
						OrderAdd("buy", ticker.Key, ticker.Value);
					}
				}
			}			

			oldTicker = Ticker;
		}

		public void OrderAdd(string side, string symbol, Ticker ticker)
		{
			if (side == "sell")
			{
				PendingOrders[symbol].Add(new PendingOrder
				{
					Symbol = symbol,
					Side = "sell",
					Quantity = Quantity,
					OpenPrice = ticker.Bid,
					ClosePercent = ClosePercent,
					StopPercent = StopPercent
				});
			}

			else if (side == "buy")
			{
				PendingOrders[symbol].Add(new PendingOrder
				{
					Symbol = symbol,
					Side = "buy",
					Quantity = Quantity,
					OpenPrice = Ticker.Ask,
					ClosePercent = ClosePercent,
					StopPercent = StopPercent
				});
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


			Console.ReadKey();
		}

		static List<Ticker> Tickers = new List<Ticker>();

		private static void HitBTCSocket_MessageReceived(string s)
		{
			if (s == "ticker")
			{
				Tickers.Add(hitBTC.Ticker);
				if (Tickers.Count > 20) Tickers.RemoveAt(0);

				Trading.trading_0(0.01f, 0.1f, 1.0f);

				screen.Print(hitBTC, Tickers, PendingOrders, ClosedOrders);
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
