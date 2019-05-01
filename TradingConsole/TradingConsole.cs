
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

	class PendingOrders: List<PendingOrder>
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

		private List<PendingOrder> PendingOrders;

		public Trading(ref HitBTCSocketAPI hitBTC, ref List<PendingOrder> pendingOrders)
		{
			this.HitBTC = hitBTC;
			this.PendingOrders = pendingOrders;
			this.StopPercent = 0.01f;
			this.ClosePercent = 0.1f;
		}

		public void trading_0(float stopPercent, float closePercent, float quantity)
		{
			this.StopPercent = stopPercent;
			this.ClosePercent = closePercent;
			this.Ticker = HitBTC.Ticker;



			if (oldTicker == null)
			{
				PendingOrders.Add(new PendingOrder
				{
					Symbol = "BTCUSD",
					Side = "sell",
					Quantity = Quantity,
					OpenPrice = Ticker.Bid,					
					ClosePercent = ClosePercent,
					StopPercent = StopPercent
				});

				PendingOrders.Add(new PendingOrder
				{
					Symbol = "BTCUSD",
					Side = "buy",
					Quantity = Quantity,
					OpenPrice = Ticker.Ask,
					ClosePercent = ClosePercent,
					StopPercent = StopPercent
				});
			}
			else
			{
				for (int i = 0; i < PendingOrders.Count; i++)
				{
					if (PendingOrders[i].Side == "sell")
					{						
						if (Ticker.Bid >= PendingOrders[i].ClosePrice)
						{
							PendingOrders.RemoveAt(i);
							PendingOrders.Add(new PendingOrder
							{
								Symbol = "BTCUSD",
								Side = "buy",
								Quantity = Quantity,
								OpenPrice = Ticker.Ask,
								ClosePercent = ClosePercent,
								StopPercent = StopPercent
							});
						}
						else if (Ticker.Bid <= PendingOrders[i].StopPrice)
						{
							PendingOrders.RemoveAt(i);
						}
					}
				}

				for (int i = 0; i < PendingOrders.Count; i++)
				{
					if (PendingOrders[i].Side == "buy")
					{
						if (Ticker.Ask <= PendingOrders[i].ClosePrice)
						{
							PendingOrders.RemoveAt(i);
							PendingOrders.Add(new PendingOrder
							{
								Symbol = "BTCUSD",
								Side = "sell",
								Quantity = Quantity,
								OpenPrice = Ticker.Bid,
								ClosePercent = ClosePercent,
								StopPercent = StopPercent
							});
						}
						else if (Ticker.Ask >= PendingOrders[i].StopPrice)
						{
							PendingOrders.RemoveAt(i);
						}
					}
				}
			}

			if (!PendingOrders.Any(t => t.Side == "sell"))
			{
				PendingOrders.Add(new PendingOrder
				{
					Symbol = "BTCUSD",
					Side = "sell",
					Quantity = Quantity,
					OpenPrice = Ticker.Bid,
					ClosePercent = ClosePercent,
					StopPercent = StopPercent
				});
			}

			if (!PendingOrders.Any(t => t.Side == "buy"))
			{
				PendingOrders.Add(new PendingOrder
				{
					Symbol = "BTCUSD",
					Side = "buy",
					Quantity = Quantity,
					OpenPrice = Ticker.Ask,
					ClosePercent = ClosePercent,
					StopPercent = StopPercent
				});
			}

			oldTicker = Ticker;
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

		static string pKey = "Y";
		static string sKey = "B";

		//static string pKey = "p";
		//static string sKey = "M";

		public static List<PendingOrder> PendingOrders;

		public static Trading Trading;

		static void Main(string[] args)
		{
			hitBTC = new HitBTCSocketAPI();
			screen = new Screen(ref hitBTC, ref PendingOrders);

			PendingOrders = new List<PendingOrder>();
			Trading = new Trading(ref hitBTC, ref PendingOrders);

			hitBTC.SocketAuth.Auth(pKey, sKey);
			hitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			hitBTC.SocketTrading.GetTradingBalance();

			Thread.Sleep(2000);

			hitBTC.SocketMarketData.SubscribeTicker("BTCUSD");
			//hitBTC.SocketMarketData.SubscribeTicker("ETHUSD");


			Console.ReadKey();
		}

		private static void HitBTCSocket_MessageReceived(string s)
		{
			if (s == "ticker")
			{
				Trading.trading_0(0.01f, 0.1f, 0.00001f);

				screen.Print(hitBTC, PendingOrders);
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
