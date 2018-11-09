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
	public class RideTheTrendSkipTheBS : Strategy
    {
        #region Props
        private String ENTER_LONG = "ENTER_LONG";
        private String ENTER_SHORT = "ENTER_SHORT";
        private String EXIT = "EXIT";

        private Brush BullishBrush = Brushes.DarkBlue;
        private Brush BearishBrush = Brushes.HotPink;
        private Brush NeutralBrush = Brushes.Gray;

        private EnhancedADX adxIndicator;
        private AdxSlope exitAdxSlopeIndicator;
        private AdxSlope enterAdxSlopeIndicator;
        private MyRange rangeIndicator;
        private TickCounter tickCounter;
        private EMA fastMaIndicator;
        private EMA slowMaIndicator;
        private Stochastics stochastics;
        //        private EMA fastMaIndicator;
        //        private EMA slowMaIndicator;

        private double tickRange;
        private double totalLimit;
        private double totalStop;
        private bool openPosition = false;
        private bool trendBarAppeared = false;
        private int enterBarNumber = 0;
        private int bullishSignalBarNumber = 0;
        private int bearishSignalBarNumber = 0;
        private bool closing = false;
        private Bridge guerillaTraderBridge = new Bridge();
        private DisableManager disableManager = new DisableManager();
        private double runningPL = 0;
        private bool targetHit = false;
        private bool trailingStopEnabled = false;
        private int maxDivided = 0;
        private double maSlope = 0;
        private double maDiff = 0;
        private int count = 0;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"RideTheTrendSkipTheBS";
                Name = "RideTheTrendSkipTheBS";
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

                TradingAccountId = 0;
                ExitPeriod = 3;
                AdxPeriod = 14;
                MinAdx = 25;
                ExitSlopeThreshold = -15;
                UseBoundaryHours = true;
                StartHour = 7;
                StartMinute = 30;
                EndHour = 12;
                EndMinute = 30;
                LossExitTicks = 8;
                EnterPeriod = 3;
                EnterSlopeThreshold = 20;
                RecordHistoricalTrades = false;
                EnterBarsLength = 2;
                ExitBarsLength = 2;
                ExitStopBuffer = 2;
                ExitLimitBuffer = 2;
                Quantity = 1;
                FireSns = false;
                MinTrendLength = 1;
                BsWindowLength = 5;
                TargetDollars = 0;
                TrailingStopMinDollars = 0;
                TrailingStopDivisorDollars = 0;
                TrailingStopCushionDollars = 0;
                OverrideRealTimeOnly = false;
            }
            else if (State == State.Configure)
            {
                //SetOrderQuantity = SetOrderQuantity.DefaultQuantity;

                trailingStopEnabled = TrailingStopMinDollars > 0;

                if(trailingStopEnabled) AddDataSeries(BarsPeriodType.Tick, 300);

                stochastics = Stochastics(7, 14, 3);
                AddChartIndicator(stochastics);

                //tickCounter = TickCounter(true, false);
                //AddChartIndicator(tickCounter);

                fastMaIndicator = EMA(5);
                //AddChartIndicator(fastMaIndicator);

                slowMaIndicator = EMA(10);
                //AddChartIndicator(slowMaIndicator);

                rangeIndicator = MyRange(3, MinTickRange);
                //AddChartIndicator(rangeIndicator);

                adxIndicator = EnhancedADX(AdxPeriod, MinAdx);
                AddChartIndicator(adxIndicator);

                if (EnterPeriod > 1)
                {
                    enterAdxSlopeIndicator = AdxSlope(AdxPeriod, EnterPeriod, EnterSlopeThreshold);
                    //AddChartIndicator(enterAdxSlopeIndicator);
                }

                if (ExitPeriod > 1)
                {
                    exitAdxSlopeIndicator = AdxSlope(AdxPeriod, ExitPeriod, ExitSlopeThreshold);
                    //AddChartIndicator(exitAdxSlopeIndicator);
                }

//                disableManager.AddRange(DayOfWeek.Monday, 6, 25, 6, 50);
//                disableManager.AddRange(DayOfWeek.Tuesday, 6, 25, 6, 50);
//                disableManager.AddRange(DayOfWeek.Wednesday, 6, 25, 6, 50);
//                disableManager.AddRange(DayOfWeek.Thursday, 6, 25, 6, 50);
//                disableManager.AddRange(DayOfWeek.Friday, 6, 25, 6, 50);

                //disableManager.AddRange(DayOfWeek.Sunday, 13, 45, 24, 0);
                //disableManager.AddRange(DayOfWeek.Monday, 13, 45, 24, 0);
                //disableManager.AddRange(DayOfWeek.Tuesday, 13, 45, 24, 0);
                //disableManager.AddRange(DayOfWeek.Wednesday, 13, 45, 24, 0);
                //disableManager.AddRange(DayOfWeek.Thursday, 13, 45, 24, 0);
                //disableManager.AddRange(DayOfWeek.Friday, 13, 45, 24, 0);

                PrintTrade("Direction", 
                    "TickRange", "TickRangeBucket", 
                    "MaSlope", "MaSlopeBucket", 
                    "MaDiff", "MaDiffBucket",
                    "Adx", "AdxBucket",
                    "AdxSlope", "AdxSlopeBucket",
                    "Hour", "Fifteen", "PL", "Win");
            }
            else if (State == State.DataLoaded)
            {
                //fastMaIndicator.PlotBrushes[0][0] = Brushes.DodgerBlue;
                //slowMaIndicator.PlotBrushes[0][0] = Brushes.HotPink;
            }
        }

        #region OrderTracking
        #region OnExecutionUpdate
        protected override void OnExecutionUpdate(Cbi.Execution execution, string executionId, double price, int quantity,
            Cbi.MarketPosition marketPosition, string orderId, DateTime time)
        {
            RecordTradeProps(execution);
        }
        #endregion

        #region RecordTradeProps
        private void RecordTradeProps(Execution execution)
        {
            if (execution.Order != null && execution.Order.OrderState == OrderState.Filled)
            {
                if (!openPosition)
                {
                    this.guerillaTraderBridge.Reset();

                    this.guerillaTraderBridge.ot_entryTimestamp = execution.Order.Time;
                    this.guerillaTraderBridge.ot_tickRange = tickRange;
                    this.guerillaTraderBridge.ot_entryPrice = execution.Order.AverageFillPrice;
                    this.guerillaTraderBridge.ot_size = execution.Order.Quantity;

                    this.guerillaTraderBridge.ot_tradeType = execution.Order.Name == ENTER_LONG ? (int)TradeTypes.LongFuture : (int)TradeTypes.ShortFuture;
                    this.guerillaTraderBridge.ot_trigger = (int)TradeTriggers.Crossover;
                    this.guerillaTraderBridge.ot_trend = execution.Order.Name == ENTER_LONG ? (int)TrendTypes.Bullish : (int)TrendTypes.Bearish;

                    this.guerillaTraderBridge.ot_diff = 0;
                    this.guerillaTraderBridge.ot_diffXX = 0;
                    this.guerillaTraderBridge.ot_diffXDiff = 0;
                    this.guerillaTraderBridge.ot_diffXSlope = 0;
                    this.guerillaTraderBridge.ot_diffXChange = 0;
                    this.guerillaTraderBridge.ot_adx = adxIndicator[0];
                    this.guerillaTraderBridge.ot_adxSlope = enterAdxSlopeIndicator[0];

                    int initialStopTicks = (int)(rangeIndicator[0] * 2);
                    initialStopTicks = LossExitTicks;

                    if (execution.Order.Name == ENTER_LONG)
                    {
                        ExitLongStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice - (TickSize * initialStopTicks),
                            EXIT, ENTER_LONG);

                        //						ExitLongStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice - (TickSize * (LossExitTicks + ExitBuffer)),
                        //							EXIT, ENTER_LONG);

                        //						ExitLongStopLimit(0, true, Quantity,
                        //							this.guerillaTraderBridge.ot_entryPrice - (TickSize * (LossExitTicks - ExitBuffer)),
                        //							this.guerillaTraderBridge.ot_entryPrice - (TickSize * LossExitTicks),
                        //							EXIT, ENTER_LONG);
                    }
                    else
                    {
                        ExitShortStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice + (TickSize * initialStopTicks),
                            EXIT, ENTER_SHORT);

                        //						ExitShortStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice + (TickSize * (LossExitTicks + ExitBuffer)),
                        //							EXIT, ENTER_SHORT);

                        //						ExitShortStopLimit(0, true, Quantity,
                        //							this.guerillaTraderBridge.ot_entryPrice + (TickSize * (LossExitTicks - ExitBuffer)),
                        //							this.guerillaTraderBridge.ot_entryPrice + (TickSize * LossExitTicks),
                        //							EXIT, ENTER_SHORT);
                    }

                    this.openPosition = true;
                }
                else if (openPosition)
                {
                    this.guerillaTraderBridge.ot_exitTimestamp = execution.Order.Time;
                    this.guerillaTraderBridge.ot_exitPrice = execution.Order.AverageFillPrice;

                    if (execution.Order.Instrument.MasterInstrument.Name == "ES")
                    {
                        this.guerillaTraderBridge.ot_marketId = 1;
                        this.guerillaTraderBridge.ot_commissions = 4.04 * Quantity;
                    }
                    else if (execution.Order.Instrument.MasterInstrument.Name == "NQ")
                    {
                        this.guerillaTraderBridge.ot_marketId = 4;
                        this.guerillaTraderBridge.ot_commissions = 4.04 * Quantity;
                    }
                    else if (execution.Order.Instrument.MasterInstrument.Name == "6E")
                    {
                        this.guerillaTraderBridge.ot_marketId = 7;
                        this.guerillaTraderBridge.ot_commissions = 5.32 * Quantity;
                    }
                    else if (execution.Order.Instrument.MasterInstrument.Name == "GC")
                    {
                        this.guerillaTraderBridge.ot_marketId = 33;
                        this.guerillaTraderBridge.ot_commissions = 2.34 * Quantity;
                    }
                    else if (execution.Order.Instrument.MasterInstrument.Name == "UB")
                    {
                        this.guerillaTraderBridge.ot_marketId = 14;
                        this.guerillaTraderBridge.ot_commissions = 1.69 * Quantity;
                    }

                    if (this.guerillaTraderBridge.ot_tradeType == (int)TradeTypes.LongFuture)
                    {
                        this.guerillaTraderBridge.ot_profitLoss = ((this.guerillaTraderBridge.ot_exitPrice - this.guerillaTraderBridge.ot_entryPrice) * execution.Order.Instrument.MasterInstrument.PointValue) * this.guerillaTraderBridge.ot_size;
                    }
                    else if (this.guerillaTraderBridge.ot_tradeType == (int)TradeTypes.ShortFuture)
                    {
                        this.guerillaTraderBridge.ot_profitLoss = ((this.guerillaTraderBridge.ot_entryPrice - this.guerillaTraderBridge.ot_exitPrice) * execution.Order.Instrument.MasterInstrument.PointValue) * this.guerillaTraderBridge.ot_size;
                    }

                    this.guerillaTraderBridge.ot_adjProfitLoss = this.guerillaTraderBridge.ot_profitLoss - this.guerillaTraderBridge.ot_commissions;
                    this.guerillaTraderBridge.ot_profitLossPerContract = this.guerillaTraderBridge.ot_adjProfitLoss / this.guerillaTraderBridge.ot_size;

                    this.openPosition = false;
                    this.closing = false;
                    this.trendBarAppeared = false;
                    this.enterBarNumber = 0;
                    this.maxDivided = 0;
                    //					this.bullishSignalBarNumber = 0;
                    //					this.bearishSignalBarNumber = 0;

                    bool targetHitThisTime = false;
                    if((State == State.Realtime || this.OverrideRealTimeOnly) && 
                        this.TargetDollars > 0 && !this.targetHit)
                    {
                        this.runningPL += this.guerillaTraderBridge.ot_adjProfitLoss;

                        if (this.runningPL > this.TargetDollars)
                        {
                            this.targetHit = true;
                            targetHitThisTime = true;
                        }
                    }

                    //if (execution.Order.IsStopMarket) Print("IsStopMarket");
                    //else Print("IsMarket");

                    //PrintTrade(this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_exitTimestamp, execution.Order.IsLimit ? "Limit" : "Stop",
                    //    totalLimit, totalStop, (totalLimit / (totalLimit + totalStop)).ToString("P"), (totalStop / (totalLimit + totalStop)).ToString("P"));

                    if ((State == State.Realtime || (State == State.Historical && RecordHistoricalTrades)) && this.TradingAccountId > 0)
                    {
                        Bridge.SaveTradeFromNt(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, this.guerillaTraderBridge.ot_adx, 0, false, true, this.FireSns, true,
                            this.guerillaTraderBridge.ot_adxSlope, targetHitThisTime ? this.TargetDollars : 0);



                        //                        PrintTrade(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
                        //                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
                        //                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, this.guerillaTraderBridge.ot_adx, false);
                    }

                    if(State == State.Historical)
                    {
                        count += 1;

                        PrintTrade(
                            this.guerillaTraderBridge.ot_tradeType == (int)TradeTypes.LongFuture ? "\"long\"" : "\"short\"",
                            tickRange.ToString("F0"),
                            this.ToBucket(tickRange, 200, 5, 10),
                            maSlope.ToString("F0"),
                            this.ToBucket(maSlope, 220, 5, -120),
                            maDiff.ToString("F2"),
                            this.ToBucket(maDiff, 46, .25, -26),
                            this.guerillaTraderBridge.ot_adx.ToString("F0"),
                            this.ToBucket(this.guerillaTraderBridge.ot_adx, 60, 2.5, 15),
                            this.guerillaTraderBridge.ot_adxSlope.ToString("F0"),
                            this.ToBucket(this.guerillaTraderBridge.ot_adxSlope, 150, 5, -75),
                            this.GetHourPosition(this.guerillaTraderBridge.ot_entryTimestamp.Hour),
                            (this.GetHourPosition(this.guerillaTraderBridge.ot_entryTimestamp.Hour) * 4) + this.ToBucket(this.guerillaTraderBridge.ot_entryTimestamp.Minute, 60, 15, 0),
                            this.guerillaTraderBridge.ot_adjProfitLoss.ToString("F0"),
                            (this.guerillaTraderBridge.ot_adjProfitLoss > 0).ToString().ToLower());
                    }
                }
            }
        }

        private void PrintTrade(params object[] list)
        {
            Print(String.Join(",", list.Select(x => x.ToString())));
        }
        #endregion

        #region Hours
        private List<int> _hours;

        public List<int> Hours
        {
            get
            {
                if (_hours == null)
                {
                    _hours = new List<int>() { 15, 16, 17, 18, 19, 20, 21, 22, 23, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
                }

                return _hours;
            }
        }

        private int GetHourPosition(int hour)
        {
            return Hours.IndexOf(hour);
        }
        #endregion

        #region Buckets
        private double ToBucket(double from, double range, double bucketSize, double offset)
        {
            for (double i = 0; i < (range / bucketSize); i++)
            {
                double bucket = ((i + 1) * bucketSize) + (offset);
                if (from < bucket)
                {
                    return i;
                }
            }

            return 0;
        }
        #endregion

        #endregion

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;

            if (BarsInProgress == 0)
            {
                DateTime debugTime = new DateTime(2018, 6, 22, 4, 42, 0);
                DateTime blahTime = Time[0];

                bool adxBullTrend = adxIndicator.DmiPlusPlot[0] > adxIndicator.DmiMinusPlot[0];
                bool adxBearTrend = adxIndicator.DmiPlusPlot[0] < adxIndicator.DmiMinusPlot[0];
                bool greenBar = Close[0] > Open[0];
                bool redBar = Close[0] < Open[0];

                int enterPeriodMinusOne = EnterPeriod - 2;
                bool bullishEnterSignal = adxIndicator[0] > MinAdx;
                bool bearishEnterSignal = adxIndicator[0] > MinAdx;

                bullishEnterSignal = bullishEnterSignal && adxBullTrend && (CountIf(() => Close[0] > Open[0], EnterBarsLength) == EnterBarsLength);
                bearishEnterSignal = bearishEnterSignal && adxBearTrend && (CountIf(() => Close[0] < Open[0], EnterBarsLength) == EnterBarsLength);

                bullishEnterSignal = bullishEnterSignal && enterAdxSlopeIndicator[0] < EnterSlopeThreshold;
                bearishEnterSignal = bearishEnterSignal && enterAdxSlopeIndicator[0] < EnterSlopeThreshold;

                bool exitBarSignal = false;

                if (!this.openPosition)
                {
                    if (bullishSignalBarNumber > 0)
                    {
                        int barDiff = bullishSignalBarNumber - CurrentBar;
                        if (barDiff > BsWindowLength)
                        {
                            bullishSignalBarNumber = 0;
                            bearishSignalBarNumber = 0;
                        }
                    }

                    if (bearishSignalBarNumber > 0)
                    {
                        int barDiff = bearishSignalBarNumber - CurrentBar;
                        if (barDiff > BsWindowLength)
                        {
                            bullishSignalBarNumber = 0;
                            bearishSignalBarNumber = 0;
                        }
                    }

                    if (bullishEnterSignal)
                    {
                        bullishSignalBarNumber = CurrentBar;
                        bearishSignalBarNumber = 0;
                        return;
                    }

                    if (bearishEnterSignal)
                    {
                        bearishSignalBarNumber = CurrentBar;
                        bullishSignalBarNumber = 0;
                        return;
                    }

                    bullishEnterSignal = false;
                    bearishEnterSignal = false;

                    if (bullishSignalBarNumber > 0 && redBar)
                    {
                        bullishEnterSignal = true;
                        bearishEnterSignal = false;
                    }
                    else if (bearishSignalBarNumber > 0 && greenBar)
                    {
                        bullishEnterSignal = false;
                        bearishEnterSignal = true;
                    }
                }

                //Bullish exit
                if (this.openPosition && this.guerillaTraderBridge.ot_tradeType == (int)TradeTypes.LongFuture)
                {
					if((CurrentBar - enterBarNumber) == 1 && redBar){
						exitBarSignal = true;
					}
                    else if (trendBarAppeared)
                    {
                        //exit if red
                        exitBarSignal = redBar;
                    }
                    else if ((CurrentBar - enterBarNumber) >= MinTrendLength)
                    {
                        //true if green
                        //trendBarAppeared = Close[0] > Open[0];
                        trendBarAppeared = (CountIf(() => Close[0] > Open[0], MinTrendLength) == MinTrendLength);
                    }
                }
                //bearish exit
                else if (this.openPosition && this.guerillaTraderBridge.ot_tradeType == (int)TradeTypes.ShortFuture)
                {
                    if((CurrentBar - enterBarNumber) == 1 && greenBar){
						exitBarSignal = true;
					}
                    else if (trendBarAppeared)
                    {
                        //exit if green
                        exitBarSignal = greenBar;
                    }
                    else if ((CurrentBar - enterBarNumber) >= MinTrendLength)
                    {
                        //true if red
                        //trendBarAppeared = Close[0] < Open[0];
                        trendBarAppeared = (CountIf(() => Close[0] < Open[0], MinTrendLength) == MinTrendLength);
                    }
                }


                DateTime start = Time[0].Date.AddHours(StartHour).AddMinutes(StartMinute);
                DateTime end = Time[0].Date.AddHours(EndHour).AddMinutes(EndMinute);
                bool validTime = disableManager.IsValidTime(Time[0]);

                if (!this.openPosition && validTime && bullishEnterSignal && !targetHit)
                {
                    this.enterBarNumber = CurrentBar;
                    this.bullishSignalBarNumber = 0;
                    this.bearishSignalBarNumber = 0;
                    this.tickRange = rangeIndicator[0];
                    this.maSlope = Slope(fastMaIndicator, 5, 0) * 10;
                    this.maDiff = fastMaIndicator[0] - slowMaIndicator[0];
                    EnterShort(0, Quantity, ENTER_SHORT);
                }
                else if (!this.openPosition && validTime && bearishEnterSignal && !targetHit)
                {
                    this.enterBarNumber = CurrentBar;
                    this.bullishSignalBarNumber = 0;
                    this.bearishSignalBarNumber = 0;
                    this.tickRange = rangeIndicator[0];
                    this.maSlope = Slope(fastMaIndicator, 3, 0) * 10;
                    this.maDiff = fastMaIndicator[0] - slowMaIndicator[0];
                    EnterLong(0, Quantity, ENTER_LONG);
                }
                else if (this.openPosition && !closing && (exitBarSignal) && this.guerillaTraderBridge.ot_tradeType == 1)
                {
                    closing = true;

                    //Double exitLimitPrice = GetCurrentBid() + (TickSize * ExitLimitBuffer);
                    //if (exitLimitPrice <= Close[0]) exitLimitPrice = Close[0] + TickSize;

                    //ExitLongLimit(0, true, Quantity, exitLimitPrice, EXIT, ENTER_LONG);

                    //Double exitStopPrice = GetCurrentBid() - (TickSize * ExitStopBuffer);
                    //if (exitStopPrice >= Close[0]) exitStopPrice = Close[0] - TickSize;

                    //ExitLongStopMarket(0, true, Quantity, exitStopPrice, EXIT, ENTER_LONG);
                    ExitLong(0, Quantity, EXIT, ENTER_LONG);
                }
                else if (this.openPosition && !closing && (exitBarSignal) && this.guerillaTraderBridge.ot_tradeType == 2)
                {
                    closing = true;

                    //Double exitLimitPrice = GetCurrentAsk() - (TickSize * ExitLimitBuffer);
                    //if (exitLimitPrice >= Close[0]) exitLimitPrice = Close[0] - TickSize;

                    //ExitShortLimit(0, true, Quantity, exitLimitPrice, EXIT, ENTER_SHORT);

                    //Double exitStopPrice = GetCurrentAsk() + (TickSize * ExitStopBuffer);
                    //if (exitStopPrice <= Close[0]) exitStopPrice = Close[0] + TickSize;

                    //ExitShortStopMarket(0, true, Quantity, exitStopPrice, EXIT, ENTER_SHORT);
                    ExitShort(0, Quantity, EXIT, ENTER_SHORT);
                }
            }
            else if (BarsInProgress == 1)
            {
                if (this.openPosition && !closing && this.guerillaTraderBridge.ot_tradeType == 1)
                {
                    Double pl = ((Close[0] - this.guerillaTraderBridge.ot_entryPrice) * Instrument.MasterInstrument.PointValue) * this.guerillaTraderBridge.ot_size;
                    if (pl > this.TrailingStopMinDollars)
                    {
                        int divided = (int)(pl / TrailingStopDivisorDollars);

                        if (divided > this.maxDivided)
                        {
                            this.maxDivided = divided;
                            int stopTicks = (int)(TrailingStopCushionDollars / Instrument.MasterInstrument.PointValue / TickSize / this.guerillaTraderBridge.ot_size);
                            Double exitStopPrice = GetCurrentBid() - (TickSize * stopTicks);

                            ExitLongStopMarket(0, true, Quantity, exitStopPrice, EXIT, ENTER_LONG);
                        }
                    }
                }
                else if (this.openPosition && !closing && this.guerillaTraderBridge.ot_tradeType == 2)
                {
                    Double pl = ((this.guerillaTraderBridge.ot_entryPrice - Close[0]) * Instrument.MasterInstrument.PointValue) * this.guerillaTraderBridge.ot_size;
                    if (pl > this.TrailingStopMinDollars)
                    {
                        int divided = (int)(pl / TrailingStopDivisorDollars);

                        if (divided > this.maxDivided)
                        {
                            this.maxDivided = divided;
                            int stopTicks = (int)(TrailingStopCushionDollars / Instrument.MasterInstrument.PointValue / TickSize / this.guerillaTraderBridge.ot_size);
                            Double exitStopPrice = GetCurrentAsk() + (TickSize * ExitStopBuffer);

                            ExitShortStopMarket(0, true, Quantity, exitStopPrice, EXIT, ENTER_SHORT);
                        }
                    }
                }
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "TradingAccountId", Order = 4, GroupName = "Parameters")]
        public int TradingAccountId
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "AdxPeriod", Order = 5, GroupName = "Parameters")]
        public int AdxPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MinAdx", Order = 6, GroupName = "Parameters")]
        public double MinAdx
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "EnterPeriod", Order = 7, GroupName = "Parameters")]
        public int EnterPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(int.MinValue, int.MaxValue)]
        [Display(Name = "EnterSlopeThreshold", Order = 8, GroupName = "Parameters")]
        public double EnterSlopeThreshold
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ExitPeriod", Order = 9, GroupName = "Parameters")]
        public int ExitPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(int.MinValue, int.MaxValue)]
        [Display(Name = "ExitSlopeThreshold", Order = 10, GroupName = "Parameters")]
        public double ExitSlopeThreshold
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "LossExitTicks", Order = 11, GroupName = "Parameters")]
        public int LossExitTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "EnterBarsLength", Order = 11, GroupName = "Parameters")]
        public int EnterBarsLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ExitBarsLength", Order = 11, GroupName = "Parameters")]
        public int ExitBarsLength
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "UseBoundaryHours", Order = 12, GroupName = "Parameters")]
        public bool UseBoundaryHours
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 23)]
        [Display(Name = "StartHour", Order = 12, GroupName = "Parameters")]
        public int StartHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 45)]
        [Display(Name = "StartMinute", Order = 13, GroupName = "Parameters")]
        public int StartMinute
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 23)]
        [Display(Name = "EndHour", Order = 14, GroupName = "Parameters")]
        public int EndHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 45)]
        [Display(Name = "EndMinute", Order = 15, GroupName = "Parameters")]
        public int EndMinute
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "RecordHistoricalTrades", Order = 16, GroupName = "Parameters")]
        public bool RecordHistoricalTrades
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ExitStopBuffer", Order = 19, GroupName = "Parameters")]
        public int ExitStopBuffer
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ExitLimitBuffer", Order = 19, GroupName = "Parameters")]
        public int ExitLimitBuffer
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity", Order = 20, GroupName = "Parameters")]
        public int Quantity
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "MinTickRange", Order = 21, GroupName = "Parameters")]
        public double MinTickRange
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "FireSns", Order = 23, GroupName = "Parameters")]
        public bool FireSns
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MinTrendLength", Order = 24, GroupName = "Parameters")]
        public int MinTrendLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "BsWindowLength", Order = 25, GroupName = "Parameters")]
        public int BsWindowLength
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "TargetDollars", Order = 26, GroupName = "Parameters")]
        public double TargetDollars
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "TrailingStopMinDollars", Order = 27, GroupName = "Parameters")]
        public int TrailingStopMinDollars
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "TrailingStopDivisorDollars", Order = 28, GroupName = "Parameters")]
        public int TrailingStopDivisorDollars
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "TrailingStopCushionDollars", Order = 29, GroupName = "Parameters")]
        public int TrailingStopCushionDollars
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "OverrideRealTimeOnly", Order = 30, GroupName = "Parameters")]
        public bool OverrideRealTimeOnly
        { get; set; }
        #endregion
    }
}
