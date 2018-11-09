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
	public class ThaAdxRangeExit : Strategy
	{
		#region Props
		private String ENTER_LONG = "ENTER_LONG";
		private String ENTER_SHORT = "ENTER_SHORT";
		private String EXIT = "EXIT";
		
        private Brush BullishBrush = Brushes.DarkBlue;
        private Brush BearishBrush = Brushes.HotPink;
        private Brush NeutralBrush = Brushes.Gray;
		
        private EnhancedADX adxIndicator;
        private AdxSlope enterAdxSlopeIndicator;
        private MyRange rangeIndicator;
        private AdxDiff adxDiffIndicator;
        private SmaSlope smaSlopeIndicator;

        private double tickRange;
        private double totalLimit;
        private double totalStop;
        private bool openPosition = false;
        private bool closing = false;
        private Bridge guerillaTraderBridge = new Bridge();
        private DisableManager disableManager = new DisableManager();
        private int dynamicQuantity = 1;
        private double startingCash = 15000;
        #endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"ThaAdxRangeExit";
				Name										= "ThaAdxRangeExit";
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
				AdxPeriod = 14;
				MinAdx = 25;
				UseBoundaryHours = true;
                StartHour = 7;
                StartMinute = 30;
                EndHour = 12;
                EndMinute = 30;
				EnterPeriod = 3;
				EnterSlopeThreshold = 20;
                RecordHistoricalTrades = false;
				EnterBarsLength = 2;
				ExitStopBuffer = 2;
				ExitLimitBuffer = 2;
				Quantity = 1;
                UseDynamicQuantity = false;
                MaxDynamicQuantity = 5;
				Contrarian = false;
                TickThreshold = 24;
                LimitMultiplier = .35;
                StopMultiplier = .7;
                SmaSlopePeriod = 14;
                SmaSlopeSlopePeriod = 14;
                SmaSlopePositiveThreshold = 20;
                SmaSlopeNegativeThreshold = -20;
				SmaSlopeInverse = false;
            }
			else if (State == State.Configure)
            {
                //SetProfitTarget(CalculationMode.Ticks, this.ExitLimitBuffer);
                //SetStopLoss(CalculationMode.Ticks, this.ExitStopBuffer);
                //SetOrderQuantity = SetOrderQuantity.DefaultQuantity;

				adxIndicator = EnhancedADX(AdxPeriod, MinAdx);
                AddChartIndicator(adxIndicator);

                if (EnterPeriod > 1)
                {
                    enterAdxSlopeIndicator = AdxSlope(AdxPeriod, EnterPeriod, EnterSlopeThreshold);
                    //AddChartIndicator(enterAdxSlopeIndicator);
                }
				
				rangeIndicator = MyRange(5, 100);
				//AddChartIndicator(rangeIndicator);
				
				adxDiffIndicator = AdxDiff(14, 25);
                //AddChartIndicator(adxDiffIndicator);

                smaSlopeIndicator = SmaSlope(SmaSlopePeriod, SmaSlopeSlopePeriod, SmaSlopePositiveThreshold, SmaSlopeNegativeThreshold, SmaSlopeInverse);
                AddChartIndicator(smaSlopeIndicator);

                disableManager.AddRange(DayOfWeek.Sunday, 13, 30, 24, 0);
                disableManager.AddRange(DayOfWeek.Monday, 13, 30, 24, 0);
                disableManager.AddRange(DayOfWeek.Tuesday, 13, 30, 24, 0);
                disableManager.AddRange(DayOfWeek.Wednesday, 13, 30, 24, 0);
                disableManager.AddRange(DayOfWeek.Thursday, 13, 30, 24, 0);
                disableManager.AddRange(DayOfWeek.Friday, 13, 30, 24, 0);
				
				
                disableManager.AddRange(DayOfWeek.Monday, 0, 0, 5, 0);
                disableManager.AddRange(DayOfWeek.Tuesday, 0, 0, 5, 0);
                disableManager.AddRange(DayOfWeek.Wednesday, 0, 0, 5, 0);
                disableManager.AddRange(DayOfWeek.Thursday, 0, 0, 5, 0);
                disableManager.AddRange(DayOfWeek.Friday, 0, 0, 5, 0);
				
				//PrintTrade("Day", "Fifteen", "Adx", "AdxSlope", "Direction", "TickRange", "AdxDiff", "Win");
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
                    this.guerillaTraderBridge.ot_tickRange = rangeIndicator[0];
                    this.guerillaTraderBridge.ot_entryPrice = execution.Order.AverageFillPrice;
                    this.guerillaTraderBridge.ot_size = dynamicQuantity;

                    this.guerillaTraderBridge.ot_tradeType = execution.Order.Name == ENTER_LONG ? (int)TradeTypes.LongFuture : (int)TradeTypes.ShortFuture;
                    this.guerillaTraderBridge.ot_trigger = (int)TradeTriggers.Crossover;
                    this.guerillaTraderBridge.ot_trend = execution.Order.Name == ENTER_LONG ? (int)TrendTypes.Bullish : (int)TrendTypes.Bearish;

                    this.guerillaTraderBridge.ot_diff = 0;
                    this.guerillaTraderBridge.ot_diffXX = 0;
                    this.guerillaTraderBridge.ot_diffXDiff = 0;
                    this.guerillaTraderBridge.ot_diffXSlope = enterAdxSlopeIndicator[0];
                    this.guerillaTraderBridge.ot_diffXChange = 0;
					this.guerillaTraderBridge.ot_adx = adxIndicator[0];
					this.guerillaTraderBridge.ot_adxDiff = adxDiffIndicator[0];

                    int limitTicks = (int)(rangeIndicator[0] * (Contrarian ? StopMultiplier : LimitMultiplier));
                    int stopTicks = (int)(rangeIndicator[0] * (Contrarian ? LimitMultiplier : StopMultiplier));

                    //PrintTrade("Limit Ticks: " + limitTicks, "Stop Ticks: " + stopTicks);

                    if (execution.Order.Name == ENTER_LONG)
                    {
                        ExitLongLimit(0, true, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_entryPrice + (TickSize * limitTicks),
                            EXIT, ENTER_LONG);

                        ExitLongStopMarket(0, true, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_entryPrice - (TickSize * stopTicks),
                            EXIT, ENTER_LONG);
                    }
                    else
                    {
                        ExitShortLimit(0, true, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_entryPrice - (TickSize * limitTicks),
                            EXIT, ENTER_SHORT);

                        ExitShortStopMarket(0, true, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_entryPrice + (TickSize * stopTicks),
                            EXIT, ENTER_SHORT);
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
                    startingCash += this.guerillaTraderBridge.ot_adjProfitLoss;
                    this.guerillaTraderBridge.ot_profitLossPerContract = this.guerillaTraderBridge.ot_adjProfitLoss / this.guerillaTraderBridge.ot_size;

                    this.openPosition = false;
					this.closing = false;

					if(execution.Order.IsLimit) totalLimit += 1;
					else totalStop += 1;

                    //PrintTrade("Day", "Fifteen", "Adx", "AdxSlope", "Direction", "TickRange", "AdxDiff", "Win");

      //              PrintTrade(
      //                  String.Format("\"{0}\"", this.guerillaTraderBridge.ot_entryTimestamp.DayOfWeek),
      //                  this.GetFifteen(this.guerillaTraderBridge.ot_entryTimestamp),
      //                  this.guerillaTraderBridge.ot_adx.ToString("N0"),
      //                  this.guerillaTraderBridge.ot_diffXSlope.ToString("N0"),
      //                  String.Format("\"{0}\"", ((TradeTypes)this.guerillaTraderBridge.ot_tradeType).ToString().Replace("Future", String.Empty)),
      //                  this.guerillaTraderBridge.ot_tickRange.ToString("N0"),
      //                  this.guerillaTraderBridge.ot_adxDiff.ToString("N0"),
						//this.guerillaTraderBridge.ot_adjProfitLoss > 0);
					
                    if ((State == State.Realtime || (State == State.Historical && RecordHistoricalTrades)) && this.TradingAccountId > 0)
                    {
                        Bridge.SaveTradeFromNt(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, this.guerillaTraderBridge.ot_adx, this.guerillaTraderBridge.ot_adxDiff, false, true, false, false, 0, 0);
						
//                        PrintTrade(this.guerillaTraderBridge.ot_marketId, this.guerillaTraderBridge.ot_tradeType, this.guerillaTraderBridge.ot_trigger, this.guerillaTraderBridge.ot_trend, this.guerillaTraderBridge.ot_diff, this.guerillaTraderBridge.ot_diffXX, this.guerillaTraderBridge.ot_diffXDiff, this.guerillaTraderBridge.ot_diffXSlope, this.guerillaTraderBridge.ot_diffXChange,
//                            this.guerillaTraderBridge.ot_tickRange, this.guerillaTraderBridge.ot_entryTimestamp, this.guerillaTraderBridge.ot_entryPrice, this.guerillaTraderBridge.ot_exitTimestamp, this.guerillaTraderBridge.ot_exitPrice, this.guerillaTraderBridge.ot_commissions, this.guerillaTraderBridge.ot_profitLoss,
//                            this.guerillaTraderBridge.ot_adjProfitLoss, this.guerillaTraderBridge.ot_size, this.guerillaTraderBridge.ot_profitLossPerContract, this.TradingAccountId, this.guerillaTraderBridge.ot_adx, false);
                    }
                }
            }
        }
		
        private int GetFifteen(DateTime timeStamp)
        {
            int fifteen = 0;
            int startMinutes = (StartHour * 60) + StartMinute;
            int currentMinutes = (timeStamp.Hour * 60) + timeStamp.Minute;
            int elapsedMinutes = currentMinutes - startMinutes;
            fifteen = (elapsedMinutes - (elapsedMinutes % 15)) / 15;

            return fifteen;
        }

		private void PrintTrade(params object[] list)
		{
			Print(String.Join(",", list.Select(x => x.ToString())));	
		}
        #endregion
        #endregion

        #region SetDynamicQuantity
        private void SetDynamicQuantity()
        {
            if (UseDynamicQuantity)
            {
                Double initialMargin = 0;
                if (Instrument.MasterInstrument.Name == "NQ")
                {
                    initialMargin = 4300;
                }

                //Double cash = Account.GetAccountItem(AccountItem.NetLiquidation, Currency.UsDollar).Value;

                dynamicQuantity = (int)(startingCash / (3 * initialMargin));
                dynamicQuantity = Math.Min(dynamicQuantity, MaxDynamicQuantity);
            }
        }
        #endregion

        protected override void OnBarUpdate()
		{
            if (CurrentBar < BarsRequiredToTrade) return;


            bool adxCrossAbove = CrossAbove(adxIndicator, MinAdx, 1);
            bool adxCrossBelow = CrossBelow(adxIndicator, MinAdx, 1);
			bool adxBullTrend = adxIndicator.DmiPlusPlot[0] > adxIndicator.DmiMinusPlot[0];
			bool adxBearTrend = adxIndicator.DmiPlusPlot[0] < adxIndicator.DmiMinusPlot[0];
			
			int enterPeriodMinusOne = EnterPeriod - 2; 
			bool enterSignal = EnterPeriod < 2 ? false : 
                ((enterAdxSlopeIndicator[0] > EnterSlopeThreshold) && smaSlopeIndicator.GoZone && rangeIndicator[0] < TickThreshold &&
                (CountIf(() => adxIndicator[0] > MinAdx, enterPeriodMinusOne) == enterPeriodMinusOne));

            if (adxBullTrend)
            {
                enterSignal = enterSignal && (CountIf(() => Close[0] > Open[0], EnterBarsLength) == EnterBarsLength);
            }
            else if (adxBearTrend)
            {
                enterSignal = enterSignal && (CountIf(() => Close[0] < Open[0], EnterBarsLength) == EnterBarsLength);
            }

            DateTime start = Time[0].Date.AddHours(StartHour).AddMinutes(StartMinute);
            DateTime end = Time[0].Date.AddHours(EndHour).AddMinutes(EndMinute);
            bool validTime = !UseBoundaryHours || (start <= Time[0]) && (end >= Time[0]);
            //bool validTime = disableManager.IsValidTime(Time[0]);
			
			Double priceOffset = Math.Round(rangeIndicator[0] / 2, MidpointRounding.AwayFromZero) * TickSize;
			
            if (!this.openPosition && validTime && (enterSignal) && adxBullTrend)
            {
                //PrintTrade(smaSlopeIndicator[0].ToString("N2"), smaSlopeIndicator.GoZone);
                SetDynamicQuantity();
				if(Contrarian){
					EnterShort(dynamicQuantity, ENTER_SHORT);
				}
				else{
                	EnterLong(dynamicQuantity, ENTER_LONG);
				}
            }
            else if (!this.openPosition && validTime && (enterSignal) && adxBearTrend)
            {
                //PrintTrade(smaSlopeIndicator[0].ToString("N2"), smaSlopeIndicator.GoZone);
                SetDynamicQuantity();
				if(Contrarian){
					EnterLong(dynamicQuantity, ENTER_LONG);
				}
				else{
                	EnterShort(dynamicQuantity, ENTER_SHORT);
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
        [Display(Name = "EnterBarsLength", Order = 11, GroupName = "Parameters")]
        public int EnterBarsLength
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "UseBoundaryHours", Order = 12, GroupName = "Parameters")]
        public bool UseBoundaryHours
        { get; set; }
		
        [NinjaScriptProperty]
        [Range(0, 23)]
        [Display(Name = "StartHour", Order = 13, GroupName = "Parameters")]
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
        [Display(Name = "UseDynamicQuantity", Order = 21, GroupName = "Parameters")]
        public bool UseDynamicQuantity
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MaxDynamicQuantity", Order = 22, GroupName = "Parameters")]
        public int MaxDynamicQuantity
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Contrarian", Order = 23, GroupName = "Parameters")]
        public bool Contrarian
        { get; set; }
		
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "TickThreshold", Order = 24, GroupName = "Parameters")]
        public int TickThreshold
        { get; set; }

        [NinjaScriptProperty]
        [Range(int.MinValue, int.MaxValue)]
        [Display(Name = "LimitMultiplier", Order = 25, GroupName = "Parameters")]
        public double LimitMultiplier
        { get; set; }

        [NinjaScriptProperty]
        [Range(int.MinValue, int.MaxValue)]
        [Display(Name = "StopMultiplier", Order = 26, GroupName = "Parameters")]
        public double StopMultiplier
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SmaSlopePeriod", GroupName = "NinjaScriptParameters", Order = 27)]
        public int SmaSlopePeriod
        { get; set; }

        [Range(2, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SmaSlopeSlopePeriod", GroupName = "NinjaScriptParameters", Order = 28)]
        public int SmaSlopeSlopePeriod
        { get; set; }

        [Range(double.MinValue, double.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SmaSlopePositiveThreshold", GroupName = "NinjaScriptParameters", Order = 29)]
        public double SmaSlopePositiveThreshold
        { get; set; }

        [Range(double.MinValue, double.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SmaSlopeNegativeThreshold", GroupName = "NinjaScriptParameters", Order = 30)]
        public double SmaSlopeNegativeThreshold
        { get; set; }

        [Display(ResourceType = typeof(Custom.Resource), Name = "SmaSlopeInverse", GroupName = "NinjaScriptParameters", Order = 31)]
        public bool SmaSlopeInverse
        { get; set; }
        #endregion
    }
}
