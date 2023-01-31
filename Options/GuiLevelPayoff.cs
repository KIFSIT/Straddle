using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;
using System.Windows.Forms.DataVisualization.Charting;
using MTCommon;
using LogWriter;
using System.Collections;

namespace Straddle
{
    public partial class GuiLevelPayoff : Form
    {
        public Dictionary<double, double> Ltp_PnlMapBNifty;
        public Dictionary<double, double> ExpiryLtp_PnlMapBNifty;
        public Dictionary<double, double> Ltp_PnlMapNifty;
        public Dictionary<double, double> ExpiryLtp_PnlMapNifty;
        public List<uint> exp_list;
        VerticalLineAnnotation VABN;
        HorizontalLineAnnotation HABN;

        VerticalLineAnnotation VANifty;
        HorizontalLineAnnotation HANifty;
        public UInt32 MinExp = 0;

        ToolTip tooltipaxis = new ToolTip();
        ToolTip tooltipaxisNifty = new ToolTip();
        //public StreamWriter file_dataWriter;
        public bool isPosZero = true;
        public GuiLevelPayoff()
        {
            InitializeComponent();
        }

        private void GuiLevelPayoff_Load(object sender, EventArgs e)
        {
            ChartArea CA = BNchart.ChartAreas[0];
            CA.AxisY.ScaleView.Zoomable = true;
            CA.CursorY.AutoScroll = true;
            CA.CursorY.IsUserSelectionEnabled = true;

            CA.AxisX.ScaleView.Zoomable = true;
            CA.CursorX.AutoScroll = true;
            CA.CursorX.IsUserSelectionEnabled = true;
            tooltipaxis.ShowAlways = false;

            ChartArea CAN = NiftyChart.ChartAreas[0];
            CAN.AxisY.ScaleView.Zoomable = true;
            CAN.CursorY.AutoScroll = true;
            CAN.CursorY.IsUserSelectionEnabled = true;

            CAN.AxisX.ScaleView.Zoomable = true;
            CAN.CursorX.AutoScroll = true;
            CAN.CursorX.IsUserSelectionEnabled = true;
            tooltipaxisNifty.ShowAlways = false;

            BNchart.ChartAreas[0].CursorX.LineColor = Color.LightBlue;
            BNchart.ChartAreas[0].CursorY.LineColor = Color.LightBlue;
            BNchart.ChartAreas[0].CursorX.LineWidth = 1;
            BNchart.ChartAreas[0].CursorY.LineWidth = 1;

            NiftyChart.ChartAreas[0].CursorX.LineColor = Color.LightBlue;
            NiftyChart.ChartAreas[0].CursorY.LineColor = Color.LightBlue;
            NiftyChart.ChartAreas[0].CursorX.LineWidth = 1;
            NiftyChart.ChartAreas[0].CursorY.LineWidth = 1;


            HABN = new HorizontalLineAnnotation();
            HABN.AxisY = BNchart.ChartAreas[0].AxisY;
            HABN.AllowMoving = false;
            HABN.IsInfinitive = true;
            HABN.ClipToChartArea = BNchart.ChartAreas[0].Name;
            HABN.LineColor = Color.Red;
            HABN.LineWidth = 1;
            HABN.Y = 0;
            BNchart.Annotations.Add(HABN);

            VABN = new VerticalLineAnnotation();
            VABN.AxisX = BNchart.ChartAreas[0].AxisX;
            VABN.AllowMoving = false;
            VABN.IsInfinitive = true;
            VABN.ClipToChartArea = BNchart.ChartAreas[0].Name;
            VABN.LineColor = Color.Red;
            VABN.LineDashStyle = ChartDashStyle.Dot;
            VABN.LineWidth = 1;
            BNchart.Annotations.Add(VABN);

            HANifty = new HorizontalLineAnnotation();
            HANifty.AxisY = NiftyChart.ChartAreas[0].AxisY;
            HANifty.AllowMoving = false;
            HANifty.IsInfinitive = true;
            HANifty.ClipToChartArea = NiftyChart.ChartAreas[0].Name;
            HANifty.LineColor = Color.Red;
            HANifty.LineWidth = 1;
            HANifty.Y = 0;
            NiftyChart.Annotations.Add(HANifty);

            VANifty = new VerticalLineAnnotation();
            VANifty.AxisX = NiftyChart.ChartAreas[0].AxisX;
            VANifty.AllowMoving = false;
            VANifty.IsInfinitive = true;
            VANifty.ClipToChartArea = NiftyChart.ChartAreas[0].Name;
            VANifty.LineColor = Color.Red;
            VANifty.LineDashStyle = ChartDashStyle.Dot;
            VANifty.LineWidth = 1;
            NiftyChart.Annotations.Add(VANifty);

            Ltp_PnlMapBNifty = new Dictionary<double, double>();
            ExpiryLtp_PnlMapBNifty = new Dictionary<double, double>();

            Ltp_PnlMapNifty = new Dictionary<double, double>();
            ExpiryLtp_PnlMapNifty = new Dictionary<double, double>();

            //CheckExpiry();
            //GetStrategyNew();
            GetStrategyOlder();
        }

