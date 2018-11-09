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
	public class Scalp2 : Strategy
	{
        #region properties
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";

        GuerillaTickProfile guerillaTickProfile;
        GuerillaStickSimple greenBarStick;
        GuerillaStickSimple redBarStick;
        GuerillaStickSimple fiftyHammerStick;
        GuerillaStickSimple fiftyManStick;
        GuerillaStickSimple fiftyBarStick;
        GuerillaStickSimple bigBarStick;

        int tradeCount = 0;
        bool printTrades = false;
        double longTimePeriod = 15;
        double shortTimePeriod = 2.5;
        bool goLong = false;
        bool goShort = false;
        private DisableManager disableManager = new DisableManager();
        DateTime enterTime = DateTime.MinValue;
        bool secondTrade = false;
        bool firstPrimaryBar = false;

        double currentStop = 0;
        TrailingStopManager trailingStopManager = new TrailingStopManager();
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "Scalp2";
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
                BarsRequiredToTrade = 2;
                ProfitTicks = 4;
                StopLossTicks = 15;
                TradeSecondTrade = true;
                TradeTails = true;
                TradeBigBars = true;
                Trade0Period = true;
                Trade1Period = true;
                TradeOpen = true;
                TickProfileMinThreshold = 12;
                BreakevenThreshold = .5;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                //SetProfitTarget(CalculationMode.Ticks, 4);
                //SetStopLoss(CalculationMode.Ticks, 25);

                AddDataSeries(Data.BarsPeriodType.Second, (int)(shortTimePeriod * 60));
                AddDataSeries(Data.BarsPeriodType.Second, 60);

                guerillaTickProfile = GuerillaTickProfile(BarsArray[0], Brushes.MediumVioletRed, Brushes.LightGray, this.TickProfileMinThreshold, 2000);
                AddChartIndicator(guerillaTickProfile);

                greenBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.GreenBar, 0, 0);
                AddChartIndicator(greenBarStick);

                redBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.RedBar, 0, 0);
                AddChartIndicator(redBarStick);

                fiftyHammerStick = GuerillaStickSimple(BarsArray[0], Brushes.HotPink, Brushes.HotPink, GuerillaChartPattern.FiftyHammer, 0, 0);
                AddChartIndicator(fiftyHammerStick);

                fiftyManStick = GuerillaStickSimple(BarsArray[0], Brushes.HotPink, Brushes.HotPink, GuerillaChartPattern.FiftyMan, 0, 0);
                AddChartIndicator(fiftyManStick);

                fiftyBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.FiftyBar, 0, 0);
                AddChartIndicator(fiftyBarStick);

                bigBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Blue, Brushes.Blue, GuerillaChartPattern.BigBar, 0, 0);
                AddChartIndicator(bigBarStick);

                List<DisasbleTimeRange> disableTimeRanges = new List<DisasbleTimeRange>();
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 5, StartMinute = 30, EndHour = 6, EndMinute = 29 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 7, StartMinute = 0, EndHour = 7, EndMinute = 29 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 9, StartMinute = 0, EndHour = 11, EndMinute = 29 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 9, StartMinute = 0, EndHour = 9, EndMinute = 29 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 10, StartMinute = 0, EndHour = 10, EndMinute = 29 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 9, StartMinute = 0, EndHour = 10, EndMinute = 59 });
                disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 12, StartMinute = 46, EndHour = 16, EndMinute = 29 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 20, StartMinute = 0, EndHour = 20, EndMinute = 59 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 17, StartMinute = 30, EndHour = 18, EndMinute = 29 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 19, StartMinute = 30, EndHour = 20, EndMinute = 59 });

                foreach (DisasbleTimeRange d in disableTimeRanges)
                {
                    disableManager.AddRange(DayOfWeek.Sunday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Monday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Tuesday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Wednesday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Thursday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                    disableManager.AddRange(DayOfWeek.Friday, d.StartHour, d.StartMinute, d.EndHour, d.EndMinute);
                }

                trailingStopManager.AddItem((int)(this.ProfitTicks * this.BreakevenThreshold), (int)(this.ProfitTicks * this.BreakevenThreshold));
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

            bool canGoLong = Position.MarketPosition == MarketPosition.Flat || Position.MarketPosition == MarketPosition.Short;
            bool canGoShort = Position.MarketPosition == MarketPosition.Flat || Position.MarketPosition == MarketPosition.Long;

            if (BarsInProgress == 0 && Position.MarketPosition == MarketPosition.Flat && heldPeriods == 0)
            {
                firstPrimaryBar = true;
            }
            else if (firstPrimaryBar && BarsInProgress == 1 && heldPeriods <= 1)
            {
                double body = Math.Abs(Closes[0][0] - Opens[0][0]);
                bool smallBody = body <= 1;

                if (this.guerillaTickProfile.WithinThreshold[0])
                {
                    bool enterLong = false;
                    bool enterShort = false;
                    bool exitLong = false;
                    bool exitShort = false;
                    bool canEnter = disableManager.IsValidTime(Time[0]);

                    if (heldPeriods == 0 && this.Trade0Period)
                    {
                        enterLong = (this.TradeTails && this.fiftyHammerStick[0] > 0 && (this.greenBarStick[0] > 0 || smallBody))
                            || (this.TradeBigBars && this.greenBarStick[0] > 0 && this.bigBarStick[0] > 0);
                        enterShort = (this.TradeTails && this.fiftyManStick[0] > 0 && (this.redBarStick[0] > 0 || smallBody))
                            || (this.TradeBigBars && this.redBarStick[0] > 0 && this.bigBarStick[0] > 0);
                    }
                    else if (heldPeriods == 1 && this.Trade1Period)
                    {
                        bool isGreen = Close[0] > Open[0];
                        bool isRed = Close[0] < Open[0];

                        enterLong = isGreen
                            && ((this.TradeTails && this.fiftyHammerStick[0] > 0)
                            || (this.TradeBigBars && this.bigBarStick[0] > 0 && this.redBarStick[0] > 0)
                            );

                        enterShort = isRed
                            && ((this.TradeTails && this.fiftyManStick[0] > 0)
                            || (this.TradeBigBars && this.bigBarStick[0] > 0 && this.greenBarStick[0] > 0)
                            );
                    }

                    if (Position.MarketPosition == MarketPosition.Flat && canEnter)
                    {
                        if (enterLong)
                        {
                            currentStop = 0;
                            EnterLong(2, 1, ENTER_LONG);
                        }
                        else if (enterShort)
                        {
                            currentStop = 0;
                            EnterShort(2, 1, ENTER_SHORT);
                        }
                    }
                    else if (Position.MarketPosition == MarketPosition.Long)
                    {
                        if (enterShort && canEnter)
                        {
                            //EnterShort(2, 1, ENTER_SHORT);
                        }
                        else if (exitLong || enterShort)
                        {
                            //ExitLong(2, 1, EXIT, ENTER_LONG);
                        }
                    }
                    else if (Position.MarketPosition == MarketPosition.Short)
                    {
                        if (enterLong && canEnter)
                        {
                            //EnterLong(2, 1, ENTER_LONG);
                        }
                        else if (exitShort || enterLong)
                        {
                            //ExitShort(2, 1, EXIT, ENTER_SHORT);
                        }
                    }
                }
            }
            //else if (BarsInProgress == 2 && Position.MarketPosition != MarketPosition.Flat)
            //{
            //    double newStop = trailingStopManager.GetStop(Position.MarketPosition, Position.AveragePrice, Close[0], TickSize);

            //    if (Position.MarketPosition == MarketPosition.Long && newStop > currentStop && currentStop == 0)
            //    {
            //        currentStop = newStop;
            //        ExitLongStopMarket(2, true, 1, currentStop, EXIT, ENTER_LONG);
            //    }
            //    else if (Position.MarketPosition == MarketPosition.Short && newStop < currentStop && currentStop == 0)
            //    {
            //        currentStop = newStop;
            //        ExitShortStopMarket(2, true, 1, currentStop, EXIT, ENTER_SHORT);
            //    }
            //}
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
                    enterTime = time;
                    ExitLongLimit(2, true, 1, execution.Order.AverageFillPrice + (this.ProfitTicks * TickSize), EXIT, ENTER_LONG);
                    ExitLongStopMarket(2, true, 1, execution.Order.AverageFillPrice - (this.StopLossTicks * TickSize), EXIT, ENTER_LONG);
                }
                else if (execution.Order.Name == ENTER_SHORT)
                {
                    enterTime = time;
                    ExitShortLimit(2, true, 1, execution.Order.AverageFillPrice - (this.ProfitTicks * TickSize), EXIT, ENTER_SHORT);
                    ExitShortStopMarket(2, true, 1, execution.Order.AverageFillPrice + (this.StopLossTicks * TickSize), EXIT, ENTER_SHORT);
                }
                else if (TradeSecondTrade)
                {
                    if (secondTrade)
                    {
                        secondTrade = false;
                    }
                    else if (execution.Order.Name == EXIT)
                    {
                        bool enterLong = (execution.Order.IsLimit && execution.Order.FromEntrySignal == ENTER_LONG);
                        bool enterShort = (execution.Order.IsLimit && execution.Order.FromEntrySignal == ENTER_SHORT);

                        // || (execution.Order.IsStopMarket && execution.Order.FromEntrySignal == ENTER_SHORT)
                        // || (execution.Order.IsStopMarket && execution.Order.FromEntrySignal == ENTER_LONG)

                        if (enterLong)
                        {
                            //secondTrade = true;
                            currentStop = 0;
                            EnterLong(2, 1, ENTER_LONG);
                        }
                        else if (enterShort)
                        {
                            //secondTrade = true;
                            currentStop = 0;
                            EnterShort(2, 1, ENTER_SHORT);
                        }
                    }
                }
            }
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
        [Display(Name = "TradeSecondTrade", Order = 3, GroupName = "Parameters")]
        public bool TradeSecondTrade
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "TradeTails", Order = 4, GroupName = "Parameters")]
        public bool TradeTails
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "TradeBigBars", Order = 5, GroupName = "Parameters")]
        public bool TradeBigBars
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trade0Period", Order = 6, GroupName = "Parameters")]
        public bool Trade0Period
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trade1Period", Order = 7, GroupName = "Parameters")]
        public bool Trade1Period
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "TradeOpen", Order = 8, GroupName = "Parameters")]
        public bool TradeOpen
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "TickProfileMinThreshold", Order = 9, GroupName = "Parameters")]
        public int TickProfileMinThreshold
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 1)]
        [Display(Name = "BreakevenThreshold", Order = 10, GroupName = "Parameters")]
        public double BreakevenThreshold
        { get; set; }
        #endregion
    }
}
