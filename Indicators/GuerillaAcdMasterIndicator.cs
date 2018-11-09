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
	public class GuerillaAcdMasterIndicator : Indicator
	{
        #region props
        private Brush textColor = Brushes.WhiteSmoke;
        private Gui.Tools.SimpleFont textFont;

        private DateTime currentDate = Core.Globals.MinDate;
        private double opening = 0;
        private double openingHigh = 0;
        private double openingLow = 0;
        private double currentAUp = 0;
        private double currentADown = 0;
        private double currentCUp = 0;
        private double currentCDown = 0;

        private double currentClose = 0;
        private double currentHigh = 0;
        private double currentLow = 0;
        private double priorDayClose = 0;
        private double priorDayHigh = 0;
        private double priorDayLow = 0;
        private double pivotUpperBound = 0;
        private double pivotLowerBound = 0;
        private double rollingPivotUpperBound = 0;
        private double rollingPivotLowerBound = 0;

        private int aUpBreachedBar = 0;
        private int aUpSuccessBar = 0;
        private int aDownBreachedBar = 0;
        private int aDownSuccessBar = 0;
        private int cUpBreachedBar = 0;
        private int cUpSuccessBar = 0;
        private int cDownBreachedBar = 0;
        private int cDownSuccessBar = 0;

        private Data.SessionIterator sessionIterator;
        private Data.SessionIterator fastSessionIterator;
        private int halfTimeFrame = 0;
        private int halfTimeFrameBars = 0;
        private int openingBars = 0;
        private Series<double> fastPivotPriceSeries;
        private DateTime fastCurrentDate = Core.Globals.MinDate;
        private int fastCurrentBar;

        private Series<double> trueRangeSeries;
        private Series<double> aSeries;
        private Series<double> cSeries;
        private bool setACValues = false;

        private Series<double> dayNumberLineSeries;
        private Series<double> numberLineSeries;
        private List<NumberLineEvents> currentNumberLineEvents = new List<NumberLineEvents>();
        private OpeningRangeCloses currentOpeningRangeClose = OpeningRangeCloses.None;
        private int dayCurrentBar;
        private double numberLineMinusOneSum = 0;
        public double numberLineDropOffScore = 0;


        private String redrawTag = String.Empty;
        private String redrawText = String.Empty;

        private SolidColorBrush openingRangeBrush = Brushes.LightGray;
        private SolidColorBrush aBrush = Brushes.HotPink;
        private SolidColorBrush cBrush = Brushes.Lime;
        private SolidColorBrush pivotBrush = Brushes.DodgerBlue;
        private SolidColorBrush rollingPivotBrush = Brushes.Yellow;
        private SolidColorBrush transparentBrush = Brushes.Transparent;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "GuerillaAcdMasterIndicator";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                IsAutoScale = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                textFont = new Gui.Tools.SimpleFont() { Size = 14 };
                textColor = Brushes.WhiteSmoke;

                ATicks = 28;
                CTicks = 16;
                ProjectHour = 13;
                ProjectMinute = 0;
                AddPlot(new Stroke(openingRangeBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "OpenHigh");
                AddPlot(new Stroke(openingRangeBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "OpenLow");
                AddPlot(new Stroke(aBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "AUp");
                AddPlot(new Stroke(aBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "ADown");
                AddPlot(new Stroke(cBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "CUp");
                AddPlot(new Stroke(cBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "CDown");
                AddPlot(new Stroke(pivotBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "PivotHigh");
                AddPlot(new Stroke(pivotBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "PivotLow");
                AddPlot(new Stroke(rollingPivotBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "RollingPivotHigh");
                AddPlot(new Stroke(rollingPivotBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "RollingPivotLow");
                AddPlot(new Stroke(transparentBrush, DashStyleHelper.Dash, 2), PlotStyle.Square, "MyOpen");
            }
            else if (State == State.Configure)
            {
                AddDataSeries(BarsPeriodType.Second, 30);
                AddDataSeries(BarsPeriodType.Day, 1);

                currentDate = Core.Globals.MinDate;
                sessionIterator = null;
                fastSessionIterator = null;

                opening = 0;
                openingHigh = 0;
                openingLow = 0;
                currentAUp = 0;
                currentADown = 0;
                currentCUp = 0;
                currentCDown = 0;

                currentClose = 0;
                currentHigh = 0;
                currentLow = 0;
                priorDayClose = 0;
                priorDayHigh = 0;
                priorDayLow = 0;
                priorDayLow = 0;
                priorDayLow = 0;

                aUpBreachedBar = 0;
                aUpSuccessBar = 0;
                aDownBreachedBar = 0;
                aDownSuccessBar = 0;
                cUpBreachedBar = 0;
                cUpSuccessBar = 0;
                pivotUpperBound = 0;
                pivotLowerBound = 0;
            }
            else if (State == State.DataLoaded)
            {
                sessionIterator = new Data.SessionIterator(BarsArray[0]);
                fastPivotPriceSeries = new Series<double>(BarsArray[1]);
                fastSessionIterator = new Data.SessionIterator(BarsArray[1]);
                halfTimeFrame = BarsArray[0].BarsPeriod.Value / 2;
                halfTimeFrameBars = halfTimeFrame * 2;
                openingBars = BarsArray[0].BarsPeriod.Value * 2;

                trueRangeSeries = new Series<double>(BarsArray[2]);
                aSeries = new Series<double>(BarsArray[2]);
                cSeries = new Series<double>(BarsArray[2]);

                dayNumberLineSeries = new Series<double>(BarsArray[2]);
                numberLineSeries = new Series<double>(BarsArray[2]);
            }
        }
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (BarsInProgress == 0)
            {
                if (currentDate != sessionIterator.GetTradingDay(Time[0]))
                {
                    opening = Open[0];
                    openingHigh = High[0];
                    openingLow = Low[0];
                    currentAUp = openingHigh + (setACValues ? aSeries[0] : (TickSize * this.ATicks));
                    currentADown = openingLow - (setACValues ? aSeries[0] : (TickSize * this.ATicks));
                    currentCUp = openingHigh + (setACValues ? cSeries[0] : (TickSize * this.CTicks));
                    currentCDown = openingLow - (setACValues ? cSeries[0] : (TickSize * this.CTicks));

                    MyOpen[0] = opening;
                    OpenHigh[0] = openingHigh;
                    OpenLow[0] = openingLow;
                    AUp[0] = currentAUp;
                    ADown[0] = currentADown;
                    CUp[0] = currentCUp;
                    CDown[0] = currentCDown;

                    currentDate = sessionIterator.GetTradingDay(Time[0]);
                    aUpBreachedBar = 0;
                    aUpSuccessBar = 0;
                    aDownBreachedBar = 0;
                    aDownSuccessBar = 0;
                    cUpBreachedBar = 0;
                    cUpSuccessBar = 0;
                    cDownBreachedBar = 0;
                    cDownSuccessBar = 0;

                    MyOpen[0] = Open[0];

                    #region Pivot
                    priorDayHigh = currentHigh;
                    priorDayLow = currentLow;
                    priorDayClose = currentClose;

                    double pivotPrice = (priorDayHigh + priorDayLow + priorDayClose) / 3;
                    double pivotHighLowMiddle = (priorDayHigh + priorDayLow) / 2;
                    double pivotDifferential = Math.Abs(pivotPrice - pivotHighLowMiddle);
                    pivotUpperBound = pivotPrice + pivotDifferential;
                    pivotLowerBound = pivotPrice - pivotDifferential;
                    PivotHigh[0] = pivotUpperBound;
                    PivotLow[0] = pivotLowerBound;

                    // Initilize the current day settings to the new days data
                    currentHigh = High[0];
                    currentLow = Low[0];
                    currentClose = Close[0];
                    #endregion

                    #region NumberLine
                    if (CurrentBars[2] > 36)
                    {
                        String numberLineText = String.Format("Outlook: {8}{1}NL: {0}{1}Days: {2}, {3}, {4}{1}Trend: {5}, {6}, {7}{1}Drop: {9}",
                            numberLineSeries[0].ToString("+#;-#;0"),
                            Environment.NewLine,
                            dayNumberLineSeries[2].ToString("+#;-#;0"),
                            dayNumberLineSeries[1].ToString("+#;-#;0"),
                            dayNumberLineSeries[0].ToString("+#;-#;0"),
                            numberLineSeries[2].ToString("+#;-#;0"),
                            numberLineSeries[1].ToString("+#;-#;0"),
                            numberLineSeries[0].ToString("+#;-#;0"),
                            Close[1] > PivotHigh[0] ? "Plus" : Close[1] < PivotLow[0] ? "Minus" : "Neutral",
                            numberLineDropOffScore.ToString("+#;-#;0"));

                        Draw.Text(this, String.Format("NumberLine_{0}", currentDate.ToShortDateString()), false, numberLineText, 5,
                            priorDayLow, 10,
                            textColor, textFont, TextAlignment.Justify, Brushes.Transparent, Brushes.Transparent, 0);
                    } 
                    #endregion
                }
                else
                {
                    MyOpen[0] = opening;
                    OpenHigh[0] = openingHigh;
                    OpenLow[0] = openingLow;
                    AUp[0] = currentAUp;
                    ADown[0] = currentADown;
                    CUp[0] = currentCUp;
                    CDown[0] = currentCDown;

                    #region Pivot
                    currentHigh = Math.Max(currentHigh, High[0]);
                    currentLow = Math.Min(currentLow, Low[0]);
                    currentClose = Close[0];

                    DateTime projectTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.ProjectHour, this.ProjectMinute, 0);

                    if (Time[0] >= projectTime)
                    {
                        double pivotPrice = (currentHigh + currentLow + currentClose) / 3;
                        double pivotHighLowMiddle = (currentHigh + currentLow) / 2;
                        double pivotDifferential = Math.Abs(pivotPrice - pivotHighLowMiddle);
                        PivotHigh[0] = pivotPrice + pivotDifferential;
                        PivotLow[0] = pivotPrice - pivotDifferential;
                    }
                    else
                    {
                        PivotHigh[0] = pivotUpperBound;
                        PivotLow[0] = pivotLowerBound;
                    }
                    #endregion

                    #region NumberLine
                    AddNumberLineEvents(currentNumberLineEvents,
                        new KeyValuePair<NumberLineEvents, bool>(NumberLineEvents.AUpBreached, aUpBreachedBar > 0),
                        new KeyValuePair<NumberLineEvents, bool>(NumberLineEvents.AUpSuccess, aUpSuccessBar > 0),
                        new KeyValuePair<NumberLineEvents, bool>(NumberLineEvents.ADownBreached, aDownBreachedBar > 0),
                        new KeyValuePair<NumberLineEvents, bool>(NumberLineEvents.ADownSuccess, aDownSuccessBar > 0),
                        new KeyValuePair<NumberLineEvents, bool>(NumberLineEvents.CUpBreached, cUpBreachedBar > 0),
                        new KeyValuePair<NumberLineEvents, bool>(NumberLineEvents.CUpSuccess, cUpSuccessBar > 0),
                        new KeyValuePair<NumberLineEvents, bool>(NumberLineEvents.CDownBreached, cDownBreachedBar > 0),
                        new KeyValuePair<NumberLineEvents, bool>(NumberLineEvents.CDownSuccess, cDownSuccessBar > 0));

                    currentOpeningRangeClose = GetOpeningRangeClose(Close[0]);

                    if (CurrentBars[2] > 36)
                    {
                        double tempNumberLineScore = ScoreNumberLineMaster();

                        //PrintValues(numberLineSeries[0], tempNumberLineSum, tempNumberLineScore);

                        String numberLineText = String.Format("Drop: {1}{0}Add: {2}{0}Proj: {3}{0}OR: {4}{0}",
                            Environment.NewLine,
                            numberLineDropOffScore.ToString("+#;-#;0"),
                            tempNumberLineScore.ToString("+#;-#;0"),
                            (numberLineMinusOneSum + tempNumberLineScore).ToString("+#;-#;0"),
                            (Math.Abs(openingHigh - openingLow).ToString("C")));

                        Draw.TextFixed(this, "numberLineToday", numberLineText, textPosition: TextPosition.BottomRight);
                    }
                    #endregion
                }

                RollingPivotHigh[0] = rollingPivotUpperBound;
                RollingPivotLow[0] = rollingPivotLowerBound;

                if (!String.IsNullOrEmpty(redrawTag) && !String.IsNullOrEmpty(redrawText))
                {
                    bool isUp = redrawTag.StartsWith("Up_");

                    Draw.Text(this, redrawTag, false, redrawText, 0,
                        isUp ? Math.Max(High[0], High[1]) : Math.Min(Low[0], Low[1]),
                        isUp ? 20 : -10,
                        textColor, textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                    redrawTag = String.Empty;
                    redrawText = String.Empty;
                }
            }
            else if (BarsInProgress == 1)
            {
                fastPivotPriceSeries[0] = (High[0] + Low[0] + Close[0]) / 3;

                if (fastCurrentDate != sessionIterator.GetTradingDay(Time[0]))
                {
                    fastCurrentDate = sessionIterator.GetTradingDay(Time[0]);
                    fastCurrentBar = 1;
                }
                else
                {
                    fastCurrentBar += 1;
                }

                if (fastCurrentBar >= openingBars)
                {
                    #region A Up
                    if (aUpBreachedBar == 0 && aDownSuccessBar == 0 && fastPivotPriceSeries[0] > currentAUp)
                    {
                        aUpBreachedBar = CurrentBar;
                    }

                    if (aUpBreachedBar > 0
                        && aUpSuccessBar == 0
                        && aDownSuccessBar == 0
                        && (CurrentBar - aUpBreachedBar) >= halfTimeFrameBars
                        && CountIf(() => fastPivotPriceSeries[0] > currentAUp, halfTimeFrameBars) >= halfTimeFrameBars)
                    {
                        aUpSuccessBar = CurrentBar;

                        redrawTag = "Up_" + currentDate.ToShortDateString();
                        redrawText = " A Up ";

                        Draw.Text(this, redrawTag, false, redrawText, 0, Highs[0][0], 50, textColor,
                                    textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                    #endregion

                    #region A Down
                    if (aDownBreachedBar == 0 && aUpSuccessBar == 0 && fastPivotPriceSeries[0] < currentADown)
                    {
                        aDownBreachedBar = CurrentBars[0];
                    }

                    if (aDownBreachedBar > 0
                        && aUpSuccessBar == 0
                        && aDownSuccessBar == 0
                        && (CurrentBar - aDownBreachedBar) >= halfTimeFrameBars
                        && CountIf(() => fastPivotPriceSeries[0] < currentADown, halfTimeFrameBars) >= halfTimeFrameBars)
                    {
                        aDownSuccessBar = CurrentBars[0];

                        redrawTag = "Down_" + currentDate.ToShortDateString();
                        redrawText = " A Down ";

                        Draw.Text(this, redrawTag, false, redrawText, 0, Lows[0][0], -50, textColor,
                                    textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                    }
                    #endregion

                    #region C Up
                    if (aDownSuccessBar > 0 && cUpBreachedBar == 0 && cDownSuccessBar == 0 && fastPivotPriceSeries[0] > currentCUp)
                    {
                        cUpBreachedBar = CurrentBar;
                    }

                    if (cUpBreachedBar > 0
                        && cUpSuccessBar == 0
                        && cDownSuccessBar == 0
                        && (CurrentBar - cUpBreachedBar) >= halfTimeFrameBars
                        && CountIf(() => fastPivotPriceSeries[0] > currentCUp, halfTimeFrameBars) >= halfTimeFrameBars)
                    {
                        cUpSuccessBar = CurrentBar;

                        redrawTag = "Up_" + currentDate.ToShortDateString();
                        redrawText = " C Up ";

                        Draw.Text(this, redrawTag, false, redrawText, 0, Highs[0][0], 50, textColor,
                                    textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                    }
                    #endregion

                    #region C Down
                    if (aUpSuccessBar > 0 && cDownBreachedBar == 0 && cUpSuccessBar == 0 && fastPivotPriceSeries[0] < currentCDown)
                    {
                        cDownBreachedBar = CurrentBar;
                    }

                    if (cDownBreachedBar > 0
                        && cDownSuccessBar == 0
                        && cUpSuccessBar == 0
                        && (CurrentBar - cDownBreachedBar) >= halfTimeFrameBars
                        && CountIf(() => fastPivotPriceSeries[0] < currentCDown, halfTimeFrameBars) >= halfTimeFrameBars)
                    {
                        cDownSuccessBar = CurrentBar;

                        redrawTag = "Down_" + currentDate.ToShortDateString();
                        redrawText = " C Down ";

                        Draw.Text(this, redrawTag, false, redrawText, 0, Lows[0][0], -50, textColor,
                                    textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                    }
                    #endregion
                }
            }
            else if (BarsInProgress == 2 && CurrentBar > 3)
            {
                double high = MAX(High, 3)[0];
                double low = MAX(High, 3)[0];

                double pivotPrice = (high + low + Close[0]) / 3;
                double pivotHighLowMiddle = (high + low) / 2;
                double pivotDifferential = Math.Abs(pivotPrice - pivotHighLowMiddle);
                rollingPivotUpperBound = pivotPrice + pivotDifferential;
                rollingPivotLowerBound = pivotPrice - pivotDifferential;

                trueRangeSeries[0] = (new List<double>() { High[0] - Low[0], Math.Abs(High[0] - Close[0]), Math.Abs(Low[0] - Close[0]) }).Max();

                if (CurrentBar > 35)
                {
                    List<double> trueRangeFactors = new List<double>();
                    for (int i = 10; i <= 30; i++)
                    {
                        trueRangeFactors.Add(SUM(trueRangeSeries, i)[0] / i);
                    }

                    double firstValue = trueRangeFactors.Average();
                    aSeries[0] = this.Instrument.MasterInstrument.RoundToTickSize(.1 * (firstValue / 2));
                    cSeries[0] = this.Instrument.MasterInstrument.RoundToTickSize(.15 * (firstValue / 2));
                    setACValues = true;
                }

                currentOpeningRangeClose = GetOpeningRangeClose(Close[0]);
                dayNumberLineSeries[0] = ScoreNumberLineMaster();
                numberLineSeries[0] = SUM(dayNumberLineSeries, 30)[0];
                numberLineMinusOneSum = SUM(dayNumberLineSeries, 29)[0];

                try
                {
                    numberLineDropOffScore = dayNumberLineSeries[29];
                }
                catch { }

                currentNumberLineEvents = new List<NumberLineEvents>();
                currentOpeningRangeClose = OpeningRangeCloses.None;

                //RollingPivotHigh[0] = rollingPivotUpperBound;
                //RollingPivotLow[0] = rollingPivotLowerBound;
                //Draw.TextFixed(this, "tag1", String.Format("Text to draw{0}Yo", Environment.NewLine), textPosition: TextPosition.BottomRight);
            }
        }
        #endregion

        #region NumberLine
        public enum NumberLineEvents
        {
            None,
            AUpBreached,
            AUpSuccess,
            ADownBreached,
            ADownSuccess,
            CUpBreached,
            CUpSuccess,
            CDownBreached,
            CDownSuccess
        }

        public enum OpeningRangeCloses
        {
            None,
            Above,
            Below,
            Inside
        }

        public void AddNumberLineEvents(List<NumberLineEvents> list, params KeyValuePair<NumberLineEvents, bool>[] events)
        {
            foreach(KeyValuePair<NumberLineEvents, bool> pair in events)
            {
                if(!list.Contains(pair.Key) && pair.Value)
                {
                    list.Add(pair.Key);
                }
            }
        }

        public OpeningRangeCloses GetOpeningRangeClose(double close)
        {
            if(close > openingHigh)
            {
                return OpeningRangeCloses.Above;
            }
            else if(close < openingLow)
            {
                return OpeningRangeCloses.Below;
            }
            else
            {
                return OpeningRangeCloses.Inside;
            }
        }

        public int ScoreNumberLineMaster()
        {
            int score = 0;

            //4.1
            score += ScoreNumberLineSlave(OpeningRangeCloses.Above, true, 2, NumberLineEvents.AUpBreached, NumberLineEvents.AUpSuccess);

            //4.2
            score += ScoreNumberLineSlave(OpeningRangeCloses.Below, true, -2, NumberLineEvents.ADownBreached, NumberLineEvents.ADownSuccess);

            //4.3
            score += ScoreNumberLineSlave(OpeningRangeCloses.Above, false, 2, NumberLineEvents.AUpBreached, NumberLineEvents.AUpSuccess);

            //4.4
            score += ScoreNumberLineSlave(OpeningRangeCloses.Below, false, -2, NumberLineEvents.ADownBreached, NumberLineEvents.ADownSuccess);

            //4.7
            score += ScoreNumberLineSlave(OpeningRangeCloses.Above, true, 4, NumberLineEvents.ADownBreached, NumberLineEvents.ADownSuccess, NumberLineEvents.CUpBreached, NumberLineEvents.CUpSuccess);

            //4.8
            score += ScoreNumberLineSlave(OpeningRangeCloses.Below, true, -4, NumberLineEvents.AUpBreached, NumberLineEvents.AUpSuccess, NumberLineEvents.CDownBreached, NumberLineEvents.CDownSuccess);

            //4.11
            score += ScoreNumberLineSlave(OpeningRangeCloses.Below, false, 0, NumberLineEvents.AUpBreached, NumberLineEvents.AUpSuccess, NumberLineEvents.CDownBreached, NumberLineEvents.CDownSuccess);

            //4.12
            score += ScoreNumberLineSlave(OpeningRangeCloses.Above, false, 0, NumberLineEvents.ADownBreached, NumberLineEvents.ADownSuccess, NumberLineEvents.CUpBreached, NumberLineEvents.CUpSuccess);

            //4.15
            score += ScoreNumberLineSlave(OpeningRangeCloses.Below, false, 3, NumberLineEvents.AUpBreached, NumberLineEvents.AUpSuccess, NumberLineEvents.CDownBreached);

            //4.16
            score += ScoreNumberLineSlave(OpeningRangeCloses.Above, false, -3, NumberLineEvents.ADownBreached, NumberLineEvents.ADownSuccess, NumberLineEvents.CUpBreached);

            //4.17
            score += ScoreNumberLineSlave(OpeningRangeCloses.Below, true, -3, NumberLineEvents.AUpBreached, NumberLineEvents.ADownBreached, NumberLineEvents.ADownSuccess);

            //4.18
            score += ScoreNumberLineSlave(OpeningRangeCloses.Above, true, 3, NumberLineEvents.ADownBreached, NumberLineEvents.AUpBreached, NumberLineEvents.AUpSuccess);

            //4.19
            score += ScoreNumberLineSlave(OpeningRangeCloses.Below, false, -1, NumberLineEvents.AUpBreached, NumberLineEvents.ADownBreached, NumberLineEvents.ADownSuccess);

            //4.20
            score += ScoreNumberLineSlave(OpeningRangeCloses.Above, false, 1, NumberLineEvents.ADownBreached, NumberLineEvents.AUpBreached, NumberLineEvents.AUpSuccess);

            //4.21
            score += ScoreNumberLineSlave(OpeningRangeCloses.Above, false, -1, NumberLineEvents.AUpBreached);

            //4.22
            score += ScoreNumberLineSlave(OpeningRangeCloses.Below, false, 1, NumberLineEvents.AUpBreached);

            return score;
        }

        public int ScoreNumberLineSlave(OpeningRangeCloses openingRangeClose, bool isClosingRangeComparison, int potentialScore, params NumberLineEvents[] scenario)
        {
            if(scenario.Count() != currentNumberLineEvents.Count
                || isClosingRangeComparison && openingRangeClose != currentOpeningRangeClose
                || !isClosingRangeComparison && openingRangeClose == currentOpeningRangeClose)
            {
                return 0;
            }
            else
            {
                for (int i = 0; i < scenario.Count(); i++)
                {
                    if (scenario[i] != currentNumberLineEvents[i])
                    {
                        return 0;
                    }
                }
            }

            return potentialScore;
        }
        #endregion

        #region PrintValues
        private void PrintValues(params object[] list)
        {
            Print(String.Join(",", list.Select(x => x.ToString())));
        }
        #endregion 

        #region Properties
        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATicks", Order=1, GroupName="Parameters")]
		public int ATicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CTicks", Order=2, GroupName="Parameters")]
		public int CTicks
		{ get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ProjectHour", Order = 5, GroupName = "Parameters")]
        public int ProjectHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ProjectMinute", Order = 10, GroupName = "Parameters")]
        public int ProjectMinute
        { get; set; }

        [Browsable(false)]
		[XmlIgnore]
		public Series<double> OpenHigh
        {
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OpenLow
        {
			get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> AUp
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ADown
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> CUp
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> CDown
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> PivotHigh
        {
            get { return Values[6]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> PivotLow
        {
            get { return Values[7]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RollingPivotHigh
        {
            get { return Values[8]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RollingPivotLow
        {
            get { return Values[9]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MyOpen
        {
            get { return Values[10]; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GuerillaAcdMasterIndicator[] cacheGuerillaAcdMasterIndicator;
		public GuerillaAcdMasterIndicator GuerillaAcdMasterIndicator(int aTicks, int cTicks, int projectHour, int projectMinute)
		{
			return GuerillaAcdMasterIndicator(Input, aTicks, cTicks, projectHour, projectMinute);
		}

		public GuerillaAcdMasterIndicator GuerillaAcdMasterIndicator(ISeries<double> input, int aTicks, int cTicks, int projectHour, int projectMinute)
		{
			if (cacheGuerillaAcdMasterIndicator != null)
				for (int idx = 0; idx < cacheGuerillaAcdMasterIndicator.Length; idx++)
					if (cacheGuerillaAcdMasterIndicator[idx] != null && cacheGuerillaAcdMasterIndicator[idx].ATicks == aTicks && cacheGuerillaAcdMasterIndicator[idx].CTicks == cTicks && cacheGuerillaAcdMasterIndicator[idx].ProjectHour == projectHour && cacheGuerillaAcdMasterIndicator[idx].ProjectMinute == projectMinute && cacheGuerillaAcdMasterIndicator[idx].EqualsInput(input))
						return cacheGuerillaAcdMasterIndicator[idx];
			return CacheIndicator<GuerillaAcdMasterIndicator>(new GuerillaAcdMasterIndicator(){ ATicks = aTicks, CTicks = cTicks, ProjectHour = projectHour, ProjectMinute = projectMinute }, input, ref cacheGuerillaAcdMasterIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GuerillaAcdMasterIndicator GuerillaAcdMasterIndicator(int aTicks, int cTicks, int projectHour, int projectMinute)
		{
			return indicator.GuerillaAcdMasterIndicator(Input, aTicks, cTicks, projectHour, projectMinute);
		}

		public Indicators.GuerillaAcdMasterIndicator GuerillaAcdMasterIndicator(ISeries<double> input , int aTicks, int cTicks, int projectHour, int projectMinute)
		{
			return indicator.GuerillaAcdMasterIndicator(input, aTicks, cTicks, projectHour, projectMinute);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GuerillaAcdMasterIndicator GuerillaAcdMasterIndicator(int aTicks, int cTicks, int projectHour, int projectMinute)
		{
			return indicator.GuerillaAcdMasterIndicator(Input, aTicks, cTicks, projectHour, projectMinute);
		}

		public Indicators.GuerillaAcdMasterIndicator GuerillaAcdMasterIndicator(ISeries<double> input , int aTicks, int cTicks, int projectHour, int projectMinute)
		{
			return indicator.GuerillaAcdMasterIndicator(input, aTicks, cTicks, projectHour, projectMinute);
		}
	}
}

#endregion
