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
	public class AutoTrendHDemoNT8 : Strategy
	{
		#region Variables
		private AutoTrendHNT8 ATH;
		// Constants 
			int cBreakUp=1;  	int cBreakDown	=-1;	
			int cRising	=1;		int cFalling	=-1;	int cFlat=0;	
			int cLong	=1;		int cShort		=-1;
			bool debug	= false;
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Demonstration of AutoTrendH Indicator.";
				Name										= "AutoTrendHDemoNT8";
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
				Alert										= true;
				ShowHistory									= true;
				LimitHistory								= false;
				LimitHistoricalLookback						= 60;
				Strength									= 15;
				StopProfit									= 0;
				StopShock									= 0;
				StopEven									= 0;
				StopLoss									= 0;
				StopReverse									= 0;
				Quantity									= 1000;
			}
			else if (State == State.Configure)
			{
				DefaultQuantity			= Quantity;
			}
			else if (State == State.DataLoaded)
			{
				ATH = AutoTrendHNT8(Alert, Strength, ShowHistory, LimitHistory, LimitHistoricalLookback, Brushes.Red, Brushes.Green, Brushes.Red, Brushes.Green);
			}
		}

		protected override void OnBarUpdate()
		{
			//PRELOAD
			//I use this as my NT is not set to account for the tick price difference in the JPY's
			if ((State == State.Historical) && (Instrument.FullName == "$USDJPY") || (Instrument.FullName=="$EURJPY"))  
			{
				DefaultQuantity=(int)(Quantity*.01);
			}
			//Preload the AutoTrendH values this bar for later use in the strategy
			int 	trendDirection	= ATH.Direction;		//1=TrendUp, -1=TrendDown, 0=New trend not yet determined
			double 	trendPrice		= ATH.TrendPrice;	//Tick value at rightmost bar of current trend line
			int 	trendSignal		= ATH.Signal[0];		//1=resistance break, -1=support break

			//ENTRYS
			// If trending and price is following trend, enter in direction of trend
			if ((trendDirection==cRising) && (Close[0]>trendPrice))
				EnterLong("TrdLg");
			if ((trendDirection==cFalling) && (Close[0]<trendPrice))
				EnterShort("TrdSh");

			// If price breaks through trend, reverse position.
			if ((trendDirection==cRising) && (Close[0]<trendPrice))
				EnterShort("BrkSh");
			if ((trendDirection==cFalling) && (Close[0]>trendPrice))
				EnterLong( "BrkLg");
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Alert", Description="Sets audible and logs alert if set to true", Order=1, GroupName="NinjaScriptStrategyParameters")]
		public bool Alert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ShowHistory", Description="Saves trendlines of auto-generated Trends", Order=2, GroupName="NinjaScriptStrategyParameters")]
		public bool ShowHistory
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="LimitHistory", Description="Limit Historical Trendlines & Breaks", Order=3, GroupName="Parameters")]
		public bool LimitHistory
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="LimitHistoricalLookback", Description="Historical Lookback Period to Limit Trendlines & Breaks", Order=4, GroupName="Parameters")]
		public int LimitHistoricalLookback
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Strength", Description="Sets the granularity of trend detection (smaller # = finer trend detection", Order=5, GroupName="NinjaScriptStrategyParameters")]
		public int Strength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="StopProfit", Description="Lock in profits after StopProfit pips/ticks", Order=6, GroupName="NinjaScriptStrategyParameters")]
		public int StopProfit
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="StopShock", Description="1 Minute timeframe catastrophic loss stop", Order=7, GroupName="NinjaScriptStrategyParameters")]
		public int StopShock
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="StopEven", Description="Move stop to breakeven after StopEven pips/ticks", Order=8, GroupName="NinjaScriptStrategyParameters")]
		public int StopEven
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="StopLoss", Description="Initial StopLoss on entry", Order=9, GroupName="NinjaScriptStrategyParameters")]
		public int StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="StopReverse", Description="Reverse position after StopReverse pips/ticks", Order=10, GroupName="NinjaScriptStrategyParameters")]
		public int StopReverse
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Quantity", Description="Number of shares/contracts/Lots to buy", Order=11, GroupName="NinjaScriptStrategyParameters")]
		public int Quantity
		{ get; set; }
		#endregion

	}
}
