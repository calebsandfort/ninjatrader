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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class Spread : Indicator
	{
		private Series<double> Instrument2Close;
		private double mul1, mul2;
		private bool supportedBarsPeriodType;
		
		private const int Instrument1=0, Instrument2=1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Spread Price of Two Instruments";
				Name										= "Spread";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Qty1										= 1;
				Qty2										= -1;
				UseMultiplier								= true;
				Symbol2										= string.Empty;
				supportedBarsPeriodType						= false;
				AddPlot(Brushes.ForestGreen, "Spread");
			}
			else if (State == State.Configure)
			{
				switch (this.BarsPeriodTypeProp)
				{
					case BarsPeriodType.Day:
					case BarsPeriodType.Week:
					case BarsPeriodType.Month:
					case BarsPeriodType.Year:
					case BarsPeriodType.Minute:
					case BarsPeriodType.Second:
						AddDataSeries(Symbol2, this.BarsPeriodTypeProp, this.BarsPeriodValueProp);
						supportedBarsPeriodType = true;
						break;
					default:
						break;						
				}
			}
			else if (State == State.DataLoaded)
			{				
				if (!supportedBarsPeriodType)
				{
					throw new ArgumentException("Input series Period must be time-based (Minute,Day,Week,Month,Year,or Second).");
					SetState(State.Terminated);
				}
				Instrument2Close = new Series<double>(this);
				
				if (UseMultiplier)
				{
					mul1 = Instruments[Instrument1].MasterInstrument.PointValue * Qty1;
					mul2 = Instruments[Instrument2].MasterInstrument.PointValue * Qty2;
				}
				else
				{
					mul1 = Qty1;
					mul2 = Qty2;
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 1 || CurrentBars[1] < 1)
				return;
			Instrument2Close[0] = Closes[1][0];
			
			Value[0] = Closes[Instrument1][0] * mul1 + Instrument2Close[0] * mul2;
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SpreadPlot
		{
			get { return Values[0]; }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Qty1", Description="Quantity 1", Order=1, GroupName="Parameters")]
		public double Qty1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Qty2", Description="Quantity 2", Order=2, GroupName="Parameters")]
		public double Qty2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseMultiplier", Description="True: Use Price * Contract Multiplier\nFalse: Use Price", Order=3, GroupName="Parameters")]
		public bool UseMultiplier
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Symbol2", Description="Symbol 2; i.e. SPY or ES 03-10\nDefault = Secondary chart instrument", Order=4, GroupName="Parameters")]
		public string Symbol2
		{ get; set; }

        [NinjaScriptProperty]
        [Display(Name = "BarsPeriodTypeProp", Description = "BarsPeriodTypeProp", Order = 5, GroupName = "Parameters")]
        public BarsPeriodType BarsPeriodTypeProp
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "BarsPeriodValueProp", Description = "BarsPeriodValue", Order = 6, GroupName = "Parameters")]
        public int BarsPeriodValueProp
        { get; set; }
        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Spread[] cacheSpread;
		public Spread Spread(double qty1, double qty2, bool useMultiplier, string symbol2, BarsPeriodType barsPeriodTypeProp, int barsPeriodValueProp)
		{
			return Spread(Input, qty1, qty2, useMultiplier, symbol2, barsPeriodTypeProp, barsPeriodValueProp);
		}

		public Spread Spread(ISeries<double> input, double qty1, double qty2, bool useMultiplier, string symbol2, BarsPeriodType barsPeriodTypeProp, int barsPeriodValueProp)
		{
			if (cacheSpread != null)
				for (int idx = 0; idx < cacheSpread.Length; idx++)
					if (cacheSpread[idx] != null && cacheSpread[idx].Qty1 == qty1 && cacheSpread[idx].Qty2 == qty2 && cacheSpread[idx].UseMultiplier == useMultiplier && cacheSpread[idx].Symbol2 == symbol2 && cacheSpread[idx].BarsPeriodTypeProp == barsPeriodTypeProp && cacheSpread[idx].BarsPeriodValueProp == barsPeriodValueProp && cacheSpread[idx].EqualsInput(input))
						return cacheSpread[idx];
			return CacheIndicator<Spread>(new Spread(){ Qty1 = qty1, Qty2 = qty2, UseMultiplier = useMultiplier, Symbol2 = symbol2, BarsPeriodTypeProp = barsPeriodTypeProp, BarsPeriodValueProp = barsPeriodValueProp }, input, ref cacheSpread);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Spread Spread(double qty1, double qty2, bool useMultiplier, string symbol2, BarsPeriodType barsPeriodTypeProp, int barsPeriodValueProp)
		{
			return indicator.Spread(Input, qty1, qty2, useMultiplier, symbol2, barsPeriodTypeProp, barsPeriodValueProp);
		}

		public Indicators.Spread Spread(ISeries<double> input , double qty1, double qty2, bool useMultiplier, string symbol2, BarsPeriodType barsPeriodTypeProp, int barsPeriodValueProp)
		{
			return indicator.Spread(input, qty1, qty2, useMultiplier, symbol2, barsPeriodTypeProp, barsPeriodValueProp);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Spread Spread(double qty1, double qty2, bool useMultiplier, string symbol2, BarsPeriodType barsPeriodTypeProp, int barsPeriodValueProp)
		{
			return indicator.Spread(Input, qty1, qty2, useMultiplier, symbol2, barsPeriodTypeProp, barsPeriodValueProp);
		}

		public Indicators.Spread Spread(ISeries<double> input , double qty1, double qty2, bool useMultiplier, string symbol2, BarsPeriodType barsPeriodTypeProp, int barsPeriodValueProp)
		{
			return indicator.Spread(input, qty1, qty2, useMultiplier, symbol2, barsPeriodTypeProp, barsPeriodValueProp);
		}
	}
}

#endregion
