using HitBTC;
using HitBTC.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Trading.Utilities;


namespace Trading
{
	public enum Type
	{
		New,
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
		public DateTime DateTime;
		public int Id;
		public static int StaticId = 0;
		public string Symbol;
		public string Side;
		public decimal Quantity;
		public decimal OpenPrice;
		public decimal StopPrice;
		public decimal ClosePrice;

		public bool Closed = false;

		public Type Type = Type.New;

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

		public decimal CurrProfitPercent = 0.0m;
		public decimal MaxProfitPercent = 0.0m;
		public decimal CurrProfitInUSD = 0.0m;
		public decimal QuantityInUSDBuy = 0.0m;
		public decimal QuantityInUSDSell = 0.0m;
		public decimal SmaMaxPrice = 0.0m;
		public decimal SmaMinPrice = 999999.0m;

		public decimal CalcCurrProfitPercent(Ticker ticker, decimal smaPrice = 0.0m)
		{
			CurrProfitPercent = ((100.0m / (OpenPrice / (Side == "sell" ? ticker.Bid : ticker.Ask))) - 100.0m) *
				(Side == "sell" ? 1.0m : (-1.0m));

			if (CurrProfitPercent > MaxProfitPercent) MaxProfitPercent = CurrProfitPercent;

			if (smaPrice > SmaMaxPrice) SmaMaxPrice = smaPrice;
			if (smaPrice < SmaMinPrice) SmaMinPrice = smaPrice;

			return CurrProfitPercent;
		}

		public decimal CalcCurrProfitPercent(decimal price)
		{
			CurrProfitPercent = ((100.0m / (OpenPrice / price)) - 100.0m) *
				(Side == "sell" ? 1.0m : (-1.0m));

			if (CurrProfitPercent > MaxProfitPercent) MaxProfitPercent = CurrProfitPercent;

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
		public decimal QuantityQuoteCurrency;
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
		public Dictionary<string, List<PendingOrder>> PendingOrders;
	}

	public class DemoBalance
	{
		public string Currency { get; set; }
		public decimal Available { get; set; }
		public decimal Reserved { get; set; }
	}

	public class Trading
	{
		public Dictionary<string, SMA> SmaSlow = new Dictionary<string, SMA>();
		public Dictionary<string, SMA> SmaFast = new Dictionary<string, SMA>();		
		public Dictionary<string, List<PendingOrder>> PendingOrders;
		public List<PendingOrder> ClosedOrders;
		public Dictionary<string, OrderParametr> OrdersParameters;
		public Dictionary<string, Balance> DemoBalance;

		public Dictionary<string, List<decimal>> d_lSmaSlow;
		public Dictionary<string, List<decimal>> d_lSmaFast;

		private HitBTCSocketAPI HitBTC;
		public bool DataFileIsloaded = false;
		private string TradingDataFileName;

		private Timer timer;
		private DateTime DateTimeStart;
		private DateTime DateTimeStartCurr;

		private void TimerTick(object obj)
		{
			string start = DateTimeStart.ToString();
			string elapsedTotalTime = (DateTime.Now - DateTimeStart).ToString(@"dd\ hh\:mm\:ss");
			string elapsedTCurrentTime = (DateTime.Now - DateTimeStartCurr).ToString(@"dd\ hh\:mm\:ss");

			Console.Title = String.Format("Start: {0}  Total work time: {1}  Current work time {2}  Current time {3}",
				start, elapsedTotalTime, elapsedTCurrentTime, DateTime.Now.ToString("HH:mm:ss"));
		}

		public Trading(ref HitBTCSocketAPI hitBTC, bool console = true)
		{
			DateTimeStart = DateTime.Now;
			DateTimeStartCurr = DateTime.Now;
			if (console)
				timer = new Timer(new TimerCallback(TimerTick), null, 0, 500);

			this.HitBTC = hitBTC;
			this.PendingOrders = new Dictionary<string, List<PendingOrder>>();
			this.OrdersParameters = new Dictionary<string, OrderParametr>();
			this.DemoBalance = new Dictionary<string, Balance>();
			this.ClosedOrders = new List<PendingOrder>();
			d_lSmaSlow = new Dictionary<string, List<decimal>>();
			d_lSmaFast = new Dictionary<string, List<decimal>>();

			HitBTC.Closed += HitBTC_Closed;
			HitBTC.MessageReceived += HitBTC_MessageReceived;
		}

