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
using MTCommon;
using System.Globalization;

namespace Straddle
{
    public partial class MainStraddle : Form
    {
        #region Variable
        string[] threeExpiry;
        List<string> _StrategyList;
        #endregion

        public MainStraddle()
        {
            InitializeComponent();
            KeyPreview = true;
            KeyPress += new KeyPressEventHandler(MainStraddle_KeyPress);
        }

        void MainStraddle_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._MainStraddle = null;
                Close();
            }
        }

        private void MainStraddle_Load(object sender, EventArgs e)
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

            threeExpiry = GetExpiryDates(ArisApi_a._arisApi.DsContract.Tables["NSEFO"]);
            DateTime dt1 = Convert.ToDateTime(threeExpiry[0].ToString());
            DateTime dt2 = Convert.ToDateTime(threeExpiry[1].ToString());
            DateTime dt3 = Convert.ToDateTime(threeExpiry[2].ToString());
            AppGlobal.enterCount = 0;

            #region contract Leg1
            string filter2 = "GatewayId = 1";
            DataTable GatewayId = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            GatewayId.DefaultView.RowFilter = filter2;

            cmbInstrument1.DataSource = GatewayId.DefaultView.ToTable(true, "InstrumentName");
            cmbInstrument1.DisplayMember = "InstrumentName";

            cmbInstrument2.DataSource = GatewayId.DefaultView.ToTable(true, "InstrumentName");
            cmbInstrument2.DisplayMember = "InstrumentName";

            cmbInstrument1.Text = "OPTIDX";
            cmbInstrument2.Text = "OPTIDX";

            string filter3 = "InstrumentName='" + cmbInstrument1.Text + "'";
            DataTable symbol = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            symbol.DefaultView.RowFilter = filter3;
            cmbSymbol1.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbSymbol1.DisplayMember = "Symbol";

            cmbSymbol2.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbSymbol2.DisplayMember = "Symbol";

            cmbSymbol1.Text = ArisApi_a._arisApi.SystemConfig.ApplicationName.ToString().ToUpper();
            cmbSymbol2.Text = ArisApi_a._arisApi.SystemConfig.ApplicationName.ToString().ToUpper();

            string filter = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + cmbSymbol1.Text.Trim() + "'";
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

            string filter1 = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s52 + "'";
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
            min.Text = Convert.ToString(minvalue);
            max.Text = Convert.ToString(maxvalue);

            cmbStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike1.DisplayMember = "StrikePrice";

            cmbStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike2.DisplayMember = "StrikePrice";
            string filter4 = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s52 + "' AND StrikePrice = '" + cmbStrike1.Text + "'";
            DataTable Series = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            Series.DefaultView.RowFilter = filter4;
            cmbSeries1.DataSource = Series.DefaultView.ToTable(true, "Series");
            cmbSeries1.DisplayMember = "Series";

            cmbSeries2.DataSource = Series.DefaultView.ToTable(true, "Series");
            cmbSeries2.DisplayMember = "Series";

            #endregion

            cmbSeries1.Text = "CE";
            cmbSeries2.Text = "PE";
            cmbSeries2.Enabled = false;
            cmbSymbol2.Enabled = false;   
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

        private void cmbInstrument1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string instrument = cmbInstrument1.Text;

            #region contract Leg1
            string filter2 = "GatewayId = 1";
            DataTable GatewayId = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            GatewayId.DefaultView.RowFilter = filter2;

            cmbInstrument1.DataSource = GatewayId.DefaultView.ToTable(true, "InstrumentName");
            cmbInstrument1.DisplayMember = "InstrumentName";



            cmbInstrument2.DataSource = GatewayId.DefaultView.ToTable(true, "InstrumentName");
            cmbInstrument2.DisplayMember = "InstrumentName";

            cmbInstrument2.Text = instrument;
            string filter3 = "InstrumentName='" + cmbInstrument1.Text + "'";
            DataTable symbol = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            symbol.DefaultView.RowFilter = filter3;
            cmbSymbol1.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbSymbol1.DisplayMember = "Symbol";


            cmbSymbol2.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbSymbol2.DisplayMember = "Symbol";

            string filter = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + lblSymbol.Text.Trim() + "'";
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

            string filter1 = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s52 + "'";
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

            min.Text = Convert.ToString(minvalue);
            max.Text = Convert.ToString(maxvalue);

            cmbStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike1.DisplayMember = "StrikePrice";

            cmbStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike2.DisplayMember = "StrikePrice";

            string filter4 = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s52 + "' AND StrikePrice = '" + cmbStrike1.Text + "'";
            DataTable Series = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            Series.DefaultView.RowFilter = filter4;
            cmbSeries1.DataSource = Series.DefaultView.ToTable(true, "Series");
            cmbSeries1.DisplayMember = "Series";

            cmbSeries2.DataSource = Series.DefaultView.ToTable(true, "Series");
            cmbSeries2.DisplayMember = "Series";

            cmbSeries1.Text = "CE";
            cmbSeries2.Text = "CE";
            #endregion
        }

        private void cmbSymbol1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string instrument = cmbInstrument1.Text;
            cmbInstrument2.Text = instrument;
            string _symbol = cmbSymbol1.Text;
            cmbSymbol2.Text = _symbol;

            #region contract Leg1
            string filter2 = "GatewayId = 1";
            DataTable GatewayId = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            GatewayId.DefaultView.RowFilter = filter2;

            string filter = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + _symbol + "'";
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

            string filter1 = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s52 + "'";
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

            min.Text = Convert.ToString(minvalue);
            max.Text = Convert.ToString(maxvalue);

            cmbStrike1.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike1.DisplayMember = "StrikePrice";



            cmbStrike2.DataSource = table.ToTable(true, "StrikePrice");
            cmbStrike2.DisplayMember = "StrikePrice";

            string filter4 = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s52 + "' AND StrikePrice = '" + cmbStrike1.Text + "'";
            DataTable Series = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            Series.DefaultView.RowFilter = filter4;
            cmbSeries1.DataSource = Series.DefaultView.ToTable(true, "Series");
            cmbSeries1.DisplayMember = "Series";

            cmbSeries2.DataSource = Series.DefaultView.ToTable(true, "Series");
            cmbSeries2.DisplayMember = "Series";

            cmbSeries1.Text = "CE";
            cmbSeries2.Text = "PE";
            #endregion

        }

        private void cmbExpiry1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string k = cmbExpiry1.Text;
            cmbExpiry2.Text = k;


            if (cmbInstrument1.Text == "OPTIDX" || cmbInstrument1.Text == "OPTSTK")
            {
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


                string filter1 = "InstrumentName='" + cmbInstrument1.Text + "' AND Symbol = '" + cmbSymbol1.Text + "' AND ExpiryDate = '" + s5 + "'";
                DataTable Strike = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                Strike.DefaultView.RowFilter = filter1;
                Strike.DefaultView.Sort = "StrikePrice";
                DataView table = Strike.DefaultView;
                cmbStrike1.DataSource = table.ToTable(true, "StrikePrice");
                cmbStrike1.DisplayMember = "StrikePrice";

                cmbStrike2.DataSource = table.ToTable(true, "StrikePrice");
                cmbStrike2.DisplayMember = "StrikePrice";


            }
            cmbSeries1.Text = "CE";
            cmbSeries2.Text = "PE";

        }

        private void cmbSeries1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string k = cmbStrike1.Text;
            cmbStrike2.Text = k;

        }

        private void MainStraddle_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._MainStraddle = null;
        }

        private void addRule1_Click(object sender, EventArgs e)
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
            string l1Series = Convert.ToString(cmbSeries1.Text);
            string l2Series = Convert.ToString(cmbSeries2.Text);


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


            int otmCallStrike = Leg1Strike + 500;
            int otmPutStrike = Leg1Strike - 500;



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
            strFilterCheck = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries1.Text + "'";
            DataRow[] drCheck = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck);
            foreach (DataRow dr in drCheck)
            {
                exp = Convert.ToUInt64(dr["SymbolDesc"]);
                TokenNo = Convert.ToInt32(dr["TokenNo"]);
            }

            UInt64 exp3 = Convert.ToUInt64(n52);
            int TokenNo3 = 0;
            string strFilterCheck1 = "";
            strFilterCheck1 = DBConst.InstrumentName + " = '" + cmbInstrument2.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
            DataRow[] drCheck1 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck1);
            foreach (DataRow dr in drCheck1)
            {
                exp3 = Convert.ToUInt64(dr["SymbolDesc"]);
                TokenNo3 = Convert.ToInt32(dr["TokenNo"]);
            }







            UInt64 Unique_id = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
            #endregion

            string strategy_type = TokenNo + "_" + TokenNo3;

            foreach (var watchT in AppGlobal.MarketWatch.Where(x => ((x.Leg1.ContractInfo.TokenNo + "_" + x.Leg1.ContractInfo.TokenNo)  == Convert.ToString(TokenNo))))
            {
                //if (watchT.Leg2.ContractInfo.TokenNo == Convert.ToString(TokenNo3))
                //{
                    if (watchT.StrategyId == 91 && watchT.Strategy == StrategyName)
                    {
                        MessageBox.Show("This Rule Already Added with GUI id : " + watchT.uniqueId + " Strategy : " + watchT.Strategy);
                        return;
                    }
                //}
            }

            if (AppGlobal.MarketWatch.Count() == 0)
            {
                return;
            }
            AppGlobal.StrategyRuleIndexNo = AppGlobal.StrategyRuleIndexNo + 1;

            #region Stragle Spread Call

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
                    watch.StrategyName = "MainStraddle_" + AppGlobal.StrategyRuleIndexNo + "_Stragle";
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
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    watch.Threshold = 2;
                    watch.Profit = 0;
                    watch.DrawDown = 0;
                    watch.RowData.Cells[WatchConst.Profit].Value = watch.Profit;
                    watch.RowData.Cells[WatchConst.DrawDown].Value = watch.DrawDown;
                    watch.RowData.Cells[WatchConst.UserDiff].Value = watch.Threshold;
                    watch.RowData.Cells[WatchConst.Expiry2].Value = watch.Expiry2;
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.IsStrikeReq = false;

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


                    strFilter1 = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(otmCallStrike) + "' AND " + DBConst.Series + "= '" + cmbSeries1.Text + "'";
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
                    /*string strFilter2 = "";

                    strFilter2 = DBConst.InstrumentName + " = '" + cmbInstrument2.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
                    DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr13)
                    {
                        watch.Leg2 = new Straddle.AppClasses.Leg();
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

                        watch.Leg2.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg2.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg2.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg2.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg2.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg2.ContractInfo.Series = Series1;
                        watch.Leg2.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg2.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg2.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg2.Counter = 1;
                        watch.Leg2.ContDetail.LotSize = Convert.ToInt32(Convert.ToDecimal(LotSize1));
                        watch.RowData.Cells[WatchConst.L2Strike].Value = watch.Leg2.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.L2Series].Value = watch.Leg2.ContractInfo.Series;


                        watch.Leg2.Ratio = 1;
                        watch.Leg2.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);
                        watch.RowData.Cells[WatchConst.Ratio2].Value = watch.Leg2.Ratio;
                        watch.Leg2.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg2.GatewayId);
                        watch.Leg2.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg2.ContDetail.LotSize = Convert.ToInt32(LotSize1);
                        watch.RowData.Cells[WatchConst.Token2].Value = watch.Leg2.ContractInfo.TokenNo;

                        #region Unique ID

                        watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                        watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));

                        watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                        #endregion

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo), list);
                        }
                    }*/
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
                    if (Convert.ToString(cmbInstrument1.Text) == "OPTIDX" || Convert.ToString(cmbInstrument1.Text) == "FUTIDX")
                    {
                        strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    }
                    else
                    {
                        strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    }

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
                watch.StrategyName = "MainStraddle_" + AppGlobal.StrategyRuleIndexNo + "_Strangle";
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
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                watch.Threshold = 2;
                watch.Profit = 0;
                watch.DrawDown = 0;
                watch.RowData.Cells[WatchConst.Profit].Value = watch.Profit;
                watch.RowData.Cells[WatchConst.DrawDown].Value = watch.DrawDown;
                watch.RowData.Cells[WatchConst.UserDiff].Value = watch.Threshold;
                watch.RowData.Cells[WatchConst.Expiry2].Value = watch.Expiry2;
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.IsStrikeReq = false;

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




                strFilter1 = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(otmCallStrike) + "' AND " + DBConst.Series + "= '" + cmbSeries1.Text + "'";
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

                if (Convert.ToString(cmbInstrument1.Text) == "OPTIDX" || Convert.ToString(cmbInstrument1.Text) == "FUTIDX")
                {
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                }
                else
                {
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                }


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
                    watch.StrategyName = "MainStraddle_" + AppGlobal.StrategyRuleIndexNo;
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
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    watch.Threshold = 2;
                    watch.Profit = 0;
                    watch.DrawDown = 0;
                    watch.RowData.Cells[WatchConst.Profit].Value = watch.Profit;
                    watch.RowData.Cells[WatchConst.DrawDown].Value = watch.DrawDown;
                    watch.RowData.Cells[WatchConst.UserDiff].Value = watch.Threshold;
                    watch.RowData.Cells[WatchConst.Expiry2].Value = watch.Expiry2;
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.IsStrikeReq = false;

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


                    strFilter1 = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries1.Text + "'";
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
                    /*string strFilter2 = "";

                    strFilter2 = DBConst.InstrumentName + " = '" + cmbInstrument2.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
                    DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr13)
                    {
                        watch.Leg2 = new Straddle.AppClasses.Leg();
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

                        watch.Leg2.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg2.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg2.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg2.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg2.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg2.ContractInfo.Series = Series1;
                        watch.Leg2.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg2.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg2.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg2.Counter = 1;
                        watch.Leg2.ContDetail.LotSize = Convert.ToInt32(Convert.ToDecimal(LotSize1));
                        watch.RowData.Cells[WatchConst.L2Strike].Value = watch.Leg2.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.L2Series].Value = watch.Leg2.ContractInfo.Series;


                        watch.Leg2.Ratio = 1;
                        watch.Leg2.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);
                        watch.RowData.Cells[WatchConst.Ratio2].Value = watch.Leg2.Ratio;
                        watch.Leg2.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg2.GatewayId);
                        watch.Leg2.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg2.ContDetail.LotSize = Convert.ToInt32(LotSize1);
                        watch.RowData.Cells[WatchConst.Token2].Value = watch.Leg2.ContractInfo.TokenNo;

                        #region Unique ID

                        watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                        watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));

                        watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                        #endregion

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo), list);
                        }
                    }*/
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
                    if (Convert.ToString(cmbInstrument1.Text) == "OPTIDX" || Convert.ToString(cmbInstrument1.Text) == "FUTIDX")
                    {
                        strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    }
                    else
                    {
                        strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    }

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
                watch.StrategyName = "MainStraddle_" + AppGlobal.StrategyRuleIndexNo;
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
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                watch.Threshold = 2;
                watch.Profit = 0;
                watch.DrawDown = 0;
                watch.RowData.Cells[WatchConst.Profit].Value = watch.Profit;
                watch.RowData.Cells[WatchConst.DrawDown].Value = watch.DrawDown;
                watch.RowData.Cells[WatchConst.UserDiff].Value = watch.Threshold;
                watch.RowData.Cells[WatchConst.Expiry2].Value = watch.Expiry2;
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.IsStrikeReq = false;

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




                strFilter1 = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike1.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries1.Text + "'";
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

               /* strFilter2 = DBConst.InstrumentName + " = '" + cmbInstrument2.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
                DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                foreach (DataRow dr in dr13)
                {
                    watch.Leg2 = new Straddle.AppClasses.Leg();
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

                    watch.Leg2.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                    watch.Leg2.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                    watch.Leg2.ContractInfo.Exchange = dr["Exchange"].ToString();
                    watch.Leg2.ContractInfo.Symbol = dr["Symbol"].ToString();
                    watch.Leg2.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                    watch.Leg2.ContractInfo.Series = Series1;
                    watch.Leg2.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                    watch.Leg2.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                    watch.Leg2.ContractInfo.InstrumentName = InstrumentName1;
                    watch.Leg2.Counter = 1;
                    watch.Leg2.ContDetail.LotSize = Convert.ToInt32(Convert.ToDecimal(LotSize1));
                    watch.RowData.Cells[WatchConst.L2Strike].Value = watch.Leg2.ContractInfo.StrikePrice;
                    watch.RowData.Cells[WatchConst.L2Series].Value = watch.Leg2.ContractInfo.Series;


                    watch.Leg2.Ratio = 1;
                    watch.Leg2.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);
                    watch.RowData.Cells[WatchConst.Ratio2].Value = watch.Leg2.Ratio;
                    watch.Leg2.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg2.GatewayId);
                    watch.Leg2.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                    watch.Leg2.ContDetail.LotSize = Convert.ToInt32(LotSize1);
                    watch.RowData.Cells[WatchConst.Token2].Value = watch.Leg2.ContractInfo.TokenNo;

                    #region Unique ID

                    watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                    watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));

                    watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                    #endregion

                    if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)))
                    {
                        List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)];
                        list.Add(selectindex);

                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(selectindex);
                        AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo), list);
                    }
                }*/
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

                if (Convert.ToString(cmbInstrument1.Text) == "OPTIDX" || Convert.ToString(cmbInstrument1.Text) == "FUTIDX")
                {
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                }
                else
                {
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                }


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
                   
                    watch.StrategyName = "MainStraddle_" + AppGlobal.StrategyRuleIndexNo;
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
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    watch.Threshold = 2;
                    watch.Profit = 0;
                    watch.DrawDown = 0;
                    watch.RowData.Cells[WatchConst.Profit].Value = watch.Profit;
                    watch.RowData.Cells[WatchConst.DrawDown].Value = watch.DrawDown;
                    watch.RowData.Cells[WatchConst.UserDiff].Value = watch.Threshold;
                    watch.RowData.Cells[WatchConst.Expiry2].Value = watch.Expiry2;
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.IsStrikeReq = false;

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


                    strFilter1 = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
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
                    /*string strFilter2 = "";

                    strFilter2 = DBConst.InstrumentName + " = '" + cmbInstrument2.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
                    DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr13)
                    {
                        watch.Leg2 = new Straddle.AppClasses.Leg();
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

                        watch.Leg2.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg2.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg2.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg2.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg2.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg2.ContractInfo.Series = Series1;
                        watch.Leg2.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg2.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg2.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg2.Counter = 1;
                        watch.Leg2.ContDetail.LotSize = Convert.ToInt32(Convert.ToDecimal(LotSize1));
                        watch.RowData.Cells[WatchConst.L2Strike].Value = watch.Leg2.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.L2Series].Value = watch.Leg2.ContractInfo.Series;


                        watch.Leg2.Ratio = 1;
                        watch.Leg2.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);
                        watch.RowData.Cells[WatchConst.Ratio2].Value = watch.Leg2.Ratio;
                        watch.Leg2.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg2.GatewayId);
                        watch.Leg2.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg2.ContDetail.LotSize = Convert.ToInt32(LotSize1);
                        watch.RowData.Cells[WatchConst.Token2].Value = watch.Leg2.ContractInfo.TokenNo;

                        #region Unique ID

                        watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                        watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));

                        watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                        #endregion

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo), list);
                        }
                    }*/
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
                    if (Convert.ToString(cmbInstrument1.Text) == "OPTIDX" || Convert.ToString(cmbInstrument1.Text) == "FUTIDX")
                    {
                        strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    }
                    else
                    {
                        strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    }

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
               
                watch.StrategyName = "MainStraddle_" + AppGlobal.StrategyRuleIndexNo;
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
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                watch.Threshold = 2;
                watch.Profit = 0;
                watch.DrawDown = 0;
                watch.RowData.Cells[WatchConst.Profit].Value = watch.Profit;
                watch.RowData.Cells[WatchConst.DrawDown].Value = watch.DrawDown;
                watch.RowData.Cells[WatchConst.UserDiff].Value = watch.Threshold;
                watch.RowData.Cells[WatchConst.Expiry2].Value = watch.Expiry2;
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.IsStrikeReq = false;

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




                strFilter1 = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
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


                /* strFilter2 = DBConst.InstrumentName + " = '" + cmbInstrument2.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
                 DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                 foreach (DataRow dr in dr13)
                 {
                     watch.Leg2 = new Straddle.AppClasses.Leg();
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

                     watch.Leg2.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                     watch.Leg2.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                     watch.Leg2.ContractInfo.Exchange = dr["Exchange"].ToString();
                     watch.Leg2.ContractInfo.Symbol = dr["Symbol"].ToString();
                     watch.Leg2.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                     watch.Leg2.ContractInfo.Series = Series1;
                     watch.Leg2.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                     watch.Leg2.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                     watch.Leg2.ContractInfo.InstrumentName = InstrumentName1;
                     watch.Leg2.Counter = 1;
                     watch.Leg2.ContDetail.LotSize = Convert.ToInt32(Convert.ToDecimal(LotSize1));
                     watch.RowData.Cells[WatchConst.L2Strike].Value = watch.Leg2.ContractInfo.StrikePrice;
                     watch.RowData.Cells[WatchConst.L2Series].Value = watch.Leg2.ContractInfo.Series;


                     watch.Leg2.Ratio = 1;
                     watch.Leg2.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);
                     watch.RowData.Cells[WatchConst.Ratio2].Value = watch.Leg2.Ratio;
                     watch.Leg2.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg2.GatewayId);
                     watch.Leg2.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                     watch.Leg2.ContDetail.LotSize = Convert.ToInt32(LotSize1);
                     watch.RowData.Cells[WatchConst.Token2].Value = watch.Leg2.ContractInfo.TokenNo;

                     #region Unique ID

                     watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                     watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));

                     watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                     #endregion

                     if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)))
                     {
                         List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)];
                         list.Add(selectindex);

                     }
                     else
                     {
                         List<int> list = new List<int>();
                         list.Add(selectindex);
                         AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo), list);
                     }
                 }*/
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

                if (Convert.ToString(cmbInstrument1.Text) == "OPTIDX" || Convert.ToString(cmbInstrument1.Text) == "FUTIDX")
                {
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                }
                else
                {
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                }


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


            #region Stragle Spread Put

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
                    watch.StrategyName = "MainStraddle_" + AppGlobal.StrategyRuleIndexNo + "_Stragle";
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
                    watch.Expiry = ExpDisplay;
                    watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                    watch.Expiry2 = ExpDisplay2;
                    watch.Threshold = 2;
                    watch.Profit = 0;
                    watch.DrawDown = 0;
                    watch.RowData.Cells[WatchConst.Profit].Value = watch.Profit;
                    watch.RowData.Cells[WatchConst.DrawDown].Value = watch.DrawDown;
                    watch.RowData.Cells[WatchConst.UserDiff].Value = watch.Threshold;
                    watch.RowData.Cells[WatchConst.Expiry2].Value = watch.Expiry2;
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    watch.IsStrikeReq = false;

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


                    strFilter1 = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(otmPutStrike) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
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
                    /*string strFilter2 = "";

                    strFilter2 = DBConst.InstrumentName + " = '" + cmbInstrument2.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol2.Text + "' AND " + DBConst.ExpiryDate + " = '" + n52 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(cmbStrike2.Text) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
                    DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                    foreach (DataRow dr in dr13)
                    {
                        watch.Leg2 = new Straddle.AppClasses.Leg();
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

                        watch.Leg2.GatewayId = uint.Parse(dr["GatewayId"].ToString());
                        watch.Leg2.ContractInfo.TokenNo = dr["TokenNo"].ToString();
                        watch.Leg2.ContractInfo.Exchange = dr["Exchange"].ToString();
                        watch.Leg2.ContractInfo.Symbol = dr["Symbol"].ToString();
                        watch.Leg2.ContractInfo.PriceDivisor = Convert.ToInt32(dr["PriceDivisor"]);
                        watch.Leg2.ContractInfo.Series = Series1;
                        watch.Leg2.ContractInfo.StrikePrice = Convert.ToInt32(Convert.ToDecimal(StrikePrice1));
                        watch.Leg2.ContractInfo.Multiplier = Convert.ToDecimal(Multiplier1);
                        watch.Leg2.ContractInfo.InstrumentName = InstrumentName1;
                        watch.Leg2.Counter = 1;
                        watch.Leg2.ContDetail.LotSize = Convert.ToInt32(Convert.ToDecimal(LotSize1));
                        watch.RowData.Cells[WatchConst.L2Strike].Value = watch.Leg2.ContractInfo.StrikePrice;
                        watch.RowData.Cells[WatchConst.L2Series].Value = watch.Leg2.ContractInfo.Series;


                        watch.Leg2.Ratio = 1;
                        watch.Leg2.expiryUniqueID = Convert.ToUInt64(SymbolDesc1);
                        watch.RowData.Cells[WatchConst.Ratio2].Value = watch.Leg2.Ratio;
                        watch.Leg2.ContDetail.PriceFormat = MTMethods.GetPriceFormat(watch.Leg2.GatewayId);
                        watch.Leg2.ContDetail.PriceTick = Convert.ToDecimal(PriceTick1);
                        watch.Leg2.ContDetail.LotSize = Convert.ToInt32(LotSize1);
                        watch.RowData.Cells[WatchConst.Token2].Value = watch.Leg2.ContractInfo.TokenNo;

                        #region Unique ID

                        watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                        watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));

                        watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                        #endregion

                        if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)))
                        {
                            List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)];
                            list.Add(i);

                        }
                        else
                        {
                            List<int> list = new List<int>();
                            list.Add(i);
                            AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo), list);
                        }
                    }*/
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
                    if (Convert.ToString(cmbInstrument1.Text) == "OPTIDX" || Convert.ToString(cmbInstrument1.Text) == "FUTIDX")
                    {
                        strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    }
                    else
                    {
                        strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                    }

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
                watch.StrategyName = "MainStraddle_" + AppGlobal.StrategyRuleIndexNo + "_Strangle";
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
                watch.Expiry = ExpDisplay;
                watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                watch.Expiry2 = ExpDisplay2;
                watch.Threshold = 2;
                watch.Profit = 0;
                watch.DrawDown = 0;
                watch.RowData.Cells[WatchConst.Profit].Value = watch.Profit;
                watch.RowData.Cells[WatchConst.DrawDown].Value = watch.DrawDown;
                watch.RowData.Cells[WatchConst.UserDiff].Value = watch.Threshold;
                watch.RowData.Cells[WatchConst.Expiry2].Value = watch.Expiry2;
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.Strategy_Type = strategy_type;
                watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                watch.IsStrikeReq = false;

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




                strFilter1 = DBConst.InstrumentName + " = '" + cmbInstrument1.Text + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + n5 + "' AND " + DBConst.StrikePrice + " = '" + Convert.ToString(otmPutStrike) + "' AND " + DBConst.Series + "= '" + cmbSeries2.Text + "'";
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

                if (Convert.ToString(cmbInstrument1.Text) == "OPTIDX" || Convert.ToString(cmbInstrument1.Text) == "FUTIDX")
                {
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                }
                else
                {
                    strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + cmbSymbol1.Text + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                }


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

        private void cmbStrike1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string k = cmbStrike1.Text;
            cmbStrike2.Text = k;
        }

    }
}
