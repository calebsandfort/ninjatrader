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
	public class AcdDailyPivotPrice : Indicator
    {
        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AcdDailyPivotPrice";
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
			}
			else if (State == State.Configure)
            {
                AddPlot(Brushes.Orange, "AcdDailyPivotPricePlot");
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress == 0)
            {
                if (CurrentBar == 0)
                {
                    return;
                }
                else
                {
                    Value[0] = (High[1] + Low[1] + Close[1]) / 3;
                }
            }
        }

        #region Properties

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> AcdDailyPivotPricePlot
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
		private AcdDailyPivotPrice[] cacheAcdDailyPivotPrice;
		public AcdDailyPivotPrice AcdDailyPivotPrice()
		{
			return AcdDailyPivotPrice(Input);
		}

		public AcdDailyPivotPrice AcdDailyPivotPrice(ISeries<double> input)
		{
			if (cacheAcdDailyPivotPrice != null)
				for (int idx = 0; idx < cacheAcdDailyPivotPrice.Length; idx++)
					if (cacheAcdDailyPivotPrice[idx] != null &&  cacheAcdDailyPivotPrice[idx].EqualsInput(input))
						return cacheAcdDailyPivotPrice[idx];
			return CacheIndicator<AcdDailyPivotPrice>(new AcdDailyPivotPrice(), input, ref cacheAcdDailyPivotPrice);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AcdDailyPivotPrice AcdDailyPivotPrice()
		{
			return indicator.AcdDailyPivotPrice(Input);
		}

		public Indicators.AcdDailyPivotPrice AcdDailyPivotPrice(ISeries<double> input )
		{
			return indicator.AcdDailyPivotPrice(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AcdDailyPivotPrice AcdDailyPivotPrice()
		{
			return indicator.AcdDailyPivotPrice(Input);
		}

		public Indicators.AcdDailyPivotPrice AcdDailyPivotPrice(ISeries<double> input )
		{
			return indicator.AcdDailyPivotPrice(input);
		}
	}
}

#endregion
