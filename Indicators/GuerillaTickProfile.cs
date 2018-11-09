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
	public class GuerillaTickProfile : Indicator
	{
        private Series<bool> withinThreshold;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "GuerillaTickProfile";
				Calculate									= Calculate.OnBarClose;
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
                GoColor = Brushes.MediumVioletRed;
                NoGoColor = Brushes.LightGray;
                MinThreshold					= 16;
				MaxThreshold					= 60;
			}
			else if (State == State.Configure)
            {
                AddPlot(new Stroke(this.NoGoColor, 2), PlotStyle.Bar, "ProfilePlot");
                AddLine(Brushes.DarkGray, this.MinThreshold, "MinThresholdLine");
                AddLine(Brushes.DarkGray, this.MaxThreshold, "MaxThresholdLine");

                withinThreshold = new Series<bool>(this);
            }
        }

		protected override void OnBarUpdate()
		{
            this.ProfilePlot[0] = (High[0] - Low[0]) / TickSize;
            this.WithinThreshold[0] = this.ProfilePlot[0] >= this.MinThreshold && this.ProfilePlot[0] <= this.MaxThreshold;

            if (this.WithinThreshold[0])
            {
                this.PlotBrushes[0][0] = this.GoColor;
            }
		}

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="GoColor", Order=1, GroupName="Parameters")]
		public Brush GoColor
		{ get; set; }

		[Browsable(false)]
		public string GoColorSerializable
		{
			get { return Serialize.BrushToString(GoColor); }
			set { GoColor = Serialize.StringToBrush(value); }
		}

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "NoGoColor", Order = 2, GroupName = "Parameters")]
        public Brush NoGoColor
        { get; set; }

        [Browsable(false)]
        public string NoGoColorSerializable
        {
            get { return Serialize.BrushToString(NoGoColor); }
            set { NoGoColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="MinThreshold", Order=4, GroupName="Parameters")]
		public int MinThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="MaxThreshold", Order=4, GroupName="Parameters")]
		public int MaxThreshold
		{ get; set; }


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ProfilePlot
		{
			get { return Values[0]; }
		}

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> WithinThreshold
        {
            get { return this.withinThreshold; }
        }
        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GuerillaTickProfile[] cacheGuerillaTickProfile;
		public GuerillaTickProfile GuerillaTickProfile(Brush goColor, Brush noGoColor, int minThreshold, int maxThreshold)
		{
			return GuerillaTickProfile(Input, goColor, noGoColor, minThreshold, maxThreshold);
		}

		public GuerillaTickProfile GuerillaTickProfile(ISeries<double> input, Brush goColor, Brush noGoColor, int minThreshold, int maxThreshold)
		{
			if (cacheGuerillaTickProfile != null)
				for (int idx = 0; idx < cacheGuerillaTickProfile.Length; idx++)
					if (cacheGuerillaTickProfile[idx] != null && cacheGuerillaTickProfile[idx].GoColor == goColor && cacheGuerillaTickProfile[idx].NoGoColor == noGoColor && cacheGuerillaTickProfile[idx].MinThreshold == minThreshold && cacheGuerillaTickProfile[idx].MaxThreshold == maxThreshold && cacheGuerillaTickProfile[idx].EqualsInput(input))
						return cacheGuerillaTickProfile[idx];
			return CacheIndicator<GuerillaTickProfile>(new GuerillaTickProfile(){ GoColor = goColor, NoGoColor = noGoColor, MinThreshold = minThreshold, MaxThreshold = maxThreshold }, input, ref cacheGuerillaTickProfile);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GuerillaTickProfile GuerillaTickProfile(Brush goColor, Brush noGoColor, int minThreshold, int maxThreshold)
		{
			return indicator.GuerillaTickProfile(Input, goColor, noGoColor, minThreshold, maxThreshold);
		}

		public Indicators.GuerillaTickProfile GuerillaTickProfile(ISeries<double> input , Brush goColor, Brush noGoColor, int minThreshold, int maxThreshold)
		{
			return indicator.GuerillaTickProfile(input, goColor, noGoColor, minThreshold, maxThreshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GuerillaTickProfile GuerillaTickProfile(Brush goColor, Brush noGoColor, int minThreshold, int maxThreshold)
		{
			return indicator.GuerillaTickProfile(Input, goColor, noGoColor, minThreshold, maxThreshold);
		}

		public Indicators.GuerillaTickProfile GuerillaTickProfile(ISeries<double> input , Brush goColor, Brush noGoColor, int minThreshold, int maxThreshold)
		{
			return indicator.GuerillaTickProfile(input, goColor, noGoColor, minThreshold, maxThreshold);
		}
	}
}

#endregion
