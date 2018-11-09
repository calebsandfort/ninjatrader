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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class LongTermAcd : Strategy
    {
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";
        private double currentHigh = 0;
        private double currentLow = 0;
        private Series<double> todayPivotPriceSeries;

        AcdDailyPivotPrice pivotPrice = null;
        SMA shortTermMA = null;
        SMA intermediateTermMA = null;
        SMA longTermMA = null;

        Brush shortTermBrush = Brushes.DodgerBlue;
        Brush intermediateTermBrush = Brushes.LimeGreen;
        Brush longTermBrush = Brushes.Purple;
        private DateTime currentDate = Core.Globals.MinDate;
        private Data.SessionIterator sessionIterator;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "LongTermAcd";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= false;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 51;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;

                ShortTermPeriod = 14;
                IntermediateTermPeriod = 30;
                LongTermPeriod = 50;
			}
			else if (State == State.Configure)
			{
                AddDataSeries(BarsPeriodType.Minute, 1);
            }
            else if (State == State.DataLoaded)
            {
                todayPivotPriceSeries = new Series<double>(BarsArray[1]);

                pivotPrice = AcdDailyPivotPrice();
                AddChartIndicator(pivotPrice);

                shortTermMA = SMA(14);
                shortTermMA.Plots[0].Brush = shortTermBrush;
                AddChartIndicator(shortTermMA);

                intermediateTermMA = SMA(30);
                intermediateTermMA.Plots[0].Brush = intermediateTermBrush;
                AddChartIndicator(intermediateTermMA);

                longTermMA = SMA(50);
                longTermMA.Plots[0].Brush = longTermBrush;
                AddChartIndicator(longTermMA);

                sessionIterator = new Data.SessionIterator(BarsArray[1]);
            }
		}

		protected override void OnBarUpdate()
		{
            DateTime startDate = new DateTime(2018, 1, 4);

            if (Time[0].Date < startDate)
            {
                return;
            }

            if (BarsInProgress == 0)
            {

            }
            else if (BarsInProgress == 1)
            {
                if (currentDate != sessionIterator.GetTradingDay(Time[0]))
                {
                    currentDate = sessionIterator.GetTradingDay(Time[0]);
                    currentHigh = High[0];
                    currentLow = Low[0];
                }
                else
                {
                    currentHigh = Math.Max(High[0], currentHigh);
                    currentLow = Math.Min(Low[0], currentLow);
                }

                if (Time[0].Hour == 13 && Time[0].Minute == 10 && Time[0].Second == 0)
                {
                    todayPivotPriceSeries[0] = (currentHigh + currentLow + Close[0]) / 3;
                    double currentShortTerm = ((todayPivotPriceSeries[0] - pivotPrice[ShortTermPeriod - 1]) / ShortTermPeriod) + shortTermMA[0];
                    double currentIntermediateTerm = ((todayPivotPriceSeries[0] - pivotPrice[IntermediateTermPeriod - 1]) / IntermediateTermPeriod) + intermediateTermMA[0];
                    double currentLongTerm = ((todayPivotPriceSeries[0] - pivotPrice[LongTermPeriod - 1]) / LongTermPeriod) + longTermMA[0];

                    int slopeLength = 2;
                    double shortTermSlope = CalculateSlope(shortTermMA, currentShortTerm, slopeLength);
                    double intermediateTermSlope = CalculateSlope(intermediateTermMA, currentIntermediateTerm, slopeLength);
                    double longTermSlope = CalculateSlope(longTermMA, currentLongTerm, slopeLength);

                    bool bullish = shortTermSlope > 0 && intermediateTermSlope > 0 && longTermSlope > 0;
                    bool bearish = shortTermSlope < 0 && intermediateTermSlope < 0 && longTermSlope < 0;
                    bool mixed = !bullish && !bearish;

                    if(Position.MarketPosition == MarketPosition.Flat)
                    {
                        if (bullish)
                        {
                            EnterLong(1, 1, ENTER_LONG);
                        }
                        else if (bearish)
                        {
                            EnterShort(1, 1, ENTER_SHORT);
                        }
                    }
                    else if (Position.MarketPosition == MarketPosition.Long)
                    {
                        if (bearish)
                        {
                            EnterShort(1, 1, ENTER_SHORT);
                        }
                        else if (mixed)
                        {
                            ExitLong(1, 1, EXIT, ENTER_LONG);
                        }
                    }
                    else if (Position.MarketPosition == MarketPosition.Short)
                    {
                        if (bullish)
                        {
                            EnterLong(1, 1, ENTER_LONG);
                        }
                        else if (mixed)
                        {
                            ExitShort(1, 1, EXIT, ENTER_SHORT);
                        }
                    }

                    //PrintValues(Time[0], shortTermSlope.ToString("N2"), intermediateTermSlope.ToString("N2"), longTermSlope.ToString("N2"));
                }

                #region adsf
                //if (Time[0].Date == today.Date)
                //{
                //    todayPivotPriceSeries[0] = (currentHigh + currentLow + Close[0]) / 3;
                //    double currentShortTerm = ((todayPivotPriceSeries[0] - pivotPrice[ShortTermPeriod - 1]) / ShortTermPeriod) + shortTermMA[0];
                //    Draw.Line(this, "shortTerm", false, 0, shortTermMA[0], -1, currentShortTerm, shortTermBrush, DashStyleHelper.Solid, 1);

                //    double currentIntermediateTerm = ((todayPivotPriceSeries[0] - pivotPrice[IntermediateTermPeriod - 1]) / IntermediateTermPeriod) + intermediateTermMA[0];
                //    Draw.Line(this, "intermediateTerm", false, 0, intermediateTermMA[0], -1, currentIntermediateTerm, intermediateTermBrush, DashStyleHelper.Solid, 1);

                //    double currentLongTerm = ((todayPivotPriceSeries[0] - pivotPrice[LongTermPeriod - 1]) / LongTermPeriod) + longTermMA[0];
                //    Draw.Line(this, "longTerm", false, 0, longTermMA[0], -1, currentLongTerm, longTermBrush, DashStyleHelper.Solid, 1);
                //} 
                #endregion
            }
        }

        private double CalculateSlope(SMA movingAverage, double currentValue, int slopeLength)
        {
            double dummyRSquared = 0;
            double dummyYIntercept = 0;
            double slope = 0;

            List<double> xValues = new List<double>();
            for (int i = 1; i <= slopeLength; i++)
            {
                xValues.Add(i);
            }

            List<double> yValues = new List<double>();
            for (int i = (slopeLength - 2); i >= 0; i--)
            {
                yValues.Add(movingAverage[i]);
            }
            yValues.Add(currentValue);
            LinearRegression(xValues, yValues, 0, slopeLength, out dummyRSquared, out dummyYIntercept, out slope);
            return slope;
        }

        private void LinearRegression(List<double> xVals, List<double> yVals,
                                            int inclusiveStart, int exclusiveEnd,
                                            out double rsquared, out double yintercept,
                                            out double slope)
        {
            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double ssX = 0;
            double ssY = 0;
            double sumCodeviates = 0;
            double sCo = 0;
            double count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            double RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            double RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            double meanX = sumOfX / count;
            double meanY = sumOfY / count;
            double dblR = RNumerator / Math.Sqrt(RDenom);
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
        }

        #region PrintValues
        private void PrintValues(params object[] list)
        {
            Print(String.Join(",", list.Select(x => x.ToString())));
        }
        #endregion 

        #region Properties
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ShortTermPeriod", Order = 1, GroupName = "Parameters")]
        public int ShortTermPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "IntermediateTermPeriod", Order = 2, GroupName = "Parameters")]
        public int IntermediateTermPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "LongTermPeriod", Order = 3, GroupName = "Parameters")]
        public int LongTermPeriod
        { get; set; }

        #endregion
    }
}
