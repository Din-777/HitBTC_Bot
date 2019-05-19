using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using HitBTC;
using HitBTC.Models;

namespace Trading
{
	public enum Type
	{
		New,
		Closed,
		Deleted,
		Processed,
		Additional
	}

	[Serializable]
	public class Sma
	{
		int _period = 0;
		Queue<decimal> _queue;
		public int Period = 1;

		public Sma()
		{
			_period = Period;
			_queue = new Queue<decimal>(Period);
		}

		public decimal Compute(decimal x)
		{
			_period = Period;

			if (_queue.Count >= _period)
			{
				_queue.Dequeue();
			}

			_queue.Enqueue(x);
			return _queue.Average();
		}
	}

	[Serializable]
	public class PendingOrder : IComparable<PendingOrder>
	{
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

		public Sma SmaCurrProfitPercent = new Sma();
		public Sma SmaMaxProfitPercent = new Sma();

		public decimal CurrProfitPercentSma = 0.0m;
		public decimal MaxProfitPercentSma = 0.0m;   

		public decimal CalcCurrProfitPercent(Ticker ticker)
		{
			CurrProfitPercent = ((100.0m / (OpenPrice / (Side == "sell" ? ticker.Bid : ticker.Ask))) - 100.0m) *
				(Side == "sell" ? 1.0m : (-1.0m));

			if (CurrProfitPercent > MaxProfitPercent) MaxProfitPercent = CurrProfitPercent;

			SmaCurrProfitPercent.Period = 20;
			SmaMaxProfitPercent.Period = 20;

			CurrProfitPercentSma = SmaCurrProfitPercent.Compute(CurrProfitPercent);
			MaxProfitPercentSma = SmaMaxProfitPercent.Compute(MaxProfitPercent);
			return CurrProfitPercent;
		}

		public PendingOrder() { StaticId += 1; Id = StaticId;  }

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
	public class OrderParameter
	{
		public decimal StartingQuantity;
		public decimal Quantity;
		public decimal StopPercent;
		public decimal ClosePercent;
	}

	[Serializable]
	class SaveObject
	{
		public List<PendingOrder> ClosedOrders;
		public Dictionary<string, decimal> DemoBalance;
		public Dictionary<string, OrderParameter> OrderOptions;
		public Dictionary<string, List<PendingOrder>> PendingOrders;
	}

	public class DemoBalance
	{
		public string Currency { get; set; }
		public decimal Available { get; set; }
	}
	
	public class Trading
	{
		public Ticker Ticker;

		private HitBTCSocketAPI HitBTC;

		public Dictionary<string, List<PendingOrder>> PendingOrders;
		public List<PendingOrder> ClosedOrders;
		public Dictionary<string, OrderParameter> OrdersParameter;
		public Dictionary<string, decimal> DemoBalance;

		public Trading(ref HitBTCSocketAPI hitBTC)
		{
			this.HitBTC = hitBTC;
			this.PendingOrders = new Dictionary<string, List<PendingOrder>>();
			this.ClosedOrders = new List<PendingOrder>();
			this.OrdersParameter = new Dictionary<string, OrderParameter>();
			this.DemoBalance = new Dictionary<string, decimal>();
			HitBTC.Closed += HitBTC_Closed;
		}

		public void Add(string symbol, decimal startingQuantity, decimal treadingQuantity, decimal stopPercent, decimal closePercent)
		{
			if (!OrdersParameter.ContainsKey(symbol))
				OrdersParameter.Add(symbol, new OrderParameter());

			OrdersParameter[symbol] = new OrderParameter
			{
				Quantity = treadingQuantity * HitBTC.Symbols[symbol].QuantityIncrement,
				StartingQuantity = startingQuantity * HitBTC.Symbols[symbol].QuantityIncrement,
				StopPercent = stopPercent,
				ClosePercent = closePercent
			};
			HitBTC.SocketMarketData.SubscribeTicker(symbol);
		}

