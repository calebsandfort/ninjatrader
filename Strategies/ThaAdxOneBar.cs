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
	public class ThaAdxOneBar : Strategy
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
//        private EMA fastMaIndicator;
//        private EMA slowMaIndicator;

        private double tickRange;
        private double totalLimit;
        private double totalStop;
        private bool openPosition = false;
        private bool closing = false;
        private Bridge guerillaTraderBridge = new Bridge();
        private DisableManager disableManager = new DisableManager();
        #endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "ThaAdxOneBar";
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
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
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
				EnterViaLimit = true;
				EnterViaStop = true;
				ExitStopBuffer = 2;
				ExitLimitBuffer = 2;
				Quantity = 2;
				MinTickRange = 8;
//                FastMaPeriod = 5;
//                SlowMaPeriod = 10;
//                MinMaDiff = 0.4;
            }
			else if (State == State.Configure)
            {
                //SetOrderQuantity = SetOrderQuantity.DefaultQuantity;

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
				
				rangeIndicator = MyRange(EnterBarsLength, MinTickRange);
				//AddChartIndicator(rangeIndicator);
				
                disableManager.AddRange(DayOfWeek.Sunday, 12, 45, 24, 0);
                disableManager.AddRange(DayOfWeek.Monday, 12, 45, 24, 0);
                disableManager.AddRange(DayOfWeek.Tuesday, 12, 45, 24, 0);
                disableManager.AddRange(DayOfWeek.Wednesday, 12, 45, 24, 0);
                disableManager.AddRange(DayOfWeek.Thursday, 12, 45, 24, 0);
                disableManager.AddRange(DayOfWeek.Friday, 12, 45, 24, 0);
				
				
//                disableManager.AddRange(DayOfWeek.Monday, 0, 0, 5, 0);
//                disableManager.AddRange(DayOfWeek.Tuesday, 0, 0, 5, 0);
//                disableManager.AddRange(DayOfWeek.Wednesday, 0, 0, 5, 0);
//                disableManager.AddRange(DayOfWeek.Thursday, 0, 0, 5, 0);
//                disableManager.AddRange(DayOfWeek.Friday, 0, 0, 5, 0);
				

                //disableManager.AddRange(DayOfWeek.Monday, 6, 30, 6, 45);
                //disableManager.AddRange(DayOfWeek.Tuesday, 6, 30, 6, 45);
                //disableManager.AddRange(DayOfWeek.Wednesday, 6, 30, 6, 45);
                //disableManager.AddRange(DayOfWeek.Thursday, 6, 30, 6, 45);
                //disableManager.AddRange(DayOfWeek.Friday, 6, 30, 6, 45);

                //				fastMaIndicator = EMA(FastMaPeriod);
                //                slowMaIndicator = EMA(SlowMaPeriod);

                //                slowMaIndicator.Plots[0].Brush = Brushes.White;
                //                slowMaIndicator.Plots[0].Width = 2;

                //                fastMaIndicator.Plots[0].Width = 2;

                //                AddChartIndicator(fastMaIndicator);
                //                AddChartIndicator(slowMaIndicator);
            }
            else if (State == State.DataLoaded)
            {
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

                    this.guerillaTraderBridge.ot_tradeType = execution.Order.Name == ENTER_LONG ? (int)TradeTypes.LongFuture : (int)TradeTypes.ShortFuture;
                    this.guerillaTraderBridge.ot_trigger = (int)TradeTriggers.Crossover;
                    this.guerillaTraderBridge.ot_trend = execution.Order.Name == ENTER_LONG ? (int)TrendTypes.Bullish : (int)TrendTypes.Bearish;

                    this.guerillaTraderBridge.ot_diff = 0;
                    this.guerillaTraderBridge.ot_diffXX = 0;
                    this.guerillaTraderBridge.ot_diffXDiff = 0;
                    this.guerillaTraderBridge.ot_diffXSlope = 0;
                    this.guerillaTraderBridge.ot_diffXChange = 0;
					this.guerillaTraderBridge.ot_adx = adxIndicator[0];

					if(execution.Order.Name == ENTER_LONG){
//						ExitLongStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice - (TickSize * LossExitTicks),
//							EXIT, ENTER_LONG);						
						
//						ExitLongStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice - (TickSize * (LossExitTicks + ExitBuffer)),
//							EXIT, ENTER_LONG);
											
//						ExitLongStopLimit(0, true, Quantity,
//							this.guerillaTraderBridge.ot_entryPrice - (TickSize * (LossExitTicks - ExitBuffer)),
//							this.guerillaTraderBridge.ot_entryPrice - (TickSize * LossExitTicks),
//							EXIT, ENTER_LONG);
					}
					else{
//						ExitShortStopMarket(0, true, Quantity, this.guerillaTraderBridge.ot_entryPrice + (TickSize * LossExitTicks),
//							EXIT, ENTER_SHORT);
						
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

					if(execution.Order.IsLimit) totalLimit += 1;
					else totalStop += 1;
					
					PrintTrade(this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_exitTimestamp, execution.Order.IsLimit ? "Limit" : "Stop",
						totalLimit, totalStop, (totalLimit/(totalLimit + totalStop)).ToString("P"), (totalStop/(totalLimit + totalStop)).ToString("P"));
					
                    if ((State == State.Realtime || (State == State.Historical && RecordHistoricalTrades)) && this.TradingAccountId > 0)
                    {
                        Bridge.SaveTradeFromNt(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, this.guerillaTraderBridge.ot_adx, 0, false, true, false, false, 0, 0);
						
											
						
//                        PrintTrade(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
//                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
//                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, this.guerillaTraderBridge.ot_adx, false);
                    }
                }
            }
        }
		
		private void PrintTrade(params object[] list)
		{
			Print(String.Join(",", list.Select(x => x.ToString())));	
		}
        #endregion
        #endregion
		
		protected override void OnBarUpdate()
		{
			//Print(CurrentBar);
            if (CurrentBar < BarsRequiredToTrade) return;
			
//			double fastMa = fastMaIndicator[0];
//            double slowMa = slowMaIndicator[0];

//            double maDiff = fastMa - slowMa;
//			Brush trendBrush = (maDiff < MinMaDiff && maDiff > -MinMaDiff) ? NeutralBrush : fastMa < slowMa ? BearishBrush : BullishBrush;
//            fastMaIndicator.PlotBrushes[0][0] = trendBrush;
			
			bool adxCrossAbove = CrossAbove(adxIndicator, MinAdx, 1);
            bool adxCrossBelow = CrossBelow(adxIndicator, MinAdx, 1);
			bool adxBullTrend = adxIndicator.DmiPlusPlot[0] > adxIndicator.DmiMinusPlot[0];
			bool adxBearTrend = adxIndicator.DmiPlusPlot[0] < adxIndicator.DmiMinusPlot[0];
			
			int enterPeriodMinusOne = EnterPeriod - 2; 
			bool enterSignal = EnterPeriod < 2 ? false : 
                ((enterAdxSlopeIndicator[0] > EnterSlopeThreshold) &&
                (CountIf(() => adxIndicator[0] > MinAdx, enterPeriodMinusOne) == enterPeriodMinusOne));

            if (adxBullTrend)
            {
                enterSignal = enterSignal && (CountIf(() => Close[0] > Open[0], EnterBarsLength) == EnterBarsLength);
            }
            else if (adxBearTrend)
            {
                enterSignal = enterSignal && (CountIf(() => Close[0] < Open[0], EnterBarsLength) == EnterBarsLength);
            }

            bool exitSlopeSignal = ExitPeriod < 2 ? false : 
                exitAdxSlopeIndicator[0] < ExitSlopeThreshold;

            bool exitBarSignal = false;

            if((!this.Contrarian && this.guerillaTraderBridge.ot_tradeType == 1) || (this.Contrarian && this.guerillaTraderBridge.ot_tradeType == 2))
            {
                exitBarSignal = (CountIf(() => Close[0] < Open[0], ExitBarsLength) == ExitBarsLength);
            }
            else if ((!this.Contrarian && this.guerillaTraderBridge.ot_tradeType == 2) || (this.Contrarian && this.guerillaTraderBridge.ot_tradeType == 1))
            {
                exitBarSignal = (CountIf(() => Close[0] > Open[0], ExitBarsLength) == ExitBarsLength);
            }

			//exitBarSignal = false;
			
            DateTime start = Time[0].Date.AddHours(StartHour).AddMinutes(StartMinute);
            DateTime end = Time[0].Date.AddHours(EndHour).AddMinutes(EndMinute);
            //bool validTime = !UseBoundaryHours || (start <= Time[0]) && (end >= Time[0]);
            bool validTime = disableManager.IsValidTime(Time[0]);
			
			Double priceOffset = Math.Round(rangeIndicator[0] / 2, MidpointRounding.AwayFromZero) * TickSize;
			
            if (!this.openPosition && validTime && (enterSignal) && adxBullTrend)
            {
				if(!this.Contrarian){
//					if(State == State.Historical){
                		EnterLong(0, Quantity, ENTER_LONG);
//					}
//					else{
//						EnterLong(ENTER_LONG);	
//					}
				}
				else{
					EnterShort(0, Quantity, ENTER_SHORT);	
				}
				
//				if(EnterViaStop){
//					EnterLongStopLimit(0, false, 1, GetCurrentAsk() + priceOffset, GetCurrentBid() + priceOffset, ENTER_LONG);
//				}
				//EnterLong();
            }
            else if (!this.openPosition && validTime && (enterSignal) && adxBearTrend)
            {
				if(!this.Contrarian){
//					if(State == State.Historical){
                		EnterShort(0, Quantity, ENTER_SHORT);
//					}
//					else{
//						EnterShort(ENTER_SHORT);	
//					}
				}
				else {
					EnterLong(0, Quantity, ENTER_LONG);
				}
				
//				if(EnterViaStop){
//					EnterShortStopLimit(0, false, 1, GetCurrentBid() - priceOffset, GetCurrentAsk() - priceOffset, ENTER_SHORT);	
//				}
				//EnterShort();
            }
            else if (this.openPosition && !closing && this.guerillaTraderBridge.ot_tradeType == 1)
            {
				closing = true;
				ExitLong(0, Quantity, EXIT, ENTER_LONG);
            }
            else if (this.openPosition && !closing && this.guerillaTraderBridge.ot_tradeType == 2)
            {
				closing = true;
				ExitShort(0, Quantity, EXIT, ENTER_SHORT);
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
        [Display(Name = "EnterViaLimit", Order = 17, GroupName = "Parameters")]
        public bool EnterViaLimit
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "EnterViaStop", Order = 18, GroupName = "Parameters")]
        public bool EnterViaStop
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
        [Display(Name = "Contrarian", Order = 22, GroupName = "Parameters")]
        public bool Contrarian
        { get; set; }

        //		[NinjaScriptProperty]
        //        [Range(1, int.MaxValue)]
        //        [Display(Name = "FastMaPeriod", Order = 11, GroupName = "Parameters")]
        //        public int FastMaPeriod
        //        { get; set; }

        //        [NinjaScriptProperty]
        //        [Range(1, int.MaxValue)]
        //        [Display(Name = "SlowMaPeriod", Order = 11, GroupName = "Parameters")]
        //        public int SlowMaPeriod
        //        { get; set; }

        //        [NinjaScriptProperty]
        //        [Range(0, double.MaxValue)]
        //        [Display(Name = "MinMaDiff", Order = 11, GroupName = "Parameters")]
        //        public double MinMaDiff
        //        { get; set; }
        #endregion
	}
}
