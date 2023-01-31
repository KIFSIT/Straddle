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
    public partial class Position_Action : Form
    {
        public Position_Action()
        {
            InitializeComponent();
            KeyPress += new KeyPressEventHandler(Position_Action_KeyPress);
        }

        void Position_Action_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._PositionAction = null;
                Close();
            }
        }

        private void Position_Action_Load(object sender, EventArgs e)
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

            if (watch.PremiumAlert)
            {
                chkAlertPremium.Checked = true;
                txtLivePremium.Text = Convert.ToString(watch.Init_Premium);
                txtPremium.Text = Convert.ToString(watch.Init_Premium);
            }
            else
            {
                chkAlertPremium.Checked = false;
                txtLivePremium.Text = Convert.ToString(watch.premium);
                txtPremium.Text = Convert.ToString(watch.premium);
            }

            if (watch.PremiumUserpxAlert)
            {
                PremiumUserPx.Checked = true;
                txtPremium.Enabled = true;
                txtLivePremium.Text = Convert.ToString(watch.premium);
                txtPremium.Text = Convert.ToString(watch.Init_Premium);
            }
            else
            {
                txtPremium.Enabled = false;
                PremiumUserPx.Checked = false;
            }

            if (watch.Premium_indicator == "Point")
            {
                txtPremiumPoint.Text = watch.Premium_dm.ToString();
                cmbPremium.Text = watch.Premium_indicator.ToString();
            }
            else if (watch.Premium_indicator == "Percent")
            {
                txtPremiumPoint.Text = watch.Premium_Percent.ToString();
                cmbPremium.Text = watch.Premium_indicator.ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.uniqueId == Convert.ToUInt64(lblUniqueId.Text))
            {
                if (chkAlertPremium.Checked)
                {

                    watch.PremiumAlert = true;
                    double userPremium = watch.premium;
                    if (PremiumUserPx.Checked)
                    {
                        watch.PremiumUserpxAlert = true;
                        userPremium = Convert.ToDouble(txtPremium.Text);
                    }
                    else
                    {
                        watch.PremiumUserpxAlert = false;
                    }
                    if (cmbPremium.Text == "Point")
                    {
                        watch.Premium_indicator = "Point";
                        watch.Premium_dm = Convert.ToDouble(txtPremiumPoint.Text);
                    }
                    else if (cmbPremium.Text == "Percent")
                    {
                        watch.Premium_indicator = "Percent";
                        watch.Premium_Percent = Convert.ToDouble(txtPremiumPoint.Text);
                        double point = (userPremium * watch.Premium_Percent / 100);
                        watch.Premium_dm = point;
                    }
                    watch.RowData.Cells[WatchConst.Premium_dm].Value = watch.Premium_dm;
                    watch.Init_Premium = userPremium;
                    watch.TG_Premium = watch.Init_Premium - watch.Premium_dm;
                    watch.RowData.Cells[WatchConst.TG_Premium].Value = Math.Round(watch.TG_Premium, 2);
                    watch.RowData.Cells[WatchConst.Init_Premium].Value = Math.Round(watch.Init_Premium, 2);


                    if (chkPremiumTrade.Checked)
                        watch.PremiumTrade = true;
                    else
                        watch.PremiumTrade = false;
                }
                else
                {
                    watch.PremiumAlert = false;
                    watch.PremiumUserpxAlert = false;
                    watch.PremiumTrade = false;
                    watch.Premium_dm = 0;
                    watch.Premium_Percent = 0;
                    watch.Init_Premium = 0;
                    watch.TG_Premium = 0;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();

            watch.PremiumAlert = false;
            watch.PremiumUserpxAlert = false;
            watch.Premium_dm = 0;
            watch.Premium_Percent = 0;
            watch.Init_Premium = 0;
            watch.TG_Premium = 0;
        }

        private void Position_Action_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._PositionAction = null;
        }

        private void PremiumUserPx_CheckedChanged(object sender, EventArgs e)
        {
            if (PremiumUserPx.Checked)
                txtPremium.Enabled = true;
            else
                txtPremium.Enabled = false;
        }
    }
}
