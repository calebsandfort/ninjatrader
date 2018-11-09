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
	public class SmaSlope : Indicator
	{
        private SMA smaIndicator;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"SmaSlope";
				Name										= "SmaSlope";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
                Period = 14;
                SlopePeriod = 14;
                PositiveThreshold = 20;
                NegativeThreshold = -20;
				Inverse = false;

                AddLine(Brushes.LightGray, 0, "SmaSlopeZeroLine");
            }
			else if (State == State.Configure)
			{
                AddLine(Brushes.Green, PositiveThreshold, "SmaSlopePositiveThresholdLine");
                AddLine(Brushes.Green, NegativeThreshold, "SmaSlopeNegativeThresholdLine");
                AddPlot(Brushes.DarkCyan, "SmaSlope");
                smaIndicator = SMA(Period);
            }
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar < Period)
            {
                Value[0] = 0;
                return;
            }

            Double slope = Slope(smaIndicator, SlopePeriod, 0);
            Value[0] = Math.Atan(slope) * 180 / Math.PI;

            if (this.GoZone)
            {
                PlotBrushes[0][0] = Brushes.DarkGreen;
            }
            else
            {
                PlotBrushes[0][0] = Brushes.DarkRed;
            }
        }

        #region Properties
        public bool GoZone
        {
            get
            {
				if(this.Inverse){
					return Value[0] < PositiveThreshold && Value[0] > NegativeThreshold;
				}
				else{
                	return Value[0] > PositiveThreshold || Value[0] < NegativeThreshold;
				}
            }
		}
		
        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
        public int Period
        { get; set; }

        [Range(2, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SlopePeriod", GroupName = "NinjaScriptParameters", Order = 1)]
        public int SlopePeriod
        { get; set; }

        [Range(double.MinValue, double.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "PositiveThreshold", GroupName = "NinjaScriptParameters", Order = 2)]
        public double PositiveThreshold
        { get; set; }

        [Range(double.MinValue, double.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "NegativeThreshold", GroupName = "NinjaScriptParameters", Order = 3)]
        public double NegativeThreshold
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Inverse", GroupName = "NinjaScriptParameters", Order = 4)]
        public bool Inverse
        { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SmaSlope[] cacheSmaSlope;
		public SmaSlope SmaSlope(int period, int slopePeriod, double positiveThreshold, double negativeThreshold, bool inverse)
		{
			return SmaSlope(Input, period, slopePeriod, positiveThreshold, negativeThreshold, inverse);
		}

		public SmaSlope SmaSlope(ISeries<double> input, int period, int slopePeriod, double positiveThreshold, double negativeThreshold, bool inverse)
		{
			if (cacheSmaSlope != null)
				for (int idx = 0; idx < cacheSmaSlope.Length; idx++)
					if (cacheSmaSlope[idx] != null && cacheSmaSlope[idx].Period == period && cacheSmaSlope[idx].SlopePeriod == slopePeriod && cacheSmaSlope[idx].PositiveThreshold == positiveThreshold && cacheSmaSlope[idx].NegativeThreshold == negativeThreshold && cacheSmaSlope[idx].Inverse == inverse && cacheSmaSlope[idx].EqualsInput(input))
						return cacheSmaSlope[idx];
			return CacheIndicator<SmaSlope>(new SmaSlope(){ Period = period, SlopePeriod = slopePeriod, PositiveThreshold = positiveThreshold, NegativeThreshold = negativeThreshold, Inverse = inverse }, input, ref cacheSmaSlope);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SmaSlope SmaSlope(int period, int slopePeriod, double positiveThreshold, double negativeThreshold, bool inverse)
		{
			return indicator.SmaSlope(Input, period, slopePeriod, positiveThreshold, negativeThreshold, inverse);
		}

		public Indicators.SmaSlope SmaSlope(ISeries<double> input , int period, int slopePeriod, double positiveThreshold, double negativeThreshold, bool inverse)
		{
			return indicator.SmaSlope(input, period, slopePeriod, positiveThreshold, negativeThreshold, inverse);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SmaSlope SmaSlope(int period, int slopePeriod, double positiveThreshold, double negativeThreshold, bool inverse)
		{
			return indicator.SmaSlope(Input, period, slopePeriod, positiveThreshold, negativeThreshold, inverse);
		}

		public Indicators.SmaSlope SmaSlope(ISeries<double> input , int period, int slopePeriod, double positiveThreshold, double negativeThreshold, bool inverse)
		{
			return indicator.SmaSlope(input, period, slopePeriod, positiveThreshold, negativeThreshold, inverse);
		}
	}
}

#endregion
