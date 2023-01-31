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
    public partial class BuyStopLoss : Form
    {
        public BuyStopLoss()
        {
            InitializeComponent();
            this.KeyPreview = true;
            KeyPress += new KeyPressEventHandler(BuyStopLoss_KeyPress);
        }

        void BuyStopLoss_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._BuyStopLoss = null;
                Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;

            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            if (watch.uniqueId == Convert.ToUInt64(lblUniqueId.Text))
            {
                if (!watch.IsStrikeReq)
                {
                    MessageBox.Show("Please Strike Request First!!!!");
                    return;
                }

                if (Convert.ToDouble(txtBuy_TriggerPrice.Text) == 0 || Convert.ToInt32(txtBuy_SLQty.Text) == 0 || Convert.ToDouble(txtBuy_ActualPrice.Text) == 0)
                {
                    MessageBox.Show("DrawDown " + "BuyTriggerPrice = " + Convert.ToString(txtBuy_TriggerPrice.Text) + " BuySLQty = " 
                        + Convert.ToString(txtBuy_SLQty.Text) + " BuyActualPrice = " + Convert.ToString(txtBuy_ActualPrice.Text));
                    return;
                }
                else
                {

                    double _tgBPrice = Convert.ToDouble(txtBuy_TriggerPrice.Text);
                    double _apBSL = Convert.ToDouble(txtBuy_ActualPrice.Text);
                    int _BQtySL = Convert.ToInt32(txtBuy_SLQty.Text);
                    if (_tgBPrice > _apBSL)
                    {
                        TransactionWatch.ErrorMessage("BuyStopLossOrder|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                      watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + _tgBPrice + "|" + _apBSL);
                        MessageBox.Show("Trigger Price Should be greater than Actual Price");
                    }
                    else
                    {
                        watch.TGBuyPrice = _tgBPrice;
                        watch.AP_BuySL = _apBSL;
                        watch.SL_BuyQty = _BQtySL;
                        watch.SL_BuyOrderflg = true;

                        watch.RowData.Cells[WatchConst.TGBuyPrice].Value = watch.TGBuyPrice;
                        watch.RowData.Cells[WatchConst.AP_BuySL].Value = watch.AP_BuySL;
                        watch.RowData.Cells[WatchConst.SL_BuyQty].Value = watch.SL_BuyQty;
                        AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.Unique].Style.BackColor = Color.MediumSpringGreen;
                    }
                }
            }

        }

        private void BuyStopLoss_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;

            lblUniqueId.Text = watch.uniqueId.ToString();

            txtBuy_TriggerPrice.Text = Convert.ToString(watch.TGBuyPrice);
            txtBuy_ActualPrice.Text = Convert.ToString(watch.AP_BuySL);
            txtBuy_SLQty.Text = Convert.ToString(watch.SL_BuyQty);

            txtBuy_TriggerPrice.KeyPress += new KeyPressEventHandler(txtBuy_TriggerPrice_KeyPress);
            txtBuy_ActualPrice.KeyPress += new KeyPressEventHandler(txtBuy_ActualPrice_KeyPress);
            txtBuy_SLQty.KeyPress += new KeyPressEventHandler(txtBuy_SLQty_KeyPress);
        }

        void txtBuy_SLQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar == '.' && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        void txtBuy_ActualPrice_KeyPress(object sender, KeyPressEventArgs e)
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

        void txtBuy_TriggerPrice_KeyPress(object sender, KeyPressEventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            //AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
            AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.Unique].Style.BackColor = Color.White;
            watch.SL_BuyOrderflg = false;


            watch.TGBuyPrice = 999999;
            watch.AP_BuySL = 999999;
            watch.SL_BuyQty = 0;
           

            watch.RowData.Cells[WatchConst.TGBuyPrice].Value = watch.TGBuyPrice;
            watch.RowData.Cells[WatchConst.AP_BuySL].Value = watch.AP_BuySL;
            watch.RowData.Cells[WatchConst.SL_BuyQty].Value = watch.SL_BuyQty;
        }

        private void BuyStopLoss_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._BuyStopLoss = null;
        }
    }
}
