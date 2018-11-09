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
	public class ResearchSignals : Strategy
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
        private DisableManager disableManager = new DisableManager();
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "ResearchSignals";
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

                guerillaTickProfile = GuerillaTickProfile(BarsArray[0], Brushes.MediumVioletRed, Brushes.LightGray, 12, 2000);
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

                List<DisasbleTimeRange> disableTimeRanges = new List<DisasbleTimeRange>();
                disableTimeRanges.Add(new DisasbleTimeRange() { Day = DayOfWeek.Monday, StartHour = 6, StartMinute = 30, EndHour = 7, EndMinute = 59 });
                disableTimeRanges.Add(new DisasbleTimeRange(){ Day = DayOfWeek.Monday, StartHour = 8, StartMinute = 30, EndHour = 9, EndMinute = 29 });
                disableTimeRanges.Add(new DisasbleTimeRange(){ Day = DayOfWeek.Monday, StartHour = 10, StartMinute = 0, EndHour = 11, EndMinute = 29 });
                disableTimeRanges.Add(new DisasbleTimeRange(){ Day = DayOfWeek.Monday, StartHour = 12, StartMinute = 46, EndHour = 14, EndMinute = 0 });

                foreach(DisasbleTimeRange d in disableTimeRanges)
                {
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
            if(Time[0].Hour == 6 && Time[0].Minute < 45)
            {
                return;
            }

            DateTime debugDate = new DateTime(2018, 10, 18, 12, 0, 0);

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

            }
            else if (BarsInProgress == 1 && heldPeriods <= 1)
            {
                DateTime startTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StartTradingHour, this.StartTradingMinute, 0);
                DateTime stopTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, this.StopTradingHour, this.StopTradingMinute, 0);
                double body = Math.Abs(Closes[0][0] - Opens[0][0]);
                bool smallBody = body <= 1;

                if (this.guerillaTickProfile.WithinThreshold[0])
                {
                    bool enterLong = false;
                    bool enterShort = false;
                    bool exitLong = false;
                    bool exitShort = false;
                    bool canEnter = disableManager.IsValidTime(Time[0]);

                    if (heldPeriods == 0)
                    {
                        enterLong = (this.fiftyHammerStick[0] > 0 && (this.greenBarStick[0] > 0 || smallBody));
                        enterShort = (this.fiftyManStick[0] > 0 && (this.redBarStick[0] > 0 || smallBody));
                        exitLong = (this.fiftyManStick[0] > 0 || (this.bigBarStick[0] > 0 && this.redBarStick[0] > 0));
                        exitShort = (this.fiftyHammerStick[0] > 0 || (this.bigBarStick[0] > 0 && this.greenBarStick[0] > 0));
                    }
                    else if (heldPeriods == 1)
                    {
                        bool isGreen = Close[0] > Open[0];
                        bool isRed = Close[0] < Open[0];

                        enterLong = isGreen && this.redBarStick[0] > 0 && this.fiftyHammerStick[0] > 0;
                        enterShort = isRed && this.greenBarStick[0] > 0 && this.fiftyManStick[0] > 0;
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
                    else if(Position.MarketPosition == MarketPosition.Long)
                    {
                        if (enterShort && canEnter)
                        {
                            EnterShort(2, 1, ENTER_SHORT);
                        }
                        else if (exitLong || enterShort)
                        {
                            ExitLong(2, 1, EXIT, ENTER_LONG);
                        }
                    }
                    else if (Position.MarketPosition == MarketPosition.Short)
                    {
                        if (enterLong && canEnter)
                        {
                            EnterLong(2, 1, ENTER_LONG);
                        }
                        else if (exitShort || enterLong)
                        {
                            ExitShort(2, 1, EXIT, ENTER_SHORT);
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
}
