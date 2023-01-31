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
    public partial class LSL_StrangleStopLoss : Form
    {
        public LSL_StrangleStopLoss()
        {
            InitializeComponent();
            this.KeyPreview = true;
            KeyPress += new KeyPressEventHandler(LSL_StrangleStopLoss_KeyPress);
        }

        void LSL_StrangleStopLoss_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)e.KeyChar)
            {
                AppGlobal._LSL_StrangleStopLoss = null;
                Close();
            }
        }

        private void LSL_StrangleStopLoss_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            string Symbol = watch.Leg1.ContractInfo.Symbol;
            string Strike = watch.Leg1.ContractInfo.StrikePrice.ToString();
            string Series = watch.Leg1.ContractInfo.Series;
            lblUniqueId.Text = watch.uniqueId.ToString();
            txtLSL_StrategyPercent.Text = Convert.ToString(watch.LSL_StopLossPercent);

            if (watch.StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo != "0")
            {
                StrategyInfo.Text = "LSL_Strangle_Strategy";
            }
            else
            {
                StrategyInfo.Text = "LSL_Strangle_Leg";
                LegsInfo.Text = Symbol + Strike + Series; 
            }          
        }

        private void LSL_StrangleStopLoss_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._LSL_StrangleStopLoss = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.uniqueId == Convert.ToUInt64(lblUniqueId.Text))
            {
                if (watch.StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo == "0")
                {
                    watch.LSL_StopLossFlg = true;
                    double LSL_StrategyPercent = Convert.ToDouble(txtLSL_StrategyPercent.Text);
                    watch.LSL_StopLossPercent = LSL_StrategyPercent;
                    watch.RowData.Cells[WatchConst.LSL_StrategyPercent].Value = watch.LSL_StopLossPercent;
                    watch.LSL_StopLossValue = (watch.Leg1.N_Price + (watch.Leg1.N_Price * watch.LSL_StopLossPercent / 100));
                    watch.RowData.Cells[WatchConst.LSL_StrategyValue].Value = watch.LSL_StopLossValue;
                }
                else if (watch.StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo != "0")
                {
                    watch.LSL_StopLossFlg = true;
                    double LSL_StrategyPrecent = Convert.ToDouble(txtLSL_StrategyPercent.Text);
                    watch.LSL_StopLossPercent = LSL_StrategyPrecent;
                    watch.RowData.Cells[WatchConst.LSL_StrategyPercent].Value = watch.LSL_StopLossPercent;
                    watch.LSL_StopLossValue = (watch.StrategyAvgPrice * watch.LSL_StopLossPercent / 100);
                    watch.RowData.Cells[WatchConst.LSL_StrategyValue].Value = watch.LSL_StopLossValue;
                }
            }
        }
    }
}
