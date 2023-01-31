using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;

namespace Straddle
{
    public partial class Initial_Trailing : Form
    {
        public Initial_Trailing()
        {
            InitializeComponent();
            this.KeyPreview = true;
            KeyPress += new KeyPressEventHandler(Initial_Trailing_KeyPress);
        }

        void Initial_Trailing_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._Initial_Trailing = null;
                Close();
            }
        }

        private void Initial_Trailing_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;
            lblUniqueId.Text = watch.uniqueId.ToString();

            AppGlobal.Unique = Convert.ToUInt64(watch.uniqueId);


            if (watch.I_Trailingflg)
            {
                chkTrailing.Checked = true;
                txtLivePrice.Text = Convert.ToString(watch.MktunWind);
                if (watch.I_UserPxTrailingflg)
                {
                    chkTrailingUserPrice.Checked = true;
                    txtLivePrice.Text = Convert.ToString(watch.MktunWind);
                    txtUserPrice.Text = Convert.ToString(watch.I_TrailingInitial);

                    if (watch.I_TrailingSide != "None")
                    {
                        cmbTrailingSide.Text = Convert.ToString(watch.I_TrailingSide);
                    }
                    txtTrailingPoint.Text = Convert.ToString(watch.I_TrailingPoint);
                    if (watch.I_TrailingTradeflg)
                    {
                        chkTrailingTrade.Checked = true;
                    }
                    else
                    {
                        chkTrailingTrade.Checked = false;
                    }
                }
            }
            else
            {
                chkTrailing.Checked = false;
                chkTrailingUserPrice.Checked = false;
                chkTrailingTrade.Checked = false;
                txtUserPrice.Enabled = false;
                txtLivePrice.Text = Convert.ToString(watch.MktunWind);
                txtUserPrice.Text = Convert.ToString(watch.MktunWind);
                cmbTrailingSide.Text = "BUY";
            }

            if (watch.I_Priceflg)
            {
                chkPrice.Checked = true;
                if (watch.I_UserPriceflg)
                {
                    chkPriceUser.Checked = true;
                    txtPricePx.Text = watch.I_Price.ToString();
                    if (watch.I_PriceSide != "None")
                    {
                        cmbPriceSide.Text = Convert.ToString(watch.I_PriceSide);
                    }
                    if (watch.I_PriceTrade)
                    {
                        chkPriceTrade.Checked = true;
                        txtPriceQty.Text = watch.I_PriceQty.ToString();
                    }
                    else
                    {
                        chkPriceTrade.Checked = false;
                        txtPriceQty.Text = "0";
                    }
                }
            }
            else
            {
                chkPrice.Checked = false;
                chkPriceUser.Checked = false;
                chkPriceTrade.Checked = false;
                txtPricePx.Enabled = false;
                txtPricePx.Text = watch.MktunWind.ToString();
                txtPriceQty.Text = "0";
                cmbPriceSide.Text = "BUY";    
            }
            if (watch.iteratorflg)
            {
                chkLevel.Checked = true;
                txtIteratior.Text = watch.iterator.ToString();
                cmdLevelSide.Text = watch.iteratorSide.ToString();
                if (watch.itreatorTradeflg)
                    chkIteratorTrade.Checked = true;
                else
                    chkIteratorTrade.Checked = false;

            }
            else
            {
                chkLevel.Checked = false;
                txtIteratior.Text = watch.iterator.ToString();
                cmdLevelSide.Text = "BUY";
                chkIteratorTrade.Checked = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            if (chkTrailing.Checked == true)
            {
                watch.I_Trailingflg = true;
                watch.I_TrailingSide = Convert.ToString(cmbTrailingSide.Text);
                watch.I_TrailingPoint = Convert.ToDouble(txtTrailingPoint.Text);
                if (chkTrailingUserPrice.Checked)
                {
                    watch.I_UserPxTrailingflg = true;
                    watch.I_TrailingInitial = Convert.ToDouble(txtUserPrice.Text);
                    if (Convert.ToString(cmbTrailingSide.Text) == "BUY")
                    {
                        watch.I_TrailingMinMaxPrice = watch.I_TrailingInitial;
                        watch.I_TrailingTriggerPx = watch.I_TrailingInitial + watch.I_TrailingPoint;
                    }
                    else
                    {
                        watch.I_TrailingMinMaxPrice = watch.I_TrailingInitial;
                        watch.I_TrailingTriggerPx = watch.I_TrailingInitial - watch.I_TrailingPoint;
                    }
                }
                else
                {
                    watch.I_TrailingInitial = Convert.ToDouble(txtLivePrice.Text);
                    if (Convert.ToString(cmbTrailingSide.Text) == "BUY")
                    {
                        watch.I_TrailingMinMaxPrice = watch.I_TrailingInitial;
                        watch.I_TrailingTriggerPx = watch.I_TrailingInitial + watch.I_TrailingPoint;
                    }
                    else
                    {
                        watch.I_TrailingMinMaxPrice = watch.I_TrailingInitial;
                        watch.I_TrailingTriggerPx = watch.I_TrailingInitial - watch.I_TrailingPoint;
                    }
                }
                if (chkTrailingTrade.Checked)
                {
                    watch.I_TrailingTradeflg = true;
                    watch.I_TrailingQty = Convert.ToInt32(txtTrailingQty.Text);
                }
                else
                {
                    watch.I_TrailingTradeflg = false;
                    watch.I_TrailingQty = 0;
                }

                watch.RowData.Cells[WatchConst.Init_TrailingPx].Value = watch.I_TrailingInitial;
                watch.RowData.Cells[WatchConst.Init_TrailingMx].Value = watch.I_TrailingMinMaxPrice;
                watch.RowData.Cells[WatchConst.Init_TrailingPt].Value = watch.I_TrailingPoint;
                watch.RowData.Cells[WatchConst.Init_TrailingTg].Value = watch.I_TrailingTriggerPx;
            }
            else
            {
                watch.I_Trailingflg = false;
                watch.I_UserPxTrailingflg = false;
                watch.I_TrailingTradeflg = false;
                watch.I_TrailingInitial = 0;
                watch.I_TrailingMinMaxPrice = 0;
                watch.I_TrailingPoint = 0;
                watch.I_TrailingTriggerPx = 0;
                watch.I_TrailingQty = 0;
                watch.I_TrailingSide = "None";

                watch.RowData.Cells[WatchConst.Init_TrailingPx].Value = watch.I_TrailingInitial;
                watch.RowData.Cells[WatchConst.Init_TrailingMx].Value = watch.I_TrailingMinMaxPrice;
                watch.RowData.Cells[WatchConst.Init_TrailingPt].Value = watch.I_TrailingPoint;
                watch.RowData.Cells[WatchConst.Init_TrailingTg].Value = watch.I_TrailingTriggerPx;
            }
        }

        private void txtUserPrice_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtTrailingPoint_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtTrailingQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar == '.' && !char.IsDigit(e.KeyChar) && e.KeyChar == '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void chkTrailingUserPrice_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTrailingUserPrice.Checked == true)
                txtUserPrice.Enabled = true;
            else
                txtUserPrice.Enabled = false;
            
        }

        private void Initial_Trailing_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._Initial_Trailing = null;
        }

        private void txtPricePx_KeyPress(object sender, KeyPressEventArgs e)
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

        private void txtPriceQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar == '.' && !char.IsDigit(e.KeyChar) && e.KeyChar == '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            if (chkPrice.Checked)
            {
                watch.I_Priceflg = true;
                watch.I_PriceSide = Convert.ToString(cmbPriceSide.Text);
                watch.I_Price = Convert.ToDouble(txtPricePx.Text);
                if (chkPriceUser.Checked)
                    watch.I_UserPriceflg = true;
                else
                    watch.I_UserPriceflg = false;

                watch.I_PriceQty = Convert.ToInt32(txtPriceQty.Text);

                if (chkPriceTrade.Checked)
                    watch.I_PriceTrade = true;
                else
                    watch.I_PriceTrade = false;
            }
            else
            {
                watch.I_Priceflg = false;
                watch.I_UserPriceflg = false;
                watch.I_PriceTrade = false;
                watch.I_Price = 0;
                watch.I_PriceSide = "None";

                chkPrice.Checked = false;
                txtPricePx.Text = watch.MktunWind.ToString();
                chkPriceUser.Checked = false;
                chkPriceTrade.Checked = false;
                txtPriceQty.Text = "0";
                cmbPriceSide.Text = "BUY";
            }
            
            TransactionWatch.ErrorMessage("SetPriceTrade|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Priceflg|" + watch.I_Priceflg + "|Userflg|" + watch.I_UserPriceflg + "|PriceTradeflg|" + watch.I_PriceTrade
                                          + "|PriceSide|" + watch.I_PriceSide + "|Price|" + watch.I_Price + "|PriceQty|" + watch.I_PriceQty);
            TransactionWatch.TransactionMessage("SetPriceTrade|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Priceflg|" + watch.I_Priceflg + "|Userflg|" + watch.I_UserPriceflg + "|PriceTradeflg|" + watch.I_PriceTrade
                                          + "|PriceSide|" + watch.I_PriceSide + "|Price|" + watch.I_Price + "|PriceQty|" + watch.I_PriceQty, Color.Blue);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            watch.I_Priceflg = false;
            watch.I_UserPriceflg = false;
            watch.I_PriceTrade = false;
            watch.I_Price = 0;
            watch.I_PriceSide = "None";


            chkPrice.Checked = false;
            txtPricePx.Text = watch.MktunWind.ToString();
            chkPriceUser.Checked = false;
            chkPriceTrade.Checked = false;
            txtPriceQty.Text = "0";
            cmbPriceSide.Text = "BUY";


            TransactionWatch.TransactionMessage("SetPriceTrade|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Priceflg|" + watch.I_Priceflg + "|Userflg|" + watch.I_UserPriceflg + "|PriceTradeflg|" + watch.I_PriceTrade
                                          + "|PriceSide|" + watch.I_PriceSide + "|Price|" + watch.I_Price + "|PriceQty|" + watch.I_PriceQty, Color.Blue);
        }

        private void chkPriceUser_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPriceUser.Checked)
                txtPricePx.Enabled = true;
            else
                txtPricePx.Enabled = false;


        }

        private void btnLevel_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (chkLevel.Checked)
            {
                if (txtIteratior.Text == "0")
                {
                    MessageBox.Show("Please mention level in Textbox");
                    return;
                }
                watch.iterator = Convert.ToInt32(txtIteratior.Text);
                watch.iteratorSide = Convert.ToString(cmdLevelSide.Text);
                

                if (chkIteratorTrade.Checked)
                    watch.itreatorTradeflg = true;
                else
                    watch.itreatorTradeflg = false;

                if (AppGlobal._ParameterInput == null)
                {
                    AppGlobal._ParameterInput = new ParameterInput();
                    AppGlobal._ParameterInput.Show();
                }
                else
                {
                    AppGlobal._ParameterInput.Show();
                    AppGlobal._ParameterInput.Activate();
                }
            }
            else
            {
                watch.iteratorflg = false;
                watch.itreatorTradeflg = false;
               
                for (int i = 0; i < watch._inputParameter.Count(); i++)
                {
                    watch._inputParameter[i].Lots = 0;
                    watch._inputParameter[i].Price = 0;
                    watch._inputParameter[i].flg = false;
                }
                watch.iterator = 0;
                watch.iteratorCount = 0;
                watch.RowData.Cells[WatchConst.LevelIterator].Value = watch.iteratorCount;

            }

        }
    }
}
