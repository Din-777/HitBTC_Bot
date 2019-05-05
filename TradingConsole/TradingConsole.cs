
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

			//hitBTC.SocketAuth.Auth(pKey, sKey);
			hitBTC.MessageReceived += HitBTCSocket_MessageReceived;

			//hitBTC.SocketTrading.GetTradingBalance();

			Thread.Sleep(2000);

			hitBTC.SocketMarketData.GetSymbols();

			Trading.Add("BTCUSD", 1.0f, 0.01f, 0.3f );
			Trading.Add("ETHUSD", 1.0f, 0.01f, 0.2f);
			Trading.Add("ETCUSD", 1.0f, 0.01f, 0.3f);
			Trading.Add("LTCUSD", 1.0f, 0.01f, 0.2f);
			Trading.Add("NANOUSD", 1.0f, 0.01f, 0.3f);


			Console.ReadKey();
		}

		private static void HitBTCSocket_MessageReceived(string s)
		{
			if (s == "ticker")
			{
				Trading.Run(hitBTC.Ticker.Symbol);

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
