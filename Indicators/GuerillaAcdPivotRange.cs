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
using NinjaTrader.Core;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class GuerillaAcdPivotRange : Indicator
	{
        private DateTime currentDate = Core.Globals.MinDate;
        private double currentClose = 0;
        private double currentHigh = 0;
        private double currentLow = 0;
        private double currentOpen = 0;
        private double priorDayClose = 0;
        private double priorDayHigh = 0;
        private double priorDayLow = 0;
        private double priorDayOpen = 0;
        private Data.SessionIterator sessionIterator;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionPriorDayOHLC;
                Name = NinjaTrader.Custom.Resource.NinjaScriptIndicatorNamePriorDayOHLC;
                IsAutoScale = false;
                IsOverlay = true;
                IsSuspendedWhileInactive = true;
                DrawOnPricePanel = false;
                ShowClose = true;
                ShowLow = true;
                ShowHigh = true;
                ShowOpen = true;

                AddPlot(new Stroke(Brushes.SteelBlue, DashStyleHelper.Dash, 2), PlotStyle.Hash, NinjaTrader.Custom.Resource.PriorDayOHLCOpen);
                AddPlot(new Stroke(Brushes.DarkCyan, 2), PlotStyle.Hash, NinjaTrader.Custom.Resource.PriorDayOHLCHigh);
                AddPlot(new Stroke(Brushes.Crimson, 2), PlotStyle.Hash, NinjaTrader.Custom.Resource.PriorDayOHLCLow);
                AddPlot(new Stroke(Brushes.SlateBlue, DashStyleHelper.Dash, 2), PlotStyle.Hash, NinjaTrader.Custom.Resource.PriorDayOHLCClose);
            }
            else if (State == State.Configure)
            {
                currentDate = Core.Globals.MinDate;
                currentClose = 0;
                currentHigh = 0;
                currentLow = 0;
                currentOpen = 0;
                priorDayClose = 0;
                priorDayHigh = 0;
                priorDayLow = 0;
                priorDayOpen = 0;
                sessionIterator = null;
            }
            else if (State == State.DataLoaded)
            {
                sessionIterator = new Data.SessionIterator(Bars);
            }
            else if (State == State.Historical)
            {
                if (!Bars.BarsType.IsIntraday)
                {
                    Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PriorDayOHLCIntradayError, TextPosition.BottomRight);
                    Log(NinjaTrader.Custom.Resource.PriorDayOHLCIntradayError, LogLevel.Error);
                }
            }
        }

        protected override void OnBarUpdate()
        {
            if (!Bars.BarsType.IsIntraday)
                return;

            // If the current date is not the same date as the current bar then its a new session
            if (currentDate != sessionIterator.GetTradingDay(Time[0]) || currentOpen == 0)
            {
                // The current day OHLC values are now the prior days value so set
                // them to their respect indicator series for plotting
                priorDayOpen = currentOpen;
                priorDayHigh = currentHigh;
                priorDayLow = currentLow;
                priorDayClose = currentClose;

                if (ShowOpen) PriorOpen[0] = priorDayOpen;
                if (ShowHigh) PriorHigh[0] = priorDayHigh;
                if (ShowLow) PriorLow[0] = priorDayLow;
                if (ShowClose) PriorClose[0] = priorDayClose;

                // Initilize the current day settings to the new days data
                currentOpen = Open[0];
                currentHigh = High[0];
                currentLow = Low[0];
                currentClose = Close[0];

                currentDate = sessionIterator.GetTradingDay(Time[0]);
            }
            else // The current day is the same day
            {
                // Set the current day OHLC values
                currentHigh = Math.Max(currentHigh, High[0]);
                currentLow = Math.Min(currentLow, Low[0]);
                currentClose = Close[0];

                if (ShowOpen) PriorOpen[0] = priorDayOpen;
                if (ShowHigh) PriorHigh[0] = priorDayHigh;
                if (ShowLow) PriorLow[0] = priorDayLow;
                if (ShowClose) PriorClose[0] = priorDayClose;
            }
        }

        #region Properties
        [Browsable(false)]  // this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]       // this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> PriorOpen
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriorHigh
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriorLow
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PriorClose
        {
            get { return Values[3]; }
        }

        [Display(ResourceType = typeof(Custom.Resource), Name = "ShowClose", GroupName = "NinjaScriptParameters", Order = 0)]
        public bool ShowClose
        { get; set; }

        [Display(ResourceType = typeof(Custom.Resource), Name = "ShowHigh", GroupName = "NinjaScriptParameters", Order = 1)]
        public bool ShowHigh
        { get; set; }

        [Display(ResourceType = typeof(Custom.Resource), Name = "ShowLow", GroupName = "NinjaScriptParameters", Order = 2)]
        public bool ShowLow
        { get; set; }

        [Display(ResourceType = typeof(Custom.Resource), Name = "ShowOpen", GroupName = "NinjaScriptParameters", Order = 3)]
        public bool ShowOpen
        { get; set; }
        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GuerillaAcdPivotRange[] cacheGuerillaAcdPivotRange;
		public GuerillaAcdPivotRange GuerillaAcdPivotRange()
		{
			return GuerillaAcdPivotRange(Input);
		}

		public GuerillaAcdPivotRange GuerillaAcdPivotRange(ISeries<double> input)
		{
			if (cacheGuerillaAcdPivotRange != null)
				for (int idx = 0; idx < cacheGuerillaAcdPivotRange.Length; idx++)
					if (cacheGuerillaAcdPivotRange[idx] != null &&  cacheGuerillaAcdPivotRange[idx].EqualsInput(input))
						return cacheGuerillaAcdPivotRange[idx];
			return CacheIndicator<GuerillaAcdPivotRange>(new GuerillaAcdPivotRange(), input, ref cacheGuerillaAcdPivotRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GuerillaAcdPivotRange GuerillaAcdPivotRange()
		{
			return indicator.GuerillaAcdPivotRange(Input);
		}

		public Indicators.GuerillaAcdPivotRange GuerillaAcdPivotRange(ISeries<double> input )
		{
			return indicator.GuerillaAcdPivotRange(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GuerillaAcdPivotRange GuerillaAcdPivotRange()
		{
			return indicator.GuerillaAcdPivotRange(Input);
		}

		public Indicators.GuerillaAcdPivotRange GuerillaAcdPivotRange(ISeries<double> input )
		{
			return indicator.GuerillaAcdPivotRange(input);
		}
	}
}

#endregion
