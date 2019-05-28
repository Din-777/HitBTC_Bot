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
		EMA EmaSlow;
		EMA EmaFast;
		string Symbol = "BTCUSD";

		public Form1()
		{
			InitializeComponent();

			HitBTC = new HitBTCSocketAPI();
			EmaFast = new EMA(6);
			EmaSlow = new EMA(11);
			HitBTC.SocketMarketData.SubscribeCandles(Symbol, period: Period.M1, limit: 100);
			HitBTC.SocketMarketData.SubscribeTrades(Symbol, limit: 1);
			System.Threading.Thread.Sleep(2000);
			
			chart1.Series.Add("New");
			chart1.DataSource = HitBTC.Candles[Symbol];			
			chart1.Series["New"].ChartType = SeriesChartType.Candlestick;
			chart1.Series["New"].XValueType = ChartValueType.Time;
			chart1.Series["New"].CustomProperties = "PriceDownColor=Red, PriceUpColor=Green";
			chart1.Series["New"].YValueMembers = "Max,Min,Open,Close";
			chart1.Series["New"].XValueMember = "TimeStamp";				
			chart1.Series["New"].BorderWidth = 2;
			chart1.Series["New"].IsXValueIndexed = true;

			chart1.ChartAreas.Add("0");
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			//chart1.ChartAreas["0"].po

			//chart1.ChartAreas[0].AxisY.ScaleView.Position = 100;

			chart1.MouseWheel += Chart1_MouseWheel;
			chart1.MouseClick += Chart1_MouseClick;

			System.Threading.Thread.Sleep(1000);

			chart1.DataBind();
						

			foreach (var c in HitBTC.Candles[Symbol])
			{
				EmaFast.Compute(c.Open);
				EmaFast.Compute(c.Close);

				EmaSlow.Compute(c.Open);
				EmaSlow.Compute(c.Close);
			}

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

			//double[] tempHighs = chart1.Series["New"].Points.Where((x, i) => x.YValues[0] >= start && x.YValues[1] <= end).Select(x => x.YValues[0]).ToArray();
			//double ymin = tempHighs.Min();
			//double ymax = tempHighs.Max();

			//chart1.ChartAreas[0].AxisY. = ymin;
			//chart1.ChartAreas[0].AxisY.Maximum = ymax;

			var minY = chart1.Series["New"].Points.Where((x, i) => i >= minX-1 && i <= maxX).Select(x => x.YValues[1]).ToArray().Min();
			var maxY = chart1.Series["New"].Points.Where((x, i) => i >= minX-1 && i <= maxX).Select(x => x.YValues[0]).ToArray().Max();

			chart1.ChartAreas[0].AxisY.Minimum = minY;
			chart1.ChartAreas[0].AxisY.Maximum = maxY;

			//textBox1.AppendText(v.ToString() + "\r\n");
		}

		private void Chart1_MouseClick(object sender, MouseEventArgs e)
		{
			this.chart1.Focus();
		}

		private void HitBTCSocket_MessageReceived(string s)
		{
			if (s == "updateCandles")
			{
				var trade = HitBTC.d_Trades[Symbol];

				EmaFast.Compute(trade.Price);
				EmaSlow.Compute(trade.Price);

				chart1.BeginInvoke((MethodInvoker)(() => chart1.DataBind()));
				//chart1.BeginInvoke((MethodInvoker)(() => 
				//	chart1.Series["New"].Points.DataBind(HitBTC.Candles[Symbol], "TimeStamp", "Max,Min,Open,Close", "")));
				 
				//chart1.BeginInvoke((MethodInvoker)(() => 
				//	chart1.Series["EmaFast"].Points.AddXY.SetValueXY(HitBTC.Candles[Symbol].Last().TimeStamp, EmaFast.Value) )) ;
			}
		}

		private void Button1_Click(object sender, EventArgs e)
		{
			chart1.ChartAreas["0"].AxisY.IsLabelAutoFit = true;
		}

		private void Button2_Click(object sender, EventArgs e)
		{
		}
	}
}
