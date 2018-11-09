#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class THA : Indicator
	{
		private Series<double> diffSeries;
		private Series<double> highLowSeries;
		private Brush BullishBrush = Brushes.DarkBlue;
		private Brush BearishBrush = Brushes.HotPink;
		public int Signal = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"THA";
				Name										= "THA";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				FastMaPeriod					= 10;
				SlowMaPeriod					= 20;
				TickRangePeriod					= 15;
				MinMaDiff					= 0.4;
				MaxMaDiff					= 10;
				MinTickRange					= 0;
				MaxTickRange					= 30;
				GoLong					= true;
				GoShort					= true;
				FireAlerts					= true;
				AddPlot(Brushes.Orange, "FastMaPlot");
				AddPlot(Brushes.White, "SlowMaPlot");
				
				Plots[0].Width= 2;
				Plots[1].Width= 2;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				diffSeries = new Series<double>(this);
				highLowSeries = new Series<double>(this);
				Signal = 0;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < SlowMaPeriod) return;
			
			double fastMa = EMA(FastMaPeriod)[0];
			double slowMa = EMA(SlowMaPeriod)[0];
			
			FastMaPlot[0] = fastMa;
			SlowMaPlot[0] = slowMa;
			
			double diff = fastMa - slowMa;
			//diffSeries[0] = diff;
			//double diffMa = EMA(diffSeries, DiffMaLength)[0];
			
			Brush trendBrush = fastMa < slowMa ? BearishBrush : BullishBrush;
			PlotBrushes[0][0] = trendBrush;
			
			bool isGreenOneBack = Close[0] > Open[0];
			bool isGreenTwoBack = Close[1] > Open[1];
			bool isRedOneBack = Close[0] < Open[0];
			bool isRedTwoBack = Close[1] < Open[1];
			
			highLowSeries[0] = (High[0] - Low[0])/TickSize;
			double tickRange = EMA(highLowSeries, TickRangePeriod)[0];
            bool validTickRange = tickRange > MinTickRange && tickRange < MaxTickRange;

			String alertText = String.Format("{0:C2} / {1:N2} / {2:N2}", Close[0], tickRange, diff);

			Print(String.Format("GoLong: {0}", GoLong));
			
			bool buy = GoLong && validTickRange && isRedTwoBack && isGreenOneBack && diff > MinMaDiff && diff < MaxMaDiff;

            if (buy)
			{
				ArrowUp buyArrow = Draw.ArrowUp(this, String.Format("buy_arrow_{0}", CurrentBar), true, 0, Low[0] - (4 * TickSize), Brushes.Yellow);
				
				if(FireAlerts)
				{
					Alert(String.Format("buy_alert_{0}", CurrentBar), Priority.High, alertText,
						NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", 10, Brushes.Green, Brushes.White);
				}
				
				Signal = 1;
			}
			
			bool sell = GoShort && validTickRange && isGreenTwoBack && isRedOneBack && diff < -MinMaDiff && diff > -MaxMaDiff;
			if(sell)
			{
				ArrowDown sellArrow = Draw.ArrowDown(this, String.Format("sell_arrow_{0}", CurrentBar), true, 0, High[0] + (4 * TickSize), Brushes.Pink);
				
				if(FireAlerts)
				{
					Alert(String.Format("sell_alert_{0}", CurrentBar), Priority.High, alertText,
						NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", 10, Brushes.Red, Brushes.White);
				}
				
				Signal = -1;
			}		

			if (!buy && !sell)
			{
				Signal = 0;
			}
			
//			Draw.TextFixed(this, "smaDiff", String.Format("ATR: {0:N0}, SMA Diff: {1:N2}", tickRange, diff), TextPosition.TopRight,
//				ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, trendBrush, 100);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastMaPeriod", Order=1, GroupName="Parameters")]
		public int FastMaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowMaPeriod", Order=2, GroupName="Parameters")]
		public int SlowMaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TickRangePeriod", Order=3, GroupName="Parameters")]
		public int TickRangePeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MinMaDiff", Order=4, GroupName="Parameters")]
		public double MinMaDiff
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MaxMaDiff", Order=5, GroupName="Parameters")]
		public double MaxMaDiff
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MinTickRange", Order=6, GroupName="Parameters")]
		public double MinTickRange
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MaxTickRange", Order=7, GroupName="Parameters")]
		public double MaxTickRange
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="GoLong", Order=8, GroupName="Parameters")]
		public bool GoLong
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="GoShort", Order=9, GroupName="Parameters")]
		public bool GoShort
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FireAlerts", Order=10, GroupName="Parameters")]
		public bool FireAlerts
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FastMaPlot
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SlowMaPlot
		{
			get { return Values[1]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private THA[] cacheTHA;
		public THA THA(int fastMaPeriod, int slowMaPeriod, int tickRangePeriod, double minMaDiff, double maxMaDiff, double minTickRange, double maxTickRange, bool goLong, bool goShort, bool fireAlerts)
		{
			return THA(Input, fastMaPeriod, slowMaPeriod, tickRangePeriod, minMaDiff, maxMaDiff, minTickRange, maxTickRange, goLong, goShort, fireAlerts);
		}

		public THA THA(ISeries<double> input, int fastMaPeriod, int slowMaPeriod, int tickRangePeriod, double minMaDiff, double maxMaDiff, double minTickRange, double maxTickRange, bool goLong, bool goShort, bool fireAlerts)
		{
			if (cacheTHA != null)
				for (int idx = 0; idx < cacheTHA.Length; idx++)
					if (cacheTHA[idx] != null && cacheTHA[idx].FastMaPeriod == fastMaPeriod && cacheTHA[idx].SlowMaPeriod == slowMaPeriod && cacheTHA[idx].TickRangePeriod == tickRangePeriod && cacheTHA[idx].MinMaDiff == minMaDiff && cacheTHA[idx].MaxMaDiff == maxMaDiff && cacheTHA[idx].MinTickRange == minTickRange && cacheTHA[idx].MaxTickRange == maxTickRange && cacheTHA[idx].GoLong == goLong && cacheTHA[idx].GoShort == goShort && cacheTHA[idx].FireAlerts == fireAlerts && cacheTHA[idx].EqualsInput(input))
						return cacheTHA[idx];
			return CacheIndicator<THA>(new THA(){ FastMaPeriod = fastMaPeriod, SlowMaPeriod = slowMaPeriod, TickRangePeriod = tickRangePeriod, MinMaDiff = minMaDiff, MaxMaDiff = maxMaDiff, MinTickRange = minTickRange, MaxTickRange = maxTickRange, GoLong = goLong, GoShort = goShort, FireAlerts = fireAlerts }, input, ref cacheTHA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.THA THA(int fastMaPeriod, int slowMaPeriod, int tickRangePeriod, double minMaDiff, double maxMaDiff, double minTickRange, double maxTickRange, bool goLong, bool goShort, bool fireAlerts)
		{
			return indicator.THA(Input, fastMaPeriod, slowMaPeriod, tickRangePeriod, minMaDiff, maxMaDiff, minTickRange, maxTickRange, goLong, goShort, fireAlerts);
		}

		public Indicators.THA THA(ISeries<double> input , int fastMaPeriod, int slowMaPeriod, int tickRangePeriod, double minMaDiff, double maxMaDiff, double minTickRange, double maxTickRange, bool goLong, bool goShort, bool fireAlerts)
		{
			return indicator.THA(input, fastMaPeriod, slowMaPeriod, tickRangePeriod, minMaDiff, maxMaDiff, minTickRange, maxTickRange, goLong, goShort, fireAlerts);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.THA THA(int fastMaPeriod, int slowMaPeriod, int tickRangePeriod, double minMaDiff, double maxMaDiff, double minTickRange, double maxTickRange, bool goLong, bool goShort, bool fireAlerts)
		{
			return indicator.THA(Input, fastMaPeriod, slowMaPeriod, tickRangePeriod, minMaDiff, maxMaDiff, minTickRange, maxTickRange, goLong, goShort, fireAlerts);
		}

		public Indicators.THA THA(ISeries<double> input , int fastMaPeriod, int slowMaPeriod, int tickRangePeriod, double minMaDiff, double maxMaDiff, double minTickRange, double maxTickRange, bool goLong, bool goShort, bool fireAlerts)
		{
			return indicator.THA(input, fastMaPeriod, slowMaPeriod, tickRangePeriod, minMaDiff, maxMaDiff, minTickRange, maxTickRange, goLong, goShort, fireAlerts);
		}
	}
}

#endregion
