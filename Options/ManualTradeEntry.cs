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
    public partial class ManualTradeEntry : Form
    {
        public ManualTradeEntry()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyPress += new KeyPressEventHandler(ManualTradeEntry_KeyPress);
        }

        void ManualTradeEntry_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._manualTrade = null;
                Close();
            }
        }

        private void ManualTradeEntry_Load(object sender, EventArgs e)
        {
            txtiswind.SelectedIndex = 0;
            if (AppGlobal.ManualCount == 2)
            {
                int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
                MarketWatch watch = new MarketWatch();
                watch = AppGlobal.MarketWatch[iRow];
                txtunique.Text = watch.uniqueId.ToString();
                txtFutToken.Text = watch.niftyLeg.ContractInfo.TokenNo.ToString();
                lblStrategy.Text = watch.StrategyId.ToString();
            }
        }

        private void ManualTradeEntry_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._manualTrade = null;
        }

        private void btnsend_Click(object sender, EventArgs e)
        {
            UInt64 UniqueId = Convert.ToUInt64(txtunique.Text);
            string iswind = Convert.ToString(txtiswind.Text);
            double rate = Convert.ToDouble(txtrate.Text);
            int Qty = Convert.ToInt32(txtQty.Text);
            int futToken = Convert.ToInt32(txtFutToken.Text);
            UInt64 strategy = Convert.ToUInt64(lblStrategy.Text);

            BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
            snd.TransCode = 8;
            snd.UniqueID = UniqueId;
            snd.StrategyId = strategy;
            snd.Token = futToken;
            snd.gui_id = AppGlobal.GUI_ID;
            if (iswind == "Wind")
            {
                snd.isWind = true;
                snd.TradePrice = rate;
            }
            else
            {
                snd.isWind = false;
                snd.TradePrice = rate;
            }


            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(snd);
            //AppGlobal.connection._tcpGUIPort.Send(bytesToSend);

            TransactionWatch.ErrorMessage("ManualEntry|" + "UniqueId|" + UniqueId + "|Gui_Id|" + AppGlobal.GUI_ID + "|Strategy|" + strategy + "|iswind|" + iswind + "|Qty|" + Qty);

            for (int i = 0; i < Qty; i++)
            {
                //AppGlobal.connection._tcpGUIPort.Send(bytesToSend);
                foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
                {
                    connectedClient.Send(bytesToSend);
                }
                System.Threading.Thread.Sleep(50);
            }
        }
    }
}
