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
    public partial class DD_SellParameter : Form
    {
        public DD_SellParameter()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyPress += new KeyPressEventHandler(DD_SellParameter_KeyPress);
        }

        void DD_SellParameter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._dd_SellParameter = null;
                Close();
            }
        }

        private void DD_SellParameter_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._dd_SellParameter = null;

            txtDD_BM_Sell.KeyPress += new KeyPressEventHandler(txtDD_BM_Sell_KeyPress);
            txtDD_SellQty.KeyPress += new KeyPressEventHandler(txtDD_SellQty_KeyPress);
        }

        void txtDD_SellQty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar == '.' && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        void txtDD_BM_Sell_KeyPress(object sender, KeyPressEventArgs e)
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

        private void DD_SellParameter_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;

            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;
            lblUniqueId.Text = watch.uniqueId.ToString();


            txtDD_BM_Sell.Text = Convert.ToString(watch.DD_bm_Sell);
            txtDD_SellQty.Text = Convert.ToString(watch.DD_SellQty);
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



                if (Convert.ToDouble(txtDD_BM_Sell.Text) == 0 || Convert.ToInt32(txtDD_SellQty.Text) == 0)
                {
                    MessageBox.Show("DrawDown " + "Sell BranchMark = " + txtDD_BM_Sell.Text + "Sell Qty = " + txtDD_SellQty.Text);
                    return;
                }
                else
                {
                    watch.DD_bm_Sell = Convert.ToDouble(txtDD_BM_Sell.Text);
                    watch.DD_SellQty = Convert.ToInt32(txtDD_SellQty.Text);
                    watch.RowData.Cells[WatchConst.DD_bm_Sell].Value = watch.DD_bm_Sell;
                    watch.RowData.Cells[WatchConst.DD_SellQty].Value = watch.DD_SellQty;
                    watch.DD_SellMinPrice = Math.Abs(watch.MktWind);
                    watch.DD_SetMin = watch.DD_SellMinPrice;
                    watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_SellMinPrice;
                    watch.DD_TGSellPrice = watch.DD_SellMinPrice - watch.DD_bm_Sell;
                    watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = GetValueAsTickMultiple(Convert.ToDecimal(watch.DD_TGSellPrice), watch.Leg1);
                    watch.DD_SellOrderflg = true;
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
                watch.DD_SellQty = 0;
                watch.DD_TGSellPrice = 0;
                watch.DD_SellMinPrice = 0;
                watch.DD_SetMin = 0;
                watch.DD_SellOrderflg = false;
                watch.DD_bm_Sell = 0;

                watch.RowData.Cells[WatchConst.DD_bm_Sell].Value = watch.DD_bm_Sell;
                watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = watch.DD_TGSellPrice;
                watch.RowData.Cells[WatchConst.DD_SellQty].Value = watch.DD_SellQty;
                watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_SellMinPrice;


                AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
            }
        }
    }
}
