#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using System.Net.Http;
using System.Globalization;
using GuerillaTraderBridge;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class ThaCrossover : Strategy
	{
        #region Props
        private Brush BullishBrush = Brushes.DarkBlue;
        private Brush BearishBrush = Brushes.HotPink;
        private Brush NeutralBrush = Brushes.Gray;

        private Series<double> diffSeries;
        private Series<double> highLowSeries;

        private EMA fastMaIndicator;
        private EMA slowMaIndicator;
        private MyRange myRange;

        private double tickRange;
        private bool openPosition = false;
        private Bridge guerillaTraderBridge = new Bridge();
        #endregion

        #region OrderTracking
        #region RecordTradeProps
        private void RecordTradeProps(Execution execution)
        {
            if (execution.Order != null && execution.Order.OrderState == OrderState.Filled)
            {
                //TradingAccountId = 8;
                //ES = 1
                //NQ = 4

                if (!openPosition)
                {
                    this.guerillaTraderBridge.Reset();

                    this.guerillaTraderBridge.ot_entryTimestamp = execution.Order.Time;
                    this.guerillaTraderBridge.ot_tickRange = tickRange;
                    this.guerillaTraderBridge.ot_entryPrice = execution.Order.AverageFillPrice;
                    this.guerillaTraderBridge.ot_size = execution.Order.Quantity;

                    this.guerillaTraderBridge.ot_tradeType = execution.Order.Name == "Buy" ? (int)TradeTypes.LongFuture : (int)TradeTypes.ShortFuture;
                    this.guerillaTraderBridge.ot_trigger = (int)TradeTriggers.Crossover;
                    this.guerillaTraderBridge.ot_trend = execution.Order.Name == "Buy" ? (int)TrendTypes.Bullish : (int)TrendTypes.Bearish;

                    this.guerillaTraderBridge.ot_diff = diffSeries[0];
                    this.guerillaTraderBridge.ot_diffXX = DiffXX;
                    this.guerillaTraderBridge.ot_diffXDiff = diffSeries[DiffXX] - diffSeries[0];
                    this.guerillaTraderBridge.ot_diffXSlope = this.guerillaTraderBridge.ot_diffXDiff / DiffXX;
                    this.guerillaTraderBridge.ot_diffXChange = this.guerillaTraderBridge.ot_diffXDiff / diffSeries[0];

                    this.openPosition = true;
                }
                else if (openPosition)
                {
                    this.guerillaTraderBridge.ot_exitTimestamp = execution.Order.Time;
                    this.guerillaTraderBridge.ot_exitPrice = execution.Order.AverageFillPrice;

                    if (execution.Order.Instrument.MasterInstrument.Name == "ES")
                    {
                        this.guerillaTraderBridge.ot_marketId = 1;
                        this.guerillaTraderBridge.ot_commissions = 4.04;
                    }
                    else if (execution.Order.Instrument.MasterInstrument.Name == "NQ")
                    {
                        this.guerillaTraderBridge.ot_marketId = 4;
                        this.guerillaTraderBridge.ot_commissions = 4.04;
                    }
                    else if (execution.Order.Instrument.MasterInstrument.Name == "6E")
                    {
                        this.guerillaTraderBridge.ot_marketId = 7;
                        this.guerillaTraderBridge.ot_commissions = 5.32;
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

                    if (State == State.Realtime && this.TradingAccountId > 0)
                    {
                        Bridge.SaveTradeFromNt(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, false);
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Event Handler - OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "ThaCrossover";
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
                DefaultQuantity = 1;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = false;

                FastMaPeriod = 10;
                SlowMaPeriod = 20;
                TickRangePeriod = 15;
                MinMaDiff = 0.4;
                MaxMaDiff = 10;
                MinTickRange = 0;
                MaxTickRange = 30;
                GoLong = true;
                GoShort = true;
                FireAlerts = false;
                DiffXX = 5;
                ProfitTarget = 5;
                StopLoss = 12;
                TradingAccountId = 0;
                BailPeriod = 3;
				GoPeriod = 3;
				RangePeriod = 20;
				MinRange = 8.5;
            }
            else if (State == State.Configure)
            {
                //SetProfitTarget(CalculationMode.Ticks, this.ProfitTarget);
                //SetStopLoss(CalculationMode.Ticks, this.StopLoss);
                SetOrderQuantity = SetOrderQuantity.DefaultQuantity;

                fastMaIndicator = EMA(FastMaPeriod);
                slowMaIndicator = EMA(SlowMaPeriod);
				myRange = MyRange(RangePeriod, MinRange);

                slowMaIndicator.Plots[0].Brush = Brushes.White;
                slowMaIndicator.Plots[0].Width = 2;

                fastMaIndicator.Plots[0].Width = 2;

                AddChartIndicator(fastMaIndicator);
                AddChartIndicator(slowMaIndicator);
            }
            else if (State == State.DataLoaded)
            {
                diffSeries = new Series<double>(this);
                highLowSeries = new Series<double>(this);

                //this.guerillaTraderBridge.Reset();
            }
        }
        #endregion

        #region Order Events
        protected override void OnExecutionUpdate(Cbi.Execution execution, string executionId, double price, int quantity,
            Cbi.MarketPosition marketPosition, string orderId, DateTime time)
        {
            RecordTradeProps(execution);
        }

        //      protected override void OnOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice, 
        //	int quantity, int filled, double averageFillPrice, 
        //	Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
        //{

        //}

        //protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
        //	int quantity, Cbi.MarketPosition marketPosition)
        //{

        //} 
        #endregion

        #region Event Handler - OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (FastMaPeriod >= SlowMaPeriod) return;

            if (CurrentBar < SlowMaPeriod) return;

            double fastMa = fastMaIndicator[0];
            double slowMa = slowMaIndicator[0];

            diffSeries[0] = fastMa - slowMa;

            highLowSeries[0] = (High[0] - Low[0]) / TickSize;
            tickRange = EMA(highLowSeries, TickRangePeriod)[0];

            if (CurrentBar < (SlowMaPeriod + Math.Max(Math.Max(GoPeriod, BailPeriod), DiffXX))) return;

            bool validTickRange = tickRange > MinTickRange && tickRange < MaxTickRange;

            Brush trendBrush = (diffSeries[0] < MinMaDiff && diffSeries[0] > -MinMaDiff) ? NeutralBrush : fastMa < slowMa ? BearishBrush : BullishBrush;
            fastMaIndicator.PlotBrushes[0][0] = trendBrush;

            bool crossAboveBull = diffSeries[0] >= MinMaDiff && diffSeries[1] < MinMaDiff;
            bool crossBelowBull = diffSeries[0] < MinMaDiff && diffSeries[1] >= MinMaDiff;

            bool crossAboveBear = diffSeries[0] > -MinMaDiff && diffSeries[1] <= -MinMaDiff;
            bool crossBelowBear = diffSeries[0] <= -MinMaDiff && diffSeries[1] > -MinMaDiff;

			bool goLong = true;
			bool goShort = true;
			
			if(GoPeriod > 0)
			{
				for(int i = 0; i < GoPeriod; i++)
				{
					if(diffSeries[i] < diffSeries[i+1])
					{
						goLong = false;
					}
					
					if(diffSeries[i] > diffSeries[i+1])
					{
						goShort = false;
					}
				}
			}
			else
			{
				goLong = false;
				goShort = false;
			}
			
			goLong = goLong && diffSeries[0] >= MinMaDiff;
			goShort = goShort && diffSeries[0] <= -MinMaDiff;
			
			bool bailLong = true;
			bool bailShort = true;
			
			if(BailPeriod > 0)
			{
				for(int i = 0; i < BailPeriod; i++)
				{
					if(diffSeries[i] > diffSeries[i+1])
					{
						bailLong = false;
					}
					
					if(diffSeries[i] < diffSeries[i+1])
					{
						bailShort = false;
					}
				}
			}
			else
			{
				bailLong = false;
				bailShort = false;
			}
			
            if (!this.openPosition && (crossAboveBull || goLong))
            {
                EnterLong();
            }
            else if (!this.openPosition && (crossBelowBear || goShort))
            {
                EnterShort();
            }
            else if (this.openPosition && (crossBelowBull || (this.guerillaTraderBridge.ot_tradeType == 1 && bailLong)))
            {
                ExitLong();
            }
            else if (this.openPosition && (crossAboveBear || (this.guerillaTraderBridge.ot_tradeType == 2 && bailShort)))
            {
                ExitShort();
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "FastMaPeriod", Order = 1, GroupName = "Parameters")]
        public int FastMaPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SlowMaPeriod", Order = 2, GroupName = "Parameters")]
        public int SlowMaPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "TickRangePeriod", Order = 3, GroupName = "Parameters")]
        public int TickRangePeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "MinMaDiff", Order = 4, GroupName = "Parameters")]
        public double MinMaDiff
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "MaxMaDiff", Order = 5, GroupName = "Parameters")]
        public double MaxMaDiff
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "MinTickRange", Order = 6, GroupName = "Parameters")]
        public double MinTickRange
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "MaxTickRange", Order = 7, GroupName = "Parameters")]
        public double MaxTickRange
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "GoLong", Order = 8, GroupName = "Parameters")]
        public bool GoLong
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "GoShort", Order = 9, GroupName = "Parameters")]
        public bool GoShort
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "FireAlerts", Order = 10, GroupName = "Parameters")]
        public bool FireAlerts
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "DiffXX", Order = 11, GroupName = "Parameters")]
        public int DiffXX
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "ProfitTarget", Order = 12, GroupName = "Parameters")]
        public double ProfitTarget
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "StopLoss", Order = 13, GroupName = "Parameters")]
        public double StopLoss
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "TradingAccountId", Order = 14, GroupName = "Parameters")]
        public int TradingAccountId
        { get; set; }
				
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "BailPeriod", Order = 15, GroupName = "Parameters")]
        public int BailPeriod
        { get; set; }
			
        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "GoPeriod", Order = 16, GroupName = "Parameters")]
        public int GoPeriod
        { get; set; }
			
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "RangePeriod", Order = 17, GroupName = "Parameters")]
        public int RangePeriod
        { get; set; }
			
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MinRange", Order = 18, GroupName = "Parameters")]
        public double MinRange
        { get; set; }
        #endregion
    }
}
