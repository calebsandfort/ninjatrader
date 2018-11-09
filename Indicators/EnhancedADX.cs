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
	public class EnhancedADX : Indicator
	{
		private Series<double>		dmPlus;
		private Series<double>		dmMinus;
		private Series<double>		sumDmPlus;
		private Series<double>		sumDmMinus;
		private Series<double>		sumTr;
		private Series<double>		tr;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionADX;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameADX;
				IsSuspendedWhileInactive	= true;
				Period						= 14;
                MinAdx = 25;


                AddPlot(Brushes.SkyBlue,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameADX);
				AddPlot(Brushes.Green, "DmiPlusPlot");
				AddPlot(Brushes.Red, "DmiMinusPlot");
				AddLine(Brushes.Goldenrod,	75,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
			}
            else if (State == State.Configure)
            {
                AddLine(Brushes.SlateBlue, MinAdx, NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
            }
			else if (State == State.DataLoaded)
			{
				dmPlus		= new Series<double>(this);
				dmMinus		= new Series<double>(this);
				sumDmPlus	= new Series<double>(this);
				sumDmMinus	= new Series<double>(this);
				sumTr		= new Series<double>(this);
				tr			= new Series<double>(this);
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
				double diff			= Math.Abs(diPlus - diMinus);
				double sum			= diPlus + diMinus;

				Value[0]			= sum.ApproxCompare(0) == 0 ? 50 : ((Period - 1) * Value[1] + 100 * diff / sum) / Period;
				Values[1][0] = diPlus;
				Values[2][0] = diMinus;
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
		public Series<double> DmiPlusPlot
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DmiMinusPlot
		{
			get { return Values[2]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private EnhancedADX[] cacheEnhancedADX;
		public EnhancedADX EnhancedADX(int period, double minAdx)
		{
			return EnhancedADX(Input, period, minAdx);
		}

		public EnhancedADX EnhancedADX(ISeries<double> input, int period, double minAdx)
		{
			if (cacheEnhancedADX != null)
				for (int idx = 0; idx < cacheEnhancedADX.Length; idx++)
					if (cacheEnhancedADX[idx] != null && cacheEnhancedADX[idx].Period == period && cacheEnhancedADX[idx].MinAdx == minAdx && cacheEnhancedADX[idx].EqualsInput(input))
						return cacheEnhancedADX[idx];
			return CacheIndicator<EnhancedADX>(new EnhancedADX(){ Period = period, MinAdx = minAdx }, input, ref cacheEnhancedADX);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EnhancedADX EnhancedADX(int period, double minAdx)
		{
			return indicator.EnhancedADX(Input, period, minAdx);
		}

		public Indicators.EnhancedADX EnhancedADX(ISeries<double> input , int period, double minAdx)
		{
			return indicator.EnhancedADX(input, period, minAdx);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EnhancedADX EnhancedADX(int period, double minAdx)
		{
			return indicator.EnhancedADX(Input, period, minAdx);
		}

		public Indicators.EnhancedADX EnhancedADX(ISeries<double> input , int period, double minAdx)
		{
			return indicator.EnhancedADX(input, period, minAdx);
		}
	}
}

#endregion
