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

#endregion

//This namespace holds MarketAnalyzerColumns in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public class Wave : MarketAnalyzerColumn
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Market Analyzer Column here.";
				Name										= "Wave";
				Calculate									= Calculate.OnPriceChange;
				FormatDecimals = 2;
			}
			else if (State == State.Configure)
			{
			}
		}
		
		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
			{
				CurrentValue = double.MinValue;
				return;
			}

			if (marketDataUpdate.MarketDataType != Data.MarketDataType.Last || marketDataUpdate.Instrument.MarketData.LastClose == null)
				return;
			
			CurrentValue =  Math.Abs((marketDataUpdate.Instrument.MarketData.DailyLow.Price - marketDataUpdate.Instrument.MarketData.DailyHigh.Price)) * marketDataUpdate.Instrument.MasterInstrument.PointValue;
		}
		
		#region Miscellaneous
		public override string Format(double value)
		{
			return Core.Globals.FormatCurrency(value, Cbi.Currency.UsDollar);
		}
		#endregion
	}
}
