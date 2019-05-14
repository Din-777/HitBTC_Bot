using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HitBTC;
using HitBTC.Models;

namespace Chart
{
	public partial class Form1 : Form
	{

		public HitBTCSocketAPI HitBTC;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			HitBTC = new HitBTCSocketAPI();
			HitBTC.MessageReceived += HitBTC_MessageReceived;
			HitBTC.SocketMarketData.SubscribeCandles("BTCUSD", Period.M1, 1);
			HitBTC.SocketMarketData.SubscribeTrades("BTCUSD", 1);
		}

		private void HitBTC_MessageReceived(string s)
		{
			if(s == "updateTrades")
			{
				this.chartCandles.BeginInvoke((MethodInvoker)(() =>
				{
					chartCandles.Series[0].Points.AddY(HitBTC.Trade.Price);
				}));
			}
		}
	}
}
