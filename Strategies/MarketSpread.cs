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
	public class MarketSpread : Strategy
	{
        #region properties
        private DateTime currentDate = Core.Globals.MinDate;
        private Data.SessionIterator sessionIterator;
        private Spread spread;
		private int canTrade = 0;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "MarketSpread";
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
                BarsRequiredToTrade = 1;
                Symbol2 = "RTY 12-18";
				SpreadThreshold = .01;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;
            }
            else if (State == State.Configure)
            {
                sessionIterator = null;

                switch (BarsPeriod.BarsPeriodType)
                {
                    case BarsPeriodType.Day:
                    case BarsPeriodType.Week:
                    case BarsPeriodType.Month:
                    case BarsPeriodType.Year:
                    case BarsPeriodType.Minute:
                    case BarsPeriodType.Second:
                        AddDataSeries(Symbol2, BarsPeriod.BarsPeriodType, BarsPeriod.Value);
                        break;
                    default:
                        break;
                }

                AddDataSeries(Data.BarsPeriodType.Second, 5);
                AddDataSeries(this.Symbol2, Data.BarsPeriodType.Second, 5);

                spread = Spread(BarsArray[0], 1, -1, true, this.Symbol2, BarsPeriod.BarsPeriodType, BarsPeriod.Value);
                AddChartIndicator(spread);
            }
            else if (State == State.DataLoaded)
            {
                sessionIterator = new Data.SessionIterator(BarsArray[0]);
            }
        }
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            if (BarsInProgress == 0)
            {
                if (currentDate != sessionIterator.GetTradingDay(Time[0]))
                {
                    currentDate = sessionIterator.GetTradingDay(Time[0]);
					canTrade += 1;
                    
                }
				
				
            }
			else if (BarsInProgress == 2 && canTrade > 1)
            {
				if (Time[0].Hour == 6 && Time[0].Minute == 35)
                {

                    double spreadChange = (spread[1] - spread[0]) / spread[0];

                    if (spreadChange <= (this.SpreadThreshold * -1))
                    {
                        EnterLong(0, 1, "ENTER_Symbol1");
                        EnterShort(1, 1, "ENTER_Symbol2");
                    }
					else if (spreadChange >= this.SpreadThreshold)
                    {
                        EnterShort(0, 1, "ENTER_Symbol1");
                        EnterLong(1, 1, "ENTER_Symbol2");
                    }
                }
				else if (Time[0].Hour == 8 && Time[0].Minute == 30)
                {
					if(Positions[0].MarketPosition == MarketPosition.Long)
					{
						ExitLong(0, 1, "EXIT_Symbol1", "ENTER_Symbol1");
						ExitShort(1, 1, "EXIT_Symbol2", "ENTER_Symbol2");
					}
					else if(Positions[0].MarketPosition == MarketPosition.Short)
					{
						ExitShort(0, 1, "EXIT_Symbol1", "ENTER_Symbol1");
						ExitLong(1, 1, "EXIT_Symbol2", "ENTER_Symbol2");
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
        [Display(Name = "Symbol2", Description = "Symbol 2; i.e. SPY or ES 03-10\nDefault = Secondary chart instrument", Order = 1, GroupName = "Parameters")]
        public string Symbol2
        { get; set; }
		
        [NinjaScriptProperty]
        [Display(Name = "SpreadThreshold", Description = "Symbol 2; i.e. SPY or ES 03-10\nDefault = Secondary chart instrument", Order = 1, GroupName = "Parameters")]
        public double SpreadThreshold
        { get; set; }
        #endregion
    }
}
