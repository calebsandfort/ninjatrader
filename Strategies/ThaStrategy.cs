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
using System.Net.Http;
using System.Globalization;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class ThaStrategy : Strategy
	{
        #region Props
        private Brush BullishBrush = Brushes.DarkBlue;
        private Brush BearishBrush = Brushes.HotPink;

        private Series<double> diffSeries;
        private Series<double> highLowSeries;

        private EMA fastMaIndicator;
        private EMA slowMaIndicator;

        private double tickRange; 
		private bool madeTrade = false;
        private EnhancedADX adxIndicator;
        #endregion

        #region OrderTracking
        #region Enums
        private enum TradeTriggers
        {
            None,
            Signals,
            Support,
            Resistance,
            BullishBreakout,
            BearishBreakout,
            Contrarian,
            Crossover
        }

        private enum TrendTypes
        {
            None,
            Bearish,
            Neutral,
            Bullish
        }

        private enum TradeTypes
        {
            None,
            LongFuture,
            ShortFuture,
            CoveredCall,
            BullPutSpread
        }
        #endregion

        #region Props
        DateTime ot_entryTimestamp;
        DateTime ot_exitTimestamp;
        double ot_diff;
        double ot_tickRange;
        double ot_entryPrice;
        double ot_exitPrice;
        int ot_size;
        int ot_tradeType;
        int ot_trigger;
        int ot_trend;
        double ot_profitLoss;
        double ot_adjProfitLoss;
        double ot_profitLossPerContract;
        int ot_marketId;
        double ot_commissions;

        int ot_diffXX;
        double ot_diffXDiff;
        double ot_diffXSlope;
        double ot_diffXChange;
        double ot_adx;
        #endregion

        #region ResetOt
        private void ResetOt()
        {
            this.ot_entryTimestamp = DateTime.MinValue;
            this.ot_exitTimestamp = DateTime.MinValue;
            this.ot_diff = 0.0;
            this.ot_tickRange = 0.0;
            this.ot_entryPrice = 0.0;
            this.ot_exitPrice = 0.0;
            this.ot_size = 0;
            this.ot_tradeType = (int)TradeTypes.None;
            this.ot_trigger = (int)TradeTriggers.None;
            this.ot_trend = (int)TrendTypes.None;
            this.ot_profitLoss = 0.0;
            this.ot_adjProfitLoss = 0.0;
            this.ot_profitLossPerContract = 0.0;

            this.ot_marketId = 0;
            this.ot_commissions = 0;

            this.ot_diffXX = 0;
            this.ot_diffXDiff = 0.0;
            this.ot_diffXSlope = 0.0;
            this.ot_diffXChange = 0.0;
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

                if (execution.Order.Name.ToLower() == "buy" || execution.Order.Name.ToLower() == "sell short")
                {
					if(State == State.Realtime){
						madeTrade = true;	
					}
					
                    this.ResetOt();

                    this.ot_entryTimestamp = execution.Order.Time;
                    this.ot_tickRange = tickRange;
                    this.ot_entryPrice = execution.Order.AverageFillPrice;
                    this.ot_size = execution.Order.Quantity;

                    this.ot_tradeType = execution.Order.Name == "Buy" ? (int)TradeTypes.LongFuture : (int)TradeTypes.ShortFuture;
                    this.ot_trigger = Contrarian ? (int)TradeTriggers.Contrarian : (int)TradeTriggers.Signals;
                    this.ot_trend = execution.Order.Name == "Buy" ? (int)TrendTypes.Bullish : (int)TrendTypes.Bearish;

                    this.ot_diff = diffSeries[0];
                    this.ot_diffXX = DiffXX;
                    this.ot_diffXDiff = diffSeries[DiffXX] - diffSeries[0];
                    this.ot_diffXSlope = this.ot_diffXDiff / DiffXX;
                    this.ot_diffXChange = this.ot_diffXDiff / diffSeries[0];
                    this.ot_adx = adxIndicator[0];
                }
                else if (execution.Order.Name.ToLower() == "profit target" || execution.Order.Name.ToLower() == "stop loss")
                {
                    this.ot_exitTimestamp = execution.Order.Time;
                    this.ot_exitPrice = execution.Order.AverageFillPrice;

                    if (execution.Order.Instrument.MasterInstrument.Name == "ES")
                    {
                        this.ot_marketId = 1;
                        this.ot_commissions = 4.04;
                    }
                    else if (execution.Order.Instrument.MasterInstrument.Name == "NQ")
                    {
                        this.ot_marketId = 4;
                        this.ot_commissions = 4.04;
                    }
                    else if (execution.Order.Instrument.MasterInstrument.Name == "6E")
                    {
                        this.ot_marketId = 7;
                        this.ot_commissions = 5.32;
                    }

                    if (this.ot_tradeType == (int)TradeTypes.LongFuture)
                    {
                        this.ot_profitLoss = ((this.ot_exitPrice - this.ot_entryPrice) * execution.Order.Instrument.MasterInstrument.PointValue) * this.ot_size;
                    }
                    else if (this.ot_tradeType == (int)TradeTypes.ShortFuture)
                    {
                        this.ot_profitLoss = ((this.ot_entryPrice - this.ot_exitPrice) * execution.Order.Instrument.MasterInstrument.PointValue) * this.ot_size;
                    }

                    this.ot_adjProfitLoss = this.ot_profitLoss - this.ot_commissions;
                    this.ot_profitLossPerContract = this.ot_adjProfitLoss / this.ot_size;

                    if (State == State.Realtime && this.TradingAccountId > 0)
                    {
                        SaveTradeFromNt(this.ot_marketId, this.ot_tradeType, this.ot_trigger, this.ot_trend, this.ot_diff, this.ot_diffXX, this.ot_diffXDiff, this.ot_diffXSlope, this.ot_diffXChange,
                            this.ot_tickRange, this.ot_entryTimestamp, this.ot_entryPrice, this.ot_exitTimestamp, this.ot_exitPrice, this.ot_commissions, this.ot_profitLoss,
                            this.ot_adjProfitLoss, this.ot_size, this.ot_profitLossPerContract, this.TradingAccountId, this.ot_adx, false);
                    }
                }
            }
        }
        #endregion

        #region RecordTrade
        private static async Task SaveTradeFromNt(int MarketId,
                        int TradeType,
                        int Trigger,
                        int Trend,
                        Double SmaDiff,
                        int DiffXX,
                        Double DiffXDiff,
                        Double DiffXSlope,
                        Double DiffXChange,
                        Double ATR,
                        DateTime EntryDate,
                        Double EntryPrice,
                        DateTime ExitDate,
                        Double ExitPrice,
                        Double Commissions,
                        Double ProfitLoss,
                        Double AdjProfitLoss,
                        int Size,
                        Double ProfitLossPerContract,
                        int TradingAccountId,
                        Double ADX,
                        bool test)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(test ? "http://dev-csandfort.gwi.com/GuerillaTrader.Web/api/services/app/" : "http://trader.calebinthecloud.com/api/services/app/");
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("MarketId", MarketId.ToString()),
                    new KeyValuePair<string, string>("TradeType", TradeType.ToString()),
                    new KeyValuePair<string, string>("Trigger", Trigger.ToString()),
                    new KeyValuePair<string, string>("Trend", Trend.ToString()),
                    new KeyValuePair<string, string>("SmaDiff", SmaDiff.ToString()),
                    new KeyValuePair<string, string>("DiffXX", DiffXX.ToString()),
                    new KeyValuePair<string, string>("DiffXDiff", DiffXDiff.ToString()),
                    new KeyValuePair<string, string>("DiffXSlope", DiffXSlope.ToString()),
                    new KeyValuePair<string, string>("DiffXChange", DiffXChange.ToString()),
                    new KeyValuePair<string, string>("ATR", ATR.ToString()),
                    new KeyValuePair<string, string>("EntryDate", EntryDate.ToString("g", DateTimeFormatInfo.InvariantInfo)),
                    new KeyValuePair<string, string>("EntryPrice", EntryPrice.ToString()),
                    new KeyValuePair<string, string>("ExitDate", ExitDate.ToString("g", DateTimeFormatInfo.InvariantInfo)),
                    new KeyValuePair<string, string>("ExitPrice", ExitPrice.ToString()),
                    new KeyValuePair<string, string>("Commissions", Commissions.ToString()),
                    new KeyValuePair<string, string>("ProfitLoss", ProfitLoss.ToString()),
                    new KeyValuePair<string, string>("AdjProfitLoss", AdjProfitLoss.ToString()),
                    new KeyValuePair<string, string>("Size", Size.ToString()),
                    new KeyValuePair<string, string>("ProfitLossPerContract", ProfitLossPerContract.ToString()),
                    new KeyValuePair<string, string>("TradingAccountId", TradingAccountId.ToString()),
                    new KeyValuePair<string, string>("ADX", ADX.ToString())
                });

                var result = await client.PostAsync("trade/saveTradeFromNt", content);
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
                Name = "ThaStrategy";
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
                DefaultQuantity = 2;
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
                StopLoss = 4;
                TradingAccountId = 0;
				Contrarian = false;
				StartHour = 6;
                StartMinute = 45;
                EndHour = 12;
                EndMinute = 45;
            }
            else if (State == State.Configure)
            {
                SetProfitTarget(CalculationMode.Ticks, this.Contrarian ? this.StopLoss : this.ProfitTarget);
                SetStopLoss(CalculationMode.Ticks, this.Contrarian ? this.ProfitTarget : this.StopLoss);
                SetOrderQuantity = SetOrderQuantity.DefaultQuantity;

                fastMaIndicator = EMA(FastMaPeriod);
                slowMaIndicator = EMA(SlowMaPeriod);

                slowMaIndicator.Plots[0].Brush = Brushes.White;

                AddChartIndicator(fastMaIndicator);
                AddChartIndicator(slowMaIndicator);
				
				adxIndicator = EnhancedADX(14, 25);
                AddChartIndicator(adxIndicator);
            }
            else if (State == State.DataLoaded)
            {
                diffSeries = new Series<double>(this);
                highLowSeries = new Series<double>(this);

                this.ResetOt();
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
            if (CurrentBar < SlowMaPeriod) return;

            double fastMa = fastMaIndicator[0];
            double slowMa = slowMaIndicator[0];

            diffSeries[0] = fastMa - slowMa;

            bool isGreenOneBack = Close[0] > Open[0];
            bool isGreenTwoBack = Close[1] > Open[1];
            bool isRedOneBack = Close[0] < Open[0];
            bool isRedTwoBack = Close[1] < Open[1];

            highLowSeries[0] = (High[0] - Low[0]) / TickSize;
            tickRange = EMA(highLowSeries, TickRangePeriod)[0];

            if (CurrentBar < (SlowMaPeriod + DiffXX)) return;

            //bool validTickRange = tickRange > MinTickRange && tickRange < MaxTickRange;
			bool validTickRange = true;
			
            Brush trendBrush = fastMa < slowMa ? BearishBrush : BullishBrush;
            fastMaIndicator.PlotBrushes[0][0] = trendBrush;

			DateTime start = Time[0].Date.AddHours(StartHour).AddMinutes(StartMinute);
            DateTime end = Time[0].Date.AddHours(EndHour).AddMinutes(EndMinute);
            bool validTime = (start <= Time[0]) && (end >= Time[0]);
			
            bool buy = !madeTrade && validTime && GoLong && validTickRange && isRedTwoBack && isGreenOneBack && diffSeries[0] > MinMaDiff && diffSeries[0] < MaxMaDiff;

            if (buy)
            {
				if(Contrarian){
					EnterShortLimit(GetCurrentAsk());
					//EnterShortStopMarket(Close[0]);
				}
				else{
                	EnterLongLimit(GetCurrentBid());
					//EnterLongLimit(Close[0]);
				}
            }

            bool sell = !madeTrade && validTime && GoShort && validTickRange && isGreenTwoBack && isRedOneBack && diffSeries[0] < -MinMaDiff && diffSeries[0] > -MaxMaDiff;

            if (sell)
            {
                if(Contrarian){
					EnterLongLimit(GetCurrentBid());
					//EnterLongStopMarket(Close[0]);
				}
				else{
                	EnterShortLimit(GetCurrentAsk());
					//EnterShortLimit(Close[0]);
				}
            }
        } 
        #endregion

        #region Properties
        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastMaPeriod", Order=1, GroupName="Parameters")]
		public int FastMaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowMaPeriod", Order=2, GroupName="Parameters")]
		public int SlowMaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TickRangePeriod", Order=3, GroupName="Parameters")]
		public int TickRangePeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MinMaDiff", Order=4, GroupName="Parameters")]
		public double MinMaDiff
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MaxMaDiff", Order=5, GroupName="Parameters")]
		public double MaxMaDiff
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MinTickRange", Order=6, GroupName="Parameters")]
		public double MinTickRange
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MaxTickRange", Order=7, GroupName="Parameters")]
		public double MaxTickRange
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="GoLong", Order=8, GroupName="Parameters")]
		public bool GoLong
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="GoShort", Order=9, GroupName="Parameters")]
		public bool GoShort
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FireAlerts", Order=10, GroupName="Parameters")]
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
		[Display(Name="Contrarian", Order=15, GroupName="Parameters")]
		public bool Contrarian
		{ get; set; }
		
		[NinjaScriptProperty]
        [Range(0, 23)]
        [Display(Name = "StartHour", Order = 16, GroupName = "Parameters")]
        public int StartHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 45)]
        [Display(Name = "StartMinute", Order = 17, GroupName = "Parameters")]
        public int StartMinute
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 23)]
        [Display(Name = "EndHour", Order = 18, GroupName = "Parameters")]
        public int EndHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, 45)]
        [Display(Name = "EndMinute", Order = 19, GroupName = "Parameters")]
        public int EndMinute
        { get; set; }
        #endregion
    }
}