using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;
using ArisDev;

namespace Straddle
{
    public partial class StraddleSellStopLoss : Form
    {
        public StraddleSellStopLoss()
        {
            InitializeComponent();

            this.KeyPreview = true;
            KeyPress += new KeyPressEventHandler(StraddleSellStopLoss_KeyPress);
        }

        private void StraddleSellStopLoss_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;
            lblUniqueId.Text = watch.uniqueId.ToString();
            if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 1113 || watch.StrategyId == 1114)
            {
                lblStrike1.Text = watch.Leg2.ContractInfo.StrikePrice.ToString();
                lblSeries1.Text = watch.Leg2.ContractInfo.Series.ToString();
            }
            if (watch.Alert)
            {
                chkAlert.Checked = true;
                txtPrice.Text = Convert.ToString(Math.Abs(watch.DD_InitialSellPrice));
                txtPrice1.Text = Convert.ToString(Math.Abs(watch.DD_InitialSellPrice));
            }
            else
            {
                chkAlert.Checked = false;
                txtPrice.Text = Convert.ToString(watch.MktunWind);
                txtPrice1.Text = Convert.ToString(watch.MktunWind);
            }
            //txtSell_Point.Text = watch.DD_bm_Sell.ToString();


            if (watch.DD_Sell_indicator == "Percent")
            {
                txtSell_Point.Text = watch.DD_bm_Sell_Percent.ToString();
                cmbStoploss.Text = watch.DD_Sell_indicator;
            }
            else if (watch.DD_Sell_indicator == "Point")
            {
                txtSell_Point.Text = watch.DD_bm_Sell.ToString();
                cmbStoploss.Text = watch.DD_Sell_indicator;
            }


            if (watch.BuyAlert)
            {
                chkBuyAlert.Checked = true;
                txtBuyPrice.Text = Convert.ToString(Math.Abs(watch.DD_InitialBuyPrice));
                txtBuyPrice1.Text = Convert.ToString(Math.Abs(watch.DD_InitialBuyPrice));
            }
            else
            {
                chkBuyAlert.Checked = false;
                txtBuyPrice.Text = Convert.ToString(Math.Abs(watch.MktWind));
                txtBuyPrice1.Text = Convert.ToString(Math.Abs(watch.MktWind));
            }
            //txtBuy_Point.Text = watch.DD_bm_Buy.ToString();
            txtPrice.Enabled = false;
            txtBuyPrice.Enabled = false;
            txtTrailPrice.Enabled = false;

            if (watch.DD_Buy_indicator == "Percent")
            {
                txtBuy_Point.Text = watch.DD_bm_Buy_Percent.ToString();
                cmbProfit.Text = watch.DD_Buy_indicator;
            }
            else if (watch.DD_Buy_indicator == "Point")
            {
                txtBuy_Point.Text = watch.DD_bm_Buy.ToString();
                cmbProfit.Text = watch.DD_Buy_indicator;
            }

            if (watch.ProfitTrail)
            {
                chkProfitTrail.Checked = true;
                txtTrailPrice.Text = Convert.ToString(Math.Abs(watch.trail_InitialPrice));
                txtTrailPrice1.Text = Convert.ToString(Math.Abs(watch.trail_InitialPrice));
            }
            else
            {
                chkProfitTrail.Checked = false;
                txtTrailPrice.Text = Convert.ToString(Math.Abs(watch.MktunWind));
                txtTrailPrice1.Text = Convert.ToString(Math.Abs(watch.MktunWind));
            }
            txttrailPoint.Text = watch.trail_bm.ToString();
            txtPrice.Text = Convert.ToString(watch.MktunWind);
            txtSell_Point.Select();

            if (watch.trail_indicator == "Percent")
            {
                txttrailPoint.Text = watch.trail_bm_Percent.ToString();
                cmbTrail.Text = watch.trail_indicator;
            }
            else if (watch.trail_indicator == "Point")
            {
                txttrailPoint.Text = watch.trail_bm.ToString();
                cmbTrail.Text = watch.trail_indicator;
            }

            if (watch.userbuy)
            {
                UserPx.Checked = true;
                txtPrice.Text = Convert.ToString(watch.DD_InitialSellPrice);
            }
            else
            {
                UserPx.Checked = false;
                txtPrice.Text = Convert.ToString(watch.MktunWind);
            }

