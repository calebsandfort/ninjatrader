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
	public class MyRange : Indicator
	{
        private double tickRange;
        private Series<double> highLowSeries;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MyRange";
				IsSuspendedWhileInactive					= true;
                RangePeriod = 15;
                MinRange = 8.5;
				AddPlot(Brushes.Orange, "TickRangePlot");
			}
			else if (State == State.Configure)
			{
				AddLine(Brushes.Red, MinRange, "MinRange");
            }
            else if (State == State.DataLoaded)
            {
                highLowSeries = new Series<double>(this);
            }
        }

		protected override void OnBarUpdate()
		{
			if (CurrentBar < RangePeriod) return;

            highLowSeries[0] = (High[0] - Low[0]) / TickSize;
            double tickRange = EMA(highLowSeries, RangePeriod)[0];

			TickRangePlot[0] = tickRange;
			PlotBrushes[0][0] = tickRange > MinRange ? Brushes.Green : Brushes.White;
            Value[0] = tickRange;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RangePeriod", Order=1, GroupName="Parameters")]
		public int RangePeriod
		{ get; set; }
			
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "MinRange", Order = 18, GroupName = "Parameters")]
        public double MinRange
        { get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TickRangePlot
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
		private MyRange[] cacheMyRange;
		public MyRange MyRange(int rangePeriod, double minRange)
		{
			return MyRange(Input, rangePeriod, minRange);
		}

		public MyRange MyRange(ISeries<double> input, int rangePeriod, double minRange)
		{
			if (cacheMyRange != null)
				for (int idx = 0; idx < cacheMyRange.Length; idx++)
					if (cacheMyRange[idx] != null && cacheMyRange[idx].RangePeriod == rangePeriod && cacheMyRange[idx].MinRange == minRange && cacheMyRange[idx].EqualsInput(input))
						return cacheMyRange[idx];
			return CacheIndicator<MyRange>(new MyRange(){ RangePeriod = rangePeriod, MinRange = minRange }, input, ref cacheMyRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MyRange MyRange(int rangePeriod, double minRange)
		{
			return indicator.MyRange(Input, rangePeriod, minRange);
		}

		public Indicators.MyRange MyRange(ISeries<double> input , int rangePeriod, double minRange)
		{
			return indicator.MyRange(input, rangePeriod, minRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MyRange MyRange(int rangePeriod, double minRange)
		{
			return indicator.MyRange(Input, rangePeriod, minRange);
		}

		public Indicators.MyRange MyRange(ISeries<double> input , int rangePeriod, double minRange)
		{
			return indicator.MyRange(input, rangePeriod, minRange);
		}
	}
}

#endregion
