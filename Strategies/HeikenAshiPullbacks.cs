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
using GuerillaTraderBridge;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class HeikenAshiPullbacks : Strategy
	{
        #region properties
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";
        private const String PROFIT_TAKER = "PROFIT_TAKER";
        private const String STOP_LOSS = "STOP_LOSS";

        AutoTrendHNT8 autoTrend;
        Swing swing;
        EnhancedStochasticsFast stochasticsFast_14_3;
        Momentum momentum;

        GuerillaStickSimple greenBarStick;
        GuerillaStickSimple redBarStick;

        private DisableManager disableManager = new DisableManager();
        double profitTaker = 0;
        double stopLoss = 0;
        bool metBarsInProgress = false;

        MarketPosition enterDirection = MarketPosition.Flat;
        double enterLimit = 0;
        int currentTrend = 0;
        bool liquidate = false;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "HeikenAshiPullbacks";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                ProfitTicks = 3;
                StopLossTicks = 8;
                TrendStrength = 5;
                InitialQuantity = 2;
                LetItRideQuantity = 1;
                TrailingBarsStop = 3;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Second, 1);

                autoTrend = AutoTrendHNT8(false, this.TrendStrength, true, false, 60, Brushes.DarkRed, Brushes.Chartreuse, Brushes.DarkRed, Brushes.Chartreuse);
                AddChartIndicator(autoTrend);

                swing = Swing(this.TrendStrength);
                AddChartIndicator(swing);

                momentum = Momentum(14);
                AddChartIndicator(momentum);

                stochasticsFast_14_3 = EnhancedStochasticsFast(3, 14, Brushes.Chartreuse, Brushes.Transparent, true, 1);
                AddChartIndicator(stochasticsFast_14_3);

                greenBarStick = GuerillaStickSimple(Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.GreenBar, 0, 0);
                AddChartIndicator(greenBarStick);

                redBarStick = GuerillaStickSimple(Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.RedBar, 0, 0);
                AddChartIndicator(redBarStick);

                List<DisasbleTimeRange> disableTimeRanges = new List<DisasbleTimeRange>();
                disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 6, StartMinute = 0, EndHour = 18, EndMinute = 00 });

                foreach (DisasbleTimeRange d in disableTimeRanges)
                {
                    disableManager.AddRange(DayOfWeek.Sunday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Monday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Tuesday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Wednesday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Thursday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Friday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                }
            }
        }
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (BarsInProgress == 0 && CurrentBar >= this.BarsRequiredToTrade) metBarsInProgress = true;

            if (!metBarsInProgress) return;

            if (BarsInProgress == 0)
            {
                //Draw.ArrowDown(this, "UpTrendBreak" + unique + lineCount.ToString(), true, barsAgo, High[barsAgo] + TickSize, DownTrendColor);

                bool bullTrendStarted = autoTrend.TrendStarted[1] != 1 && autoTrend.TrendStarted[0] == 1
                    && CountIf(() => autoTrend.Signal[0] == -1, this.TrendStrength * 2) == 0;

                if (bullTrendStarted && Position.MarketPosition == MarketPosition.Flat)
                {
                    currentTrend = 1;
                    enterDirection = MarketPosition.Long;
                    enterLimit = Close[0];
                    stopLoss = swing.SwingLow[0];
                }
                else if (bullTrendStarted && Position.MarketPosition == MarketPosition.Short)
                {
                    //liquidate = true;
                }

                bool bullTrendBroken = currentTrend == 1 && autoTrend.Signal[1] != -1 && autoTrend.Signal[0] == -1;

                if (bullTrendBroken)
                {
                    currentTrend = 0;
                    enterDirection = MarketPosition.Flat;
                    enterLimit = 0;
                }

                bool bearTrendStarted = autoTrend.TrendStarted[1] != -1 && autoTrend.TrendStarted[0] == -1
                    && CountIf(() => autoTrend.Signal[0] == 1, this.TrendStrength * 2) == 0;

                if (bearTrendStarted && Position.MarketPosition == MarketPosition.Flat)
                {
                    currentTrend = -1;
                    enterDirection = MarketPosition.Short;
                    enterLimit = Close[0];
                    stopLoss = swing.SwingHigh[0];
                }
                else if (bearTrendStarted && Position.MarketPosition == MarketPosition.Long)
                {
                    //liquidate = true;
                }

                bool bearTrendBroken = currentTrend == -1 && autoTrend.Signal[1] != 1 && autoTrend.Signal[0] == 1;

                if (bearTrendBroken)
                {
                    currentTrend = 0;
                    enterDirection = MarketPosition.Flat;
                    enterLimit = 0;

                }

                if(Position.MarketPosition == MarketPosition.Long && Position.Quantity == this.LetItRideQuantity)
                {
                    stopLoss = GetStop(stopLoss, MIN(Low, TrailingBarsStop)[0] - TickSize, MarketPosition.Long);
                }
                else if (Position.MarketPosition == MarketPosition.Long && Position.Quantity == this.LetItRideQuantity)
                {
                    stopLoss = GetStop(stopLoss, MAX(High, TrailingBarsStop)[0] + TickSize, MarketPosition.Short);
                }
            }
            else if (BarsInProgress == 1 && Position.MarketPosition == MarketPosition.Flat && disableManager.IsValidTime(Time[0]))
            {
                if(enterDirection == MarketPosition.Long && enterLimit > 0 && Close[0] > enterLimit)
                {
                    enterDirection = MarketPosition.Flat;
                    enterLimit = 0;
                    EnterLong(this.InitialQuantity, ENTER_LONG);
                    PrintValues(Time[0], String.Format("{0:N2}", autoTrend.ChangePerBar[0]), momentum[0]);
                }
                else if (enterDirection == MarketPosition.Short && enterLimit > 0 && Close[0] < enterLimit)
                {
                    enterDirection = MarketPosition.Flat;
                    enterLimit = 0;
                    EnterShort(this.InitialQuantity, ENTER_SHORT);
                    PrintValues(Time[0], String.Format("{0:N2}", autoTrend.ChangePerBar[0]), momentum[0]);
                }
            }
            else if (BarsInProgress == 1 && Position.MarketPosition != MarketPosition.Flat)
            {
                if (liquidate && Position.MarketPosition == MarketPosition.Long)
                {
                    ExitLong(Position.Quantity, STOP_LOSS, ENTER_LONG);
                }
                else if (liquidate && Position.MarketPosition == MarketPosition.Short)
                {
                    ExitShort(Position.Quantity, STOP_LOSS, ENTER_SHORT);
                }
                else if (Position.MarketPosition == MarketPosition.Long && profitTaker > 0 && High[0] >= profitTaker)
                {
                    stopLoss = GetStop(Position.AveragePrice, MIN(Lows[0], TrailingBarsStop)[0] - TickSize, MarketPosition.Long);
                    profitTaker = 0;
                    ExitLong(this.InitialQuantity - this.LetItRideQuantity, PROFIT_TAKER, ENTER_LONG);
                }
                else if (Position.MarketPosition == MarketPosition.Short && profitTaker > 0 && Low[0] <= profitTaker)
                {
                    stopLoss = GetStop(Position.AveragePrice, MAX(Highs[0], TrailingBarsStop)[0] + TickSize, MarketPosition.Short);
                    profitTaker = 0;
                    ExitShort(this.InitialQuantity - this.LetItRideQuantity, PROFIT_TAKER, ENTER_SHORT);
                }
                if (Position.MarketPosition == MarketPosition.Long && Low[0] <= stopLoss)
                {
                    ExitLong(Position.Quantity, STOP_LOSS, ENTER_LONG);
                }
                else if (Position.MarketPosition == MarketPosition.Short && High[0] >= stopLoss)
                {
                    ExitShort(Position.Quantity, STOP_LOSS, ENTER_SHORT);
                }
            }
        }
        #endregion

        #region OnExecutionUpdate
        protected override void OnExecutionUpdate(Cbi.Execution execution, string executionId, double price, int quantity,
            Cbi.MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (execution.Order != null && execution.Order.OrderState == OrderState.Filled)
            {
                if (execution.Order.Name == ENTER_LONG)
                {
                    profitTaker = execution.Order.AverageFillPrice + (this.ProfitTicks * TickSize);
                    double tempStopLoss = execution.Order.AverageFillPrice - (this.StopLossTicks * TickSize);
                    stopLoss = Math.Max(stopLoss, tempStopLoss);
                }
                else if (execution.Order.Name == ENTER_SHORT)
                {
                    profitTaker = execution.Order.AverageFillPrice - (this.ProfitTicks * TickSize);
                    double tempStopLoss = execution.Order.AverageFillPrice + (this.StopLossTicks * TickSize);
                    stopLoss = Math.Min(stopLoss, tempStopLoss);
                }
            }
        }
        #endregion

        #region GetStop
        private double GetStop(double val1, double val2, MarketPosition marketPosition)
        {
            return marketPosition == MarketPosition.Long ? Math.Max(val1, val2) : Math.Min(val1, val2);
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
        [Range(0, int.MaxValue)]
        [Display(Name = "ProfitTicks", Order = 1, GroupName = "Parameters")]
        public int ProfitTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "StopLossTicks", Order = 2, GroupName = "Parameters")]
        public int StopLossTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "TrendStrength", Order = 3, GroupName = "Parameters")]
        public int TrendStrength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "InitialQuantity", Order = 4, GroupName = "Parameters")]
        public int InitialQuantity
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "LetItRideQuantity", Order = 5, GroupName = "Parameters")]
        public int LetItRideQuantity
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "TrailingBarsStop", Order = 6, GroupName = "Parameters")]
        public int TrailingBarsStop
        { get; set; }
        #endregion
    }
}
