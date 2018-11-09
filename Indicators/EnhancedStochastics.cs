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
	public class EnhancedStochastics : Indicator
    {
        private Series<double> den;
        private Series<double> fastK;
        private MIN min;
        private MAX max;
        private Series<double> nom;
        private SMA smaFastK;
        private SMA smaK;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionStochastics;
                Name = NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameStochastics;
                IsSuspendedWhileInactive = true;
                PeriodD = 7;
                PeriodK = 14;
                Smooth = 3;
                DColor = Brushes.DodgerBlue;
                KColor = Brushes.Goldenrod;
            }
            else if (State == State.Configure)
            {
                AddPlot(DColor, NinjaTrader.Custom.Resource.StochasticsD);
                AddPlot(KColor, NinjaTrader.Custom.Resource.StochasticsK);
                AddLine(Brushes.PaleGoldenrod, 20, NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
                AddLine(Brushes.PaleGoldenrod, 80, NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
            }
            else if (State == State.DataLoaded)
            {
                den = new Series<double>(this);
                nom = new Series<double>(this);
                fastK = new Series<double>(this);
                min = MIN(Low, PeriodK);
                max = MAX(High, PeriodK);
                smaFastK = SMA(fastK, Smooth);
                smaK = SMA(K, PeriodD);
            }
        }

        protected override void OnBarUpdate()
        {
            double min0 = min[0];
            nom[0] = Close[0] - min0;
            den[0] = max[0] - min0;

            if (den[0].ApproxCompare(0) == 0)
                fastK[0] = CurrentBar == 0 ? 50 : fastK[1];
            else
                fastK[0] = Math.Min(100, Math.Max(0, 100 * nom[0] / den[0]));

            // Slow %K == Fast %D
            K[0] = smaFastK[0];
            D[0] = smaK[0];
        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> D
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> K
        {
            get { return Values[1]; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "PeriodD", GroupName = "NinjaScriptParameters", Order = 0)]
        public int PeriodD
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "PeriodK", GroupName = "NinjaScriptParameters", Order = 1)]
        public int PeriodK
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "NinjaScriptParameters", Order = 2)]
        public int Smooth
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "DColor", Order = 3, GroupName = "NinjaScriptParameters")]
        public Brush DColor
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "KColor", Order = 4, GroupName = "NinjaScriptParameters")]
        public Brush KColor
        { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private EnhancedStochastics[] cacheEnhancedStochastics;
		public EnhancedStochastics EnhancedStochastics(int periodD, int periodK, int smooth, Brush dColor, Brush kColor)
		{
			return EnhancedStochastics(Input, periodD, periodK, smooth, dColor, kColor);
		}

		public EnhancedStochastics EnhancedStochastics(ISeries<double> input, int periodD, int periodK, int smooth, Brush dColor, Brush kColor)
		{
			if (cacheEnhancedStochastics != null)
				for (int idx = 0; idx < cacheEnhancedStochastics.Length; idx++)
					if (cacheEnhancedStochastics[idx] != null && cacheEnhancedStochastics[idx].PeriodD == periodD && cacheEnhancedStochastics[idx].PeriodK == periodK && cacheEnhancedStochastics[idx].Smooth == smooth && cacheEnhancedStochastics[idx].DColor == dColor && cacheEnhancedStochastics[idx].KColor == kColor && cacheEnhancedStochastics[idx].EqualsInput(input))
						return cacheEnhancedStochastics[idx];
			return CacheIndicator<EnhancedStochastics>(new EnhancedStochastics(){ PeriodD = periodD, PeriodK = periodK, Smooth = smooth, DColor = dColor, KColor = kColor }, input, ref cacheEnhancedStochastics);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EnhancedStochastics EnhancedStochastics(int periodD, int periodK, int smooth, Brush dColor, Brush kColor)
		{
			return indicator.EnhancedStochastics(Input, periodD, periodK, smooth, dColor, kColor);
		}

		public Indicators.EnhancedStochastics EnhancedStochastics(ISeries<double> input , int periodD, int periodK, int smooth, Brush dColor, Brush kColor)
		{
			return indicator.EnhancedStochastics(input, periodD, periodK, smooth, dColor, kColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EnhancedStochastics EnhancedStochastics(int periodD, int periodK, int smooth, Brush dColor, Brush kColor)
		{
			return indicator.EnhancedStochastics(Input, periodD, periodK, smooth, dColor, kColor);
		}

		public Indicators.EnhancedStochastics EnhancedStochastics(ISeries<double> input , int periodD, int periodK, int smooth, Brush dColor, Brush kColor)
		{
			return indicator.EnhancedStochastics(input, periodD, periodK, smooth, dColor, kColor);
		}
	}
}

#endregion
