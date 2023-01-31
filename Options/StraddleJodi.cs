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
using ArisDev;
using System.Globalization;

namespace Straddle
{
    public partial class StraddleJodi : Form
    {

        #region Variable
        string[] threeExpiry;
        List<string> _StrategyList;
        #endregion

        public StraddleJodi()
        {
            InitializeComponent();
        }

        private void StraddleJodi_Load(object sender, EventArgs e)
        {
            if (AppGlobal.Global_StrategyName != "")
            {
                lblStrategy.Text = AppGlobal.Global_StrategyName;
            }
            else
            {
                int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentCell.RowIndex;
                MarketWatch watch = new MarketWatch();
                watch = AppGlobal.MarketWatch[iRow];
                lblStrategy.Text = watch.Strategy;
            }
            _StrategyList = new List<string>();

            for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
            {
                MarketWatch watch = AppGlobal.MarketWatch[index];
                string Strategy_name = Convert.ToString(watch.Strategy);
                if (!_StrategyList.Contains(Strategy_name))
                {
                    _StrategyList.Add(Strategy_name);
                    cmbStrategy.Items.Add(Strategy_name);
                }
            }
            cmbStrategy.SelectedItem = AppGlobal.SelectedStrategy;


            double ltp = Convert.ToDouble(AppGlobal.frmWatch.txtbankValue.Text);
            double diff = 100;
            double remainder = Math.Round(Convert.ToDouble(ltp % diff), 2);
            double atm_strike = 0;
            if (remainder > (diff / 2))
            {
                atm_strike = Convert.ToDouble(ltp + diff) - Convert.ToDouble(ltp % diff);
            }
            else
            {
                atm_strike = Convert.ToDouble(ltp - (ltp % diff));
            }



            threeExpiry = GetExpiryDates(ArisApi_a._arisApi.DsContract.Tables["NSEFO"]);
            DateTime dt1 = Convert.ToDateTime(threeExpiry[0].ToString());
            DateTime dt2 = Convert.ToDateTime(threeExpiry[1].ToString());
            DateTime dt3 = Convert.ToDateTime(threeExpiry[2].ToString());

            #region Contract 

            string filter3 = "InstrumentName='" + "OPTIDX" + "'";
            DataTable symbol = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            symbol.DefaultView.RowFilter = filter3;
            cmbSymbol1.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbSymbol1.DisplayMember = "Symbol";

            cmbSymbol2.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbSymbol2.DisplayMember = "Symbol";

            cmbHedgeSymbol1.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbHedgeSymbol1.DisplayMember = "Symbol";

            cmbHedgeSymbol2.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbHedgeSymbol2.DisplayMember = "Symbol";

            cmbSymbol1.Text = "BANKNIFTY";
            cmbSymbol2.Text = "BANKNIFTY";


            cmbHedgeSymbol1.Text = "BANKNIFTY";
            cmbHedgeSymbol2.Text = "BANKNIFTY";


            string filter = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + cmbSymbol1.Text.Trim() + "'";
            DataTable expiry = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            expiry.DefaultView.RowFilter = filter;
            expiry.DefaultView.Sort = "ExpiryDate asc";

            DataTable exp2 = expiry.DefaultView.ToTable(true, "ExpiryDate");

            foreach (DataRow dr in exp2.Rows)
            {
                string s1 = dr["ExpiryDate"].ToString();
                string s2 = s1.Substring(0, 4);
                string s3 = s1.Substring(4, 2);
                string s4 = s1.Substring(6, 2);
                System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
                string month = mfi.GetMonthName(Convert.ToInt32(s3)).ToString();
                month = month.Substring(0, 3);
                string s5 = s2 + month + s4;

                dr["ExpiryDate"] = s5;
            }
            cmbExpiry1.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
            cmbExpiry1.DisplayMember = "ExpiryDate";

            cmbExpiry2.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
            cmbExpiry2.DisplayMember = "ExpiryDate";

            cmbHedgeExpiry1.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
            cmbHedgeExpiry1.DisplayMember = "ExpiryDate";

            cmbHedgeExpiry2.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
            cmbHedgeExpiry2.DisplayMember = "ExpiryDate";

            string s12 = Convert.ToString(cmbExpiry1.Text);
            string s22 = s12.Substring(0, 4);
            string s32 = s12.Substring(4, 3);
            string s42 = s12.Substring(7, 2);
            int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
            string monString = "";
            if (mont <= 9)
            {
                monString = "0" + Convert.ToString(mont);
            }
            else
            {
                monString = Convert.ToString(mont);
            }
            string s52 = s22 + monString + s42;

            string filter1 = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s52 + "'";
            DataTable Strike = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            Strike.DefaultView.RowFilter = filter1;
            Strike.DefaultView.Sort = "StrikePrice";


            DataTable strike1 = Strike.DefaultView.ToTable(true, "StrikePrice");
            int minvalue = 0;
            int maxvalue = 0;
            int i = 0;
            foreach (DataRow dr in strike1.Rows)
            {
                int str = Convert.ToInt32(dr["StrikePrice"]);
                if (i == 0)
                {
                    minvalue = str;
                    maxvalue = str;
                }
                if (str >= maxvalue)
                {
                    maxvalue = str;
                }
                if (str <= minvalue)
                    minvalue = str;
                dr["StrikePrice"] = str;
                i++;
            }
            DataView table = Strike.DefaultView;
          

            cmbStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike1.DisplayMember = "StrikePrice";
            

            cmbStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike2.DisplayMember = "StrikePrice";
         

            cmbHedgeStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbHedgeStrike1.DisplayMember = "StrikePrice";
         

            cmbHedgeStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbHedgeStrike2.DisplayMember = "StrikePrice";

            if (atm_strike != 0)
            {
                cmbStrike1.Text = Convert.ToInt32(atm_strike).ToString();
                cmbStrike2.Text = Convert.ToInt32(atm_strike).ToString();
                cmbHedgeStrike1.Text = Convert.ToInt32(atm_strike).ToString();
                cmbHedgeStrike2.Text = Convert.ToInt32(atm_strike).ToString();
            }

            #endregion

            cmbSymbol2.Enabled = false; 
            rdoMain.Checked = true;
            chkHedgeJodi.Checked = true;
            rdoStraddle.Checked = true;
            cmbHedgeSymbol2.Enabled = false;
            cmbHedgeExpiry2.Enabled = false;
            rdoHedgeStraddle.Checked = true;

            

        }

