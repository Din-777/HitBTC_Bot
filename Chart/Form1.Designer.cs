namespace Chart
{
	partial class Form1
	{
		/// <summary>
		/// Обязательная переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Требуемый метод для поддержки конструктора — не изменяйте 
		/// содержимое этого метода с помощью редактора кода.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			this.chartCandles = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.candleBindingSource = new System.Windows.Forms.BindingSource(this.components);
			((System.ComponentModel.ISupportInitialize)(this.chartCandles)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.candleBindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// chartCandles
			// 
			this.chartCandles.BackColor = System.Drawing.Color.Black;
			this.chartCandles.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.chartCandles.BorderlineColor = System.Drawing.Color.DimGray;
			this.chartCandles.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
			chartArea1.AlignmentOrientation = System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations.None;
			chartArea1.AxisX.InterlacedColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX.LabelStyle.ForeColor = System.Drawing.Color.Lime;
			chartArea1.AxisX.LineColor = System.Drawing.Color.Lime;
			chartArea1.AxisX.MajorGrid.Interval = 0D;
			chartArea1.AxisX.MajorGrid.IntervalOffset = 0D;
			chartArea1.AxisX.MajorGrid.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisX.MajorGrid.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX.MajorTickMark.Interval = 0D;
			chartArea1.AxisX.MajorTickMark.IntervalOffset = 0D;
			chartArea1.AxisX.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisX.MajorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX.MinorGrid.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX.MinorTickMark.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX.ScaleBreakStyle.Enabled = true;
			chartArea1.AxisX.ScaleBreakStyle.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX.TitleForeColor = System.Drawing.Color.Lime;
			chartArea1.AxisX2.MajorGrid.Interval = 0D;
			chartArea1.AxisX2.MajorGrid.IntervalOffset = 0D;
			chartArea1.AxisX2.MajorGrid.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisX2.MajorGrid.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisX2.MajorGrid.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX2.MajorTickMark.Interval = 0D;
			chartArea1.AxisX2.MajorTickMark.IntervalOffset = 0D;
			chartArea1.AxisX2.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisX2.MajorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisX2.MajorTickMark.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX2.MinorGrid.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisX2.MinorTickMark.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.Lime;
			chartArea1.AxisY.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY.MajorGrid.Interval = 0D;
			chartArea1.AxisY.MajorGrid.IntervalOffset = 0D;
			chartArea1.AxisY.MajorGrid.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisY.MajorGrid.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY.MajorTickMark.Interval = 0D;
			chartArea1.AxisY.MajorTickMark.IntervalOffset = 0D;
			chartArea1.AxisY.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisY.MajorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY.MinorGrid.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY.MinorTickMark.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY.ScaleBreakStyle.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY.TitleForeColor = System.Drawing.Color.Lime;
			chartArea1.AxisY2.LabelStyle.IsEndLabelVisible = false;
			chartArea1.AxisY2.MajorGrid.Interval = 0D;
			chartArea1.AxisY2.MajorGrid.IntervalOffset = 0D;
			chartArea1.AxisY2.MajorGrid.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisY2.MajorGrid.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisY2.MajorGrid.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY2.MajorTickMark.Interval = 0D;
			chartArea1.AxisY2.MajorTickMark.IntervalOffset = 0D;
			chartArea1.AxisY2.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisY2.MajorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
			chartArea1.AxisY2.MajorTickMark.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY2.MinorGrid.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.AxisY2.MinorTickMark.LineColor = System.Drawing.SystemColors.ActiveCaption;
			chartArea1.BackColor = System.Drawing.Color.Black;
			chartArea1.BackImageAlignment = System.Windows.Forms.DataVisualization.Charting.ChartImageAlignmentStyle.TopRight;
			chartArea1.Name = "ChartArea1";
			this.chartCandles.ChartAreas.Add(chartArea1);
			this.chartCandles.Dock = System.Windows.Forms.DockStyle.Fill;
			legend1.BackColor = System.Drawing.Color.White;
			legend1.BackImageAlignment = System.Windows.Forms.DataVisualization.Charting.ChartImageAlignmentStyle.TopRight;
			legend1.Enabled = false;
			legend1.Name = "Legend1";
			this.chartCandles.Legends.Add(legend1);
			this.chartCandles.Location = new System.Drawing.Point(0, 0);
			this.chartCandles.Name = "chartCandles";
			series1.BackImageAlignment = System.Windows.Forms.DataVisualization.Charting.ChartImageAlignmentStyle.TopRight;
			series1.ChartArea = "ChartArea1";
			series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
			series1.CustomProperties = "PriceDownColor=Red, PriceUpColor=Green";
			series1.EmptyPointStyle.BackImageAlignment = System.Windows.Forms.DataVisualization.Charting.ChartImageAlignmentStyle.TopRight;
			series1.LabelBackColor = System.Drawing.Color.Black;
			series1.LabelBorderColor = System.Drawing.Color.Black;
			series1.LabelBorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.NotSet;
			series1.LabelForeColor = System.Drawing.Color.Lime;
			series1.Legend = "Legend1";
			series1.MarkerBorderColor = System.Drawing.Color.Black;
			series1.MarkerBorderWidth = 2;
			series1.MarkerColor = System.Drawing.Color.Black;
			series1.MarkerImageTransparentColor = System.Drawing.Color.Black;
			series1.Name = "Candles";
			series1.SmartLabelStyle.CalloutBackColor = System.Drawing.SystemColors.ActiveCaption;
			series1.SmartLabelStyle.CalloutLineColor = System.Drawing.SystemColors.ActiveCaption;
			series1.YValuesPerPoint = 4;
			this.chartCandles.Series.Add(series1);
			this.chartCandles.Size = new System.Drawing.Size(893, 594);
			this.chartCandles.TabIndex = 0;
			// 
			// candleBindingSource
			// 
			this.candleBindingSource.DataSource = typeof(HitBTC.Models.Candle);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Indigo;
			this.ClientSize = new System.Drawing.Size(893, 594);
			this.Controls.Add(this.chartCandles);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.chartCandles)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.candleBindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataVisualization.Charting.Chart chartCandles;
		private System.Windows.Forms.BindingSource candleBindingSource;
	}
}

