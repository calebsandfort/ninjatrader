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
	public class ThaStrategy : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "ThaStrategy";
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

                FastMaPeriod                                = 10;
                SlowMaPeriod                                = 20;
                TickRangePeriod                             = 15;
                MinMaDiff                                   = 0.4;
                MaxMaDiff                                   = 10;
                MinTickRange                                = 0;
                MaxTickRange                                = 30;
                GoLong                                      = true;
                GoShort                                     = true;
                FireAlerts                                  = true;
            }
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnExecutionUpdate(Cbi.Execution execution, string executionId, double price, int quantity, 
			Cbi.MarketPosition marketPosition, string orderId, DateTime time)
		{
			
		}

		protected override void OnOrderUpdate(Cbi.Order order, double limitPrice, double stopPrice, 
			int quantity, int filled, double averageFillPrice, 
			Cbi.OrderState orderState, DateTime time, Cbi.ErrorCode error, string comment)
		{
			
		}

		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
		}

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
        #endregion
    }
}
