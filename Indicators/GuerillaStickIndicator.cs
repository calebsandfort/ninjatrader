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
	public class GuerillaStickIndicator : Indicator
    {
        private Brush downColor = Brushes.DimGray;
        private Brush upColor = Brushes.DimGray;
        private Brush textColor = Brushes.DimGray;
        private bool downTrend;
        private bool upTrend;
        private bool showPatternCount = true;
        private bool showAlerts = true;
        private bool drawLabels = true;
        private int patternsFound;
        private Gui.Tools.SimpleFont textFont;
        private GuerillaChartPattern pattern = GuerillaChartPattern.MorningStar;
        private TextPosition textBoxPosition = TextPosition.BottomRight;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionCandlestickPattern;
                Name = NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameCandlestickPattern;
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DrawOnPricePanel = true;
                DisplayInDataBox = false;
                IsAutoScale = false;
                PaintPriceMarkers = false;
                TrendStrength = 4;
                textFont = new Gui.Tools.SimpleFont() { Size = 14 };

                downColor = Brushes.DimGray;
                upColor = Brushes.DimGray;
                textColor = Brushes.DimGray;

                AddPlot(Brushes.Transparent, NinjaTrader.Custom.Resource.CandlestickPatternFound);
            }
            else if (State == State.Configure)
            {
                if (Calculate == Calculate.OnEachTick || Calculate == Calculate.OnPriceChange)
                    Calculate = Calculate.OnBarClose;
            }
            else if (State == State.Historical)
            {
                if (ChartControl != null)
                {
                    downColor = ChartControl.Properties.AxisPen.Brush; // Get the color of the chart axis brush
                    textColor = ChartControl.Properties.ChartText;
                }

                if (((SolidColorBrush)downColor).Color == ((SolidColorBrush)upColor).Color)
                    upColor = Brushes.Transparent;
                else
                    upColor = Brushes.DimGray;
            }
        }

        protected override void OnBarUpdate()
        {
            // Calculates trend lines and prevailing trend for patterns that require a trend
            if (TrendStrength > 0 && CurrentBar >= TrendStrength)
            {
                CalculateTrendLines();
            }

            PatternFound[0] = 0; //	Initialize each bar to No Pattern found

            switch (pattern)
            {
                case GuerillaChartPattern.BearishBeltHold:
                    {
                        #region Bearish Belt Hold - Need to find test data w & w/o strength

                        if (CurrentBar < 1 || (TrendStrength > 0 && !upTrend))
                            return;

                        if (Close[1] > Open[1] && Open[0] > Close[1] + 5 * TickSize && Open[0] == High[0] && Close[0] < Open[0])
                        {
                            if (ChartBars != null)
                            {
                                BarBrushes[1] = upColor;
                                CandleOutlineBrushes[1] = downColor;
                                BarBrush = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Bearish Belt Hold" + CurrentBar, false, " Bearish Belt\nHold # " + patternsFound, 0, Math.Max(High[0], High[1]), 40, textColor,
                                textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BearishEngulfing:
                    {
                        #region Bearish Engulfing
                        if (CurrentBar < 1 || (TrendStrength > 0 && !upTrend))
                            return;

                        if (Close[1] > Open[1] && Close[0] < Open[0] && Open[0] > Close[1] && Close[0] < Open[1])
                        {
                            BarBrushes[1] = upColor;
                            BarBrush = downColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Bearish Engulfing" + CurrentBar, false, " Bearish\nEngulfing # " + patternsFound, 0, Math.Max(High[0], High[1]), 50, textColor,
                                textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BearishHarami:
                    {
                        #region Bearish Harami
                        if (CurrentBar < 1 || (TrendStrength > 0 && !upTrend))
                            return;

                        if (Close[0] < Open[0] && Close[1] > Open[1] && Low[0] >= Open[1] && High[0] <= Close[1])
                        {
                            BarBrushes[1] = upColor;
                            BarBrush = downColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Bearish Harami" + CurrentBar, false, "Bearish\nHarami # " + patternsFound, 0, Math.Max(High[0], High[1]), 40, textColor,
                                textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BearishHaramiCross:
                    {
                        #region Bearish Harami Cross
                        if (CurrentBar < 1 || (TrendStrength > 0 && !upTrend))
                            return;

                        if ((High[0] <= Close[1]) && (Low[0] >= Open[1]) && Open[0] <= Close[1] && Close[0] >= Open[1]
                            && ((Close[0] >= Open[0] && Close[0] <= Open[0] + TickSize) || (Close[0] <= Open[0] && Close[0] >= Open[0] - TickSize)))
                        {
                            BarBrush = downColor;
                            BarBrushes[1] = upColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Bearish Harami Cross" + CurrentBar, false, "Bearish Harami\nCross # " + patternsFound, 0, Math.Max(High[0], High[1]), 40, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BullishBeltHold:
                    {
                        #region Bullish Belt Hold
                        if (CurrentBar < 1 || (TrendStrength > 0 && !downTrend))
                            return;

                        if (Close[1] < Open[1] && Open[0] < Close[1] - 5 * TickSize && Open[0] == Low[0] && Close[0] > Open[0])
                        {
                            if (ChartBars != null)
                            {
                                BarBrushes[1] = downColor;
                                BarBrush = upColor;
                                CandleOutlineBrushes[0] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Bullish Belt Hold" + CurrentBar, false, "Bullish Belt\nHold # " + patternsFound, 0, Math.Min(Low[0], Low[1]), -10, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BullishEngulfing:
                    {
                        #region Bullish Engulfing
                        if (CurrentBar < 1 || (TrendStrength > 0 && !downTrend))
                            return;

                        if (Close[1] < Open[1] && Close[0] > Open[0] && Close[0] > Open[1] && Open[0] < Close[1])
                        {
                            if (ChartBars != null)
                            {
                                BarBrushes[1] = downColor;
                                BarBrush = upColor;
                                CandleOutlineBrushes[0] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Bullish Engulfing" + CurrentBar, false, "Bullish\nEngulfing # " + patternsFound, 0, Math.Min(Low[0], Low[1]), -10, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BullishHarami:
                    {
                        #region Bullish Harami
                        if (CurrentBar < 1 || (TrendStrength > 0 && !downTrend))
                            return;

                        if (Close[0] > Open[0] && Close[1] < Open[1] && Low[0] >= Close[1] && High[0] <= Open[1])
                        {
                            if (ChartBars != null)
                            {
                                BarBrushes[1] = downColor;
                                BarBrush = upColor;
                                CandleOutlineBrushes[0] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Bullish Harami" + CurrentBar, false, "Bullish\nHarami # " + patternsFound, 0, Math.Min(Low[0], Low[1]), -10, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BullishHaramiCross:
                    {
                        #region Bullish Harami Cross
                        if (CurrentBar < 1 || (TrendStrength > 0 && !downTrend))
                            return;

                        if ((High[0] <= Open[1]) && (Low[0] >= Close[1]) && Open[0] >= Close[1] && Close[0] <= Open[1]
                            && ((Close[0] >= Open[0] && Close[0] <= Open[0] + TickSize) || (Close[0] <= Open[0] && Close[0] >= Open[0] - TickSize)))
                        {
                            if (ChartBars != null)
                            {
                                BarBrushes[1] = downColor;
                                BarBrush = upColor;
                                CandleOutlineBrushes[0] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Bullish Harami Cross" + CurrentBar, false, "Bullish\nHarami\nCross # " + patternsFound, 0, Math.Min(Low[0], Low[1]), -10, textColor,
                                textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.DarkCloudCover:
                    {
                        #region Dark Cloud Cover
                        if (CurrentBar < 1 || (TrendStrength > 0 && !upTrend))
                            return;

                        if (Open[0] > High[1] && Close[1] > Open[1] && Close[0] < Open[0] && Close[0] <= Close[1] - (Close[1] - Open[1]) / 2 && Close[0] >= Open[1])
                        {
                            if (ChartBars != null)
                            {
                                BarBrushes[1] = upColor;
                                CandleOutlineBrushes[1] = downColor;
                                BarBrush = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Dark Cloud Cover" + CurrentBar, false, "Dark Cloud\nCover # " + patternsFound, 1, Math.Max(High[0], High[1]), 50, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.Doji:
                    {
                        #region Doji

                        if (Math.Abs(Close[0] - Open[0]) <= (High[0] - Low[0]) * 0.07)
                        {
                            //if (ChartBars != null)
                            //{
                            //    BarBrush = upColor;
                            //    CandleOutlineBrushes[0] = downColor;
                            //}

                            patternsFound++;

                            PatternFound[0] = 1;

                            int yOffset = Close[0] > Close[Math.Min(1, CurrentBar)] ? 20 : -20;

                            if (DrawLabels)
                            {
                                Draw.Text(this, "Doji Text" + CurrentBar, true, "Doji\n# " + patternsFound, 0, (yOffset > 0 ? High[0] : Low[0]), yOffset, textColor, textFont,
                                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.DownsideTasukiGap:
                    {
                        #region Downside Tasuki Gap

                        if (CurrentBar < 2)
                            return;

                        if (Close[2] < Open[2] && Close[1] < Open[1] && Close[0] > Open[0]
                            && High[1] < Low[2]
                            && Open[0] > Close[1] && Open[0] < Open[1]
                            && Close[0] > Open[1] && Close[0] < Close[2])
                        {
                            if (ChartBars != null)
                            {
                                BarBrush = upColor;
                                CandleOutlineBrushes[0] = downColor;
                                BarBrushes[1] = downColor;
                                BarBrushes[2] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Downside Tasuki Gap" + CurrentBar, false, "Downside Tasuki\n Gap # " + patternsFound, 1, MAX(High, 3)[0], 10, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.EveningStar:
                    {
                        #region Evening Star
                        if (CurrentBar < 2)
                            return;

                        if (Close[2] > Open[2] && Close[1] > Close[2] && Open[0] < (Math.Abs((Close[1] - Open[1]) / 2) + Open[1]) && Close[0] < Open[0])
                        {
                            if (ChartBars != null)
                            {
                                if (Close[0] > Open[0])
                                {
                                    BarBrush = upColor;
                                    CandleOutlineBrushes[0] = downColor;
                                }
                                else
                                    BarBrush = downColor;

                                if (Close[1] > Open[1])
                                {
                                    BarBrushes[1] = upColor;
                                    CandleOutlineBrushes[1] = downColor;
                                }
                                else
                                    BarBrushes[1] = downColor;

                                if (Close[2] > Open[2])
                                {
                                    BarBrushes[2] = upColor;
                                    CandleOutlineBrushes[2] = downColor;
                                }
                                else
                                    BarBrushes[2] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Evening Star Text" + CurrentBar, false, "Evening\nStar # " + patternsFound, 1, MAX(High, 3)[0], 40, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.FallingThreeMethods:
                    {
                        #region Falling Three Methods
                        if (CurrentBar < 5)
                            return;

                        if (Close[4] < Open[4] && Close[0] < Open[0] && Close[0] < Low[4]
                            && High[3] < High[4] && Low[3] > Low[4]
                            && High[2] < High[4] && Low[2] > Low[4]
                            && High[1] < High[4] && Low[1] > Low[4])
                        {
                            if (ChartBars != null)
                            {
                                BarBrush = downColor;
                                BarBrushes[4] = downColor;

                                int x = 1;
                                while (x < 4)
                                {
                                    if (Close[x] > Open[x])
                                    {
                                        BarBrushes[x] = upColor;
                                        CandleOutlineBrushes[x] = downColor;
                                    }
                                    else
                                        BarBrushes[x] = downColor;
                                    x++;
                                }
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Falling Three Methods" + CurrentBar, false, "Falling Three\nMethods # " + patternsFound, 2, Math.Max(High[0], High[4]), 40, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.Hammer:
                    {
                        #region Hammer
                        //if (TrendStrength > 0)
                        //{
                        //    if (!downTrend || MIN(Low, TrendStrength)[0] != Low[0])
                        //        return;
                        //}

                        //if (
                        //    Low[0] < Open[0] - 5 * TickSize &&                                  //Lower shadow is > 5 ticks
                        //    Math.Abs(Open[0] - Close[0]) < (0.10 * (High[0] - Low[0])) &&       // Body < 10% range
                        //    (High[0] - Close[0]) < (0.25 * (High[0] - Low[0]))                  //Upper shadow < 25% of range

                        //    )
                        //{
                        //    if (ChartBars != null)
                        //    {
                        //        if (Close[0] > Open[0])
                        //        {
                        //            BarBrush = upColor;
                        //            CandleOutlineBrushes[0] = downColor;
                        //        }
                        //        else
                        //            BarBrush = downColor;
                        //    }

                        //    patternsFound++;

                        //    PatternFound[0] = 1;

                        //    Draw.Text(this, "Hammer" + CurrentBar, false, "Hammer\n # " + patternsFound, 0, Low[0], -20, textColor, textFont,
                        //        TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        //}
                        #endregion
                        #region Hammer
                        if (CurrentBar < 1 || (TrendStrength > 0 && !downTrend))
                            return;

                        //Red bar
                        if ((
                            Close[0] < Open[0] &&                                       //Red bar
                            Low[0] < Close[0] &&                                        //Has lower shadow
                            (Close[0] - Low[0]) >= 2 * (Open[0] - Close[0]) &&          //Lower shadow 2x >= body
                            (High[0] - Open[0]) <= 2 * TickSize) ||                     //Upper shadow <= 2 ticks

                        //Green bar
                            (
                            Open[0] < Close[0] &&                                       //Green bar
                            Low[0] < Open[0] &&                                        //Has lower shadow
                            (Open[0] - Low[0]) >= 2 * (Close[0] - Open[0]) &&          //Lower shadow 2x >= body
                            (High[0] - Close[0]) <= 2 * TickSize)                        //Upper shadow <= 2 ticks
                            )
                        {
                            //if (ChartBars != null)
                            //{
                            //    if (Close[0] > Open[0])
                            //    {
                            //        BarBrush = upColor;
                            //        CandleOutlineBrushes[0] = downColor;
                            //    }
                            //    else
                            //        BarBrush = downColor;
                            //}

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "Hammer" + CurrentBar, false, "Hammer\n # " + patternsFound, 0, Low[0], -20, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.HangingMan:
                    {
                        #region Hanging Man - Need data with/strength
                        if (TrendStrength > 0)
                        {
                            if (!upTrend || MAX(High, TrendStrength)[0] != High[0])
                                return;
                        }

                        if (Low[0] < Open[0] - 5 * TickSize && Math.Abs(Open[0] - Close[0]) < (0.10 * (High[0] - Low[0])) && (High[0] - Close[0]) < (0.25 * (High[0] - Low[0])))
                        {
                            if (ChartBars != null)
                            {
                                if (Close[0] > Open[0])
                                {
                                    BarBrush = upColor;
                                    CandleOutlineBrushes[0] = downColor;
                                }
                                else
                                    BarBrush = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Hanging Man" + CurrentBar, false, "Hanging\nMan # " + patternsFound, 0, Low[0], -20, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.InvertedHammer:
                    {
                        #region Inverted Hammer
                        if (TrendStrength > 0)
                        {
                            if (!upTrend || MAX(High, TrendStrength)[0] != High[0])
                                return;
                        }

                        if (High[0] > Open[0] + 5 * TickSize && Math.Abs(Open[0] - Close[0]) < (0.10 * (High[0] - Low[0])) && (Close[0] - Low[0]) < (0.25 * (High[0] - Low[0])))
                        {
                            if (ChartBars != null)
                            {
                                if (Close[0] > Open[0])
                                {
                                    BarBrush = upColor;
                                    CandleOutlineBrushes[0] = downColor;
                                }
                                else
                                    BarBrush = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Inverted Hammer" + CurrentBar, false, "Inverted\nHammer\n # " + patternsFound, 0, Low[0] - 2 * TickSize, 20, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.MorningStar:
                    {
                        #region Morning Star
                        if (CurrentBar < 2)
                            return;

                        if (Close[2] < Open[2] && Close[1] < Close[2] && Open[0] > (Math.Abs((Close[1] - Open[1]) / 2) + Open[1]) && Close[0] > Open[0])
                        {

                            if (ChartBars != null)
                            {
                                if (Close[0] > Open[0])
                                {
                                    BarBrush = upColor;
                                    CandleOutlineBrushes[0] = downColor;
                                }
                                else
                                    BarBrush = downColor;

                                if (Close[1] > Open[1])
                                {
                                    BarBrushes[1] = upColor;
                                    CandleOutlineBrushes[1] = downColor;
                                }
                                else
                                    BarBrushes[1] = downColor;


                                if (Close[2] > Open[2])
                                {
                                    BarBrushes[2] = upColor;
                                    CandleOutlineBrushes[2] = downColor;
                                }
                                else
                                    BarBrushes[2] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Morning Star Text" + CurrentBar, false, "Morning\nStar # " + patternsFound, 1, MIN(Low, 3)[0], -20, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.PiercingLine:
                    {
                        #region Piercing Line
                        if (CurrentBar < 1 || (TrendStrength > 0 && !downTrend))
                            return;

                        if (Open[0] < Low[1] && Close[1] < Open[1] && Close[0] > Open[0] && Close[0] >= Close[1] + (Open[1] - Close[1]) / 2 && Close[0] <= Open[1])
                        {
                            if (ChartBars != null)
                            {
                                CandleOutlineBrushes[1] = downColor;
                                BarBrushes[1] = upColor;
                                BarBrush = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Piercing Line" + CurrentBar, false, "Piercing\nLine # " + patternsFound, 1, Low[0], -10, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                        }

                        #endregion
                        break;
                    }

                case GuerillaChartPattern.RisingThreeMethods:
                    {
                        #region Rising Three Methods
                        if (CurrentBar < 5)
                            return;

                        if (Close[4] > Open[4] && Close[0] > Open[0] && Close[0] > High[4]
                            && High[3] < High[4] && Low[3] > Low[4]
                            && High[2] < High[4] && Low[2] > Low[4]
                            && High[1] < High[4] && Low[1] > Low[4])
                        {
                            if (ChartBars != null)
                            {
                                BarBrush = upColor;
                                CandleOutlineBrushes[0] = downColor;
                                BarBrushes[4] = upColor;
                                CandleOutlineBrushes[4] = downColor;

                                int x = 1;
                                while (x < 4)
                                {
                                    if (Close[x] > Open[x])
                                    {
                                        BarBrushes[x] = upColor;
                                        CandleOutlineBrushes[x] = downColor;
                                    }
                                    else
                                        BarBrushes[x] = downColor;
                                    x++;
                                }
                            }
                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Rising Three Methods" + CurrentBar, false, " Rising Three\nMethods #" + patternsFound, 2, MIN(Low, 5)[0], -10, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.ShootingStar:
                    {
                        #region Shooting Star
                        if (CurrentBar < 1 || (TrendStrength > 0 && !upTrend))
                            return;

                        //Red bar
                        if ((
                            Close[0] < Open[0] &&                                       //Red bar
                            High[0] > Open[0] &&                                        //Has upper shadow
                            (High[0] - Open[0]) >= 2 * (Open[0] - Close[0]) &&          //Upper shadow 2x >= body
                            (Close[0] - Low[0]) <= 2 * TickSize) ||                     //Lower shadow <= 2 ticks

                        //Green bar
                            (
                            Open[0] < Close[0] &&                                       //Green bar
                            High[0] > Close[0] &&                                       //Has upper shadow
                            (High[0] - Close[0]) >= 2 * (Close[0] - Open[0]) &&         //Upper shadow 2x >= body
                            (Open[0] - Low[0]) <= 2 * TickSize)                         //Lower shadow <= 2 ticks
                            )
                        {
                            //if (ChartBars != null)
                            //    BarBrush = downColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "Shooting Star" + CurrentBar, false, "Shooting\nStar # " + patternsFound, 0, High[0], 30, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.StickSandwich:
                    {
                        #region Stick Sandwich
                        if (CurrentBar < 2)
                            return;

                        if (Close[2] == Close[0] && Close[2] < Open[2] && Close[1] > Open[1] && Close[0] < Open[0])
                        {
                            if (ChartBars != null)
                            {
                                BarBrush = downColor;
                                BarBrushes[1] = upColor;
                                CandleOutlineBrushes[1] = downColor;
                                BarBrushes[2] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Stick Sandwich" + CurrentBar, false, "Stick\nSandwich\n  # " + patternsFound, 1, MAX(High, 3)[0], 50, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.ThreeBlackCrows:
                    {
                        #region Three Black Crows
                        if (CurrentBar < 2 || (TrendStrength > 0 && !upTrend))
                            return;

                        if (PatternFound[1] == 0 && PatternFound[2] == 0
                            && Close[0] < Open[0] && Close[1] < Open[1] && Close[2] < Open[2]
                            && Close[0] < Close[1] && Close[1] < Close[2]
                            && Open[0] < Open[1] && Open[0] > Close[1]
                            && Open[1] < Open[2] && Open[1] > Close[2])
                        {
                            if (ChartBars != null)
                            {
                                BarBrush = downColor;
                                BarBrushes[1] = downColor;
                                BarBrushes[2] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Three Black Crows" + CurrentBar, false, "Three Black\nCrows # " + patternsFound, 1, MAX(High, 3)[0], 50, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.ThreeWhiteSoldiers:
                    {
                        #region Three White Soldiers
                        if (CurrentBar < 2 || (TrendStrength > 0 && !downTrend))
                            return;

                        if (PatternFound[1] == 0 && PatternFound[2] == 0
                            && Close[0] > Open[0] && Close[1] > Open[1] && Close[2] > Open[2]
                            && Close[0] > Close[1] && Close[1] > Close[2]
                            && Open[0] < Close[1] && Open[0] > Open[1]
                            && Open[1] < Close[2] && Open[1] > Open[2])
                        {
                            if (ChartBars != null)
                            {
                                BarBrush = upColor;
                                CandleOutlineBrushes[0] = downColor;
                                BarBrushes[1] = upColor;
                                CandleOutlineBrushes[1] = downColor;
                                BarBrushes[2] = upColor;
                                CandleOutlineBrushes[2] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Three White Soldiers" + CurrentBar, false, "Three White\nSoldiers # " + patternsFound, 1, Low[2], -10, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.UpsideGapTwoCrows:
                    {
                        #region Upside Gap Two Crows - need data to find pattern!!
                        if (CurrentBar < 2 || (TrendStrength > 0 && !upTrend))
                            return;

                        if (Close[2] > Open[2] && Close[1] < Open[1] && Close[0] < Open[0]
                            && Low[1] > High[2]
                            && Close[0] > High[2]
                            && Close[0] < Close[1] && Open[0] > Open[1])
                        {
                            if (ChartBars != null)
                            {
                                BarBrush = downColor;
                                BarBrushes[1] = downColor;
                                BarBrushes[2] = upColor;
                                CandleOutlineBrushes[2] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Upside Gap Two Crows" + CurrentBar, false, "Upside Gap\nTwo Crows # " + patternsFound, 1, Math.Max(High[0], High[1]), 10, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.UpsideTasukiGap:
                    {
                        #region Upside Tasuki Gap
                        if (CurrentBar < 2)
                            return;

                        if (Close[2] > Open[2] && Close[1] > Open[1] && Close[0] < Open[0]
                            && Low[1] > High[2]
                            && Open[0] < Close[1] && Open[0] > Open[1]
                            && Close[0] < Open[1] && Close[0] > Close[2])
                        {
                            if (ChartBars != null)
                            {
                                BarBrush = downColor;
                                BarBrushes[1] = upColor;
                                CandleOutlineBrushes[1] = downColor;
                                BarBrushes[2] = upColor;
                                CandleOutlineBrushes[2] = downColor;
                            }

                            patternsFound++;

                            PatternFound[0] = 1;

                            Draw.Text(this, "Upside Tasuki Gap" + CurrentBar, false, "Upside\nTasuki\nGap # " + patternsFound, 1, MIN(Low, 3)[0], -20, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BullishTrendBar:
                    {
                        #region BullishTrendBar
                        if (CurrentBar < 1)
                            return;

                        if (
                            (Open[0] < Close[0] &&                                       //Green bar
                            ((Close[0] - Open[0]) / (High[0] - Low[0])) > .7 &&         //Body > 70% of range
                            ((Open[0] - Low[0]) / (High[0] - Low[0])) < .25) ||           //Upper shadow < 25% of range

                            (Open[0] < Close[0] &&                                       //Green bar
                            (Open[0] - Low[0]) <= 2 * TickSize &&                     //Lower shadow <= 2 ticks
                            ((Close[0] - Open[0]) / (High[0] - Low[0])) > .5) ||           //Upper shadow < 25% of range

                            (Open[0] < Close[0] &&                                       //Green bar
                            Open[0] == Low[0] &&                     //Lower shadow <= 2 ticks
                            ((Close[0] - Open[0]) / (High[0] - Low[0])) > .33         //Body > 35% of range
                            ))               
                        {
                            //if (ChartBars != null)
                            //    BarBrush = upColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "Bullish Trend Bar" + CurrentBar, false, "Bullish Trend\nBar # " + patternsFound, 0, Low[0], -20, textColor, textFont,
                                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BearishTrendBar:
                    {
                        #region BearishTrendBar
                        if (CurrentBar < 1)
                            return;

                        //Red bar
                        if (
                            (Close[0] < Open[0] &&                                      //Red bar
                            ((Open[0] - Close[0]) / (High[0] - Low[0])) > .7 &&         //Body > 70% of range
                            ((High[0] - Open[0]) / (High[0] - Low[0])) < .25) ||        //Upper shadow < 25% of range

                            (Close[0] < Open[0] &&                                      //Red bar
                            (High[0] - Open[0]) <= 2 * TickSize &&                      //Upper shadow <= 2 ticks
                            ((Open[0] - Close[0]) / (High[0] - Low[0])) > .5) ||        //Upper shadow < 25% of range

                            (Close[0] < Open[0] &&                                      //Red bar
                            High[0] == Open[0] &&                                       //Upper shadow <= 2 ticks
                            ((Open[0] - Close[0]) / (High[0] - Low[0])) > .33))                                                      
                        {
                            //if (ChartBars != null)
                            //    BarBrush = downColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "Bearish Trend Bar" + CurrentBar, false, "Bearish Trend\nBar # " + patternsFound, 0, High[0], 30, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.GreenBar:
                    {
                        #region GreenBar
                        if (CurrentBar < 1)
                            return;

                        if (Open[0] < Close[0])
                        {
                            //if (ChartBars != null)
                            //    BarBrush = upColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "Green Bar" + CurrentBar, false, "Green\nBar # " + patternsFound, 0, Low[0], -20, textColor, textFont,
                                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.RedBar:
                    {
                        #region RedBar
                        if (CurrentBar < 1)
                            return;

                        if (Close[0] < Open[0])
                        {
                            //if (ChartBars != null)
                            //    BarBrush = upColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "Red Bar" + CurrentBar, false, "Red\nBar # " + patternsFound, 0, High[0], 30, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.IndecisionBar:
                    {
                        #region IndecisionBar
                        if (CurrentBar < 1)
                            return;

                        //Red bar
                        if ((
                            Close[0] < Open[0] &&                                       //Red bar
                            (((High[0] - Open[0]) / (High[0] - Low[0])) + ((Close[0] - Low[0]) / (High[0] - Low[0]))) > .75
                            ) ||
                        //Green bar
                            (
                            Open[0] < Close[0] &&                                       //Green bar
                            (((High[0] - Close[0]) / (High[0] - Low[0])) + ((Open[0] - Low[0]) / (High[0] - Low[0]))) > .75        //Upper shadow > 30% of body
                            ))
                        {
                            //if (ChartBars != null)
                            //    BarBrush = downColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "ID Bar" + CurrentBar, false, "ID\nBar # " + patternsFound, 0,
                                    Close[0] < Open[0] ? High[0] : Low[0],
                                    Close[0] < Open[0] ? 30 : -20,
                                    textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }
                case GuerillaChartPattern.BullishHalfBar:
                    {
                        #region BullishTrendBar
                        if (CurrentBar < 1)
                            return;

                        if (
                            Open[0] < Close[0] &&                                       //Green bar
                            ((Close[0] - Open[0]) / (High[0] - Low[0])) > .7         //Body > 50% of range
                            //&& ((Open[0] - Low[0]) > (High[0] - Close[0]))
                            )
                        {
                            //if (ChartBars != null)
                            //    BarBrush = upColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "Bullish Trend Bar" + CurrentBar, false, "Bullish Trend\nBar # " + patternsFound, 0, Low[0], -20, textColor, textFont,
                                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }

                case GuerillaChartPattern.BearishHalfBar:
                    {
                        #region BearishTrendBar
                        if (CurrentBar < 1)
                            return;

                        //Red bar
                        if (
                            Open[0] > Close[0] &&                                       //Green bar
                            ((Open[0] - Close[0]) / (High[0] - Low[0])) > .7         //Body > 70% of range
                            //&& ((Close[0] - Low[0]) < (High[0] - Open[0]))
                            )
                        {
                            //if (ChartBars != null)
                            //    BarBrush = downColor;

                            patternsFound++;

                            PatternFound[0] = 1;

                            if (this.DrawLabels)
                            {
                                Draw.Text(this, "Bearish Trend Bar" + CurrentBar, false, "Bearish Trend\nBar # " + patternsFound, 0, High[0], 30, textColor, textFont,
                                TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
                            }

                        }
                        #endregion
                        break;
                    }
            }

            if (showPatternCount)
                Draw.TextFixed(this, "Count", patternsFound + " " + pattern + "\n  patterns found", textBoxPosition, textColor, textFont, Brushes.Transparent, Brushes.Transparent, 0);


            if (PatternFound[0] == 1 && showAlerts)
            {
                Alert("myAlert", Priority.Low, "Pattern(s) found: " + patternsFound + " " + pattern + " on " + Instrument.FullName + " " + BarsPeriod.Value + " " +
                BarsPeriod.BarsPeriodType + " Chart", "Alert3.wav", 10, Brushes.DimGray, Brushes.DimGray);
            }
        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PatternFound
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
        [Display(ResourceType = typeof(Custom.Resource), Name = "SendAlerts", Description = "Set true to send alert message to Alerts window", GroupName = "NinjaScriptGeneral", Order = 2)]
        public bool ShowAlerts
        {
            get { return showAlerts; }
            set { showAlerts = value; }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "DrawLabels", Description = "Set true to draw label", GroupName = "NinjaScriptGeneral", Order = 6)]
        public bool DrawLabels
        {
            get { return drawLabels; }
            set { drawLabels = value; }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "ShowPatternCount", Description = "Set true to display on chart the count of patterns found", GroupName = "NinjaScriptGeneral", Order = 3)]
        public bool ShowPatternCount
        {
            get { return showPatternCount; }
            set { showPatternCount = value; }
        }

        [Display(ResourceType = typeof(Custom.Resource), Name = "TextFont", Description = "select font, style, size to display on chart", GroupName = "NinjaScriptGeneral", Order = 4)]
        public Gui.Tools.SimpleFont TextFont
        {
            get { return textFont; }
            set { textFont = value; }
        }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(ResourceType = typeof(Custom.Resource), Name = "TrendStrength", Description = "Number of bars required to define a trend when a pattern requires a prevailing trend. \nA value of zero will disable trend requirement.",
        GroupName = "NinjaScriptGeneral", Order = 5)]
        public int TrendStrength
        { get; set; }
        #endregion

        #region Misc

        public override string ToString()
        {
            return string.Format("{0}({1})", Name, pattern);
        }

        // Calculate trend lines and prevailing trend
        private void CalculateTrendLines()
        {
            // Calculate up trend line
            int upTrendStartBarsAgo = 0;
            int upTrendEndBarsAgo = 0;
            int upTrendOccurence = 1;

            while (Low[upTrendEndBarsAgo] <= Low[upTrendStartBarsAgo])
            {
                upTrendStartBarsAgo = Swing(TrendStrength).SwingLowBar(0, upTrendOccurence + 1, CurrentBar);
                upTrendEndBarsAgo = Swing(TrendStrength).SwingLowBar(0, upTrendOccurence, CurrentBar);

                if (upTrendStartBarsAgo < 0 || upTrendEndBarsAgo < 0)
                    break;

                upTrendOccurence++;
            }

            // Calculate down trend line
            int downTrendStartBarsAgo = 0;
            int downTrendEndBarsAgo = 0;
            int downTrendOccurence = 1;

            while (High[downTrendEndBarsAgo] >= High[downTrendStartBarsAgo])
            {

                downTrendStartBarsAgo = Swing(TrendStrength).SwingHighBar(0, downTrendOccurence + 1, CurrentBar);
                downTrendEndBarsAgo = Swing(TrendStrength).SwingHighBar(0, downTrendOccurence, CurrentBar);

                if (downTrendStartBarsAgo < 0 || downTrendEndBarsAgo < 0)
                    break;

                downTrendOccurence++;
            }

            if (upTrendStartBarsAgo > 0 && upTrendEndBarsAgo > 0 && upTrendStartBarsAgo < downTrendStartBarsAgo)
            {
                upTrend = true;
                downTrend = false;
            }
            else if (downTrendStartBarsAgo > 0 && downTrendEndBarsAgo > 0 && upTrendStartBarsAgo > downTrendStartBarsAgo)
            {
                upTrend = false;
                downTrend = true;
            }
            else
            {
                upTrend = false;
                downTrend = false;
            }
        }
        #endregion

    }
}

public enum GuerillaChartPattern
{
    BearishBeltHold,
    BearishEngulfing,
    BearishHarami,
    BearishHaramiCross,
    BullishBeltHold,
    BullishEngulfing,
    BullishHarami,
    BullishHaramiCross,
    DarkCloudCover,
    Doji,
    DownsideTasukiGap,
    EveningStar,
    FallingThreeMethods,
    Hammer,
    HangingMan,
    InvertedHammer,
    MorningStar,
    PiercingLine,
    RisingThreeMethods,
    ShootingStar,
    StickSandwich,
    ThreeBlackCrows,
    ThreeWhiteSoldiers,
    UpsideGapTwoCrows,
    UpsideTasukiGap,
    BullishTrendBar,
    BearishTrendBar,
    RedBar,
    GreenBar,
    IndecisionBar,
    BullishHalfBar,
    BearishHalfBar,
    FiftyHammer,
    FiftyMan,
    FiftyBar,
    BigBar
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GuerillaStickIndicator[] cacheGuerillaStickIndicator;
		public GuerillaStickIndicator GuerillaStickIndicator(GuerillaChartPattern pattern, bool showAlerts, bool drawLabels, bool showPatternCount, int trendStrength)
		{
			return GuerillaStickIndicator(Input, pattern, showAlerts, drawLabels, showPatternCount, trendStrength);
		}

		public GuerillaStickIndicator GuerillaStickIndicator(ISeries<double> input, GuerillaChartPattern pattern, bool showAlerts, bool drawLabels, bool showPatternCount, int trendStrength)
		{
			if (cacheGuerillaStickIndicator != null)
				for (int idx = 0; idx < cacheGuerillaStickIndicator.Length; idx++)
					if (cacheGuerillaStickIndicator[idx] != null && cacheGuerillaStickIndicator[idx].Pattern == pattern && cacheGuerillaStickIndicator[idx].ShowAlerts == showAlerts && cacheGuerillaStickIndicator[idx].DrawLabels == drawLabels && cacheGuerillaStickIndicator[idx].ShowPatternCount == showPatternCount && cacheGuerillaStickIndicator[idx].TrendStrength == trendStrength && cacheGuerillaStickIndicator[idx].EqualsInput(input))
						return cacheGuerillaStickIndicator[idx];
			return CacheIndicator<GuerillaStickIndicator>(new GuerillaStickIndicator(){ Pattern = pattern, ShowAlerts = showAlerts, DrawLabels = drawLabels, ShowPatternCount = showPatternCount, TrendStrength = trendStrength }, input, ref cacheGuerillaStickIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GuerillaStickIndicator GuerillaStickIndicator(GuerillaChartPattern pattern, bool showAlerts, bool drawLabels, bool showPatternCount, int trendStrength)
		{
			return indicator.GuerillaStickIndicator(Input, pattern, showAlerts, drawLabels, showPatternCount, trendStrength);
		}

		public Indicators.GuerillaStickIndicator GuerillaStickIndicator(ISeries<double> input , GuerillaChartPattern pattern, bool showAlerts, bool drawLabels, bool showPatternCount, int trendStrength)
		{
			return indicator.GuerillaStickIndicator(input, pattern, showAlerts, drawLabels, showPatternCount, trendStrength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GuerillaStickIndicator GuerillaStickIndicator(GuerillaChartPattern pattern, bool showAlerts, bool drawLabels, bool showPatternCount, int trendStrength)
		{
			return indicator.GuerillaStickIndicator(Input, pattern, showAlerts, drawLabels, showPatternCount, trendStrength);
		}

		public Indicators.GuerillaStickIndicator GuerillaStickIndicator(ISeries<double> input , GuerillaChartPattern pattern, bool showAlerts, bool drawLabels, bool showPatternCount, int trendStrength)
		{
			return indicator.GuerillaStickIndicator(input, pattern, showAlerts, drawLabels, showPatternCount, trendStrength);
		}
	}
}

#endregion
