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
    public class DayTradingRockStar : Strategy
    {
        #region properties
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";
        private const String PROFIT_TAKER = "PROFIT_TAKER";
        private const String STOP_LOSS = "STOP_LOSS";

        EnhancedStochasticsFast stochasticsFast_14_3;
        EnhancedStochasticsFast stochasticsFast_40_4;
        EnhancedStochastics stochastics_60_10_1;
        EnhancedStochasticsFast stochasticsFast_9_3;
        StochasticsFast stochasticsFast_14_3_5min;
        AutoTrendHNT8 autoTrend;
        EMA ema20;
        EMA ema50;
        EMA ema200;

        GuerillaStickSimple greenBarStick;
        GuerillaStickSimple redBarStick;
        GuerillaStickSimple fiftyHammerStick;
        GuerillaStickSimple fiftyManStick;
        GuerillaStickSimple bigBarStick;

        private DisableManager disableManager = new DisableManager();
        double profitTaker = 0;
        double stopLoss = 0;
        bool metBarsInProgress = false;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "DayTradingRockStar";
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
                StopLossTicks = 5;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Second, 5);
                //AddDataSeries(Data.BarsPeriodType.Minute, 5);

                autoTrend = AutoTrendHNT8(false, 5, true, false, 60, Brushes.DarkRed, Brushes.Chartreuse, Brushes.DarkRed, Brushes.Chartreuse);
                AddChartIndicator(autoTrend);

                stochasticsFast_14_3 = EnhancedStochasticsFast(3, 14, Brushes.Chartreuse, Brushes.Transparent, true, 1);
                AddChartIndicator(stochasticsFast_14_3);

                //stochasticsFast_14_3_5min = StochasticsFast(BarsArray[2], 3, 14);
                //AddChartIndicator(stochasticsFast_14_3_5min);

                stochastics_60_10_1 = EnhancedStochastics(1, 60, 10, Brushes.Transparent, Brushes.DodgerBlue);
                AddChartIndicator(stochastics_60_10_1);

                stochasticsFast_40_4 = EnhancedStochasticsFast(4, 40, Brushes.Violet, Brushes.Transparent, false, 1);
                AddChartIndicator(stochasticsFast_40_4);

                stochasticsFast_9_3 = EnhancedStochasticsFast(3, 9, Brushes.DarkRed, Brushes.Transparent, false, 1);
                AddChartIndicator(stochasticsFast_9_3);

                ema20 = EMA(20);
                ema20.Plots[0].Brush = Brushes.Green;
                AddChartIndicator(ema20);

                ema50 = EMA(50);
                ema50.Plots[0].Brush = Brushes.DodgerBlue;
                AddChartIndicator(ema50);

                ema200 = EMA(200);
                ema200.Plots[0].Brush = Brushes.Red;
                AddChartIndicator(ema200);

                greenBarStick = GuerillaStickSimple(Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.GreenBar, 0, 0);
                AddChartIndicator(greenBarStick);

                redBarStick = GuerillaStickSimple(Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.RedBar, 0, 0);
                AddChartIndicator(redBarStick);

                fiftyHammerStick = GuerillaStickSimple(Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.FiftyHammer, 0, 0);
                AddChartIndicator(fiftyHammerStick);

                fiftyManStick = GuerillaStickSimple(Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.FiftyMan, 0, 0);
                AddChartIndicator(fiftyManStick);

                bigBarStick = GuerillaStickSimple(Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.BigBar, 0, 0);
                AddChartIndicator(bigBarStick);

                List<DisasbleTimeRange> disableTimeRanges = new List<DisasbleTimeRange>();
                disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 0, StartMinute = 0, EndHour = 9, EndMinute = 59 });
                disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 12, StartMinute = 30, EndHour = 24, EndMinute = 59 });

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

            if (BarsInProgress == 0 && disableManager.IsValidTime(Time[0]))
            {
                if (Position.MarketPosition == MarketPosition.Flat)
                {
                    double trendStartPrice = 0;
                    double trendEndPrice = 0;

                        //Break of bullish trend line
                        //Check for diverging stochastic


                    for (int i = 0; i < 5; i++)
                    {
                        if (stochasticsFast_14_3.TrendStartPrice[i] > 0)
                        {
                            trendStartPrice = stochasticsFast_14_3.TrendStartPrice[i];
                            trendEndPrice = stochasticsFast_14_3.TrendEndPrice[i];
                        }
                    }

                    if (
                        autoTrend.Signal[0] == -1
                        && trendStartPrice > trendEndPrice
                        && trendStartPrice > 80
                        //&& trendEndPrice > 65
                        && this.redBarStick[0] > 0
                        && this.bigBarStick[0] > 0
                        )
                    {
                        PrintValues(Time[0], CountIf(() => stochasticsFast_14_3.D[0] > 80 && stochasticsFast_40_4.D[0] > 80 && stochasticsFast_9_3.D[0] > 80, 5) > 0);
                        EnterShort(1, 1, ENTER_SHORT);
                        stopLoss = MAX(High, 3)[0] + TickSize;
                        Draw.VerticalLine(this, String.Format("vl_{0}", CurrentBar), Time[0], Brushes.White);
                    }
                }
                //else if (Position.MarketPosition == MarketPosition.Short)
                //{
                //    if (stochasticsFast_14_3.CrossAboveLowerThreshold[0])
                //    {
                //        ExitShort(1, 1, PROFIT_TAKER, ENTER_SHORT);
                //    }
                //}
            }
            else if (BarsInProgress == 1 && Position.MarketPosition != MarketPosition.Flat)
            {

                if (Position.MarketPosition == MarketPosition.Long && High[0] >= profitTaker)
                {
                    ExitLong(1, 1, PROFIT_TAKER, ENTER_LONG);
                }
                else if (Position.MarketPosition == MarketPosition.Short && Low[0] <= profitTaker)
                {
                    ExitShort(1, 1, PROFIT_TAKER, ENTER_SHORT);
                }
                if (Position.MarketPosition == MarketPosition.Long && Low[0] <= stopLoss)
                {
                    ExitLong(1, 1, STOP_LOSS, ENTER_LONG);
                }
                else if (Position.MarketPosition == MarketPosition.Short && High[0] >= stopLoss)
                {
                    ExitShort(1, 1, STOP_LOSS, ENTER_SHORT);
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
                    stopLoss = execution.Order.AverageFillPrice - (this.StopLossTicks * TickSize);
                }
                else if (execution.Order.Name == ENTER_SHORT)
                {
                    profitTaker = execution.Order.AverageFillPrice - (this.ProfitTicks * TickSize);
                    stopLoss = execution.Order.AverageFillPrice + (this.StopLossTicks * TickSize);
                }

                //if (execution.Order.Name == ENTER_LONG)
                //{

                //}
                //else if (execution.Order.Name == ENTER_SHORT)
                //{
                //    ExitShortStopMarket(1, stopLoss, EXIT, ENTER_SHORT);
                //}
            }
        }
        #endregion

        #region PrintValues
        private void PrintValues(params object[] list)
        {
            Print(String.Join(",", list.Select(x => x.ToString())));
        }
        #endregion

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
    }
}
