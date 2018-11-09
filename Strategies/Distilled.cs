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
	public class Distilled : Strategy
    {
        private DateTime currentDate = Core.Globals.MinDate;
        private double opening = 0;
        private Data.SessionIterator sessionIterator;

        private GuerillaStickIndicator fullGreenBarIndicator;
        private GuerillaStickIndicator fullRedBarIndicator;
        private GuerillaStickIndicator halfGreenBarIndicator;
        private GuerillaStickIndicator halfRedBarIndicator;

        private GuerillaStickIndicator fullHammerBarIndicator;
        private GuerillaStickIndicator halfHammerBarIndicator;

        private GuerillaStickIndicator fullShootingStarBarIndicator;
        private GuerillaStickIndicator halfShootingStarBarIndicator;

        private GuerillaStickIndicator fullBullishTrendBarIndicator;
        private GuerillaStickIndicator fullBearishTrendBarIndicator;
        private GuerillaStickIndicator halfBullishTrendBarIndicator;
        private GuerillaStickIndicator halfBearishTrendBarIndicator;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "Distilled";
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
			}
			else if (State == State.Configure)
			{
                sessionIterator = null;

                SetProfitTarget(CalculationMode.Ticks, 6);
                SetStopLoss(CalculationMode.Ticks, 16);

                //AddDataSeries(Data.BarsPeriodType.Second, 450);

            }
            else if (State == State.DataLoaded)
            {
                sessionIterator = new Data.SessionIterator(BarsArray[0]);

                fullGreenBarIndicator = GuerillaStickIndicator(BarsArray[0], GuerillaChartPattern.GreenBar, false, false, false, 0);
                fullRedBarIndicator = GuerillaStickIndicator(BarsArray[0], GuerillaChartPattern.RedBar, false, false, false, 0);

                fullHammerBarIndicator = GuerillaStickIndicator(BarsArray[0], GuerillaChartPattern.Hammer, false, false, false, 0);
                fullShootingStarBarIndicator = GuerillaStickIndicator(BarsArray[0], GuerillaChartPattern.ShootingStar, false, false, false, 0);

                fullBullishTrendBarIndicator = GuerillaStickIndicator(BarsArray[0], GuerillaChartPattern.BullishHalfBar, false, false, false, 0);
                fullBearishTrendBarIndicator = GuerillaStickIndicator(BarsArray[0], GuerillaChartPattern.BearishHalfBar, false, false, false, 0);

                //halfGreenBarIndicator = GuerillaStickIndicator(BarsArray[1], GuerillaChartPattern.GreenBar, false, false, false, 0);
                //halfRedBarIndicator = GuerillaStickIndicator(BarsArray[1], GuerillaChartPattern.RedBar, false, false, false, 0);
            }
		}

        protected override void OnBarUpdate()
        {
            if (BarsInProgress == 0)
            {
                if (currentDate != sessionIterator.GetTradingDay(Time[0]))
                {
                    opening = Open[0];

                    currentDate = sessionIterator.GetTradingDay(Time[0]);
                }

                if(Time[0].Hour >= 7 && Time[0].Minute >= 15 && Position.MarketPosition == MarketPosition.Flat)
                {
                    if(Close[0] > opening
                        && 
                            (fullHammerBarIndicator[0] > 0
                            || fullBullishTrendBarIndicator[0] > 0))
                    {
                        EnterLong();
                    }
                    else if (Close[0] < opening
                        &&
                            (fullShootingStarBarIndicator[0] > 0
                            || fullBearishTrendBarIndicator[0] > 0))
                    {
                        EnterShort();
                    }
                }
            }
            else if (BarsInProgress == 1)
            {

            }
        }

        #region PrintValues
        private void PrintValues(params object[] list)
        {
            Print(String.Join(",", list.Select(x => x.ToString())));
        }
        #endregion
    }
}