        public void CheckExpiry(string sym)
        {
            try
            {
                exp_list = new List<uint>();
               
                foreach (var watch in AppGlobal.MarketWatch.Where(x=> x.posInt != 0 && x.Leg1.ContractInfo.Symbol == sym))
                {
                    exp_list.Add(Convert.ToUInt32(watch.Leg1.expiryUniqueID));
                }
                if (exp_list.Count != 0)
                {
                    MinExp= exp_list.Min();
                    isPosZero = false;
                }
            }
            catch (Exception)
            {

            }
        }


        public void GetStrategyOlder()
        {
            Ltp_PnlMapBNifty.Clear();
            ExpiryLtp_PnlMapBNifty.Clear();
            BNchart.Series["ExpiryDay"].Points.Clear();
            BNchart.Series["T+1"].Points.Clear();

            Ltp_PnlMapNifty.Clear();
            ExpiryLtp_PnlMapNifty.Clear();
            NiftyChart.Series["ExpiryDay"].Points.Clear();
            NiftyChart.Series["T+1"].Points.Clear();

            double future_ltp = 0; double spot_price = 0; string symbol = "";
            var resultlist = AppGlobal.MarketWatch.Select(o => o.Leg1.ContractInfo.Symbol).Distinct().ToList();
            foreach (var watch in resultlist.Where(x => x != null))
            {
                symbol = watch;
                CheckExpiry(symbol);
                if (symbol == "NIFTY")
                {
                    spot_price = Convert.ToDouble(AppGlobal.frmWatch.lblcashNifty.Text);//Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                    VANifty.X = spot_price;
                }
                else if (symbol == "BANKNIFTY")
                {
                    spot_price = Convert.ToDouble(AppGlobal.frmWatch.lblcashbk.Text);//Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                    VABN.X = spot_price;
                }

                double min = spot_price - (spot_price * 0.1);
                double max = spot_price + (spot_price * 0.1);
                double interval = 25;
                for (double i = min; i <= max; i = i + interval)
                {
                    AppGlobal.var_total_pnl = 0;
                    AppGlobal.varexp_total_pnl = 0;
                    //AppGlobal.var_total_pnl_nifty = 0;
                    //AppGlobal.varexp_total_pnl_nifty = 0;
                    future_ltp = Math.Round(i);
                    //id = watch1.StrategyId;
                    foreach (MarketWatch watch1 in AppGlobal.MarketWatch.Where(x => x.posInt != 0 && x.Leg1.ContractInfo.Symbol == symbol))
                    {
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
                            //double min_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(MinExp)));
                            double watch_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(watch1.Leg1.expiryUniqueID)));
                            //double daysleft = watch_time_to_exp - min_time_to_exp;
                            GetOptPrices121(watch1, future_ltp);
                            GetExpiryOptPricesFor121(watch1, future_ltp, watch_time_to_exp);
                        }
                    }
                    //double sqpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.Sqpnl).Sum();//changes strategyid ==0
                    //double carryForward = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.CarryForwardPnl).Sum();//changes strategyid ==0
                    if (symbol == "BANKNIFTY")
                    {
                        double sqpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.Leg1.ContractInfo.Symbol == "BANKNIFTY").Select(x => x.Sqpnl).Sum();//changes strategyid ==0
                        double carryForward = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.Leg1.ContractInfo.Symbol == "BANKNIFTY").Select(x => x.CarryForwardPnl).Sum();//changes strategyid ==0
                        if (!Ltp_PnlMapBNifty.ContainsKey(future_ltp))
                        {
                            Ltp_PnlMapBNifty.Add(future_ltp, Math.Round(AppGlobal.var_total_pnl + sqpnl + carryForward));//
                        }

