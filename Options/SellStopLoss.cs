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
    public partial class SellStopLoss : Form
    {
        public SellStopLoss()
        {
            InitializeComponent();
            this.KeyPreview = true;
            KeyPress += new KeyPressEventHandler(SellStopLoss_KeyPress);
        }

        void SellStopLoss_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._SellStopLoss = null;
                Close();
            }

        }

        private void SellStopLoss_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;

            lblUniqueId.Text = watch.uniqueId.ToString();

            txtSell_TriggerPrice.Text = Convert.ToString(watch.TGSellPrice);
            txtSell_ActualPrice.Text = Convert.ToString(watch.AP_SellSL);
            txtSell_SLQty.Text = Convert.ToString(watch.SL_SellQty);

            txtSell_TriggerPrice.KeyPress += new KeyPressEventHandler(txtSell_TriggerPrice_KeyPress);
            txtSell_ActualPrice.KeyPress += new KeyPressEventHandler(txtSell_ActualPrice_KeyPress);
            txtSell_SLQty.KeyPress += new KeyPressEventHandler(txtSell_SLQty_KeyPress);
        }

        void txtSell_SLQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar == '.' && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        void txtSell_ActualPrice_KeyPress(object sender, KeyPressEventArgs e)
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

        void txtSell_TriggerPrice_KeyPress(object sender, KeyPressEventArgs e)
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
            watch.SL_SellOrderflg = false;

            watch.TGSellPrice = 999999;
            watch.AP_SellSL = 999999;
            watch.SL_SellQty = 0;
            

            watch.RowData.Cells[WatchConst.TGSellPrice].Value = watch.TGSellPrice;
            watch.RowData.Cells[WatchConst.AP_SellSL].Value = watch.AP_SellSL;
            watch.RowData.Cells[WatchConst.SL_SellQty].Value = watch.SL_SellQty;

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

                if (Convert.ToDouble(txtSell_TriggerPrice.Text) == 0 || Convert.ToInt32(txtSell_SLQty.Text) == 0 || Convert.ToDouble(txtSell_ActualPrice.Text) == 0)
                {
                    MessageBox.Show("DrawDown " + "SellTriggerPrice = " + Convert.ToString(txtSell_TriggerPrice.Text) + " SellSLQty = "
                        + Convert.ToString(txtSell_SLQty.Text) + " SellActualPrice = " + Convert.ToString(txtSell_ActualPrice.Text));
                    return;
                }
                else
                {
                    double _tgSPrice = Convert.ToDouble(txtSell_TriggerPrice.Text);
                    double _apSSL = Convert.ToDouble(txtSell_ActualPrice.Text);
                    int _SQtySL = Convert.ToInt32(txtSell_SLQty.Text);

                    if (_tgSPrice < _apSSL)
                    {
                        TransactionWatch.ErrorMessage("SellStopLossOrder|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                      watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + _tgSPrice + "|" + _apSSL);
                        MessageBox.Show("Trigger Price Should be less than Actual Price");
                    }
                    else
                    {
                        watch.TGSellPrice = _tgSPrice;
                        watch.AP_SellSL = _apSSL;
                        watch.SL_SellQty = _SQtySL;
                        watch.SL_SellOrderflg = true;

                        watch.RowData.Cells[WatchConst.TGSellPrice].Value = watch.TGSellPrice;
                        watch.RowData.Cells[WatchConst.AP_SellSL].Value = watch.AP_SellSL;
                        watch.RowData.Cells[WatchConst.SL_SellQty].Value = watch.SL_SellQty;

                        //AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.MediumSpringGreen;

                        AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.Unique].Style.BackColor = Color.MediumSpringGreen;
                        //AppGlobal.frmWatch.dgvMarketWatch.

                    }
                }
            }
        }

        private void SellStopLoss_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._SellStopLoss = null;
        }
    }
}