            if (watch.usersell)
            {
                buyUserPx.Checked = true;
                txtBuyPrice.Text = Convert.ToString(watch.DD_InitialBuyPrice);
            }
            else
            {
                buyUserPx.Checked = false;
                txtBuyPrice.Text = Convert.ToString(watch.MktunWind);
            }

            if (watch.usertrail)
            {
                chkTrailPrice.Checked = true;
                txtTrailPrice.Text = Convert.ToString(watch.trail_InitialPrice);
            }
            else
            {
                chkTrailPrice.Checked = false;
                txtTrailPrice.Text = Convert.ToString(watch.MktunWind);
            }


            if (watch.StoplossTrade)
                chkStoplossTrade.Checked = true;
            else
                chkStoplossTrade.Enabled = true;

            if (watch.TrailTrade)
                chkTrailTrade.Checked = true;
            else
                chkTrailTrade.Enabled = true;

            if (watch.ProfitTrade)
                chkProfitBookTrade.Checked = true;
            else
                chkProfitBookTrade.Enabled = true;



            if (watch.SqTimeflg)
            {
                chksqoffTime.Checked = true;
                dtpSqOff.Text = watch.SqTime.ToString();
            }
            else
                chksqoffTime.Checked = false;

            if (watch.SQVegaflg)
            {
                chkVegaSq.Checked = true;
                if (watch.SQVegaType == "Point")
                {
                    cmbVega.Text = "Point";
                    lblVegaLive.Text = Math.Round(watch.Leg1.VegaV,2).ToString();
                    lblVegaPrice.Text = Math.Round(watch.Init_SQVegaPrice,2).ToString();
                    txtSqVega.Text = Math.Round(watch.SQVegaPoint,2).ToString();
                }
                else
                {
                    cmbVega.Text = "Percent";
                    lblVegaLive.Text = Math.Round(watch.Leg1.VegaV,2).ToString();
                    lblVegaPrice.Text = Math.Round(watch.Init_SQVegaPrice,2).ToString();
                    txtSqVega.Text = Math.Round(watch.Per_SQVegaPrice, 2).ToString();
                }
            }
            else
            {
                chkVegaSq.Checked = false;
                lblVegaLive.Text = Math.Round(watch.Leg1.VegaV, 2).ToString();
               
            }

            if (watch.SQPremiumflg)
            {

                chkPremiumflg.Checked = true;
                lblLivePremium.Text = watch.LivePremium.ToString();
                lblPremiumPrice.Text = watch.Init_SQPremiumPrice.ToString();
                txtPremium.Text = Math.Round(watch.SQPremiumPoint, 2).ToString();
               
            }
            else
            {
                chkPremiumflg.Checked = false;
                lblPremiumPrice.Text = watch.premium.ToString();
                lblLivePremium.Text = watch.LivePremium.ToString();
            }
            if (watch.SQLossflg)
            {
                chkLossflg.Checked = true;
                lblLossValue.Text = watch.Init_SQLossPrice.ToString();
            }
            else
            {
                chkLossflg.Checked = false;
                lblLossValue.Text = watch.pnl.ToString();
            }

