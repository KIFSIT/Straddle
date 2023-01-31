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
    public partial class BuyOrder : Form
    {
        public BuyOrder()
        {
            InitializeComponent();
        }

        private void BuyOrder_Load(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;
            lblUniqueId.Text = watch.uniqueId.ToString();

            lblBid.Text = Convert.ToDouble(watch.Leg1.BuyPrice).ToString();
            lblAsk.Text = Convert.ToDouble(watch.Leg1.SellPrice).ToString();
            lblLtp.Text = Convert.ToDouble(watch.Leg1.LastTradedPrice).ToString();




            txtNoOfLots.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            double Ltp = Convert.ToDouble(watch.Leg1.LastTradedPrice);



            if (watch.Leg1.ContractInfo.Series != "XX")
            {
                double priceLimit = 0;
                if (Ltp <= 50)
                {
                    priceLimit = Ltp + AppGlobal.upperlimit;
                    double userprice = Convert.ToDouble(txtPrice.Text);
                    if (userprice > priceLimit)
                    {
                        MessageBox.Show("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockOptionLimit|" + AppGlobal.upperlimit + "|LTP|" + Ltp);
                        TransactionWatch.TransactionMessage("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockOptionLimit|" + AppGlobal.upperlimit + "|LTP|" + Ltp, Color.Red);
                        TransactionWatch.ErrorMessage("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockOptionLimit|" + AppGlobal.upperlimit + "|LTP|" + Ltp);

                    }
                }
                else
                {
                    priceLimit = Ltp + (Ltp * AppGlobal.lowerlimit);
                    double userprice = Convert.ToDouble(txtPrice.Text);
                    if (userprice > priceLimit)
                    {
                        MessageBox.Show("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockOptionLimitPercentage|" + AppGlobal.lowerlimit + "|LTP|" + Ltp);
                        TransactionWatch.TransactionMessage("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockOptionLimitPercentage|" + AppGlobal.lowerlimit + "|LTP|" + Ltp, Color.Red);
                        TransactionWatch.ErrorMessage("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockOptionLimitPercentage|" + AppGlobal.lowerlimit + "|LTP|" + Ltp);
                    }
                }
                //double priceLimit = Ltp + AppGlobal.upperlimit;
            }
            else
            {
                double priceLimit = Ltp + (Ltp * AppGlobal.Stocklimit);
                double userprice = Convert.ToDouble(txtPrice.Text);
                if (userprice > priceLimit)
                {
                    MessageBox.Show("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockFutLimit|" + AppGlobal.Stocklimit + "|LTP|" + Ltp);
                    TransactionWatch.TransactionMessage("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockFutLimit|" + AppGlobal.Stocklimit + "|LTP|" + Ltp, Color.Red);
                    TransactionWatch.ErrorMessage("BuyOrderRejected|" + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|StockOptionLimit|" + priceLimit + "|BuyPrice|" + userprice + "|StockFutLimit|" + AppGlobal.Stocklimit + "|LTP|" + Ltp);

                }

            }
        }

        private void BuyOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._buyorder = null;
        }
    }
}
