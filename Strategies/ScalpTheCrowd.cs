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
	public class ScalpTheCrowd : Strategy
	{
        #region properties
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";
        private const String PROFIT_TAKER = "PROFIT_TAKER";
        private const String STOP_LOSS = "STOP_LOSS";

        GuerillaTickProfile guerillaTickProfile;

        GuerillaStickSimple primaryGreenBarStick;
        GuerillaStickSimple primaryRedBarStick;
        GuerillaStickSimple primaryFiftyHammerStick;
        GuerillaStickSimple primaryFiftyManStick;
        GuerillaStickSimple primaryFiftyBarStick;
        GuerillaStickSimple primaryBigBarStick;

        GuerillaStickSimple secondaryGreenBarStick;
        GuerillaStickSimple secondaryRedBarStick;
        GuerillaStickSimple secondaryFiftyHammerStick;
        GuerillaStickSimple secondaryFiftyManStick;
        GuerillaStickSimple secondaryFiftyBarStick;
        GuerillaStickSimple secondaryBigBarStick;

        private DisableManager disableManager = new DisableManager();
        double longTimePeriod = 15;
        double shortTimePeriod = 2.5;
        bool firstPrimaryBar = false;
        double profitTaker = 0;
        double stopLoss = 0;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "ScalpTheCrowd";
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
                BarsRequiredToTrade = 1;
                ProfitTicks = 5;
                StopLossTicks = 4;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Second, (int)(shortTimePeriod * 60));
                AddDataSeries(Data.BarsPeriodType.Tick, 1);

                guerillaTickProfile = GuerillaTickProfile(BarsArray[0], Brushes.MediumVioletRed, Brushes.LightGray, 13, 2000);
                AddChartIndicator(guerillaTickProfile);

                #region Primary Sticks
                primaryGreenBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.GreenBar, 0, 0);
                AddChartIndicator(primaryGreenBarStick);

                primaryRedBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.RedBar, 0, 0);
                AddChartIndicator(primaryRedBarStick);

                primaryFiftyHammerStick = GuerillaStickSimple(BarsArray[0], Brushes.HotPink, Brushes.HotPink, GuerillaChartPattern.FiftyHammer, 0, 0);
                AddChartIndicator(primaryFiftyHammerStick);

                primaryFiftyManStick = GuerillaStickSimple(BarsArray[0], Brushes.HotPink, Brushes.HotPink, GuerillaChartPattern.FiftyMan, 0, 0);
                AddChartIndicator(primaryFiftyManStick);

                primaryFiftyBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.FiftyBar, 0, 0);
                AddChartIndicator(primaryFiftyBarStick);

                primaryBigBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Blue, Brushes.Blue, GuerillaChartPattern.BigBar, 0, 0);
                AddChartIndicator(primaryBigBarStick);
                #endregion

                #region Secondary Sticks
                secondaryGreenBarStick = GuerillaStickSimple(BarsArray[1], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.GreenBar, 0, 0);
                AddChartIndicator(secondaryGreenBarStick);

                secondaryRedBarStick = GuerillaStickSimple(BarsArray[1], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.RedBar, 0, 0);
                AddChartIndicator(secondaryRedBarStick);

                secondaryFiftyHammerStick = GuerillaStickSimple(BarsArray[1], Brushes.HotPink, Brushes.HotPink, GuerillaChartPattern.FiftyHammer, 0, 0);
                AddChartIndicator(secondaryFiftyHammerStick);

                secondaryFiftyManStick = GuerillaStickSimple(BarsArray[1], Brushes.HotPink, Brushes.HotPink, GuerillaChartPattern.FiftyMan, 0, 0);
                AddChartIndicator(secondaryFiftyManStick);

                secondaryFiftyBarStick = GuerillaStickSimple(BarsArray[1], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.FiftyBar, 0, 0);
                AddChartIndicator(secondaryFiftyBarStick);

                secondaryBigBarStick = GuerillaStickSimple(BarsArray[1], Brushes.Blue, Brushes.Blue, GuerillaChartPattern.BigBar, 0, 0);
                AddChartIndicator(secondaryBigBarStick);
                #endregion

                List<DisasbleTimeRange> disableTimeRanges = new List<DisasbleTimeRange>();
                disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 5, StartMinute = 30, EndHour = 7, EndMinute = 44 });
                disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 12, StartMinute = 46, EndHour = 16, EndMinute = 29 });

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
            DateTime debugDate = new DateTime(2017, 12, 13, 20, 45, 0);

            if (debugDate == Time[0] && BarsInProgress == 1)
            {
                int d = 0;
            }

            Double minutes = Time[0].Minute + (Time[0].Second == 30 ? .5 : 0);
            double heldPeriods = (minutes % longTimePeriod) / shortTimePeriod;

            if (BarsInProgress == 0 && Position.MarketPosition == MarketPosition.Flat && heldPeriods == 0)
            {
                firstPrimaryBar = true;
            }

            else if (firstPrimaryBar && BarsInProgress == 1 && heldPeriods == 1)
            {
                double body = Math.Abs(Closes[0][0] - Opens[0][0]);
                bool smallBody = body <= 1;

                if (this.guerillaTickProfile.WithinThreshold[0])
                {
                    bool enterLong = false;
                    bool enterShort = false;
                    bool canEnter = disableManager.IsValidTime(Time[0]);

                    if (heldPeriods == 1)
                    {
                        enterLong = this.secondaryGreenBarStick[0] > 0;
                        enterShort = this.secondaryRedBarStick[0] > 0;
                    }

                    if (Position.MarketPosition == MarketPosition.Flat && canEnter)
                    {
                        if (enterLong)
                        {
                            EnterLong(2, 1, ENTER_LONG);
                        }
                        else if (enterShort)
                        {
                            EnterShort(2, 1, ENTER_SHORT);
                        }
                    }
                }
            }
            else if (BarsInProgress == 2 && Position.MarketPosition != MarketPosition.Flat)
            {

                if (Position.MarketPosition == MarketPosition.Long && High[0] >= profitTaker)
                {
                    ExitLong(2, 1, PROFIT_TAKER, ENTER_LONG);
                }
                else if (Position.MarketPosition == MarketPosition.Short && Low[0] <= profitTaker)
                {
                    ExitShort(2, 1, PROFIT_TAKER, ENTER_SHORT);
                }
                else if (Position.MarketPosition == MarketPosition.Long && Low[0] <= stopLoss)
                {
                    ExitLong(2, 1, STOP_LOSS, ENTER_LONG);
                }
                else if (Position.MarketPosition == MarketPosition.Short && High[0] >= stopLoss)
                {
                    ExitShort(2, 1, STOP_LOSS, ENTER_SHORT);
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
            }
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
        #endregion
    }
}
