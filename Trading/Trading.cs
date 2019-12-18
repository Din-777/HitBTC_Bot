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

	public class OrderBuy
	{
		public string Symbol;
		public decimal BuyPrice;
	}

	public class OrderSell
	{
		public string Symbol;
		public decimal SellPrice;
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
		public Dictionary<string, OrderBuy>  d_OrdersBuy;
		public Dictionary<string, OrderSell> d_OrdersSell;

		public Dictionary<string, PendingOrder> PendingOrders;
		public List<PendingOrder> ClosedOrders;
		public Dictionary<string, OrderParametr> OrdersParameters;
		public Dictionary<string, Balance> DemoBalance;

		private HitBTCSocketAPI HitBTC;
		public bool DataFileIsloaded = false;
		private string TradingDataFileName;
		public bool Demo = true;

		private Timer timer;
		private DateTime DateTimeStart;
		private DateTime DateTimeStartCurr;

		public Dictionary<string, Strategies.SafetyOrders> d_Strategies;

		private void TimerTick(object obj)
		{
			string start = DateTimeStart.ToString();
			string elapsedTotalTime   = (DateTime.Now - DateTimeStart).ToString(@"dd\ hh\:mm\:ss");
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
				timer = new Timer(new TimerCallback(TimerTick), null, 0, 1000);

			this.HitBTC = hitBTC;
			this.PendingOrders = new Dictionary<string, PendingOrder>();
			this.OrdersParameters = new Dictionary<string, OrderParametr>();
			this.DemoBalance = new Dictionary<string, Balance>();
			this.ClosedOrders = new List<PendingOrder>();

			this.d_OrdersBuy  = new Dictionary<string, OrderBuy>();
			this.d_OrdersSell = new Dictionary<string, OrderSell>();

			d_Strategies = new Dictionary<string, Strategies.SafetyOrders>();

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

			if(!d_Strategies.ContainsKey(symbol))
				d_Strategies.Add(symbol, new Strategies.SafetyOrders(0));

			//HitBTC.SocketMarketData.SubscribeTicker(symbol);
			Thread.Sleep(10);
			//HitBTC.SocketMarketData.SubscribeTrades(symbol, 5000);
			Thread.Sleep(10);
			HitBTC.SocketMarketData.SubscribeCandles(symbol, period, 1000);
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

				d_Strategies[symbol] = new Strategies.SafetyOrders(openPrice: price, takeProfitPercent: 1, stopLosePercent: 7);

				if (!PendingOrders.ContainsKey(symbol))
					PendingOrders.Add(symbol, PendingOrder);
				else PendingOrders[symbol] = PendingOrder;
			}
			return PendingOrder;
		}

		public void Run_1_RSIStrategy(string symbol, Candle candle)
		{
			var price = candle.Close;
			if (!PendingOrders.ContainsKey(symbol))
			{
				PendingOrderAdd(symbol, "buy", price).Type = Type.First;
			}
			else
			{
				PendingOrders[symbol].CalcCurrProfitPercent(price);
				var signal = d_Strategies[symbol].Update(price: price);

				if (PendingOrders[symbol].Side == "buy")
				{
					_Buy(PendingOrders[symbol], price: price);
					d_Strategies[symbol] = new Strategies.SafetyOrders(price);
				}
				else if (PendingOrders[symbol].Side == "sell")
				{
					if(signal == Strategies.Signal.Sell)
						_Sell(pendingOrder: PendingOrders[symbol], price: price);

					if (signal == Strategies.Signal.Buy)
					{
						var result = Buy(symbol: symbol, price: price,
							quantityInQuote: PendingOrders[symbol].QuantityInUSDBuy);
						if (result.IsBuy)
						{
							PendingOrders[symbol].QuantityInUSDBuy += result.QuantityQuote;
							PendingOrders[symbol].Quantity += result.QuantityBase;
						}
					}
				}
			}

			if ((PendingOrders[symbol].Type == Type.Deleted) ||
				(PendingOrders[symbol].Type == Type.Closed))
			{
				PendingOrders.Remove(symbol);
			}

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

				d_Strategies[symbol].Update(candle);

				if (!d_DateTimes.ContainsKey(symbol))
				{
					d_DateTimes.Add(symbol, new List<DateTime>());
					d_DateTimes[symbol].Add(candle.TimeStamp);
				}

				if (candle.TimeStamp > d_DateTimes[symbol].Last())
				{
					d_DateTimes[symbol].Add(candle.TimeStamp);
				}
				else
				{
				}
			}
			else if (notification == "snapshotCandles" && symbol != null)
			{
				if (!d_DateTimes.ContainsKey(symbol))
					d_DateTimes.Add(symbol, new List<DateTime>());

				d_DateTimes[symbol].Clear();
				foreach (var candle in HitBTC.Candles[symbol])
				{
					d_DateTimes[symbol].Add(candle.TimeStamp);
					d_Strategies[symbol].Update(candle);
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
