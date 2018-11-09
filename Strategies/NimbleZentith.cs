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
	public class NimbleZentith : Strategy
	{
        #region Props
        private String ENTER_LONG = "ENTER_LONG";
        private String ENTER_SHORT = "ENTER_SHORT";
        private String EXIT = "EXIT";

        private List<GuerillaStickIndicator> shootingStarIndicatorList = new List<GuerillaStickIndicator>();
        private List<GuerillaStickIndicator> hammerIndicatorList = new List<GuerillaStickIndicator>();
        private List<GuerillaStickIndicator> dojiIndicatorList = new List<GuerillaStickIndicator>();
        private List<GuerillaStickIndicator> indecisionBarIndicatorList = new List<GuerillaStickIndicator>();
        private List<GuerillaStickIndicator> bullishTrendBarIndicatorList = new List<GuerillaStickIndicator>();
        private List<GuerillaStickIndicator> bearishTrendBarIndicatorList = new List<GuerillaStickIndicator>();
        private List<GuerillaStickIndicator> greenBarIndicatorList = new List<GuerillaStickIndicator>();
        private List<GuerillaStickIndicator> redBarIndicatorList = new List<GuerillaStickIndicator>();
        private List<Stochastics> stochasticsList = new List<Stochastics>();

        private bool openingPosition = false;
        private bool closingPosition = false;

        private GuerillaStrategyItem openingPositionStrategyItem = null;
        private GuerillaStrategyItem positionStrategyItem = null;

        private Bridge guerillaTraderBridge = new Bridge();
        private DisableManager disableManager = new DisableManager();
        private GuerillaStrategy guerillaStrategy = new GuerillaStrategy();
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "NimbleZentith";
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
                Quantity = 2;
                LogTrades = false;
            }
            else if (State == State.Configure)
            {
                //Strategy tick interval: 377
                AddHeikenAshi(Instrument.FullName, BarsPeriodType.Tick, 610, MarketDataType.Last);
                AddHeikenAshi(Instrument.FullName, BarsPeriodType.Tick, 1280, MarketDataType.Last);

                guerillaStrategy.AddStrategyItem(1280, false, true, 6);
                guerillaStrategy.AddStrategyItem(610, true, false, 7);
                guerillaStrategy.AddStrategyItem(610, false, false, 9);
                guerillaStrategy.AddStrategyItem(1280, true, false, 10);
                guerillaStrategy.AddStrategyItem(377, true, true, 11);
                guerillaStrategy.AddStrategyItem(1280, true, false, 12);
            }
            else if (State == State.DataLoaded)
            {
                for (int i = 0; i < BarsArray.Count(); i++)
                {
                    shootingStarIndicatorList.Add(GuerillaStickIndicator(GuerillaChartPattern.ShootingStar, false, false, false, 0));
                    hammerIndicatorList.Add(GuerillaStickIndicator(GuerillaChartPattern.Hammer, false, false, false, 0));
                    dojiIndicatorList.Add(GuerillaStickIndicator(GuerillaChartPattern.Doji, false, false, false, 0));
                    indecisionBarIndicatorList.Add(GuerillaStickIndicator(GuerillaChartPattern.IndecisionBar, false, false, false, 0));
                    bullishTrendBarIndicatorList.Add(GuerillaStickIndicator(GuerillaChartPattern.BullishTrendBar, false, false, false, 0));
                    bearishTrendBarIndicatorList.Add(GuerillaStickIndicator(GuerillaChartPattern.BearishTrendBar, false, false, false, 0));
                    greenBarIndicatorList.Add(GuerillaStickIndicator(GuerillaChartPattern.GreenBar, false, false, false, 0));
                    redBarIndicatorList.Add(GuerillaStickIndicator(GuerillaChartPattern.RedBar, false, false, false, 0));

                    stochasticsList.Add(Stochastics(BarsArray[i], 7, 14, 3));
                }
            }
        } 
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade) return;

            GuerillaStrategyItem activeItem = guerillaStrategy.GetActiveItem(Time[0]);
            bool runActiveItem = activeItem != null && activeItem.TickInterval == BarsArray[BarsInProgress].BarsPeriod.BaseBarsPeriodValue;
            bool runPositionItem = positionStrategyItem != null && positionStrategyItem.TickInterval == BarsArray[BarsInProgress].BarsPeriod.BaseBarsPeriodValue;

            //Active only -> BAU
            if (runActiveItem && positionStrategyItem == null)
            {
                FillSignals(activeItem);
                OrderStuff(activeItem, true);
            }
            //Active == position -> BAU
            else if (runActiveItem && runPositionItem && activeItem.ToString() == positionStrategyItem.ToString())
            {
                FillSignals(activeItem);
                OrderStuff(activeItem, true);
            }
            //Active != position -> position to close
            else if(runActiveItem && runPositionItem && activeItem.ToString() != positionStrategyItem.ToString())
            {
                FillSignals(positionStrategyItem);
                OrderStuff(positionStrategyItem, false);
            }
            //Position only -> position to close
            else if (runPositionItem)
            {
                FillSignals(positionStrategyItem);
                OrderStuff(positionStrategyItem, false);
            }
        }
        #endregion

        #region FillSignals
        private void FillSignals(GuerillaStrategyItem strategyItem)
        {
            if (strategyItem == null) return;

            #region Misc
            int previousTrendLookback = 4;
            int previousTrendBarLookback = 4;
            int previousRequiredTrendBars = 2;
            int requiredTrendBars = 1;
            int jumpInTrendPeriod = 5;

            int indecisionPeriod = 5;
            bool previousIndecision = this.GuerillaCountIf(bullishTrendBarIndicatorList[BarsInProgress], x => x == 1, 2, indecisionPeriod, x => x == 0)
                && this.GuerillaCountIf(bearishTrendBarIndicatorList[BarsInProgress], x => x == 1, 2, indecisionPeriod, x => x == 0);
            #endregion

            #region Bearish Signals
            List<bool> bearishEntrySetups = new List<bool>();
            bool overbought = strategyItem.StochasticsFilter ? this.GuerillaCountIf(stochasticsList[BarsInProgress], x => x >= 70, 0, 4, x => x > 0) : true;

            #region Shooting star reversal
            bearishEntrySetups.Add(this.GuerillaCountIf(greenBarIndicatorList[BarsInProgress], x => x == 1, 2, previousTrendLookback, x => x >= previousTrendLookback - 1)
                && this.GuerillaCountIf(bullishTrendBarIndicatorList[BarsInProgress], x => x == 1, 2, previousTrendBarLookback, x => x >= previousRequiredTrendBars)
                && shootingStarIndicatorList[BarsInProgress][1] == 1
                && bearishTrendBarIndicatorList[BarsInProgress][0] == 1
                && overbought);

            if (bearishEntrySetups.Last())
            {
                strategyItem.InitialStopLoss = High[1] + TickSize;
            }
            #endregion

            #region Doji entries
            if (!bearishEntrySetups.Any(x => x))
            {
                bearishEntrySetups.Add(this.GuerillaCountIf(dojiIndicatorList[BarsInProgress], x => x == 1, 2, 2, x => x >= 1)
                    && this.GuerillaCountIf(bearishTrendBarIndicatorList[BarsInProgress], x => x == 1, 0, 2, x => x >= 1)
                && overbought);

                if (bearishEntrySetups.Last())
                {
                    int dojiIndex = dojiIndicatorList[BarsInProgress][2] == 1 ? 2 : 3;
                    strategyItem.InitialStopLoss = High[dojiIndex] + TickSize;
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }

            if (!bearishEntrySetups.Any(x => x))
            {
                bearishEntrySetups.Add(dojiIndicatorList[BarsInProgress][1] == 1
                    && bearishTrendBarIndicatorList[BarsInProgress][0] == 1
                && overbought);

                if (bearishEntrySetups.Last())
                {
                    strategyItem.InitialStopLoss = High[1] + TickSize;
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }
            #endregion

            #region Trend bar reversal
            if (!bearishEntrySetups.Any(x => x))
            {
                bearishEntrySetups.Add(this.GuerillaCountIf(greenBarIndicatorList[BarsInProgress], x => x == 1, 2, previousTrendLookback, x => x >= previousTrendLookback - 1)
                && this.GuerillaCountIf(bullishTrendBarIndicatorList[BarsInProgress], x => x == 1, 2, previousTrendBarLookback, x => x >= previousRequiredTrendBars)
                && this.GuerillaCountIf(bearishTrendBarIndicatorList[BarsInProgress], x => x == 1, 0, 2, x => x == requiredTrendBars)
                && overbought);

                if (bearishEntrySetups.Last())
                {
                    strategyItem.InitialStopLoss = High[2] + TickSize;
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }
            #endregion

            #region Indecision breakout
            if (!bearishEntrySetups.Any(x => x))
            {
                bearishEntrySetups.Add(previousIndecision
                    && this.GuerillaCountIf(bearishTrendBarIndicatorList[BarsInProgress], x => x == 1, 0, 2, x => x == requiredTrendBars));

                if (bearishEntrySetups.Last())
                {
                    double high = 0;
                    for (int i = 0; i < indecisionPeriod; i++)
                    {
                        if (High[i + 2] > high) high = High[i + 2];
                    }

                    strategyItem.InitialStopLoss = high + TickSize;
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }
            #endregion

            #region Jump in trend
            if (!bearishEntrySetups.Any(x => x) && !strategyItem.Contrarian)
            {
                bearishEntrySetups.Add(this.GuerillaCountIf(bearishTrendBarIndicatorList[BarsInProgress], x => x == 1, 0, jumpInTrendPeriod, x => x == jumpInTrendPeriod));

                if (bearishEntrySetups.Last())
                {
                    strategyItem.InitialStopLoss = MAX(High, jumpInTrendPeriod)[0];
                }
            }
            else
            {
                bearishEntrySetups.Add(false);
            }
            #endregion

            strategyItem.BearishSetupsString = String.Join(",", bearishEntrySetups.Select(x => x.ToString().ToLower()));

            strategyItem.BearishEntrySignal = bearishEntrySetups.Any(x => x);
            #endregion

            #region Bullish Signals
            List<bool> bullishEntrySetups = new List<bool>();
            bool oversold = strategyItem.StochasticsFilter ? this.GuerillaCountIf(stochasticsList[BarsInProgress], x => x <= 30, 0, 4, x => x > 0) : true;

            #region Shooting star reversal
            bullishEntrySetups.Add(this.GuerillaCountIf(redBarIndicatorList[BarsInProgress], x => x == 1, 2, previousTrendLookback, x => x >= previousTrendLookback - 1)
                && this.GuerillaCountIf(bearishTrendBarIndicatorList[BarsInProgress], x => x == 1, 2, previousTrendBarLookback, x => x >= previousRequiredTrendBars)
                && hammerIndicatorList[BarsInProgress][1] == 1
                && bullishTrendBarIndicatorList[BarsInProgress][0] == 1
                && oversold);

            if (bullishEntrySetups.Last())
            {
                strategyItem.InitialStopLoss = Low[1] - TickSize;
            }
            #endregion

            #region Doji entries
            if (!bullishEntrySetups.Any(x => x))
            {
                bullishEntrySetups.Add(this.GuerillaCountIf(dojiIndicatorList[BarsInProgress], x => x == 1, 2, 2, x => x >= 1)
                    && this.GuerillaCountIf(bullishTrendBarIndicatorList[BarsInProgress], x => x == 1, 0, 2, x => x >= 1)
                && oversold);

                if (bullishEntrySetups.Last())
                {
                    int dojiIndex = dojiIndicatorList[BarsInProgress][2] == 1 ? 2 : 3;
                    strategyItem.InitialStopLoss = Low[dojiIndex] - TickSize;
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }

            if (!bullishEntrySetups.Any(x => x))
            {
                bullishEntrySetups.Add(dojiIndicatorList[BarsInProgress][1] == 1 && bullishTrendBarIndicatorList[BarsInProgress][0] == 1
                && oversold);

                if (bullishEntrySetups.Last())
                {
                    strategyItem.InitialStopLoss = Low[1] - TickSize;
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }
            #endregion

            #region Trend bar reversal
            if (!bullishEntrySetups.Any(x => x))
            {
                bullishEntrySetups.Add(this.GuerillaCountIf(redBarIndicatorList[BarsInProgress], x => x == 1, 2, previousTrendLookback, x => x >= previousTrendLookback - 1)
                    && this.GuerillaCountIf(bearishTrendBarIndicatorList[BarsInProgress], x => x == 1, 2, previousTrendBarLookback, x => x >= previousRequiredTrendBars)
                    && this.GuerillaCountIf(bullishTrendBarIndicatorList[BarsInProgress], x => x == 1, 0, 2, x => x == requiredTrendBars)
                && oversold);

                if (bullishEntrySetups.Last())
                {
                    strategyItem.InitialStopLoss = Low[2] - TickSize;
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }
            #endregion

            #region Indecision breakout
            if (!bullishEntrySetups.Any(x => x))
            {
                bullishEntrySetups.Add(previousIndecision
                    && this.GuerillaCountIf(bullishTrendBarIndicatorList[BarsInProgress], x => x == 1, 0, 2, x => x == requiredTrendBars));

                if (bearishEntrySetups.Last())
                {
                    double low = 0;
                    for (int i = 0; i < indecisionPeriod; i++)
                    {
                        if (High[i + 2] > low) low = Low[i + 2];
                    }

                    strategyItem.InitialStopLoss = low - TickSize;
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }
            #endregion

            #region Jump in trend
            if (!bullishEntrySetups.Any(x => x) && !strategyItem.Contrarian)
            {
                bullishEntrySetups.Add(this.GuerillaCountIf(bullishTrendBarIndicatorList[BarsInProgress], x => x == 1, 0, jumpInTrendPeriod, x => x == jumpInTrendPeriod));

                if (bullishEntrySetups.Last())
                {
                    strategyItem.InitialStopLoss = MIN(Low, jumpInTrendPeriod)[0];
                }
            }
            else
            {
                bullishEntrySetups.Add(false);
            }
            #endregion

            strategyItem.BullishSetupsString = String.Join(",", bullishEntrySetups.Select(x => x.ToString().ToLower()));

            strategyItem.BullishEntrySignal = bullishEntrySetups.Any(x => x);
            #endregion

            #region Exit Signals
            strategyItem.BullishExitSignal = this.GuerillaCountIf(indecisionBarIndicatorList[BarsInProgress], x => x == 1, 0, 2, x => x == 2)
                || dojiIndicatorList[BarsInProgress][0] == 1
                || bearishTrendBarIndicatorList[BarsInProgress][0] == 1
                || strategyItem.BearishEntrySignal;

            strategyItem.BearishExitSignal = this.GuerillaCountIf(indecisionBarIndicatorList[BarsInProgress], x => x == 1, 0, 2, x => x == 2)
                || dojiIndicatorList[BarsInProgress][0] == 1
                || bullishTrendBarIndicatorList[BarsInProgress][0] == 1
                || strategyItem.BullishEntrySignal;
            #endregion
        }
        #endregion

        #region OrderStuff
        private bool OrderStuff(GuerillaStrategyItem strategyItem, bool canOpenPosition)
        {
            if (strategyItem == null) return false;

            bool orderSent = false;

            if (!strategyItem.Contrarian)
            {
                if (this.Position.MarketPosition == MarketPosition.Flat && !openingPosition && canOpenPosition && strategyItem.BearishEntrySignal)
                {
                    openingPosition = true;
                    openingPositionStrategyItem = strategyItem;

                    EnterShort(0, Quantity, ENTER_SHORT);
                    if (this.LogTrades) PrintValues(Time[0].ToString("g"), "EnterShort", strategyItem);
                }
                else if (this.Position.MarketPosition == MarketPosition.Flat && !openingPosition && canOpenPosition && strategyItem.BullishEntrySignal)
                {
                    openingPosition = true;
                    openingPositionStrategyItem = strategyItem;

                    EnterLong(0, Quantity, ENTER_LONG);
                    if(this.LogTrades) PrintValues(Time[0].ToString("g"), "EnterLong", strategyItem);
                }
                else if (this.Position.MarketPosition == MarketPosition.Long && strategyItem.BullishExitSignal)
                {
                    if (strategyItem.BearishEntrySignal && canOpenPosition)
                    {
                        openingPosition = true;
                        openingPositionStrategyItem = strategyItem;

                        closingPosition = true;

                        EnterShort(0, this.Position.Quantity, ENTER_SHORT);
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ReverseToShort", strategyItem);
                    }
                    else
                    {
                        closingPosition = true;

                        ExitLong(0, this.Position.Quantity, EXIT, ENTER_LONG);
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ExitLong", strategyItem);
                    }
                }
                else if (this.Position.MarketPosition == MarketPosition.Short && strategyItem.BearishExitSignal)
                {
                    if (strategyItem.BullishEntrySignal && canOpenPosition)
                    {
                        openingPosition = true;
                        openingPositionStrategyItem = strategyItem;

                        closingPosition = true;

                        EnterLong(0, this.Position.Quantity, ENTER_LONG);
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ReverseToLong", strategyItem);
                    }
                    else
                    {
                        closingPosition = true;

                        ExitShort(0, this.Position.Quantity, EXIT, ENTER_SHORT);
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ExitShort", strategyItem);
                    }
                }
            }
            else
            {
                if (this.Position.MarketPosition == MarketPosition.Flat && !openingPosition && canOpenPosition && strategyItem.BullishEntrySignal)
                {
                    openingPosition = true;
                    openingPositionStrategyItem = strategyItem;

                    EnterShort(0, this.Quantity, ENTER_SHORT);
                    if (this.LogTrades) PrintValues(Time[0].ToString("g"), "EnterShort", strategyItem);
                }
                else if (this.Position.MarketPosition == MarketPosition.Flat && !openingPosition && canOpenPosition && strategyItem.BearishEntrySignal)
                {
                    openingPosition = true;
                    openingPositionStrategyItem = strategyItem;

                    EnterLong(0, this.Quantity, ENTER_LONG);
                    if (this.LogTrades) PrintValues(Time[0].ToString("g"), "EnterLong", strategyItem);
                }
                else if (this.Position.MarketPosition == MarketPosition.Long && strategyItem.BearishExitSignal)
                {
                    if (strategyItem.BullishEntrySignal && canOpenPosition)
                    {
                        openingPosition = true;
                        openingPositionStrategyItem = strategyItem;

                        closingPosition = true;

                        EnterShort(0, this.Position.Quantity, ENTER_SHORT);
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ReverseToShort", strategyItem);
                    }
                    else
                    {
                        closingPosition = true;
                        ExitLong(0, this.Position.Quantity, EXIT, ENTER_LONG);
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ExitLong", strategyItem);
                    }
                }
                else if (this.Position.MarketPosition == MarketPosition.Short && strategyItem.BullishExitSignal)
                {
                    if (strategyItem.BearishEntrySignal && canOpenPosition)
                    {
                        openingPosition = true;
                        openingPositionStrategyItem = strategyItem;

                        closingPosition = true;

                        EnterLong(0, this.Position.Quantity, ENTER_LONG);
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ReverseToLong", strategyItem);
                    }
                    else
                    {
                        closingPosition = true;
                        ExitShort(0, this.Position.Quantity, EXIT, ENTER_SHORT);
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ExitShort", strategyItem);
                    }
                }
            }

            return orderSent;
        }
        #endregion

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
                if ((execution.Order.Name == ENTER_LONG || execution.Order.Name == ENTER_SHORT) && this.Position.Quantity >= this.Quantity)
                {
                    this.guerillaTraderBridge.Reset();

                    this.guerillaTraderBridge.ot_entryTimestamp = execution.Order.Time;
                    this.guerillaTraderBridge.ot_tickRange = 0;
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
                    this.guerillaTraderBridge.ot_adx = 0;
                    this.guerillaTraderBridge.ot_adxSlope = 0;
                    this.guerillaTraderBridge.ot_extraData1 = this.openingPositionStrategyItem.BearishSetupsString;
                    this.guerillaTraderBridge.ot_extraData2 = this.openingPositionStrategyItem.BullishSetupsString;

                    if (this.openingPositionStrategyItem.InitialStopLoss > 0)
                    {
                        if (execution.Order.Name == ENTER_LONG)
                        {
                            if (this.openingPositionStrategyItem.Contrarian)
                            {
                                this.openingPositionStrategyItem.InitialStopLoss = this.guerillaTraderBridge.ot_entryPrice - (this.openingPositionStrategyItem.InitialStopLoss - this.guerillaTraderBridge.ot_entryPrice);
                            }

                            double currentBid = GetCurrentBid();
                            if (this.openingPositionStrategyItem.InitialStopLoss >= currentBid)
                            {
                                this.openingPositionStrategyItem.InitialStopLoss = currentBid - TickSize;
                            }

                            ExitLongStopMarket(0, true, this.Position.Quantity, this.openingPositionStrategyItem.InitialStopLoss, EXIT, ENTER_LONG);
                        }
                        else
                        {
                            if (this.openingPositionStrategyItem.Contrarian)
                            {
                                this.openingPositionStrategyItem.InitialStopLoss = this.guerillaTraderBridge.ot_entryPrice + (this.guerillaTraderBridge.ot_entryPrice - this.openingPositionStrategyItem.InitialStopLoss);
                            }

                            double currentAsk = GetCurrentAsk();
                            if (this.openingPositionStrategyItem.InitialStopLoss <= currentAsk)
                            {
                                this.openingPositionStrategyItem.InitialStopLoss = currentAsk + TickSize;
                            }

                            ExitShortStopMarket(0, true, this.Position.Quantity, this.openingPositionStrategyItem.InitialStopLoss, EXIT, ENTER_SHORT);
                        }

                        this.openingPositionStrategyItem.InitialStopLoss = 0;
                    }

                    this.openingPosition = false;
                    this.positionStrategyItem = openingPositionStrategyItem;
                    this.openingPositionStrategyItem = null;
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

                    if (State == State.Realtime && this.TradingAccountId > 0)
                    {
                        Bridge.SaveTradeFromNt(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, this.guerillaTraderBridge.ot_adx, 0, false, true, false,
                            this.positionStrategyItem.Contrarian,
                            this.guerillaTraderBridge.ot_adxSlope, 0, this.guerillaTraderBridge.ot_extraData1, this.guerillaTraderBridge.ot_extraData2, this.positionStrategyItem.ToString());
                    }

                    if (execution.Order.IsStopMarket)
                    {
                        if (this.LogTrades) PrintValues(Time[0].ToString("g"), "ExitStop", positionStrategyItem);
                    }

                    this.closingPosition = false;
                    this.positionStrategyItem = null;
                }
            }
        }

        private void PrintTrade(params object[] list)
        {
            Print(String.Join(",", list.Select(x => x.ToString())));
        }
        #endregion
        #endregion

        #region Utilities
        #region GuerillaCountIf
        private bool GuerillaCountIf(Indicator indicator, Func<double, bool> compareFunc, int offset, int period, Func<int, bool> countFunc)
        {
            return countFunc(CountIf(() => compareFunc(indicator[offset]), period));
        }
        #endregion

        #region PrintValues
        private void PrintValues(params object[] list)
        {
            Print(String.Join(",", list.Select(x => x.ToString())));
        }
        #endregion 
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "TradingAccountId", Order = 1, GroupName = "Parameters")]
        public int TradingAccountId
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Quantity", Order = 5, GroupName = "Parameters")]
        public int Quantity
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "LogTrades", Order = 10, GroupName = "Parameters")]
        public bool LogTrades
        { get; set; }
        #endregion
    }
}
