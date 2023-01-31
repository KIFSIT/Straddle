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
    public partial class StrategySqOff : Form
    {
        public StrategySqOff()
        {
            InitializeComponent();
        }

        private void StrategySqOff_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            lblStrategyName.Text = watch.Strategy.ToString();


            foreach (var _watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch.Strategy) && (x.StrategyId == 0)))
            {
                if (_watch.SQVegaflg)
                {
                    chkStrategyVega.Checked = true;
                    if (_watch.SQVegaType == "Point")
                    {
                        cmbStrategyVega.Text = "Point";
                        lblStrategyVegaLive.Text = Math.Round(_watch.VegaV, 2).ToString();
                        lblStrategyVegaPrice.Text = Math.Round(_watch.Init_SQVegaPrice, 2).ToString();
                        txtStrategyVega.Text = Math.Round(_watch.SQVegaPoint, 2).ToString();
                    }
                    else
                    {
                        cmbStrategyVega.Text = "Percent";
                        lblStrategyVegaLive.Text = Math.Round(_watch.VegaV, 2).ToString();
                        lblStrategyVegaPrice.Text = Math.Round(_watch.Init_SQVegaPrice, 2).ToString();
                        txtStrategyVega.Text = Math.Round(_watch.Per_SQVegaPrice, 2).ToString();
                    }
                }
                else
                {
                    chkStrategyVega.Checked = false;
                    cmbStrategyVega.Text = "Point";
                    lblStrategyVegaLive.Text = Math.Round(_watch.VegaV, 2).ToString();
                }

                if (_watch.SQPremiumflg)
                {
                    chkStrategyPremium.Checked = true;
                    if (_watch.SQPremiumType == "Point")
                    {
                        cmbStrategyPremium.Text = "Point";
                        lblPremiumLive.Text = Math.Round(_watch.premium, 2).ToString();
                        lblStrategyPremiumPrice.Text = Math.Round(_watch.Init_SQPremiumPrice, 2).ToString();
                        txtStrategyPremium.Text = Math.Round(_watch.SQPremiumPoint, 2).ToString();
                    }
                    else
                    {
                        cmbStrategyPremium.Text = "Percent";
                        lblPremiumLive.Text = Math.Round(_watch.VegaV, 2).ToString();
                        lblStrategyPremiumPrice.Text = Math.Round(_watch.Init_SQPremiumPrice, 2).ToString();
                        txtStrategyPremium.Text = Math.Round(_watch.Per_SQPremiumPrice, 2).ToString();
                    }
                }
                else
                {
                    chkStrategyPremium.Checked = false;
                    cmbStrategyPremium.Text = "Point";
                    lblPremiumLive.Text = Math.Round(_watch.premium, 2).ToString();
                }

                if (_watch.SQLossflg)
                {
                    chkStrategyLoss.Checked = true;
                    if (_watch.SQLossType == "Point")
                    {
                        cmbStrategyLoss.Text = "Point";
                        lblLossLive.Text = Math.Round(_watch.pnl, 2).ToString();
                        lblStrategyLossPrice.Text = Math.Round(_watch.SQLossPrice, 2).ToString();
                        txtStrategyLoss.Text = Math.Round(_watch.SQLossPoint, 2).ToString();
                    }
                    else
                    {
                        cmbStrategyLoss.Text = "Percent";
                        lblLossLive.Text = Math.Round(_watch.pnl, 2).ToString();
                        lblStrategyLossPrice.Text = Math.Round(_watch.Init_SQLossPrice, 2).ToString();
                        txtStrategyLoss.Text = Math.Round(_watch.Per_SQLossPrice, 2).ToString();

                    }
                }
                else
                {
                    chkStrategyLoss.Checked = false;
                    cmbStrategyLoss.Text = "Point";
                    lblLossLive.Text = Math.Round(_watch.pnl, 2).ToString();
                }

                

                if (watch.SqTimeflg)
                {
                    chksqoffTime.Checked = true;
                    dtpSqOff.Text = watch.SqTime.ToString();
                }
                else
                    chksqoffTime.Checked = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;

            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];


            foreach (var _watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch.Strategy) && (x.StrategyId == 0)))
            {
                if (chkStrategyVega.Checked == true)
                {
                    _watch.SQVegaflg = true;
                    if (cmbStrategyVega.Text == "Point")
                    {
                        _watch.SQVegaType = "Point";
                        _watch.SQVegaPoint = Math.Round(Convert.ToDouble(txtStrategyVega.Text), 2);
                        _watch.SQVegaPrice = Math.Round((Convert.ToDouble(lblStrategyVegaLive.Text)) - (Convert.ToDouble(txtStrategyVega.Text)), 2); 
                        _watch.RowData.Cells[WatchConst.SQ_TVega].Value = Math.Round(_watch.SQVegaPrice, 2);
                        _watch.Init_SQVegaPrice = Math.Round(_watch.SQVegaPrice, 2);

                    }
                    else
                    {
                        _watch.SQVegaType = "Percent";
                        _watch.Per_SQVegaPrice = Convert.ToDouble(txtStrategyVega.Text);
                        _watch.SQVegaPoint = Math.Round(Math.Abs(Convert.ToDouble(lblStrategyVegaLive.Text)) * (_watch.Per_SQVegaPrice / 100), 2);
                        _watch.SQVegaPrice = Math.Round((Convert.ToDouble(lblStrategyVegaLive.Text)) - (Convert.ToDouble(_watch.SQVegaPoint)), 2);
                        _watch.RowData.Cells[WatchConst.SQ_TVega].Value = Math.Round(_watch.SQVegaPrice, 2);
                        _watch.Init_SQVegaPrice = Math.Round(_watch.SQVegaPrice, 2);
                    }
                }
                else
                {
                    _watch.SQVegaflg = false;
                    _watch.SQVegaType = "Point";
                    _watch.SQVegaPrice = 0;
                    _watch.Init_SQVegaPrice = 0;
                    _watch.RowData.Cells[WatchConst.SQ_TVega].Value = Math.Round(_watch.SQVegaPrice, 2);
                }


                if (chkStrategyPremium.Checked)
                {
                    _watch.SQPremiumflg = true;
                    if (cmbStrategyPremium.Text == "Point")
                    {
                        _watch.SQPremiumType = "Point";
                        _watch.SQPremiumPoint = Math.Round(Convert.ToDouble(txtStrategyPremium.Text), 2);
                        _watch.SQPremiumPrice = Math.Round(Convert.ToDouble(lblPremiumLive.Text) - (Convert.ToDouble(_watch.SQPremiumPoint)), 2);
                        _watch.RowData.Cells[WatchConst.SQ_TPremium].Value = Math.Round(_watch.SQPremiumPrice, 2);
                        _watch.Init_SQPremiumPrice = Math.Round(_watch.SQPremiumPrice, 2);
                    }
                    else
                    {
                        _watch.SQPremiumType = "Percent";
                        _watch.Per_SQPremiumPrice = Convert.ToDouble(txtStrategyPremium.Text);
                        _watch.SQPremiumPoint = Math.Round(Math.Abs(Convert.ToDouble(lblPremiumLive.Text)) * (_watch.Per_SQPremiumPrice / 100), 2);
                        _watch.SQPremiumPrice = Math.Round((Convert.ToDouble(lblPremiumLive.Text) - Convert.ToDouble(_watch.SQPremiumPoint)), 2);
                        _watch.RowData.Cells[WatchConst.SQ_TPremium].Value = Math.Round(_watch.SQPremiumPrice,2);
                        _watch.Init_SQPremiumPrice = Math.Round(_watch.SQPremiumPrice, 2);
                    }
                }
                else
                {
                    _watch.SQPremiumflg = false;
                    _watch.SQPremiumType = "Point";
                    _watch.SQPremiumPrice = 0;
                    _watch.Init_SQPremiumPrice = 0;
                    _watch.RowData.Cells[WatchConst.SQ_TPremium].Value = Math.Round(_watch.SQPremiumPrice,2);
                }

                if (chkStrategyLoss.Checked)
                {
                    _watch.SQLossflg = true;
                    if (cmbStrategyLoss.Text == "Point")
                    {
                        _watch.SQLossType = "Point";
                        _watch.SQLossPoint = Math.Round(Convert.ToDouble(txtStrategyLoss.Text), 2);
                        _watch.SQLossPrice = Math.Round(Convert.ToDouble(lblLossLive.Text) - Convert.ToDouble(_watch.SQLossPoint), 2);
                        _watch.RowData.Cells[WatchConst.SQ_TLoss].Value = Math.Round(_watch.SQLossPrice, 2);
                        _watch.Init_SQLossPrice = Math.Round(_watch.SQLossPrice, 2);
                    }
                    else
                    {
                        _watch.SQLossType = "Percent";
                        _watch.Per_SQLossPrice = Convert.ToDouble(txtStrategyLoss.Text);
                        _watch.SQLossPoint = Math.Round(Math.Abs(Convert.ToDouble(lblLossLive.Text)) * (_watch.Per_SQLossPrice / 100), 2);
                        _watch.Per_SQLossPrice = Math.Round(Convert.ToDouble(lblLossLive.Text) - Convert.ToDouble(_watch.SQLossPoint), 2);
                        _watch.RowData.Cells[WatchConst.SQ_TLoss].Value = Math.Round(_watch.SQLossPrice, 2);
                        _watch.Init_SQLossPrice = Math.Round(_watch.SQLossPrice, 2);
                    }
                }
                else
                {
                    _watch.SQLossflg = false;
                    _watch.SQLossType = "Point";
                    _watch.SQLossPrice = 0;
                    _watch.Init_SQLossPrice = 0;
                    _watch.RowData.Cells[WatchConst.SQ_TLoss].Value = Math.Round(_watch.SQLossPrice, 2);
                }

                if (chksqoffTime.Checked == true)
                {
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
                else
                {
                    watch.SqTimeflg = false;
                }
            }            
        }

        private void StrategySqOff_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._strategySqOff = null;
        }
    }
}