		public void Add(string symbol, Period period, decimal treadingQuantity, decimal stopPercent, decimal closePercent, int SmaPeriodFast = 5, int SmaPeriodSlow = 50)
		{
			if (!OrdersParameters.ContainsKey(symbol))
				OrdersParameters.Add(symbol, new OrderParametr());

			OrdersParameters[symbol] = new OrderParametr
			{
				QuantityQuoteCurrency = treadingQuantity,
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

			//HitBTC.SocketMarketData.SubscribeTicker(symbol);
			Thread.Sleep(10);
			//HitBTC.SocketMarketData.SubscribeTrades(symbol, 5000);
			Thread.Sleep(10);
			HitBTC.SocketMarketData.SubscribeCandles(symbol, period, 1000);
			Thread.Sleep(10);
		}

		public PendingOrder PendingOrderAdd(string side, Ticker ticker)
		{
			string symbol = ticker.Symbol;
			PendingOrder PendingOrder = null;

			if (OrdersParameters.ContainsKey(symbol))
			{
				string baseCurrency = HitBTC.Symbols[symbol].BaseCurrency;
				string quoteCurrency = HitBTC.Symbols[symbol].QuoteCurrency;

				if (side == "sell")
				{
					if (!PendingOrders.ContainsKey(symbol))
						PendingOrders.Add(symbol, new List<PendingOrder>());

					PendingOrder = new PendingOrder
					{
						Side = "sell",
						Symbol = symbol,
						OpenPrice = ticker.Ask,
						Quantity = OrdersParameters[symbol].QuantityQuoteCurrency,
						StopPercent = OrdersParameters[symbol].StopPercent,
						ClosePercent = OrdersParameters[symbol].ClosePercent
					};

					decimal price = ticker.Bid;
					decimal OrderParameterQuantity;
					decimal quantity = 0;

					if (quoteCurrency == "USD")
					{
						OrderParameterQuantity = OrdersParameters[symbol].QuantityQuoteCurrency;
						quantity = (OrderParameterQuantity / price) - ((OrderParameterQuantity / price) % HitBTC.Symbols[symbol].QuantityIncrement);
					}
					else
					{
						OrderParameterQuantity = OrdersParameters[symbol].QuantityQuoteCurrency / HitBTC.d_Tickers[String.Concat(quoteCurrency, "USD")].Ask;
						quantity = (OrderParameterQuantity / price) - ((OrderParameterQuantity / price) % HitBTC.Symbols[symbol].QuantityIncrement);
					}

					PendingOrder.Quantity = quantity;
				}
				else if (side == "buy")
				{
					if (!PendingOrders.ContainsKey(ticker.Symbol))
						PendingOrders.Add(symbol, new List<PendingOrder>());

					PendingOrder = new PendingOrder
					{
						Side = "buy",
						Symbol = symbol,
						OpenPrice = ticker.Ask,
						Quantity = OrdersParameters[symbol].QuantityQuoteCurrency,
						StopPercent = OrdersParameters[symbol].StopPercent,
						ClosePercent = OrdersParameters[symbol].ClosePercent
					};

					decimal price = ticker.Ask;
					decimal OrderParameterQuantity;
					decimal quantity = 0;

					if (quoteCurrency == "USD")
					{
						OrderParameterQuantity = OrdersParameters[symbol].QuantityQuoteCurrency;
						quantity = (OrderParameterQuantity / price) - ((OrderParameterQuantity / price) % HitBTC.Symbols[symbol].QuantityIncrement);
					}
					else
					{
						OrderParameterQuantity = OrdersParameters[symbol].QuantityQuoteCurrency / HitBTC.d_Tickers[String.Concat(quoteCurrency, "USD")].Ask;
						quantity = (OrderParameterQuantity / price) - ((OrderParameterQuantity / price) % HitBTC.Symbols[symbol].QuantityIncrement);
					}

					PendingOrder.Quantity = quantity;
				}

				PendingOrders[symbol].Add(PendingOrder);
			}
			return PendingOrder;
		}

		public PendingOrder PendingOrderAdd(string symbol, string side, decimal price)
		{
			PendingOrder PendingOrder = null;

			if (OrdersParameters.ContainsKey(symbol))
			{
				string baseCurrency = HitBTC.Symbols[symbol].BaseCurrency;
				string quoteCurrency = HitBTC.Symbols[symbol].QuoteCurrency;

				if (!PendingOrders.ContainsKey(symbol))
					PendingOrders.Add(symbol, new List<PendingOrder>());

				decimal OrderParameterQuantity;
				decimal quantityInBaseCurrency = 0;

				OrdersParameters[symbol].QuantityQuoteCurrency -= OrdersParameters[symbol].QuantityQuoteCurrency * HitBTC.Symbols[symbol].TakeLiquidityRate;

				OrderParameterQuantity = OrdersParameters[symbol].QuantityQuoteCurrency;
				quantityInBaseCurrency = (OrderParameterQuantity / price) - ((OrderParameterQuantity / price) % HitBTC.Symbols[symbol].QuantityIncrement);

				PendingOrder = new PendingOrder
				{
					Side = side,
					Symbol = symbol,
					OpenPrice = price,
					Quantity = quantityInBaseCurrency,
					StopPercent = OrdersParameters[symbol].StopPercent,
					ClosePercent = OrdersParameters[symbol].ClosePercent
				};

				PendingOrders[symbol].Add(PendingOrder);
			}
			return PendingOrder;
		}

		public void Run_6(string symbol, decimal price)
		{
			if (!PendingOrders.ContainsKey(symbol))
			{
				PendingOrderAdd(symbol, "buy", price).Type = Type.First;
			}
			else
			{
				foreach (var pendingOrder in PendingOrders[symbol])
				{
					pendingOrder.CalcCurrProfitPercent(price);
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					var SmaFastPrice = SmaFast[symbol].LastAverage;
					var SmaSlowPrice = SmaSlow[symbol].LastAverage;

					if (PendingOrders[symbol][i].Side == "buy")
					{
						if (SmaFastPrice > SmaSlowPrice
							&& d_lSmaSlow[symbol].Last() == d_lSmaSlow[symbol].ElementAt(d_lSmaSlow[symbol].Count - 1))
						{
							if (PendingOrders[symbol][i].Type == Type.Processed)
							{
								PendingOrders[symbol][i].Type = Type.Confirmed;
							}
							else if (PendingOrders[symbol][i].Type == Type.Confirmed)
							{
								var result = Buy(symbol, price, PendingOrders[symbol][i].Quantity);
								if (result.Item1 == true)
								{
									ClosedOrders.Add(PendingOrders[symbol][i]);
									ClosedOrders.Last().ClosePrice = price;
									ClosedOrders.Last().CurrProfitPercent = -HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;

									PendingOrders[symbol][i].Type = Type.Closed;

									PendingOrderAdd(symbol, "sell", price).Type = Type.New;
									PendingOrders[symbol].Last().QuantityInUSDBuy = result.Item2;
								}
								else
									PendingOrders[symbol][i].Type = Type.First;
							}
						}
						else if (SmaFastPrice < SmaSlowPrice && PendingOrders[symbol][i].Type == Type.First)
						{
							PendingOrders[symbol][i].Type = Type.New;
						}
						else if (SmaFastPrice < SmaSlowPrice && PendingOrders[symbol][i].Type == Type.Confirmed)
						{
							PendingOrders[symbol][i].Type = Type.Processed;
						}

					}
					else if (PendingOrders[symbol][i].Side == "sell")
					{
						if (SmaFastPrice < SmaSlowPrice && SmaFastPrice > PendingOrders[symbol][i].ClosePrice)
						{
							if (PendingOrders[symbol][i].Type == Type.Processed)
							{
								PendingOrders[symbol][i].Type = Type.Confirmed;
							}
							else if (PendingOrders[symbol][i].Type == Type.Confirmed)
							{
								var result = Sell(symbol, price, PendingOrders[symbol][i].Quantity);
								if (result.Item1 == true)
								{
									PendingOrders[symbol][i].QuantityInUSDSell = result.Item2;
									PendingOrders[symbol][i].CurrProfitInUSD = result.Item2 - PendingOrders[symbol][i].QuantityInUSDBuy;

									ClosedOrders.Add(PendingOrders[symbol].ElementAt(i));
									ClosedOrders.Last().ClosePrice = price;
									ClosedOrders.Last().CurrProfitPercent -= HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;

									PendingOrders[symbol].ElementAt(i).Type = Type.Closed;

									PendingOrderAdd(symbol, "buy", price).Type = Type.New;
								}
							}
						}
						else if (PendingOrders[symbol][i].Type == Type.Confirmed)
						{
							PendingOrders[symbol][i].Type = Type.Processed;
						}
						else if (SmaFastPrice < PendingOrders[symbol][i].StopPrice)
						{
							var result = Sell(symbol, price, PendingOrders[symbol][i].Quantity);
							if (result.Item1 == true)
							{
								PendingOrders[symbol][i].QuantityInUSDSell = result.Item2;
								PendingOrders[symbol][i].CurrProfitInUSD = result.Item2 - PendingOrders[symbol][i].QuantityInUSDBuy;

								ClosedOrders.Add(PendingOrders[symbol].ElementAt(i));
								ClosedOrders.Last().ClosePrice = price;
								ClosedOrders.Last().CurrProfitPercent -= HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;

								PendingOrders[symbol].ElementAt(i).Type = Type.Deleted;

								PendingOrderAdd(symbol, "buy", price).Type = Type.New;
							}
						}

						if (PendingOrders[symbol][i].CurrProfitPercent > 50)
						{
							var result = Sell(symbol, price, PendingOrders[symbol][i].Quantity);
							if (result.Item1 == true)
							{
								PendingOrders[symbol][i].QuantityInUSDSell = result.Item2;
								PendingOrders[symbol][i].CurrProfitInUSD = result.Item2 - PendingOrders[symbol][i].QuantityInUSDBuy;

								ClosedOrders.Add(PendingOrders[symbol].ElementAt(i));
								ClosedOrders.Last().ClosePrice = price;
								ClosedOrders.Last().CurrProfitPercent -= HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;

								PendingOrders[symbol].ElementAt(i).Type = Type.Closed;

								PendingOrderAdd(symbol, "buy", price).Type = Type.New;
							}
						}
					}
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if ((PendingOrders[symbol][i].Type == Type.Deleted) ||
						(PendingOrders[symbol][i].Type == Type.Closed))
					{
						PendingOrders[symbol].RemoveAt(i);
						i -= 1;
					}
					else if (PendingOrders[symbol][i].Type == Type.New)
						PendingOrders[symbol][i].Type = Type.Processed;
				}
			}
		}

		public void Run_7(string symbol, decimal price)
		{
			if (!PendingOrders.ContainsKey(symbol))
			{
				PendingOrderAdd(symbol, "buy", price).Type = Type.New;
			}
			else
			{
				foreach (var pendingOrder in PendingOrders[symbol])
				{
					pendingOrder.CalcCurrProfitPercent(price);
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					var SmaFastPrice = SmaFast[symbol].LastAverage;
					var SmaSlowPrice = SmaSlow[symbol].LastAverage;

					if (PendingOrders[symbol][i].Side == "buy")
					{
						if (SmaFastPrice > d_lSmaFast[symbol].ElementAt(d_lSmaFast[symbol].Count - 5))
						{
							var result = Buy(symbol, price, PendingOrders[symbol][i].Quantity);
							if (result.Item1 == true)
							{
								ClosedOrders.Add(PendingOrders[symbol][i]);
								ClosedOrders.Last().ClosePrice = price;
								ClosedOrders.Last().CurrProfitPercent = -HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;

								PendingOrders[symbol][i].Type = Type.Closed;

								PendingOrderAdd(symbol, "sell", price).Type = Type.New;
								PendingOrders[symbol].Last().QuantityInUSDBuy = result.Item2;
							}
						}

					}
					else if (PendingOrders[symbol][i].Side == "sell")
					{
						if (SmaFastPrice < d_lSmaFast[symbol].ElementAt(d_lSmaFast[symbol].Count - 5)
							&& SmaFastPrice > PendingOrders[symbol][i].ClosePrice)
						{
							var result = Sell(symbol, price, PendingOrders[symbol][i].Quantity);
							if (result.Item1 == true)
							{
								PendingOrders[symbol][i].QuantityInUSDSell = result.Item2;
								PendingOrders[symbol][i].CurrProfitInUSD = result.Item2 - PendingOrders[symbol][i].QuantityInUSDBuy;

								ClosedOrders.Add(PendingOrders[symbol].ElementAt(i));
								ClosedOrders.Last().ClosePrice = price;
								ClosedOrders.Last().CurrProfitPercent -= HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;

								PendingOrders[symbol].ElementAt(i).Type = Type.Closed;

								PendingOrderAdd(symbol, "buy", price).Type = Type.New;
							}
						}
						else if (SmaFastPrice < PendingOrders[symbol][i].StopPrice)
						{
							var result = Sell(symbol, price, PendingOrders[symbol][i].Quantity);
							if (result.Item1 == true)
							{
								PendingOrders[symbol][i].QuantityInUSDSell = result.Item2;
								PendingOrders[symbol][i].CurrProfitInUSD = result.Item2 - PendingOrders[symbol][i].QuantityInUSDBuy;

								ClosedOrders.Add(PendingOrders[symbol].ElementAt(i));
								ClosedOrders.Last().ClosePrice = price;
								ClosedOrders.Last().CurrProfitPercent -= HitBTC.Symbols[symbol].TakeLiquidityRate * 100.0m;

								PendingOrders[symbol].ElementAt(i).Type = Type.Deleted;

								PendingOrderAdd(symbol, "buy", price).Type = Type.New;
							}
						}
					}
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if ((PendingOrders[symbol][i].Type == Type.Deleted) ||
						(PendingOrders[symbol][i].Type == Type.Closed))
					{
						PendingOrders[symbol].RemoveAt(i);
						i -= 1;
					}
					else if (PendingOrders[symbol][i].Type == Type.New)
						PendingOrders[symbol][i].Type = Type.Processed;
				}
			}
		}

		public (bool, decimal) Sell(string symbol, decimal price, decimal quantity, bool demo = true)
		{
			HitBTC.SocketTrading.GetTradingBalance();
			var result = (false, 0.0m);

			if (demo == false)
				DemoBalance = HitBTC.Balance;

			string baseCurrency = HitBTC.Symbols[symbol].BaseCurrency;
			string quoteCurrency = HitBTC.Symbols[symbol].QuoteCurrency;
			decimal baseAvailable = DemoBalance[baseCurrency].Available;

			if (baseAvailable < HitBTC.Symbols[symbol].QuantityIncrement)
				return result;

			if (quantity > baseAvailable)
				quantity = baseAvailable;

			quantity -= quantity % HitBTC.Symbols[symbol].QuantityIncrement;

			if (quantity == 0)
				return result;

			decimal quantityQuoteCurrency = quantity * price * (1 - HitBTC.Symbols[symbol].TakeLiquidityRate);

			if (demo == false)
				HitBTC.SocketTrading.PlaceNewOrder(symbol, "sell", quantity);

			DemoBalance[baseCurrency].Available -= quantity;
			DemoBalance[quoteCurrency].Available += quantityQuoteCurrency;

			result.Item1 = true; result.Item2 = quantityQuoteCurrency;
			return result;
		}

		public (bool, decimal) Buy(string symbol, decimal price, decimal quantity, bool demo = true)
		{
			HitBTC.SocketTrading.GetTradingBalance();
			var result = (false, 0.0m);

			if (demo == false)
				DemoBalance = HitBTC.Balance;

			string baseCurrency = HitBTC.Symbols[symbol].BaseCurrency;
			string quoteCurrency = HitBTC.Symbols[symbol].QuoteCurrency;
			decimal realPrice = price * (1 + HitBTC.Symbols[symbol].TakeLiquidityRate);
			decimal quoteAvailable = DemoBalance[quoteCurrency].Available;

			if (quoteAvailable < (HitBTC.Symbols[symbol].QuantityIncrement * realPrice))
				return result;

			if (quoteAvailable < (quantity * price))
			{
				quantity = (quoteAvailable / realPrice) - ((quoteAvailable / realPrice) % HitBTC.Symbols[symbol].QuantityIncrement);
			}

			if (quantity == 0)
				return result;

			decimal quantityQuoteCurrency = quantity * realPrice;

			if (demo == false)
				HitBTC.SocketTrading.PlaceNewOrder(symbol, "buy", quantity);

			DemoBalance[baseCurrency].Available += quantity;
			DemoBalance[quoteCurrency].Available -= quantityQuoteCurrency;

			result.Item1 = true; result.Item2 = quantityQuoteCurrency;

			return result;
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
					return true;
				}
			}
			catch
			{
				return false;
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

					d_lSmaFast[symbol].Add(SmaFast[symbol].NextAverage(candle.Close));
					d_lSmaSlow[symbol].Add(SmaFast[symbol].NextAverage(candle.Close));
					d_DateTimes[symbol].Add(candle.TimeStamp);
				}
				else
				{
					d_lSmaFast[symbol][d_lSmaFast[symbol].Count - 1] = SmaFast[symbol].Average(candle.Close);
					d_lSmaSlow[symbol][d_lSmaSlow[symbol].Count - 1] = SmaSlow[symbol].Average(candle.Close);
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

				if(!d_DateTimes.ContainsKey(symbol))
					d_DateTimes.Add(symbol, new List<DateTime>());

				foreach (var candle in HitBTC.Candles[symbol])
				{
					d_lSmaFast[symbol].Add(SmaFast[symbol].NextAverage(candle.Close));
					d_lSmaSlow[symbol].Add(SmaSlow[symbol].NextAverage(candle.Close));
					d_DateTimes[symbol].Add(candle.TimeStamp);
				}
			}
		}

		private void HitBTC_Closed(string s, string ss)
		{
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
