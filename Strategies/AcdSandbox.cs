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
	public class AcdSandbox : Strategy
    {
        #region Other Props
        private const String ENTER_LONG = "ENTER_LONG";
        private const String ENTER_SHORT = "ENTER_SHORT";
        private const String EXIT = "EXIT";
        private DateTime currentDate = Core.Globals.MinDate;
        private GuerillaAcdMasterIndicator guerillaAcdMasterIndicator;
        private GuerillaStickIndicator greenBarIndicator;
        private GuerillaStickIndicator redBarIndicator;
        private Data.SessionIterator sessionIterator;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "AcdSandbox";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = false;
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
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration = true;

                ATicks = 28;
                CTicks = 16;
                ProjectHour = 13;
                ProjectMinute = 0;
            }
            else if (State == State.Configure)
            {
                currentDate = Core.Globals.MinDate;
                sessionIterator = null;

                AddDataSeries(Data.BarsPeriodType.Second, 30);
                AddDataSeries(Data.BarsPeriodType.Day, 1);

                guerillaAcdMasterIndicator = GuerillaAcdMasterIndicator(this.ATicks, this.CTicks, this.ProjectHour, this.ProjectMinute);
                AddChartIndicator(guerillaAcdMasterIndicator);

                greenBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.GreenBar, false, false, false, 0);
                AddChartIndicator(greenBarIndicator);

                redBarIndicator = GuerillaStickIndicator(GuerillaChartPattern.RedBar, false, false, false, 0);
                AddChartIndicator(redBarIndicator);
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
            DateTime startDate = new DateTime(2018, 1, 4);

            if (Time[0].Date < startDate)
            {
                return;
            }

            if (BarsInProgress == 0)
            {
                if (currentDate != sessionIterator.GetTradingDay(Time[0]))
                {
                    currentDate = sessionIterator.GetTradingDay(Time[0]);

                    int barPeriod = 1;

                    if (Position.MarketPosition == MarketPosition.Long && CountIf(() => redBarIndicator[0] == 1, barPeriod) == barPeriod)
                    {
                        ExitLong(1, 1, EXIT, ENTER_LONG);
                    }
                    else if (Position.MarketPosition == MarketPosition.Short && CountIf(() => greenBarIndicator[0] == 1, barPeriod) == barPeriod)
                    {
                        ExitShort(1, 1, EXIT, ENTER_SHORT);
                    }
                }
            }
            else if (BarsInProgress == 1)
            {
                if (Time[0].Hour == 13 && Time[0].Minute == 14 && Time[0].Second == 0)
                {
                    bool upDay = guerillaAcdMasterIndicator.MyOpen[0] < Close[0];

                    //PrintValues(Time[0].ToShortDateString(), guerillaAcdMasterIndicator.PivotHigh[0].ToString("C"), guerillaAcdMasterIndicator.PivotLow[0].ToString("C"));

                    if (upDay && Close[0] >= guerillaAcdMasterIndicator.PivotHigh[0])
                    {
                        EnterLong(1, 1, ENTER_LONG);
                    }
                    else if (!upDay && Close[0] <= guerillaAcdMasterIndicator.PivotLow[0])
                    {
                        EnterShort(1, 1, ENTER_SHORT);
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
        [Range(1, int.MaxValue)]
        [Display(Name = "ATicks", Order = 1, GroupName = "Parameters")]
        public int ATicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "CTicks", Order = 10, GroupName = "Parameters")]
        public int CTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ProjectHour", Order = 20, GroupName = "Parameters")]
        public int ProjectHour
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "ProjectMinute", Order = 30, GroupName = "Parameters")]
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
