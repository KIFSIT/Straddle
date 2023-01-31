using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArisDev;
using MTCommon;
using MTControls;
using Straddle.AppClasses;

namespace Straddle
{
    public partial class ContractPanel : UserControl
    {

        public MTEnums.GatewayId AllowedGateway { get; set; }
        public ContractArgs ContractData { get; set; }
        public DataSet DsContractCollection { get; set; }
        public ContractDetails ContractDetail { get; set; }
        public ContractPanel()
        {
            InitializeComponent();
            ContractData = new ContractArgs();
            ContractDetail = new ContractDetails();
        }

        private void ContractPanel_Load(object sender, EventArgs e)
        {
            instrument = new Control();
            instrument.Location = cmbInstrumentName.Location;
            symbol = new Control();
            symbol.Location = cmbSymbol.Location;
            expirydate = new Control();
            expirydate.Location = cmbExpiryDate.Location;
            strike = new Control();
            strike.Location = cmbStrikePrice.Location;
            series = new Control();
            series.Location = cmbSeries.Location;
            cmbGateway.Items.Add("NseCm");
            cmbGateway.Items.Add("NseFo");
            cmbGateway.SelectedIndex = 0;
        }

        void resetData()
        {
            cmbInstrumentName.Visible = true;
            cmbExpiryDate.Visible = true;
            cmbStrikePrice.Visible = true;
            cmbSeries.Visible = true;

            cmbInstrumentName.Location = new Point(instrument.Location.X, instrument.Location.Y);
            cmbSymbol.Location = new Point(symbol.Location.X, symbol.Location.Y);
            cmbExpiryDate.Location = new Point(expirydate.Location.X, expirydate.Location.Y);
            cmbStrikePrice.Location = new Point(strike.Location.X, strike.Location.Y);
            cmbSeries.Location = new Point(series.Location.X, series.Location.Y);

            cmbInstrumentName.DataSource = null;
            cmbSymbol.DataSource = null;
            cmbStrikePrice.DataSource = null;
            cmbExpiryDate.DataSource = null;
            cmbSeries.DataSource = null;
        }

        Control instrument, symbol, expirydate, strike, series;

        public void cmbGateway_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            resetData();
            if (cmbGateway.Text == "NseCm")
            {
                cmbInstrumentName.Visible = false;
                cmbExpiryDate.Visible = false;
                cmbStrikePrice.Visible = false;

                cmbSymbol.Location = new Point(instrument.Location.X, instrument.Location.Y);
                cmbSeries.Location = new Point(instrument.Location.X + 110, symbol.Location.Y);

                cmbSymbol.DataSource = ArisApi_a._arisApi.DsContract.Tables["NseCm"];
                cmbSymbol.DisplayMember = "Symbol";
                cmbSymbol.ValueMember = "TokenNo";

                cmbSeries.Text = "EQ";
            }
            else if (cmbGateway.Text == "NseFo")
            {
                string filter = "GatewayId = 1";
                DataTable GatewayId = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                GatewayId.DefaultView.RowFilter = filter;

                cmbInstrumentName.DataSource = GatewayId.DefaultView.ToTable(true, "InstrumentName");
                cmbInstrumentName.DisplayMember = "InstrumentName";
                cmbInstrumentName.SelectedIndex = 0;
            }
        }

