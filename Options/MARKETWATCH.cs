using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArisDev;
using Straddle.AppClasses;
using MTCommon;
using LogWriter;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Straddle
{
    
    public partial class MARKETWATCH : Form
    {

        //internal DataTable _marketBookTable;
        internal DataTable _StrategyMarketTable;
        internal DataTable _SummaryStrategyTable;
        Dictionary<string, CountWindUnWind> DictData;
        public MARKETWATCH()
        {
            InitializeComponent();
          

            AppGlobal.connection.MKTMessageRecived += new AppGlobal.MKTTerminal_MessageRecivedDel(connection_MKTMessageRecived);

            _StrategyMarketTable = new DataTable();
            _StrategyMarketTable.TableName = "MarketBook1";
            _SummaryStrategyTable = new DataTable();
            _SummaryStrategyTable.TableName = "_SummaryStrategyTable";
            CreateTableStrategy();
            CreateTableSummaryStrategy();
            DictData = new Dictionary<string, CountWindUnWind>();
        }

        void CreateTableStrategy()
        {
            _StrategyMarketTable.Columns.Add(TradeConst.Time);
            _StrategyMarketTable.Columns.Add(TradeConst.Expiry);
            _StrategyMarketTable.Columns.Add(TradeConst.IsWind);
            _StrategyMarketTable.Columns.Add(TradeConst.L1Stk);
            _StrategyMarketTable.Columns.Add(TradeConst.L2Stk);
            _StrategyMarketTable.Columns.Add(TradeConst.Sprd);
            _StrategyMarketTable.Columns.Add(TradeConst.WindCount);
            _StrategyMarketTable.Columns.Add(TradeConst.UnwindCount);

            _StrategyMarketTable.Columns.Add(TradeConst.AvgWind, typeof(double));
            _StrategyMarketTable.Columns.Add(TradeConst.AvgUnwind, typeof(double));
            _StrategyMarketTable.Columns.Add(TradeConst.SUMWIND, typeof(double));
            _StrategyMarketTable.Columns.Add(TradeConst.SUMUNWIND, typeof(double));
        }

        void CreateTableSummaryStrategy()
        {
            _SummaryStrategyTable.Columns.Add(TradeConst.Time);
            _SummaryStrategyTable.Columns.Add(TradeConst.Expiry);
            _SummaryStrategyTable.Columns.Add(TradeConst.IsWind);
            _SummaryStrategyTable.Columns.Add(TradeConst.L1Stk);
            _SummaryStrategyTable.Columns.Add(TradeConst.L2Stk);
            _SummaryStrategyTable.Columns.Add(TradeConst.Sprd);
            _SummaryStrategyTable.Columns.Add(TradeConst.WindCount, typeof(int));
            _SummaryStrategyTable.Columns.Add(TradeConst.UnwindCount, typeof(int));
        }



        void connection_MKTMessageRecived(Socket socket, byte[] message)
        {
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)(() => connection_MKTMessageRecived(socket, message)));
            else
            {
                try
                {
                    //UInt64 TransCode = BitConverter.ToUInt64(message, 0);
                    //if (TransCode == 3434)
                    //{
                    //    BTPacket.BoxTradeMsg packetHeader = PinnedPacket<BTPacket.BoxTradeMsg>(message);
                    //    AllInsertTrade(packetHeader);
                    //    SummaryInsertTrade(packetHeader);
                    //}
                }
                catch (Exception)
                {

                }
            }
        }

        public T PinnedPacket<T>(byte[] data)
        {
            object packet = new object();
            try
            {
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr IntPtrOfObject = handle.AddrOfPinnedObject();
                packet = Marshal.PtrToStructure(IntPtrOfObject, typeof(T));
                handle.Free();
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
            return (T)packet;
        }

        public void AllInsertTrade(BTPacket.BoxTradeMsg Trade)
        {
            if (AppGlobal.frmWatch != null && AppGlobal.frmWatch.InvokeRequired)
            {
                AppGlobal.frmWatch.BeginInvoke((MethodInvoker)(() => AllInsertTrade(Trade)));
            }
            else
            {
                if (Trade.TransCode == 3434)
                {
                    DataRow row = _StrategyMarketTable.NewRow();
                    row[TradeConst.Time] = DateTime.Now.ToString("HH:mm:ss:ffff");
                    if (Trade.IsWind == 1)
                        row[TradeConst.IsWind] = "Wind";
                    else
                        row[TradeConst.IsWind] = "UnWind";
                    row[TradeConst.L1Stk] = (Convert.ToDouble(Trade.Strike1) / 100).ToString();
                    row[TradeConst.L2Stk] = (Convert.ToDouble(Trade.Strike2) / 100).ToString();
                   
                    string DATE = SecondToDateTime(Market.NseCm, Convert.ToInt32(Trade.Expiry)).ToString("ddMMMyyyy");
                    row[TradeConst.Expiry] = DATE;
                    row[TradeConst.Sprd] = Math.Round(Trade.Spread / 100,2);
                    
                    //string key = Convert.ToString((Convert.ToDouble(Trade.Strike1) / 100)) + Convert.ToString((Convert.ToDouble(Trade.Strike1) / 100)) + DATE;
                    //if (!DictData.ContainsKey(key))
                    //{
                    //    DictData.Add(key, new CountWindUnWind());
                    //    if (Trade.IsWind == 1)
                    //    {
                    //        DictData[key].windCount = 1;
                    //        DictData[key].UnwindCount = 0;
                    //        row[TradeConst.WindCount] = DictData[key].windCount;
                    //        row[TradeConst.UnwindCount] = DictData[key].UnwindCount;
                    //    }
                    //    else
                    //    {
                    //        DictData[key].UnwindCount = 1;
                    //        DictData[key].windCount = 0;
                    //        row[TradeConst.WindCount] = DictData[key].windCount;
                    //        row[TradeConst.UnwindCount] = DictData[key].UnwindCount;
                    //    }
                    //}
                    //else
                    //{
                    //    if (Trade.IsWind == 1)
                    //    {
                    //        DictData[key].windCount++;
                    //        row[TradeConst.WindCount] = DictData[key].windCount;
                    //        row[TradeConst.UnwindCount] = DictData[key].UnwindCount;
                    //    }
                    //    else
                    //    {
                    //        DictData[key].UnwindCount++;
                    //        row[TradeConst.WindCount] = DictData[key].windCount;
                    //        row[TradeConst.UnwindCount] = DictData[key].UnwindCount;
                    //    }
                    //}
                    _StrategyMarketTable.Rows.InsertAt(row, 0);
                    this.Invoke(new MethodInvoker(delegate { tradeBookDataGrid1.Refresh(); }));
                    tradeBookDataGrid1.CurrentCell = tradeBookDataGrid1[1, 0];

                    if (_StrategyMarketTable.Rows.Count > 200)
                    {
                        DataRow dr = _StrategyMarketTable.Rows[_StrategyMarketTable.Rows.Count];
                        dr.Delete();
                    }
                }
            }
        }

        public void SummaryInsertTrade(BTPacket.BoxTradeMsg Trade)
        {
            bool baddNew = true;
            if (AppGlobal.frmWatch != null && AppGlobal.frmWatch.InvokeRequired)
            {
                AppGlobal.frmWatch.BeginInvoke((MethodInvoker)(() => SummaryInsertTrade(Trade)));
            }
            else
            {
                if (Trade.TransCode == 3434)
                {
                    DataRow dr = null;
                    string wind_unwind = "";
                    if (Trade.IsWind == 1)
                        wind_unwind = "Wind";
                    else
                        wind_unwind = "UnWind";
                    string DATE = SecondToDateTime(Market.NseCm, Convert.ToInt32(Trade.Expiry)).ToString("ddMMMyyyy");
                   
                    if (_SummaryStrategyTable.Rows.Count > 0)
                    {
                        string filter = TradeConst.L1Stk + "='" + (Convert.ToInt32(Trade.Strike1) / 100).ToString().Trim() + "' " + " AND " +
                                                                        TradeConst.L2Stk + "='" + (Convert.ToInt32(Trade.Strike2) / 100).ToString().Trim() + "' " + " AND " +                                                                        
                                                                        TradeConst.Expiry + "='" + (DATE.Trim()).ToString() + "' ";

                        DataRow[] drExist = _SummaryStrategyTable.Select(filter);
                        if (drExist.Length > 0)
                        {
                            dr = drExist[0];
                            baddNew = false;
                        }
                        else
                        {

                            dr = _SummaryStrategyTable.NewRow();
                        }

                    }
                    else
                    {
                        if (_StrategyMarketTable != null)
                        {
                            dr = _SummaryStrategyTable.NewRow();
                        }
                    }
                    if (baddNew)
                    {
                        dr[TradeConst.Time] = DateTime.Now.ToString("HH:mm:ss:ffff");
                        dr[TradeConst.IsWind] = wind_unwind;
                        dr[TradeConst.L1Stk] = (Convert.ToDouble(Trade.Strike1) / 100).ToString();
                        dr[TradeConst.L2Stk] = (Convert.ToDouble(Trade.Strike2) / 100).ToString();
                        
                        dr[TradeConst.Expiry] = DATE;
                        dr[TradeConst.Sprd] = Math.Round(Trade.Spread / 100, 2);
                        if (Trade.IsWind == 1)
                        {
                            dr[TradeConst.WindCount] = 1;
                            dr[TradeConst.UnwindCount] = 0;
                            dr[TradeConst.SUMWIND] = Math.Round(Trade.Spread / 100, 2);
                            dr[TradeConst.SUMUNWIND] = 0;
                            dr[TradeConst.AvgWind] = Math.Round(Trade.Spread / 100, 2);
                        }
                        else
                        {
                            dr[TradeConst.WindCount] = 1;
                            dr[TradeConst.UnwindCount] = 0;
                            dr[TradeConst.SUMUNWIND] = Math.Round(Trade.Spread / 100, 2);
                            dr[TradeConst.SUMWIND] = 0;
                            dr[TradeConst.AvgUnwind] = Math.Round(Trade.Spread / 100, 2);
                        }
                        _SummaryStrategyTable.Rows.Add(dr);
                    }
                    else
                    {
                        if (Trade.IsWind == 1)
                        {
                            dr[TradeConst.WindCount] = Convert.ToInt32(dr[TradeConst.WindCount]) + 1;
                            dr[TradeConst.SUMWIND] = Math.Round(Convert.ToDouble(dr[TradeConst.SUMWIND]) + Math.Round(Trade.Spread / 100, 2), 2);
                            if (Convert.ToInt32(dr[TradeConst.WindCount]) > 0)
                                dr[TradeConst.AvgWind] = Math.Round(Convert.ToDouble(dr[TradeConst.SUMWIND]) / Convert.ToDouble(dr[TradeConst.WindCount]), 2);
                        }
                        else
                        {
                            dr[TradeConst.UnwindCount] = Convert.ToInt32(dr[TradeConst.UnwindCount]) + 1;
                            dr[TradeConst.SUMUNWIND] = Math.Round(Convert.ToDouble(dr[TradeConst.SUMUNWIND]) + Math.Round(Trade.Spread / 100, 2), 2);
                            if (Convert.ToInt32(dr[TradeConst.UnwindCount]) > 0)
                                dr[TradeConst.AvgUnwind] = Math.Round(Convert.ToDouble(dr[TradeConst.SUMUNWIND]) / Convert.ToDouble(dr[TradeConst.UnwindCount]), 2);
                        }
                        dr[TradeConst.IsWind] = wind_unwind;
                        dr[TradeConst.Sprd] = Math.Round(Trade.Spread / 100, 2);
                    }
                    this.Invoke(new MethodInvoker(delegate { summary.Refresh(); }));
                    summary.CurrentCell = summary[1, 0];
                }
            }
        }

        #region Date converstion methods

        public DateTime SecondToDateTime(Market market, UInt32 second)
        {
            try
            {
                DateTime date = new DateTime();
                if (market == Market.NseCm || market == Market.NseFO)
                    date = new DateTime(1980, 1, 1, 0, 0, 0, 0);
                else if (market == Market.Own || market == Market.Mcx || market == Market.Mcxsx)
                    date = new DateTime(1970, 1, 1, 0, 0, 0, 0);

                date = date.AddSeconds(second);
                return date;
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        public DateTime SecondToDateTime(Market market, Int32 second)
        {
            try
            {
                DateTime date = new DateTime();
                if (market == Market.NseCm || market == Market.NseFO)
                    date = new DateTime(1980, 1, 1, 0, 0, 0, 0);
                else if (market == Market.Own || market == Market.Mcx || market == Market.Mcxsx)
                    date = new DateTime(1970, 1, 1, 0, 0, 0, 0);

                date = date.AddSeconds(second);
                return date;
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
        }

        public UInt32 DateTimeToSecond(Market market, DateTime date)
        {
            try
            {
                DateTime startDate = new DateTime();
                if (market == Market.NseCm || market == Market.NseFO)
                    startDate = new DateTime(1980, 1, 1, 0, 0, 0, 0);
                else if (market == Market.Own || market == Market.Mcx || market == Market.Mcxsx)
                    startDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);

                TimeSpan ts = date - startDate;

                return (UInt32)ts.TotalSeconds;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        #endregion

        void connection_OnMarketWatch(BTPacket.GUIUpdate _maketWatchGUI)
        {
            
        }

        private void MARKETWATCH_Load(object sender, EventArgs e)
        {
            tradeBookDataGrid1.DataSource = _StrategyMarketTable;
            summary.DataSource = _SummaryStrategyTable;

            summary.LoadSaveSettings();

        }
         
        

        private void MARKETWATCH_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (AppGlobal.frmMarketWatch != null)
                {
                    AppGlobal.frmMarketWatch = null;
                }                    
            }
            catch(Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "frmTrade_FormClosed")
                             , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        private void MARKETWATCH_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal.frmMarketWatch = null;
           
        }

        private void tradeBookDataGrid1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            
        }

        private void summary_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int iRow = summary.CurrentCell.RowIndex;
            int iCount = summary.RowCount - 1;
            bool Flag = false;
            UInt64 uniqueid;
            string strl1 = Convert.ToString(Convert.ToInt32(summary.Rows[iRow].Cells[TradeConst.L1Stk].Value) / 100);
            string strl2 = Convert.ToString(Convert.ToInt32(summary.Rows[iRow].Cells[TradeConst.L2Stk].Value) / 100);
            uniqueid = Convert.ToUInt64(Convert.ToString(strl1 + strl2));
            double Spread = Convert.ToDouble(summary.Rows[iRow].Cells[TradeConst.Sprd].Value);
            string iswind = Convert.ToString(summary.Rows[iRow].Cells[TradeConst.IsWind].Value);
            BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
            foreach (var watch in AppGlobal.MarketWatch.Where(x => (x.uniqueId == uniqueid)))
            {
                Flag = true;
                if (iswind == "Wind")
                {

                    snd.TransCode = 1;
                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    snd.UniqueID = unique;
                    snd.Wind = Spread; //Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
                    snd.Unwind = Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
                    snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                    snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                    snd.StrategyId = 3434;
                }
                else if (iswind == "UnWind")
                {

                    snd.TransCode = 1;
                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    snd.UniqueID = unique;
                    snd.Wind = Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
                    snd.Unwind = Spread;//Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
                    snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                    snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                    snd.StrategyId = 3434;
                }
                if (((snd.Wind / 100) + (snd.Unwind / 100)) < 0)
                {
                    MessageBox.Show("Please Check Wind and Unwind Parameter!!!!");
                    return;
                }
            }
            if (Flag)
            {
                TransactionWatch.TransactionMessage("send data to intermediate !!!", Color.Blue);
                long seq = ClassDisruptor.ringBufferRequest.Next();
                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                ClassDisruptor.ringBufferRequest.Publish(seq);
            }
            else
            {
                MessageBox.Show("Please add Rule and send order!!!!");
            }
        }
    }

    public class CountWindUnWind
    {
        public int windCount;
        public int UnwindCount;
    }
}
