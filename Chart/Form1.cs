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

namespace Chart
{
	public partial class Form1 : Form
	{
		HitBTCSocketAPI HitBTC;
		SMA EmaSlow;
		SMA EmaFast;
		List<decimal> lEmaSlow;
		List<decimal> lEmaFast;
		List<DateTime> lDateTime;
		string Symbol = "BTCUSD";

		public Form1()
		{
			InitializeComponent();

			HitBTC = new HitBTCSocketAPI();
			EmaFast = new SMA(7);
			EmaSlow = new SMA(11);
			lEmaSlow = new List<decimal>();
			lEmaFast = new List<decimal>();
			lDateTime = new List<DateTime>();
			HitBTC.SocketMarketData.SubscribeCandles(Symbol, period: Period.M1, limit: 500);
			HitBTC.SocketMarketData.SubscribeTrades(Symbol, limit: 1);
			System.Threading.Thread.Sleep(2000);			

			chart1.Series.Add("New");
			chart1.Series["New"].ChartType = SeriesChartType.Candlestick;
			chart1.Series["New"].XValueType = ChartValueType.Time;
			chart1.Series["New"].CustomProperties = "PriceDownColor=Red, PriceUpColor=Green";
			chart1.Series["New"]. YValueMembers = "Max,Min,Open,Close";
			//chart1.Series["New"].XValueMember = "TimeStamp";
			chart1.Series["New"].BorderWidth = 2;
			chart1.Series["New"].IsXValueIndexed = true;

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

			chart1.ChartAreas.Add("0");
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;

			chart1.MouseWheel += Chart1_MouseWheel;
			chart1.MouseClick += Chart1_MouseClick;

			System.Threading.Thread.Sleep(3000);

			chart1.DataBind();						

			foreach (var c in HitBTC.Candles[Symbol])
			{
				EmaFast.NextAverage(c.Open);
				//EmaFast.Compute(c.Close);

				EmaSlow.NextAverage(c.Open);
				//EmaSlow.Compute(c.Close);

				lDateTime.Add(c.TimeStamp);
				lEmaSlow.Add(EmaSlow.LastAverage);
				lEmaFast.Add(EmaFast.LastAverage);
			}

			chart1.Series["New"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");

			chart1.Series["EmaSlow"].Points.DataBindXY(lDateTime, lEmaSlow);
			chart1.Series["EmaFast"].Points.DataBindXY(lDateTime, lEmaFast);

			HitBTC.MessageReceived += HitBTCSocket_MessageReceived;
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

			var minY = chart1.Series["New"].Points.Where((x, i) => i >= minX-1 && i <= maxX).Select(x => x.YValues[1]).ToArray().Min();
			var maxY = chart1.Series["New"].Points.Where((x, i) => i >= minX-1 && i <= maxX).Select(x => x.YValues[0]).ToArray().Max();

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

				if (HitBTC.Candles[Symbol].Count == lDateTime.Count)
				{
					lEmaSlow[lEmaSlow.Count - 1] = EmaSlow.Average(HitBTC.d_Candle[Symbol].Close);
					lEmaFast[lEmaFast.Count - 1] = EmaFast.Average(HitBTC.d_Candle[Symbol].Close);
				}
				else
				{
					lEmaSlow.Add(EmaSlow.NextAverage(HitBTC.d_Candle[Symbol].Close));
					lEmaFast.Add(EmaFast.NextAverage(HitBTC.d_Candle[Symbol].Close));
					lDateTime.Add(dateTime);
				}

				chart1.BeginInvoke((MethodInvoker)(() =>
				{
					chart1.Series["New"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");
					chart1.Series["EmaSlow"].Points.DataBindXY(lDateTime, lEmaSlow);
					chart1.Series["EmaFast"].Points.DataBindXY(lDateTime, lEmaFast);
				}));
			}
		}

		private void Button1_Click(object sender, EventArgs e)
		{
			int period = (int)numericUpDown1.Value;
			EmaFast.Period = period;
			EmaFast.Clear();
			for (int i = 0; i < HitBTC.Candles[Symbol].Count; i++)
			{
				if (i < lEmaFast.Count)
					lEmaFast[i] = EmaFast.NextAverage(HitBTC.Candles[Symbol][i].Close);
				else
					lEmaFast.Add(EmaFast.NextAverage(HitBTC.Candles[Symbol][i].Close));
			}

			chart1.BeginInvoke((MethodInvoker)(() =>
			{
				chart1.Series["New"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");
				chart1.Series["EmaSlow"].Points.DataBindXY(lDateTime, lEmaSlow);
				chart1.Series["EmaFast"].Points.DataBindXY(lDateTime, lEmaFast);
			}));
		}
		private void Button2_Click(object sender, EventArgs e)
		{
			int period = (int)numericUpDown2.Value;
			EmaSlow.Period = period;
			EmaSlow.Clear();
			for (int i = 0; i < HitBTC.Candles[Symbol].Count; i++)
			{
				if (i < lEmaSlow.Count)
					lEmaSlow[i] = EmaSlow.NextAverage(HitBTC.Candles[Symbol][i].Close);
				else
					lEmaSlow.Add(EmaSlow.NextAverage(HitBTC.Candles[Symbol][i].Close));
			}

			chart1.BeginInvoke((MethodInvoker)(() =>
			{
				chart1.Series["New"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "");
				chart1.Series["EmaSlow"].Points.DataBindXY(lDateTime, lEmaSlow);
				chart1.Series["EmaFast"].Points.DataBindXY(lDateTime, lEmaFast);
			}));
		}
	}
}
