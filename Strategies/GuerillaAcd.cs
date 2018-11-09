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
	public class GuerillaAcd : Strategy
	{
        private GuerillaAcdMasterIndicator guerillaAcdMasterIndicator;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "GuerillaAcd";
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

                ATicks = 28;
                CTicks = 16;
                ProjectHour = 13;
                ProjectMinute = 0;

                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration	= true;

			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Second, 30);
                AddDataSeries(Data.BarsPeriodType.Day, 1);

                guerillaAcdMasterIndicator = GuerillaAcdMasterIndicator(this.ATicks, this.CTicks, this.ProjectHour, this.ProjectMinute);
                AddChartIndicator(guerillaAcdMasterIndicator);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
		}

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATicks", Order = 1, GroupName = "Parameters")]
        public int ATicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "CTicks", Order = 2, GroupName = "Parameters")]
        public int CTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ProjectHour", Order = 5, GroupName = "Parameters")]
        public int ProjectHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ProjectMinute", Order = 10, GroupName = "Parameters")]
        public int ProjectMinute
        { get; set; }

        [Browsable(false)]
		[XmlIgnore]
		public Series<double> Dummy
		{
			get { return Values[0]; }
		}
		#endregion

	}
}
