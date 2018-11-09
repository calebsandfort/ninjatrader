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
	public class Fastigium : Strategy
	{
        #region Props
        private String ENTER_LONG = "ENTER_LONG";
        private String ENTER_SHORT = "ENTER_SHORT";
        private String EXIT = "EXIT";

        private GuerillaStickIndicator shootingStarIndicator;
        private GuerillaStickIndicator hammerIndicator;
        private GuerillaStickIndicator dojiIndicator;
        private GuerillaStickIndicator indecisionBarIndicator;
        private GuerillaStickIndicator bullishTrendBarIndicator;
        private GuerillaStickIndicator bearishTrendBarIndicator;
        private GuerillaStickIndicator greenBarIndicator;
        private GuerillaStickIndicator redBarIndicator;
        private Stochastics stochastics;
        
        private String bearishSetupsString = String.Empty;
        private String bullishSetupsString = String.Empty;
        private double initialStopLoss = 0;

        private Bridge guerillaTraderBridge = new Bridge();
        private DisableManager disableManager = new DisableManager();
        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "Fastigium";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
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
				BarsRequiredToTrade							= 20;

                RecordHistoricalTrades                      = false;
                TradingAccountId                            = 0;
                Quantity                                    = 2;
                FireSns                                     = false;
                Contrarian                                  = false;
                StochasticsFilter                                   = false;
                Conservative = true;
                StopLossTicks = 15;
                ProfitTakerTicks = 10;

                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
                shootingStarIndicator = GuerillaStickIndicator(GuerillaChartPattern.ShootingStar, false, false, false, 0);
                AddChartIndicator(shootingStarIndicator);

                hammerIndicator = GuerillaStickIndicator(GuerillaChartPattern.Hammer, false, false, false, 0);
                AddChartIndicator(hammerIndicator);

                dojiIndicator = GuerillaStickIndicator(GuerillaChartPattern.Doji, false, false, false, 0);
                AddChartIndicator(dojiIndicator);

                indecisionBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.IndecisionBar, false, false, false, 0);
                AddChartIndicator(indecisionBarIndicator);

                bullishTrendBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.BullishTrendBar, false, false, false, 0);
                AddChartIndicator(bullishTrendBarIndicator);

                bearishTrendBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.BearishTrendBar, false, false, false, 0);
                AddChartIndicator(bearishTrendBarIndicator);

                greenBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.GreenBar, false, false, false, 0);
                AddChartIndicator(greenBarIndicator);

                redBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.RedBar, false, false, false, 0);
                AddChartIndicator(redBarIndicator);

                stochastics = Stochastics(7, 14, 3);
                AddChartIndicator(stochastics);

                disableManager.AddRange(DayOfWeek.Sunday, 13, 30, 24, 0);
                disableManager.AddRange(DayOfWeek.Monday, 13, 30, 16, 40);
                disableManager.AddRange(DayOfWeek.Tuesday, 13, 30, 16, 40);
                disableManager.AddRange(DayOfWeek.Wednesday, 13, 30, 16, 40);
                disableManager.AddRange(DayOfWeek.Thursday, 13, 30, 16, 40);
                disableManager.AddRange(DayOfWeek.Friday, 13, 30, 16, 40);
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
                if (
                    //(execution.Order.Name == ENTER_LONG || execution.Order.Name == ENTER_SHORT) && 
                    this.Position.Quantity >= this.Quantity
                    )
                {
                    this.guerillaTraderBridge.Reset();

                    this.guerillaTraderBridge.ot_entryTimestamp = execution.Order.Time;
                    this.guerillaTraderBridge.ot_tickRange = 0;
                    this.guerillaTraderBridge.ot_entryPrice = execution.Order.AverageFillPrice;
                    this.guerillaTraderBridge.ot_size = execution.Order.Quantity;

                    this.guerillaTraderBridge.ot_tradeType = this.Position.MarketPosition == MarketPosition.Long ? (int)TradeTypes.LongFuture : (int)TradeTypes.ShortFuture;
                    this.guerillaTraderBridge.ot_trigger = (int)TradeTriggers.Crossover;
                    this.guerillaTraderBridge.ot_trend = this.Position.MarketPosition == MarketPosition.Long ? (int)TrendTypes.Bullish : (int)TrendTypes.Bearish;

                    this.guerillaTraderBridge.ot_diff = 0;
                    this.guerillaTraderBridge.ot_diffXX = 0;
                    this.guerillaTraderBridge.ot_diffXDiff = 0;
                    this.guerillaTraderBridge.ot_diffXSlope = 0;
                    this.guerillaTraderBridge.ot_diffXChange = 0;
                    this.guerillaTraderBridge.ot_adx = 0;
                    this.guerillaTraderBridge.ot_adxSlope = 0;
                    this.guerillaTraderBridge.ot_extraData1 = this.bearishSetupsString;
                    this.guerillaTraderBridge.ot_extraData2 = this.bullishSetupsString;

                    double stopLossValue = 0;
                    double profitTakerPrice = 0;
                    double profitTakerMultiplier = 1;

                    if (initialStopLoss > 0)
                    {
                        if (this.Position.MarketPosition == MarketPosition.Long)
                        {
                            stopLossValue = this.guerillaTraderBridge.ot_entryPrice - initialStopLoss;

                            if (this.Contrarian)
                            {
                                initialStopLoss = this.guerillaTraderBridge.ot_entryPrice + stopLossValue;
                                profitTakerPrice = this.guerillaTraderBridge.ot_entryPrice - (stopLossValue * profitTakerMultiplier);
                            }
                            else
                            {
                                profitTakerPrice = this.Instrument.MasterInstrument.RoundToTickSize(this.guerillaTraderBridge.ot_entryPrice + (stopLossValue * profitTakerMultiplier));
                            }

                            double currentBid = GetCurrentBid();
                            if (initialStopLoss >= currentBid)
                            {
                                initialStopLoss = currentBid - TickSize;
                            }

                            ExitLongStopMarket(0, true, this.Position.Quantity, initialStopLoss, EXIT, ENTER_LONG);

                            if (this.Conservative)
                            {
                                ExitLongLimit(0, true, Quantity, profitTakerPrice, EXIT, ENTER_LONG);
                            }

//                            ExitLongStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice - (this.StopLossTicks * TickSize), EXIT, ENTER_LONG);
//                            ExitLongLimit(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice + (this.ProfitTakerTicks * TickSize), EXIT, ENTER_LONG);
                        }
                        else
                        {
                            stopLossValue = initialStopLoss - this.guerillaTraderBridge.ot_entryPrice;

                            if (this.Contrarian)
                            {
                                initialStopLoss = this.guerillaTraderBridge.ot_entryPrice - stopLossValue;
                                profitTakerPrice = this.guerillaTraderBridge.ot_entryPrice + (stopLossValue * profitTakerMultiplier);
                            }
                            else
                            {
                                profitTakerPrice = this.Instrument.MasterInstrument.RoundToTickSize(this.guerillaTraderBridge.ot_entryPrice - (stopLossValue * profitTakerMultiplier));
                            }

                            double currentAsk = GetCurrentAsk();
                            if (initialStopLoss <= currentAsk)
                            {
                                initialStopLoss = currentAsk + TickSize;
                            }

                            ExitShortStopMarket(0, true, this.Position.Quantity, initialStopLoss, EXIT, ENTER_SHORT);

                            if (this.Conservative)
                            {
                                ExitShortLimit(0, true, Quantity, profitTakerPrice, EXIT, ENTER_SHORT);
                            }

//                            ExitShortStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice + (this.StopLossTicks * TickSize), EXIT, ENTER_SHORT);
//                            ExitShortLimit(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice - (this.ProfitTakerTicks * TickSize), EXIT, ENTER_SHORT);
                        }

                        initialStopLoss = 0;
                    }
                }
                else if (this.Position.MarketPosition == MarketPosition.Flat)
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

                    if ((State == State.Realtime || (State == State.Historical && RecordHistoricalTrades)) && this.TradingAccountId > 0)
                    {
                        Bridge.SaveTradeFromNt(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, this.guerillaTraderBridge.ot_adx, 0, false, true, this.FireSns, this.Contrarian,
                            this.guerillaTraderBridge.ot_adxSlope, 0, this.guerillaTraderBridge.ot_extraData1, this.guerillaTraderBridge.ot_extraData2);
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

        private List<int> Hours
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

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;

            //props
            int previousTrendLookback = 4;
            int previousTrendBarLookback = 4;
            int previousRequiredTrendBars = 2;
            int requiredTrendBars = 1;
            int jumpInTrendPeriod = 5;

            int indecisionPeriod = 5;
            bool previousIndecision = this.GuerillaCountIf(bullishTrendBarIndicator, x => x == 1, 2, indecisionPeriod, x => x == 0)
                && this.GuerillaCountIf(bearishTrendBarIndicator, x => x == 1, 2, indecisionPeriod, x => x == 0);

            //Bearish
            List<bool> bearishEntrySetups = new List<bool>();
            bool overbought = this.StochasticsFilter ? this.GuerillaCountIf(stochastics, x => x >= 70, 0, 4, x => x > 0) : true;

            //Shooting star reversal
            bearishEntrySetups.Add(this.GuerillaCountIf(greenBarIndicator, x => x == 1, 2, previousTrendLookback, x => x >= previousTrendLookback - 1)
                && this.GuerillaCountIf(bullishTrendBarIndicator, x => x == 1, 2, previousTrendBarLookback, x => x >= previousRequiredTrendBars)
                && shootingStarIndicator[1] == 1
                && bearishTrendBarIndicator[0] == 1
                && overbought);

            if (bearishEntrySetups.Last())
            {
                initialStopLoss = High[1] + TickSize;
            }

            //Doji entries
            if (!bearishEntrySetups.Any(x => x))
            {
                bearishEntrySetups.Add(this.GuerillaCountIf(dojiIndicator, x => x == 1, 2, 2, x => x >= 1)
                    && this.GuerillaCountIf(bearishTrendBarIndicator, x => x == 1, 0, 2, x => x >= 1)
                && overbought);

                if (bearishEntrySetups.Last())
                {
                    int dojiIndex = dojiIndicator[2] == 1 ? 2 : 3;
                    initialStopLoss = High[dojiIndex] + TickSize;
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }

            if (!bearishEntrySetups.Any(x => x))
            {
                bearishEntrySetups.Add(dojiIndicator[1] == 1
                    && bearishTrendBarIndicator[0] == 1
                && overbought);

                if (bearishEntrySetups.Last())
                {
                    initialStopLoss = High[1] + TickSize;
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }

            //Trend bar reversal
            if (!bearishEntrySetups.Any(x => x))
            {
                bearishEntrySetups.Add(this.GuerillaCountIf(greenBarIndicator, x => x == 1, 2, previousTrendLookback, x => x >= previousTrendLookback - 1)
                && this.GuerillaCountIf(bullishTrendBarIndicator, x => x == 1, 2, previousTrendBarLookback, x => x >= previousRequiredTrendBars)
                && this.GuerillaCountIf(bearishTrendBarIndicator, x => x == 1, 0, 2, x => x == requiredTrendBars)
                && overbought);

                if (bearishEntrySetups.Last())
                {
                    initialStopLoss = High[2] + TickSize;
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }

            //Indecision breakout
            if (!bearishEntrySetups.Any(x => x) && !this.Conservative)
            {
                bearishEntrySetups.Add(previousIndecision
                    && this.GuerillaCountIf(bearishTrendBarIndicator, x => x == 1, 0, 2, x => x == requiredTrendBars));

                if (bearishEntrySetups.Last())
                {
                    double high = 0;
                    for (int i = 0; i < indecisionPeriod; i++)
                    {
                        if (High[i + 2] > high) high = High[i + 2];
                    }

                    initialStopLoss = high + TickSize;
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }

            //Jump in trend
            if (!bearishEntrySetups.Any(x => x) && !this.Contrarian && !this.Conservative)
            {
                bearishEntrySetups.Add(this.GuerillaCountIf(bearishTrendBarIndicator, x => x == 1, 0, jumpInTrendPeriod, x => x == jumpInTrendPeriod));

                if (bearishEntrySetups.Last())
                {
                    initialStopLoss = MAX(High, jumpInTrendPeriod)[0];
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }

            this.bearishSetupsString = String.Join(",", bearishEntrySetups.Select(x => x.ToString().ToLower()));

            bool bearishEntrySignal = bearishEntrySetups.Any(x => x);

            //Bullish
            List<bool> bullishEntrySetups = new List<bool>();
            bool oversold = this.StochasticsFilter ? this.GuerillaCountIf(stochastics, x => x <= 30, 0, 4, x => x > 0) : true;

            //Shooting star reversal
            bullishEntrySetups.Add(this.GuerillaCountIf(redBarIndicator, x => x == 1, 2, previousTrendLookback, x => x >= previousTrendLookback - 1)
                && this.GuerillaCountIf(bearishTrendBarIndicator, x => x == 1, 2, previousTrendBarLookback, x => x >= previousRequiredTrendBars)
                && hammerIndicator[1] == 1
                && bullishTrendBarIndicator[0] == 1
                && oversold);

            if (bullishEntrySetups.Last())
            {
                initialStopLoss = Low[1] - TickSize;
            }

            //Doji entries
            if (!bullishEntrySetups.Any(x => x))
            {
                bullishEntrySetups.Add(this.GuerillaCountIf(dojiIndicator, x => x == 1, 2, 2, x => x >= 1)
                    && this.GuerillaCountIf(bullishTrendBarIndicator, x => x == 1, 0, 2, x => x >= 1)
                && oversold);

                if (bullishEntrySetups.Last())
                {
                    int dojiIndex = dojiIndicator[2] == 1 ? 2 : 3;
                    initialStopLoss = Low[dojiIndex] - TickSize;
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }

            if (!bullishEntrySetups.Any(x => x))
            {
                bullishEntrySetups.Add(dojiIndicator[1] == 1 && bullishTrendBarIndicator[0] == 1
                && oversold);

                if (bullishEntrySetups.Last())
                {
                    initialStopLoss = Low[1] - TickSize;
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }

            //Trend bar reversal
            if (!bullishEntrySetups.Any(x => x))
            {
                bullishEntrySetups.Add(this.GuerillaCountIf(redBarIndicator, x => x == 1, 2, previousTrendLookback, x => x >= previousTrendLookback - 1)
                    && this.GuerillaCountIf(bearishTrendBarIndicator, x => x == 1, 2, previousTrendBarLookback, x => x >= previousRequiredTrendBars)
                    && this.GuerillaCountIf(bullishTrendBarIndicator, x => x == 1, 0, 2, x => x == requiredTrendBars)
                && oversold);

                if (bullishEntrySetups.Last())
                {
                    initialStopLoss = Low[2] - TickSize;
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }

            //Indecision breakout
            if (!bullishEntrySetups.Any(x => x) && !this.Conservative)
            {
                bullishEntrySetups.Add(previousIndecision
                    && this.GuerillaCountIf(bullishTrendBarIndicator, x => x == 1, 0, 2, x => x == requiredTrendBars));

                if (bearishEntrySetups.Last())
                {
                    double low = 0;
                    for (int i = 0; i < indecisionPeriod; i++)
                    {
                        if (High[i + 2] > low) low = Low[i + 2];
                    }

                    initialStopLoss = low - TickSize;
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }

            //Jump in trend
            if (!bullishEntrySetups.Any(x => x) && !this.Contrarian && !this.Conservative)
            {
                bullishEntrySetups.Add(this.GuerillaCountIf(bullishTrendBarIndicator, x => x == 1, 0, jumpInTrendPeriod, x => x == jumpInTrendPeriod));

                if (bullishEntrySetups.Last())
                {
                    initialStopLoss = MIN(Low, jumpInTrendPeriod)[0];
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }

            this.bullishSetupsString = String.Join(",", bullishEntrySetups.Select(x => x.ToString().ToLower()));

            bool bullishEntrySignal = bullishEntrySetups.Any(x => x);

            //Exit
            bool bullishExitSignal = this.GuerillaCountIf(indecisionBarIndicator, x => x == 1, 0, 2, x => x == 2)
                || dojiIndicator[0] == 1
                || bearishTrendBarIndicator[0] == 1
                || bearishEntrySignal;

            bool bearishExitSignal = this.GuerillaCountIf(indecisionBarIndicator, x => x == 1, 0, 2, x => x == 2)
                || dojiIndicator[0] == 1
                || bullishTrendBarIndicator[0] == 1
                || bullishEntrySignal;

            bool validTime = disableManager.IsValidTime(Time[0]);

            if (!Contrarian)
            {
                if (this.Position.MarketPosition == MarketPosition.Flat && validTime && bearishEntrySignal)
                {
                    EnterShort(0, Quantity, ENTER_SHORT);
                }
                else if (this.Position.MarketPosition == MarketPosition.Flat && validTime && bullishEntrySignal)
                {
                    EnterLong(0, Quantity, ENTER_LONG);
                }
                else if (this.Position.MarketPosition == MarketPosition.Long && bullishExitSignal)
                {
                    if (bearishEntrySignal)
                    {
                        EnterShort(0, this.Position.Quantity, ENTER_SHORT);
                    }
                    else if(!this.Conservative)
                    {
                        ExitLong(0, this.Position.Quantity, EXIT, ENTER_LONG);
                    }
                }
                else if (this.Position.MarketPosition == MarketPosition.Short && bearishExitSignal)
                {
                    if (bullishEntrySignal)
                    {
                        EnterLong(0, this.Position.Quantity, ENTER_LONG);
                    }
                    else if (!this.Conservative)
                    {
                        ExitShort(0, this.Position.Quantity, EXIT, ENTER_SHORT);
                    }
                }
            }
            else
            {
                if (this.Position.MarketPosition == MarketPosition.Flat && validTime && bullishEntrySignal)
                {
                    EnterShort(0, this.Quantity, ENTER_SHORT);
                }
                else if (this.Position.MarketPosition == MarketPosition.Flat && validTime && bearishEntrySignal)
                {
                    EnterLong(0, this.Quantity, ENTER_LONG);
                }
                else if (this.Position.MarketPosition == MarketPosition.Long && bearishExitSignal)
                {
                    if (bullishEntrySignal)
                    {
                        EnterShort(0, this.Position.Quantity, ENTER_SHORT);
                    }
                    else if (!this.Conservative)
                    {
                        ExitLong(0, this.Position.Quantity, EXIT, ENTER_LONG);
                    }
                }
                else if (this.Position.MarketPosition == MarketPosition.Short && bullishExitSignal)
                {
                    if (bearishEntrySignal)
                    {
                        EnterLong(0, this.Position.Quantity, ENTER_LONG);
                    }
                    else if (!this.Conservative)
                    {
                        ExitShort(0, this.Position.Quantity, EXIT, ENTER_SHORT);
                    }
                }
            }
        }
        #endregion

        #region GuerillaCountIf
        private bool GuerillaCountIf(Indicator indicator, Func<double, bool> compareFunc, int offset, int period, Func<int, bool> countFunc)
        {
            return countFunc(CountIf(() => compareFunc(indicator[offset]), period));
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "TradingAccountId", Order = 15, GroupName = "Parameters")]
        public int TradingAccountId
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "RecordHistoricalTrades", Order = 16, GroupName = "Parameters")]
        public bool RecordHistoricalTrades
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity", Order = 20, GroupName = "Parameters")]
        public int Quantity
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "FireSns", Order = 23, GroupName = "Parameters")]
        public bool FireSns
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Contrarian", Order = 27, GroupName = "Parameters")]
        public bool Contrarian
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "StochasticsFilter", Order = 28, GroupName = "Parameters")]
        public bool StochasticsFilter
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "StopLossTicks", Order = 29, GroupName = "Parameters")]
        public int StopLossTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ProfitTakerTicks", Order = 30, GroupName = "Parameters")]
        public int ProfitTakerTicks
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Conservative", Order = 35, GroupName = "Parameters")]
        public bool Conservative
        { get; set; }
        #endregion
    }
}
