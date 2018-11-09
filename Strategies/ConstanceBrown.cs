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
	public class ConstanceBrown : Strategy
	{
        #region properties
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";
        private const String PROFIT_TAKER = "PROFIT_TAKER";
        private const String STOP_LOSS = "STOP_LOSS";

        StochasticsFast stochasticsFast_14_3;
        CompositeIndex compositeIndex;
        AutoTrendHNT8 autoTrend;

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
                Name = "ConstanceBrown";
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
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Second, 5);

                stochasticsFast_14_3 = StochasticsFast(3, 14);
                stochasticsFast_14_3.Plots[0].Brush = Brushes.Transparent;
                stochasticsFast_14_3.Plots[1].Brush = Brushes.Red;
                AddChartIndicator(stochasticsFast_14_3);

                compositeIndex = CompositeIndex(14, 9, 3, 3);
                AddChartIndicator(compositeIndex);

                List<DisasbleTimeRange> disableTimeRanges = new List<DisasbleTimeRange>();
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 0, StartMinute = 0, EndHour = 9, EndMinute = 59 });
                //disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 12, StartMinute = 30, EndHour = 24, EndMinute = 59 });

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

                }
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
