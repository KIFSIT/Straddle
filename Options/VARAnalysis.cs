using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;
using MTCommon;
using LogWriter;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace Straddle
{
    public partial class VARAnalysis : Form
    {
        public Dictionary<double, double> ProfitMap;
        public Dictionary<double, double> LossMap;
        public Dictionary<double, double> Ltp_PnlMap;
        public Dictionary<double, double> ExpiryLtp_PnlMap;
        public List<uint> exp_list;
        VerticalLineAnnotation VA;
        HorizontalLineAnnotation HA;
        public UInt32 MinExp = 0;
        ToolTip tooltipaxis = new ToolTip();
        //public StreamWriter file_dataWriter;
        public bool isPosZero = true;
        public bool isZoomed = false;
        public VARAnalysis()
        {
            InitializeComponent();
        }

        private void VARAnalysis_Load(object sender, EventArgs e)
        {
            try
            {
                ProfitMap = new Dictionary<double, double>();
                LossMap = new Dictionary<double, double>();

                ChartArea CA = chart1.ChartAreas[0];
                CA.AxisY.ScaleView.Zoomable = true;
                CA.CursorY.AutoScroll = true;
                CA.CursorY.IsUserSelectionEnabled = true;
                //chart1.MouseWheel += new MouseEventHandler(chart1_MouseWheel);
                
                CA.AxisX.ScaleView.Zoomable = true;
                CA.CursorX.AutoScroll = true;
                CA.CursorX.IsUserSelectionEnabled = true;
                tooltipaxis.ShowAlways = false;

                chart1.ChartAreas[0].CursorX.LineColor = Color.LightBlue;
                chart1.ChartAreas[0].CursorY.LineColor = Color.LightBlue;
                chart1.ChartAreas[0].CursorX.LineWidth = 1;
                chart1.ChartAreas[0].CursorY.LineWidth = 1;

                HA = new HorizontalLineAnnotation();
                HA.AxisY = chart1.ChartAreas[0].AxisY;
                HA.AllowMoving = false;
                HA.IsInfinitive = true;
                HA.ClipToChartArea = chart1.ChartAreas[0].Name;
                HA.LineColor = Color.Red;
                HA.LineWidth = 1;
                HA.Y = 0;
                chart1.Annotations.Add(HA);

                VA = new VerticalLineAnnotation();
                VA.AxisX = chart1.ChartAreas[0].AxisX;
                VA.AllowMoving = false;
                VA.IsInfinitive = true;
                VA.ClipToChartArea = chart1.ChartAreas[0].Name;
                VA.LineColor = Color.Red;
                VA.LineDashStyle = ChartDashStyle.Dot;
                VA.LineWidth = 1;
                chart1.Annotations.Add(VA);

                Ltp_PnlMap = new Dictionary<double, double>();
                ExpiryLtp_PnlMap = new Dictionary<double, double>();

                CheckExpiry();
                GetStrategyNew();
            }
            catch (Exception)
            {

            }
        }

        void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            //if (e.Delta > 0)
            //{

            //    //Axis ax = chart1.ChartAreas[0].AxisX;
            //    //ax.ScaleView.Size = double.IsNaN(ax.ScaleView.Size) ?
            //    //                    (ax.Maximum - ax.Minimum) / 2 : ax.ScaleView.Size /= 2;
            //    Axis ay = chart1.ChartAreas[0].AxisY;
            //    ay.ScaleView.Size = double.IsNaN(ay.ScaleView.Size) ?
            //                        (ay.Maximum - ay.Minimum) / 2 : ay.ScaleView.Size /= 2;
            //}
            //else
            //{

            //    //Axis ax = chart1.ChartAreas[0].AxisX;
            //    //ax.ScaleView.Size = double.IsNaN(ax.ScaleView.Size) ?
            //    //                    ax.Maximum : ax.ScaleView.Size *= 2;
            //    //if (ax.ScaleView.Size > ax.Maximum - ax.Minimum)
            //    //{
            //    //    ax.ScaleView.Size = ax.Maximum;
            //    //    ax.ScaleView.Position = 0;
            //    //}
            //    Axis ay = chart1.ChartAreas[0].AxisY;
            //    ay.ScaleView.Size = double.IsNaN(ay.ScaleView.Size) ?
            //                        ay.Maximum : ay.ScaleView.Size *= 2;
            //    if (ay.ScaleView.Size > ay.Maximum - ay.Minimum)
            //    {
            //        ay.ScaleView.Size = ay.Maximum;
            //        ay.ScaleView.Position = 0;
            //    }
            //}
        }

        public void CheckExpiry()
        {
            try
            {
                int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
                MarketWatch watch = AppGlobal.MarketWatch[iRow];
                string startegy_name = watch.Strategy;
                exp_list = new List<uint>();
                foreach (MarketWatch watch1 in AppGlobal.MarketWatch.Where(x => x.Strategy == startegy_name && x.posInt != 0))
                {
                    exp_list.Add(Convert.ToUInt32(watch1.Leg1.expiryUniqueID));
                }
                if (exp_list.Count != 0)
                {
                    MinExp = exp_list.Min();
                    isPosZero = false;
                }
            }
            catch (Exception)
            { 
            
            }
        }

        public void GetExpiryOptPrices(MarketWatch watch, double spot,double remainingdays)
        {
            try
            {
                double pnlpershare = 0;
                double watchpnl = 0;
                double actual_price = 0;
                
                if (watch.Leg1.ContractInfo.Series == "CE" || watch.Leg1.ContractInfo.Series == "PE")
                {
                    GreeksVariable greek = new GreeksVariable();
                    greek.IntrestRate = 0;
                    greek.StrikePrice = watch.Leg1.ContractInfo.StrikePrice;
                    greek.SpotPrice = spot;
                    if (remainingdays != 0)
                        greek.Volatility = watch.Leg1.BuyIV;
                    //greek.Volatility = 0;//.Round(watch.Leg1.BuyIV, 2);
                    if (remainingdays == 0)
                    {
                        remainingdays = 1;
                    }
                    greek.TimeToExpiry = remainingdays;
                    string series = watch.Leg1.ContractInfo.Series;
                    if (series == "CE")
                    {
                        actual_price = Math.Round(Convert.ToDouble(CalculatorUtils.CallPrice(greek)), 2);
                    }
                    else
                    {
                        actual_price = Math.Round(Convert.ToDouble(CalculatorUtils.PutPrice(greek)), 2);
                    }

                    double average_Price = 0;
                    average_Price = Math.Round(watch.Leg1.A_Value / watch.Leg1.Net_Qty, 2);

                    pnlpershare = Math.Round(actual_price - average_Price, 2);
                    watchpnl = Math.Round(pnlpershare * watch.Leg1.ContDetail.LotSize * watch.posInt, 2);
                    AppGlobal.varexp_total_pnl = AppGlobal.varexp_total_pnl + watchpnl;
                }
                else
                {
                    actual_price = spot;

                    double average_price = 0;
                    average_price = Math.Round(watch.Leg1.A_Value / watch.Leg1.Net_Qty, 2);
                    pnlpershare = Math.Round(actual_price - average_price, 2);

                    watchpnl = Math.Round(pnlpershare * watch.Leg1.ContDetail.LotSize * watch.posInt, 2);
                    AppGlobal.varexp_total_pnl = AppGlobal.varexp_total_pnl + watchpnl;
                }
            }
            catch (Exception)
            {

            }
        }

        public void GetExpiryOptPricesFor121(MarketWatch watch, double spot, double daysleft)
        {
            try
            {
                double pnlpershare = 0;
                double watchpnl = 0;
                double actual_price = 0; double actual_price2 = 0; double actual_price3 = 0;
                #region leg1
                if (watch.Leg1.ContractInfo.Series == "CE" || watch.Leg1.ContractInfo.Series == "PE")
                {
                    GreeksVariable greek = new GreeksVariable();
                    greek.IntrestRate = 0;
                    greek.StrikePrice = watch.Leg1.ContractInfo.StrikePrice;
                    greek.SpotPrice = spot;
                    if (daysleft != 0)
                        greek.Volatility = watch.Leg1.BuyIV;
                    //greek.Volatility = 0;//Math.Round(watch.Leg1.BuyIV, 2);
                    if (daysleft == 0)
                    {
                        daysleft = 0.5;
                    }
                    greek.TimeToExpiry = daysleft;
                    string series = watch.Leg1.ContractInfo.Series;
                    if (series == "CE")
                    {
                        actual_price = Math.Round(Convert.ToDouble(CalculatorUtils.CallPrice(greek)), 2);
                    }
                    else
                    {
                        actual_price = Math.Round(Convert.ToDouble(CalculatorUtils.PutPrice(greek)), 2);
                    }
                }

                #endregion


                #region leg2
                if (watch.Leg2.ContractInfo.Series == "CE" || watch.Leg2.ContractInfo.Series == "PE")
                {
                    GreeksVariable greek = new GreeksVariable();
                    greek.IntrestRate = 0;
                    greek.StrikePrice = watch.Leg2.ContractInfo.StrikePrice;
                    greek.SpotPrice = spot;
                    //greek.Volatility = 0;//Math.Round(watch.Leg2.BuyIV, 2);
                    if (daysleft != 0)
                        greek.Volatility = watch.Leg1.BuyIV;
                    if (daysleft == 0)
                    {
                        daysleft = 0.5;
                    }
                    greek.TimeToExpiry = daysleft;
                    string series = watch.Leg2.ContractInfo.Series;
                    if (series == "CE")
                    {
                        actual_price2 = Math.Round(Convert.ToDouble(CalculatorUtils.CallPrice(greek)), 2);
                    }
                    else
                    {
                        actual_price2 = Math.Round(Convert.ToDouble(CalculatorUtils.PutPrice(greek)), 2);
                    }
                }
                #endregion

                if (watch.Leg3.ContractInfo.Series == "CE" || watch.Leg3.ContractInfo.Series == "PE")
                {
                    GreeksVariable greek = new GreeksVariable();
                    greek.IntrestRate = 0;
                    greek.StrikePrice = watch.Leg3.ContractInfo.StrikePrice;
                    greek.SpotPrice = spot;
                    if (daysleft != 0)
                        greek.Volatility = watch.Leg1.BuyIV;
                    //greek.Volatility = 0;//Math.Round(watch.Leg3.BuyIV, 2);
                    if (daysleft == 0)
                    {
                        daysleft = 0.5;
                    }
                    greek.TimeToExpiry = daysleft;
                    string series = watch.Leg3.ContractInfo.Series;
                    if (series == "CE")
                    {
                        actual_price3 = Math.Round(Convert.ToDouble(CalculatorUtils.CallPrice(greek)), 2);
                    }
                    else
                    {
                        actual_price3 = Math.Round(Convert.ToDouble(CalculatorUtils.PutPrice(greek)), 2);
                    }
                }
                double butterfly_spread = 0;
                if (watch.PosType == "Wind")
                {
                    //sell buy sell
                    butterfly_spread = (-1 * actual_price) + (2 * actual_price2) + (-1 * actual_price3);
                }
                else
                {
                    //buy sell buy
                    butterfly_spread = (actual_price) + (-1 * 2 * actual_price2) + (actual_price3);

                }

                double average_Price = 0;
                average_Price = Math.Round(watch.avgPrice / watch.posInt, 2);
                pnlpershare = Math.Round(average_Price - butterfly_spread, 2);
                watchpnl = Math.Round(pnlpershare * watch.Leg1.ContDetail.LotSize * watch.posInt, 2);//
                AppGlobal.varexp_total_pnl = AppGlobal.varexp_total_pnl + watchpnl;
            }
            catch (Exception)
            {

            }
        }
    

        public void GetOptPrices121(MarketWatch watch, double spot)
        {
            try
            {
                double pnlpershare = 0;
                double watchpnl = 0;
                double actual_price = 0; double actual_price2 = 0; double actual_price3 = 0;
                if (watch.Leg1.ContractInfo.Series == "CE" || watch.Leg1.ContractInfo.Series == "PE")
                {
                    GreeksVariable greek = new GreeksVariable();
                    greek.IntrestRate = 0;
                    greek.StrikePrice = watch.Leg1.ContractInfo.StrikePrice;
                    greek.SpotPrice = spot;
                    greek.Volatility = Math.Round(watch.Leg1.BuyIV, 2);
                    ulong exp = watch.Leg1.expiryUniqueID;
                    double time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(exp)));
                    if (time_to_exp == 1 || time_to_exp == 0)
                    {
                        time_to_exp = 1;
                        greek.TimeToExpiry = time_to_exp;
                    }
                    else
                    {
                        greek.TimeToExpiry = time_to_exp - 1;
                    }
                    //greek.TimeToExpiry = time_to_exp-1;
                    string series = watch.Leg1.ContractInfo.Series;
                    if (series == "CE")
                    {
                        actual_price = Math.Round(Convert.ToDouble(CalculatorUtils.CallPrice(greek)), 2);
                    }
                    else
                    {
                        actual_price = Math.Round(Convert.ToDouble(CalculatorUtils.PutPrice(greek)), 2);
                    }
                   
                }

                if (watch.Leg2.ContractInfo.Series == "CE" || watch.Leg2.ContractInfo.Series == "PE")
                {
                    GreeksVariable greek = new GreeksVariable();
                    greek.IntrestRate = 0;
                    greek.StrikePrice = watch.Leg2.ContractInfo.StrikePrice;
                    greek.SpotPrice = spot;
                    greek.Volatility = Math.Round(watch.Leg2.BuyIV, 2);
                    ulong exp = watch.Leg2.expiryUniqueID;
                    double time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(exp)));
                    if (time_to_exp == 1 || time_to_exp == 0)
                    {
                        time_to_exp = 1;
                        greek.TimeToExpiry = time_to_exp;
                    }
                    else
                    {
                        greek.TimeToExpiry = time_to_exp - 1;
                    }
                    //greek.TimeToExpiry = time_to_exp - 1;
                    string series = watch.Leg2.ContractInfo.Series;
                    if (series == "CE")
                    {
                        actual_price2 = Math.Round(Convert.ToDouble(CalculatorUtils.CallPrice(greek)), 2);
                    }
                    else
                    {
                        actual_price2 = Math.Round(Convert.ToDouble(CalculatorUtils.PutPrice(greek)), 2);
                    }

                }

                //actual - 

                if (watch.Leg3.ContractInfo.Series == "CE" || watch.Leg3.ContractInfo.Series == "PE")
                {
                    GreeksVariable greek = new GreeksVariable();
                    greek.IntrestRate = 0;
                    greek.StrikePrice = watch.Leg3.ContractInfo.StrikePrice;
                    greek.SpotPrice = spot;
                    greek.Volatility = Math.Round(watch.Leg3.BuyIV, 2);
                    ulong exp = watch.Leg3.expiryUniqueID;
                    double time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(exp)));
                    if (time_to_exp == 1 || time_to_exp == 0)
                    {
                        time_to_exp = 1;
                        greek.TimeToExpiry = time_to_exp;
                    }
                    else
                    {
                        greek.TimeToExpiry = time_to_exp - 1;
                    }
                    //greek.TimeToExpiry = time_to_exp - 1;
                    string series = watch.Leg3.ContractInfo.Series;
                    if (series == "CE")
                    {
                        actual_price3 = Math.Round(Convert.ToDouble(CalculatorUtils.CallPrice(greek)), 2);
                    }
                    else
                    {
                        actual_price3 = Math.Round(Convert.ToDouble(CalculatorUtils.PutPrice(greek)), 2);
                    }

                }


                double butterfly_spread = 0;
                //if (actual_price != 0 || actual_price2 != 0 || actual_price3 != 0)
                {
                    if (watch.PosType == "Wind")
                    {
                        //sell buy sell
                        butterfly_spread = (-1 * actual_price) + (2 * actual_price2) + (-1 * actual_price3);
                    }
                    else
                    {
                        //buy sell buy
                        butterfly_spread = (actual_price) + (-1 * 2 * actual_price2) + (actual_price3);

                    }
                }

                //if (butterfly_spread != 0)
                {
                    double average_Price = 0;
                    //if (watch.posInt < 0)
                    {
                        average_Price = Math.Round(watch.avgPrice / watch.posInt, 2);
                    }
                    //else
                    //{

                    //    average_Price = Math.Round(watch.avgPrice / watch.posInt, 2);
                    //}
                    pnlpershare = Math.Round(average_Price - butterfly_spread, 2);

                    watchpnl = Math.Round(pnlpershare * watch.Leg1.ContDetail.LotSize * watch.posInt, 2);//
                    AppGlobal.var_total_pnl = AppGlobal.var_total_pnl + watchpnl;
                }
            }
            catch (Exception)
            {

            }
        }

        public void GetOptPrices(MarketWatch watch, double spot)
        {
            try
            {
                double pnlpershare = 0;
                double watchpnl = 0;
                double actual_price = 0;
                if (watch.Leg1.ContractInfo.Series == "CE" || watch.Leg1.ContractInfo.Series == "PE")
                {
                    GreeksVariable greek = new GreeksVariable();
                    greek.IntrestRate = 0;
                    greek.StrikePrice = watch.Leg1.ContractInfo.StrikePrice;
                    greek.SpotPrice = spot;
                    greek.Volatility =Math.Round(watch.Leg1.BuyIV, 2);
                    ulong exp = watch.Leg1.expiryUniqueID;
                    double time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(exp)));
                    if (time_to_exp == 1 || time_to_exp == 0)
                    {
                        time_to_exp = 1;
                        greek.TimeToExpiry = time_to_exp;
                    }
                    else
                    {
                        greek.TimeToExpiry = time_to_exp - 1;
                    }
                    string series = watch.Leg1.ContractInfo.Series;
                    if (series == "CE")
                    {
                        actual_price = Math.Round(Convert.ToDouble(CalculatorUtils.CallPrice(greek)), 2);
                    }
                    else
                    {
                        actual_price = Math.Round(Convert.ToDouble(CalculatorUtils.PutPrice(greek)), 2);
                    }
                    double average_Price = 0;
                    average_Price = Math.Round(watch.Leg1.A_Value / watch.Leg1.Net_Qty, 2);

                    pnlpershare = Math.Round(actual_price - average_Price, 2);

                    watchpnl = Math.Round(pnlpershare * watch.Leg1.ContDetail.LotSize * watch.posInt, 2);
                    AppGlobal.var_total_pnl = AppGlobal.var_total_pnl + watchpnl;
                }
                else
                {
                    actual_price = spot;
                    double average_price = 0;
                    average_price = Math.Round(watch.Leg1.A_Value / watch.Leg1.Net_Qty, 2);
                    pnlpershare = Math.Round(actual_price - average_price, 2);

                    watchpnl = Math.Round(pnlpershare * watch.Leg1.ContDetail.LotSize * watch.posInt, 2);
                    AppGlobal.var_total_pnl = AppGlobal.var_total_pnl + watchpnl;
                }
            }
            catch (Exception)
            {

            }
        }

        public void SetRangeAndInterval()
        {
            try
            {
                //double min, max = 0;
                //double minexp = ExpiryLtp_PnlMap.Values.Min();
                //double maxexp = ExpiryLtp_PnlMap.Values.Max();
                //double min_tmrw = Ltp_PnlMap.Values.Min();
                //double max_tmrw = Ltp_PnlMap.Values.Max();
                //if (minexp < min_tmrw)
                //{
                //    min = minexp;
                //}
                //else
                //{
                //    min = min_tmrw;
                //}

                //if (maxexp > max_tmrw)
                //{
                //    max = maxexp;
                //}
                //else
                //{
                //    max = max_tmrw;
                //}

                //double interval = Convert.ToDouble((max - min) * 0.25);
                //chart1.ChartAreas[0].AxisY.Interval = interval;
            }
            catch (Exception)
            {

            }
        }

        public int GetIndex(Dictionary<double, double> dictionary, double key)
        {
            for (int index = 0; index < dictionary.Count; index++)
            {
                if (dictionary.ElementAt(index).Value == key)
                    return index;
            }
            return -1;
        }

        public void setYrangenew()
        {
            try
            {
                //var bottom5 = ExpiryLtp_PnlMap.OrderBy(pair => pair.Value).Take(ExpiryLtp_PnlMap.Count);
                ExpiryLtp_PnlMap = ExpiryLtp_PnlMap.OrderBy(pair => pair.Value).Take(ExpiryLtp_PnlMap.Count).ToDictionary(pair => pair.Key, pair => pair.Value);
                double max_exp = ExpiryLtp_PnlMap.Values.Max();
                int index_max = GetIndex(ExpiryLtp_PnlMap, max_exp);
                int lower = index_max - 25;
                int higher = index_max + 25;
                double lowerval = ExpiryLtp_PnlMap.ElementAt(lower).Value;
                double higherval = ExpiryLtp_PnlMap.ElementAt(higher).Value;
                if (higher <= ExpiryLtp_PnlMap.Count)
                {
                    chart1.ChartAreas[0].AxisX.Minimum = lowerval - 100;
                    chart1.ChartAreas[0].AxisX.Maximum = higherval + 100;
                    chart1.ChartAreas[0].AxisY.Maximum = 0 - (higherval + 500);
                    chart1.ChartAreas[0].AxisY.Minimum = 0 + (higherval + 500);
                }
                else
                {
                    chart1.ChartAreas[0].AxisX.Minimum = ExpiryLtp_PnlMap.Last().Key - 100;
                    chart1.ChartAreas[0].AxisX.Maximum = ExpiryLtp_PnlMap.Last().Key + 100;
                    chart1.ChartAreas[0].AxisY.Maximum = 0 + ((ExpiryLtp_PnlMap.Last().Value) + 500);
                    chart1.ChartAreas[0].AxisY.Minimum = 0 - (Math.Abs(ExpiryLtp_PnlMap.Last().Value) + 500);
                }
                var fmap = ExpiryLtp_PnlMap.Where(x => x.Value >= lowerval && x.Value <= higherval).ToDictionary(pair => pair.Key, pair => pair.Value);
                fmap = fmap.OrderBy(pair => pair.Key).Take(fmap.Count).ToDictionary(pair => pair.Key, pair => pair.Value);
                for (int i = 0; i < fmap.Count; i++)
                {
                    if (i <= fmap.Count)
                    {
                        var item = fmap.ElementAt(i);
                        chart1.Series[0].Points.AddXY(item.Key, item.Value);
                    }
                }

                double min_exp = ExpiryLtp_PnlMap.Values.Min();
                int index_min = GetIndex(ExpiryLtp_PnlMap, min_exp);
                int lowermin = index_min - 25;
                int highermin = index_min + 25;
                double lowervalmin = ExpiryLtp_PnlMap.ElementAt(lowermin).Value;
                double highervalmin = ExpiryLtp_PnlMap.ElementAt(highermin).Value;
                if (higher <= ExpiryLtp_PnlMap.Count)
                {
                    //chart1.ChartAreas[0].AxisX.Minimum = ExpiryLtp_PnlMap.ElementAt(lowermin).Key - 100;
                    //chart1.ChartAreas[0].AxisX.Maximum = ExpiryLtp_PnlMap.ElementAt(highermin).Key + 100;
                    chart1.ChartAreas[0].AxisY.Maximum = 0 - (highervalmin + 500);
                    chart1.ChartAreas[0].AxisY.Minimum = 0 + (highervalmin + 500);
                }
                else
                {
                    //chart1.ChartAreas[0].AxisX.Minimum = ExpiryLtp_PnlMap.Last().Key - 100;
                    //chart1.ChartAreas[0].AxisX.Maximum = ExpiryLtp_PnlMap.Last().Key + 100;
                    chart1.ChartAreas[0].AxisY.Maximum = 0 + ((ExpiryLtp_PnlMap.Last().Value) + 500);
                    chart1.ChartAreas[0].AxisY.Minimum = 0 - (Math.Abs(ExpiryLtp_PnlMap.Last().Value) + 500);
                }
                var fmapmin = ExpiryLtp_PnlMap.Where(x => x.Value >= lowerval && x.Value <= higherval).ToDictionary(pair => pair.Key, pair => pair.Value);
                fmapmin = fmapmin.OrderBy(pair => pair.Key).Take(fmapmin.Count).ToDictionary(pair => pair.Key, pair => pair.Value);
                for (int i = 0; i < fmapmin.Count; i++)
                {
                    if (i <= fmapmin.Count)
                    {
                        var item = fmapmin.ElementAt(i);
                        chart1.Series[0].Points.AddXY(item.Key, item.Value);
                    }
                }
            }
            catch (Exception ex)
            { 
            }
        }

        public void SetYRange()
        {
            try
            {
                //Ltp_PnlMap.Values.All(x => x < 0);
                double min, max = 0;
                double min_td = Math.Abs(Ltp_PnlMap.Values.Min());
                double max_td =Math.Abs(Ltp_PnlMap.Values.Max());
                double min_exp = Math.Abs(ExpiryLtp_PnlMap.Values.Min());
                double max_exp = Math.Abs(ExpiryLtp_PnlMap.Values.Max());
                if (min_exp < min_td)
                {
                    min = min_exp;
                }
                else
                {
                    min = min_td;
                }

                if (max_exp > max_td)
                {
                    max = max_exp;
                }
                else
                {
                    max = max_td;
                }
                double final_range = 0;
                if (min > max)
                {
                    final_range = min;
                }
                else
                {
                    final_range = max;
                }
                chart1.ChartAreas[0].AxisY.Maximum = 0 + (final_range + 5000);
                chart1.ChartAreas[0].AxisY.Minimum = 0 - (final_range + 5000);
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "SetYRange")
                                                 , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        public void PlotExpiryGraph()
        {
            try
            {
                foreach (var item in ExpiryLtp_PnlMap)
                {
                    double key = Math.Round(item.Key);
                    double value = item.Value;
                    chart1.Series["ExpiryDay"].Points.AddXY(key, value);
                }
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "PlotExpiryGraph")
                                             , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        public void PlotChart()
        {
            try
            {
                foreach (var item in ExpiryLtp_PnlMap)
                {
                    double key = Math.Round(item.Key);
                    double value = item.Value;
                    if (value > 0)
                    {
                        ProfitMap.Add(key, value);
                    }

                    if (value < 0)
                    {
                        LossMap.Add(key, value);
                    }
                }

                double max=0, min = 0;
                if (ProfitMap.Count != 0)
                {
                    max = ProfitMap.Values.Max();
                    chart1.ChartAreas[0].AxisY.Maximum = 0 + (max + 500);
                }
                else
                {
                    chart1.ChartAreas[0].AxisY.Maximum = 0;
                   
                }

                if (LossMap.Count != 0)
                {
                    min = LossMap.Values.Min();
                    chart1.ChartAreas[0].AxisY.Minimum = 0 - Math.Abs(min + 500);
                }
                else
                {
                    chart1.ChartAreas[0].AxisY.Minimum = 0;
                }


                Dictionary<double, double> higher_val = ProfitMap.OrderBy(x => x.Value).Where(x => x.Value < max).Take(50).ToDictionary(pair => pair.Key, pair => pair.Value);
                higher_val = higher_val.OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (var item in higher_val)
                {
                    chart1.Series[0].Points.AddXY(item.Key, item.Value);
                }

                Dictionary<double, double> higher_val1 = LossMap.OrderBy(x => x.Value).Where(x => x.Value > min).Take(50).ToDictionary(pair => pair.Key, pair => pair.Value);
                higher_val1 = higher_val1.OrderBy(x => x.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                foreach (var item in higher_val1)
                {
                    chart1.Series[0].Points.AddXY(item.Key, item.Value);
                }
            }
            catch (Exception ex)
            { 
            
            }
        }

        //internal void GeneratePnLFiles(string strategy)
        //{
        //    try
        //    {
        //        //string path = ArisApi_a._arisAp6i.SystemConfig.DBFilePath;
        //        string path = "D:\\P&LFiles\\";
        //        if (!Directory.Exists(path))
        //        {
        //            Directory.CreateDirectory(path);
        //        }
        //        string date = DateTime.Now.ToString("H_mm") + ".csv";
        //        //string fileName = path + strategy + "_" + DateTime.Now + ".csv";
        //        string fileName = path + strategy + "-" + date;
        //        string colheaders = "FuturePrice" + "," + "P&L"; ;
        //        if (!File.Exists(fileName))
        //        {
        //            file_dataWriter = new StreamWriter(fileName, true);
        //            file_dataWriter.AutoFlush = true;
        //            file_dataWriter.WriteLine(colheaders);
        //        }
        //        else
        //        {
        //            file_dataWriter = new StreamWriter(fileName, true);
        //            file_dataWriter.AutoFlush = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //ArisApi_a._arisApi.WriteToErrorLog(MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
        //    }
        //}

        public void GetStrategyNew()
        {
            try
            {
                int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
                MarketWatch watch = AppGlobal.MarketWatch[iRow];
                string startegy_name = watch.Strategy;
                string symbol = watch.Leg1.ContractInfo.Symbol;
                double spot_price = 0;
                if (symbol == "NIFTY")
                {
                    spot_price = Convert.ToDouble(AppGlobal.frmWatch.lblcashNifty.Text);//Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                }
                else if (symbol == "BANKNIFTY")
                {
                    spot_price = Convert.ToDouble(AppGlobal.frmWatch.lblcashbk.Text);//Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                }
                else
                {
                    spot_price = Convert.ToDouble(AppGlobal.frmWatch.lblFinNiftySpot.Text);//Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                }

                VA.X = spot_price;

                double min = spot_price - (spot_price * 0.1);
                double max = spot_price + (spot_price * 0.1);
                double interval = 25;

                chart1.ChartAreas[0].AxisX.Minimum = Math.Round(Math.Floor(min));
                chart1.ChartAreas[0].AxisX.Maximum = Math.Round(Math.Ceiling(max));
                chart1.ChartAreas[0].AxisX.Interval =Convert.ToInt32((max - min) / 4);

                //chart1.ChartAreas[0].AxisY.Minimum = 0;
                //chart1.ChartAreas[0].AxisY.Maximum = 0;
                //chart1.ChartAreas[0].AxisY.Interval = 100;
                int id = 0; 
                for (double i = min; i <= max; i = i + interval)
                {
                    AppGlobal.var_total_pnl = 0;
                    AppGlobal.varexp_total_pnl = 0;
                    double future_ltp = Math.Round(i);
                    foreach (MarketWatch watch1 in AppGlobal.MarketWatch.Where(x => x.Strategy == startegy_name && x.posInt != 0))
                    {
                      
                        id = watch1.StrategyId;
                        if (watch1.StrategyId == 91)
                        {
                            double min_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(MinExp)));
                            double watch_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(watch1.Leg1.expiryUniqueID)));
                            double daysleft = watch_time_to_exp - min_time_to_exp;
                            double daystoexp = daysleft;
                            GetOptPrices(watch1, future_ltp);
                            GetExpiryOptPrices(watch1, future_ltp, daystoexp);
                        }
                        else if (watch1.StrategyId == 121)
                        {
                            double min_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(MinExp)));
                            double watch_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(watch1.Leg1.expiryUniqueID)));
                            //double daysleft = watch_time_to_exp - min_time_to_exp;
                            GetOptPrices121(watch1, future_ltp);
                            GetExpiryOptPricesFor121(watch1, future_ltp, watch_time_to_exp);
                        }
                    }
#region b4
		                    //foreach (MarketWatch watch1 in AppGlobal.MarketWatch.Where(x => x.Strategy == startegy_name && x.posInt != 0 && x.StrategyId == 91))
                    //{
                    //    double daystoexp = 0;

                    //    //if (watch1.Leg1.expiryUniqueID == MinExp)
                    //    //{
                    //    //    daystoexp = 1;
                    //    //}
                    //        double min_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(MinExp)));
                    //        double watch_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(watch1.Leg1.expiryUniqueID)));
                    //        double daysleft = watch_time_to_exp - min_time_to_exp;
                    //        daystoexp = daysleft;
                    //    GetOptPrices(watch1, future_ltp);
                    //    GetExpiryOptPrices(watch1, future_ltp, daystoexp);
                    //}

                    //foreach (MarketWatch watch1 in AppGlobal.MarketWatch.Where(x => x.Strategy == startegy_name && x.posInt != 0 && x.StrategyId == 121))
                    //{

                    //    double min_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(MinExp)));
                    //    double watch_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(watch1.Leg1.expiryUniqueID)));
                    //    //double daysleft = watch_time_to_exp - min_time_to_exp;
                    //    GetOptPrices121(watch1,future_ltp);
                    //    GetExpiryOptPricesFor121(watch1, future_ltp, watch_time_to_exp);

                    //}

                    //double sqpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId == strategy_id && x.Strategy == startegy_name).Select(x => x.Sqpnl).Sum();//changes strategyid ==0
                    //double carryForward = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId == strategy_id && x.Strategy == startegy_name).Select(x => x.CarryForwardPnl).Sum();//changes strategyid ==0
#endregion
                    double sqpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId == id && x.Strategy == startegy_name).Select(x => x.Sqpnl).Sum();//changes strategyid ==0
                    double carryForward = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId == id && x.Strategy == startegy_name).Select(x => x.CarryForwardPnl).Sum();//changes strategyid ==0
                    if (!Ltp_PnlMap.ContainsKey(future_ltp))
                    {
                        Ltp_PnlMap.Add(future_ltp, Math.Round(AppGlobal.var_total_pnl + sqpnl + carryForward));//
                    }

                    if (!ExpiryLtp_PnlMap.ContainsKey(future_ltp))
                    {
                        ExpiryLtp_PnlMap.Add(future_ltp, Math.Round(AppGlobal.varexp_total_pnl + sqpnl + carryForward));//
                    }
                }
               
                if (isPosZero == false)
                {
                    SetYRange();
                    PlotGraph();
                    PlotExpiryGraph();
                    //PlotChart();
                    //setYrangenew();

                    DataPoint be_point1; DataPoint be_point2;
                    string break_evens = "";
                    if (chart1.Series[0].Points[0].YValues[0] > 0)
                    {
                        for (int i = 0; i < chart1.Series[0].Points.Count-1; i++)
                        {
                            if (chart1.Series[0].Points[i].YValues[0] > 0)
                            {
                                if (chart1.Series[0].Points[i + 1].YValues[0] < 0)
                                {
                                    be_point1 = new DataPoint(chart1.Series[0].Points[i + 1].XValue, chart1.Series[0].Points[i + 1].YValues[0]);
                                    //chart1.Series["BE1"].LegendText = "ExpiryBE1: " + be_point1.XValue.ToString();
                                    if (break_evens != "")
                                    {
                                        break_evens = break_evens + "-" + be_point1.XValue.ToString();
                                    }
                                    else
                                    {
                                        break_evens = be_point1.XValue.ToString();
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < chart1.Series[0].Points.Count-1; i++)
                        {
                            if (chart1.Series[0].Points[i].YValues[0] < 0)
                            {
                                
                                if (chart1.Series[0].Points[i + 1].YValues[0] > 0)
                                {
                                    be_point2 = new DataPoint(chart1.Series[0].Points[i + 1].XValue, chart1.Series[0].Points[i + 1].YValues[0]);
                                    //chart1.Series["BE2"].LegendText = "ExpiryBE2: " + be_point2.XValue.ToString();
                                    if (break_evens != "")
                                    {
                                        break_evens = break_evens + "-" + be_point2.XValue.ToString();
                                    }
                                    else
                                    {
                                        break_evens = be_point2.XValue.ToString();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < chart1.Series[0].Points.Count-1; i++)
                        {
                            if (chart1.Series[0].Points[i].YValues[0] < 0)
                            {
                                if (chart1.Series[0].Points[i + 1].YValues[0] > 0)
                                {
                                    be_point1 = new DataPoint(chart1.Series[0].Points[i + 1].XValue, chart1.Series[0].Points[i + 1].YValues[0]);
                                    //chart1.Series["BE1"].LegendText = "ExpiryBE1: " + be_point1.XValue.ToString();
                                    if (break_evens != "")
                                    {
                                        break_evens = break_evens + "-" + be_point1.XValue.ToString();
                                    }
                                    else
                                    {
                                        break_evens = be_point1.XValue.ToString();
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < chart1.Series[0].Points.Count-1; i++)
                        {
                            if (chart1.Series[0].Points[i].YValues[0] > 0)
                            {
                                if (chart1.Series[0].Points[i + 1].YValues[0] < 0)
                                {
                                    be_point2 = new DataPoint(chart1.Series[0].Points[i + 1].XValue, chart1.Series[0].Points[i + 1].YValues[0]);
                                    //chart1.Series["BE2"].LegendText = "ExpiryBE2: " + be_point2.XValue.ToString();
                                    if (break_evens != "")
                                    {
                                        break_evens = break_evens + "-" + be_point2.XValue.ToString();
                                    }
                                    else
                                    {
                                        break_evens = be_point2.XValue.ToString();
                                    }
                                }
                            }
                        }
                    }

                    string[] numbers = break_evens.Split('-').ToArray();
                    Array.Sort(numbers);
                    string result = string.Join("-", numbers);
                    chart1.Series["BE1"].LegendText = result;


                   double maxprofit = Math.Round(ExpiryLtp_PnlMap.Values.Max());
                   //chart1.Series["Max Profit"].LegendText = "Max Prof: " +maxprofit.ToString();
                   if (maxprofit > 0)
                   {
                       chart1.Series["Max Profit"].LegendText = "Max Profit: " + maxprofit.ToString();
                   }
                   else
                   {
                       chart1.Series["Max Profit"].LegendText = "Max Profit: " + "0";
                   }
                 
                   double maxloss = Math.Round(ExpiryLtp_PnlMap.Values.Min());
                   if (maxloss < 0)
                   {
                       chart1.Series["Max Loss"].LegendText = "Max Loss: " + maxloss.ToString();
                   }
                   else
                   {
                       chart1.Series["Max Loss"].LegendText = "Max Loss: " +"0";
                   }

                   double watchpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.Strategy == startegy_name && x.StrategyId != 0).Select(x => x.Sqpnl).Sum();//changes strategyid ==0
                    chart1.Series["Totalp&L"].LegendText = "Total P&L: " + watchpnl;


                   double positive_count = ExpiryLtp_PnlMap.Values.Count(x => x > 0);
                   double c= ExpiryLtp_PnlMap.Values.Count;
                   double prob =Math.Round((positive_count /c ) * 100,2);
                   chart1.Series["Prob. Of Profit"].LegendText = "Prob. Of Profit: " + prob + "%";
                }
                else
                {
                    MessageBox.Show("Position is closed.");
                    AppGlobal._VARAnalysis.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void PlotGraph()
        {
            try
            {
                foreach (var item in Ltp_PnlMap)
                {
                    double key = Math.Round(item.Key);
                    double value = item.Value;
                    chart1.Series["T+1"].Points.AddXY(key, value);
                }
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "PlotExpiryGraph")
                                            , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        private void VARAnalysis_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._VARAnalysis = null;
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var pos = e.Location;
                var results = chart1.HitTest(pos.X, pos.Y, false, ChartElementType.PlottingArea);
                foreach (var result in results)
                {
                    if (result.ChartElementType == ChartElementType.PlottingArea)
                    {
                        Point mousepoint = new Point(e.X, e.Y);

                        chart1.ChartAreas[0].CursorX.Interval = 0;
                        chart1.ChartAreas[0].CursorY.Interval = 0;
                        chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousepoint, true);
                        chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousepoint, true);

                        var xValue = chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                        var yValue = chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                        Series S = chart1.Series["T+1"];
                        Series Series_exp = chart1.Series["ExpiryDay"];
                        DataPoint pPrev = S.Points.Select(x => x).Where(x => x.XValue >= xValue).DefaultIfEmpty(S.Points.First()).First();
                        DataPoint pPrevexp = Series_exp.Points.Select(x => x).Where(x => x.XValue >= xValue).DefaultIfEmpty(Series_exp.Points.First()).First();

                        string value = string.Format("Price:{0},{1}T+1:{2},{3}Expiry:{4}", Math.Round(xValue, 2), Environment.NewLine, Math.Round(pPrev.YValues[0], 2), Environment.NewLine, Math.Round(pPrevexp.YValues[0], 2));
                        tooltipaxis.Show(value, this.chart1, e.X, e.Y);
                    }
                    else if (result.ChartElementType == ChartElementType.Nothing)
                    {
                        tooltipaxis.Hide(this.chart1);
                    }
                }

                //#1
                //var pos = e.Location;
                //Point mousepoint = new Point(e.X, e.Y);
                //chart1.ChartAreas[0].CursorX.Interval = 0;
                //chart1.ChartAreas[0].CursorY.Interval = 0;
                //chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousepoint, true);
                //chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousepoint, true);
                //tooltipchart.Show("X=" + chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X).ToString("0.##") + ", Y=" + chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y).ToString("0.##"), this.chart1, pos.X, pos.Y);

                //#2
                //var pos = e.Location;
                //if (prevPosition.HasValue && pos == prevPosition.Value)
                //    return;
                //tooltipchart.RemoveAll();
                //prevPosition = pos;
                //var results = chart1.HitTest(pos.X, pos.Y, false, ChartElementType.PlottingArea);
                //foreach (var result in results)
                //{
                //    if (result.ChartElementType == ChartElementType.PlottingArea)
                //    {
                //        var xVal = Math.Round(result.ChartArea.AxisX.PixelPositionToValue(pos.X), 2);
                //        var yVal = Math.Round(result.ChartArea.AxisY.PixelPositionToValue(pos.Y), 2);
                //        tooltipchart.Show("X=" + xVal + ", Y=" + yVal, this.chart1, pos.X, pos.Y);
                //    }
                //}
            }
            catch (Exception)
            {

            }
        }
    }
}
    
    
    
    
