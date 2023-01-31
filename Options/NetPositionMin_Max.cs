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
using LogWriter;
using System.Net.Sockets;
using ArisDev;


namespace Straddle
{
    public partial class NetPositionMin_Max : Form
    {
        public NetPositionMin_Max()
        {
            InitializeComponent();
            AppGlobal.connection.RMSMessageRecived += new AppGlobal.RMSTerminal_MessageRecivedDel(connection_RMSMessageRecived);
        }

        private void NetPositionMin_Max_Load(object sender, EventArgs e)
        {
            GenerateTrdColumns();
            LoadEvent();
        }

        public void LoadEvent()
        {
            mtDataGridView1.Rows.Clear();
           // AppGlobal.NetMarketWatch = NetPositionWatch.ReadXmlProfile();
           // AssignMarketStructValue1(AppGlobal.NetMarketWatch);
        }

        void connection_RMSMessageRecived(Socket socket, byte[] message)
        {
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)(() => connection_RMSMessageRecived(socket, message)));
            else
            {
                try
                {
                }
                catch(Exception)
                {

                }
            }
        }

        private void GenerateTrdColumns()
        {
            try
            {
                GenerateTrdColumn(TradeConst.uniqueId, MTEnums.FieldType.None, true);
                GenerateTrdColumn(TradeConst.AvgPrice, MTEnums.FieldType.Price, true);
                GenerateTrdColumn(TradeConst.posInt, MTEnums.FieldType.Quantity, true);
                GenerateTrdColumn(TradeConst.posType, MTEnums.FieldType.None, true);
                GenerateTrdColumn(TradeConst.StrategyName, MTEnums.FieldType.None, true);
                GenerateTrdColumn(TradeConst.windAvg, MTEnums.FieldType.Price, true);
                GenerateTrdColumn(TradeConst.unwindAvg, MTEnums.FieldType.Price, true);
                GenerateTrdColumn(TradeConst.L1Ser, MTEnums.FieldType.Quantity, true);
                GenerateTrdColumn(TradeConst.L1Stk, MTEnums.FieldType.Quantity, true);
                GenerateTrdColumn(TradeConst.L2Stk, MTEnums.FieldType.Quantity, true);
                GenerateTrdColumn(TradeConst.L3Stk, MTEnums.FieldType.Quantity, true);
                GenerateTrdColumn(TradeConst.L4Stk, MTEnums.FieldType.Quantity, true);
                GenerateTrdColumn(TradeConst.Expiry, MTEnums.FieldType.None, true);
                //GenerateTrdColumn(TradeConst.PNL, MTEnums.FieldType.Price, true);
            }
            catch (Exception)
            {
                
               
            }
        }

        private void GenerateTrdColumn(string clName, MTEnums.FieldType fieldType, bool Editable)
        {
            mtDataGridView1.Columns.Add(clName, clName);
            mtDataGridView1.Columns[clName].ReadOnly = Editable;

            switch (fieldType)
            {
                case MTEnums.FieldType.None:
                    break;

                case MTEnums.FieldType.Date:
                    mtDataGridView1.Columns[clName].DefaultCellStyle.Format = MTConstant.DateFormatGrid;
                    break;
                case MTEnums.FieldType.Time:
                    mtDataGridView1.Columns[clName].DefaultCellStyle.Format = MTConstant.TimeFormatGrid;
                    break;
                case MTEnums.FieldType.Price:
                    mtDataGridView1.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Quantity:
                    mtDataGridView1.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Percentage:
                    mtDataGridView1.Columns[clName].DefaultCellStyle.Format = "0.00%";
                    mtDataGridView1.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Indicator:
                    mtDataGridView1.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    break;
                case MTEnums.FieldType.DateTime:
                    break;
            }
        }

        private void NetPositionMin_Max_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._NetMax_Min = null;
        }

        public void AssignMarketStructValue1(List<NetPositionWatch> NetMarketWatch)
        {
            //try
            //{
            //    if (AppGlobal.NetMarketWatch == null) return;
            //    // int temp = 0;
            //    mtDataGridView1.Rows.Add();
            //    for (int index = 0; index < AppGlobal.NetMarketWatch.Count; index++)
            //    {
            //        NetPositionWatch watch = AppGlobal.NetMarketWatch[index];
            //        watch.RowData = mtDataGridView1.Rows[index];
            //        watch.RowData.Cells[TradeConst.uniqueId].Value = watch.Leg.displayUniqueId;


            //        string strFilterCheck = "";
            //        string _Strike1 = "";
            //        string _Series = "";
            //        strFilterCheck = DBConst.TokenNo + " = '" + watch.Token1 + "'";
            //        DataRow[] drCheck = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck);
            //        foreach (DataRow dr in drCheck)
            //        {
            //            _Strike1 = Convert.ToString(dr["StrikePrice"]);
            //            _Series = Convert.ToString(dr["Series"]);
            //        }
            //        string strFilterCheck2 = "";
            //        string _Strike2 = "";
            //        strFilterCheck2 = DBConst.TokenNo + " = '" + watch.Token2 + "'";
            //        DataRow[] drCheck2 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck2);
            //        foreach (DataRow dr in drCheck2)
            //        {
            //            _Strike2 = Convert.ToString(dr["StrikePrice"]);

            //        }
            //        string strFilterCheck3 = "";
            //        string _Strike3 = "";
            //        strFilterCheck3 = DBConst.TokenNo + " = '" + watch.Token3 + "'";
            //        DataRow[] drCheck3 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck3);
            //        foreach (DataRow dr in drCheck3)
            //        {
            //            _Strike3 = Convert.ToString(dr["StrikePrice"]);

            //        }
            //        string strFilterCheck4 = "";
            //        string _Strike4 = "";
            //        strFilterCheck4 = DBConst.TokenNo + " = '" + watch.Token4 + "'";
            //        DataRow[] drCheck4 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilterCheck4);
            //        foreach (DataRow dr in drCheck4)
            //        {
            //            _Strike4 = Convert.ToString(dr["StrikePrice"]);

            //        }

            //        watch.RowData.Cells[TradeConst.L1Ser].Value = _Series;

            //        watch.RowData.Cells[TradeConst.L1Stk].Value = _Strike1;
            //        watch.RowData.Cells[TradeConst.L2Stk].Value = _Strike2;
            //        watch.RowData.Cells[TradeConst.L3Stk].Value = _Strike3;
            //        watch.RowData.Cells[TradeConst.L4Stk].Value = _Strike4;
            //        if (watch.Leg.B_Qty != 0)
            //        {
            //            watch.windAvg = Convert.ToDouble(watch.Leg.B_Value / Math.Abs(watch.Leg.B_Qty));
            //        }
            //        if (watch.Leg.S_Qty != 0)
            //        {
            //            watch.unwindAvg = Convert.ToDouble(watch.Leg.S_Value / Math.Abs(watch.Leg.S_Qty));
            //        }
            //        watch.RowData.Cells[TradeConst.windAvg].Value = watch.windAvg;
            //        watch.RowData.Cells[TradeConst.unwindAvg].Value = watch.unwindAvg;

            //        watch.RowData.Cells[TradeConst.AvgPrice].Value = watch.Leg.N_Price;
            //        watch.RowData.Cells[TradeConst.posInt].Value = watch.posInt;
            //        watch.RowData.Cells[TradeConst.posType].Value = watch.posType;
            //        watch.RowData.Cells[TradeConst.Expiry].Value = watch.Expiry;
            //        watch.RowData.Cells[TradeConst.StrategyName].Value = watch.StrategyName;
            //        mtDataGridView1.Rows.Add();
            //    }
            //    //if (AppGlobal.NetMarketWatch.Count == 0)
            //    //    mtDataGridView1.Rows.Add();
            //}
            //catch (Exception ex)
            //{
            //    Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "AssignMarketStructValue")
            //                 , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            //}
        }

    }
}
