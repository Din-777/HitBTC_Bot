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

namespace Chart
{
	public partial class Form1 : Form
	{
		HitBTCSocketAPI HitBTC;
		List<decimal> lBuy;
		List<decimal> lSell;
		List<DateTime> lDateTime;
		static string baseCurrency = "EDO";
		string Symbol = baseCurrency + "USD";
		Trading.Trading Trading;
		SMA SmaPrice = new SMA(50);

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			string pKey = "YGzq3GQP9vIybW8CcT6+e3pBqX8Tgbr6";
			string sKey = "B37LaDlfa70YM9gorzpjYGQAZVRNXDj3";

			HitBTC = new HitBTCSocketAPI();
			HitBTC.SocketAuth.Auth(pKey, sKey);
			HitBTC.SocketMarketData.GetSymbols();

			Trading = new Trading.Trading(ref HitBTC, console: false);
			Trading.DemoBalance.Add("USD", new Balance { Currency = "USD", Available = 100.0m });
			Trading.DemoBalance.Add(baseCurrency, new Balance { Currency = baseCurrency, Available = 00.0m });
			Trading.Add(Symbol, period: Period.H1, treadingQuantity: 100.0m, stopPercent: 10.0m, closePercent: 10.0m);

			lBuy = new List<decimal>();
			lSell = new List<decimal>();
			lDateTime = new List<DateTime>();
			System.Threading.Thread.Sleep(2000);

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

			chart1.Series.Add("Buy");
			chart1.Series["Buy"].ChartType = SeriesChartType.Point;
			chart1.Series["Buy"].XValueType = ChartValueType.Time;
			chart1.Series["Buy"].BorderWidth = 2;
			chart1.Series["Buy"].IsXValueIndexed = true;
			chart1.Series["Buy"].Color = Color.Black;

			chart1.Series.Add("Sell");
			chart1.Series["Sell"].ChartType = SeriesChartType.Point;
			chart1.Series["Sell"].XValueType = ChartValueType.Time;
			chart1.Series["Sell"].BorderWidth = 3;
			chart1.Series["Sell"].IsXValueIndexed = true;
			chart1.Series["Sell"].Color = Color.Gold;

			chart1.ChartAreas.Add("0");
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].BackColor = Color.Gainsboro;
			chart1.BackColor = Color.Gainsboro;

			chart1.MouseWheel += Chart1_MouseWheel;
			chart1.MouseClick += Chart1_MouseClick;
			chart1.MouseDown  += chart1_MouseDown;
			chart1.MouseMove  += chart1_MouseMove;
			chart1.MouseUp    += chart1_MouseUp;

			//System.Threading.Thread.Sleep(5000);

			foreach (var c in HitBTC.Candles[Symbol])
			{
				lDateTime.Add(c.TimeStamp);
				lBuy.Add(0);
				lSell.Add(0);
			}

			chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");
			chart1.Series["SmaFast"].Points.DataBindXY(lDateTime, Trading.d_lSmaFast[Symbol]);
			chart1.Series["SmaSlow"].Points.DataBindXY(lDateTime, Trading.d_lSmaSlow[Symbol]);
			chart1.Series["Sell"].Points.DataBindXY(lDateTime, lSell);
			chart1.Series["Buy"].Points.DataBindXY(lDateTime, lBuy);
						
