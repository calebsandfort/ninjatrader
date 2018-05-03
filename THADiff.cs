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
	public class THADiff : Indicator
	{
		private Series<double> diffSeries;
		private Brush BullishBrush = Brushes.DeepSkyBlue;
		private Brush BearishBrush = Brushes.HotPink;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"THADiff";
				Name										= "THADiff";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				DiffMaLength					= 15;
				SmaThreshold					= 0.4;
				FastMaPeriod					= 10;
				SlowMaPeriod					= 20;
				AddPlot(Brushes.Orange, "DiffPlot");
				
				Stroke aStroke = new Stroke(Brushes.White, DashStyleHelper.Dash, 1);
				AddLine(aStroke, SmaThreshold, "UpperThreshold");
				AddLine(aStroke, -SmaThreshold, "LowerThreshold");
				
				Plots[0].Width= 2;
				
				diffSeries = new Series<double>(this);
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < SlowMaPeriod) return;
			
			double fastMa = EMA(FastMaPeriod)[0];
			double slowMa = EMA(SlowMaPeriod)[0];
			
			
			double diff = fastMa - slowMa;
			diffSeries[0] = diff;
			double diffMa = EMA(diffSeries, DiffMaLength)[0];
			DiffPlot[0] = diff;
			
			PlotBrushes[0][0] = (diff > SmaThreshold || diff < -SmaThreshold) ? (fastMa < slowMa ? BearishBrush : BullishBrush) : Brushes.White;
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="DiffMaLength", Order=1, GroupName="Parameters")]
		public int DiffMaLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="SmaThreshold", Order=2, GroupName="Parameters")]
		public double SmaThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastMaPeriod", Order=3, GroupName="Parameters")]
		public int FastMaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowMaPeriod", Order=4, GroupName="Parameters")]
		public int SlowMaPeriod
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffPlot
		{
			get { return Values[0]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private THADiff[] cacheTHADiff;
		public THADiff THADiff(int diffMaLength, double smaThreshold, int fastMaPeriod, int slowMaPeriod)
		{
			return THADiff(Input, diffMaLength, smaThreshold, fastMaPeriod, slowMaPeriod);
		}

		public THADiff THADiff(ISeries<double> input, int diffMaLength, double smaThreshold, int fastMaPeriod, int slowMaPeriod)
		{
			if (cacheTHADiff != null)
				for (int idx = 0; idx < cacheTHADiff.Length; idx++)
					if (cacheTHADiff[idx] != null && cacheTHADiff[idx].DiffMaLength == diffMaLength && cacheTHADiff[idx].SmaThreshold == smaThreshold && cacheTHADiff[idx].FastMaPeriod == fastMaPeriod && cacheTHADiff[idx].SlowMaPeriod == slowMaPeriod && cacheTHADiff[idx].EqualsInput(input))
						return cacheTHADiff[idx];
			return CacheIndicator<THADiff>(new THADiff(){ DiffMaLength = diffMaLength, SmaThreshold = smaThreshold, FastMaPeriod = fastMaPeriod, SlowMaPeriod = slowMaPeriod }, input, ref cacheTHADiff);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.THADiff THADiff(int diffMaLength, double smaThreshold, int fastMaPeriod, int slowMaPeriod)
		{
			return indicator.THADiff(Input, diffMaLength, smaThreshold, fastMaPeriod, slowMaPeriod);
		}

		public Indicators.THADiff THADiff(ISeries<double> input , int diffMaLength, double smaThreshold, int fastMaPeriod, int slowMaPeriod)
		{
			return indicator.THADiff(input, diffMaLength, smaThreshold, fastMaPeriod, slowMaPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.THADiff THADiff(int diffMaLength, double smaThreshold, int fastMaPeriod, int slowMaPeriod)
		{
			return indicator.THADiff(Input, diffMaLength, smaThreshold, fastMaPeriod, slowMaPeriod);
		}

		public Indicators.THADiff THADiff(ISeries<double> input , int diffMaLength, double smaThreshold, int fastMaPeriod, int slowMaPeriod)
		{
			return indicator.THADiff(input, diffMaLength, smaThreshold, fastMaPeriod, slowMaPeriod);
		}
	}
}

#endregion