		public PendingOrder PendingOrderAdd(string side, Ticker ticker)
		{
			string symbol = ticker.Symbol;
			PendingOrder PendingOrder;

			if (side == "sell")
			{
				if (!PendingOrders.ContainsKey(symbol))
					PendingOrders.Add(symbol, new List<PendingOrder>());

				PendingOrder = new PendingOrder
				{
					Side = "sell",
					Symbol = symbol,
					OpenPrice = ticker.Bid,
					Quantity = OrdersParameter[symbol].Quantity,
					StopPercent = OrdersParameter[symbol].StopPercent,
					ClosePercent = OrdersParameter[symbol].ClosePercent
				};
			}
			else
			{
				if (!PendingOrders.ContainsKey(Ticker.Symbol))
					PendingOrders.Add(symbol, new List<PendingOrder>());

				PendingOrder = new PendingOrder
				{
					Side = "buy",
					Symbol = symbol,
					OpenPrice = ticker.Ask,
					Quantity = OrdersParameter[symbol].Quantity,
					StopPercent = OrdersParameter[symbol].StopPercent,
					ClosePercent = OrdersParameter[symbol].ClosePercent
				};
			}
			PendingOrders[symbol].Add(PendingOrder);
			return PendingOrder;

		}

		public void Run_1(Ticker ticker = null)
		{
			if (ticker == null)
				Ticker = HitBTC.Ticker;
			else Ticker = ticker;

			string symbol = Ticker.Symbol;

			if (!PendingOrders.ContainsKey(symbol))
			{
				if (Buy(symbol, Ticker.Ask, OrdersParameter[symbol].Quantity * 2))
					PendingOrderAdd("sell", Ticker);
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
						if ((Ticker.Bid > PendingOrders[symbol][i].ClosePrice) &&
							 (PendingOrders[symbol][i].CurrProfitPercent < (PendingOrders[symbol][i].MaxProfitPercent * 0.9m)))
						{
							if (Sell(symbol, Ticker.Bid, PendingOrders[symbol][i].Quantity))
							{
								ClosedOrders.Add(PendingOrders[symbol].ElementAtOrDefault(i));

								PendingOrders[symbol].RemoveAt(i);

								PendingOrderAdd("buy", Ticker);
							}
							else PendingOrders[symbol].ElementAtOrDefault(i).Closed = true;
						}
						else if (Ticker.Bid < PendingOrders[symbol][i].ClosePrice) PendingOrders[symbol][i].MaxProfitPercent = 0.0m;
						else if (Ticker.Bid < PendingOrders[symbol][i].StopPrice)
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
						if ((Ticker.Ask < PendingOrders[symbol][i].ClosePrice) &&
							 (PendingOrders[symbol][i].CurrProfitPercent < (PendingOrders[symbol][i].MaxProfitPercent * 0.9m)))
						{
							if (Buy(symbol, Ticker.Ask, PendingOrders[symbol][i].Quantity))
							{
								ClosedOrders.Add(PendingOrders[symbol].ElementAtOrDefault(i));

								PendingOrders[symbol].RemoveAt(i);

								PendingOrderAdd("sell", Ticker);
							}
							else PendingOrders[symbol].ElementAtOrDefault(i).Closed = true;
						}
						else if (Ticker.Ask > PendingOrders[symbol][i].ClosePrice) PendingOrders[symbol][i].MaxProfitPercent = 0.0m;
						else if (Ticker.Ask > PendingOrders[symbol][i].StopPrice)
						{
							PendingOrders[symbol].RemoveAt(i);
						}
						else PendingOrders[symbol].ElementAtOrDefault(i).Closed = false;
					}
				}

				if (!PendingOrders[symbol].Any(t => t.Side == "sell"))
					PendingOrderAdd("sell", Ticker);
				if (!PendingOrders[symbol].Any(t => t.Side == "buy"))
					PendingOrderAdd("buy", Ticker);
			}
		}