			HitBTC.ReceiveMessages(false);
			HitBTC.MessageReceived -= HitBTCSocket_MessageReceived;
		}

		private void Button1_Click(object sender, EventArgs e)
		{
			int periodFast = (int)numericUpDown1.Value;
			int periodSlow = (int)numericUpDown2.Value;
			lBuy.Clear();
			lSell.Clear();
			Trading.PendingOrders.Clear();
			SmaPrice.Period = periodFast;
			Trading.DemoBalance["USD"].Available = 100.0m;

			Trading.SmaFast[Symbol].Clear();
			Trading.SmaSlow[Symbol].Clear();
			Trading.d_lSmaFast[Symbol].Clear();
			Trading.d_lSmaSlow[Symbol].Clear();

			lDateTime.Clear();

			textBox1.BeginInvoke((MethodInvoker)(() => { textBox1.Clear(); }));

			for (int i = 0; i < HitBTC.Candles[Symbol].Count; i++)
			{
				var candle = HitBTC.Candles[Symbol][i];
				lDateTime.Add(candle.TimeStamp);
				var val = candle.Close;

				Trading.d_lSmaFast[Symbol].Add(Trading.SmaFast[Symbol].NextAverage(val));
				Trading.d_lSmaSlow[Symbol].Add(Trading.SmaSlow[Symbol].NextAverage(val));

				SmaPrice.NextAverage(val);

				if (Trading.SmaFast[Symbol].isPrimed() && Trading.SmaSlow[Symbol].isPrimed())
				{
					var oldBalance = Trading.DemoBalance["USD"].Available;

					Trading.Run_6(Symbol, val);

					var newBalance = Trading.DemoBalance["USD"].Available;

					if (oldBalance == newBalance)
					{
						lBuy.Add(0);
						lSell.Add(0);
					}
					else if (oldBalance > newBalance)
					{
						lBuy.Add(val);
						lSell.Add(0);
						textBox1.BeginInvoke((MethodInvoker)(() => { textBox1.AppendText("buy" + newBalance + "\r\n"); }));

					}
					else if (oldBalance < newBalance)
					{
						lBuy.Add(0);
						lSell.Add(val);
						textBox1.BeginInvoke((MethodInvoker)(() => { textBox1.AppendText("sel" + newBalance + "\r\n"); }));
					}
				}
				else
				{
					lBuy.Add(0);
					lSell.Add(0);
				}
			}

			chart1.BeginInvoke((MethodInvoker)(() =>
			{
				chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");
				chart1.Series["SmaFast"].Points.DataBindXY(lDateTime, Trading.d_lSmaFast[Symbol]);
				chart1.Series["SmaSlow"].Points.DataBindXY(lDateTime, Trading.d_lSmaSlow[Symbol]);
				chart1.Series["Sell"].Points.DataBindXY(lDateTime, lSell);
				chart1.Series["Buy"].Points.DataBindXY(lDateTime, lBuy);
			}));
		}

		private void chart1_MouseDown(object sender, MouseEventArgs e)
		{
		}

		private void chart1_MouseUp(object sender, MouseEventArgs e)
		{
		}

		private void chart1_MouseMove(object sender, MouseEventArgs e)
		{
		}

		private void Chart1_MouseWheel(object sender, MouseEventArgs e)
		{
			var minX = chart1.ChartAreas["0"].AxisX.Minimum;
			var maxX = chart1.ChartAreas["0"].AxisX.Maximum;

			minX += e.Delta/10;

			if (minX < 0) minX = 0;
			else if (minX > chart1.ChartAreas["0"].AxisX.Maximum)
				minX = chart1.ChartAreas["0"].AxisX.Maximum-10;

			chart1.BeginInvoke((MethodInvoker)(() => chart1.ChartAreas["0"].AxisX.Minimum = minX));

			var minY = chart1.Series["Candles"].Points.Where((x, i) => i >= minX-1 && i <= maxX).Select(x => x.YValues[1]).ToArray().Min();
			var maxY = chart1.Series["Candles"].Points.Where((x, i) => i >= minX-1 && i <= maxX).Select(x => x.YValues[0]).ToArray().Max();

			chart1.ChartAreas[0].AxisY.Minimum = minY;
			chart1.ChartAreas[0].AxisY.Maximum = maxY;
		}

		private void Chart1_MouseClick(object sender, MouseEventArgs e)
		{
			this.chart1.Focus();
		}

		private void HitBTCSocket_MessageReceived(string s, string symbol)
		{
			if (s == "updateCandles" && symbol != null)
			{
				var candle = HitBTC.d_Candle[symbol];

				chart1.BeginInvoke((MethodInvoker)(() =>
				{
					chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");					
					chart1.Series["SmaFast"].Points.DataBindXY(lDateTime, Trading.d_lSmaFast[Symbol]);
					chart1.Series["SmaSlow"].Points.DataBindXY(lDateTime, Trading.d_lSmaSlow[Symbol]);
					chart1.Series["Sell"].Points.DataBindXY(lDateTime, lSell);
					chart1.Series["Buy"].Points.DataBindXY(lDateTime, lBuy);
				}));
			}
		}
	}
}