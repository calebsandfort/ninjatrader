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
	public class EnhancedStochasticsFast : Indicator
    {
        #region Indicator Variables
        private Series<double> den;
        private MAX max;
        private MIN min;
        private Series<double> nom;
        private SMA smaK; 
        #endregion

        #region Trend Variables			
        private int triggerBarIndex = 0;
        private Series<int> signal;                 // 0 = no signal, 1 = buy signal on down trend break, -1 = sell signal on up trend break
        private Series<int> direction;              // 0 = Undefined or broken trend, 1=active up trend, -1=active down trend
        private Series<double> trendPrice;
        private Series<double> trendStartPrice;
        private Series<double> trendEndPrice;
        private Series<bool> crossBelowUpperThreshold;
        private Series<bool> crossAboveLowerThreshold;
        private int lineWidth = 1;
        private int lineCount = 0;
        private double startBarPriceOld = 0;
        private string unique = string.Empty;

        private Brush DownTrendColor = Brushes.Red;
        private Brush UpTrendColor = Brushes.Green;

        private Brush DownHistColor = Brushes.DarkRed;
        private Brush UpHistColor = Brushes.DarkGreen;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionStochasticsFast;
                Name = String.Format("Enhanced{0}", NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameStochasticsFast);
                IsSuspendedWhileInactive = true;
                PeriodD = 3;
                PeriodK = 14;
                DColor = Brushes.DodgerBlue;
                KColor = Brushes.Goldenrod;
                DetectTrends = false;
                Strength = 1;
            }
            else if(State == State.Configure)
            {
                AddPlot(DColor, NinjaTrader.Custom.Resource.StochasticsD);
                AddPlot(KColor, NinjaTrader.Custom.Resource.StochasticsK);
                AddPlot(new Stroke(Brushes.LightBlue, DashStyleHelper.Solid, 1), PlotStyle.Square, "TrendInfo");
                AddLine(Brushes.PaleGoldenrod, 20, NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
                AddLine(Brushes.PaleGoldenrod, 80, NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
            }
            else if (State == State.DataLoaded)
            {
                den = new Series<double>(this);
                nom = new Series<double>(this);
                min = MIN(Low, PeriodK);
                max = MAX(High, PeriodK);
                smaK = SMA(K, PeriodD);

                unique = Guid.NewGuid().ToString();

                signal = new Series<int>(this);
                direction = new Series<int>(this);
                trendPrice = new Series<double>(this);
                trendStartPrice = new Series<double>(this);
                trendEndPrice = new Series<double>(this);
                crossBelowUpperThreshold = new Series<bool>(this);
                crossAboveLowerThreshold = new Series<bool>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            #region Indicator
            double min0 = min[0];
            nom[0] = Close[0] - min0;
            den[0] = max[0] - min0;

            if (den[0].ApproxCompare(0) == 0)
                K[0] = CurrentBar == 0 ? 50 : K[1];
            else
                K[0] = Math.Min(100, Math.Max(0, 100 * nom[0] / den[0]));

            D[0] = smaK[0];

            CrossBelowUpperThreshold[0] = CrossBelow(D, 80, 1);
            CrossAboveLowerThreshold[0] = CrossAbove(D, 20, 1);
            #endregion

            #region TrendDetection
            if (this.DetectTrends)
            {
                //DETERMINE LOCATION OF LAST UP/DOWN TREND LINES	
                signal[0] = 0;
                trendStartPrice[0] = 0;
                trendEndPrice[0] = 0;
                int upTrendOccurence = 1; int upTrendStartBarsAgo = 0; int upTrendEndBarsAgo = 0;
                int downTrendOccurence = 1; int downTrendStartBarsAgo = 0; int downTrendEndBarsAgo = 0;

                upTrendOccurence = 1;
                //Goes until first found or reaches lookback period
                while (D[upTrendEndBarsAgo] <= D[upTrendStartBarsAgo])
                {
                    upTrendStartBarsAgo = Swing(D, Strength).SwingLowBar(0, upTrendOccurence + 1, CurrentBar);
                    upTrendEndBarsAgo = Swing(D, Strength).SwingLowBar(0, upTrendOccurence, CurrentBar);
                    if (upTrendStartBarsAgo < 0 || upTrendEndBarsAgo < 0)
                        break;
                    upTrendOccurence++;
                }

                // Calculate down trend line	
                downTrendOccurence = 1;
                while (D[downTrendEndBarsAgo] >= D[downTrendStartBarsAgo])
                {
                    downTrendStartBarsAgo = Swing(D, Strength).SwingHighBar(0, downTrendOccurence + 1, CurrentBar);
                    downTrendEndBarsAgo = Swing(D, Strength).SwingHighBar(0, downTrendOccurence, CurrentBar);
                    if (downTrendStartBarsAgo < 0 || downTrendEndBarsAgo < 0)
                        break;
                    downTrendOccurence++;
                }

                //PROCESS UPTREND LINE IF CURRENT
                if (upTrendStartBarsAgo > 0 && upTrendEndBarsAgo > 0 && upTrendStartBarsAgo < downTrendStartBarsAgo)
                {
                    RemoveDrawObject("DownTrendRay" + unique);
                    double startBarPrice = D[upTrendStartBarsAgo];
                    double endBarPrice = D[upTrendEndBarsAgo];
                    double changePerBar = (endBarPrice - startBarPrice) / (Math.Abs(upTrendEndBarsAgo - upTrendStartBarsAgo));
                    //Test to see if this is a new trendline and increment lineCounter if so.
                    if (startBarPrice != startBarPriceOld)
                    {
                        direction[0] = 1;  //Signal that we have a new uptrend and put dot on trendline where new trend detected
                                           //if (ShowHistory)
                                           //{
                                           Draw.Dot(this, CurrentBar.ToString(), true, 0, startBarPrice + (upTrendStartBarsAgo * changePerBar), UpTrendColor, false);
                        trendStartPrice[0] = startBarPrice;
                        trendEndPrice[0] = endBarPrice;
                        //}
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
                        changePerBar = (endBarPrice - startBarPrice) / (Math.Abs(upTrendRay.EndAnchor.BarsAgo - upTrendRay.StartAnchor.BarsAgo));
                    }
                    else
                    {
                        Draw.Ray(this, "UpTrendRay" + unique, false, upTrendStartBarsAgo, startBarPrice, upTrendEndBarsAgo, endBarPrice, UpTrendColor, DashStyleHelper.Solid, lineWidth, false);
                    }

                    //Draw the history line that will stay persistent on chart using lineCounter to establish a unique name
                    //if (ShowHistory)
                    //{
                    Draw.Line(this, "HistoryLine" + unique + lineCount.ToString(), false, upTrendStartBarsAgo, startBarPrice, 0, startBarPrice + (upTrendStartBarsAgo * changePerBar), UpHistColor, DashStyleHelper.Solid, lineWidth, false);
                    //}

                    //SET RETURN VALUES FOR INDICATOR
                    // Check for an uptrend line break
                    trendPrice[0] = (startBarPrice + (upTrendStartBarsAgo * changePerBar));
                    for (int barsAgo = upTrendEndBarsAgo - 1; barsAgo >= 0; barsAgo--)
                    {
                        if (D[barsAgo] < endBarPrice + (Math.Abs(upTrendEndBarsAgo - barsAgo) * changePerBar))
                        {
                            //if (ShowHistory)
                            //{
                            Draw.ArrowDown(this, "UpTrendBreak" + unique + lineCount.ToString(), true, barsAgo, D[barsAgo] + TickSize, DownTrendColor, false);
                            //}
                            //else
                            //{
                            //    Draw.ArrowDown(this, "UpTrendBreak" + unique, true, barsAgo, D[barsAgo] + TickSize, DownTrendColor, false);
                            //}
                            // Set the break signal only if the break is on the right most bar
                            if (barsAgo == 0)
                                signal[0] = -1;
                            // Alert will only trigger in real-time
                            //if (AlertOnBreak && triggerBarIndex == 0)
                            //{
                            //    triggerBarIndex = CurrentBar - upTrendEndBarsAgo;
                            //    Alert("Alert" + unique, Priority.High, "Up trend line broken", "Alert2.wav", 100000, Brushes.Black, Brushes.Red);
                            //}
                            break;
                        }
                    }
                }

                else
                //DETECT AND PROCESS DOWNTREND LINE	IF CURRENT	
                if (downTrendStartBarsAgo > 0 && downTrendEndBarsAgo > 0 && upTrendStartBarsAgo > downTrendStartBarsAgo)
                {
                    RemoveDrawObject("UpTrendRay" + unique);
                    double startBarPrice = D[downTrendStartBarsAgo];
                    double endBarPrice = D[downTrendEndBarsAgo];
                    double changePerBar = (endBarPrice - startBarPrice) / (Math.Abs(downTrendEndBarsAgo - downTrendStartBarsAgo));
                    //Test to see if this is a new trendline and increment lineCount if so.
                    if (startBarPrice != startBarPriceOld)
                    {
                        direction[0] = -1;     //signl that we have a new downtrend
                                            //if (ShowHistory)
                                            //{
                                            Draw.Dot(this, CurrentBar.ToString(), true, 0, startBarPrice + (downTrendStartBarsAgo * changePerBar), DownTrendColor, false);
                        trendStartPrice[0] = startBarPrice;
                        trendEndPrice[0] = endBarPrice;
                        //}
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
                        changePerBar = (endBarPrice - startBarPrice) / (Math.Abs(downTrendRay.EndAnchor.BarsAgo - downTrendRay.StartAnchor.BarsAgo));
                    }
                    else
                    {
                        Draw.Ray(this, "DownTrendRay" + unique, false, downTrendStartBarsAgo, startBarPrice, downTrendEndBarsAgo, endBarPrice, DownTrendColor, DashStyleHelper.Solid, lineWidth, false);
                    }
                    //if (ShowHistory)
                    //{
                    Draw.Line(this, "HistoryLine" + unique + lineCount.ToString(), false, downTrendStartBarsAgo, startBarPrice, 0, startBarPrice + (downTrendStartBarsAgo * changePerBar), DownHistColor, DashStyleHelper.Solid, lineWidth, false);
                    //}
                    //SET RETURN VALUES FOR INDICATOR
                    // Check for a down trend line break
                    trendPrice[0] = (startBarPrice + (downTrendStartBarsAgo * changePerBar));
                    for (int barsAgo = downTrendEndBarsAgo - 1; barsAgo >= 0; barsAgo--)
                    {
                        //	direction=-1;
                        if (D[barsAgo] > endBarPrice + (Math.Abs(downTrendEndBarsAgo - barsAgo) * changePerBar))
                        {
                            //if (ShowHistory)
                            //{
                            Draw.ArrowUp(this, "DownTrendBreak" + unique + lineCount.ToString(), true, barsAgo, D[barsAgo] - TickSize, UpTrendColor, false);
                            //}
                            //else
                            //{
                            //    Draw.ArrowUp(this, "DownTrendBreak" + unique, true, barsAgo, D[barsAgo] - TickSize, UpTrendColor, false);
                            //}
                            // Set the break signal only if the break is on the right most bar
                            if (barsAgo == 0)
                                signal[0] = 1;
                            // Alert will only trigger in real-time
                            //if (AlertOnBreak && triggerBarIndex == 0)
                            //{
                            //    triggerBarIndex = CurrentBar - downTrendEndBarsAgo;
                            //    Alert("Alert" + unique, Priority.High, "Down trend line broken", "Alert2.wav", 100000, Brushes.Black, Brushes.Green);
                            //}
                            break;
                        }
                    }
                }

                //switch (direction[0])
                //{
                //    case -1:
                //        TrendInfo[0] = 35;
                //        break;
                //    case 0:
                //        TrendInfo[0] = 50;
                //        break;
                //    case 1:
                //        TrendInfo[0] = 65;
                //        break;
                //}

                //TrendInfo[0] = trendEndPrice[0];
            }
            #endregion
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


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> TrendInfo
        {
            get { return Values[2]; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "PeriodD", GroupName = "NinjaScriptParameters", Order = 0)]
        public int PeriodD
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "PeriodK", GroupName = "NinjaScriptParameters", Order = 1)]
        public int PeriodK
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "DColor", Order = 2, GroupName = "NinjaScriptParameters")]
        public Brush DColor
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "KColor", Order = 3, GroupName = "NinjaScriptParameters")]
        public Brush KColor
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "DetectTrends", Order = 5, GroupName = "NinjaScriptParameters")]
        public bool DetectTrends
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Strength", Description = "Number of bars required on each side swing pivot points used to connect the trend lines", Order = 2, GroupName = "NinjaScriptParameters")]
        public int Strength
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> Signal
        {
            get { return signal; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> Direction
        {
            get { return direction; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TrendPrice
        {
            get { return trendPrice; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TrendStartPrice
        {
            get { return trendStartPrice; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TrendEndPrice
        {
            get { return trendEndPrice; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> CrossBelowUpperThreshold
        {
            get { return crossBelowUpperThreshold; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> CrossAboveLowerThreshold
        {
            get { return crossAboveLowerThreshold; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private EnhancedStochasticsFast[] cacheEnhancedStochasticsFast;
		public EnhancedStochasticsFast EnhancedStochasticsFast(int periodD, int periodK, Brush dColor, Brush kColor, bool detectTrends, int strength)
		{
			return EnhancedStochasticsFast(Input, periodD, periodK, dColor, kColor, detectTrends, strength);
		}

		public EnhancedStochasticsFast EnhancedStochasticsFast(ISeries<double> input, int periodD, int periodK, Brush dColor, Brush kColor, bool detectTrends, int strength)
		{
			if (cacheEnhancedStochasticsFast != null)
				for (int idx = 0; idx < cacheEnhancedStochasticsFast.Length; idx++)
					if (cacheEnhancedStochasticsFast[idx] != null && cacheEnhancedStochasticsFast[idx].PeriodD == periodD && cacheEnhancedStochasticsFast[idx].PeriodK == periodK && cacheEnhancedStochasticsFast[idx].DColor == dColor && cacheEnhancedStochasticsFast[idx].KColor == kColor && cacheEnhancedStochasticsFast[idx].DetectTrends == detectTrends && cacheEnhancedStochasticsFast[idx].Strength == strength && cacheEnhancedStochasticsFast[idx].EqualsInput(input))
						return cacheEnhancedStochasticsFast[idx];
			return CacheIndicator<EnhancedStochasticsFast>(new EnhancedStochasticsFast(){ PeriodD = periodD, PeriodK = periodK, DColor = dColor, KColor = kColor, DetectTrends = detectTrends, Strength = strength }, input, ref cacheEnhancedStochasticsFast);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EnhancedStochasticsFast EnhancedStochasticsFast(int periodD, int periodK, Brush dColor, Brush kColor, bool detectTrends, int strength)
		{
			return indicator.EnhancedStochasticsFast(Input, periodD, periodK, dColor, kColor, detectTrends, strength);
		}

		public Indicators.EnhancedStochasticsFast EnhancedStochasticsFast(ISeries<double> input , int periodD, int periodK, Brush dColor, Brush kColor, bool detectTrends, int strength)
		{
			return indicator.EnhancedStochasticsFast(input, periodD, periodK, dColor, kColor, detectTrends, strength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EnhancedStochasticsFast EnhancedStochasticsFast(int periodD, int periodK, Brush dColor, Brush kColor, bool detectTrends, int strength)
		{
			return indicator.EnhancedStochasticsFast(Input, periodD, periodK, dColor, kColor, detectTrends, strength);
		}

		public Indicators.EnhancedStochasticsFast EnhancedStochasticsFast(ISeries<double> input , int periodD, int periodK, Brush dColor, Brush kColor, bool detectTrends, int strength)
		{
			return indicator.EnhancedStochasticsFast(input, periodD, periodK, dColor, kColor, detectTrends, strength);
		}
	}
}

#endregion
