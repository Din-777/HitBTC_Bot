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
		SMA SmaSlow;
		SMA SmaFast;
		List<decimal> lSmaSlow;
		List<decimal> lSmaFast;
		List<decimal> lBuySell;
		List<DateTime> lDateTime;
		string Symbol = "ZRXUSD";
		Trading.Trading Trading;
		Revers Rev;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			HitBTC = new HitBTCSocketAPI();
			Trading = new Trading.Trading(ref HitBTC, console: false);
			Rev = new Revers();
			SmaFast = new SMA(7);
			SmaSlow = new SMA(11);
			lSmaSlow = new List<decimal>();
			lSmaFast = new List<decimal>();
			lBuySell = new List<decimal>();
			lDateTime = new List<DateTime>();
			HitBTC.SocketMarketData.SubscribeCandles(Symbol, period: Period.M1, limit: 500);
			HitBTC.SocketMarketData.SubscribeTrades(Symbol, limit: 1);
			System.Threading.Thread.Sleep(2000);

			chart1.Series.Add("Candles");
			chart1.Series["Candles"].ChartType = SeriesChartType.Candlestick;
			chart1.Series["Candles"].XValueType = ChartValueType.Time;
			chart1.Series["Candles"].CustomProperties = "PriceDownColor=Red, PriceUpColor=Green";
			chart1.Series["Candles"].YValueMembers = "Max,Min,Open,Close";
			chart1.Series["Candles"].XValueMember = "TimeStamp";
			chart1.Series["Candles"].BorderWidth = 2;
			chart1.Series["Candles"].IsXValueIndexed = true;

			chart1.Series.Add("EmaSlow");
			chart1.Series["EmaSlow"].ChartType = SeriesChartType.Line;
			chart1.Series["EmaSlow"].XValueType = ChartValueType.Time;
			chart1.Series["EmaSlow"].YValueMembers = "EMA";
			chart1.Series["EmaSlow"].XValueMember = "TimeStamp";
			chart1.Series["EmaSlow"].BorderWidth = 2;
			chart1.Series["EmaSlow"].IsXValueIndexed = true;
			chart1.Series["EmaSlow"].Color = Color.Violet;

			chart1.Series.Add("EmaFast");
			chart1.Series["EmaFast"].ChartType = SeriesChartType.Line;
			chart1.Series["EmaFast"].XValueType = ChartValueType.Time;
			chart1.Series["EmaFast"].YValueMembers = "EMA";
			chart1.Series["EmaFast"].XValueMember = "TimeStamp";
			chart1.Series["EmaFast"].BorderWidth = 2;
			chart1.Series["EmaFast"].IsXValueIndexed = true;
			chart1.Series["EmaFast"].Color = Color.Orange;

			chart1.Series.Add("BuySell");
			chart1.Series["BuySell"].ChartType = SeriesChartType.Point;
			chart1.Series["BuySell"].XValueType = ChartValueType.Time;
			chart1.Series["BuySell"].BorderWidth = 2;
			chart1.Series["BuySell"].IsXValueIndexed = true;
			chart1.Series["BuySell"].Color = Color.Black;

			chart1.ChartAreas.Add("0");
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;

			chart1.MouseWheel += Chart1_MouseWheel;
			chart1.MouseClick += Chart1_MouseClick;
			chart1.MouseDown += chart1_MouseDown;
			chart1.MouseMove += chart1_MouseMove;
			chart1.MouseUp += chart1_MouseUp;

			System.Threading.Thread.Sleep(3000);

			foreach (var c in HitBTC.Candles[Symbol])
			{
				SmaFast.NextAverage(c.Close);
				SmaSlow.NextAverage(c.Close);

				lDateTime.Add(c.TimeStamp);
				lSmaSlow.Add(SmaSlow.LastAverage);
				lSmaFast.Add(SmaFast.LastAverage);
				lBuySell.Add(0);
			}

			chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");
			chart1.Series["EmaSlow"].Points.DataBindXY(lDateTime, lSmaSlow);
			chart1.Series["EmaFast"].Points.DataBindXY(lDateTime, lSmaFast);
			chart1.Series["BuySell"].Points.DataBindXY(lDateTime, lBuySell);

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
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
			if (s == "updateCandles")
			{
				var trade = HitBTC.d_Trades[Symbol];
				var dateTime = HitBTC.Candles[Symbol].Last().TimeStamp;

				var val = (HitBTC.d_Candle[Symbol].Open +
							HitBTC.d_Candle[Symbol].Close +
							HitBTC.d_Candle[Symbol].Min +
							HitBTC.d_Candle[Symbol].Max) / 4.0m;

				if (HitBTC.Candles[Symbol].Count == lDateTime.Count)
				{
					lSmaSlow[lSmaSlow.Count - 1] = SmaSlow.Average(val);
					lSmaFast[lSmaFast.Count - 1] = SmaFast.Average(val);
					lBuySell[lBuySell.Count - 1] = 0;
				}
				else
				{
					lSmaSlow.Add(SmaSlow.NextAverage(HitBTC.d_Candle[Symbol].Close));
					lSmaFast.Add(SmaFast.NextAverage(HitBTC.d_Candle[Symbol].Close));
					lBuySell.Add(0);
					lDateTime.Add(dateTime);
				}

				var diff = lSmaSlow[lSmaSlow.Count - 1] - lSmaFast[lSmaFast.Count - 1];
				if(Rev.IsRevers(diff))
					textBox1.BeginInvoke((MethodInvoker)(() => { textBox1.AppendText((diff > 0.0m ? "sell" : "buy") + 
						"  " + HitBTC.Candles[Symbol].Last().Close.ToString() + "\r\n"); }));

				chart1.BeginInvoke((MethodInvoker)(() =>
				{
					chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");
					chart1.Series["EmaSlow"].Points.DataBindXY(lDateTime, lSmaSlow);
					chart1.Series["EmaFast"].Points.DataBindXY(lDateTime, lSmaFast);
					chart1.Series["BuySell"].Points.DataBindXY(lDateTime, lBuySell);
				}));
			}
		}

		private void Button1_Click(object sender, EventArgs e)
		{
			int periodFast = (int)numericUpDown1.Value;
			int periodSlow = (int)numericUpDown2.Value;
			SmaFast.Period = periodFast;
			SmaSlow.Period = periodSlow;
			SmaFast.Clear();
			SmaSlow.Clear();
			lBuySell.Clear();
			

			for (int i = 0; i < HitBTC.Candles[Symbol].Count; i++)
			{
				var val = (HitBTC.Candles[Symbol][i].Open +
							HitBTC.Candles[Symbol][i].Close +
							HitBTC.Candles[Symbol][i].Min +
							HitBTC.Candles[Symbol][i].Max) / 4.0m;

				if (i < lSmaSlow.Count)
					lSmaSlow[i] = SmaSlow.NextAverage(val);
				else
					lSmaSlow.Add(SmaSlow.NextAverage(val));

				if (i < lSmaFast.Count)
					lSmaFast[i] = SmaFast.NextAverage(val);
				else
					lSmaFast.Add(SmaFast.NextAverage(val));
				
				decimal emaDiff = SmaSlow.LastAverage - SmaFast.LastAverage;
				Rev.IsRevers(emaDiff);

				if (SmaSlow.isPrimed() && SmaFast.isPrimed())
				{
					if (Rev.ReversNow)
					{
						if (emaDiff > 0.0m)
						{
							lBuySell.Add(HitBTC.Candles[Symbol][i].Close);
						}
						else if (emaDiff < 0.0m)
						{
							lBuySell.Add(HitBTC.Candles[Symbol][i].Close);
						}
						else
							lBuySell.Add(0);
					}
					else
						lBuySell.Add(0);
				}
				else
					lBuySell.Add(0);
			}

			chart1.BeginInvoke((MethodInvoker)(() =>
			{
				chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");
				chart1.Series["EmaSlow"].Points.DataBindXY(lDateTime, lSmaSlow);
				chart1.Series["EmaFast"].Points.DataBindXY(lDateTime, lSmaFast);
				chart1.Series["BuySell"].Points.DataBindXY(lDateTime, lBuySell);
			}));
		}		
	}
}
