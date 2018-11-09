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
	public class FirstHourScalp : Strategy
	{
        #region Plumbing
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";
        private GuerillaStickIndicator bullishHalfBarIndicator;
        private GuerillaStickIndicator bearishHalfBarIndicator;
        private GuerillaStickIndicator greenBarIndicator;
        private GuerillaStickIndicator redBarIndicator;
        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "FirstHourScalp";
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
				StopTicks					= 30;
				TargetTicks					= 7;
			}
			else if (State == State.Configure)
			{
                SetProfitTarget(CalculationMode.Ticks, this.TargetTicks);
                SetStopLoss(CalculationMode.Ticks, this.StopTicks);

                //AddDataSeries(Data.BarsPeriodType.Second, 1);

                bullishHalfBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.BullishHalfBar, false, false, false, 0);
                AddChartIndicator(bullishHalfBarIndicator);

                bearishHalfBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.BearishHalfBar, false, false, false, 0);
                AddChartIndicator(bearishHalfBarIndicator);

                greenBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.GreenBar, false, false, false, 0);
                AddChartIndicator(greenBarIndicator);

                redBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.RedBar, false, false, false, 0);
                AddChartIndicator(redBarIndicator);
            }
		}

		protected override void OnBarUpdate()
		{
			if(BarsInProgress == 0)
            {
                if (HourMinuteCompare(7, 0))
                {
                    if (greenBarIndicator[0] == 1 && greenBarIndicator[1] == 1)
                    {
                        //PrintValues(Time[0], "bullishHalfBar");
                        EnterShort(10, ENTER_SHORT);
                    }

                    if (redBarIndicator[0] == 1 && redBarIndicator[1] == 1)
                    {
                        //PrintValues(Time[0], "bearishHalfBar");
                        EnterLong(10, ENTER_LONG);
                    }
                }
            }
		}

        #region Utilities
        #region HourMinuteCompare
        private bool HourMinuteCompare(int hour, int minute)
        {
            return Time[0].Hour == hour && Time[0].Minute == minute;
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
		[Range(1, int.MaxValue)]
		[Display(Name="StopTicks", Order=1, GroupName="Parameters")]
		public int StopTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TargetTicks", Order=2, GroupName="Parameters")]
		public int TargetTicks
		{ get; set; }
		#endregion

	}
}
