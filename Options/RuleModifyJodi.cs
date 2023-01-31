using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;
using System.Globalization;
using MTCommon;
using ArisDev;

namespace Straddle
{
    public partial class RuleModifyJodi : Form
    {

        #region Variable
        string[] threeExpiry;
        List<string> _StrategyList;

        string mainCallLeg = "";
        string mainPutLeg = "";
        string hedgeCallLeg = "";
        string hedgePutLeg = "";


        #endregion

        public RuleModifyJodi()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyPress += new KeyPressEventHandler(RuleModifyJodi_KeyPress);
        }

        void RuleModifyJodi_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._ruleModifyJodi = null;
                Close();
            }
        }

        private void RuleModifyJodi_Load(object sender, EventArgs e)
        {
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
            #endregion

            string Symbol1 = "";
            string Symbol2 = "";
            string Strike1 = "";
            string Strike2 = "";
            string Expiry1 = "";
            string Expiry2 = "";
            string HedgeSymbol1 = "";
            string HedgeSymbol2 = "";
            string HedgeStrike1 = "";
            string HedgeStrike2 = "";
            string HedgeExpiry1 = "";
            string HedgeExpiry2 = "";
            string round1P = "";
            string round2P = "";
            string round3P = "";
            string round4P = "";
            string round1Q = "";
            string round2Q = "";
            string round3Q = "";
            string round4Q = "";
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            lblStrategy.Text = watch.Strategy;
            lblruleName.Text = watch.Strategy_Type;
            string _strategyName = watch.StrategyName;
            const char fieldSeparator = '_';
            List<string> split = _strategyName.Split(fieldSeparator).ToList();
            string _findStrategy = split[0] + "_" + split[1];
            lblStrategyInfo.Text = _findStrategy;

            if (watch.StrategyId != 0)
            {
                int level = watch.AlertLevel;
                if (level == 1)
                {
                    txtRound1P.Enabled = true;
                    txtRound2P.Enabled = true;
                    txtRound3P.Enabled = true;
                    txtRound4P.Enabled = true;
                    txtRound1Q.Enabled = true;
                    txtRound2Q.Enabled = true;
                    txtRound3Q.Enabled = true;
                    txtRound4Q.Enabled = true;
                }
                else if (level == 2)
                {
                    txtRound1P.Enabled = false;
                    txtRound2P.Enabled = false;
                    txtRound3P.Enabled = false;
                    txtRound4P.Enabled = false;
                    txtRound1Q.Enabled = false;
                    txtRound2Q.Enabled = true;
                    txtRound3Q.Enabled = true;
                    txtRound4Q.Enabled = true;
                }
                else if (level == 3)
                {
                    txtRound1P.Enabled = false;
                    txtRound2P.Enabled = false;
                    txtRound3P.Enabled = false;
                    txtRound4P.Enabled = false;
                    txtRound1Q.Enabled = false;
                    txtRound2Q.Enabled = false;
                    txtRound3Q.Enabled = true;
                    txtRound4Q.Enabled = true;
                }
                else if (level == 4)
                {
                    txtRound1P.Enabled = false;
                    txtRound2P.Enabled = false;
                    txtRound3P.Enabled = false;
                    txtRound4P.Enabled = false;
                    txtRound1Q.Enabled = false;
                    txtRound2Q.Enabled = false;
                    txtRound3Q.Enabled = false;
                    txtRound4Q.Enabled = true; 
                }
                int count = AppGlobal.MarketWatch.Where(x => x.StrategyName.Contains(_findStrategy)).Count();
                if (count == 2)
                {
                    chkHedgeJodi.Checked = false;
                    cmbHedgeSymbol1.Enabled = false;
                    cmbHedgeSymbol2.Enabled = false;
                    cmbHedgeStrike1.Enabled = false;
                    cmbHedgeStrike2.Enabled = false;
                    cmbHedgeExpiry1.Enabled = false;
                    cmbSymbol2.Enabled = false;
                    if (watch.StrategyName.Contains("MainJodiStraddle"))
                        rdoStraddle.Checked = true;
                    else
                        rdoStrangle.Checked = true;
                    //cmbHedgeExpiry2.Enabled = false;
                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => x.StrategyName.Contains(_findStrategy)))
                    {
                        if (watch1.Leg1.ContractInfo.Series == "CE")
                        {
                            Symbol1 = Convert.ToString(watch1.Leg1.ContractInfo.Symbol);
                            string n1 = Convert.ToString(watch1.Expiry);
                            string n2 = n1.Substring(0, 2);
                            string n3 = n1.Substring(2, 3);
                            string n4 = n1.Substring(5, 4);
                            string _expiry = n4 + n3 + n2;
                            Expiry1 = _expiry.ToString();
                            Strike1 = Convert.ToString(watch1.Leg1.ContractInfo.StrikePrice);
                            mainCallLeg = Convert.ToString(watch1.Leg1.ContractInfo.TokenNo);
                        }
                        else if (watch1.Leg1.ContractInfo.Series == "PE")
                        {
                            Symbol2 = Convert.ToString(watch1.Leg1.ContractInfo.Symbol);
                            string n1 = Convert.ToString(watch1.Expiry);
                            string n2 = n1.Substring(0, 2);
                            string n3 = n1.Substring(2, 3);
                            string n4 = n1.Substring(5, 4);
                            string _expiry = n4 + n3 + n2;
                            Expiry2 = _expiry.ToString();
                            Strike2 = Convert.ToString(watch1.Leg1.ContractInfo.StrikePrice);
                            mainPutLeg = Convert.ToString(watch1.Leg1.ContractInfo.TokenNo);
                        }
                    }
                    cmbSymbol1.Text = Symbol1.ToString();
                    cmbExpiry1.Text = Expiry1.ToString();
                    cmbStrike1.Text = Strike1.ToString();
                    cmbSymbol2.Text = Symbol2.ToString();
                    cmbExpiry2.Text = Expiry2.ToString();
                    cmbStrike2.Text = Strike2.ToString();
                }
                else if (count == 4)
                {                    
                    cmbSymbol2.Enabled = false;
                    cmbHedgeSymbol1.Enabled = true;
                    cmbHedgeSymbol2.Enabled = false;
                    cmbHedgeStrike1.Enabled = true;
                    cmbHedgeStrike2.Enabled = false;
                    cmbHedgeExpiry1.Enabled = true;
                    if (watch.StrategyName.Contains("MainJodiStraddle"))
                        rdoStraddle.Checked = true;
                    else
                        rdoStrangle.Checked = true;                  
                    if (watch.Track == "Main")
                        rdoMain.Checked = true;
                    else
                        rdoHedge.Checked = true;
                    bool HedgeRule = true;
                    bool chkhedgeflg = false;
                    bool chkAlertflg = false;
                    string strategyName = watch.StrategyName.ToString();
                    txtDrawdown.Text = watch.StrategyDrawDown.ToString();
                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => x.StrategyName.Contains(_findStrategy)))
                    {
                        if (watch1.StrategyName.Contains("_Straddle"))
                        {
                            if (watch1.Leg1.ContractInfo.Series == "CE")
                            {
                                HedgeSymbol1 = Convert.ToString(watch1.Leg1.ContractInfo.Symbol);
                                string n1 = Convert.ToString(watch1.Expiry);
                                string n2 = n1.Substring(0, 2);
                                string n3 = n1.Substring(2, 3);
                                string n4 = n1.Substring(5, 4);
                                string _expiry = n4 + n3 + n2;
                                HedgeExpiry1 = _expiry.ToString();
                                HedgeStrike1 = Convert.ToString(watch1.Leg1.ContractInfo.StrikePrice);
                                hedgeCallLeg = Convert.ToString(watch1.Leg1.ContractInfo.TokenNo);
                                round1P = Convert.ToString(watch1.round1Percent);
                                round2P = Convert.ToString(watch1.round2Percent);
                                round3P = Convert.ToString(watch1.round3Percent);
                                round4P = Convert.ToString(watch1.round4Percent);

                                round1Q = Convert.ToString(watch1.round1Point);
                                round2Q = Convert.ToString(watch1.round2Point);
                                round3Q = Convert.ToString(watch1.round3Point);
                                round4Q = Convert.ToString(watch1.round4Point);
                            }
                            else if (watch1.Leg1.ContractInfo.Series == "PE")
                            {
                                HedgeSymbol2 = Convert.ToString(watch1.Leg1.ContractInfo.Symbol);
                                string n1 = Convert.ToString(watch1.Expiry);
                                string n2 = n1.Substring(0, 2);
                                string n3 = n1.Substring(2, 3);
                                string n4 = n1.Substring(5, 4);
                                string _expiry = n4 + n3 + n2;
                                HedgeExpiry2 = _expiry.ToString();
                                HedgeStrike2 = Convert.ToString(watch1.Leg1.ContractInfo.StrikePrice);
                                hedgePutLeg = Convert.ToString(watch1.Leg1.ContractInfo.TokenNo);
                                chkAlertflg = watch.Alert;

                            }
                        }
                        else if (watch1.StrategyName.Contains("_Strangle"))
                        {
                            if (watch1.Leg1.ContractInfo.Series == "CE")
                            {
                                HedgeSymbol1 = Convert.ToString(watch1.Leg1.ContractInfo.Symbol);
                                string n1 = Convert.ToString(watch1.Expiry);
                                string n2 = n1.Substring(0, 2);
                                string n3 = n1.Substring(2, 3);
                                string n4 = n1.Substring(5, 4);
                                string _expiry = n4 + n3 + n2;
                                HedgeExpiry1 = _expiry.ToString();
                                HedgeStrike1 = Convert.ToString(watch1.Leg1.ContractInfo.StrikePrice);
                                hedgeCallLeg = Convert.ToString(watch1.Leg1.ContractInfo.TokenNo);
                                round1P = Convert.ToString(watch1.round1Percent);
                                round2P = Convert.ToString(watch1.round2Percent);
                                round3P = Convert.ToString(watch1.round3Percent);
                                round4P = Convert.ToString(watch1.round4Percent);
                                round1Q = Convert.ToString(watch1.round1Point);
                                round2Q = Convert.ToString(watch1.round2Point);
                                round3Q = Convert.ToString(watch1.round3Point);
                                round4Q = Convert.ToString(watch1.round4Point);
                            }
                            else if (watch1.Leg1.ContractInfo.Series == "PE")
                            {
                                HedgeSymbol2 = Convert.ToString(watch1.Leg1.ContractInfo.Symbol);
                                string n1 = Convert.ToString(watch1.Expiry);
                                string n2 = n1.Substring(0, 2);
                                string n3 = n1.Substring(2, 3);
                                string n4 = n1.Substring(5, 4);
                                string _expiry = n4 + n3 + n2;
                                HedgeExpiry2 = _expiry.ToString();
                                HedgeStrike2 = Convert.ToString(watch1.Leg1.ContractInfo.StrikePrice);
                                hedgePutLeg = Convert.ToString(watch1.Leg1.ContractInfo.TokenNo);
                                chkAlertflg = watch.Alert;
                            }
                            HedgeRule = false;
                        }
                        else
                        {
                            if (watch1.Leg1.ContractInfo.Series == "CE")
                            {
                                Symbol1 = Convert.ToString(watch1.Leg1.ContractInfo.Symbol);
                                string n1 = Convert.ToString(watch1.Expiry);
                                string n2 = n1.Substring(0, 2);
                                string n3 = n1.Substring(2, 3);
                                string n4 = n1.Substring(5, 4);
                                string _expiry = n4 + n3 + n2;
                                Expiry1 = _expiry.ToString();
                                Strike1 = Convert.ToString(watch1.Leg1.ContractInfo.StrikePrice);
                                mainCallLeg = Convert.ToString(watch1.Leg1.ContractInfo.TokenNo);
                            }
                            else if (watch1.Leg1.ContractInfo.Series == "PE")
                            {
                                Symbol2 = Convert.ToString(watch1.Leg1.ContractInfo.Symbol);
                                string n1 = Convert.ToString(watch1.Expiry);
                                string n2 = n1.Substring(0, 2);
                                string n3 = n1.Substring(2, 3);
                                string n4 = n1.Substring(5, 4);
                                string _expiry = n4 + n3 + n2;
                                Expiry2 = _expiry.ToString();
                                Strike2 = Convert.ToString(watch1.Leg1.ContractInfo.StrikePrice);
                                mainPutLeg = Convert.ToString(watch1.Leg1.ContractInfo.TokenNo);
                            }
                        }
                        chkhedgeflg = watch.Hedgeflg;
                    }
                    cmbSymbol1.Text = Symbol1.ToString();
                    cmbExpiry1.Text = Expiry1.ToString();
                    cmbStrike1.Text = Strike1.ToString();
                    cmbSymbol2.Text = Symbol2.ToString();
                    cmbExpiry2.Text = Expiry2.ToString();
                    cmbStrike2.Text = Strike2.ToString();
                    cmbHedgeSymbol1.Text = HedgeSymbol1.ToString();
                    cmbHedgeExpiry1.Text = HedgeExpiry1.ToString();
                    cmbHedgeStrike1.Text = HedgeStrike1.ToString();
                    cmbHedgeSymbol2.Text = HedgeSymbol2.ToString();
                    cmbHedgeExpiry2.Text = HedgeExpiry2.ToString();
                    cmbHedgeStrike2.Text = HedgeStrike2.ToString();
                    txtRound1P.Text = round1P.ToString();
                    txtRound2P.Text = round2P.ToString();
                    txtRound3P.Text = round3P.ToString();
                    txtRound4P.Text = round4P.ToString();
                    txtRound1Q.Text = round1Q.ToString();
                    txtRound2Q.Text = round2Q.ToString();
                    txtRound3Q.Text = round3Q.ToString();
                    txtRound4Q.Text = round4Q.ToString();
                    if (chkAlertflg)
                        chkAlert.Checked = true;
                    if (chkhedgeflg)
                    {
                        chkHedgeJodi.Checked = true;
                        if (HedgeRule)
                            rdoHedgeStraddle.Checked = true;
                        else
                            rdoHedgeStrangle.Checked = true;
                    }
                    else
                    {
                        chkHedgeJodi.Checked = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("Please Select Proper Rule!!!!");
                return;
            }
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

        private void RuleModifyJodi_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._ruleModifyJodi = null;
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

        private void cmbStrike1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (rdoStraddle.Checked)
            {
                string k = cmbStrike1.Text;
                cmbStrike2.Text = k;
            }
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

        private void addRule1_Click(object sender, EventArgs e)
        {
            string Strategy_Type = lblStrategyInfo.Text;
            double hedgelegce = 0;
            double hedgelegpe = 0;
            double mainlegce = 0;
            double mainlegpe = 0;

            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(Strategy_Type))))
            {
                if (watch.Track != "None")
                {
                    if (rdoMain.Checked)
                    {
                        watch.Track = "Main";

                        if(watch.StrategyName == Strategy_Type && watch.Leg1.ContractInfo.Series == "CE")
                        {
                            if (watch.posInt != 0)
                                mainlegce = watch.NetAvgPrice;
                        }
                        if (watch.StrategyName == Strategy_Type && watch.Leg1.ContractInfo.Series == "PE")
                        {
                            if (watch.posInt != 0)
                                mainlegpe = watch.NetAvgPrice;
                        }
                    }
                    else
                    {
                        watch.Track = "Hedge";
                        if (watch.StrategyName.Contains("_Straddle") || watch.StrategyName.Contains("_Strangle") && watch.Leg1.ContractInfo.Series == "CE")
                            hedgelegce = watch.MktWind;
                        if (watch.StrategyName.Contains("_Straddle") || watch.StrategyName.Contains("_Strangle") && watch.Leg1.ContractInfo.Series == "PE")
                            hedgelegpe = watch.MktWind;                        
                    }
                    watch.RowData.Cells[WatchConst.Track].Value = watch.Track;
                    watch.StrategyDrawDown = Convert.ToDouble(txtDrawdown.Text);
                    watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                }
                if (chkHedgeJodi.Checked)
                {
                    watch.Hedgeflg = true;    
                }
                else
                {
                    watch.Hedgeflg = false;
                }
            }
            if (chkHedgeJodi.Checked)
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
                if (rdoHedge.Checked)
                {
                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(Strategy_Type))))
                    {
                        if (hedgelegce != 0 && hedgelegpe != 0)
                        {
                            double straddleavg = Math.Round(hedgelegce + hedgelegpe, 2);
                            int level = watch1.AlertLevel;
                            if (level == 1)
                            {
                                watch1.StraddlAvg = Math.Round(hedgelegce + hedgelegpe, 2);
                                watch1.prvStraddleAvg = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.StrategyAvg].Value = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch1.prvStraddleAvg;

                                watch1.round1Percent = round1P;
                                watch1.round2Percent = round2P;
                                watch1.round3Percent = round3P;
                                watch1.round4Percent = round4P;

                                watch1.round1Point = round1Q;
                                watch1.round2Point = round2Q;
                                watch1.round3Point = round3Q;
                                watch1.round4Point = round4Q;

                                watch1.StrategyDrawDown = watch1.round1Point;
                                watch1.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch1.round1Point;

                                if (chkAlert.Checked)
                                    watch1.Alert = true;
                                else
                                    watch1.Alert = false;
                            }
                            else if (level == 2)
                            {
                                watch1.prvStraddleAvg = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.StrategyAvg].Value = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch1.prvStraddleAvg;
                                watch1.round2Percent = round2P;
                                watch1.round3Percent = round3P;
                                watch1.round4Percent = round4P;
                                watch1.StrategyDrawDown = watch1.round2Point;
                                watch1.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch1.StrategyDrawDown;
                            }
                            else if (level == 3)
                            {
                                watch1.prvStraddleAvg = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.StrategyAvg].Value = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch1.prvStraddleAvg;
                                watch1.round3Percent = round3P;
                                watch1.round4Percent = round4P;
                                watch1.StrategyDrawDown = watch1.round3Point;
                                watch1.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch1.StrategyDrawDown;
                            }
                            else if (level == 4)
                            {
                                watch1.prvStraddleAvg = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.StrategyAvg].Value = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch1.prvStraddleAvg;
                                watch1.round4Percent = round4P;
                                watch1.StrategyDrawDown = watch1.round4Point;
                                watch1.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch1.StrategyDrawDown;
                            }

                        }
                    }
                }
                else if (rdoMain.Checked)
                {
                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(Strategy_Type))))
                    {
                        if (mainlegce != 0 && mainlegpe != 0)
                        {
                            double straddleavg = Math.Round(mainlegce + mainlegpe, 2);
                            int level = watch1.AlertLevel;

                            if (level == 1)
                            {
                                watch1.StraddlAvg = Math.Round(mainlegce + mainlegpe, 2);
                                watch1.prvStraddleAvg = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.StrategyAvg].Value = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch1.prvStraddleAvg;
                                watch1.round1Percent = round1P;
                                watch1.round2Percent = round2P;
                                watch1.round3Percent = round3P;
                                watch1.round4Percent = round4P;
                                watch1.round1Point = round1Q;
                                watch1.round2Point = round2Q;
                                watch1.round3Point = round3Q;
                                watch1.round4Point = round4Q;
                                watch1.StrategyDrawDown = watch1.round1Point;
                                watch1.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch1.round1Point;
                                if (chkAlert.Checked)
                                    watch1.Alert = true;
                                else
                                    watch1.Alert = false;
                            }
                            else if (level == 2)
                            {
                                watch1.prvStraddleAvg = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.StrategyAvg].Value = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch1.prvStraddleAvg;
                                watch1.round2Percent = round2P;
                                watch1.round3Percent = round3P;
                                watch1.round4Percent = round4P;
                                watch1.StrategyDrawDown = watch1.round2Point;
                                watch1.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch1.StrategyDrawDown;
                            }
                            else if (level == 3)
                            {
                                watch1.prvStraddleAvg = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.StrategyAvg].Value = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch1.prvStraddleAvg;
                                watch1.round3Percent = round3P;
                                watch1.round4Percent = round4P;
                                watch1.StrategyDrawDown = watch1.round3Point;
                                watch1.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch1.StrategyDrawDown;
                            }
                            else if (level == 4)
                            {
                                watch1.prvStraddleAvg = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.StrategyAvg].Value = watch1.StraddlAvg;
                                watch1.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch1.prvStraddleAvg;                               
                                watch1.round4Percent = round4P;
                                watch1.StrategyDrawDown = watch1.round4Point;
                                watch1.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch1.StrategyDrawDown; 
                            }                           
                        }
                    }
                }
            }
        }
    }
}
