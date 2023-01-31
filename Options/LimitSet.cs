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
    public partial class LimitSet : Form
    {
        public LimitSet()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TransactionWatch.ErrorMessage("PreiousPriceProtectionLimitSet|" + "StockOption|" + "|BuyLimit|" + AppGlobal.upperlimit + "|SellLimit|" + AppGlobal.lowerlimit + "|FutureLimit|" + AppGlobal.Stocklimit);

            if (Convert.ToDouble(txtLimit1.Text) >= 20)
            {
                MessageBox.Show("Option LPP Limit 1  should be Less than 20 Rs");
                TransactionWatch.ErrorMessage("Option LPP Limit 1  should be Less than 20 Rs " + " userInput " + Convert.ToString(txtLimit1.Text));
                return;
            }

            if (Convert.ToDouble(txtLimit2.Text) >= 40)
            {
                MessageBox.Show("Option LPP Limit 2 should be Less than 40 percentage");
                TransactionWatch.ErrorMessage("Option LPP Limit 2 should be Less than 40 percentage " + " userInput " + Convert.ToString(txtLimit2.Text));
                return;
             }

            if (Convert.ToDouble(txtLimit3.Text) >= 3)
            {
                MessageBox.Show("Future LPP Limit 3 should be Less than 3 percentage");
                TransactionWatch.ErrorMessage("Future LPP Limit 3 should be Less than 3 percentage " + " userInput " + Convert.ToString(txtLimit3.Text));
                return;
            }


            
            AppGlobal.upperlimit = Convert.ToDouble(txtLimit1.Text);
            AppGlobal.lowerlimit = Convert.ToDouble(txtLimit2.Text);
            AppGlobal.Stocklimit = Convert.ToDouble(txtLimit3.Text);

            MessageBox.Show("PriceProtectionLimitSet|" + "StockOption|" + "|BuyLimit|" + AppGlobal.upperlimit + "|SellLimit|" + AppGlobal.lowerlimit + "|FutureLimit|" + AppGlobal.Stocklimit);
            TransactionWatch.ErrorMessage("PriceProtectionLimitSet|" + "StockOption|" + "|BuyLimit|" + AppGlobal.upperlimit + "|SellLimit|" + AppGlobal.lowerlimit + "|FutureLimit|" + AppGlobal.Stocklimit);
            TransactionWatch.TransactionMessage("PriceProtectionLimitSet|" + "StockOption|" + "|BuyLimit|" + AppGlobal.upperlimit + "|SellLimit|" + AppGlobal.lowerlimit + "|FutureLimit|" + AppGlobal.Stocklimit,Color.Red);
        }

        private void LimitSet_Load(object sender, EventArgs e)
        {
            txtLimit1.Text = Convert.ToString(AppGlobal.upperlimit);
            txtLimit2.Text = Convert.ToString(AppGlobal.lowerlimit);
            txtLimit3.Text = Convert.ToString(AppGlobal.Stocklimit);
        }

        private void LimitSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._limitset = null; 
        }
    }
}
