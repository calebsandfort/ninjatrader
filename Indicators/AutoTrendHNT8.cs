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
    public class AutoTrendHNT8 : Indicator
    {
        #region Variables			
        private int triggerBarIndex = 0;
        private Series<int> signal;             // 0 = no signal, 1 = buy signal on down trend break, -1 = sell signal on up trend break
        private Series<int> trendStarted;       // 0 = no signal, 1 = up trend started, -1 = down trend started
        private Series<double> changePerBar;
        private int direction = 0;              // 0 = Undefined or broken trend, 1=active up trend, -1=active down trend
        private double trendPrice = 0;
        private int lineWidth = 1;
        private int lineCount = 0;
        private double startBarPriceOld = 0;
        private string unique = string.Empty;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"";
                Name = "AutoTrendHNT8";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                AlertOnBreak = true;
                Strength = 5;
                ShowHistory = true;
                LimitHistory = false;
                LimitHistoricalLookback = 60;
                DownTrendColor = Brushes.Red;
                UpTrendColor = Brushes.Green;
                DownHistColor = Brushes.Red;
                UpHistColor = Brushes.Green;
            }
            else if (State == State.DataLoaded)
            {
                unique = Guid.NewGuid().ToString();
                signal = new Series<int>(this);
                trendStarted = new Series<int>(this);
                changePerBar = new Series<double>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            // Limit Historical processing in case a chart uses too many bars for calculation (I.E. EURUSD Renko charts)
            if (State == State.Historical && LimitHistory && CurrentBar < Bars.Count - LimitHistoricalLookback)
                return;

            //DETERMINE LOCATION OF LAST UP/DOWN TREND LINES	
            signal[0] = 0;
            trendStarted[0] = 0;
            int upTrendOccurence = 1; int upTrendStartBarsAgo = 0; int upTrendEndBarsAgo = 0;
            int downTrendOccurence = 1; int downTrendStartBarsAgo = 0; int downTrendEndBarsAgo = 0;

            // Only calculate new autotrend line if ray hasent been put into manual mode by unlocking current ray
            if (((DrawObjects["UpTrendRay" + unique] == null) || (DrawObjects["UpTrendRay" + unique].IsLocked)) && ((DrawObjects["DownTrendRay" + unique] == null) || (DrawObjects["DownTrendRay" + unique].IsLocked)))
            {
                //Only do the following if existing ray is in auto mode	
                // Calculate up trend line
                upTrendOccurence = 1;
                while (Low[upTrendEndBarsAgo] <= Low[upTrendStartBarsAgo])
                {
                    upTrendStartBarsAgo = Swing(Strength).SwingLowBar(0, upTrendOccurence + 1, CurrentBar);
                    upTrendEndBarsAgo = Swing(Strength).SwingLowBar(0, upTrendOccurence, CurrentBar);
                    if (upTrendStartBarsAgo < 0 || upTrendEndBarsAgo < 0)
                        break;
                    upTrendOccurence++;
                }
                // Calculate down trend line	
                downTrendOccurence = 1;
                while (High[downTrendEndBarsAgo] >= High[downTrendStartBarsAgo])
                {
                    downTrendStartBarsAgo = Swing(Strength).SwingHighBar(0, downTrendOccurence + 1, CurrentBar);
                    downTrendEndBarsAgo = Swing(Strength).SwingHighBar(0, downTrendOccurence, CurrentBar);
                    if (downTrendStartBarsAgo < 0 || downTrendEndBarsAgo < 0)
                        break;
                    downTrendOccurence++;
                }
            }

            // Clear out arrows that mark trend line breaks unless ShowHistory flag is true
            if (!ShowHistory) RemoveDrawObject("DownTrendBreak" + unique);
            if (!ShowHistory) RemoveDrawObject("UpTrendBreak" + unique);

            //PROCESS UPTREND LINE IF CURRENT
            if (upTrendStartBarsAgo > 0 && upTrendEndBarsAgo > 0 && upTrendStartBarsAgo < downTrendStartBarsAgo)
            {
                RemoveDrawObject("DownTrendRay" + unique);
                double startBarPrice = Low[upTrendStartBarsAgo];
                double endBarPrice = Low[upTrendEndBarsAgo];
                changePerBar[0] = (endBarPrice - startBarPrice) / (Math.Abs(upTrendEndBarsAgo - upTrendStartBarsAgo));
                //Test to see if this is a new trendline and increment lineCounter if so.
                if (startBarPrice != startBarPriceOld)
                {
                    direction = 1;  //Signal that we have a new uptrend and put dot on trendline where new trend detected
                    if (ShowHistory)
                    {
                        Draw.Dot(this, CurrentBar.ToString(), true, 0, startBarPrice + (upTrendStartBarsAgo * changePerBar[0]), UpTrendColor);
                        trendStarted[0] = 1;
                    }
                    lineCount = lineCount + 1;
                    triggerBarIndex = 0;
                    //ResetAlert("Alert");
                }
                startBarPriceOld = startBarPrice;
                //
                // Draw the up trend line
                // If user has unlocked the ray use manual rays position instead of auto generated positions to track ray position
                if ((DrawObjects["UpTrendRay" + unique] != null) && (!DrawObjects["UpTrendRay" + unique].IsLocked))
                {
                    Ray upTrendRay = (Ray)DrawObjects["UpTrendRay" + unique];
                    startBarPrice = upTrendRay.StartAnchor.Price;
                    endBarPrice = upTrendRay.EndAnchor.Price;
                    upTrendStartBarsAgo = upTrendRay.StartAnchor.BarsAgo;
                    upTrendEndBarsAgo = upTrendRay.EndAnchor.BarsAgo;
                    changePerBar[0] = (endBarPrice - startBarPrice) / (Math.Abs(upTrendRay.EndAnchor.BarsAgo - upTrendRay.StartAnchor.BarsAgo));
                }
                else
                {
                    Draw.Ray(this, "UpTrendRay" + unique, false, upTrendStartBarsAgo, startBarPrice, upTrendEndBarsAgo, endBarPrice, UpTrendColor, DashStyleHelper.Solid, lineWidth);
                }

                //Draw the history line that will stay persistent on chart using lineCounter to establish a unique name
                if (ShowHistory) Draw.Line(this, "HistoryLine" + unique + lineCount.ToString(), false, upTrendStartBarsAgo, startBarPrice, 0, startBarPrice + (upTrendStartBarsAgo * changePerBar[0]), UpHistColor, DashStyleHelper.Solid, lineWidth);
                //SET RETURN VALUES FOR INDICATOR
                // Check for an uptrend line break
                trendPrice = (startBarPrice + (upTrendStartBarsAgo * changePerBar[0]));
                for (int barsAgo = upTrendEndBarsAgo - 1; barsAgo >= 0; barsAgo--)
                {
                    if (Close[barsAgo] < endBarPrice + (Math.Abs(upTrendEndBarsAgo - barsAgo) * changePerBar[0]))
                    {
                        if (ShowHistory)
                        {
                            Draw.ArrowDown(this, "UpTrendBreak" + unique + lineCount.ToString(), true, barsAgo, High[barsAgo] + TickSize, DownTrendColor);
                        }
                        else
                        {
                            Draw.ArrowDown(this, "UpTrendBreak" + unique, true, barsAgo, High[barsAgo] + TickSize, DownTrendColor);
                        }
                        // Set the break signal only if the break is on the right most bar
                        //if (barsAgo == 0)
                            signal[barsAgo] = -1;
                        // Alert will only trigger in real-time
                        if (AlertOnBreak && triggerBarIndex == 0)
                        {
                            triggerBarIndex = CurrentBar - upTrendEndBarsAgo;
                            Alert("Alert" + unique, Priority.High, "Up trend line broken", "Alert2.wav", 100000, Brushes.Black, Brushes.Red);
                        }
                        break;
                    }
                }
            }

            else
            //DETECT AND PROCESS DOWNTREND LINE	IF CURRENT	
            if (downTrendStartBarsAgo > 0 && downTrendEndBarsAgo > 0 && upTrendStartBarsAgo > downTrendStartBarsAgo)
            {
                RemoveDrawObject("UpTrendRay" + unique);
                double startBarPrice = High[downTrendStartBarsAgo];
                double endBarPrice = High[downTrendEndBarsAgo];
                changePerBar[0] = (endBarPrice - startBarPrice) / (Math.Abs(downTrendEndBarsAgo - downTrendStartBarsAgo));
                //Test to see if this is a new trendline and increment lineCount if so.
                if (startBarPrice != startBarPriceOld)
                {
                    direction = -1;     //signl that we have a new downtrend
                    if (ShowHistory)
                    {
                        Draw.Dot(this, CurrentBar.ToString(), true, 0, startBarPrice + (downTrendStartBarsAgo * changePerBar[0]), DownTrendColor);
                        trendStarted[0] = -1;
                    }
                    lineCount = lineCount + 1;
                    triggerBarIndex = 0;
                }
                startBarPriceOld = startBarPrice;
                //
                // Draw the down trend line
                // If user has unlocked the ray use manual rays position instead
                if ((DrawObjects["DownTrendRay" + unique] != null) && (!DrawObjects["DownTrendRay" + unique].IsLocked))
                {
                    Ray downTrendRay = (Ray)DrawObjects["DownTrendRay" + unique];
                    startBarPrice = downTrendRay.StartAnchor.Price;
                    endBarPrice = downTrendRay.EndAnchor.Price;
                    downTrendStartBarsAgo = downTrendRay.StartAnchor.BarsAgo;
                    downTrendEndBarsAgo = downTrendRay.EndAnchor.BarsAgo;
                    changePerBar[0] = (endBarPrice - startBarPrice) / (Math.Abs(downTrendRay.EndAnchor.BarsAgo - downTrendRay.StartAnchor.BarsAgo));
                }
                else
                {
                    Draw.Ray(this, "DownTrendRay" + unique, false, downTrendStartBarsAgo, startBarPrice, downTrendEndBarsAgo, endBarPrice, DownTrendColor, DashStyleHelper.Solid, lineWidth);
                }
                if (ShowHistory) Draw.Line(this, "HistoryLine" + unique + lineCount.ToString(), false, downTrendStartBarsAgo, startBarPrice, 0, startBarPrice + (downTrendStartBarsAgo * changePerBar[0]), DownHistColor, DashStyleHelper.Solid, lineWidth);
                //SET RETURN VALUES FOR INDICATOR
                // Check for a down trend line break
                trendPrice = (startBarPrice + (downTrendStartBarsAgo * changePerBar[0]));
                for (int barsAgo = downTrendEndBarsAgo - 1; barsAgo >= 0; barsAgo--)
                {
                    //	direction=-1;
                    if (Close[barsAgo] > endBarPrice + (Math.Abs(downTrendEndBarsAgo - barsAgo) * changePerBar[0]))
                    {
                        if (ShowHistory)
                        {
                            Draw.ArrowUp(this, "DownTrendBreak" + unique + lineCount.ToString(), true, barsAgo, Low[barsAgo] - TickSize, UpTrendColor);
                        }
                        else
                        {
                            Draw.ArrowUp(this, "DownTrendBreak" + unique, true, barsAgo, Low[barsAgo] - TickSize, UpTrendColor);
                        }
                        // Set the break signal only if the break is on the right most bar
                        //if (barsAgo == 0)
                            signal[barsAgo] = 1;
                        // Alert will only trigger in real-time
                        if (AlertOnBreak && triggerBarIndex == 0)
                        {
                            triggerBarIndex = CurrentBar - downTrendEndBarsAgo;
                            Alert("Alert" + unique, Priority.High, "Down trend line broken", "Alert2.wav", 100000, Brushes.Black, Brushes.Green);
                        }
                        break;
                    }
                }
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "AlertOnBreak", Description = "Generates a visual and audible alert on a trend line break", Order = 1, GroupName = "Parameters")]
        public bool AlertOnBreak
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Strength", Description = "Number of bars required on each side swing pivot points used to connect the trend lines", Order = 2, GroupName = "Parameters")]
        public int Strength
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ShowHistory", Description = "Show Historical Trendlines & Breaks", Order = 3, GroupName = "Parameters")]
        public bool ShowHistory
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "LimitHistory", Description = "Limit Historical Trendlines & Breaks", Order = 4, GroupName = "Parameters")]
        public bool LimitHistory
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "LimitHistoricalLookback", Description = "Historical Lookback Period to Limit Trendlines & Breaks", Order = 5, GroupName = "Parameters")]
        public int LimitHistoricalLookback
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> Signal
        {
            get { return signal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> TrendStarted
        {
            get { return trendStarted; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ChangePerBar
        {
            get { return changePerBar; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public int Direction
        {
            get { Update(); return direction; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public double TrendPrice
        {
            get { Update(); return trendPrice; }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "DownTrendColor", Description = "Color of the down trend line.", Order = 6, GroupName = "Parameters")]
        public Brush DownTrendColor
        { get; set; }

        [Browsable(false)]
        public string DownTrendColorSerializable
        {
            get { return Serialize.BrushToString(DownTrendColor); }
            set { DownTrendColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "UpTrendColor", Description = "Color of the up trend line.", Order = 7, GroupName = "Parameters")]
        public Brush UpTrendColor
        { get; set; }

        [Browsable(false)]
        public string UpTrendColorSerializable
        {
            get { return Serialize.BrushToString(UpTrendColor); }
            set { UpTrendColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "DownHistColor", Description = "Color of History down trend line.", Order = 8, GroupName = "Parameters")]
        public Brush DownHistColor
        { get; set; }

        [Browsable(false)]
        public string DownHistColorSerializable
        {
            get { return Serialize.BrushToString(DownHistColor); }
            set { DownHistColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "UpHistColor", Description = "Color of History up trend line.", Order = 9, GroupName = "Parameters")]
        public Brush UpHistColor
        { get; set; }

        [Browsable(false)]
        public string UpHistColorSerializable
        {
            get { return Serialize.BrushToString(UpHistColor); }
            set { UpHistColor = Serialize.StringToBrush(value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AutoTrendHNT8[] cacheAutoTrendHNT8;
		public AutoTrendHNT8 AutoTrendHNT8(bool alertOnBreak, int strength, bool showHistory, bool limitHistory, int limitHistoricalLookback, Brush downTrendColor, Brush upTrendColor, Brush downHistColor, Brush upHistColor)
		{
			return AutoTrendHNT8(Input, alertOnBreak, strength, showHistory, limitHistory, limitHistoricalLookback, downTrendColor, upTrendColor, downHistColor, upHistColor);
		}

		public AutoTrendHNT8 AutoTrendHNT8(ISeries<double> input, bool alertOnBreak, int strength, bool showHistory, bool limitHistory, int limitHistoricalLookback, Brush downTrendColor, Brush upTrendColor, Brush downHistColor, Brush upHistColor)
		{
			if (cacheAutoTrendHNT8 != null)
				for (int idx = 0; idx < cacheAutoTrendHNT8.Length; idx++)
					if (cacheAutoTrendHNT8[idx] != null && cacheAutoTrendHNT8[idx].AlertOnBreak == alertOnBreak && cacheAutoTrendHNT8[idx].Strength == strength && cacheAutoTrendHNT8[idx].ShowHistory == showHistory && cacheAutoTrendHNT8[idx].LimitHistory == limitHistory && cacheAutoTrendHNT8[idx].LimitHistoricalLookback == limitHistoricalLookback && cacheAutoTrendHNT8[idx].DownTrendColor == downTrendColor && cacheAutoTrendHNT8[idx].UpTrendColor == upTrendColor && cacheAutoTrendHNT8[idx].DownHistColor == downHistColor && cacheAutoTrendHNT8[idx].UpHistColor == upHistColor && cacheAutoTrendHNT8[idx].EqualsInput(input))
						return cacheAutoTrendHNT8[idx];
			return CacheIndicator<AutoTrendHNT8>(new AutoTrendHNT8(){ AlertOnBreak = alertOnBreak, Strength = strength, ShowHistory = showHistory, LimitHistory = limitHistory, LimitHistoricalLookback = limitHistoricalLookback, DownTrendColor = downTrendColor, UpTrendColor = upTrendColor, DownHistColor = downHistColor, UpHistColor = upHistColor }, input, ref cacheAutoTrendHNT8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AutoTrendHNT8 AutoTrendHNT8(bool alertOnBreak, int strength, bool showHistory, bool limitHistory, int limitHistoricalLookback, Brush downTrendColor, Brush upTrendColor, Brush downHistColor, Brush upHistColor)
		{
			return indicator.AutoTrendHNT8(Input, alertOnBreak, strength, showHistory, limitHistory, limitHistoricalLookback, downTrendColor, upTrendColor, downHistColor, upHistColor);
		}

		public Indicators.AutoTrendHNT8 AutoTrendHNT8(ISeries<double> input , bool alertOnBreak, int strength, bool showHistory, bool limitHistory, int limitHistoricalLookback, Brush downTrendColor, Brush upTrendColor, Brush downHistColor, Brush upHistColor)
		{
			return indicator.AutoTrendHNT8(input, alertOnBreak, strength, showHistory, limitHistory, limitHistoricalLookback, downTrendColor, upTrendColor, downHistColor, upHistColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AutoTrendHNT8 AutoTrendHNT8(bool alertOnBreak, int strength, bool showHistory, bool limitHistory, int limitHistoricalLookback, Brush downTrendColor, Brush upTrendColor, Brush downHistColor, Brush upHistColor)
		{
			return indicator.AutoTrendHNT8(Input, alertOnBreak, strength, showHistory, limitHistory, limitHistoricalLookback, downTrendColor, upTrendColor, downHistColor, upHistColor);
		}

		public Indicators.AutoTrendHNT8 AutoTrendHNT8(ISeries<double> input , bool alertOnBreak, int strength, bool showHistory, bool limitHistory, int limitHistoricalLookback, Brush downTrendColor, Brush upTrendColor, Brush downHistColor, Brush upHistColor)
		{
			return indicator.AutoTrendHNT8(input, alertOnBreak, strength, showHistory, limitHistory, limitHistoricalLookback, downTrendColor, upTrendColor, downHistColor, upHistColor);
		}
	}
}

#endregion