            cmbVega.Text = watch.SQVegaType;
            cmbPremium.Text = watch.SQPremiumType;
            cmbLoss.Text = watch.SQLossType;


        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;

            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            if (watch.uniqueId == Convert.ToUInt64(lblUniqueId.Text))
            {
                if (chkAlert.Checked == true)
                {
                    watch.DD_SellOrderflg = true;
                    watch.Alert = true;

                    double userPx = watch.MktunWind;
                    if (UserPx.Checked == true)
                    {
                        watch.userbuy = true;
                        userPx = Convert.ToDouble(txtPrice.Text);
                    }
                    else
                    {
                        watch.userbuy = false;
                    }
                    if (cmbStoploss.Text == "Point")
                    {
                        watch.DD_Sell_indicator = "Point";
                        watch.DD_bm_Sell = Convert.ToDouble(txtSell_Point.Text);
                    }
                    else if (cmbStoploss.Text == "Percent")
                    {
                        watch.DD_Sell_indicator = "Percent";
                        watch.DD_bm_Sell_Percent = Convert.ToDouble(txtSell_Point.Text);
                        double point = (userPx * watch.DD_bm_Sell_Percent / 100);
                        watch.DD_bm_Sell = point;
                    }
                    watch.RowData.Cells[WatchConst.DD_bm_Sell].Value = Math.Round(watch.DD_bm_Sell, 2);
                    watch.DD_SellMinPrice = userPx;
                    watch.DD_InitialSellPrice = watch.DD_SellMinPrice;
                    watch.FutPrice = Convert.ToDouble(watch.niftyLeg.LastTradedPrice);

                    watch.DD_SetMin = watch.DD_SellMinPrice;
                    watch.RowData.Cells[WatchConst.DD_MxSell].Value = Math.Round(watch.DD_SellMinPrice, 2);
                    watch.DD_TGSellPrice = watch.DD_SellMinPrice + watch.DD_bm_Sell;
                    watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = Math.Round(watch.DD_TGSellPrice, 2);
                    watch.RowData.Cells[WatchConst.FutPrice].Value = watch.FutPrice;

                    if (chkStoplossTrade.Checked)
                    {
                        watch.StoplossTrade = true;
                    }
                    else
                    {
                        watch.StoplossTrade = false;
                    }
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.Strategy].Style.BackColor = Color.ForestGreen;
                    // TransactionWatch.TransactionMessage("SetSellDrawDown|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMin + "|" + watch.DD_bm_Sell_Percent, Color.Blue);
                    TransactionWatch.ErrorMessage("SetSellDrawDown|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMin + "|" + watch.DD_bm_Sell_Percent + "|" + watch.DD_bm_Sell);

                }
                else
                {
                    watch.Alert = false;
                    watch.userbuy = false;
                    watch.StoplossTrade = false;
                    watch.DD_SellOrderflg = false;
                    watch.DD_Sell_indicator = "Point";
                    watch.DD_TGSellPrice = 0;
                    watch.DD_SellMinPrice = 0;
                    watch.DD_SetMin = 0;
                    watch.DD_bm_Sell = 0;
                    watch.DD_InitialSellPrice = 0;
                    watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = watch.DD_TGSellPrice;
                    watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_SellMinPrice;
                    watch.RowData.Cells[WatchConst.DD_bm_Sell].Value = watch.DD_bm_Sell;
                }
                if (chkBuyAlert.Checked == true)
                {
                    watch.DD_BuyOrderflg = true;
                    watch.BuyAlert = true;

                    double userPx = Math.Abs(watch.MktWind);
                    if (buyUserPx.Checked == true)
                    {
                        watch.usersell = true;
                        userPx = Convert.ToDouble(txtBuyPrice.Text);
                    }
                    else
                        watch.usersell = false;

                    if (cmbProfit.Text == "Point")
                    {
                        watch.DD_Buy_indicator = "Point";
                        watch.DD_bm_Buy = Convert.ToDouble(txtBuy_Point.Text);
                    }
                    else if (cmbProfit.Text == "Percent")
                    {
                        watch.DD_Buy_indicator = "Percent";
                        watch.DD_bm_Buy_Percent = Convert.ToDouble(txtBuy_Point.Text);
                        double point = (userPx * watch.DD_bm_Buy_Percent / 100);
                        watch.DD_bm_Buy = point;
                    }
                    //watch.DD_bm_Buy = Convert.ToDouble(txtBuy_Point.Text);
                    watch.RowData.Cells[WatchConst.DD_bm_Buy].Value = Math.Round(watch.DD_bm_Buy, 2);
                    watch.DD_BuyMaxPrice = userPx;
                    watch.DD_InitialBuyPrice = watch.DD_BuyMaxPrice;

                    watch.DD_SetMax = watch.DD_BuyMaxPrice;
                    watch.RowData.Cells[WatchConst.DD_MinBuy].Value = Math.Round(watch.DD_BuyMaxPrice, 2);
                    watch.DD_TGBuyPrice = watch.DD_BuyMaxPrice - watch.DD_bm_Buy;
                    watch.RowData.Cells[WatchConst.DD_TGBuyPrice].Value = Math.Round(watch.DD_TGBuyPrice, 2);
                    if (chkProfitBookTrade.Checked)
                        watch.ProfitTrade = true;
                    else
                        watch.ProfitTrade = false;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.StrategyName].Style.BackColor = Color.ForestGreen;
                    //TransactionWatch.TransactionMessage("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.DD_BuyMaxPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMax + "|" + watch.DD_bm_Buy_Percent, Color.Blue);
                    TransactionWatch.ErrorMessage("SetBuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.DD_BuyMaxPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMax + "|" + watch.DD_bm_Buy_Percent + "|" + watch.DD_bm_Buy);
                }
                else
                {
                    watch.BuyAlert = false;
                    watch.usersell = false;
                    watch.DD_BuyOrderflg = false;
                    watch.ProfitTrade = false;

                    watch.DD_Buy_indicator = "Point";
                    watch.DD_TGBuyPrice = 0;
                    watch.DD_BuyMaxPrice = 0;
                    watch.DD_SetMax = 0;
                    watch.DD_bm_Buy = 0;
                    watch.DD_InitialBuyPrice = 0;

                    watch.RowData.Cells[WatchConst.DD_TGBuyPrice].Value = watch.DD_TGBuyPrice;
                    watch.RowData.Cells[WatchConst.DD_MinBuy].Value = watch.DD_BuyMaxPrice;
                    watch.RowData.Cells[WatchConst.DD_bm_Buy].Value = watch.DD_bm_Buy;
                }
                if (chkProfitTrail.Checked)
                {
                    watch.ProfitTrail = true;
                    double userPx = Math.Abs(watch.MktunWind);
                    if (chkTrailPrice.Checked == true)
                    {
                        watch.usertrail = true;
                        userPx = Convert.ToDouble(txtTrailPrice.Text);
                        watch.UserPriceflg = true;
                        watch.TrailingStart = false;
                        AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.L1Strike].Style.BackColor = Color.ForestGreen;
                    }
                    else
                    {
                        watch.usertrail = false;
                        watch.UserPriceflg = false;
                        watch.TrailingStart = true;
                        AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.L1Strike].Style.BackColor = Color.YellowGreen;
                    }

