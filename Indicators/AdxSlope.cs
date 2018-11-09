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
	public class AdxSlope : Indicator
	{
		
        private ADX adxIndicator;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AdxSlope";
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
				Period = 14;
				SlopePeriod = 3;
				AddLine(Brushes.LightGray,	0,	"AdxSlopeZeroLine");
			}
			else if (State == State.Configure)
			{
				AddLine(Threshold < 0 ? Brushes.Red : Brushes.Green, Threshold,	"AdxSlopeThresholdLine");
				AddPlot(Brushes.DarkCyan, "AdxSlope");
				adxIndicator = ADX(Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < Period) {
				Value[0] = 0;
				return;
			}
			
			Double slope = Slope(adxIndicator, SlopePeriod, 0);
            Value[0] = Math.Atan(slope) * 180 / Math.PI;
			
			if(Threshold < 0 && Value[0] < Threshold){
				PlotBrushes[0][0] = Brushes.DarkRed;
			}
			else if(Threshold > 0 && Value[0] > Threshold){
				PlotBrushes[0][0] = Brushes.DarkGreen;
			}
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SlopePeriod", GroupName = "NinjaScriptParameters", Order = 1)]
		public int SlopePeriod
		{ get; set; }
		
		[Range(double.MinValue, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Threshold", GroupName = "NinjaScriptParameters", Order = 2)]
		public double Threshold
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AdxSlope[] cacheAdxSlope;
		public AdxSlope AdxSlope(int period, int slopePeriod, double threshold)
		{
			return AdxSlope(Input, period, slopePeriod, threshold);
		}

		public AdxSlope AdxSlope(ISeries<double> input, int period, int slopePeriod, double threshold)
		{
			if (cacheAdxSlope != null)
				for (int idx = 0; idx < cacheAdxSlope.Length; idx++)
					if (cacheAdxSlope[idx] != null && cacheAdxSlope[idx].Period == period && cacheAdxSlope[idx].SlopePeriod == slopePeriod && cacheAdxSlope[idx].Threshold == threshold && cacheAdxSlope[idx].EqualsInput(input))
						return cacheAdxSlope[idx];
			return CacheIndicator<AdxSlope>(new AdxSlope(){ Period = period, SlopePeriod = slopePeriod, Threshold = threshold }, input, ref cacheAdxSlope);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AdxSlope AdxSlope(int period, int slopePeriod, double threshold)
		{
			return indicator.AdxSlope(Input, period, slopePeriod, threshold);
		}

		public Indicators.AdxSlope AdxSlope(ISeries<double> input , int period, int slopePeriod, double threshold)
		{
			return indicator.AdxSlope(input, period, slopePeriod, threshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AdxSlope AdxSlope(int period, int slopePeriod, double threshold)
		{
			return indicator.AdxSlope(Input, period, slopePeriod, threshold);
		}

		public Indicators.AdxSlope AdxSlope(ISeries<double> input , int period, int slopePeriod, double threshold)
		{
			return indicator.AdxSlope(input, period, slopePeriod, threshold);
		}
	}
}

#endregion
