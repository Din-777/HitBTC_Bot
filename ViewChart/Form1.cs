using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using HitBTC;
using HitBTC.Models;
using Trading;

namespace ViewChart
{
	public partial class Form1 : Form
	{
		public HitBTCSocketAPI HitBTC;
		Trading.Trading Trading;

		string Symbol = "BTCUSD";

		public Form1(HitBTCSocketAPI hitBTC, Trading.Trading trading)
		{
			HitBTC = hitBTC;
			Trading = trading;
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

			chart1.ChartAreas.Add("0");
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].AxisY.IsStartedFromZero = false;
			chart1.ChartAreas["0"].BackColor = Color.Gainsboro;
			chart1.BackColor = Color.Gainsboro;

			while(!HitBTC.Candles.ContainsKey(Symbol)) Thread.Sleep(200);

			chart1.MouseWheel += Chart1_MouseWheel;
			chart1.MouseClick += Chart1_MouseClick;
			chart1.MouseDown += chart1_MouseDown;
			chart1.MouseMove += chart1_MouseMove;
			chart1.MouseUp += chart1_MouseUp;
		}
			   
		private void Form1_Shown(object sender, EventArgs e)
		{
			List<String> l_symbols = HitBTC.Candles.Keys.ToList(); ;
			comboBoxSymbols.DataSource = l_symbols;

			CastomInitializeComponent();

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

			Trading.DemoBalance["USD"].Available = 100.0m;
			Trading.PendingOrders.Clear();
			Trading.ClosedOrders.Clear();
			textBox1.BeginInvoke((MethodInvoker)(() =>
			{
				textBox1.Clear();
			}));

			Trading.Add(symbol: Symbol, period: Period.H1, tradingQuantityInPercent: 10.0m, stopPercent: 10.0m, closePercent: 5.0m);

			while (!HitBTC.Candles.ContainsKey(Symbol)) Thread.Sleep(100);
			HitBTC.SocketMarketData.UnSubscribeCandles(symbol: Symbol);

			decimal BalanceUSD = Trading.DemoBalance["USD"].Available;

			foreach (var candle in HitBTC.Candles[Symbol])
			{
				Trading.Run_6(Symbol, candle.Close);

				if (Trading.DemoBalance["USD"].Available > BalanceUSD)
					textBox1.BeginInvoke((MethodInvoker)(() =>
					{
						textBox1.AppendText("sell " + Trading.DemoBalance["USD"].Available.ToString().PadRight(10).Substring(0, 10) + "\r\n");
					}));
				else if (Trading.DemoBalance["USD"].Available < BalanceUSD)
					textBox1.BeginInvoke((MethodInvoker)(() =>
					{
						textBox1.AppendText("buy " + Trading.DemoBalance["USD"].Available.ToString().PadRight(10).Substring(0, 10) + "\r\n");
					}));
			}

			chart1.BeginInvoke((MethodInvoker)(() =>
			{
				chart1.Series["Candles"].Points.Clear();
				chart1.Series["SmaFast"].Points.Clear();
				chart1.Series["SmaSlow"].Points.Clear();

				chart1.Series["Candles"].Points.DataBind(HitBTC.Candles[Symbol].ToList(), "TimeStamp", "Max,Min,Open,Close", "");
				chart1.Series["SmaFast"].Points.DataBindXY(Trading.d_DateTimes[Symbol].ToList(), Trading.d_lSmaFast[Symbol].ToList());
				chart1.Series["SmaSlow"].Points.DataBindXY(Trading.d_DateTimes[Symbol].ToList(), Trading.d_lSmaSlow[Symbol].ToList());
			}));
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
