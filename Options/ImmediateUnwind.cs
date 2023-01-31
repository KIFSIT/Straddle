using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;
using System.Threading;

namespace Straddle
{
    public partial class ImmediateUnwind : Form
    {
        public ImmediateUnwind()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyPress += new KeyPressEventHandler(ImmediateUnwind_KeyPress);
        }

        void ImmediateUnwind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._ImmediateUnWind = null;
                Close();
            }
        }

        private void ImmediateUnwind_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;
            lblUniqueId.Text = watch.uniqueId.ToString();

            txtNoOfLots.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtNoOfLots.Text == "")
            {
                MessageBox.Show("Please Enter Lots ");
                return;
            }
            string password = Convert.ToString(txtPassword.Text);
            int lots = Convert.ToInt32(txtNoOfLots.Text);
            if (lots > 25)
            {
                MessageBox.Show("No of Lots is not more than 25");
                return;
            }

            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.IsStrikeReq != true)
            {
                MessageBox.Show("Please Strike Req First !!!!");
                return;
            }
           
            if (password == "123")
            {
                Thread t = new Thread(() =>
                {
                if (watch.uniqueId == Convert.ToUInt64(lblUniqueId.Text))
                {
                    for (int i = 0; i < Math.Abs(lots); i++)
                    {
                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                        snd.TransCode = 10;
                        UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                        snd.UniqueID = unique;
                        snd.gui_id = AppGlobal.GUI_ID;
                        snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                        snd.isWind = false;
                        snd.Open = 0;

                        long seq = ClassDisruptor.ringBufferRequest.Next();
                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                        ClassDisruptor.ringBufferRequest.Publish(seq);
                        TransactionWatch.TransactionMessage("Trade|" + watch.uniqueId + "|UnWindCount|" + (i + 1), Color.Blue);
                        System.Threading.Thread.Sleep(50);
                    }
                }
                });
                t.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta
                t.Start();
            }
            else
            {
                MessageBox.Show("WrongPassword|" + watch.uniqueId + "| Lots | " + lots);
                TransactionWatch.ErrorMessage("WrongPassword|" + watch.uniqueId + "| Lots | " + lots);
            }

        }

        private void txtNoOfLots_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '.' && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void ImmediateUnwind_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._ImmediateUnWind = null;
        }
    }
}
