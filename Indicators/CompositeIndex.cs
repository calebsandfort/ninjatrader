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
	public class CompositeIndex : Indicator
	{
        #region properties
        RSI rsi;
        RSI shortRsi;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "CompositeIndex";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                BarsRequiredToPlot = 20;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                RsiLength = 14;
                RsiMomLength = 9;
                RsiMaLength = 3;
                MaLength = 3;

                AddPlot(Brushes.Red, "S");
            }
            else if (State == State.DataLoaded)
            {
                rsi = RSI(RsiLength, 3);
                shortRsi = RSI(RsiMaLength, MaLength);
            }
        } 
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            Momentum rsiDelta = Momentum(rsi, RsiMomLength);

            S[0] = rsiDelta[0] + shortRsi.Avg[0];
        }
        #endregion

        #region Properties
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> S
        {
            get { return Values[0]; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "RsiLength", GroupName = "NinjaScriptParameters", Order = 0)]
        public int RsiLength
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "RsiMomLength", GroupName = "NinjaScriptParameters", Order = 1)]
        public int RsiMomLength
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "RsiMaLength", GroupName = "NinjaScriptParameters", Order = 2)]
        public int RsiMaLength
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "MaLength", GroupName = "NinjaScriptParameters", Order = 3)]
        public int MaLength
        { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CompositeIndex[] cacheCompositeIndex;
		public CompositeIndex CompositeIndex(int rsiLength, int rsiMomLength, int rsiMaLength, int maLength)
		{
			return CompositeIndex(Input, rsiLength, rsiMomLength, rsiMaLength, maLength);
		}

		public CompositeIndex CompositeIndex(ISeries<double> input, int rsiLength, int rsiMomLength, int rsiMaLength, int maLength)
		{
			if (cacheCompositeIndex != null)
				for (int idx = 0; idx < cacheCompositeIndex.Length; idx++)
					if (cacheCompositeIndex[idx] != null && cacheCompositeIndex[idx].RsiLength == rsiLength && cacheCompositeIndex[idx].RsiMomLength == rsiMomLength && cacheCompositeIndex[idx].RsiMaLength == rsiMaLength && cacheCompositeIndex[idx].MaLength == maLength && cacheCompositeIndex[idx].EqualsInput(input))
						return cacheCompositeIndex[idx];
			return CacheIndicator<CompositeIndex>(new CompositeIndex(){ RsiLength = rsiLength, RsiMomLength = rsiMomLength, RsiMaLength = rsiMaLength, MaLength = maLength }, input, ref cacheCompositeIndex);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CompositeIndex CompositeIndex(int rsiLength, int rsiMomLength, int rsiMaLength, int maLength)
		{
			return indicator.CompositeIndex(Input, rsiLength, rsiMomLength, rsiMaLength, maLength);
		}

		public Indicators.CompositeIndex CompositeIndex(ISeries<double> input , int rsiLength, int rsiMomLength, int rsiMaLength, int maLength)
		{
			return indicator.CompositeIndex(input, rsiLength, rsiMomLength, rsiMaLength, maLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CompositeIndex CompositeIndex(int rsiLength, int rsiMomLength, int rsiMaLength, int maLength)
		{
			return indicator.CompositeIndex(Input, rsiLength, rsiMomLength, rsiMaLength, maLength);
		}

		public Indicators.CompositeIndex CompositeIndex(ISeries<double> input , int rsiLength, int rsiMomLength, int rsiMaLength, int maLength)
		{
			return indicator.CompositeIndex(input, rsiLength, rsiMomLength, rsiMaLength, maLength);
		}
	}
}

#endregion
