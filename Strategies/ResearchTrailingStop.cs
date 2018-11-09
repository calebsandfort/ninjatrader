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
    #region Class - ResearchTrailingStop
    public class ResearchTrailingStop : Strategy
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
        double shortTimePeriod = 5;
        bool goLong = false;
        bool goShort = false;
        double currentStop = 0;

        TrailingStopManager trailingStopManager = new TrailingStopManager();
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "ResearchTrailingStop";
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
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
                StartTradingHour = 7;
                StartTradingMinute = 45;
                StopTradingHour = 12;
                StopTradingMinute = 45;
                ProfitTicks = 5;
                HoldPeriods = 1;
            }
            else if (State == State.Configure)
            {
                //SetProfitTarget(CalculationMode.Ticks, 50);
                //SetStopLoss(CalculationMode.Ticks, 40);
                //SetTrailStop(CalculationMode.Ticks, 40);

                AddDataSeries(Data.BarsPeriodType.Second, (int)(shortTimePeriod * 60));
                AddDataSeries(Data.BarsPeriodType.Second, 5);

                guerillaTickProfile = GuerillaTickProfile(BarsArray[0], Brushes.MediumVioletRed, Brushes.LightGray, 28, 200);
                AddChartIndicator(guerillaTickProfile);

                greenBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.GreenBar, 0, 0);
                AddChartIndicator(greenBarStick);

                redBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Transparent, Brushes.Transparent, GuerillaChartPattern.RedBar, 0, 0);
                AddChartIndicator(redBarStick);

                fiftyHammerStick = GuerillaStickSimple(BarsArray[0], Brushes.Chartreuse, Brushes.Firebrick, GuerillaChartPattern.FiftyHammer, 0, 0);
                AddChartIndicator(fiftyHammerStick);

                fiftyManStick = GuerillaStickSimple(BarsArray[0], Brushes.Chartreuse, Brushes.Firebrick, GuerillaChartPattern.FiftyMan, 0, 0);
                AddChartIndicator(fiftyManStick);

                fiftyBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Yellow, Brushes.Yellow, GuerillaChartPattern.FiftyBar, 0, 0);
                AddChartIndicator(fiftyBarStick);

                bigBarStick = GuerillaStickSimple(BarsArray[0], Brushes.Blue, Brushes.Blue, GuerillaChartPattern.BigBar, 0, 0);
                AddChartIndicator(bigBarStick);

                //$2536
                trailingStopManager.AddItem(-20, 40);
                trailingStopManager.AddItem(20, 30);
                trailingStopManager.AddItem(40, 20);
                trailingStopManager.AddItem(60, 10);
                trailingStopManager.AddItem(70, 5);

                ////$2536
                //trailingStopManager.AddItem(-20, 22);
                //trailingStopManager.AddItem(20, 40);
                //trailingStopManager.AddItem(40, 20);
                //trailingStopManager.AddItem(60, 10);
                //trailingStopManager.AddItem(70, 5);


            }
        }
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            DateTime debugDate = new DateTime(2018, 9, 17, 11, 10, 0);

            if (debugDate == Time[0])
            {
                int d = 0;
            }

            Double minutes = Time[0].Minute + (Time[0].Second == 30 ? .5 : 0);
            double heldPeriods = (minutes % longTimePeriod) / shortTimePeriod;

            if (BarsInProgress == 0 && Position.MarketPosition == MarketPosition.Flat && heldPeriods == 0)
            {

            }
            else if (BarsInProgress == 1 && Position.MarketPosition == MarketPosition.Flat && heldPeriods == 0)
            {
                DateTime startTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StartTradingHour, this.StartTradingMinute, 0);
                DateTime stopTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StopTradingHour, this.StopTradingMinute, 0);

                if (Time[0] >= startTime && Time[0] <= stopTime && this.guerillaTickProfile.WithinThreshold[0])
                {
                    if (this.greenBarStick[0] > 0)
                    {
                        goLong = false;

                        if (this.fiftyHammerStick[0] > 0)
                        {
                            goLong = true;
                        }

                        if (goLong)
                        {
                            currentStop = 0;
                            EnterLong(2, 1, ENTER_LONG);
                        }
                    }
                    else if (this.redBarStick[0] > 0)
                    {
                        goShort = false;

                        if (this.fiftyManStick[0] > 0)
                        {
                            goShort = true;
                        }

                        if (goShort)
                        {
                            currentStop = 1000000;
                            EnterShort(2, 1, ENTER_SHORT);
                        }
                    }
                }
            }
            else if (BarsInProgress == 1 && Position.MarketPosition == MarketPosition.Flat && heldPeriods == 1)
            {
                DateTime startTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StartTradingHour, this.StartTradingMinute, 0);
                DateTime stopTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StopTradingHour, this.StopTradingMinute, 0);

                if (Time[0] >= startTime && Time[0] <= stopTime && this.guerillaTickProfile.WithinThreshold[0])
                {
                    bool isGreen = Close[0] > Open[0];
                    bool isRed = Close[0] < Open[0];

                    if (isGreen && this.redBarStick[0] > 0 && this.fiftyHammerStick[0] > 0)
                    {
                        currentStop = 0;
                        EnterLong(2, 1, ENTER_LONG);
                    }
                    else if (isRed && this.greenBarStick[0] > 0 && this.fiftyManStick[0] > 0)
                    {
                        currentStop = 1000000;
                        EnterShort(2, 1, ENTER_SHORT);
                    }
                }
            }
            else if (BarsInProgress == 1 && Position.MarketPosition != MarketPosition.Flat)
            {
                bool isGreen = Close[0] > Open[0];
                bool isRed = Close[0] < Open[0];

                //if (Position.MarketPosition == MarketPosition.Long && isRed)
                //{
                //    ExitLong(2, 1, EXIT, ENTER_LONG);
                //}
                //else if (Position.MarketPosition == MarketPosition.Short && isGreen)
                //{
                //    ExitShort(2, 1, EXIT, ENTER_SHORT);
                //}
            }
            else if (BarsInProgress == 2 && Position.MarketPosition != MarketPosition.Flat)
            {
                double newStop = trailingStopManager.GetStop(Position.MarketPosition, Position.AveragePrice, Close[0], TickSize);

                if(Position.MarketPosition == MarketPosition.Long && newStop > currentStop)
                {
                    currentStop = newStop;
                    ExitLongStopMarket(2, true, 1, currentStop, EXIT, ENTER_LONG);
                }
                else if (Position.MarketPosition == MarketPosition.Short && newStop < currentStop)
                {
                    currentStop = newStop;
                    ExitShortStopMarket(2, true, 1, currentStop, EXIT, ENTER_SHORT);
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
        [Display(Name = "StartTradingHour", Order = 1, GroupName = "Parameters")]
        public int StartTradingHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "StartTradingMinute", Order = 2, GroupName = "Parameters")]
        public int StartTradingMinute
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "StopTradingHour", Order = 3, GroupName = "Parameters")]
        public int StopTradingHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "StopTradingMinute", Order = 4, GroupName = "Parameters")]
        public int StopTradingMinute
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ProfitTicks", Order = 5, GroupName = "Parameters")]
        public int ProfitTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "HoldPeriods", Order = 6, GroupName = "Parameters")]
        public int HoldPeriods
        { get; set; }
        #endregion
    } 
    #endregion

    #region Class - TrailingStopItem
    public class TrailingStopItem
    {
        public int ProfitTrigger { get; set; }
        public int StopLoss { get; set; }
    }
    #endregion

    #region Class - TrailingStopManager
    public class TrailingStopManager
    {
        public List<TrailingStopItem> Items { get; set; }

        public TrailingStopManager()
        {
            this.Items = new List<TrailingStopItem>();
        }

        public void AddItem(int profitTrigger, int stopLoss)
        {
            this.Items.Add(new TrailingStopItem() { ProfitTrigger = profitTrigger, StopLoss = stopLoss });
        }

        //After Profit Target use stop value

        public double GetStop(MarketPosition marketPosition, double entry, double last, double tickSize)
        {
            int plTicks = 0;
            double stopPrice = 0;

            if(marketPosition == MarketPosition.Long)
            {
                plTicks = (int)((last - entry) / tickSize);
            }
            else if(marketPosition == MarketPosition.Short)
            {
                plTicks = (int)((entry - last) / tickSize);
            }

            for(int i = 0; i < this.Items.Count; i++)
            {
                //last item
                if(i == (this.Items.Count - 1) && plTicks >= this.Items[i].ProfitTrigger)
                {
                    if (marketPosition == MarketPosition.Long)
                    {
                        stopPrice = last - (this.Items[i].StopLoss * tickSize);
                        break;
                    }
                    else if (marketPosition == MarketPosition.Short)
                    {
                        stopPrice = last + (this.Items[i].StopLoss * tickSize);
                        break;
                    }
                }
                else if(i < (this.Items.Count - 1))
                {
                    int lowerBound = this.Items[i].ProfitTrigger;
                    int upperBound = this.Items[i + 1].ProfitTrigger;

                    if(plTicks >= lowerBound && plTicks < upperBound)
                    {
                        if (marketPosition == MarketPosition.Long)
                        {
                            stopPrice = last - (this.Items[i].StopLoss * tickSize);
                            break;
                        }
                        else if (marketPosition == MarketPosition.Short)
                        {
                            stopPrice = last + (this.Items[i].StopLoss * tickSize);
                            break;
                        }
                    }
                }
            }

            return stopPrice;
        }
    } 
    #endregion
}
