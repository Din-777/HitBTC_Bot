using HitBTC;
using HitBTC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Trading.Utilities;
using CsvHelper;


namespace Trading
{
	public enum Type
	{
		New,
		Sell,
		First,
		Closed,
		Deleted,
		Processed,
		Confirmed,
		Additional
	}

	[Serializable]
	public class PendingOrder : IComparable<PendingOrder>
	{
		public DateTime DateTime { get; set; }
		public int Id { get; set; }
		public static int StaticId;
		public string Symbol { get; set; }
		public string Side { get; set; }
		public decimal Quantity { get; set; }
		public decimal OpenPrice { get; set; }
		public decimal StopPrice { get; set; }
		public decimal ClosePrice { get; set; }

		public Type Type { get; set; }

		private decimal stopPercent;
		private decimal closePercent;

		public decimal StopPercent
		{
			get { return stopPercent; }
			set
			{
				stopPercent = value;
				StopPrice = Side == "sell" ? OpenPrice - OpenPrice.Percent(stopPercent) : OpenPrice + OpenPrice.Percent(stopPercent);
			}
		}

		public decimal ClosePercent
		{
			get { return closePercent; }
			set
			{
				closePercent = value;
				ClosePrice = Side == "sell" ? OpenPrice + OpenPrice.Percent(closePercent) : OpenPrice - OpenPrice.Percent(closePercent);
			}
		}

		public decimal CurrProfitPercent { get; set; }
		public decimal CurrProfitInUSD { get; set; }
		public decimal QuantityInUSDBuy { get; set; }
		public decimal QuantityInUSDSell { get; set; }

		public decimal CalcCurrProfitPercent(decimal price)
		{
			CurrProfitPercent = ((100.0m / (OpenPrice / price)) - 100.0m) *
				(Side == "sell" ? 1.0m : (-1.0m));

			if(Side == "sell")
			{
				CurrProfitInUSD = (Quantity * price) - QuantityInUSDBuy;
			}

			return CurrProfitPercent;
		}

