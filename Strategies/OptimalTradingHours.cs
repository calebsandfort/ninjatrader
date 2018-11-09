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
	public class OptimalTradingHours : Strategy
	{
        Dictionary<String, List<double>> volumes = new Dictionary<string, List<double>>();
		int totalBars = 12043;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "OptimalTradingHours";
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
			}
			else if (State == State.Terminated)
			{
                //Print("Time,Avg");
				//Print(Time[0]);
            }
			else if (State == State.Transition)
			{
                //Print("Transition");
            }
		}

		protected override void OnBarUpdate()
		{
			String key = String.Format("{0:h:mm:tt} - {1:h:mm:tt}", Time[0].AddMinutes(-10), Time[0]);

            if (!volumes.ContainsKey(key))
            {
                volumes.Add(key, new List<double>());
            }

            volumes[key].Add(Volume[0]);
			//Print(CurrentBar);
            if (CurrentBar == totalBars)
            {
                Print("Time;Avg");
                foreach(KeyValuePair<String, List<double>> pair in volumes)
                {
                    Print(String.Format("\"{0}\"; {1:N0}", pair.Key, pair.Value.Average()));
                }
            }
		}
	}
}