        private string[] GetExpiryDates(DataTable expTable)
        {
            try
            {
                var dateList = new HashSet<String>();
                var dateList1 = new HashSet<String>();
                AppGlobal.monthint = new List<int>();
                foreach (DataRow r1 in expTable.Rows)
                {
                    if ((r1[DBConst.InstrumentName].ToString() == "FUTIDX" || r1[DBConst.InstrumentName].ToString() == "FUTSTK") && r1[DBConst.Symbol].ToString() == "NIFTY")
                    {
                        string eDate = r1[DBConst.ExpiryDate].ToString();
                        dateList.Add(eDate);
                    }
                }
                AppGlobal.monthint.Clear();
                foreach (string s1 in dateList)
                {
                    string s2 = s1.Substring(0, 4);
                    string s3 = s1.Substring(4, 2);
                    string s4 = s1.Substring(6, 2);
                    System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
                    string month = mfi.GetMonthName(Convert.ToInt32(s3)).ToString();
                    month = month.Substring(0, 3);
                    string s5 = s2 + month + s4;

                    int dateno = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(s5));
                    AppGlobal.monthint.Add(dateno);
                }
                AppGlobal.monthint.Sort();
                foreach (int k in AppGlobal.monthint)
                {
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, k));
                    dateList1.Add(month);
                }
                string[] threeDates = { "", "", "" };
                int i = 0;
                foreach (string s1 in dateList1)
                {
                    if (s1.Length != 0)
                    {
                        threeDates[i] = s1;
                        i++;
                        if (i > 2) break;
                    }
                }
                return threeDates;
            }
            catch (Exception) { return null; }
        }

        private void cmbSymbol1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string _symbol = cmbSymbol1.Text;
            cmbSymbol2.Text = _symbol;

            string filter = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + _symbol + "'";
            DataTable expiry = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            expiry.DefaultView.RowFilter = filter;
            expiry.DefaultView.Sort = "ExpiryDate asc";

            DataTable exp2 = expiry.DefaultView.ToTable(true, "ExpiryDate");

            foreach (DataRow dr in exp2.Rows)
            {
                string s1 = dr["ExpiryDate"].ToString();
                string s2 = s1.Substring(0, 4);
                string s3 = s1.Substring(4, 2);
                string s4 = s1.Substring(6, 2);
                System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
                string month = mfi.GetMonthName(Convert.ToInt32(s3)).ToString();
                month = month.Substring(0, 3);
                string s5 = s2 + month + s4;

                dr["ExpiryDate"] = s5;
            }
            cmbExpiry1.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
            cmbExpiry1.DisplayMember = "ExpiryDate";

            cmbExpiry2.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
            cmbExpiry2.DisplayMember = "ExpiryDate";


            string s12 = Convert.ToString(cmbExpiry1.Text);
            string s22 = s12.Substring(0, 4);
            string s32 = s12.Substring(4, 3);
            string s42 = s12.Substring(7, 2);
            int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
            string monString = "";
            if (mont <= 9)
            {
                monString = "0" + Convert.ToString(mont);
            }
            else
            {
                monString = Convert.ToString(mont);
            }
            string s52 = s22 + monString + s42;

            string filter1 = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s52 + "'";
            DataTable Strike = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            Strike.DefaultView.RowFilter = filter1;
            Strike.DefaultView.Sort = "StrikePrice";

            DataTable strike1 = Strike.DefaultView.ToTable(true, "StrikePrice");
            int minvalue = 0;
            int maxvalue = 0;
            int i = 0;
            foreach (DataRow dr in strike1.Rows)
            {

                int str = Convert.ToInt32(dr["StrikePrice"]);
                if (i == 0)
                {
                    minvalue = str;
                    maxvalue = str;
                }
                if (str >= maxvalue)
                {
                    maxvalue = str;
                }
                if (str <= minvalue)
                    minvalue = str;
                dr["StrikePrice"] = str;
                i++;
            }

            DataView table = Strike.DefaultView;

         
            cmbStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike1.DisplayMember = "StrikePrice";

            cmbStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike2.DisplayMember = "StrikePrice";

        }

        private void cmbExpiry1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string k = cmbExpiry1.Text;
            cmbExpiry2.Text = k;

            cmbStrike1.Visible = true;
            string s1 = Convert.ToString(cmbExpiry1.Text);
            string s2 = s1.Substring(0, 4);
            string s3 = s1.Substring(4, 3);
            string s4 = s1.Substring(7, 2);
            int mont = DateTime.ParseExact(s3, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
            string monString = "";
            if (mont <= 9)
            {
                monString = "0" + Convert.ToString(mont);
            }
            else
            {
                monString = Convert.ToString(mont);
            }

            string s5 = s2 + monString + s4;
            string filter1 = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s5 + "'";
            DataTable Strike = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            Strike.DefaultView.RowFilter = filter1;
            Strike.DefaultView.Sort = "StrikePrice";
            DataView table = Strike.DefaultView;
            cmbStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike1.DisplayMember = "StrikePrice";
            cmbStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike2.DisplayMember = "StrikePrice";
        }

        private void StraddleJodi_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._straddleJodi = null;
        }

        private void cmbHedgeSymbol1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string _symbol = cmbHedgeSymbol1.Text;
            cmbHedgeSymbol2.Text = _symbol;

            string filter = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + _symbol + "'";
            DataTable expiry = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            expiry.DefaultView.RowFilter = filter;
            expiry.DefaultView.Sort = "ExpiryDate asc";

            DataTable exp2 = expiry.DefaultView.ToTable(true, "ExpiryDate");

            foreach (DataRow dr in exp2.Rows)
            {
                string s1 = dr["ExpiryDate"].ToString();
                string s2 = s1.Substring(0, 4);
                string s3 = s1.Substring(4, 2);
                string s4 = s1.Substring(6, 2);
                System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
                string month = mfi.GetMonthName(Convert.ToInt32(s3)).ToString();
                month = month.Substring(0, 3);
                string s5 = s2 + month + s4;

                dr["ExpiryDate"] = s5;
            }
            cmbHedgeExpiry1.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
            cmbHedgeExpiry1.DisplayMember = "ExpiryDate";

            cmbHedgeExpiry2.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
            cmbHedgeExpiry2.DisplayMember = "ExpiryDate";


            string s12 = Convert.ToString(cmbExpiry1.Text);
            string s22 = s12.Substring(0, 4);
            string s32 = s12.Substring(4, 3);
            string s42 = s12.Substring(7, 2);
            int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
            string monString = "";
            if (mont <= 9)
            {
                monString = "0" + Convert.ToString(mont);
            }
            else
            {
                monString = Convert.ToString(mont);
            }
            string s52 = s22 + monString + s42;

            string filter1 = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + cmbHedgeSymbol1.Text + "' AND ExpiryDate = '" + s52 + "'";
            DataTable Strike = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            Strike.DefaultView.RowFilter = filter1;
            Strike.DefaultView.Sort = "StrikePrice";

            DataTable strike1 = Strike.DefaultView.ToTable(true, "StrikePrice");
            int minvalue = 0;
            int maxvalue = 0;
            int i = 0;
            foreach (DataRow dr in strike1.Rows)
            {

                int str = Convert.ToInt32(dr["StrikePrice"]);
                if (i == 0)
                {
                    minvalue = str;
                    maxvalue = str;
                }
                if (str >= maxvalue)
                {
                    maxvalue = str;
                }
                if (str <= minvalue)
                    minvalue = str;
                dr["StrikePrice"] = str;
                i++;
            }

            DataView table = Strike.DefaultView;


            cmbHedgeStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbHedgeStrike1.DisplayMember = "StrikePrice";

            cmbHedgeStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbHedgeStrike2.DisplayMember = "StrikePrice";
        }

        private void cmbHedgeExpiry1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string k = cmbHedgeExpiry1.Text;
            cmbHedgeExpiry2.Text = k;

            cmbHedgeStrike1.Visible = true;
            string s1 = Convert.ToString(cmbHedgeExpiry1.Text);
            string s2 = s1.Substring(0, 4);
            string s3 = s1.Substring(4, 3);
            string s4 = s1.Substring(7, 2);
            int mont = DateTime.ParseExact(s3, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
            string monString = "";
            if (mont <= 9)
            {
                monString = "0" + Convert.ToString(mont);
            }
            else
            {
                monString = Convert.ToString(mont);
            }

            string s5 = s2 + monString + s4;
            string filter1 = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + cmbHedgeSymbol1.Text + "' AND ExpiryDate = '" + s5 + "'";
            DataTable Strike = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            Strike.DefaultView.RowFilter = filter1;
            Strike.DefaultView.Sort = "StrikePrice";
            DataView table = Strike.DefaultView;
            cmbHedgeStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbHedgeStrike1.DisplayMember = "StrikePrice";
            cmbHedgeStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbHedgeStrike2.DisplayMember = "StrikePrice";
        }

        private void rdoStraddle_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoStraddle.Checked)
            {
                string k = cmbStrike1.Text;
                cmbStrike2.Text = k;
                cmbStrike2.Enabled = false;
            }
            else
            {
                cmbStrike2.Enabled = true;
            }
        }

        private void addRule1_Click(object sender, EventArgs e)
        {
            if (rdoStraddle.Checked)
            {
                if (chkHedgeJodi.Checked == false)
                {
                    LegsAddWithOutHedge("Straddle");

                }
                else if(chkHedgeJodi.Checked == true)
                {

                    double round1P = Convert.ToDouble(txtRound1P.Text);
                    double round2P = Convert.ToDouble(txtRound2P.Text);
                    double round3P = Convert.ToDouble(txtRound3P.Text);
                    double round4P = Convert.ToDouble(txtRound4P.Text);


                    double round1Q = Convert.ToDouble(txtRound1Q.Text);
                    double round2Q = Convert.ToDouble(txtRound2Q.Text);
                    double round3Q = Convert.ToDouble(txtRound3Q.Text);
                    double round4Q = Convert.ToDouble(txtRound4Q.Text);


                    if ((round1P == 0 && round2P == 0 && round3P == 0 && round4P == 0) || (round1Q == 0 && round2Q == 0 && round3Q == 0 && round4Q == 0))                    
                    {
                        MessageBox.Show("Please Enter Proper Percentage or Qty");
                        return;
                    }

                    double totalround = round1P + round2P + round3P + round4P;
                    if (totalround != 100)
                    {
                        MessageBox.Show("Please Qty Percentage is not 100%");
                        return;
                    }
                    if (rdoHedgeStraddle.Checked == true)
                    {
                        StraddleLegsAddWithHedge("Straddle", round1P, round1Q, round2P, round2Q, round3P, round3Q, round4P, round4Q);
                    }
                    else if (rdoHedgeStrangle.Checked == true)
                    {
                        StraddleLegsAddWithHedge("Strangle", round1P, round1Q, round2P, round2Q, round3P, round3Q, round4P, round4Q);

                    }

                }
            }
            else if(rdoStrangle.Checked)
            {
                if (chkHedgeJodi.Checked == false)
                {
                    LegsAddWithOutHedge("Strangle");
                }
                else
                {
                    double round1P = Convert.ToDouble(txtRound1P.Text);
                    double round2P = Convert.ToDouble(txtRound2P.Text);
                    double round3P = Convert.ToDouble(txtRound3P.Text);
                    double round4P = Convert.ToDouble(txtRound4P.Text);


                    double round1Q = Convert.ToDouble(txtRound1Q.Text);
                    double round2Q = Convert.ToDouble(txtRound2Q.Text);
                    double round3Q = Convert.ToDouble(txtRound3Q.Text);
                    double round4Q = Convert.ToDouble(txtRound4Q.Text);


                    if ((round1P == 0 && round2P == 0 && round3P == 0 && round4P == 0) || (round1Q == 0 && round2Q == 0 && round3Q == 0 && round4Q == 0))
                    {
                        MessageBox.Show("Please Enter Proper Percentage or Qty");
                        return;
                    }

                    double totalround = round1P + round2P + round3P + round4P;
                    if (totalround != 100)
                    {
                        MessageBox.Show("Please Qty Percentage is not 100%");
                        return;
                    }
                    else if (rdoHedgeStrangle.Checked == true)
                    {
                        StrangleLegsAddWithHedge("Strangle", round1P, round1Q, round2P, round2Q, round3P, round3Q, round4P, round4Q);
                    }
                    else if (rdoHedgeStraddle.Checked == true)
                    {
                        StrangleLegsAddWithHedge("Straddle", round1P, round1Q, round2P, round2Q, round3P, round3Q, round4P, round4Q);
                    }
                }
            }
        }

        public void LegsAddWithOutHedge(string Type)
        {

            string StrategyName = Convert.ToString(cmbStrategy.Text);
            string[] strategyArray = StrategyName.Split('_');
            int strategy_No = Convert.ToInt32(strategyArray[1]);

            string n1 = Convert.ToString(cmbExpiry1.Text);
            string n2 = n1.Substring(0, 4);
            string n3 = n1.Substring(4, 3);
            string n4 = n1.Substring(7, 2);
            int mont0 = DateTime.ParseExact(n3, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi0 = new System.Globalization.DateTimeFormatInfo();
            string monString0 = "";
            if (mont0 <= 9)
            {
                monString0 = "0" + Convert.ToString(mont0);
            }
            else
            {
                monString0 = Convert.ToString(mont0);
            }
            string n5 = n2 + monString0 + n4;
            string ExpDisplay = n4 + n3 + n2;
            string n12 = Convert.ToString(cmbExpiry2.Text);
            string n22 = n12.Substring(0, 4);
            string n32 = n12.Substring(4, 3);
            string n42 = n12.Substring(7, 2);
            int mont02 = DateTime.ParseExact(n32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi02 = new System.Globalization.DateTimeFormatInfo();
            string monString02 = "";
            if (mont02 <= 9)
            {
                monString02 = "0" + Convert.ToString(mont02);
            }
            else
            {
                monString02 = Convert.ToString(mont02);
            }
            string n52 = n22 + monString02 + n42;
            string ExpDisplay2 = n42 + n32 + n22;

            #region Future Exp
            int currentmonth = mont0;

            uint expiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[0]));
            string expiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, expiry).ToString("yyyyMMMdd");
            string sf12 = Convert.ToString(expiry1);
            string sf22 = sf12.Substring(0, 4);
            string sf32 = sf12.Substring(4, 3);
            string sf42 = sf12.Substring(7, 2);
            int montf = DateTime.ParseExact(sf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mffi1 = new System.Globalization.DateTimeFormatInfo();
            string monStringf = "";
            if (montf <= 9)
            {
                monStringf = "0" + Convert.ToString(montf);
            }
            else
            {
                monStringf = Convert.ToString(montf);
            }
            string sf52 = sf22 + monStringf + sf42;
            string selectFut = sf52;


            uint nxtexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[1]));
            string nxtexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, nxtexpiry).ToString("yyyyMMMdd");
            string nxtsf12 = Convert.ToString(nxtexpiry1);
            string nxtsf22 = nxtsf12.Substring(0, 4);
            string nxtsf32 = nxtsf12.Substring(4, 3);
            string nxtsf42 = nxtsf12.Substring(7, 2);
            int nxtmontf = DateTime.ParseExact(nxtsf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo nxtmffi1 = new System.Globalization.DateTimeFormatInfo();
            string nxtmonStringf = "";
            if (nxtmontf <= 9)
            {
                nxtmonStringf = "0" + Convert.ToString(nxtmontf);
            }
            else
            {
                nxtmonStringf = Convert.ToString(nxtmontf);
            }
            string nxtsf52 = nxtsf22 + nxtmonStringf + nxtsf42;


            if (currentmonth == nxtmontf)
                selectFut = nxtsf52;


            uint farexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[2]));
            string farexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, farexpiry).ToString("yyyyMMMdd");
            string farsf12 = Convert.ToString(farexpiry1);
            string farsf22 = farsf12.Substring(0, 4);
            string farsf32 = farsf12.Substring(4, 3);
            string farsf42 = farsf12.Substring(7, 2);
            int farmontf = DateTime.ParseExact(farsf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo farmffi1 = new System.Globalization.DateTimeFormatInfo();
            string farmonStringf = "";
            if (farmontf <= 9)
            {
                farmonStringf = "0" + Convert.ToString(farmontf);
            }
            else
            {
                farmonStringf = Convert.ToString(farmontf);
            }
            string farsf52 = farsf22 + farmonStringf + farsf42;
            if (currentmonth == farmontf)
                selectFut = farsf52;

            #endregion


            string Sym = Convert.ToString(cmbSymbol1.Text);
            string l1Series = "CE";
            string l2Series = "PE";

            #region Check Unique Id
            int StrikeGap = 0;
            int Leg1Strike = 0;
            int Leg3Strike = 0;
            StrikeGap = Math.Abs(Convert.ToInt32(cmbStrike1.Text) - Convert.ToInt32(cmbStrike2.Text));
            string txtG = Convert.ToString(StrikeGap);
            string _Gap = "";
            if (txtG.Length == 3)
                _Gap = "0" + Convert.ToString(txtG);
            else
                _Gap = Convert.ToString(txtG);
            Leg1Strike = Convert.ToInt32(cmbStrike1.Text);
            Leg3Strike = Convert.ToInt32(cmbStrike2.Text);


            //if (Leg1Strike != Leg3Strike)
            //{
            //    MessageBox.Show("Please check Strike");
            //    return;
            //}

            string Strike1 = "";


            if (Leg1Strike > 9999)
            {
                Strike1 = Convert.ToString(Convert.ToInt32(Leg1Strike) / 100);
            }
            else
            {
                Strike1 = Convert.ToString(Convert.ToInt32(Leg1Strike) / 10);
            }

            UInt64 exp = Convert.ToUInt64(n5);
            int TokenNo = 0;
            string strFilterCheck = "";
            strFilterCheck = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
            DataRow[] drCheck = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck);
            foreach (DataRow dr in drCheck)
            {
                exp = Convert.ToUInt64(dr["SymbolDesc"]);
                TokenNo = Convert.ToInt32(dr["TokenNo"]);
            }

            UInt64 exp3 = Convert.ToUInt64(n5);
            int TokenNo3 = 0;
            string strFilterCheck1 = "";
            strFilterCheck1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
            DataRow[] drCheck1 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck1);
            foreach (DataRow dr in drCheck1)
            {
                exp3 = Convert.ToUInt64(dr["SymbolDesc"]);
                TokenNo3 = Convert.ToInt32(dr["TokenNo"]);
            }

            UInt64 Unique_id = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
            #endregion


            string strategy_type = TokenNo + "_" + TokenNo3;

            foreach (var watchT in AppGlobal.MarketWatch.Where(x => ((x.Leg1.ContractInfo.TokenNo + "_" + x.Leg1.ContractInfo.TokenNo) == Convert.ToString(strategy_type))))
            {
                if (watchT.StrategyId == 91 && watchT.Strategy == StrategyName)
                {
                    MessageBox.Show("This Rule Already Added with GUI id : " + watchT.uniqueId + " Strategy : " + watchT.Strategy);
                    return;
                }
            }
            if (AppGlobal.MarketWatch.Count() == 0)
            {
                return;
            }
            
            
            AppGlobal.StrategyRuleIndexNo = AppGlobal.StrategyRuleIndexNo + 1;

            string strType = "MainJodi" + Type + "_" + AppGlobal.StrategyRuleIndexNo;


            #region Gui No is changing

            int flg = 0;
            //int rowcount = 0;
            bool NextStrategy = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;
                    watch.StrategyName = strType;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 0.05M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                   
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.IsStrikeReq = false;
                    watch.Track = "None";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                    watch.Hedgeflg = false;
  


                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbExpiry1.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;


                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2
                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;
                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";
                    
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                   
                   

                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }

                    }

                    #endregion


                    AppGlobal.MarketWatch.Insert(i, watch);

                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Aqua;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg = 1;
                    NextStrategy = false;
                }
            }
            #endregion

            if (NextStrategy)
            {
                #region Old add Code

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;
                 watch.StrategyName = strType; 
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
              
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.IsStrikeReq = false;
                watch.Hedgeflg = false;
                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbExpiry1.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;
                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";


                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;

               
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg

                
                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                


                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Aqua;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);


            #region Gui No is changing

            int flg1 = 0;
            //int rowcount = 0;
            bool NextStrategy1 = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg1 == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;

                    watch.StrategyName = strType;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 0.05M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.IsStrikeReq = false;
                    watch.Hedgeflg = false;

                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbExpiry1.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;


                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2
                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;                    
                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";                    
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                   
                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }

                    }

                    #endregion


                    AppGlobal.MarketWatch.Insert(i, watch);

                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Aqua;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg1 = 1;
                    NextStrategy1 = false;
                }
            }

            #endregion

            if (NextStrategy1)
            {
                #region Old add Code
                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;

                watch.StrategyName = strType;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.IsStrikeReq = false;

                watch.Hedgeflg = false;

                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbExpiry1.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;




                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;                
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg
                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
               
                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Aqua;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);

        }

        public void StraddleLegsAddWithHedge(string HedgeType, double rnd1P, double rnd1Q, double rnd2P, double rnd2Q, double rnd3P, double rnd3Q, double rnd4P, double rnd4Q)
        {
            string StrategyName = Convert.ToString(cmbStrategy.Text);
            string[] strategyArray = StrategyName.Split('_');
            int strategy_No = Convert.ToInt32(strategyArray[1]);

            bool Alert = false;

            if (chkAlert.Checked)
                Alert = true;
            



            string n1 = Convert.ToString(cmbExpiry1.Text);
            string n2 = n1.Substring(0, 4);
            string n3 = n1.Substring(4, 3);
            string n4 = n1.Substring(7, 2);
            int mont0 = DateTime.ParseExact(n3, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi0 = new System.Globalization.DateTimeFormatInfo();
            string monString0 = "";
            if (mont0 <= 9)
            {
                monString0 = "0" + Convert.ToString(mont0);
            }
            else
            {
                monString0 = Convert.ToString(mont0);
            }
            string n5 = n2 + monString0 + n4;
            string ExpDisplay = n4 + n3 + n2;
            string n12 = Convert.ToString(cmbExpiry2.Text);
            string n22 = n12.Substring(0, 4);
            string n32 = n12.Substring(4, 3);
            string n42 = n12.Substring(7, 2);
            int mont02 = DateTime.ParseExact(n32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi02 = new System.Globalization.DateTimeFormatInfo();
            string monString02 = "";
            if (mont02 <= 9)
            {
                monString02 = "0" + Convert.ToString(mont02);
            }
            else
            {
                monString02 = Convert.ToString(mont02);
            }
            string n52 = n22 + monString02 + n42;

            string ExpDisplay2 = n42 + n32 + n22;

            string hn1 = Convert.ToString(cmbHedgeExpiry1.Text);

            string hn2 = hn1.Substring(0, 4);
            string hn3 = hn1.Substring(4, 3);
            string hn4 = hn1.Substring(7, 2);

            string ExpDisplay_h = hn4 + hn3 + hn2;


            string h2n1 = Convert.ToString(cmbHedgeExpiry2.Text);

            string h2n2 = h2n1.Substring(0, 4);
            string h2n3 = h2n1.Substring(4, 3);
            string h2n4 = h2n1.Substring(7, 2);

            string ExpDisplay_h2 = h2n4 + h2n3 + h2n2;


            #region Future Exp
            int currentmonth = mont0;

            uint expiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[0]));
            string expiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, expiry).ToString("yyyyMMMdd");
            string sf12 = Convert.ToString(expiry1);
            string sf22 = sf12.Substring(0, 4);
            string sf32 = sf12.Substring(4, 3);
            string sf42 = sf12.Substring(7, 2);
            int montf = DateTime.ParseExact(sf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mffi1 = new System.Globalization.DateTimeFormatInfo();
            string monStringf = "";
            if (montf <= 9)
            {
                monStringf = "0" + Convert.ToString(montf);
            }
            else
            {
                monStringf = Convert.ToString(montf);
            }
            string sf52 = sf22 + monStringf + sf42;
            string selectFut = sf52;


            uint nxtexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[1]));
            string nxtexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, nxtexpiry).ToString("yyyyMMMdd");
            string nxtsf12 = Convert.ToString(nxtexpiry1);
            string nxtsf22 = nxtsf12.Substring(0, 4);
            string nxtsf32 = nxtsf12.Substring(4, 3);
            string nxtsf42 = nxtsf12.Substring(7, 2);
            int nxtmontf = DateTime.ParseExact(nxtsf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo nxtmffi1 = new System.Globalization.DateTimeFormatInfo();
            string nxtmonStringf = "";
            if (nxtmontf <= 9)
            {
                nxtmonStringf = "0" + Convert.ToString(nxtmontf);
            }
            else
            {
                nxtmonStringf = Convert.ToString(nxtmontf);
            }
            string nxtsf52 = nxtsf22 + nxtmonStringf + nxtsf42;


            if (currentmonth == nxtmontf)
                selectFut = nxtsf52;


            uint farexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[2]));
            string farexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, farexpiry).ToString("yyyyMMMdd");
            string farsf12 = Convert.ToString(farexpiry1);
            string farsf22 = farsf12.Substring(0, 4);
            string farsf32 = farsf12.Substring(4, 3);
            string farsf42 = farsf12.Substring(7, 2);
            int farmontf = DateTime.ParseExact(farsf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo farmffi1 = new System.Globalization.DateTimeFormatInfo();
            string farmonStringf = "";
            if (farmontf <= 9)
            {
                farmonStringf = "0" + Convert.ToString(farmontf);
            }
            else
            {
                farmonStringf = Convert.ToString(farmontf);
            }
            string farsf52 = farsf22 + farmonStringf + farsf42;
            if (currentmonth == farmontf)
                selectFut = farsf52;

            #endregion



            string Sym = Convert.ToString(cmbSymbol1.Text);
        

            #region Check Unique Id
            int StrikeGap = 0;
            int Leg1Strike = 0;
            int Leg3Strike = 0;
            StrikeGap = Math.Abs(Convert.ToInt32(cmbStrike1.Text) - Convert.ToInt32(cmbStrike2.Text));
            string txtG = Convert.ToString(StrikeGap);
            string _Gap = "";
            if (txtG.Length == 3)
                _Gap = "0" + Convert.ToString(txtG);
            else
                _Gap = Convert.ToString(txtG);
            Leg1Strike = Convert.ToInt32(cmbStrike1.Text);
            Leg3Strike = Convert.ToInt32(cmbStrike2.Text);





            if (Leg1Strike != Leg3Strike)
            {
                MessageBox.Show("Please check Strike");
                return;
            }

            string Strike1 = "";


            if (Leg1Strike > 9999)
            {
                Strike1 = Convert.ToString(Convert.ToInt32(Leg1Strike) / 100);
            }
            else
            {
                Strike1 = Convert.ToString(Convert.ToInt32(Leg1Strike) / 10);
            }

            UInt64 exp = Convert.ToUInt64(n5);
            int TokenNo = 0;
            string strFilterCheck = "";
            strFilterCheck = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
            DataRow[] drCheck = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck);
            foreach (DataRow dr in drCheck)
            {
                exp = Convert.ToUInt64(dr["SymbolDesc"]);
                TokenNo = Convert.ToInt32(dr["TokenNo"]);
            }

            UInt64 exp3 = Convert.ToUInt64(n52);
            int TokenNo3 = 0;
            string strFilterCheck1 = "";
            strFilterCheck1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
            DataRow[] drCheck1 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck1);
            foreach (DataRow dr in drCheck1)
            {
                exp3 = Convert.ToUInt64(dr["SymbolDesc"]);
                TokenNo3 = Convert.ToInt32(dr["TokenNo"]);
            }

            UInt64 Unique_id = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
            #endregion

            string strategy_type = TokenNo + "_" + TokenNo3;
            foreach (var watchT in AppGlobal.MarketWatch.Where(x => ((x.Leg1.ContractInfo.TokenNo + "_" + x.Leg1.ContractInfo.TokenNo) == Convert.ToString(strategy_type))))
            {
                if (watchT.StrategyId == 91 && watchT.Strategy == StrategyName)
                {
                    MessageBox.Show("This Rule Already Added with GUI id : " + watchT.uniqueId + " Strategy : " + watchT.Strategy);
                    return;
                }
            }

            if (AppGlobal.MarketWatch.Count() == 0)
            {
                return;
            }
            AppGlobal.StrategyRuleIndexNo = AppGlobal.StrategyRuleIndexNo + 1;

            string strType = "MainJodiStraddle_" + AppGlobal.StrategyRuleIndexNo + "_" + HedgeType;

            double DrawDown = Convert.ToDouble(txtRound1Q.Text);



            #region Straddle Spread Call

            #region Gui No is changing

            int flg_1 = 0;
            //int rowcount = 0;
            bool NextStrategy_1 = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg_1 == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;
                    watch.StrategyName = strType;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 0.05M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;
                    watch.Alert = Alert;



                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay_h;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.StrategyDrawDown = DrawDown;
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;

                    watch.Expiry2 = ExpDisplay2;
                    watch.Threshold = 2;
                    watch.Profit = 0;
                    watch.DrawDown = 0;
                   
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.Strategy_Type = strategy_type;
                    watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                    watch.IsStrikeReq = false;
                    watch.Hedgeflg = true;

                    watch.round1Percent = rnd1P;
                    watch.round2Percent = rnd2P;
                    watch.round3Percent = rnd3P;
                    watch.round4Percent = rnd4P;

                    watch.round1Point = rnd1Q;
                    watch.round2Point = rnd2Q;
                    watch.round3Point = rnd3Q;
                    watch.round4Point = rnd4Q;

                    if (rdoMain.Checked)
                        watch.Track = "Main";
                    else
                        watch.Track = "Hedge";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;


                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbHedgeExpiry1.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;


                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbHedgeStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2
                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;                    
                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                   

                    #region FutLeg
                    string strFilter2 = "";
                    
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    
                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);
                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);
                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }
                    }

                    #endregion


                    #region Unique ID

                    watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                    #endregion

                    AppGlobal.MarketWatch.Insert(i, watch);

                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Gray;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg_1 = 1;
                    NextStrategy_1 = false;
                }
            }
            #endregion

            if (NextStrategy_1)
            {
                #region Old add Code

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;
                watch.StrategyName = strType;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.StrategyDrawDown = DrawDown;
                watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay_h;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                watch.Threshold = 2;
                watch.Alert = Alert;
                              
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.IsStrikeReq = false;
                watch.Hedgeflg = true;
                watch.round1Percent = rnd1P;
                watch.round2Percent = rnd2P;
                watch.round3Percent = rnd3P;
                watch.round4Percent = rnd4P;

                watch.round1Point = rnd1Q;
                watch.round2Point = rnd2Q;
                watch.round3Point = rnd3Q;
                watch.round4Point = rnd4Q;

                if (rdoMain.Checked)
                    watch.Track = "Main";
                else
                    watch.Track = "Hedge";
                watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbHedgeExpiry1.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;

                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbHedgeStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                

                #region FutLeg

                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
               
                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                #region Unique ID

                watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Gray;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);
            #endregion

            #region Gui No is changing

            int flg = 0;
            //int rowcount = 0;
            bool NextStrategy = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;
                    watch.StrategyName = "MainJodiStraddle_" + AppGlobal.StrategyRuleIndexNo;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 0.05M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;
                    watch.Alert = Alert;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.StrategyDrawDown = DrawDown;
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.IsStrikeReq = false;
                    watch.Hedgeflg = true;

                    watch.round1Percent = rnd1P;
                    watch.round2Percent = rnd2P;
                    watch.round3Percent = rnd3P;
                    watch.round4Percent = rnd4P;

                    watch.round1Point = rnd1Q;
                    watch.round2Point = rnd2Q;
                    watch.round3Point = rnd3Q;
                    watch.round4Point = rnd4Q;

                    if (rdoMain.Checked)
                        watch.Track = "Main";
                    else
                        watch.Track = "Hedge";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbExpiry1.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;
                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2

                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;
                    
                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    

                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }

                    }

                    #endregion

                    #region Unique ID

                    watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                    #endregion

                    AppGlobal.MarketWatch.Insert(i, watch);
                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Aqua;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg = 1;
                    NextStrategy = false;
                }
            }
            #endregion

            if (NextStrategy)
            {
                #region Old add Code

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;
                watch.StrategyName = "MainJodiStraddle_" + AppGlobal.StrategyRuleIndexNo;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;
                watch.Alert = Alert;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.StrategyDrawDown = DrawDown;
                watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.IsStrikeReq = false;
                watch.Hedgeflg = true;
                watch.round1Percent = rnd1P;
                watch.round2Percent = rnd2P;
                watch.round3Percent = rnd3P;
                watch.round4Percent = rnd4P;

                watch.round1Point = rnd1Q;
                watch.round2Point = rnd2Q;
                watch.round3Point = rnd3Q;
                watch.round4Point = rnd4Q;


                if (rdoMain.Checked)
                    watch.Track = "Main";
                else
                    watch.Track = "Hedge";
                watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbExpiry1.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;




                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;               
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg
                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                
                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                #region Unique ID

                watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Aqua;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);

            #region Gui No is changing

            int flg1 = 0;
            //int rowcount = 0;
            bool NextStrategy1 = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg1 == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;

                    watch.StrategyName = "MainJodiStraddle_" + AppGlobal.StrategyRuleIndexNo;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 0.05M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;
                    watch.Alert = Alert;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.StrategyDrawDown = DrawDown;
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                   
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.IsStrikeReq = false;
                    watch.Hedgeflg = true;
                    watch.round1Percent = rnd1P;
                    watch.round2Percent = rnd2P;
                    watch.round3Percent = rnd3P;
                    watch.round4Percent = rnd4P;

                    watch.round1Point = rnd1Q;
                    watch.round2Point = rnd2Q;
                    watch.round3Point = rnd3Q;
                    watch.round4Point = rnd4Q;

                    if (rdoMain.Checked)
                        watch.Track = "Main";
                    else
                        watch.Track = "Hedge";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbExpiry2.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;


                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2

                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;
                    
                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    
                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }

                    }

                    #endregion

                    #region Unique ID

                    watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                    #endregion

                    AppGlobal.MarketWatch.Insert(i, watch);

                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Aqua;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg1 = 1;
                    NextStrategy1 = false;
                }
            }

            #endregion

            if (NextStrategy1)
            {
                #region Old add Code
                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;

                watch.StrategyName = "MainJodiStraddle_" + AppGlobal.StrategyRuleIndexNo;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.StrategyDrawDown = DrawDown;
                watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                watch.IsStrikeReq = false;
                watch.Hedgeflg = true;
                watch.Alert = Alert;
                watch.round1Percent = rnd1P;
                watch.round2Percent = rnd2P;
                watch.round3Percent = rnd3P;
                watch.round4Percent = rnd4P;

                watch.round1Point = rnd1Q;
                watch.round2Point = rnd2Q;
                watch.round3Point = rnd3Q;
                watch.round4Point = rnd4Q;


                if (rdoMain.Checked)
                    watch.Track = "Main";
                else
                    watch.Track = "Hedge";
                watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbExpiry2.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;




                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;

               
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg

                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                
                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                #region Unique ID

                watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Aqua;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);


            #region Straddle Spread Put

            #region Gui No is changing

            int flg_2 = 0;
            //int rowcount = 0;
            bool NextStrategy_2 = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg_2 == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;
                    watch.StrategyName = strType;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 999999.0M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;
                    watch.Alert = Alert;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay_h2;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.StrategyDrawDown = DrawDown;
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                    watch.IsStrikeReq = false;

                    watch.Hedgeflg = true;

                    watch.round1Percent = rnd1P;
                    watch.round2Percent = rnd2P;
                    watch.round3Percent = rnd3P;
                    watch.round4Percent = rnd4P;

                    watch.round1Point = rnd1Q;
                    watch.round2Point = rnd2Q;
                    watch.round3Point = rnd3Q;
                    watch.round4Point = rnd4Q;

                    if (rdoMain.Checked)
                        watch.Track = "Main";
                    else
                        watch.Track = "Hedge";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbHedgeExpiry2.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;


                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbHedgeStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2

                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;
                    
                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    

                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }

                    }

                    #endregion


                    #region Unique ID

                    watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                    #endregion

                    AppGlobal.MarketWatch.Insert(i, watch);

                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Gray;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg_2 = 1;
                    NextStrategy_2 = false;
                }
            }
            #endregion

            if (NextStrategy_2)
            {
                #region Old add Code

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;
                watch.StrategyName = strType;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;
                watch.Alert = Alert;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay_h2;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.StrategyDrawDown = DrawDown;
                watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                watch.IsStrikeReq = false;

                watch.Hedgeflg = true;
                watch.round1Percent = rnd1P;
                watch.round2Percent = rnd2P;
                watch.round3Percent = rnd3P;
                watch.round4Percent = rnd4P;

                watch.round1Point = rnd1Q;
                watch.round2Point = rnd2Q;
                watch.round3Point = rnd3Q;
                watch.round4Point = rnd4Q;


                if (rdoMain.Checked)
                    watch.Track = "Main";
                else
                    watch.Track = "Hedge";
                watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbHedgeExpiry2.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;




                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX"  + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbHedgeStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg

                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";                

                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion


                #region Unique ID
                watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Gray;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);


            #endregion
        }

        public void StrangleLegsAddWithHedge(string HedgeType, double rnd1P, double rnd1Q, double rnd2P, double rnd2Q, double rnd3P, double rnd3Q, double rnd4P, double rnd4Q)
        {
            string StrategyName = Convert.ToString(cmbStrategy.Text);
            string[] strategyArray = StrategyName.Split('_');
            int strategy_No = Convert.ToInt32(strategyArray[1]);

            bool Alert = false;

            if (chkAlert.Checked)
                Alert = true;

            string n1 = Convert.ToString(cmbExpiry1.Text);
            string n2 = n1.Substring(0, 4);
            string n3 = n1.Substring(4, 3);
            string n4 = n1.Substring(7, 2);
            int mont0 = DateTime.ParseExact(n3, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi0 = new System.Globalization.DateTimeFormatInfo();
            string monString0 = "";
            if (mont0 <= 9)
            {
                monString0 = "0" + Convert.ToString(mont0);
            }
            else
            {
                monString0 = Convert.ToString(mont0);
            }
            string n5 = n2 + monString0 + n4;
            string ExpDisplay = n4 + n3 + n2;
            string n12 = Convert.ToString(cmbExpiry2.Text);
            string n22 = n12.Substring(0, 4);
            string n32 = n12.Substring(4, 3);
            string n42 = n12.Substring(7, 2);
            int mont02 = DateTime.ParseExact(n32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mfi02 = new System.Globalization.DateTimeFormatInfo();
            string monString02 = "";
            if (mont02 <= 9)
            {
                monString02 = "0" + Convert.ToString(mont02);
            }
            else
            {
                monString02 = Convert.ToString(mont02);
            }
            string n52 = n22 + monString02 + n42;

            string ExpDisplay2 = n42 + n32 + n22;

            string hn1 = Convert.ToString(cmbHedgeExpiry1.Text);

            string hn2 = hn1.Substring(0, 4);
            string hn3 = hn1.Substring(4, 3);
            string hn4 = hn1.Substring(7, 2);

            string ExpDisplay_h = hn4 + hn3 + hn2;


            string h2n1 = Convert.ToString(cmbHedgeExpiry2.Text);

            string h2n2 = h2n1.Substring(0, 4);
            string h2n3 = h2n1.Substring(4, 3);
            string h2n4 = h2n1.Substring(7, 2);

            string ExpDisplay_h2 = h2n4 + h2n3 + h2n2;


            #region Future Exp
            int currentmonth = mont0;

            uint expiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[0]));
            string expiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, expiry).ToString("yyyyMMMdd");
            string sf12 = Convert.ToString(expiry1);
            string sf22 = sf12.Substring(0, 4);
            string sf32 = sf12.Substring(4, 3);
            string sf42 = sf12.Substring(7, 2);
            int montf = DateTime.ParseExact(sf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo mffi1 = new System.Globalization.DateTimeFormatInfo();
            string monStringf = "";
            if (montf <= 9)
            {
                monStringf = "0" + Convert.ToString(montf);
            }
            else
            {
                monStringf = Convert.ToString(montf);
            }
            string sf52 = sf22 + monStringf + sf42;
            string selectFut = sf52;


            uint nxtexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[1]));
            string nxtexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, nxtexpiry).ToString("yyyyMMMdd");
            string nxtsf12 = Convert.ToString(nxtexpiry1);
            string nxtsf22 = nxtsf12.Substring(0, 4);
            string nxtsf32 = nxtsf12.Substring(4, 3);
            string nxtsf42 = nxtsf12.Substring(7, 2);
            int nxtmontf = DateTime.ParseExact(nxtsf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo nxtmffi1 = new System.Globalization.DateTimeFormatInfo();
            string nxtmonStringf = "";
            if (nxtmontf <= 9)
            {
                nxtmonStringf = "0" + Convert.ToString(nxtmontf);
            }
            else
            {
                nxtmonStringf = Convert.ToString(nxtmontf);
            }
            string nxtsf52 = nxtsf22 + nxtmonStringf + nxtsf42;


            if (currentmonth == nxtmontf)
                selectFut = nxtsf52;


            uint farexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[2]));
            string farexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, farexpiry).ToString("yyyyMMMdd");
            string farsf12 = Convert.ToString(farexpiry1);
            string farsf22 = farsf12.Substring(0, 4);
            string farsf32 = farsf12.Substring(4, 3);
            string farsf42 = farsf12.Substring(7, 2);
            int farmontf = DateTime.ParseExact(farsf32, "MMM", new CultureInfo("en-US")).Month;
            System.Globalization.DateTimeFormatInfo farmffi1 = new System.Globalization.DateTimeFormatInfo();
            string farmonStringf = "";
            if (farmontf <= 9)
            {
                farmonStringf = "0" + Convert.ToString(farmontf);
            }
            else
            {
                farmonStringf = Convert.ToString(farmontf);
            }
            string farsf52 = farsf22 + farmonStringf + farsf42;
            if (currentmonth == farmontf)
                selectFut = farsf52;

            #endregion



            string Sym = Convert.ToString(cmbSymbol1.Text);


            #region Check Unique Id
            int StrikeGap = 0;
            int Leg1Strike = 0;
            int Leg3Strike = 0;
            StrikeGap = Math.Abs(Convert.ToInt32(cmbStrike1.Text) - Convert.ToInt32(cmbStrike2.Text));
            string txtG = Convert.ToString(StrikeGap);
            string _Gap = "";
            if (txtG.Length == 3)
                _Gap = "0" + Convert.ToString(txtG);
            else
                _Gap = Convert.ToString(txtG);
            Leg1Strike = Convert.ToInt32(cmbStrike1.Text);
            Leg3Strike = Convert.ToInt32(cmbStrike2.Text);





            //if (Leg1Strike != Leg3Strike)
            //{
            //    MessageBox.Show("Please check Strike");
            //    return;
            //}

            string Strike1 = "";


            if (Leg1Strike > 9999)
            {
                Strike1 = Convert.ToString(Convert.ToInt32(Leg1Strike) / 100);
            }
            else
            {
                Strike1 = Convert.ToString(Convert.ToInt32(Leg1Strike) / 10);
            }

            UInt64 exp = Convert.ToUInt64(n5);
            int TokenNo = 0;
            string strFilterCheck = "";
            strFilterCheck = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
            DataRow[] drCheck = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck);
            foreach (DataRow dr in drCheck)
            {
                exp = Convert.ToUInt64(dr["SymbolDesc"]);
                TokenNo = Convert.ToInt32(dr["TokenNo"]);
            }

            UInt64 exp3 = Convert.ToUInt64(n52);
            int TokenNo3 = 0;
            string strFilterCheck1 = "";
            strFilterCheck1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
            DataRow[] drCheck1 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck1);
            foreach (DataRow dr in drCheck1)
            {
                exp3 = Convert.ToUInt64(dr["SymbolDesc"]);
                TokenNo3 = Convert.ToInt32(dr["TokenNo"]);
            }

            UInt64 Unique_id = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
            #endregion

            string strategy_type = TokenNo + "_" + TokenNo3;
            foreach (var watchT in AppGlobal.MarketWatch.Where(x => ((x.Leg1.ContractInfo.TokenNo + "_" + x.Leg1.ContractInfo.TokenNo) == Convert.ToString(strategy_type))))
            {
                if (watchT.StrategyId == 91 && watchT.Strategy == StrategyName)
                {
                    MessageBox.Show("This Rule Already Added with GUI id : " + watchT.uniqueId + " Strategy : " + watchT.Strategy);
                    return;
                }
            }

            if (AppGlobal.MarketWatch.Count() == 0)
            {
                return;
            }
            AppGlobal.StrategyRuleIndexNo = AppGlobal.StrategyRuleIndexNo + 1;

            double DrawDown = Convert.ToDouble(txtDrawdown.Text);
            string strType = "MainJodiStrangle_" + AppGlobal.StrategyRuleIndexNo + "_" + HedgeType;


            #region Straddle Spread Call

            #region Gui No is changing

            int flg_1 = 0;
            //int rowcount = 0;
            bool NextStrategy_1 = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg_1 == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;
                    watch.StrategyName = strType;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 0.05M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay_h;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.StrategyDrawDown = DrawDown;
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                    watch.IsStrikeReq = false;
                    watch.Hedgeflg = true;
                    watch.Alert = Alert;

                    watch.round1Percent = rnd1P;
                    watch.round2Percent = rnd2P;
                    watch.round3Percent = rnd3P;
                    watch.round4Percent = rnd4P;

                    watch.round1Point = rnd1Q;
                    watch.round2Point = rnd2Q;
                    watch.round3Point = rnd3Q;
                    watch.round4Point = rnd4Q;


                    if (rdoMain.Checked)
                        watch.Track = "Main";
                    else
                        watch.Track = "Hedge";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;


                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbHedgeExpiry1.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;


                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbHedgeStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2
                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;
                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";

                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";

                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);
                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);
                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }
                    }

                    #endregion


                    AppGlobal.MarketWatch.Insert(i, watch);

                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Gray;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg_1 = 1;
                    NextStrategy_1 = false;
                }
            }
            #endregion

            if (NextStrategy_1)
            {
                #region Old add Code

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;
                watch.StrategyName = strType;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 999999.0M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay_h;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
               
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.StrategyDrawDown = DrawDown;
                watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                watch.IsStrikeReq = false;
                watch.Hedgeflg = true;
                watch.Alert = Alert;

                watch.round1Percent = rnd1P;
                watch.round2Percent = rnd2P;
                watch.round3Percent = rnd3P;
                watch.round4Percent = rnd4P;

                watch.round1Point = rnd1Q;
                watch.round2Point = rnd2Q;
                watch.round3Point = rnd3Q;
                watch.round4Point = rnd4Q;

                if (rdoMain.Checked)
                    watch.Track = "Main";
                else
                    watch.Track = "Hedge";
                watch.RowData.Cells[WatchConst.Track].Value = watch.Track;


                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbHedgeExpiry1.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;

                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbHedgeStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg

                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";

                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Gray;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);
            #endregion

            #region Gui No is changing

            int flg = 0;
            //int rowcount = 0;
            bool NextStrategy = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;
                    watch.StrategyName = "MainJodiStrangle_" + AppGlobal.StrategyRuleIndexNo;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 0.05M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.StrategyDrawDown = DrawDown;
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                    watch.IsStrikeReq = false;
                    watch.Hedgeflg = true;
                    watch.Alert = Alert;

                    watch.round1Percent = rnd1P;
                    watch.round2Percent = rnd2P;
                    watch.round3Percent = rnd3P;
                    watch.round4Percent = rnd4P;

                    watch.round1Point = rnd1Q;
                    watch.round2Point = rnd2Q;
                    watch.round3Point = rnd3Q;
                    watch.round4Point = rnd4Q;

                    if (rdoMain.Checked)
                        watch.Track = "Main";
                    else
                        watch.Track = "Hedge";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;


                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbExpiry1.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;
                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2

                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;

                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";


                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }

                    }

                    #endregion

                    AppGlobal.MarketWatch.Insert(i, watch);
                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Aqua;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg = 1;
                    NextStrategy = false;
                }
            }
            #endregion

            if (NextStrategy)
            {
                #region Old add Code

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;
                watch.StrategyName = "MainJodiStrangle_" + AppGlobal.StrategyRuleIndexNo;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
               
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.StrategyDrawDown = DrawDown;
                watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                watch.IsStrikeReq = false;
                watch.Hedgeflg = true;
                watch.Alert = Alert;

                watch.round1Percent = rnd1P;
                watch.round2Percent = rnd2P;
                watch.round3Percent = rnd3P;
                watch.round4Percent = rnd4P;

                watch.round1Point = rnd1Q;
                watch.round2Point = rnd2Q;
                watch.round3Point = rnd3Q;
                watch.round4Point = rnd4Q;

                if (rdoMain.Checked)
                    watch.Track = "Main";
                else
                    watch.Track = "Hedge";
                watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbExpiry1.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;
                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + "CE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg
                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";

                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Aqua;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);

            #region Gui No is changing

            int flg1 = 0;
            //int rowcount = 0;
            bool NextStrategy1 = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg1 == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;

                    watch.StrategyName = "MainJodiStraddle_" + AppGlobal.StrategyRuleIndexNo;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 0.05M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                   
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.StrategyDrawDown = DrawDown;
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                    watch.IsStrikeReq = false;
                    watch.Hedgeflg = true;
                    watch.Alert = Alert;

                    watch.round1Percent = rnd1P;
                    watch.round2Percent = rnd2P;
                    watch.round3Percent = rnd3P;
                    watch.round4Percent = rnd4P;

                    watch.round1Point = rnd1Q;
                    watch.round2Point = rnd2Q;
                    watch.round3Point = rnd3Q;
                    watch.round4Point = rnd4Q;

                    if (rdoMain.Checked)
                        watch.Track = "Main";
                    else
                        watch.Track = "Hedge";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;

                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbExpiry2.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;


                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2

                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;

                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";

                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }

                    }

                    #endregion


                    AppGlobal.MarketWatch.Insert(i, watch);

                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Aqua;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg1 = 1;
                    NextStrategy1 = false;
                }
            }

            #endregion

            if (NextStrategy1)
            {
                #region Old add Code
                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;

                watch.StrategyName = "MainJodiStrangle_" + AppGlobal.StrategyRuleIndexNo;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.StrategyDrawDown = DrawDown;
                watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                watch.IsStrikeReq = false;

                watch.Hedgeflg = true;
                watch.Alert = Alert;

                watch.round1Percent = rnd1P;
                watch.round2Percent = rnd2P;
                watch.round3Percent = rnd3P;
                watch.round4Percent = rnd4P;

                watch.round1Point = rnd1Q;
                watch.round2Point = rnd2Q;
                watch.round3Point = rnd3Q;
                watch.round4Point = rnd4Q;

                if (rdoMain.Checked)
                    watch.Track = "Main";
                else
                    watch.Track = "Hedge";
                watch.RowData.Cells[WatchConst.Track].Value = watch.Track;


                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbExpiry2.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;




                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;


                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg

                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";

                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Aqua;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);


            #region Straddle Spread Put

            #region Gui No is changing

            int flg_2 = 0;
            //int rowcount = 0;
            bool NextStrategy_2 = true;

            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watchfirst = new MarketWatch();
                watchfirst = AppGlobal.MarketWatch[i];

                if (flg_2 == 1)
                    continue;

                string _StrategyName = Convert.ToString(watchfirst.Strategy);
                string[] _strategyArray = _StrategyName.Split('_');
                int _strategy_No = Convert.ToInt32(_strategyArray[1]);
                if (_strategy_No > strategy_No)
                {
                    for (int j = i + 1; j < AppGlobal.MarketWatch.Count; j++)
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watchfirst);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watchfirst);
                        break;
                    }

                    #region Old add Code
                    MarketWatch watch = new MarketWatch();
                    //int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                    watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[i];
                    watch.SeqaureOff = 1;
                    watch.StrategyId = 91;
                    watch.StrategyName = strType;
                    watch.sendStrikeRequest = false;
                    watch.enterCount = 0;
                    watch.Wind = 999999.0M;
                    watch.unWind = 999999.0M;

                    watch.Over = 0;
                    watch.Round = 0;

                    int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                    string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                    AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                    int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                    string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                    AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                    int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                    watch.RemainingDay = maxRemainingDay;
                    watch.URem_Day = maxRemainingDay;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                    watch.Strategy = StrategyName;
                    watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                    watch.posInt = 0;
                    watch.avgPrice = 0;
                    watch.Ruleno = AppGlobal.RuleIndexNo;
                    watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                    watch.Gui_id = AppGlobal.GUI_ID;
                    watch.Expiry = ExpDisplay_h2;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.StrategyDrawDown = DrawDown;
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                    watch.IsStrikeReq = false;
                    watch.Hedgeflg = true;
                    watch.Alert = Alert;

                    watch.round1Percent = rnd1P;
                    watch.round2Percent = rnd2P;
                    watch.round3Percent = rnd3P;
                    watch.round4Percent = rnd4P;

                    watch.round1Point = rnd1Q;
                    watch.round2Point = rnd2Q;
                    watch.round3Point = rnd3Q;
                    watch.round4Point = rnd4Q;

                    if (rdoMain.Checked)
                        watch.Track = "Main";
                    else
                        watch.Track = "Hedge";
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;


                    #region Row 1

                    #region Leg1
                    string strFilter1 = "";

                    string s12 = Convert.ToString(cmbHedgeExpiry2.Text);
                    string s22 = s12.Substring(0, 4);
                    string s32 = s12.Substring(4, 3);
                    string s42 = s12.Substring(7, 2);
                    int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                    System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                    string monString = "";
                    if (mont <= 9)
                    {
                        monString = "0" + Convert.ToString(mont);
                    }
                    else
                    {
                        monString = Convert.ToString(mont);
                    }
                    string s52 = s22 + monString + s42;


                    strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbHedgeStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                    foreach (DataRow dr in dr11)
                    {
                        watch.Leg1 = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();
                        watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg1.ContractInfo.Series = Series1;
                        watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                        watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.Type = watch.Leg1.ContractInfo.Series;
                        watch.Leg1.Counter = 1;
                        watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                        watch.Leg1.Ratio = 1;
                        watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                        watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                        watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                        }
                    }
                    #endregion

                    #region Leg2

                    watch.Leg2 = new Straddle.AppClasses.Leg();
                    watch.Leg2.ContractInfo.TokenNo = "0";
                    watch.Leg2.Counter = 0;

                    #endregion

                    #region Leg3
                    watch.Leg3 = new Straddle.AppClasses.Leg();
                    watch.Leg3.ContractInfo.TokenNo = "0";
                    watch.Leg3.Counter = 0;
                    #endregion

                    #region Leg4
                    watch.Leg4 = new Straddle.AppClasses.Leg();
                    watch.Leg4.ContractInfo.TokenNo = "0";
                    watch.Leg4.Counter = 0;
                    #endregion

                    watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                    watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                    #region FutLeg
                    string strFilter2 = "";
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";


                    DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr1F)
                    {
                        watch.niftyLeg = new Straddle.AppClasses.Leg();
                        string InstrumentName1 = dr["InstrumentName"].ToString();
                        string StrikePrice1 = dr["StrikePrice"].ToString();
                        string Series1 = dr["Series"].ToString();
                        string PriceTick1 = dr["PriceTick"].ToString();
                        string LotSize1 = dr["LotSize"].ToString();
                        string SymbolDesc1 = dr["SymbolDesc"].ToString();
                        string TradingUnit1 = dr["TradingUnit"].ToString();
                        string Currency1 = dr["Currency"].ToString();
                        string PriceDivisor1 = dr["PriceDivisor"].ToString();
                        string ExchPointValue1 = dr["ExchPointValue"].ToString();
                        string Multiplier1 = dr["Multiplier"].ToString();
                        string DprHigh1 = dr["DprHigh"].ToString();
                        string DprLow1 = dr["DprLow"].ToString();
                        string ClosePrice1 = dr["ClosePrice"].ToString();
                        string RBIViolation1 = dr["RBIViolation"].ToString();
                        string ISINNumber1 = dr["ISINNumber"].ToString();
                        string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                        string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                        string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                        string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                        string IsRBIViolation1 = dr["RBIViolation"].ToString();

                        watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.niftyLeg.ContractInfo.Series = Series1;
                        watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                        watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                        watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                        watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                        watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                        AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                        }

                    }

                    #endregion


                    AppGlobal.MarketWatch.Insert(i, watch);

                    #endregion

                    watch.Checked = true;
                    DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                    if (watch.Checked)
                    {
                        ToggleButton.Value = "ON";
                        ToggleButton.Style.ForeColor = Color.Green;
                    }
                    else
                    {
                        ToggleButton.Value = "OFF";
                        ToggleButton.Style.ForeColor = Color.Red;
                    }
                    ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.Checked] = ToggleButton;

                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Gray;
                    AppGlobal.RuleIndexNo++;
                    #endregion

                    flg_2 = 1;
                    NextStrategy_2 = false;
                }
            }
            #endregion

            if (NextStrategy_2)
            {
                #region Old add Code

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                watch.SeqaureOff = 1;
                watch.StrategyId = 91;
                watch.StrategyName = strType;
                watch.sendStrikeRequest = false;
                watch.enterCount = 0;
                watch.Wind = 0.05M;
                watch.unWind = 999999.0M;

                watch.Over = 0;
                watch.Round = 0;

                int rem = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n1));
                string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem));
                AppGlobal.RemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month));

                int rem1 = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(n12));
                string month1 = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, rem1));
                AppGlobal.URemainDay = CalculatorUtils.CalculateDay(Convert.ToDateTime(month1));
                int maxRemainingDay = Math.Max((int)AppGlobal.RemainDay, (int)AppGlobal.URemainDay);
                watch.RemainingDay = maxRemainingDay;
                watch.URem_Day = maxRemainingDay;
                watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Strategy = StrategyName;
                watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                watch.posInt = 0;
                watch.avgPrice = 0;
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.Expiry = ExpDisplay_h2;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.StrategyDrawDown = DrawDown;
                watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                watch.IsStrikeReq = false;
                watch.Hedgeflg = true;
                watch.Alert = Alert;

                watch.round1Percent = rnd1P;
                watch.round2Percent = rnd2P;
                watch.round3Percent = rnd3P;
                watch.round4Percent = rnd4P;

                watch.round1Point = rnd1Q;
                watch.round2Point = rnd2Q;
                watch.round3Point = rnd3Q;
                watch.round4Point = rnd4Q;


                if (rdoMain.Checked)
                    watch.Track = "Main";
                else
                    watch.Track = "Hedge";
                watch.RowData.Cells[WatchConst.Track].Value = watch.Track;


                #region Row 1

                #region Leg1
                string strFilter1 = "";

                string s12 = Convert.ToString(cmbHedgeExpiry2.Text);
                string s22 = s12.Substring(0, 4);
                string s32 = s12.Substring(4, 3);
                string s42 = s12.Substring(7, 2);
                int mont = DateTime.ParseExact(s32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mfi1 = new System.Globalization.DateTimeFormatInfo();
                string monString = "";
                if (mont <= 9)
                {
                    monString = "0" + Convert.ToString(mont);
                }
                else
                {
                    monString = Convert.ToString(mont);
                }
                string s52 = s22 + monString + s42;




                strFilter1 = DBConst.InstrumentName + " = '" + "OPTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbHedgeSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + s52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbHedgeStrike2.Text) + "' AND " + DBConst.Series + "= '" + "PE" + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter1);
                foreach (DataRow dr in dr11)
                {
                    watch.Leg1 = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();
                    watch.Leg1.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg1.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg1.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg1.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg1.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg1.ContractInfo.Series = Series1;
                    watch.Leg1.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg1.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg1.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg1.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg1.GatewayId);
                    watch.Leg1.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg1.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.Type = watch.Leg1.ContractInfo.Series;
                    watch.Leg1.Counter = 1;
                    watch.Leg1.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);//dr["SymbolDesc"];//(UInt64)ArisDev.ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr["ExpiryDate"].ToString()));
                    watch.Leg1.Ratio = 1;
                    watch.RowData.Cells[WatchConst.Ratio1].Value = watch.Leg1.Ratio;
                    watch.RowData.Cells[WatchConst.L1Series].Value = watch.Leg1.ContractInfo.Series;
                    watch.RowData.Cells[WatchConst.L1Strike].Value = watch.Leg1.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.avgPrice;
                    watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                    watch.RowData.Cells[WatchConst.Token].Value = watch.Leg1.ContractInfo.TokenNo;

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                    }
                }
                #endregion

                #region Leg2
                string strFilter2 = "";
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;
                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;
                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;
                #endregion

                watch.strikediff = Math.Abs(Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) - Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice));
                watch.RowData.Cells[WatchConst.StrikeDiff].Value = watch.strikediff;

                #region FutLeg

                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";

                DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr1F)
                {
                    watch.niftyLeg = new Straddle.AppClasses.Leg();
                    string InstrumentName1 = dr["InstrumentName"].ToString();
                    string StrikePrice1 = dr["StrikePrice"].ToString();
                    string Series1 = dr["Series"].ToString();
                    string PriceTick1 = dr["PriceTick"].ToString();
                    string LotSize1 = dr["LotSize"].ToString();
                    string SymbolDesc1 = dr["SymbolDesc"].ToString();
                    string TradingUnit1 = dr["TradingUnit"].ToString();
                    string Currency1 = dr["Currency"].ToString();
                    string PriceDivisor1 = dr["PriceDivisor"].ToString();
                    string ExchPointValue1 = dr["ExchPointValue"].ToString();
                    string Multiplier1 = dr["Multiplier"].ToString();
                    string DprHigh1 = dr["DprHigh"].ToString();
                    string DprLow1 = dr["DprLow"].ToString();
                    string ClosePrice1 = dr["ClosePrice"].ToString();
                    string RBIViolation1 = dr["RBIViolation"].ToString();
                    string ISINNumber1 = dr["ISINNumber"].ToString();
                    string MaxSingleTransactionQty1 = dr["MaxSingleTransactionQty"].ToString();
                    string MaxSingleTransactionValue1 = dr["MaxSingleTransactionValue"].ToString();
                    string PermittedToTrade1 = dr["PermittedToTrade"].ToString();
                    string IsAutoAllowed1 = dr["IsAutoAllowed"].ToString();
                    string IsRBIViolation1 = dr["RBIViolation"].ToString();

                    watch.niftyLeg.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.niftyLeg.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.niftyLeg.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.niftyLeg.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.niftyLeg.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.niftyLeg.ContractInfo.Series = Series1;
                    watch.niftyLeg.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1)) / watch.niftyLeg.ContractInfo.PriceDivisor;
                    watch.niftyLeg.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.niftyLeg.ContractInfo.InstrumentName = InstrumentName1;
                    watch.niftyLeg.ContDetail.LotSize = Convert.ToInt32(LotSize1);

                    watch.niftyLeg.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.niftyLeg.GatewayId);
                    watch.niftyLeg.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);

                    AppGlobal.FutToken = Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo);

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                    }

                }

                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);

                #endregion

                watch.Checked = true;
                DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                if (watch.Checked)
                {
                    ToggleButton.Value = "ON";
                    ToggleButton.Style.ForeColor = Color.Green;
                }
                else
                {
                    ToggleButton.Value = "OFF";
                    ToggleButton.Style.ForeColor = Color.Red;
                }
                ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;

                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.Gray;
                AppGlobal.RuleIndexNo++;
                #endregion
            }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            AppGlobal.frmWatch.AssignMarketStructValue_1(AppGlobal.MarketWatch);


            #endregion
        }

        private void chkHedgeJodi_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkHedgeJodi.Checked)
            {
                cmbHedgeSymbol1.Enabled = false;
                cmbHedgeSymbol2.Enabled = false;
                cmbHedgeExpiry1.Enabled = false;
                cmbHedgeExpiry2.Enabled = false;
                cmbHedgeStrike1.Enabled = false;
                cmbHedgeStrike2.Enabled = false;

            }
            else 
            {
                if (rdoHedgeStraddle.Checked)
                {
                    cmbHedgeSymbol1.Enabled = true;
                    cmbHedgeExpiry1.Enabled = true;
                    cmbHedgeStrike1.Enabled = true;
                    cmbHedgeSymbol2.Enabled = false;
                    cmbHedgeExpiry2.Enabled = false;
                    cmbHedgeStrike2.Enabled = false;
                }
                else if(rdoHedgeStrangle.Checked)
                {
                    cmbHedgeSymbol1.Enabled = true;
                    cmbHedgeExpiry1.Enabled = true;
                    cmbHedgeStrike1.Enabled = true;
                    cmbHedgeStrike2.Enabled = true;
                    cmbHedgeSymbol2.Enabled = false;
                    cmbHedgeExpiry2.Enabled = false;
                }
            }

        }

        private void cmbStrike1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (rdoStraddle.Checked)
            {
                string k = cmbStrike1.Text;
                cmbStrike2.Text = k;
            } 
        }

        private void cmbHedgeStrike1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (chkHedgeJodi.Checked)
            {
                if (rdoHedgeStraddle.Checked)
                {
                    string k = cmbHedgeStrike1.Text;
                    cmbHedgeStrike2.Text = k;
                }
            }
        }

        private void rdoHedgeStraddle_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoHedgeStraddle.Checked)
            {
                cmbHedgeSymbol1.Enabled = true;
                cmbHedgeExpiry1.Enabled = true;
                cmbHedgeStrike1.Enabled = true;
                cmbHedgeSymbol2.Enabled = false;
                cmbHedgeExpiry2.Enabled = false;
                cmbHedgeStrike2.Enabled = false;
            }
            else if (rdoHedgeStrangle.Checked)
            {
                cmbHedgeSymbol1.Enabled = true;
                cmbHedgeExpiry1.Enabled = true;
                cmbHedgeStrike1.Enabled = true;
                cmbHedgeStrike2.Enabled = true;
                cmbHedgeSymbol2.Enabled = false;
                cmbHedgeExpiry2.Enabled = false;
            }
        }

        private void rdoHedgeStrangle_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoHedgeStraddle.Checked)
            {
                cmbHedgeSymbol1.Enabled = true;
                cmbHedgeExpiry1.Enabled = true;
                cmbHedgeStrike1.Enabled = true;
                cmbHedgeSymbol2.Enabled = false;
                cmbHedgeExpiry2.Enabled = false;
                cmbHedgeStrike2.Enabled = false;
            }
            else if (rdoHedgeStrangle.Checked)
            {
                cmbHedgeSymbol1.Enabled = true;
                cmbHedgeExpiry1.Enabled = true;
                cmbHedgeStrike1.Enabled = true;
                cmbHedgeStrike2.Enabled = true;
                cmbHedgeSymbol2.Enabled = false;
                cmbHedgeExpiry2.Enabled = false;
                
            }
        }

    }
}
