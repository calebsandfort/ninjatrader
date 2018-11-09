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
	public class AdxDiff : Indicator
	{
		private Series<double>		dmPlus;
		private Series<double>		dmMinus;
		private Series<double>		sumDmPlus;
		private Series<double>		sumDmMinus;
		private Series<double>		sumTr;
		private Series<double>		tr;
		private Series<double>		diffSeries;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MyRange";
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
                Period						= 14;
                MinAdx = 25;
				AddPlot(Brushes.Orange, "AdxDiffPlot");
			}
			else if (State == State.Configure)
			{
            }
            else if (State == State.DataLoaded)
            {
                dmPlus = new Series<double>(this);
                dmMinus = new Series<double>(this);
                sumDmPlus = new Series<double>(this);
                sumDmMinus = new Series<double>(this);
                sumTr = new Series<double>(this);
                tr = new Series<double>(this);
                diffSeries = new Series<double>(this);
            }
        }

		protected override void OnBarUpdate()
		{
			double high0	= High[0];
			double low0		= Low[0];

			if (CurrentBar == 0)
			{
				tr[0]				= high0 - low0;
				dmPlus[0]			= 0;
				dmMinus[0]			= 0;
				sumTr[0]			= tr[0];
				sumDmPlus[0]		= dmPlus[0];
				sumDmMinus[0]		= dmMinus[0];
				Value[0]			= 50;
			}
			else
			{
				double high1		= High[1];
				double low1			= Low[1];
				double close1		= Close[1];

				tr[0]				= Math.Max(Math.Abs(low0 - close1), Math.Max(high0 - low0, Math.Abs(high0 - close1)));
				dmPlus[0]			= high0 - high1 > low1 - low0 ? Math.Max(high0 - high1, 0) : 0;
				dmMinus[0]			= low1 - low0 > high0 - high1 ? Math.Max(low1 - low0, 0) : 0;

				if (CurrentBar < Period)
				{
					sumTr[0]		= sumTr[1] + tr[0];
					sumDmPlus[0]	= sumDmPlus[1] + dmPlus[0];
					sumDmMinus[0]	= sumDmMinus[1] + dmMinus[0];
				}
				else
				{
					double sumTr1		= sumTr[1];
					double sumDmPlus1	= sumDmPlus[1];
					double sumDmMinus1	= sumDmMinus[1];

					sumTr[0]			= sumTr1 - sumTr1 / Period + tr[0];
					sumDmPlus[0]		= sumDmPlus1 - sumDmPlus1 / Period + dmPlus[0];
					sumDmMinus[0]		= sumDmMinus1 - sumDmMinus1 / Period + dmMinus[0];
				}

				double sumTr0		= sumTr[0];
				double diPlus		= 100 * (sumTr0.ApproxCompare(0) == 0 ? 0 : sumDmPlus[0] / sumTr[0]);
				double diMinus		= 100 * (sumTr0.ApproxCompare(0) == 0 ? 0 : sumDmMinus[0] / sumTr[0]);
				diffSeries[0]		= diPlus - diMinus;
				double diffEma 		= EMA(diffSeries, 5)[0];
				
				AdxDiffPlot[0] = diffEma;
            	Value[0] = diffEma;
			}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }

        [Range(1, double.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "MinAdx", GroupName = "NinjaScriptParameters", Order = 1)]
        public double MinAdx
        { get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AdxDiffPlot
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
		private AdxDiff[] cacheAdxDiff;
		public AdxDiff AdxDiff(int period, double minAdx)
		{
			return AdxDiff(Input, period, minAdx);
		}

		public AdxDiff AdxDiff(ISeries<double> input, int period, double minAdx)
		{
			if (cacheAdxDiff != null)
				for (int idx = 0; idx < cacheAdxDiff.Length; idx++)
					if (cacheAdxDiff[idx] != null && cacheAdxDiff[idx].Period == period && cacheAdxDiff[idx].MinAdx == minAdx && cacheAdxDiff[idx].EqualsInput(input))
						return cacheAdxDiff[idx];
			return CacheIndicator<AdxDiff>(new AdxDiff(){ Period = period, MinAdx = minAdx }, input, ref cacheAdxDiff);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AdxDiff AdxDiff(int period, double minAdx)
		{
			return indicator.AdxDiff(Input, period, minAdx);
		}

		public Indicators.AdxDiff AdxDiff(ISeries<double> input , int period, double minAdx)
		{
			return indicator.AdxDiff(input, period, minAdx);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AdxDiff AdxDiff(int period, double minAdx)
		{
			return indicator.AdxDiff(Input, period, minAdx);
		}

		public Indicators.AdxDiff AdxDiff(ISeries<double> input , int period, double minAdx)
		{
			return indicator.AdxDiff(input, period, minAdx);
		}
	}
}

#endregion