		public PendingOrder()
		{
			StaticId += 1; Id = StaticId;
			DateTime = DateTime.Now;
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

	[Serializable]
	public class OrderParametr
	{
		public decimal QuantityInPercentByBalanceQuote;
		public decimal StopPercent;
		public decimal ClosePercent;
		public Period Period;
		public int SmaPeriodFast;
		public int SmaPeriodSlow;
	}

	[Serializable]
	class SaveObject
	{
		public int StaticId;
		public DateTime DateTimeStart;
		public DateTime DateTimeStartCurr;
		public List<PendingOrder> ClosedOrders;
		public Dictionary<string, Balance> DemoBalance;
		public Dictionary<string, OrderParametr> OrdersParameters;
		public Dictionary<string, PendingOrder> PendingOrders;
	}

	public class Trading
	{		
		public Dictionary<string, PendingOrder> PendingOrders;
		public List<PendingOrder> ClosedOrders;
		public Dictionary<string, OrderParametr> OrdersParameters;
		public Dictionary<string, Balance> DemoBalance;

		public Dictionary<string, SMA> SmaSlow = new Dictionary<string, SMA>();
		public Dictionary<string, SMA> SmaFast = new Dictionary<string, SMA>();
		public Dictionary<string, RSI> RSI = new Dictionary<string, RSI>();
		public Dictionary<string, BB> BB = new Dictionary<string, BB>();

		public Dictionary<string, List<decimal>> d_lSmaSlow;
		public Dictionary<string, List<decimal>> d_lSmaFast;
		public Dictionary<string, List<decimal>> d_lRSI;
		public Dictionary<string, List<(decimal Sma, decimal BU, decimal BD)>> d_lBB;

		private HitBTCSocketAPI HitBTC;
		public bool DataFileIsloaded = false;
		private string TradingDataFileName;
		public bool Demo = true;

		private Timer timer;
		private DateTime DateTimeStart;
		private DateTime DateTimeStartCurr;

		private void TimerTick(object obj)
		{
			string start = DateTimeStart.ToString();
			string elapsedTotalTime = (DateTime.Now - DateTimeStart).ToString(@"dd\ hh\:mm\:ss");
			string elapsedCurrentTime = (DateTime.Now - DateTimeStartCurr).ToString(@"dd\ hh\:mm\:ss");

			Console.Title = String.Format("Start: {0}  Total work time: {1}  Current work time {2}  Current time {3}",
				start, elapsedTotalTime, elapsedCurrentTime, DateTime.Now.ToString("HH:mm:ss"));
		}

		public Trading(ref HitBTCSocketAPI hitBTC, bool console = true, bool demo = true)
		{
			Demo = demo;
			DateTimeStart = DateTime.Now;
			DateTimeStartCurr = DateTime.Now;
			if (console)
				timer = new Timer(new TimerCallback(TimerTick), null, 0, 500);

			this.HitBTC = hitBTC;
			this.PendingOrders = new Dictionary<string, PendingOrder>();
			this.OrdersParameters = new Dictionary<string, OrderParametr>();
			this.DemoBalance = new Dictionary<string, Balance>();
			this.ClosedOrders = new List<PendingOrder>();
			d_lSmaSlow = new Dictionary<string, List<decimal>>();
			d_lSmaFast = new Dictionary<string, List<decimal>>();
			d_lRSI = new Dictionary<string, List<decimal>>();
			d_lBB = new Dictionary<string, List<(decimal Sma, decimal BU, decimal BD)>>();

			HitBTC.Closed += HitBTC_Closed;
			HitBTC.MessageReceived += HitBTC_MessageReceived;
		}

		public void Add(string symbol, Period period, decimal tradingQuantityInPercent, decimal stopPercent, decimal closePercent, int SmaPeriodFast = 5, int SmaPeriodSlow = 50)
		{
			if (!OrdersParameters.ContainsKey(symbol))
				OrdersParameters.Add(symbol, new OrderParametr());

			OrdersParameters[symbol] = new OrderParametr
			{
				QuantityInPercentByBalanceQuote = tradingQuantityInPercent,
				StopPercent = stopPercent,
				ClosePercent = closePercent,
				Period = period,
				SmaPeriodFast = SmaPeriodFast,
				SmaPeriodSlow = SmaPeriodSlow
			};

			if (!SmaFast.ContainsKey(symbol))
				SmaFast.Add(symbol, new SMA(SmaPeriodFast));
			if (!SmaSlow.ContainsKey(symbol))
				SmaSlow.Add(symbol, new SMA(SmaPeriodSlow));
			if (!RSI.ContainsKey(symbol))
				RSI.Add(symbol, new RSI(14));
			if (!BB.ContainsKey(symbol))
				BB.Add(symbol, new BB(20));

			//HitBTC.SocketMarketData.SubscribeTicker(symbol);
			Thread.Sleep(10);
			//HitBTC.SocketMarketData.SubscribeTrades(symbol, 5000);
			Thread.Sleep(10);
			HitBTC.SocketMarketData.SubscribeCandles(symbol, period, 200);
			Thread.Sleep(200);
		}

		public PendingOrder PendingOrderAdd(string symbol, string side, decimal price)
		{
			PendingOrder PendingOrder = null;

			if (OrdersParameters.ContainsKey(symbol))
			{
				string baseCurrency = HitBTC.Symbols[symbol].BaseCurrency;
				string quoteCurrency = HitBTC.Symbols[symbol].QuoteCurrency;				

				decimal OrderParameterQuantity = 0;
				decimal quantityInBaseCurrency = 0;			

				OrderParameterQuantity = OrdersParameters[symbol].QuantityInPercentByBalanceQuote;
				quantityInBaseCurrency = (OrderParameterQuantity / price) - ((OrderParameterQuantity / price) % HitBTC.Symbols[symbol].QuantityIncrement);

				PendingOrder = new PendingOrder
				{
					Side = side,
					Type = Type.First,
					Symbol = symbol,
					OpenPrice = price,
					Quantity = OrderParameterQuantity,
					StopPercent = OrdersParameters[symbol].StopPercent,
					ClosePercent = OrdersParameters[symbol].ClosePercent
				};

				if (!PendingOrders.ContainsKey(symbol))
					PendingOrders.Add(symbol, PendingOrder);
				else PendingOrders[symbol] = PendingOrder;
			}
			return PendingOrder;
		}

		public void ClosedOrderById(int id)
		{

		}

		public void _Buy(PendingOrder pendingOrder, decimal price)
		{
			//HitBTC.SocketTrading.GetTradingBalance();
			//Thread.Sleep(200);

			string symbol = pendingOrder.Symbol;

			if (Demo == false)
				DemoBalance = HitBTC.Balance;

			var quoteCurrence = HitBTC.Symbols[symbol].QuoteCurrency;
			var quantityInQuote = DemoBalance[quoteCurrence].Available.Percent(pendingOrder.Quantity);

			var result = Buy(symbol, price, quantityInQuote: quantityInQuote);

			if (result.IsBuy == true)
			{
				ClosedOrders.Add(pendingOrder);
				ClosedOrders.Last().ClosePrice = price;
				ClosedOrders.Last().CurrProfitPercent = -HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;
				ClosedOrders.Last().QuantityInUSDBuy = result.QuantityQuote;

				pendingOrder.Type = Type.Closed;

				PendingOrderAdd(symbol, "sell", price).Type = Type.New;
				PendingOrders[symbol].QuantityInUSDBuy = result.QuantityQuote;
				PendingOrders[symbol].Quantity = result.QuantityBase;
			}
			else
				pendingOrder.Type = Type.First;
		}

		public void _Sell(PendingOrder pendingOrder, decimal price)
		{
			//HitBTC.SocketTrading.GetTradingBalance();
			//Thread.Sleep(200);

			string symbol = pendingOrder.Symbol;

			if (Demo == false)
				DemoBalance = HitBTC.Balance;

			var quantityInBase = pendingOrder.Quantity;
			var result = Sell(symbol, price, quantityInBase: quantityInBase);

			if (result.IsSell == true)
			{
				ClosedOrders.Add(pendingOrder);
				ClosedOrders.Last().ClosePrice = price;
				ClosedOrders.Last().CurrProfitPercent -= HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;
				ClosedOrders.Last().QuantityInUSDSell = result.QuantityQuote;
				ClosedOrders.Last().CurrProfitInUSD = result.QuantityQuote - ClosedOrders.Last().QuantityInUSDBuy;

				pendingOrder.Type = Type.Closed;

				PendingOrderAdd(symbol, "buy", price).Type = Type.New;
			}
			else
				pendingOrder.Type = Type.First;
		}

		public void Run_6(string symbol, decimal price)
		{
			if (!PendingOrders.ContainsKey(symbol))
			{
				PendingOrderAdd(symbol, "buy", price).Type = Type.First;
			}
			else
			{
				PendingOrders[symbol].CalcCurrProfitPercent(price);

				var SmaFastPrice = SmaFast[symbol].LastAverage;
				var SmaSlowPrice = SmaSlow[symbol].LastAverage;

				if (PendingOrders[symbol].Side == "buy")
				{
					if (SmaFastPrice > SmaSlowPrice)
					{
						if (PendingOrders[symbol].Type == Type.Processed)
						{
							PendingOrders[symbol].Type = Type.Confirmed;
						}
						else if (PendingOrders[symbol].Type == Type.Confirmed)
						{
							_Buy(PendingOrders[symbol], price: price);
						}
					}
					else if (SmaFastPrice < SmaSlowPrice && PendingOrders[symbol].Type == Type.First)
					{
						PendingOrders[symbol].Type = Type.New;
					}
					else if (SmaFastPrice < SmaSlowPrice && PendingOrders[symbol].Type == Type.Confirmed)
					{
						PendingOrders[symbol].Type = Type.Processed;
					}

				}
				else if (PendingOrders[symbol].Side == "sell")
				{
					if (SmaFastPrice < SmaSlowPrice && SmaFastPrice > PendingOrders[symbol].ClosePrice)
					{
						if (PendingOrders[symbol].Type == Type.Processed)
						{
							PendingOrders[symbol].Type = Type.Confirmed;
						}
						else if (PendingOrders[symbol].Type == Type.Confirmed)
						{
							_Sell(pendingOrder: PendingOrders[symbol], price: price);
							if (PendingOrders[symbol].Quantity < 100)
							{
								PendingOrders[symbol].Quantity += 1;
								OrdersParameters[symbol].QuantityInPercentByBalanceQuote += 1;
							}

						}
					}
					else if (PendingOrders[symbol].Type == Type.Sell)
					{
						_Sell(pendingOrder: PendingOrders[symbol], price: price);
					}
					else if (PendingOrders[symbol].Type == Type.Confirmed)
					{
						PendingOrders[symbol].Type = Type.Processed;
					}
					else if (SmaFastPrice < PendingOrders[symbol].StopPrice)
					{
						_Sell(pendingOrder: PendingOrders[symbol], price: price);
						if (PendingOrders[symbol].Quantity > 1)
						{
							PendingOrders[symbol].Quantity -= 1;
							OrdersParameters[symbol].QuantityInPercentByBalanceQuote -= 1;
						}
					}

					if (PendingOrders[symbol].CurrProfitPercent > 50.0m)
					{
						_Sell(pendingOrder: PendingOrders[symbol], price: price);
						if (PendingOrders[symbol].Quantity < 100)
						{
							PendingOrders[symbol].Quantity += 1;
							OrdersParameters[symbol].QuantityInPercentByBalanceQuote += 1;
						}
					}
				}
			}

			if ((PendingOrders[symbol].Type == Type.Deleted) ||
				(PendingOrders[symbol].Type == Type.Closed))
			{
				PendingOrders.Remove(symbol);
			}
			else if (PendingOrders[symbol].Type == Type.New)
				PendingOrders[symbol].Type = Type.Processed;
		}

		public void Run_7_RSI(string symbol, decimal price)
		{
			if (!PendingOrders.ContainsKey(symbol))
			{
				PendingOrderAdd(symbol, "buy", price).Type = Type.First;
			}
			else
			{
				var SmaFastPrice = SmaFast[symbol].LastAverage;
				var SmaSlowPrice = SmaSlow[symbol].LastAverage;
				var SmaDiff = SmaFastPrice - SmaSlowPrice;
				PendingOrders[symbol].CalcCurrProfitPercent(price);

				var rsi_value = d_lRSI[symbol][d_lRSI[symbol].Count - 1];

				if (PendingOrders[symbol].Side == "buy")
				{
					if (rsi_value < 20)
					{
						PendingOrders[symbol].Type = Type.Processed;
					}
					else if (rsi_value > 20)
					{
						if(PendingOrders[symbol].Type == Type.Processed)
							_Buy(PendingOrders[symbol], price: price);
					}
				}
				else if (PendingOrders[symbol].Side == "sell")
				{
					if(rsi_value > 80)
					{
						PendingOrders[symbol].Type = Type.Processed;
					}
					if (rsi_value > 80)
					{
						if(PendingOrders[symbol].Type == Type.Processed)
							_Sell(pendingOrder: PendingOrders[symbol], price: price);
					}
					else if (SmaFastPrice < PendingOrders[symbol].StopPrice)
					{
						_Sell(pendingOrder: PendingOrders[symbol], price: price);
					}

					if (PendingOrders[symbol].CurrProfitPercent > 50.0m)
					{
						_Sell(pendingOrder: PendingOrders[symbol], price: price);
					}
				}
			}

			if ((PendingOrders[symbol].Type == Type.Deleted) ||
				(PendingOrders[symbol].Type == Type.Closed))
			{
				PendingOrders.Remove(symbol);
			}
		}

		public (bool IsSell, decimal QuantityBase, decimal QuantityQuote) Sell(string symbol, decimal price, decimal quantityInBase, bool demo = true)
		{
			var result = (isSell: false, quantityBase: 0.0m, quantityQuote: 0.0m);
			if (quantityInBase == 0)
				return result;

			//HitBTC.SocketTrading.GetTradingBalance();
			//Thread.Sleep(200);

			if (demo == false)
				DemoBalance = HitBTC.Balance;

			string baseCurrency = HitBTC.Symbols[symbol].BaseCurrency;
			string quoteCurrency = HitBTC.Symbols[symbol].QuoteCurrency;

			decimal baseAvailable = DemoBalance[baseCurrency].Available;
			decimal quantityInBaseCurrency = quantityInBase;
			decimal quantityInQuoteCurrency;
			decimal realPrice = price * (1 - HitBTC.Symbols[symbol].TakeLiquidityRate);

			if (baseAvailable < HitBTC.Symbols[symbol].QuantityIncrement)
				return result;
			if (quantityInBaseCurrency > baseAvailable)
				quantityInBaseCurrency = baseAvailable;

			quantityInBaseCurrency -= quantityInBaseCurrency % HitBTC.Symbols[symbol].QuantityIncrement;
			quantityInQuoteCurrency = quantityInBaseCurrency * realPrice;

			if (demo == false)
				HitBTC.SocketTrading.PlaceNewOrder(symbol, "sell", quantityInBaseCurrency);

			DemoBalance[baseCurrency].Available  -= quantityInBaseCurrency;
			DemoBalance[quoteCurrency].Available += quantityInQuoteCurrency;

			result.isSell = true;
			result.quantityBase = quantityInBaseCurrency;
			result.quantityQuote = quantityInQuoteCurrency;
			return result;
		}

		public (bool IsBuy, decimal QuantityBase, decimal QuantityQuote) Buy(string symbol, decimal price, decimal quantityInQuote, bool demo = true)
		{
			var result = (isBuy: false, quantityBase: 0.0m, quantityQuote: 0.0m);
			if (quantityInQuote == 0)
				return result;

			//HitBTC.SocketTrading.GetTradingBalance();
			//Thread.Sleep(200);			

			if (demo == false)
				DemoBalance = HitBTC.Balance;
			
			string baseCurrency  = HitBTC.Symbols[symbol].BaseCurrency;
			string quoteCurrency = HitBTC.Symbols[symbol].QuoteCurrency;

			decimal quantityInBaseCurrency;
			decimal quantityInQuoteCurrency = quantityInQuote;
			decimal quoteAvailable = DemoBalance[quoteCurrency].Available;
			decimal realPrice = price * (1 + HitBTC.Symbols[symbol].TakeLiquidityRate);

			if (quoteAvailable < (HitBTC.Symbols[symbol].QuantityIncrement * realPrice))
				return result;
			if (quantityInQuoteCurrency < (HitBTC.Symbols[symbol].QuantityIncrement * realPrice))
				return result;
			if (quantityInQuoteCurrency > quoteAvailable)
				quantityInQuoteCurrency = quoteAvailable;

			quantityInBaseCurrency = quantityInQuoteCurrency / realPrice;
			quantityInBaseCurrency -= quantityInBaseCurrency % HitBTC.Symbols[symbol].QuantityIncrement;

			quantityInQuoteCurrency = quantityInBaseCurrency * realPrice;

			if (demo == false)
				HitBTC.SocketTrading.PlaceNewOrder(symbol, "buy", quantityInBaseCurrency);

			DemoBalance[baseCurrency].Available += quantityInBaseCurrency;
			DemoBalance[quoteCurrency].Available -= quantityInQuoteCurrency;

			result.isBuy = true;
			result.quantityBase = quantityInBaseCurrency;
			result.quantityQuote = quantityInQuoteCurrency;
			return result;
		}

		public void SellAll(string quoteCurrency)
		{
			for (int i = 0; i < DemoBalance.Keys.Count; i++)
			{
				string baseCurrency = DemoBalance.ElementAt(i).Key;
				string symbol = String.Concat(baseCurrency, quoteCurrency);

				if (DemoBalance.ElementAt(i).Key != quoteCurrency)
					if (DemoBalance.ElementAt(i).Value.Available > 0.0m)
						if (HitBTC.d_Candle.ContainsKey(symbol))
						{
							Sell(symbol, HitBTC.d_Candle[symbol].Close, DemoBalance.ElementAt(i).Value.Available);
						}
						else if (HitBTC.d_Candle.ContainsKey(String.Concat(baseCurrency, "BTC")))
						{
							quoteCurrency = "BTC";
							symbol = String.Concat(baseCurrency, quoteCurrency);
							Sell(symbol, HitBTC.d_Candle[symbol].Close, DemoBalance.ElementAt(i).Value.Available);
						}
						else if (HitBTC.d_Candle.ContainsKey(String.Concat(baseCurrency, "ETH")))
						{
							quoteCurrency = "ETH";
							symbol = String.Concat(baseCurrency, quoteCurrency);
							Sell(symbol, HitBTC.d_Candle[symbol].Close, DemoBalance.ElementAt(i).Value.Available);
						}
			}
		}

		public void Save(string fileNeme)
		{
			BinaryFormatter formatter = new BinaryFormatter();

			using (FileStream fs = new FileStream(fileNeme, FileMode.OpenOrCreate))
			{
				formatter.Serialize(fs, new SaveObject
				{
					StaticId = PendingOrder.StaticId,
					DateTimeStart = DateTimeStart,
					DemoBalance = DemoBalance,
					ClosedOrders = ClosedOrders,
					OrdersParameters = OrdersParameters,
					PendingOrders = PendingOrders
				});
			}

			SaveCsv();
		}

		public bool Load(string fileNeme)
		{
			TradingDataFileName = fileNeme;
			BinaryFormatter formatter = new BinaryFormatter();
			try
			{
				using (FileStream fs = new FileStream(fileNeme, FileMode.OpenOrCreate))
				{
					SaveObject saveObject = (SaveObject)formatter.Deserialize(fs);

					DateTimeStart = saveObject.DateTimeStart;
					DemoBalance = saveObject.DemoBalance;
					ClosedOrders = saveObject.ClosedOrders;
					PendingOrders = saveObject.PendingOrders;
					OrdersParameters = saveObject.OrdersParameters;
					PendingOrder.StaticId = saveObject.StaticId;

					if (DateTimeStart == null)
						DateTimeStart = DateTime.Now;

					DataFileIsloaded = true;

					LoadCsv();

					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		public void SaveCsv(string fileNeme = "pendinOrders.csv")
		{
			using (var writer = new StreamWriter("pendinOrders.csv"))
			using (var csv = new CsvWriter(writer))
			{
				csv.WriteRecords(PendingOrders);
			}

			using (var writer = new StreamWriter("closedOrders.csv"))
			using (var csv = new CsvWriter(writer))
			{
				csv.WriteRecords(ClosedOrders);
			}
		}

		public void LoadCsv(string fileNeme = "pendinOrders.csv")
		{
			using (var reader = new StreamReader("pendinOrders.csv"))
			using (var csv = new CsvReader(reader))
			{
				var records = csv.GetRecords<PendingOrder>();
				PendingOrders = records.ToDictionary(d => d.Symbol);
			}

			using (var reader = new StreamReader("closedOrders.csv"))
			using (var csv = new CsvReader(reader))
			{
				var records = csv.GetRecords<PendingOrder>();
				ClosedOrders = records.ToList();
			}
		}

		public Dictionary<string, List<DateTime>> d_DateTimes = new Dictionary<string, List<DateTime>>();
		private void HitBTC_MessageReceived(string notification, string symbol)
		{
			if (notification == "updateCandles" && symbol != null)
			{
				var candle = HitBTC.d_Candle[symbol];
				if (!d_DateTimes.ContainsKey(symbol))
				{
					d_DateTimes.Add(symbol, new List<DateTime>());
					d_DateTimes[symbol].Add(candle.TimeStamp);
				}

				if (candle.TimeStamp > d_DateTimes[symbol].Last())
				{
					d_DateTimes[symbol][d_DateTimes[symbol].Count - 1] = candle.TimeStamp;

					d_lSmaFast[symbol].Add(SmaFast[symbol].NextValue(candle.Close));
					d_lSmaSlow[symbol].Add(SmaFast[symbol].NextValue(candle.Close));
					d_lRSI[symbol].Add(RSI[symbol].NextValue(candle.Close));
					d_lBB[symbol].Add(BB[symbol].NextValue(candle.Close));
					d_DateTimes[symbol].Add(candle.TimeStamp);
				}
				else
				{
					d_lSmaFast[symbol][d_lSmaFast[symbol].Count - 1] = SmaFast[symbol].Value(candle.Close);
					d_lSmaSlow[symbol][d_lSmaSlow[symbol].Count - 1] = SmaSlow[symbol].Value(candle.Close);
					d_lRSI[symbol][d_lRSI[symbol].Count - 1] = RSI[symbol].Value(candle.Close);
					d_lBB[symbol][d_lBB[symbol].Count - 1] = BB[symbol].Value(candle.Close);
				}
			}
			else if (notification == "snapshotCandles" && symbol != null)
			{
				if (!SmaFast.ContainsKey(symbol))
					SmaFast.Add(symbol, new SMA(5));

				if (!SmaSlow.ContainsKey(symbol))
					SmaSlow.Add(symbol, new SMA(50));

				if (!d_lSmaFast.ContainsKey(symbol))
					d_lSmaFast.Add(symbol, new List<decimal>());

				if (!d_lSmaSlow.ContainsKey(symbol))
					d_lSmaSlow.Add(symbol, new List<decimal>());

				if (!d_lRSI.ContainsKey(symbol))
					d_lRSI.Add(symbol, new List<decimal>());

				if (!d_lBB.ContainsKey(symbol))
					d_lBB.Add(symbol, new List<(decimal Sma, decimal BU, decimal BD)>());

				if (!d_DateTimes.ContainsKey(symbol))
					d_DateTimes.Add(symbol, new List<DateTime>());

				foreach (var candle in HitBTC.Candles[symbol])
				{
					d_lSmaFast[symbol].Add(SmaFast[symbol].NextValue(candle.Close));
					d_lSmaSlow[symbol].Add(SmaSlow[symbol].NextValue(candle.Close));
					d_lRSI[symbol].Add(RSI[symbol].NextValue(candle.Close));
					d_lBB[symbol].Add(BB[symbol].NextValue(candle.Close));
					d_DateTimes[symbol].Add(candle.TimeStamp);
				}
			}
		}

		private void HitBTC_Closed(string s, string ss)
		{
			Save(TradingDataFileName);

			Thread.Sleep(5000);
			HitBTC.SocketConnect();
			Thread.Sleep(5000);

			Console.Clear();
			HitBTC.SocketAuth.Auth();
			HitBTC.SocketMarketData.GetSymbols();

			foreach (var p in OrdersParameters)
			{
				//HitBTC.SocketMarketData.SubscribeTicker(p.Key);
				HitBTC.SocketMarketData.SubscribeCandles(p.Key, period: p.Value.Period, 500);
				Thread.Sleep(200);
				//HitBTC.SocketMarketData.SubscribeTrades(p.Key, 10);
			}
		}
	}

	public static class DecimalExtension
	{
		public static decimal Percent(this decimal number, decimal percent)
		{
			//return ((double) 80         *       25)/100;
			return (number * percent) / 100.0m;
		}
	}
}
