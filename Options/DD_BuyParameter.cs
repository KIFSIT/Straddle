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

namespace Straddle
{
    public partial class DD_BuyParameter : Form
    {
        public DD_BuyParameter()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyPress += new KeyPressEventHandler(DD_BuyParameter_KeyPress);

        }

        void DD_BuyParameter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._dd_BuyParameter = null;
                Close();
            }
        }

        private void DD_BuyParameter_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;

            lblUniqueId.Text = watch.uniqueId.ToString();

            txtDD_BM_Buy.Text = Convert.ToString(watch.DD_bm_Buy);
            txtDD_BuyQty.Text = Convert.ToString(watch.DD_BuyQty);


            txtDD_BM_Buy.KeyPress += new KeyPressEventHandler(txtDD_BM_Buy_KeyPress);
            txtDD_BuyQty.KeyPress += new KeyPressEventHandler(txtDD_BuyQty_KeyPress);
        }

        void txtDD_BuyQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar == '.' && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            //if (e.KeyChar == '.'
            //&& (sender as TextBox).Text.IndexOf('.') > -1)
            //{
            //    e.Handled = true;
            //}
        }

        void txtDD_BM_Buy_KeyPress(object sender, KeyPressEventArgs e)
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

        private void DD_BuyParameter_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._dd_BuyParameter = null;
        }

        public static decimal GetValueAsTickMultiple(decimal value, Straddle.AppClasses.Leg Leg1)
        {
            if (Leg1.ContractInfo.PriceDivisor != MTConstant.PriceDivisor100)
                return Math.Round(Math.Round(value / Leg1.ContDetail.PriceTick) * Leg1.ContDetail.PriceTick, 4);
            return Math.Round(Math.Round(value / Leg1.ContDetail.PriceTick) * Leg1.ContDetail.PriceTick, 2);
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
                if (Convert.ToDouble(txtDD_BM_Buy.Text) == 0 || Convert.ToInt32(txtDD_BuyQty.Text) == 0)
                {
                    MessageBox.Show("DrawDown " + "Buy BranchMark = " + txtDD_BM_Buy.Text + "Buy Qty = " + txtDD_BuyQty.Text);
                    return;
                }
                else
                {
                    watch.DD_BuyQty = Convert.ToInt32(txtDD_BuyQty.Text);
                    watch.DD_bm_Buy = Convert.ToDouble(txtDD_BM_Buy.Text);
                    watch.RowData.Cells[WatchConst.DD_bm_Buy].Value = watch.DD_bm_Buy;
                    watch.RowData.Cells[WatchConst.DD_BuyQty].Value = watch.DD_BuyQty;
                    watch.DD_BuyMaxPrice = Math.Abs(watch.MktunWind);
                    watch.DD_SetMax = watch.DD_BuyMaxPrice;
                    watch.RowData.Cells[WatchConst.DD_MinBuy].Value = watch.DD_BuyMaxPrice;
                    watch.DD_TGBuyPrice = watch.DD_BuyMaxPrice + watch.DD_bm_Buy;
                    watch.RowData.Cells[WatchConst.DD_TGBuyPrice].Value = GetValueAsTickMultiple(Convert.ToDecimal(watch.DD_TGBuyPrice), watch.Leg1);
                    watch.DD_BuyOrderflg = true;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.MediumSeaGreen;
                }
            }
            else
            {
                MessageBox.Show("Please check Rule");
               
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.uniqueId == Convert.ToUInt64(lblUniqueId.Text))
            {
                watch.DD_BuyQty = 0;
                watch.DD_TGBuyPrice = 0;
                watch.DD_BuyMaxPrice = 0;
                watch.DD_SetMax = 0;
                watch.DD_BuyOrderflg = false;
                watch.DD_bm_Buy = 0;
                watch.RowData.Cells[WatchConst.DD_bm_Buy].Value = watch.DD_bm_Buy;
                watch.RowData.Cells[WatchConst.DD_TGBuyPrice].Value = watch.DD_TGBuyPrice;
                watch.RowData.Cells[WatchConst.DD_BuyQty].Value = watch.DD_BuyQty;
                watch.RowData.Cells[WatchConst.DD_MinBuy].Value = watch.DD_BuyMaxPrice;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
            }
            
        }



    }
}