        public void cmbInstrumentName_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string filter = "InstrumentName='" + cmbInstrumentName.Text + "'";
            DataTable symbol = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            symbol.DefaultView.RowFilter = filter;
            cmbSymbol.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbSymbol.DisplayMember = "Symbol";
           // cmbSymbol.SelectedIndex = 0;
        }

        public void cmbSymbol_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cmbGateway.Text == "NseFo")
            {
                string filter = "InstrumentName='" + cmbInstrumentName.Text + "' AND Symbol = '" + cmbSymbol.Text + "'";
                DataTable expiry = ArisApi_a._arisApi.DsContract.Tables["NseFo"];

                expiry.DefaultView.Sort = "ExpiryDate ASC";
                expiry.DefaultView.RowFilter = filter;

                cmbExpiryDate.DataSource = expiry.DefaultView.ToTable(true, "ExpiryDate");
                cmbExpiryDate.DisplayMember = "ExpiryDate";
                //cmbExpiryDate.SelectedIndex = 0;
            }
        }

        public void cmbExpiryDate_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cmbGateway.Text == "NseFo")
            {
                string filter = "InstrumentName='" + cmbInstrumentName.Text + "' AND Symbol = '" + cmbSymbol.Text + "' AND ExpiryDate = '" + cmbExpiryDate.Text + "'";
                DataTable Strike = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                Strike.DefaultView.RowFilter = filter;
                Strike.DefaultView.Sort = "StrikePrice";
                DataView table = Strike.DefaultView;
                cmbStrikePrice.DataSource = table.ToTable(true, "StrikePrice");
                cmbStrikePrice.DisplayMember = "StrikePrice";
                //cmbStrikePrice.SelectedIndex = 0;
            }
        }

        public void cmbStrikePrice_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cmbGateway.Text == "NseFo")
            {
                string filter = "InstrumentName='" + cmbInstrumentName.Text + "' AND Symbol = '" + cmbSymbol.Text + "' AND ExpiryDate = '" + cmbExpiryDate.Text + "' AND StrikePrice = '" + cmbStrikePrice.Text + "'";
                DataTable Series = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                Series.DefaultView.RowFilter = filter;
                cmbSeries.DataSource = Series.DefaultView.ToTable(true, "Series");
                cmbSeries.DisplayMember = "Series";
               // cmbSeries.SelectedIndex = 0;
            }
        }

        public void cmbSeries_SelectionChangeCommitted(object sender, EventArgs e)
        {

            try
            {
                string strFilter = "InstrumentName = '" + cmbInstrumentName.Text + "' AND  Symbol = '" + cmbSymbol.Text + "' AND StrikePrice = '" + cmbStrikePrice.Text + "' AND Series= '" + cmbSeries.Text + "' AND ExpiryDate = '" + cmbExpiryDate.Text + "'";
                DataRow[] dr1 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(strFilter);
                if (dr1.Length > 0)
                {
                    ContractData.ContractInfo.TokenNo = Convert.ToString(dr1[0]["TokenNo"]);
                    ContractData.GatewayId = uint.Parse(Convert.ToString(dr1[0]["GatewayId"]));
                    ContractData.ContractInfo.Symbol = cmbSymbol.Text;
                    ContractData.ContractInfo.Exchange = Convert.ToString(dr1[0]["Exchange"]);
                    ContractData.ContractInfo.InstrumentName = cmbInstrumentName.Text;
                    ContractData.ContractInfo.Series = cmbSeries.Text;
                    ContractData.ContractInfo.StrikePrice = int.Parse(cmbStrikePrice.Text);
                    ContractData.ContractInfo.PriceDivisor = int.Parse(Convert.ToString(dr1[0]["PriceDivisor"]));
                    ContractData.ContractInfo.Multiplier = int.Parse(Convert.ToString(dr1[0]["Multiplier"]));
                    ContractData.ContractInfo.ExpiryDate = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(dr1[0]["ExpiryDate"]));
                    ContractDetail.LotSize = Convert.ToInt32(dr1[0]["LotSize"]);
                    ContractDetail.PriceTick = Convert.ToDecimal(dr1[0]["PriceTick"]);

                    //ArisApi_a._arisApi.SubscribeMarketFeeds(ContractData.ContractInfo.TokenNo
                    //                                 , (uint)ContractData.GatewayId
                    //                                 , ContractData.ContractInfo.Exchange
                    //                                 , ContractData.ContractInfo.PriceDivisor
                    //                                 , ContractData.ContractInfo.Symbol);
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void cmbInstrumentName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filter = "InstrumentName='" + cmbInstrumentName.Text + "'";
            DataTable symbol = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
            symbol.DefaultView.RowFilter = filter;
            cmbSymbol.DataSource = symbol.DefaultView.ToTable(true, "Symbol");
            cmbSymbol.DisplayMember = "Symbol";
            //cmbSymbol.SelectedIndex = 0;
        }

        public void cmbExpiryDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGateway.Text == "NseFo")
            {
                string filter = "InstrumentName='" + cmbInstrumentName.Text + "' AND Symbol = '" + cmbSymbol.Text + "' AND ExpiryDate = '" + cmbExpiryDate.Text + "'";
                DataTable Strike = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                Strike.DefaultView.RowFilter = filter;

                cmbStrikePrice.DataSource = Strike.DefaultView.ToTable(true, "StrikePrice");
                cmbStrikePrice.DisplayMember = "StrikePrice";
                //cmbStrikePrice.SelectedIndex = 0;
            }
        }

        public void cmbSymbol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGateway.Text == "NseFo")
            {
                string filter = "InstrumentName='" + cmbInstrumentName.Text + "' AND Symbol = '" + cmbSymbol.Text + "'";
                DataTable expiry = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                expiry.DefaultView.RowFilter = filter;
                cmbExpiryDate.DataSource = expiry.DefaultView.ToTable(true, "ExpiryDate");
                cmbExpiryDate.DisplayMember = "ExpiryDate";
                //cmbExpiryDate.SelectedIndex = 0;
            }
        }

        private void cmbStrikePrice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGateway.Text == "NseFo")
            {
                string filter = "InstrumentName='" + cmbInstrumentName.Text + "' AND Symbol = '" + cmbSymbol.Text + "' AND ExpiryDate = '" + cmbExpiryDate.Text + "' AND StrikePrice = '" + cmbStrikePrice.Text + "'";
                DataTable Series = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                Series.DefaultView.RowFilter = filter;
                cmbSeries.DataSource = Series.DefaultView.ToTable(true, "Series");
                cmbSeries.DisplayMember = "Series";
            }
        }





       

       
    }
}
