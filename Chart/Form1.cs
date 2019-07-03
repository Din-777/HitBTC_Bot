using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Windows.Forms.DataVisualization.Charting;
using HitBTC;
using HitBTC.Models;
using HitBTC.Categories;
using Trading.Utilities;
using Trading;
using System.Threading;

namespace Chart
{
	public partial class Form1 : Form
	{
		string Symbol = "BTCUSD";

		static HitBTCSocketAPI HitBTC;

		static string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
		static string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";

		public Trading.Trading Trading;
		public string TradingDataFileName = "tr.dat";

		public Form1()
		{
			InitializeComponent();
		}

		private void CastomInitializeComponent()
		{
			chart1.Series.Add("Candles");
			chart1.Series["Candles"].ChartType = SeriesChartType.Candlestick;
			chart1.Series["Candles"].XValueType = ChartValueType.Time;
			chart1.Series["Candles"].CustomProperties = "PriceDownColor=Red, PriceUpColor=Green";
			chart1.Series["Candles"].YValueMembers = "Max,Min,Open,Close";
			chart1.Series["Candles"].XValueMember = "TimeStamp";
			chart1.Series["Candles"].BorderWidth = 2;
			chart1.Series["Candles"].IsXValueIndexed = true;

			chart1.Series.Add("SmaSlow");
			chart1.Series["SmaSlow"].ChartType = SeriesChartType.Line;
			chart1.Series["SmaSlow"].XValueType = ChartValueType.Time;
			chart1.Series["SmaSlow"].YValueMembers = "EMA";
			chart1.Series["SmaSlow"].XValueMember = "TimeStamp";
			chart1.Series["SmaSlow"].BorderWidth = 2;
			chart1.Series["SmaSlow"].IsXValueIndexed = true;
			chart1.Series["SmaSlow"].Color = Color.Violet;

			chart1.Series.Add("SmaFast");
			chart1.Series["SmaFast"].ChartType = SeriesChartType.Line;
			chart1.Series["SmaFast"].XValueType = ChartValueType.Time;
			chart1.Series["SmaFast"].YValueMembers = "EMA";
			chart1.Series["SmaFast"].XValueMember = "TimeStamp";
			chart1.Series["SmaFast"].BorderWidth = 2;
			chart1.Series["SmaFast"].IsXValueIndexed = true;
			chart1.Series["SmaFast"].Color = Color.Crimson;

			chart1.Series.Add("SMA");
			chart1.Series["SMA"].ChartType = SeriesChartType.Line;
			chart1.Series["SMA"].XValueType = ChartValueType.Time;
			chart1.Series["SMA"].YValueMembers = "EMA";
			chart1.Series["SMA"].XValueMember = "TimeStamp";
			chart1.Series["SMA"].BorderWidth = 2;
			chart1.Series["SMA"].IsXValueIndexed = true;
			chart1.Series["SMA"].Color = Color.Crimson;

			chart1.ChartAreas.Add("0");
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].BackColor = Color.Gainsboro;
			chart1.BackColor = Color.Gainsboro;

			chart1.MouseWheel += Chart1_MouseWheel;
			chart1.MouseClick += Chart1_MouseClick;
			chart1.MouseDown += chart1_MouseDown;
			chart1.MouseMove += chart1_MouseMove;
			chart1.MouseUp += chart1_MouseUp;
		}

		private void Form1_Shown(object sender, EventArgs e)
		{
			CastomInitializeComponent();

			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC, console: false);

			HitBTC.SocketMarketData.GetSymbols();
			while (HitBTC.MessageType != "getSymbol") { Thread.Sleep(1); }

			List<String> symbols = HitBTC.Symbols.Keys.Where(v => v.EndsWith("USD") || v.EndsWith("USDT")).ToList();
			comboBoxSymbols.DataSource = symbols;