                    if (cmbTrail.Text == "Point")
                    {
                        watch.trail_indicator = "Point";
                        watch.trail_bm = Convert.ToDouble(txttrailPoint.Text);
                    }
                    else if (cmbTrail.Text == "Percent")
                    {
                        watch.trail_indicator = "Percent";
                        watch.trail_bm_Percent = Convert.ToDouble(txttrailPoint.Text);
                        double point = (userPx * watch.trail_bm_Percent / 100);
                        watch.trail_bm = point;
                    }
                    watch.RowData.Cells[WatchConst.trail_bm].Value = Math.Round(watch.trail_bm, 2);
                    watch.trail_MinPrice = userPx;
                    watch.trail_InitialPrice = watch.trail_MinPrice;

                    watch.trail_SetMax = watch.trail_MinPrice;
                    watch.RowData.Cells[WatchConst.trail_Mx].Value = Math.Round(watch.trail_MinPrice, 2);

                    watch.trail_TGPrice = watch.trail_MinPrice + watch.trail_bm;
                    watch.RowData.Cells[WatchConst.trail_TGPrice].Value = Math.Round(watch.trail_TGPrice, 2);

                    if (chkTrailTrade.Checked)
                        watch.TrailTrade = true;
                    else
                        watch.TrailTrade = false;
                    TransactionWatch.ErrorMessage("SetBuyTrailBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.trail_TGPrice + "|" + watch.trail_MinPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.trail_SetMax + "|" + watch.trail_bm_Percent + "|" + watch.trail_bm);
                }
                else
                {
                    watch.ProfitTrail = false;
                    watch.UserPriceflg = false;
                    watch.usertrail = false;
                    watch.TrailTrade = false;
                    watch.trail_indicator = "Point";

                    watch.trail_TGPrice = 0;
                    watch.trail_MinPrice = 0;
                    watch.trail_SetMax = 0;
                    watch.trail_bm = 0;
                    watch.trail_InitialPrice = 0;
                    watch.RowData.Cells[WatchConst.trail_TGPrice].Value = watch.trail_TGPrice;
                    watch.RowData.Cells[WatchConst.trail_Mx].Value = watch.trail_MinPrice;
                    watch.RowData.Cells[WatchConst.trail_bm].Value = watch.trail_SetMax;
                }
                if (chksqoffTime.Checked == true)
                {
                    DateTime str = Convert.ToDateTime(dtpSqOff.Text);

                    string strTime = str.ToString("HH:mm:ss");
                    UInt64 uintTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(str));
                    UInt64 nowTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                    if (uintTime < nowTime)
                    {
                        MessageBox.Show("Time should be less than current time");
                        return;
                    }
                    else
                    {
                        watch.SqTimeflg = true;
                        watch.SqTime = strTime;
                        watch.RowData.Cells[WatchConst.SQ_Time].Value = watch.SqTime;
                    }
                }
                else
                {
                    watch.SqTimeflg = false;
                }

                if (chkVegaSq.Checked == true)
                {
                    watch.SQVegaflg = true;
                    if (cmbVega.Text == "Point")
                    {
                        watch.SQVegaType = "Point";
                        watch.SQVegaPoint = Math.Round(Convert.ToDouble(txtSqVega.Text),2);
                        watch.SQVegaPrice = Math.Round((Convert.ToDouble(lblVegaLive.Text)) - (Convert.ToDouble(txtSqVega.Text)),2);
                        watch.Init_SQVegaPrice = Math.Round(watch.SQVegaPrice,2);

                    }
                    else
                    {
                        watch.SQVegaType = "Percent";
                        watch.Per_SQVegaPrice = Convert.ToDouble(txtSqVega.Text);
                        watch.SQVegaPoint = Math.Round(Math.Abs(Convert.ToDouble(lblVegaLive.Text)) * (watch.Per_SQVegaPrice / 100),2);
                        watch.SQVegaPrice = Math.Round((Convert.ToDouble(lblVegaLive.Text)) - (Convert.ToDouble(watch.SQVegaPoint)),2);
                        watch.Init_SQVegaPrice = Math.Round(watch.SQVegaPrice,2);
                    }
                }
                else
                {
                    watch.SQVegaflg = false;
                    watch.SQVegaType = "Point";
                    watch.SQVegaPrice = 0;
                    watch.Init_SQVegaPrice = 0;
                }


                if (chkPremiumflg.Checked == true)
                {
                    if (watch.posInt < 0)
                    {
                        watch.SQPremiumflg = true;
                        if (cmbPremium.Text == "Point")
                        {
                            watch.SQPremiumType = "Point";
                            watch.SQPremiumPoint = Convert.ToDouble(txtPremium.Text);
                            watch.SQPremiumPrice = Math.Round((Convert.ToDouble(lblPremiumPrice.Text)) - (Convert.ToDouble(txtPremium.Text)), 2);
                            watch.Init_SQPremiumPrice = Convert.ToDouble(watch.SQPremiumPrice);

                        }
                        else
                        {
                            watch.SQPremiumType = "Percent";
                            watch.Per_SQPremiumPrice = Convert.ToDouble(txtPremium.Text);
                            watch.SQPremiumPoint = Math.Round(Math.Abs(Convert.ToDouble(lblPremiumPrice.Text)) * (watch.Per_SQPremiumPrice / 100),2);
                            watch.SQPremiumPrice = Math.Round((Convert.ToDouble(lblPremiumPrice.Text)) - Convert.ToDouble(watch.SQPremiumPoint), 2);
                            watch.Init_SQPremiumPrice = Math.Round(watch.Init_SQPremiumPrice,2);

                        }
                    }
                }
                else
                {
                    watch.SQPremiumflg = false;
                    watch.SQPremiumType = "Point";
                    watch.SQPremiumPrice = 0;
                    watch.Init_SQPremiumPrice = 0;
                }


                if (chkLossflg.Checked == true)
                {
                    watch.SQLossflg = true;
                    if (cmbLoss.Text == "Point")
                    {
                        watch.SQLossType = "Point";
                        watch.SQLossPrice = Convert.ToDouble(txtLoss.Text);
                        watch.Init_SQLossPrice = Convert.ToDouble(txtLoss.Text);

                    }
                    else
                    {
                        watch.SQLossType = "Percent";
                        watch.Per_SQLossPrice = Convert.ToDouble(txtLoss.Text);

                    }
                }
                else
                {
                    watch.SQLossflg = false;
                    watch.SQLossType = "Point";
                    watch.SQLossPrice = 0;
                    watch.Init_SQLossPrice = 0;
                    watch.Per_SQLossPrice = 0;
                }

            }
            else
            {
                MessageBox.Show("Please select Proper rule " + watch.uniqueId + " | " + Convert.ToUInt64(lblUniqueId.Text));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.uniqueId == Convert.ToUInt64(lblUniqueId.Text))
            {
                watch.DD_SellQty = 0;
                watch.DD_TGSellPrice = 0;
                watch.DD_SellMinPrice = 0;
                watch.DD_SetMin = 0;
                watch.DD_SellOrderflg = false;
                watch.DD_bm_Sell = 0;
                watch.FutPrice = 0;
               
                watch.DD_TGBuyPrice = 0;
                watch.DD_BuyMaxPrice = 0;
                watch.DD_SetMax = 0;
                watch.DD_BuyOrderflg = false;
                watch.DD_bm_Buy = 0;              
                watch.ProfitTrail = false;
                watch.UserPriceflg = false;
                watch.trail_TGPrice = 0;
                watch.trail_MinPrice = 0;
                watch.trail_SetMax = 0;
                watch.trail_bm = 0;

                watch.Alert = false;
                watch.BuyAlert = false;
                watch.ProfitTrail = false;
                watch.SqTimeflg = false;

                watch.SQPremiumflg = false;
                watch.SQVegaflg = false;
                watch.SQLossflg = false;

                watch.SQVegaPrice = 0;
                watch.Init_SQVegaPrice = 0;
                watch.Per_SQVegaPrice = 0;

                watch.SQPremiumPrice = 0;
                watch.Init_SQPremiumPrice = 0;
                watch.Per_SQPremiumPrice = 0;

                watch.SQLossPrice = 0;
                watch.Init_SQLossPrice = 0;
                watch.Per_SQLossPrice = 0;


                
                watch.RowData.Cells[WatchConst.DD_bm_Buy].Value = watch.DD_bm_Buy;
                watch.RowData.Cells[WatchConst.DD_TGBuyPrice].Value = watch.DD_TGBuyPrice;
                watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_BuyMaxPrice;
                watch.RowData.Cells[WatchConst.DD_bm_Sell].Value = watch.DD_bm_Sell;
                watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = watch.DD_TGSellPrice;                
                watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_SellMinPrice;
                watch.RowData.Cells[WatchConst.FutPrice].Value = watch.FutPrice;
                watch.RowData.Cells[WatchConst.trail_TGPrice].Value = watch.trail_TGPrice;
                watch.RowData.Cells[WatchConst.trail_Mx].Value = watch.trail_MinPrice;
                watch.RowData.Cells[WatchConst.trail_bm].Value = watch.trail_SetMax;

                TransactionWatch.ErrorMessage("SelectOff|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series);

            }
        }

        private void StraddleSellStopLoss_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._straddleSellStopLoss = null;
        }

        private void UserPx_CheckedChanged(object sender, EventArgs e)
        {
            if (UserPx.Checked == true) 
                txtPrice.Enabled = true;
            else
                txtPrice.Enabled = false; 
        }       

        private void buyUserPx_CheckedChanged(object sender, EventArgs e)
        {
            if (buyUserPx.Checked == true)
                txtBuyPrice.Enabled = true;
            else
                txtBuyPrice.Enabled = false;
        }

        private void chkTrailPrice_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTrailPrice.Checked)
                txtTrailPrice.Enabled = true;
            else
                txtTrailPrice.Enabled = false;
        }

        private void txtSell_Point_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '.' && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '.' && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void txtBuy_Point_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '.' && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void txtBuyPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '.' && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void txttrailPoint_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '.' && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void txtTrailPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '.' && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void StraddleSellStopLoss_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._straddleSellStopLoss = null;
                Close();
            }
        }        
    }
}