		public void Run_2(Ticker ticker = null)
		{
			if (ticker == null)
				Ticker = HitBTC.Ticker;
			else Ticker = ticker;

			string symbol = Ticker.Symbol;

			if (DemoBalance[symbol.Substring(0, 3)] == 0)
			{
				if (Buy(symbol, Ticker.Ask, OrdersParameter[symbol].Quantity * 1))
					PendingOrderAdd("sell", Ticker);
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
						if ((Ticker.Bid > PendingOrders[symbol][i].ClosePrice) &&
							 (PendingOrders[symbol][i].CurrProfitPercent < (PendingOrders[symbol][i].MaxProfitPercent * 0.7m)))
						{
							if (Sell(symbol, Ticker.Bid, PendingOrders[symbol][i].Quantity))
							{
								ClosedOrders.Add(PendingOrders[symbol].ElementAtOrDefault(i));
								ClosedOrders.Last().ClosePrice = Ticker.Bid;
								PendingOrders[symbol].RemoveAt(i);
								PendingOrderAdd("buy", Ticker);
							}
							else PendingOrders[symbol].ElementAtOrDefault(i).Closed = true;
						}						
						else if (Ticker.Bid < PendingOrders[symbol][i].StopPrice)
						{	
							PendingOrders[symbol].RemoveAt(i);
						}
						else if (Ticker.Bid < PendingOrders[symbol].ElementAtOrDefault(i).ClosePrice)
							PendingOrders[symbol].ElementAtOrDefault(i).MaxProfitPercent = 0.0m;
						else PendingOrders[symbol].ElementAtOrDefault(i).Closed = false;						
					}
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if (PendingOrders[symbol][i].Side == "buy")
					{
						if ((Ticker.Ask < PendingOrders[symbol][i].ClosePrice) &&
							 (PendingOrders[symbol][i].CurrProfitPercent < (PendingOrders[symbol][i].MaxProfitPercent * 0.7m)))
						{
							if (Buy(symbol, Ticker.Ask, PendingOrders[symbol][i].Quantity))
							{
								ClosedOrders.Add(PendingOrders[symbol].ElementAtOrDefault(i));
								ClosedOrders.Last().ClosePrice = Ticker.Ask;
								PendingOrders[symbol].RemoveAt(i);
								i -= 1;
								PendingOrderAdd("sell", Ticker);
							}
							else PendingOrders[symbol].ElementAtOrDefault(i).Closed = true;
						}
						else if (Ticker.Ask > PendingOrders[symbol][i].StopPrice)
						{
							PendingOrders[symbol].RemoveAt(i);
							i -= 1;
						}
						else if (Ticker.Ask > PendingOrders[symbol].ElementAtOrDefault(i).ClosePrice)
							PendingOrders[symbol].ElementAtOrDefault(i).MaxProfitPercent = 0.0m;
						else PendingOrders[symbol].ElementAtOrDefault(i).Closed = false;						
					}
				}