                        if (!ExpiryLtp_PnlMapBNifty.ContainsKey(future_ltp))
                        {
                            ExpiryLtp_PnlMapBNifty.Add(future_ltp, Math.Round(AppGlobal.varexp_total_pnl + sqpnl + carryForward));//
                        }
                    }
                    else
                    {
                        double sqpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.Leg1.ContractInfo.Symbol == "NIFTY").Select(x => x.Sqpnl).Sum();//changes strategyid ==0
                        double carryForward = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.Leg1.ContractInfo.Symbol == "NIFTY").Select(x => x.CarryForwardPnl).Sum();//changes strategyid ==0
                        if (!Ltp_PnlMapNifty.ContainsKey(future_ltp))
                        {
                            Ltp_PnlMapNifty.Add(future_ltp, Math.Round(AppGlobal.var_total_pnl + sqpnl + carryForward));//
                        }

                        if (!ExpiryLtp_PnlMapNifty.ContainsKey(future_ltp))
                        {
                            ExpiryLtp_PnlMapNifty.Add(future_ltp, Math.Round(AppGlobal.varexp_total_pnl + sqpnl + carryForward));//
                        }
                    }
                }
                if (isPosZero == false)
                {
                    if (symbol == "BANKNIFTY")
                    {
                        SetYRange(BNchart, ExpiryLtp_PnlMapBNifty, Ltp_PnlMapBNifty);
                        PlotGraph(Ltp_PnlMapBNifty, BNchart);
                        PlotExpiryGraph(ExpiryLtp_PnlMapBNifty, BNchart);
                        GetAllNotes(symbol, BNchart, ExpiryLtp_PnlMapBNifty, Ltp_PnlMapBNifty); 
                    }
                    else
                    {
                        SetYRange(NiftyChart, ExpiryLtp_PnlMapNifty, Ltp_PnlMapNifty);
                        PlotGraph(Ltp_PnlMapNifty, NiftyChart);
                        PlotExpiryGraph(ExpiryLtp_PnlMapNifty, NiftyChart);
                        GetAllNotes(symbol, NiftyChart, ExpiryLtp_PnlMapNifty, Ltp_PnlMapNifty); 
                    }
                }
            }
        }

        public void GetAllNotes(string symbol,Chart chart,Dictionary<double,double> expMap, Dictionary<double,double> tmrwMap)
        {
            //if (isPosZero == false)
            {
                DataPoint be_point1; DataPoint be_point2;
                string break_evens = "";
                if (chart.Series[0].Points[0].YValues[0] > 0)
                {
                    for (int i = 0; i < chart.Series[0].Points.Count - 1; i++)
                    {
                        if (chart.Series[0].Points[i].YValues[0] > 0)
                        {
                            if (chart.Series[0].Points[i + 1].YValues[0] < 0)
                            {
                                be_point1 = new DataPoint(chart.Series[0].Points[i + 1].XValue, chart.Series[0].Points[i + 1].YValues[0]);
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

                    for (int i = 0; i < chart.Series[0].Points.Count - 1; i++)
                    {
                        if (chart.Series[0].Points[i].YValues[0] < 0)
                        {

                            if (chart.Series[0].Points[i + 1].YValues[0] > 0)
                            {
                                be_point2 = new DataPoint(chart.Series[0].Points[i + 1].XValue, chart.Series[0].Points[i + 1].YValues[0]);
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
                    for (int i = 0; i < chart.Series[0].Points.Count - 1; i++)
                    {
                        if (chart.Series[0].Points[i].YValues[0] < 0)
                        {
                            if (chart.Series[0].Points[i + 1].YValues[0] > 0)
                            {
                                be_point1 = new DataPoint(chart.Series[0].Points[i + 1].XValue, chart.Series[0].Points[i + 1].YValues[0]);
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

                    for (int i = 0; i < chart.Series[0].Points.Count - 1; i++)
                    {
                        if (chart.Series[0].Points[i].YValues[0] > 0)
                        {
                            if (chart.Series[0].Points[i + 1].YValues[0] < 0)
                            {
                                be_point2 = new DataPoint(chart.Series[0].Points[i + 1].XValue, chart.Series[0].Points[i + 1].YValues[0]);
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
                chart.Series["BE1"].LegendText = result;


                double maxprofit = Math.Round(expMap.Values.Max());
                if (maxprofit > 0)
                {
                    chart.Series["Max Profit"].LegendText = "Max Profit: " + maxprofit.ToString();
                }
                else
                {
                    chart.Series["Max Profit"].LegendText = "Max Profit: " + "0";
                }

                double maxloss = Math.Round(expMap.Values.Min());
                if (maxloss < 0)
                {
                    chart.Series["Max Loss"].LegendText = "Max Loss: " + maxloss.ToString();
                }
                else
                {
                    chart.Series["Max Loss"].LegendText = "Max Loss: " + "0";
                }

                double watchpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0).Select(x => x.Sqpnl).Sum();//changes strategyid ==0
                chart.Series["Totalp&L"].LegendText = "Total P&L: " + watchpnl;


                double positive_count = expMap.Values.Count(x => x > 0);
                double c = expMap.Values.Count;
                double prob = Math.Round((positive_count / c) * 100, 2);
                chart.Series["Prob. Of Profit"].LegendText = "Prob. Of Profit: " + prob + "%";
            }
        }

        //public void GetStrategyNew()
        //{
        //    Ltp_PnlMapBNifty.Clear();
        //    ExpiryLtp_PnlMapBNifty.Clear();
        //    BNchart.Series["ExpiryDay"].Points.Clear();
        //    BNchart.Series["T+1"].Points.Clear();
        //    double future_ltp = 0; double spot_price = 0;
        //    var resultlist = AppGlobal.MarketWatch.Select(o => o.Leg1.ContractInfo.Symbol).Distinct().ToList();
        //    foreach (var watch in resultlist.Where(x => x != null))
        //    {
        //        string symbol = watch;
        //        if (symbol == "NIFTY")
        //        {
        //            spot_price = Convert.ToDouble(AppGlobal.frmWatch.lblcashNifty.Text);//Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
        //        }
        //        else if (symbol == "BANKNIFTY")
        //        {
        //            spot_price = Convert.ToDouble(AppGlobal.frmWatch.lblcashbk.Text);//Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
        //        }
        //        VA.X = spot_price;

        //        double min = spot_price - (spot_price * 0.1);
        //        double max = spot_price + (spot_price * 0.1);
        //        double interval = 25;
        //        for (double i = min; i <= max; i = i + interval)
        //        {
        //            AppGlobal.var_total_pnl = 0;
        //            AppGlobal.varexp_total_pnl = 0;
        //            future_ltp = Math.Round(i);
        //            //id = watch1.StrategyId;
        //            foreach (MarketWatch watch1 in AppGlobal.MarketWatch.Where(x => x.posInt != 0 && x.Leg1.ContractInfo.Symbol == symbol))
        //            {
        //                if (watch1.StrategyId == 91)
        //                {
        //                    double min_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(MinExp)));
        //                    double watch_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(watch1.Leg1.expiryUniqueID)));
        //                    double daysleft = watch_time_to_exp - min_time_to_exp;
        //                    double daystoexp = daysleft;
        //                    GetOptPrices(watch1, future_ltp);
        //                    GetExpiryOptPrices(watch1, future_ltp, daystoexp);
        //                }
        //                else if (watch1.StrategyId == 121)
        //                {
        //                    //double min_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(MinExp)));
        //                    double watch_time_to_exp = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(ArisDev.Market.NseCm, Convert.ToUInt32(watch1.Leg1.expiryUniqueID)));
        //                    //double daysleft = watch_time_to_exp - min_time_to_exp;
        //                    GetOptPrices121(watch1, future_ltp);
        //                    GetExpiryOptPricesFor121(watch1, future_ltp, watch_time_to_exp);
        //                }
        //            }
        //            double sqpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.Sqpnl).Sum();//changes strategyid ==0
        //            double carryForward = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.CarryForwardPnl).Sum();//changes strategyid ==0
        //            if (!Ltp_PnlMap.ContainsKey(future_ltp))
        //            {
        //                Ltp_PnlMap.Add(future_ltp, Math.Round(AppGlobal.var_total_pnl + sqpnl + carryForward));//
        //            }

        //            if (!ExpiryLtp_PnlMap.ContainsKey(future_ltp))
        //            {
        //                ExpiryLtp_PnlMap.Add(future_ltp, Math.Round(AppGlobal.varexp_total_pnl + sqpnl + carryForward));//
        //            }
        //        }

        //    }

        //    if (isPosZero == false)
        //    {
        //        SetYRange();
        //        PlotGraph();
        //        PlotExpiryGraph();
        //        DataPoint be_point1; DataPoint be_point2;
        //        string break_evens = "";
        //        if (BNchart.Series[0].Points[0].YValues[0] > 0)
        //        {
        //            for (int i = 0; i < BNchart.Series[0].Points.Count - 1; i++)
        //            {
        //                if (BNchart.Series[0].Points[i].YValues[0] > 0)
        //                {
        //                    if (BNchart.Series[0].Points[i + 1].YValues[0] < 0)
        //                    {
        //                        be_point1 = new DataPoint(BNchart.Series[0].Points[i + 1].XValue, BNchart.Series[0].Points[i + 1].YValues[0]);
        //                        //chart1.Series["BE1"].LegendText = "ExpiryBE1: " + be_point1.XValue.ToString();
        //                        if (break_evens != "")
        //                        {
        //                            break_evens = break_evens + "-" + be_point1.XValue.ToString();
        //                        }
        //                        else
        //                        {
        //                            break_evens = be_point1.XValue.ToString();
        //                        }
        //                    }
        //                }
        //            }

        //            for (int i = 0; i < BNchart.Series[0].Points.Count - 1; i++)
        //            {
        //                if (BNchart.Series[0].Points[i].YValues[0] < 0)
        //                {

        //                    if (BNchart.Series[0].Points[i + 1].YValues[0] > 0)
        //                    {
        //                        be_point2 = new DataPoint(BNchart.Series[0].Points[i + 1].XValue, BNchart.Series[0].Points[i + 1].YValues[0]);
        //                        //chart1.Series["BE2"].LegendText = "ExpiryBE2: " + be_point2.XValue.ToString();
        //                        if (break_evens != "")
        //                        {
        //                            break_evens = break_evens + "-" + be_point2.XValue.ToString();
        //                        }
        //                        else
        //                        {
        //                            break_evens = be_point2.XValue.ToString();
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 0; i < BNchart.Series[0].Points.Count - 1; i++)
        //            {
        //                if (BNchart.Series[0].Points[i].YValues[0] < 0)
        //                {
        //                    if (BNchart.Series[0].Points[i + 1].YValues[0] > 0)
        //                    {
        //                        be_point1 = new DataPoint(BNchart.Series[0].Points[i + 1].XValue, BNchart.Series[0].Points[i + 1].YValues[0]);
        //                        //chart1.Series["BE1"].LegendText = "ExpiryBE1: " + be_point1.XValue.ToString();
        //                        if (break_evens != "")
        //                        {
        //                            break_evens = break_evens + "-" + be_point1.XValue.ToString();
        //                        }
        //                        else
        //                        {
        //                            break_evens = be_point1.XValue.ToString();
        //                        }
        //                    }
        //                }
        //            }

        //            for (int i = 0; i < BNchart.Series[0].Points.Count - 1; i++)
        //            {
        //                if (BNchart.Series[0].Points[i].YValues[0] > 0)
        //                {
        //                    if (BNchart.Series[0].Points[i + 1].YValues[0] < 0)
        //                    {
        //                        be_point2 = new DataPoint(BNchart.Series[0].Points[i + 1].XValue, BNchart.Series[0].Points[i + 1].YValues[0]);
        //                        //chart1.Series["BE2"].LegendText = "ExpiryBE2: " + be_point2.XValue.ToString();
        //                        if (break_evens != "")
        //                        {
        //                            break_evens = break_evens + "-" + be_point2.XValue.ToString();
        //                        }
        //                        else
        //                        {
        //                            break_evens = be_point2.XValue.ToString();
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        string[] numbers = break_evens.Split('-').ToArray();
        //        Array.Sort(numbers);
        //        string result = string.Join("-", numbers);
        //        BNchart.Series["BE1"].LegendText = result;


        //        double maxprofit = Math.Round(ExpiryLtp_PnlMap.Values.Max());
        //        //chart1.Series["Max Profit"].LegendText = "Max Prof: " +maxprofit.ToString();
        //        if (maxprofit > 0)
        //        {
        //            BNchart.Series["Max Profit"].LegendText = "Max Profit: " + maxprofit.ToString();
        //        }
        //        else
        //        {
        //            BNchart.Series["Max Profit"].LegendText = "Max Profit: " + "0";
        //        }

        //        double maxloss = Math.Round(ExpiryLtp_PnlMap.Values.Min());
        //        if (maxloss < 0)
        //        {
        //            BNchart.Series["Max Loss"].LegendText = "Max Loss: " + maxloss.ToString();
        //        }
        //        else
        //        {
        //            BNchart.Series["Max Loss"].LegendText = "Max Loss: " + "0";
        //        }

        //        double watchpnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0).Select(x => x.Sqpnl).Sum();//changes strategyid ==0
        //        BNchart.Series["Totalp&L"].LegendText = "Total P&L: " + watchpnl;


        //        double positive_count = ExpiryLtp_PnlMap.Values.Count(x => x > 0);
        //        double c = ExpiryLtp_PnlMap.Values.Count;
        //        double prob = Math.Round((positive_count / c) * 100, 2);
        //        BNchart.Series["Prob. Of Profit"].LegendText = "Prob. Of Profit: " + prob + "%";
        //    }
        //}

        public void SetYRange(Chart chart, Dictionary<double, double> expMap, Dictionary<double, double> tmrwMap)
        {
            try
            {
                //if (symbol == "BANKNIFTY")
                {
                    double min, max = 0;
                    double min_td = Math.Abs(tmrwMap.Values.Min());
                    double max_td = Math.Abs(tmrwMap.Values.Max());
                    double min_exp = Math.Abs(expMap.Values.Min());
                    double max_exp = Math.Abs(expMap.Values.Max());
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
                    chart.ChartAreas[0].AxisY.Maximum = 0 + (final_range + 5000);
                    chart.ChartAreas[0].AxisY.Minimum = 0 - (final_range + 5000);
                }
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "SetYRange")
                                                 , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }


        public void PlotExpiryGraph(Dictionary<double, double> expMap, Chart chart1)
        {
            try
            {
                foreach (var item in expMap)
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

        public void PlotGraph(Dictionary<double,double> tmrwMap, Chart chart1)
        {
            try
            {
                foreach (var item in tmrwMap)
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

        public void GetExpiryOptPrices(MarketWatch watch, double spot, double remainingdays)
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
                        remainingdays = 0.5;
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
                    //if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                    {
                        AppGlobal.varexp_total_pnl = AppGlobal.varexp_total_pnl + watchpnl;
                    }
                    //else
                    //{
                    //    AppGlobal.varexp_total_pnl_nifty = AppGlobal.varexp_total_pnl_nifty + watchpnl;
                    //}
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
                    if (daysleft != 0)
                        greek.Volatility = watch.Leg1.BuyIV;
                    //greek.Volatility = 0;//Math.Round(watch.Leg2.BuyIV, 2);
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
                    //if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                    {
                        AppGlobal.varexp_total_pnl = AppGlobal.varexp_total_pnl + watchpnl;
                    }
                    //else
                    //{
                    //    AppGlobal.varexp_total_pnl_nifty = AppGlobal.varexp_total_pnl_nifty + watchpnl;
                    //}
                }
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
                    //if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                    {
                        AppGlobal.var_total_pnl = AppGlobal.var_total_pnl + watchpnl;
                    }
                    //else
                    //{
                    //    AppGlobal.var_total_pnl_nifty = AppGlobal.var_total_pnl_nifty + watchpnl;
                    //}
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
                    //if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                    {
                        AppGlobal.var_total_pnl = AppGlobal.var_total_pnl + watchpnl;
                    }
                    //else
                    //{
                    //    AppGlobal.var_total_pnl_nifty = AppGlobal.var_total_pnl_nifty + watchpnl;
                    //}
                }
                else
                {
                    actual_price = spot;
                    double average_price = 0;
                    average_price = Math.Round(watch.Leg1.A_Value / watch.Leg1.Net_Qty, 2);
                    pnlpershare = Math.Round(actual_price - average_price, 2);

                    watchpnl = Math.Round(pnlpershare * watch.Leg1.ContDetail.LotSize * watch.posInt, 2);

                    //if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                    {
                        AppGlobal.var_total_pnl = AppGlobal.var_total_pnl + watchpnl;
                    }
                    //else
                    //{
                    //    AppGlobal.var_total_pnl_nifty = AppGlobal.var_total_pnl_nifty + watchpnl;
                    //}
                }
            }
            catch (Exception)
            {

            }
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var pos = e.Location;
                var results = BNchart.HitTest(pos.X, pos.Y, false, ChartElementType.PlottingArea);
                foreach (var result in results)
                {
                    if (result.ChartElementType == ChartElementType.PlottingArea)
                    {
                        Point mousepoint = new Point(e.X, e.Y);

                        BNchart.ChartAreas[0].CursorX.Interval = 0;
                        BNchart.ChartAreas[0].CursorY.Interval = 0;
                        BNchart.ChartAreas[0].CursorX.SetCursorPixelPosition(mousepoint, true);
                        BNchart.ChartAreas[0].CursorY.SetCursorPixelPosition(mousepoint, true);

                        var xValue = BNchart.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                        var yValue = BNchart.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                        Series S = BNchart.Series["T+1"];
                        Series Series_exp = BNchart.Series["ExpiryDay"];
                        DataPoint pPrev = S.Points.Select(x => x).Where(x => x.XValue >= xValue).DefaultIfEmpty(S.Points.First()).First();
                        DataPoint pPrevexp = Series_exp.Points.Select(x => x).Where(x => x.XValue >= xValue).DefaultIfEmpty(Series_exp.Points.First()).First();

                        string value = string.Format("Price:{0},{1}T+1:{2},{3}Expiry:{4}", Math.Round(xValue, 2), Environment.NewLine, Math.Round(pPrev.YValues[0], 2), Environment.NewLine, Math.Round(pPrevexp.YValues[0], 2));
                        tooltipaxis.Show(value, this.BNchart, e.X, e.Y);
                    }
                    else if (result.ChartElementType == ChartElementType.Nothing)
                    {
                        tooltipaxis.Hide(this.BNchart);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void NiftyChart_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var pos = e.Location;
                var results = NiftyChart.HitTest(pos.X, pos.Y, false, ChartElementType.PlottingArea);
                foreach (var result in results)
                {
                    if (result.ChartElementType == ChartElementType.PlottingArea)
                    {
                        Point mousepoint = new Point(e.X, e.Y);

                        NiftyChart.ChartAreas[0].CursorX.Interval = 0;
                        NiftyChart.ChartAreas[0].CursorY.Interval = 0;
                        NiftyChart.ChartAreas[0].CursorX.SetCursorPixelPosition(mousepoint, true);
                        NiftyChart.ChartAreas[0].CursorY.SetCursorPixelPosition(mousepoint, true);

                        var xValue = NiftyChart.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                        var yValue = NiftyChart.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);
                        Series S = NiftyChart.Series["T+1"];
                        Series Series_exp = NiftyChart.Series["ExpiryDay"];
                        DataPoint pPrev = S.Points.Select(x => x).Where(x => x.XValue >= xValue).DefaultIfEmpty(S.Points.First()).First();
                        DataPoint pPrevexp = Series_exp.Points.Select(x => x).Where(x => x.XValue >= xValue).DefaultIfEmpty(Series_exp.Points.First()).First();

                        string value = string.Format("Price:{0},{1}T+1:{2},{3}Expiry:{4}", Math.Round(xValue, 2), Environment.NewLine, Math.Round(pPrev.YValues[0], 2), Environment.NewLine, Math.Round(pPrevexp.YValues[0], 2));
                        tooltipaxisNifty.Show(value, this.NiftyChart, e.X, e.Y);
                    }
                    else if (result.ChartElementType == ChartElementType.Nothing)
                    {
                        tooltipaxisNifty.Hide(this.NiftyChart);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}