			HitBTC.MessageReceived += HitBTC_MessageReceived;
		}

		private void HitBTC_MessageReceived(string notification, string symbol)
		{
			if (notification == "updateCandles" && symbol != null)
			{
				DataTable table = new DataTable();


				//textBox1.BeginInvoke((MethodInvoker)(() => { textBox1.AppendText(HitBTC.d_Candle[Symbol].Close.ToString() + "\r\n"); }));
				/*if(symbol == Symbol)
				{
					chart1.BeginInvoke((MethodInvoker)(() =>
					{
						chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol].ToList(), "TimeStamp", "Max,Min,Open,Close", "");
						chart1.Series["SmaFast"].Points.DataBindXY(Trading.d_DateTimes[symbol].ToList(), Trading.d_lSmaFast[Symbol].ToList());
						chart1.Series["SmaSlow"].Points.DataBindXY(Trading.d_DateTimes[symbol].ToList(), Trading.d_lSmaSlow[Symbol].ToList());
					}));
				}	*/
			}
		}

		private void Button1_Click(object sender, EventArgs e)
		{
		}

		private void ComboBoxSymbols_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.Symbol = comboBoxSymbols.SelectedItem.ToString();

			if (!Trading.DemoBalance.ContainsKey("USD"))
				Trading.DemoBalance.Add("USD", new Balance());
			Trading.DemoBalance["USD"].Available = 100.0m;

			if (!Trading.DemoBalance.ContainsKey(HitBTC.Symbols[Symbol].BaseCurrency))
				Trading.DemoBalance.Add(HitBTC.Symbols[Symbol].BaseCurrency, new Balance());
			Trading.DemoBalance[HitBTC.Symbols[Symbol].BaseCurrency].Available = 0.0m;

			if (Trading.ClosedOrders != null)
				Trading.ClosedOrders.Clear();
			if(Trading.PendingOrders.ContainsKey(Symbol))
				Trading.PendingOrders.Remove(Symbol);			

			textBox1.BeginInvoke((MethodInvoker)(() =>
			{
				textBox1.Clear();
			}));

			if (HitBTC.Candles.ContainsKey(Symbol))
				HitBTC.Candles.Clear();
			if (HitBTC.d_Candle.ContainsKey(Symbol))
				HitBTC.d_Candle.Clear();

			HitBTC.ReceiveMessages(true);
			Trading.Add(symbol: Symbol, period: Period.M5, tradingQuantityInPercent: 100.0m, stopPercent: 20.0m, closePercent: 2.0m,
				SmaPeriodFast: 50, SmaPeriodSlow: 5);			

			while (!HitBTC.Candles.ContainsKey(Symbol)) Thread.Sleep(1);
			HitBTC.SocketMarketData.UnSubscribeCandles(symbol: Symbol);
			HitBTC.ReceiveMessages(false);

			Thread.Sleep(200);

			if (Trading.d_DateTimes.ContainsKey(Symbol))
				Trading.d_DateTimes[Symbol].Clear();
			if (Trading.d_lSmaFast.ContainsKey(Symbol))
				Trading.d_lSmaFast[Symbol].Clear();
			if (Trading.d_lSmaSlow.ContainsKey(Symbol))
				Trading.d_lSmaSlow[Symbol].Clear();
			if (Trading.SmaFast.ContainsKey(Symbol))
				Trading.SmaFast[Symbol].Clear();
			if (Trading.SmaSlow.ContainsKey(Symbol))
				Trading.SmaSlow[Symbol].Clear();
			if (Trading.RSI.ContainsKey(Symbol))
				Trading.RSI[Symbol].Clear();
			if (Trading.d_lRSI.ContainsKey(Symbol))
				Trading.d_lRSI[Symbol].Clear();
			if (Trading.d_lBB.ContainsKey(Symbol))
				Trading.d_lBB[Symbol].Clear();

			decimal BalanceUSD = Trading.DemoBalance["USD"].Available;
			List<Candle> candles = HitBTC.Candles[Symbol].ToList();
			foreach (var candle in candles)
			{
				Trading.d_lSmaFast[Symbol].Add(Trading.SmaFast[Symbol].NextValue(candle.Close));
				Trading.d_lSmaSlow[Symbol].Add(Trading.SmaSlow[Symbol].NextValue(candle.Close));
				Trading.d_lRSI[Symbol].Add(Trading.RSI[Symbol].NextValue(candle.Close));
				Trading.d_lBB[Symbol].Add(Trading.BB[Symbol].NextValue(candle.Close));
				Trading.d_DateTimes[Symbol].Add(candle.TimeStamp);

				if (HitBTC.d_Candle[Symbol].VolumeQuote > 0.5m)
				{
					if (Trading.RSI[Symbol].IsPrimed())
						Trading.Run_7_RSI(Symbol, candle.Close);
				}

				var currBal = Trading.DemoBalance["USD"].Available;
				if (currBal > BalanceUSD)
					textBox1.BeginInvoke((MethodInvoker)(() =>
					{
						textBox1.AppendText("sell " + currBal.ToString().PadRight(10).Substring(0, 10) + "  " +
							candle.Close.ToString().PadRight(10).Substring(0, 10) + "  " +
							Trading.d_lRSI[Symbol].Last().ToString().PadRight(10).Substring(0, 10) + "  " + candle.TimeStamp + "\r\n");
					}));
				else if (currBal < BalanceUSD)
					textBox1.BeginInvoke((MethodInvoker)(() =>
					{
						textBox1.AppendText("buy " + currBal.ToString().PadRight(10).Substring(0, 10) + "  " +
							candle.Close.ToString().PadRight(10).Substring(0, 10) + "  " +
							Trading.d_lRSI[Symbol].Last().ToString().PadRight(10).Substring(0, 10) + "  " + candle.TimeStamp + "\r\n");
					}));
				BalanceUSD = currBal;
			}

			if (Trading.DemoBalance[HitBTC.Symbols[Symbol].BaseCurrency].Available > 0)
			{
				Trading.Sell(Symbol, candles.Last().Close, Trading.DemoBalance[HitBTC.Symbols[Symbol].BaseCurrency].Available);
				textBox1.AppendText("sell " + Trading.DemoBalance["USD"].Available.ToString().PadRight(10).Substring(0, 10) + "  " +
								candles.Last().Close.ToString().PadRight(10).Substring(0, 10) + "  " +
								candles.Last().TimeStamp + "\r\n");
			}

			chart1.BeginInvoke((MethodInvoker)(() =>
			{
				chart1.Series["Candles"].Points.Clear();
				chart1.Series["SmaFast"].Points.Clear();
				chart1.Series["SmaSlow"].Points.Clear();

				chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol].ToList(), "TimeStamp", "Max,Min,Open,Close", "");
				//chart1.Series["SmaFast"].Points.DataBindXY(Trading.d_DateTimes[Symbol].ToList(), Trading.d_lSmaFast[Symbol].ToList());
				//chart1.Series["SmaSlow"].Points.DataBindXY(Trading.d_DateTimes[Symbol].ToList(), Trading.d_lSmaSlow[Symbol].ToList());
				//chart1.Series["SmaSlow"].Points.DataBindXY(Trading.d_DateTimes[Symbol].ToList(), Trading.d_lRSI[Symbol].ToList());

				var BU = Trading.d_lBB[Symbol].Select(v => v.BU).ToList();
				var BD = Trading.d_lBB[Symbol].Select(v => v.BD).ToList();
				var SMA = Trading.d_lBB[Symbol].Select(v => v.Sma).ToList();
				chart1.Series["SmaFast"].Points.DataBindXY(Trading.d_DateTimes[Symbol].ToList(), BU);
				chart1.Series["SmaSlow"].Points.DataBindXY(Trading.d_DateTimes[Symbol].ToList(), BD);
				chart1.Series["SMA"].Points.DataBindXY(Trading.d_DateTimes[Symbol].ToList(), SMA);
			}));

			Trading.Save("1");
		}

		private void CheckBoxAll_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxAll.Checked)
			{
				List<String> symbols = HitBTC.Symbols.Keys.Where(v => v.EndsWith("USD") || v.EndsWith("USDT")).ToList();
				comboBoxSymbols.DataSource = symbols;
			}
			else
			{
				if (HitBTC.Candles.Keys.Count > 0)
				{
					List<String> symbols = HitBTC.Candles.Keys.ToList();
					comboBoxSymbols.DataSource = symbols;
				}
			}
		}


		private void chart1_MouseUp(object sender, MouseEventArgs e)
		{
		}

		private void chart1_MouseMove(object sender, MouseEventArgs e)
		{
		}

		private void chart1_MouseDown(object sender, MouseEventArgs e)
		{
		}

		private void Chart1_MouseClick(object sender, MouseEventArgs e)
		{
			this.chart1.Focus();
		}

		private void Chart1_MouseWheel(object sender, MouseEventArgs e)
		{
			var minX = chart1.ChartAreas["0"].AxisX.Minimum;
			var maxX = chart1.ChartAreas["0"].AxisX.Maximum;

			minX += e.Delta / 10;

			if (minX < 0) minX = 0;
			else if (minX > chart1.ChartAreas["0"].AxisX.Maximum)
				minX = chart1.ChartAreas["0"].AxisX.Maximum - 10;

			chart1.BeginInvoke((MethodInvoker)(() => chart1.ChartAreas["0"].AxisX.Minimum = minX));

			var minY = chart1.Series["Candles"].Points.Where((x, i) => i >= minX - 1 && i <= maxX).Select(x => x.YValues[1]).ToArray().Min();
			var maxY = chart1.Series["Candles"].Points.Where((x, i) => i >= minX - 1 && i <= maxX).Select(x => x.YValues[0]).ToArray().Max();

			chart1.ChartAreas[0].AxisY.Minimum = minY;
			chart1.ChartAreas[0].AxisY.Maximum = maxY;
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}
	}
}