				if (!PendingOrders[symbol].Any(t => t.Side == "sell"))
					PendingOrderAdd("sell", Ticker);
				if (!PendingOrders[symbol].Any(t => t.Side == "buy"))
					PendingOrderAdd("buy", Ticker);
			}
		}

		public void Run_3(Ticker ticker = null)
		{
			if (ticker == null)
				Ticker = HitBTC.Ticker;
			else Ticker = ticker;

			string symbol = Ticker.Symbol;

			if (!PendingOrders.ContainsKey(symbol))
			{
				PendingOrderAdd("buy", Ticker).Type = Type.New;
			}
			else
			{
				foreach (var PendingOrder in PendingOrders[symbol])
				{
					PendingOrder.CalcCurrProfitPercent(Ticker);
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if (PendingOrders[symbol][i].Side == "sell")
					{
						if (Ticker.Bid > PendingOrders[symbol][i].ClosePrice)
						{
							if (PendingOrders[symbol][i].CurrProfitPercentSma < (PendingOrders[symbol][i].MaxProfitPercentSma * 0.8m))
							{
								if (Sell(symbol, Ticker.Bid, PendingOrders[symbol][i].Quantity))
								{
									ClosedOrders.Add(PendingOrders[symbol].ElementAt(i));
									ClosedOrders.Last().ClosePrice = Ticker.Bid;
									PendingOrders[symbol].ElementAt(i).Type = Type.Closed;
									if(PendingOrders[symbol].ElementAt(i).Type != Type.Additional)
										PendingOrderAdd("buy", Ticker).Type = Type.New;	
								}
								else if (PendingOrders[symbol].ElementAt(i).Type == Type.Additional)
									PendingOrders[symbol].ElementAt(i).Type = Type.Deleted;
								else PendingOrders[symbol].ElementAt(i).Closed = true;
							}
							else if(PendingOrders[symbol].ElementAt(i).Type == Type.New)
							{
								PendingOrders[symbol].ElementAt(i).Type = Type.Processed;
								PendingOrderAdd("sell", Ticker).Type = Type.Additional;
							}
						}
						else if (Ticker.Bid < PendingOrders[symbol][i].StopPrice)
						{
							//if (Sell(symbol, Ticker.Bid, PendingOrders[symbol][i].Quantity))
							//{
								ClosedOrders.Add(PendingOrders[symbol].ElementAt(i));
								ClosedOrders.Last().ClosePrice = Ticker.Bid;
								PendingOrders[symbol].ElementAt(i).Type = Type.Closed;
								PendingOrderAdd("buy", Ticker).Type = Type.New;
							//}
						}
						else if (Ticker.Bid < PendingOrders[symbol].ElementAt(i).ClosePrice)
							PendingOrders[symbol].ElementAtOrDefault(i).MaxProfitPercent = 0.0m;
						else PendingOrders[symbol].ElementAtOrDefault(i).Closed = false;
					}
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if (PendingOrders[symbol][i].Side == "buy")
					{
						if (Ticker.Ask < PendingOrders[symbol][i].ClosePrice)							
						{
							if (PendingOrders[symbol][i].CurrProfitPercentSma < (PendingOrders[symbol][i].MaxProfitPercentSma * 0.8m))
							{
								if (Buy(symbol, Ticker.Ask, PendingOrders[symbol][i].Quantity))
								{
									ClosedOrders.Add(PendingOrders[symbol].ElementAt(i));
									ClosedOrders.Last().ClosePrice = Ticker.Ask;
									PendingOrders[symbol].ElementAt(i).Type = Type.Closed;
								if (PendingOrders[symbol].ElementAt(i).Type != Type.Additional)
									PendingOrderAdd("sell", Ticker).Type = Type.New;
								}
								else if (PendingOrders[symbol].ElementAt(i).Type == Type.Additional)
									PendingOrders[symbol].ElementAt(i).Type = Type.Deleted;
								else PendingOrders[symbol].ElementAtOrDefault(i).Closed = true;
							}
							else if (PendingOrders[symbol].ElementAt(i).Type == Type.New)
							{
								PendingOrders[symbol].ElementAt(i).Type = Type.Processed;
								PendingOrderAdd("buy", Ticker).Type = Type.Additional;
							}
						}
						if (Ticker.Ask > PendingOrders[symbol][i].StopPrice)
						{
							if (PendingOrders[symbol].ElementAt(i).Type == Type.Additional)
								PendingOrders[symbol].ElementAt(i).Type = Type.Deleted;
						}
						else if (Ticker.Ask > PendingOrders[symbol].ElementAt(i).ClosePrice)
							PendingOrders[symbol].ElementAtOrDefault(i).MaxProfitPercent = 0.0m;
						else PendingOrders[symbol].ElementAt(i).Closed = false;
					}
				}

				for (int i = 0; i < PendingOrders[symbol].Count; i++)
				{
					if ( (PendingOrders[symbol].ElementAt(i).Type == Type.Deleted) ||
						(PendingOrders[symbol].ElementAtOrDefault(i).Type == Type.Closed) )
					{
						PendingOrders[symbol].RemoveAt(i);
						i -= 1;
					}
				}

				/*if (!PendingOrders[symbol].Any(t => t.Side == "sell"))
					PendingOrderAdd("sell", Ticker).Type = Type.Additional;
				if (!PendingOrders[symbol].Any(t => t.Side == "buy"))
					PendingOrderAdd("buy", Ticker).Type = Type.Additional;*/
			}
		}

		public bool Sell(string symbol, decimal price, decimal quantity, bool test = false)
		{
			//HitBTC.SocketTrading.PlaceNewOrder(symbol, "sell", quantity);

			string baseCurrency  = symbol.Substring(0, symbol.Length - 3);
			string quoteCurrency = symbol.Substring(symbol.Length - 3, 3);

			if (DemoBalance[baseCurrency] < HitBTC.Symbols[symbol].QuantityIncrement)
				return false;

			if (quantity > DemoBalance[baseCurrency])
				quantity = DemoBalance[baseCurrency];


			decimal realQuoteCurrency = quantity * price * (1 - HitBTC.Symbols[symbol].TakeLiquidityRate);
			//float real = quantity * price * (1 - 0.001f);

			if (test == false)
			{
					DemoBalance[baseCurrency] -= quantity;
					DemoBalance[quoteCurrency] += realQuoteCurrency;
			}
			return true;
		}

		public bool Buy(string symbol, decimal price, decimal quantity, bool test = false)
		{
			//HitBTC.SocketTrading.PlaceNewOrder(symbol, "buy", quantity);
			string baseCurrency = symbol.Substring(0, symbol.Length - 3);
			string quoteCurrency = symbol.Substring(symbol.Length - 3, 3);

			if (DemoBalance[quoteCurrency] < (HitBTC.Symbols[symbol].QuantityIncrement * price))
				return false;

			decimal realQuoteCurrency = 0;
			if (DemoBalance[quoteCurrency] < (quantity * price * (1 - HitBTC.Symbols[symbol].TakeLiquidityRate)))
			{
				decimal realPrice = price * (1 + HitBTC.Symbols[symbol].TakeLiquidityRate);
				quantity = (DemoBalance[quoteCurrency] / realPrice) - ( (DemoBalance[quoteCurrency] / realPrice) % HitBTC.Symbols[symbol].QuantityIncrement );
				//real = quantity * price * (1 - 0.001m);
			}
			realQuoteCurrency = quantity * price * (1 + HitBTC.Symbols[symbol].TakeLiquidityRate);

			if (test == false)
			{
				DemoBalance[baseCurrency] += quantity;
				DemoBalance[quoteCurrency] -= realQuoteCurrency;
			}
			return true;
		}


		public void Save(string fileNeme)
		{
			BinaryFormatter formatter = new BinaryFormatter();

			using (FileStream fs = new FileStream(fileNeme, FileMode.OpenOrCreate))
			{
				formatter.Serialize(fs, new SaveObject
				{
					DemoBalance = DemoBalance,
					ClosedOrders = ClosedOrders,
					OrderOptions = OrdersParameter,
					PendingOrders = PendingOrders
				});
			}
		}

		public bool Load(string fileNeme)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			try
			{
				using (FileStream fs = new FileStream(fileNeme, FileMode.OpenOrCreate))
				{
					SaveObject saveObject = (SaveObject)formatter.Deserialize(fs);

					DemoBalance = saveObject.DemoBalance;
					ClosedOrders = saveObject.ClosedOrders;
					OrdersParameter = saveObject.OrderOptions;
					PendingOrders = saveObject.PendingOrders;

					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		private void HitBTC_Closed(string s)
		{
			Thread.Sleep(5000);			
			HitBTC.SocketConnect();
			Thread.Sleep(5000);

			Console.Clear();
			HitBTC.SocketAuth.Auth();
			HitBTC.SocketMarketData.GetSymbols();

			foreach (var v in OrdersParameter)
				HitBTC.SocketMarketData.SubscribeTicker(v.Key);

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
