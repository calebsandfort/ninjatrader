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
	public class GuerillaStickSimple : Indicator
	{
        #region properties
        private GuerillaChartPattern pattern = GuerillaChartPattern.MorningStar;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "GuerillaStickSimple";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                UpColor = Brushes.Chartreuse;
                DownColor = Brushes.Firebrick;
                Threshold1 = .5;
                Threshold2 = .5;
            }
            else if (State == State.Configure)
            {
                AddPlot(new Stroke(this.UpColor, 2), PlotStyle.Dot, "PatternPlot");
            }
        }
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1) return;

            DateTime debugDate = new DateTime(2018, 9, 25, 7, 30, 0);

            if(debugDate == Time[0])
            {
                int d = 0;
            }

            bool green = Close[0] > Open[0];
            bool red = Open[0] > Close[0];

            double rangeTicks = (High[0] - Low[0]) / TickSize;

            double bodyTicks = 0;
            double upperShadowTicks = 0;
            double lowerShadowTicks = 0;
            if (green)
            {
                bodyTicks = (Close[0] - Open[0]) / TickSize;
                upperShadowTicks = (High[0] - Close[0]) / TickSize;
                lowerShadowTicks = (Open[0] - Low[0]) / TickSize;
            }
            else if (red)
            {
                bodyTicks = (Open[0] - Close[0]) / TickSize;
                upperShadowTicks = (High[0] - Open[0]) / TickSize;
                lowerShadowTicks = (Close[0] - Low[0]) / TickSize;
            }

            double bodyPtg = bodyTicks / rangeTicks;
            double upperShadowPtg = upperShadowTicks / rangeTicks;
            double lowerShadowPtg = lowerShadowTicks / rangeTicks;

            switch (pattern)
            {
                case GuerillaChartPattern.GreenBar:
                    {
                        if (green)
                        {
                            PatternPlot[0] = Math.Min(Low[0], Low[1]);
                            PlotBrushes[0][0] = UpColor;
                        }
                        break;
                    }
                case GuerillaChartPattern.RedBar:
                    {
                        if (red)
                        {
                            PatternPlot[0] = Math.Max(High[0], High[1]);
                            PlotBrushes[0][0] = DownColor;
                        }
                        break;
                    }
                case GuerillaChartPattern.FiftyHammer:
                {
                        if (green
                            //&& upperShadowPtg <= .25
                            && lowerShadowPtg >= (upperShadowPtg * 1.5)
                            && lowerShadowPtg >= .45)
                        {
                            PatternPlot[0] = Math.Min(Low[0], Low[1]);
                            PlotBrushes[0][0] = UpColor;
                        }
                        else if(red
                            //&& upperShadowPtg <= .25
                            //&& bodyPtg >= (upperShadowPtg * .5)
                            && lowerShadowPtg >= .55)
                        {
                            PatternPlot[0] = Math.Max(High[0], High[1]);
                            PlotBrushes[0][0] = DownColor;
                        }

                    break;
                }
                case GuerillaChartPattern.FiftyMan:
                    {
                        if (green
                            //&& lowerShadowPtg <= .25
                            && upperShadowPtg >= (lowerShadowPtg * 1.5)
                            && upperShadowPtg >= .55)
                        {
                            PatternPlot[0] = Math.Min(Low[0], Low[1]);
                            PlotBrushes[0][0] = UpColor;
                        }
                        else if (red
                            //&& lowerShadowPtg <= .25
                            && bodyPtg >= (lowerShadowPtg * .5)
                            && upperShadowPtg >= .45)
                        {
                            PatternPlot[0] = Math.Max(High[0], High[1]);
                            PlotBrushes[0][0] = DownColor;
                        }

                        break;
                    }
                case GuerillaChartPattern.FiftyBar:
                    {
                        if (green
                            //&& lowerShadowPtg <= .25
                            && upperShadowPtg <= lowerShadowPtg
                            && bodyPtg >= .45)
                        {
                            PatternPlot[0] = Math.Min(Low[0], Low[1]);
                            PlotBrushes[0][0] = UpColor;
                        }
                        else if (red
                            //&& lowerShadowPtg <= .25
                            && lowerShadowPtg <= upperShadowPtg
                            && bodyPtg >= .45)
                        {
                            PatternPlot[0] = Math.Max(High[0], High[1]);
                            PlotBrushes[0][0] = DownColor;
                        }

                        break;
                    }
                case GuerillaChartPattern.BigBar:
                    {
                        if (green
                            && bodyPtg >= this.Threshold1)
                        {
                            PatternPlot[0] = Math.Min(Low[0], Low[1]);
                            PlotBrushes[0][0] = UpColor;
                        }
                        else if (red
                            && bodyPtg >= this.Threshold1)
                        {
                            PatternPlot[0] = Math.Max(High[0], High[1]);
                            PlotBrushes[0][0] = DownColor;
                        }
                        break;
                    }
            }
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="UpColor", Order=1, GroupName="Parameters")]
		public Brush UpColor
		{ get; set; }

		[Browsable(false)]
		public string UpColorSerializable
		{
			get { return Serialize.BrushToString(UpColor); }
			set { UpColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="DownColor", Order=2, GroupName="Parameters")]
		public Brush DownColor
		{ get; set; }

		[Browsable(false)]
		public string DownColorSerializable
		{
			get { return Serialize.BrushToString(DownColor); }
			set { DownColor = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PatternPlot
		{
			get { return Values[0]; }
		}

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SelectPattern", Description = "Choose a pattern to detect", GroupName = "NinjaScriptGeneral", Order = 1)]
        public GuerillaChartPattern Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Threshold1", Order = 5, GroupName = "Parameters")]
        public double Threshold1
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Threshold2", Order = 6, GroupName = "Parameters")]
        public double Threshold2
        { get; set; }
        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GuerillaStickSimple[] cacheGuerillaStickSimple;
		public GuerillaStickSimple GuerillaStickSimple(Brush upColor, Brush downColor, GuerillaChartPattern pattern, double threshold1, double threshold2)
		{
			return GuerillaStickSimple(Input, upColor, downColor, pattern, threshold1, threshold2);
		}

		public GuerillaStickSimple GuerillaStickSimple(ISeries<double> input, Brush upColor, Brush downColor, GuerillaChartPattern pattern, double threshold1, double threshold2)
		{
			if (cacheGuerillaStickSimple != null)
				for (int idx = 0; idx < cacheGuerillaStickSimple.Length; idx++)
					if (cacheGuerillaStickSimple[idx] != null && cacheGuerillaStickSimple[idx].UpColor == upColor && cacheGuerillaStickSimple[idx].DownColor == downColor && cacheGuerillaStickSimple[idx].Pattern == pattern && cacheGuerillaStickSimple[idx].Threshold1 == threshold1 && cacheGuerillaStickSimple[idx].Threshold2 == threshold2 && cacheGuerillaStickSimple[idx].EqualsInput(input))
						return cacheGuerillaStickSimple[idx];
			return CacheIndicator<GuerillaStickSimple>(new GuerillaStickSimple(){ UpColor = upColor, DownColor = downColor, Pattern = pattern, Threshold1 = threshold1, Threshold2 = threshold2 }, input, ref cacheGuerillaStickSimple);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GuerillaStickSimple GuerillaStickSimple(Brush upColor, Brush downColor, GuerillaChartPattern pattern, double threshold1, double threshold2)
		{
			return indicator.GuerillaStickSimple(Input, upColor, downColor, pattern, threshold1, threshold2);
		}

		public Indicators.GuerillaStickSimple GuerillaStickSimple(ISeries<double> input , Brush upColor, Brush downColor, GuerillaChartPattern pattern, double threshold1, double threshold2)
		{
			return indicator.GuerillaStickSimple(input, upColor, downColor, pattern, threshold1, threshold2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GuerillaStickSimple GuerillaStickSimple(Brush upColor, Brush downColor, GuerillaChartPattern pattern, double threshold1, double threshold2)
		{
			return indicator.GuerillaStickSimple(Input, upColor, downColor, pattern, threshold1, threshold2);
		}

		public Indicators.GuerillaStickSimple GuerillaStickSimple(ISeries<double> input , Brush upColor, Brush downColor, GuerillaChartPattern pattern, double threshold1, double threshold2)
		{
			return indicator.GuerillaStickSimple(input, upColor, downColor, pattern, threshold1, threshold2);
		}
	}
}

#endregion
