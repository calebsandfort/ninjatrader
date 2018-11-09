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
	public class RenkoPullbacks : Strategy
	{
        #region properties
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";
        private const String PROFIT_TAKER = "PROFIT_TAKER";
        private const String STOP_LOSS = "STOP_LOSS";

        AutoTrendHNT8 autoTrend;

        GuerillaStickSimple greenBarStick;
        GuerillaStickSimple redBarStick;

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
                Name = "RenkoPullbacks";
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
                TrendStrength = 5;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Second, 5);

                autoTrend = AutoTrendHNT8(false, this.TrendStrength, true, false, 60, Brushes.DarkRed, Brushes.Chartreuse, Brushes.DarkRed, Brushes.Chartreuse);
                AddChartIndicator(autoTrend);
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
                    //PrintValues(Time[0], CountIf(() => stochasticsFast_14_3.D[0] > 80 && stochasticsFast_40_4.D[0] > 80 && stochasticsFast_9_3.D[0] > 80, 5) > 0);
                    //EnterShort(1, 1, ENTER_SHORT);
                    //stopLoss = MAX(High, 3)[0] + TickSize;
                    //Draw.VerticalLine(this, String.Format("vl_{0}", CurrentBar), Time[0], Brushes.White);
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
        [Range(1, int.MaxValue)]
        [Display(Name = "TrendStrength", Order = 3, GroupName = "Parameters")]
        public int TrendStrength
        { get; set; }
        #endregion
    }
}
