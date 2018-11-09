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
	public class BarVelocity : Indicator
    {
        private Series<double> timeDiff;
        private EMA timeDiffEma;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BarVelocity";
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
				Period					= 6;
				AddPlot(Brushes.HotPink, "BarVelocityPlot");
			}
			else if (State == State.DataLoaded)
			{
                timeDiff = new Series<double>(this);
                timeDiffEma = EMA(timeDiff, Period);
            }
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar == 0) return;

            timeDiff[0] = (Time[0] - Time[1]).TotalMilliseconds;
            BarVelocityPlot[0] = timeDiffEma[0];
        }

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BarVelocityPlot
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
		private BarVelocity[] cacheBarVelocity;
		public BarVelocity BarVelocity(int period)
		{
			return BarVelocity(Input, period);
		}

		public BarVelocity BarVelocity(ISeries<double> input, int period)
		{
			if (cacheBarVelocity != null)
				for (int idx = 0; idx < cacheBarVelocity.Length; idx++)
					if (cacheBarVelocity[idx] != null && cacheBarVelocity[idx].Period == period && cacheBarVelocity[idx].EqualsInput(input))
						return cacheBarVelocity[idx];
			return CacheIndicator<BarVelocity>(new BarVelocity(){ Period = period }, input, ref cacheBarVelocity);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BarVelocity BarVelocity(int period)
		{
			return indicator.BarVelocity(Input, period);
		}

		public Indicators.BarVelocity BarVelocity(ISeries<double> input , int period)
		{
			return indicator.BarVelocity(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BarVelocity BarVelocity(int period)
		{
			return indicator.BarVelocity(Input, period);
		}

		public Indicators.BarVelocity BarVelocity(ISeries<double> input , int period)
		{
			return indicator.BarVelocity(input, period);
		}
	}
}

#endregion
