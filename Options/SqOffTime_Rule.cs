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
    public partial class SqOffTime_Rule : Form
    {
        public SqOffTime_Rule()
        {
            InitializeComponent();
        }

        private void SqOffTime_Rule_Load(object sender, EventArgs e)
        {
            //int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            //MarketWatch watch = new MarketWatch();
            //watch = AppGlobal.MarketWatch[iRow];
            //DateTime str = Convert.ToDateTime(dtpSqOff.Text);

            //string strTime = str.ToString("HH:mm:ss");
            //UInt64 uintTime =  ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(str));
            //UInt64 nowTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));

            //if (uintTime < nowTime)
            //{
            //    MessageBox.Show("Time should be less than current time");
            //    return;
            //}
            //else
            //{
            //    watch.SqTimeflg = true;
            //    watch.SqTime = strTime;
            //    watch.RowData.Cells[WatchConst.SQ_Time].Value = watch.SqTime;
            //}

        }

        private void SqOffTime_Rule_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._sqoffTimeRule = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            watch.SqTimeflg = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
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
    }
}
