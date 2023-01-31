using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using ClientCommon;
using Straddle.AppClasses;
using LogWriter;
using MTCommon;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;
using ArisDev;
using MTControls;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Reflection;
using Disruptor;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Threading;



namespace Straddle
{
    public partial class OptionWatch : DockContent
    {
        #region Variables
        private ToolStripMenuItem tlsmiActiveDeActive = new ToolStripMenuItem();
        private ToolStripSeparator tlsSeparator = new ToolStripSeparator();
        internal DataTable _tradeBookTable1;
        System.Windows.Forms.Timer RunningPnl = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer heartBeatCheck = new System.Windows.Forms.Timer();
       

        public string[] threeExpiry;
        string[] threeFinNiftyExpiry;
        int count = 100;
        //Timer timer;
        List<string> _StrategyList = new List<string>();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        #endregion

        #region Constructor

        public OptionWatch()
        {
            InitializeComponent();

            initializeDis();

            _tradeBookTable1 = new DataTable();
            _tradeBookTable1.TableName = "TradeBook1";
            CreateTable();

            AppGlobal.connection = new Connection();
            AppGlobal.connection.MKTClientConnect += new AppGlobal.MKTTerminal_ConnectDel(connection_MKTClientConnect);
            AppGlobal.connection.MKTClientDisconnect += new AppGlobal.MKTTerminal_DisconnectDel(connection_MKTClientDisconnect);
            //AppGlobal.connection.RMSMessageRecived += new AppGlobal.RMSTerminal_MessageRecivedDel(connection_RMSMessageRecived);

            BindOrderTradeEvents();


            if (ArisApi_a._arisApi.SystemConfig.Type == "TCP")
            {
                AppGlobal.connection.setTcpMdSocket();
                TransactionWatch.TransactionMessage("Connected with TCP Connection", Color.Blue);
            }
            if (ArisApi_a._arisApi.SystemConfig.RmsConnect == true)
            {
                AppGlobal.connection._setRMSConnection();
                TransactionWatch.TransactionMessage("RMS is Connected", Color.Blue);
            }
            else
            {
                TransactionWatch.TransactionMessage("RMS is not Connected", Color.Blue);
            }
            FormClosing += FrmWatch_FormClosing;
            FormClosed += FrmWatch_FormClosed;
            dgvMarketWatch.CellFormatting += dgvMarketWatch_CellFormatting;
            dgvMarketWatch.MouseClick += dgvMarketWatch_MouseClick;
            dgvMarketWatch.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            tlsSeparator = new ToolStripSeparator();
            tlsmiActiveDeActive = new ToolStripMenuItem();
            tlsmiActiveDeActive.Name = "tlsmiActiveDeActive";
            tlsmiActiveDeActive.Text = "Active/Deactive";
            tlsmiActiveDeActive.CheckOnClick = true;
            tlsmiActiveDeActive.Click += tlsmiActiveDeActive_Click;
            dgvMarketWatch.cmsColumn.Items.Insert(0, tlsSeparator);
            dgvMarketWatch.cmsColumn.Items.Insert(0, tlsmiActiveDeActive);

       

        }
        #endregion

        public void back_Files()
        {
            string path = Application.StartupPath + "\\" + "Logs" + "\\";
            string date = DateTime.Now.ToString("ddMMMyyyy") + ".csv";
            string fileName = path + "SQPnl" + ".csv";
            if (File.Exists(fileName))
                readFileExcelScrip(fileName);
            TransactionWatch.ErrorMessage("GUI Received|" + AppGlobal.GUI_ID);
            TransactionWatch.ErrorMessage("PNL|" + Math.Round(AppGlobal.OverAllPnl, 2).ToString());

            AppGlobal.OverAllPnl = AppGlobal.OverAllPnl + AppGlobal.DuePnl;
            OverAll_pnl.Text = Math.Round(AppGlobal.OverAllPnl, 2).ToString();
            lblPnl.Text = Math.Round(AppGlobal.Pnl, 2).ToString();

            string fileName1 = path + "Count.csv";
            if (File.Exists(fileName1))
                readFileExcelScripCount(fileName1);
        }


        public void MatchUniqueNo()
        {
            bool mismatch = true;
            bool mismatchStrategy = true;
            AppGlobal.uniqueNoMatch.Clear();
            AppGlobal.uniqueStrategyMatch.Clear();
            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watch = AppGlobal.MarketWatch[i];

                if (watch.StrategyId != 0)
                {
                    if (!AppGlobal.uniqueNoMatch.Contains(watch.uniqueId))
                    {
                        AppGlobal.uniqueNoMatch.Add(watch.uniqueId);
                    }
                    else
                    {
                        mismatch = false;
                    }
                    if (mismatch == false)
                    {
                        MessageBox.Show("Same Uniqueid is created multiple time " + watch.uniqueId);
                        return;
                    }
                }
                else if (watch.StrategyId == 0)
                {
                    if (!AppGlobal.uniqueStrategyMatch.Contains(watch.Strategy))
                    {
                        AppGlobal.uniqueStrategyMatch.Add(watch.Strategy);
                    }
                    else
                    {
                        mismatchStrategy = false;
                    }
                    if (mismatchStrategy == false)
                    {
                        MessageBox.Show("Same StrategyName is created multiple time");
                        return;
                    } 
                }

            }

            TransactionWatch.ErrorMessage("All Unique is created onetime only");
            TransactionWatch.TransactionMessage("All Unique is created onetime only", Color.Red);
        }



        //void connection_RMSMessageRecived(Socket socket, byte[] message)
        //{
        //    if (InvokeRequired)
        //        BeginInvoke((MethodInvoker)(() => connection_RMSMessageRecived(socket, message)));
        //    else
        //    {
        //        try
        //        {
        //            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(message);
        //            if (packetHeader.TransCode == 99)
        //            {
        //                if (packetHeader.UniqueID != 0)
        //                {
        //                    threeExpiry = GetExpiryDates(ArisApi_a._arisApi.DsContract.Tables["NSEFO"]);
        //                    DateTime dt1 = Convert.ToDateTime(threeExpiry[0].ToString());
        //                    DateTime dt2 = Convert.ToDateTime(threeExpiry[1].ToString());
        //                    DateTime dt3 = Convert.ToDateTime(threeExpiry[2].ToString());
        //                    ArisApi_a._arisApi.GenerateTradeFiles();
        //                    AppGlobal.GUI_ID = packetHeader.UniqueID;
        //                    txtUniqueid.Text = AppGlobal.GUI_ID.ToString();
        //                    AppGlobal.MarketWatch = MarketWatch.ReadXmlProfile();
        //                    AssignMarketStructValue(AppGlobal.MarketWatch);
        //                    LSL_Strangle_AvgPrice();
        //                    back_Files();
        //                    lblMargin.Text = Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3).ToString();
        //                    lblcallbuy.Text = Math.Round(AppGlobal.CallBuyMTM, 2).ToString();
        //                    lblcallsell.Text = Math.Round(AppGlobal.CallSellMTM, 2).ToString();
        //                    lblputbuy.Text = Math.Round(AppGlobal.PutBuyMTM, 2).ToString();
        //                    lblputsell.Text = Math.Round(AppGlobal.PutSellMTM, 2).ToString();
        //                    CallMTM.Text = (Math.Round(AppGlobal.CallBuyMTM, 2) + Math.Round(AppGlobal.PutBuyMTM, 2)).ToString();
        //                    PutMTM.Text = (Math.Round(AppGlobal.CallSellMTM, 2) + Math.Round(AppGlobal.PutSellMTM, 2)).ToString();
        //                    CrashRMS.Text = "ON";
        //                    CrashRMS.BackColor = Color.Green;
        //                    AppGlobal.connection.MKTMessageRecived += new AppGlobal.MKTTerminal_MessageRecivedDel(connection_MKTMessageRecived);
        //                    //ArisApi_a._arisApi.OnMarketDepthUpdate += new ArisApi_a.MarketDepthUpdateDelegate(_arisApi_OnMarketDepthUpdate);
        //                    //ArisApi_a._arisApi.OnIndexBroadCast += new ArisApi_a.IndexBroadCastUpdateDelegate(_arisApi_OnIndexBroadCast);
        //                    SendToTradeAdmin("Connect");
        //                    RunningPnl.Interval = Convert.ToInt32(ArisApi_a._arisApi.SystemConfig.updateMin * 60000);
        //                    RunningPnl.Tick += new EventHandler(RunningPnl_Tick);
        //                    RunningPnl.Start();

        //                    heartBeatCheck.Interval = Convert.ToInt32(2 * 60000);
        //                    heartBeatCheck.Tick += new EventHandler(heartBeatCheck_Tick);
        //                    heartBeatCheck.Start(); 
        //                    MatchUniqueNo();
        //                }
        //                else
        //                {
        //                    TransactionWatch.ErrorMessage("Application Already open with GUI id" + AppGlobal.GUI_ID);
        //                    AppGlobal.frmWatch.Close();
        //                }
        //            }
        //            else if (packetHeader.TransCode == 17)
        //            {
        //                if (packetHeader.gui_id == AppGlobal.GUI_ID)
        //                {
        //                    CrashRMS.Text = "OFF";
        //                    CrashRMS.BackColor = Color.Red;
        //                    MessageBox.Show("Trading Has been Stopped!!!!");
        //                }
        //            }
        //            else if (packetHeader.TransCode == 20)
        //            {
        //               MessageBox.Show("User id | " + packetHeader.Token + " Limit Hit");
        //            }
        //            else if (packetHeader.TransCode == 1 || packetHeader.TransCode == 2 || packetHeader.TransCode == 5)
        //            {
        //                if (packetHeader.gui_id == AppGlobal.GUI_ID)
        //                {
        //                    if (packetHeader.StrategyId == 91)
        //                    {
        //                        #region TransCode 1 for strategy id 91
        //                        if (packetHeader.TransCode == 1)
        //                        {
        //                            if (packetHeader.StrategyId == 91)
        //                            {
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    int i = watch1.RowData.Index;
        //                                    watch1.Wind = Convert.ToDecimal(packetHeader.Wind / 100);
        //                                    watch1.unWind = Convert.ToDecimal(packetHeader.Unwind / 100);

        //                                    watch1.RowData.Cells[WatchConst.Wind].Value = watch1.Wind;
        //                                    watch1.RowData.Cells[WatchConst.UnWind].Value = watch1.unWind;


        //                                }
        //                            }
        //                        }
        //                        #endregion

        //                        #region TransCode 2 for strategy id 91
        //                        if (packetHeader.TransCode == 2)
        //                        {
        //                            if (packetHeader.StrategyId == 91)
        //                            {
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    int i = watch1.RowData.Index;
        //                                    dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.White;
        //                                }
        //                            }
        //                        }
        //                        #endregion

        //                        #region TransCode 5 for strategy id 91
        //                        if (packetHeader.TransCode == 5)
        //                        {
        //                            if (packetHeader.StrategyId == 91)
        //                            {
        //                                //Thread write1 = new Thread(() =>
        //                                //{
        //                                    AllInsertTrade(packetHeader);
        //                                //});
        //                                //write1.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta
        //                                //write1.Start();

        //                                #region trade for Watch
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    //Thread write = new Thread(() =>
        //                                    //{
        //                                        double PrvMargin = 0;
        //                                        int i = watch1.RowData.Index;
        //                                        string side = "";
        //                                        double mtm = 0;
        //                                        if (packetHeader.OverNightWindPos >= watch1.Over || packetHeader.OverNightUnWindPos >= watch1.Round)
        //                                        {

        //                                        }
        //                                        else
        //                                        {
        //                                            TransactionWatch.ErrorMessage("over night wind : " + packetHeader.OverNightWindPos + " : over night unwind : " + packetHeader.OverNightUnWindPos
        //                                                                            + " : my open : " + watch1.Over + " : my Round : " + watch1.Round);
        //                                        }

        //                                        if (packetHeader.isWind)
        //                                            dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
        //                                        else
        //                                            dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;

        //                                        if (packetHeader.isWind)
        //                                            side = "Wind";
        //                                        else
        //                                            side = "UnWind";
        //                                        PrvMargin = Convert.ToDouble(watch1.MarginUtilise);
        //                                        mtm = Convert.ToDouble(watch1.premium);
        //                                        if (side == "Wind")
        //                                        {
        //                                            if (watch1.Leg1.MidPrice != 0)
        //                                            {
        //                                                if (watch1.Leg1.ContractInfo.Series == "XX")
        //                                                    watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0001 * watch1.Leg1.Ratio);
        //                                                else
        //                                                    watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0007 * watch1.Leg1.Ratio);
        //                                            }
        //                                            else
        //                                                watch1.UnwindTrnCost = 0;
        //                                            watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) + watch1.UnwindTrnCost);
        //                                            watch1.Leg1.B_Qty = watch1.Leg1.B_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                            watch1.Leg1.B_Value = watch1.Leg1.B_Value + ((packetHeader.TradePrice + watch1.UnwindTrnCost) * watch1.Leg1.ContDetail.LotSize);
        //                                            watch1.Leg1.Buy_Qty = watch1.Leg1.Buy_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                        }
        //                                        else
        //                                        {
        //                                            if (watch1.Leg1.MidPrice != 0)
        //                                            {

        //                                                if (watch1.Leg1.ContractInfo.Series == "XX")
        //                                                    watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0001 * watch1.Leg1.Ratio);
        //                                                else
        //                                                    watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0011 * watch1.Leg1.Ratio);
        //                                            }
        //                                            else
        //                                                watch1.WindTrnCost = 0;
        //                                            watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) - watch1.WindTrnCost);
        //                                            watch1.Leg1.S_Qty = watch1.Leg1.S_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                            watch1.Leg1.S_Value = watch1.Leg1.S_Value + ((packetHeader.TradePrice - watch1.WindTrnCost) * watch1.Leg1.ContDetail.LotSize);
        //                                            watch1.Leg1.Sell_Qty = watch1.Leg1.Sell_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                        }
        //                                        watch1.Leg1.N_Qty = (watch1.Leg1.B_Qty - watch1.Leg1.S_Qty);
        //                                        watch1.Leg1.Net_Qty = (watch1.Leg1.Sell_Qty - watch1.Leg1.Buy_Qty);
        //                                        watch1.Leg1.A_Value = (watch1.Leg1.S_Value - watch1.Leg1.B_Value);

        //                                        watch1.ProfitFlg = false;
        //                                        watch1.DrawDownFlg = false;
        //                                        if (watch1.Leg1.N_Qty != 0)
        //                                        {
        //                                            watch1.Leg1.N_Price = Math.Round(watch1.Leg1.A_Value / watch1.Leg1.Net_Qty, 2);
        //                                            watch1.NetAvgPrice = (watch1.Leg1.S_Value - watch1.Leg1.B_Value) / (watch1.Leg1.Net_Qty);
        //                                            watch1.RowData.Cells[WatchConst.AvgPrice].Value = Math.Round(watch1.Leg1.N_Price, 2);
        //                                        }
        //                                        else
        //                                        {
                                                    
        //                                            watch1.Sqpnl = watch1.Sqpnl + (watch1.Leg1.S_Value - watch1.Leg1.B_Value);
        //                                            AppGlobal.OverAllPnl = AppGlobal.OverAllPnl + (watch1.Leg1.S_Value - watch1.Leg1.B_Value);
        //                                            OverAll_pnl.Text = Math.Round(AppGlobal.OverAllPnl, 2).ToString();
        //                                            watch1.Leg1.N_Price = 0;
        //                                            watch1.avgPrice = 0;
        //                                            watch1.NetAvgPrice = 0;
        //                                            watch1.RowData.Cells[WatchConst.AvgPrice].Value = watch1.Leg1.N_Price;
        //                                            watch1.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch1.Sqpnl, 2);
        //                                            #region Strategy Square off Pnl
        //                                            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch1.Strategy) &&
        //                                                                                                             (Convert.ToString(x.StrategyId) == "0")))
        //                                            {
        //                                                watch.Sqpnl = watch.Sqpnl + watch1.Sqpnl;
        //                                                watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
        //                                            }
        //                                            #endregion
        //                                        }
        //                                        if (watch1.Leg1.N_Qty > 0)
        //                                        {
        //                                            watch1.PosType = "Wind";
        //                                            watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
        //                                            watch1.posInt = (watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize));
        //                                            watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                        }
        //                                        else if (watch1.Leg1.N_Qty < 0)
        //                                        {
        //                                            watch1.PosType = "UnWind";
        //                                            watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
        //                                            watch1.posInt = (watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize));
        //                                            watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                        }
        //                                        else
        //                                        {
        //                                            watch1.pnl = 0;
        //                                            watch1.RowData.Cells[WatchConst.PNL].Value = watch1.pnl;
        //                                            watch1.RowData.Cells[WatchConst.PosType].Value = "None";
        //                                            watch1.posInt = 0;
        //                                            watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                            watch1.avgPrice = 0;
        //                                            watch1.Leg1.B_Qty = 0;
        //                                            watch1.Leg1.S_Qty = 0;
        //                                            watch1.Leg1.B_Value = 0;
        //                                            watch1.Leg1.S_Value = 0;
        //                                            watch1.Leg1.N_Qty = 0;
        //                                            watch1.Leg1.N_Price = 0;
        //                                            watch1.Leg1.Buy_Qty = 0;
        //                                            watch1.Leg1.Sell_Qty = 0;
        //                                            watch1.Leg1.Net_Qty = 0;
        //                                        }

        //                                        foreach (var Stranglewatch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.UniqueIdLeg1) == packetHeader.UniqueID) && x.Leg2.ContractInfo.TokenNo != "0"))
        //                                        {
        //                                            Stranglewatch.L1PosInt = watch1.posInt;
        //                                            Stranglewatch.RowData.Cells[WatchConst.LSL_L1PosInt].Value = Stranglewatch.L1PosInt;

        //                                            Stranglewatch.LSL_AvgPriceCE = watch1.Leg1.N_Price;
        //                                            Stranglewatch.RowData.Cells[WatchConst.LSL_AvgPriceCE].Value = Stranglewatch.LSL_AvgPriceCE;
        //                                        }
        //                                        foreach (var Stranglewatch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.UniqueIdLeg2) == packetHeader.UniqueID) && x.Leg2.ContractInfo.TokenNo != "0"))
        //                                        {
        //                                            Stranglewatch.L2PosInt = watch1.posInt;
        //                                            Stranglewatch.RowData.Cells[WatchConst.LSL_L2PosInt].Value = Stranglewatch.L2PosInt;

        //                                            Stranglewatch.LSL_AvgPricePE = watch1.Leg1.N_Price;
        //                                            Stranglewatch.RowData.Cells[WatchConst.LSL_AvgPricePE].Value = Stranglewatch.LSL_AvgPricePE;
        //                                        }

        //                                        watch1.premium = watch1.Leg1.N_Price * watch1.posInt * watch1.Leg1.ContDetail.LotSize * -1;
        //                                        watch1.RowData.Cells[WatchConst.Premium].Value = Math.Round(watch1.premium, 2);
        //                                        watch1.TradedQty = watch1.Leg1.ContDetail.LotSize * watch1.posInt;
        //                                        watch1.RowData.Cells[WatchConst.TradedQty].Value = watch1.TradedQty;
        //                                        if (watch1.Leg1.ContractInfo.Series == "CE")
        //                                        {
        //                                            AppGlobal.CallMTM = AppGlobal.CallMTM - mtm + watch1.premium;
        //                                            if (watch1.posInt > 0)
        //                                            {
        //                                                AppGlobal.CallBuyMTM = AppGlobal.CallBuyMTM - mtm + watch1.premium;
        //                                            }
        //                                            else
        //                                            {
        //                                                AppGlobal.CallSellMTM = AppGlobal.CallSellMTM - mtm + watch1.premium;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            AppGlobal.PutMTM = AppGlobal.PutMTM - mtm + watch1.premium;
        //                                            if (watch1.posInt > 0)
        //                                            {
        //                                                AppGlobal.PutBuyMTM = AppGlobal.PutBuyMTM - mtm + watch1.premium;
        //                                            }
        //                                            else
        //                                            {
        //                                                AppGlobal.PutSellMTM = AppGlobal.PutSellMTM - mtm + watch1.premium;
        //                                            }
        //                                        }
        //                                        AppGlobal.overallPremium = (AppGlobal.overallPremium - mtm) + (watch1.premium);
        //                                        if (AppGlobal.overallPremium != 0)
        //                                        {
        //                                            premiumlbl.Text = Convert.ToString(Math.Round(AppGlobal.overallPremium / 10000000, 3));
        //                                        }
        //                                        else
        //                                        {
        //                                            premiumlbl.Text = "0";
        //                                        }

        //                                        #region Margin Calculate
        //                                        if (watch1.posInt != 0)
        //                                        {
        //                                            if (watch1.posInt < 0)
        //                                            {
        //                                                if (watch1.Leg1.ContractInfo.Symbol == "NIFTY")
        //                                                    watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.niftyMargin), 2);
        //                                                if (watch1.Leg1.ContractInfo.Symbol == "BANKNIFTY")
        //                                                    watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.bankniftyMargin), 2);
        //                                            }

        //                                        }
        //                                        else if (watch1.posInt == 0)
        //                                        {
        //                                            watch1.MarginUtilise = 0;
        //                                        }
        //                                        watch1.RowData.Cells[WatchConst.MarginUtilise].Value = watch1.MarginUtilise;
        //                                        AppGlobal.OverallMarginUtilize = (AppGlobal.OverallMarginUtilize - PrvMargin) + (watch1.MarginUtilise);
        //                                        if (AppGlobal.OverallMarginUtilize != 0)
        //                                        {
        //                                            lblMargin.Text = Convert.ToString(Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3));
        //                                        }
        //                                        else
        //                                        {
        //                                            lblMargin.Text = "0";
        //                                        }

        //                                        TransactionWatch.ErrorMessage("|UniqueId|" + watch1.uniqueId + "|Strategy|" + watch1.StrategyId + "|symbol|" + watch1.Leg1.ContractInfo.Symbol + "|strike|" + watch1.Leg1.ContractInfo.StrikePrice + "|lotsize|" + watch1.Leg1.ContDetail.LotSize + "|NetQty|" +
        //                                                                       watch1.Leg1.N_Qty + "|NetAvg|" + watch1.Leg1.N_Price + "|SqPnl|" + watch1.Sqpnl + "|CurrPnl|" + watch1.pnl + "|Type|" + side + "|TradePrice|" + packetHeader.TradePrice.ToString() + "|AvgPrice|" + watch1.NetAvgPrice
        //                                                                       + "|PosInt|" + watch1.posInt + "|BuyValue|" + watch1.Leg1.B_Value + "|SellValue|" + watch1.Leg1.S_Value + "|NetValue|" + watch1.Leg1.A_Value); 
        //                                        #endregion

        //                                        if (side == "Wind")
        //                                        {
        //                                            TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                          + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
        //                                                                          + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + packetHeader.TradePrice.ToString() + "," + "0" + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());

        //                                            TransactionWatch.OnlyTradeMessage(DateTime.Now.ToString("HH:mm:ss") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.TokenNo + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                           + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
        //                                                                           + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + packetHeader.TradePrice.ToString() + "," + "0" + "," + "0" + "," + "Wind" + "," + packetHeader.WindPos + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());

        //                                        }
        //                                        else
        //                                        {
        //                                            TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                          + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
        //                                                                          + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + packetHeader.TradePrice.ToString() + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());

        //                                            TransactionWatch.OnlyTradeMessage(DateTime.Now.ToString("HH:mm:ss") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.TokenNo + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                         + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
        //                                                                         + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + packetHeader.TradePrice.ToString() + "," + "UnWind" + "," + packetHeader.UnWindPos + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());

        //                                        }
        //                                        if (AppGlobal.SQAllFlg)
        //                                        {
        //                                            if (watch1.posInt == 0)
        //                                            {
        //                                                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
        //                                                AppGlobal.SQAllFlg = true;
        //                                            }
        //                                        }
        //                                        else
        //                                            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
        //                                    //});
        //                                    //write.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta
        //                                    //write.Start();
        //                                }
        //                                #endregion

        //                                AppGlobal.Count_single = AppGlobal.Count_single + 1;
        //                                AppGlobal.TotalTrade = AppGlobal.TotalTrade + 1;
        //                                lblTotalTrade.Text = Convert.ToString(AppGlobal.TotalTrade);
        //                                //Thread write3 = new Thread(() =>
        //                                //{
        //                                //    //if (AppGlobal.SQAllFlg)
        //                                //    //{
        //                                //        MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
        //                                //    //}

        //                                //});
        //                                //write3.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta
        //                                //write3.Start();

        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    else if (packetHeader.StrategyId == 2211)
        //                    {

        //                        #region TransCode 1 for strategy id 2211
        //                        if (packetHeader.TransCode == 1)
        //                        {
        //                            if (packetHeader.StrategyId == 2211)
        //                            {
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    int i = watch1.RowData.Index;
        //                                    watch1.Wind = Convert.ToDecimal(packetHeader.Wind / 100);
        //                                    watch1.unWind = Convert.ToDecimal(packetHeader.Unwind / 100);

        //                                    watch1.RowData.Cells[WatchConst.Wind].Value = watch1.Wind;
        //                                    watch1.RowData.Cells[WatchConst.UnWind].Value = watch1.unWind;


        //                                }
        //                            }
        //                        }
        //                        #endregion

        //                        #region TransCode 2 for strategy id 2211
        //                        if (packetHeader.TransCode == 2)
        //                        {
        //                            if (packetHeader.StrategyId == 2211)
        //                            {
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    int i = watch1.RowData.Index;
        //                                    dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.White;
        //                                }
        //                            }
        //                        }
        //                        #endregion

        //                        #region TransCode 5 for strategy id 2211
        //                        if (packetHeader.TransCode == 5)
        //                        {
        //                            if (packetHeader.StrategyId == 2211)
        //                            {
        //                                AllInsertTrade(packetHeader);

        //                                #region Trade for Watch
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    double PrvMargin = 0;
        //                                    string side = "";
        //                                    double mtm = 0;
        //                                    int i = watch1.RowData.Index;
        //                                    if (packetHeader.isWind)
        //                                        dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
        //                                    else
        //                                        dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
        //                                    if (packetHeader.isWind)
        //                                        side = "Wind";
        //                                    else
        //                                        side = "UnWind";
        //                                    PrvMargin = Convert.ToDouble(watch1.MarginUtilise);
        //                                    mtm = Convert.ToDouble(watch1.premium);
        //                                    if (side == "Wind")
        //                                    {
        //                                        if (watch1.Leg1.MidPrice != 0 && watch1.Leg2.MidPrice != 0)
        //                                            watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0007) + (watch1.Leg2.MidPrice * 0.0011 * watch1.Leg2.Ratio);
        //                                        else
        //                                            watch1.WindTrnCost = 0;
        //                                        watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) - watch1.WindTrnCost);
        //                                        watch1.Leg1.B_Qty = watch1.Leg1.B_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                        watch1.Leg1.B_Value = watch1.Leg1.B_Value + (((packetHeader.TradePrice) - watch1.WindTrnCost) * watch1.Leg1.ContDetail.LotSize);
        //                                        watch1.Leg1.Buy_Qty = watch1.Leg1.Buy_Qty + (watch1.Leg1.ContDetail.LotSize * watch1.Leg1.Ratio);
        //                                        watch1.Leg2.Buy_Qty = watch1.Leg2.Buy_Qty + (watch1.Leg2.ContDetail.LotSize * watch1.Leg2.Ratio);
        //                                    }
        //                                    else
        //                                    {
        //                                        if (watch1.Leg1.MidPrice != 0 && watch1.Leg2.MidPrice != 0)
        //                                            watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0011) + (watch1.Leg2.MidPrice * 0.0007 * watch1.Leg2.Ratio);
        //                                        else
        //                                            watch1.UnwindTrnCost = 0;
        //                                        watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) - watch1.UnwindTrnCost);
        //                                        watch1.Leg1.S_Qty = watch1.Leg1.S_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                        watch1.Leg1.S_Value = watch1.Leg1.S_Value + ((packetHeader.TradePrice) - watch1.UnwindTrnCost) * watch1.Leg1.ContDetail.LotSize;
        //                                        watch1.Leg2.Sell_Qty = watch1.Leg2.Sell_Qty + (watch1.Leg2.ContDetail.LotSize * watch1.Leg2.Ratio);
        //                                        watch1.Leg1.Sell_Qty = watch1.Leg1.Sell_Qty + (watch1.Leg1.ContDetail.LotSize * watch1.Leg1.Ratio);
        //                                    }
        //                                    watch1.Leg1.N_Qty = watch1.Leg1.B_Qty - watch1.Leg1.S_Qty;
        //                                    watch1.Leg1.Net_Qty = watch1.Leg1.Buy_Qty - watch1.Leg1.Sell_Qty;
        //                                    watch1.Leg2.Net_Qty = watch1.Leg2.Buy_Qty - watch1.Leg2.Sell_Qty;
        //                                    watch1.ProfitFlg = false;
        //                                    watch1.DrawDownFlg = false;
        //                                    if (watch1.Leg1.N_Qty != 0)
        //                                    {
        //                                        watch1.Leg1.N_Price = Math.Round((watch1.avgPrice / (Math.Abs(watch1.Leg1.N_Qty) / watch1.Leg1.ContDetail.LotSize)), 2);
        //                                        watch1.RowData.Cells[WatchConst.AvgPrice].Value = watch1.Leg1.N_Price;
        //                                    }
        //                                    else
        //                                    {
        //                                        watch1.Leg1.N_Price = 0;
        //                                        watch1.avgPrice = 0;
        //                                        watch1.Sqpnl = watch1.Sqpnl + (watch1.Leg1.B_Value - watch1.Leg1.S_Value);
        //                                        AppGlobal.OverAllPnl = AppGlobal.OverAllPnl + (watch1.Leg1.B_Value - watch1.Leg1.S_Value);
        //                                        OverAll_pnl.Text = Math.Round(AppGlobal.OverAllPnl, 2).ToString();
        //                                        watch1.RowData.Cells[WatchConst.AvgPrice].Value = watch1.Leg1.N_Price;
        //                                        watch1.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch1.Sqpnl, 2);
        //                                        #region Strategy Square off Pnl
        //                                        foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch1.Strategy) &&
        //                                                                                                            (Convert.ToString(x.StrategyId) == "0")))
        //                                        {
        //                                            watch.Sqpnl = watch.Sqpnl + watch1.Sqpnl;
        //                                            watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
        //                                        }
        //                                        #endregion
        //                                    }
        //                                    if (watch1.Leg1.N_Qty > 0)
        //                                    {
        //                                        watch1.PosType = "Wind";
        //                                        watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
        //                                        watch1.posInt = ((watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize)));
        //                                        watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                    }
        //                                    else if (watch1.Leg1.N_Qty < 0)
        //                                    {
        //                                        watch1.PosType = "UnWind";
        //                                        watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
        //                                        watch1.posInt = ((watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize)));
        //                                        watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                    }
        //                                    else
        //                                    {
        //                                        watch1.pnl = 0;
        //                                        watch1.RowData.Cells[WatchConst.PNL].Value = watch1.pnl;
        //                                        watch1.RowData.Cells[WatchConst.PosType].Value = "None";
        //                                        watch1.posInt = 0;
        //                                        watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                        watch1.avgPrice = 0;
        //                                        watch1.Leg1.B_Qty = 0;
        //                                        watch1.Leg1.S_Qty = 0;
        //                                        watch1.Leg1.N_Qty = 0;
        //                                        watch1.Leg1.N_Price = 0;
        //                                        watch1.Leg1.B_Value = 0;
        //                                        watch1.Leg1.S_Value = 0;
        //                                        watch1.Leg1.Buy_Qty = 0;
        //                                        watch1.Leg1.Sell_Qty = 0;
        //                                        watch1.Leg2.Buy_Qty = 0;
        //                                        watch1.Leg2.Sell_Qty = 0;
        //                                        watch1.Leg1.Net_Qty = 0;
        //                                        watch1.Leg2.Net_Qty = 0;
        //                                    }
        //                                    watch1.premium = watch1.Leg1.N_Price * watch1.posInt * watch1.Leg1.ContDetail.LotSize;
        //                                    watch1.RowData.Cells[WatchConst.Premium].Value = Math.Round(watch1.premium, 2);
        //                                    watch1.TradedQty = watch1.Leg1.ContDetail.LotSize * watch1.posInt;
        //                                    watch1.RowData.Cells[WatchConst.TradedQty].Value = watch1.TradedQty;
        //                                    if (watch1.Leg1.ContractInfo.Series == "CE")
        //                                    {
        //                                        AppGlobal.CallMTM = AppGlobal.CallMTM - mtm + (watch1.premium / 2);
        //                                    }
        //                                    else
        //                                    {
        //                                        AppGlobal.PutMTM = AppGlobal.PutMTM - mtm + (watch1.premium / 2);
        //                                    }
        //                                    if (watch1.posInt > 0)
        //                                    {
        //                                        AppGlobal.CallBuyMTM = AppGlobal.CallBuyMTM + (watch1.premium / 2);
        //                                        AppGlobal.PutBuyMTM = AppGlobal.PutBuyMTM + (watch1.premium / 2);
        //                                    }
        //                                    else
        //                                    {
        //                                        AppGlobal.CallSellMTM = AppGlobal.CallSellMTM + (watch1.premium / 2);
        //                                        AppGlobal.PutSellMTM = AppGlobal.PutSellMTM + (watch1.premium / 2);
        //                                    }
        //                                    #region Margin Calculate
        //                                    if (watch1.posInt != 0)
        //                                    {
        //                                        if (watch1.posInt < 0)
        //                                        {
        //                                            if (watch1.Leg1.ContractInfo.Symbol == "NIFTY")
        //                                                watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * watch1.Leg1.Ratio * AppGlobal.niftyMargin * 2), 2);
        //                                            if (watch1.Leg1.ContractInfo.Symbol == "BANKNIFTY")
        //                                                watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * watch1.Leg1.Ratio * AppGlobal.bankniftyMargin * 2), 2);
        //                                        }
        //                                        else if (watch1.posInt > 0)
        //                                        {
        //                                            watch1.MarginUtilise = Math.Round(Convert.ToDouble(watch1.Leg1.N_Price * Math.Abs(watch1.posInt) * watch1.Leg1.ContDetail.LotSize) / 2, 2);
        //                                        }

        //                                    }
        //                                    else if (watch1.posInt == 0)
        //                                    {
        //                                        watch1.MarginUtilise = 0;
        //                                    }
        //                                    watch1.RowData.Cells[WatchConst.MarginUtilise].Value = watch1.MarginUtilise;
        //                                    AppGlobal.OverallMarginUtilize = (AppGlobal.OverallMarginUtilize - PrvMargin) + (watch1.MarginUtilise);
        //                                    if (AppGlobal.OverallMarginUtilize != 0)
        //                                    {
        //                                        lblMargin.Text = Convert.ToString(Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3));
        //                                    }
        //                                    else
        //                                    {
        //                                        lblMargin.Text = "0";
        //                                    }

        //                                    TransactionWatch.ErrorMessage("|UniqueId|" + watch1.uniqueId + "|Strategy|" + watch1.StrategyId + "|symbol|" + watch1.Leg1.ContractInfo.Symbol + "|strike|" + watch1.Leg1.ContractInfo.StrikePrice + "|lotsize|" + watch1.Leg1.ContDetail.LotSize + "|NetQty|" +
        //                                                                      watch1.Leg1.N_Qty + "|NetAvg|" + watch1.Leg1.N_Price + "|SqPnl|" + watch1.Sqpnl + "|CurrPnl|" + watch1.pnl + "|Type|" + side + "|TradePrice|" + packetHeader.TradePrice.ToString() + "|AvgPrice|" + watch1.NetAvgPrice
        //                                                                      + "|PosInt|" + watch1.posInt + "|BuyValue|" + watch1.Leg1.B_Value + "|SellValue|" + watch1.Leg1.S_Value + "|NetValue|" + watch1.Leg1.A_Value); 
        //                                    #endregion
        //                                    if (side == "UnWind")
        //                                    {
        //                                        #region Wind Trade entry in Log file
        //                                        if (packetHeader.StrategyId == 2211)
        //                                        {
        //                                            TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                            + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
        //                                                                            + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize * watch1.Leg1.Ratio).ToString() + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
        //                                            TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                            + watch1.Expiry2 + "," + watch1.Leg2.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg2.ContractInfo.Series + ","
        //                                                                            + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize * watch1.Leg2.Ratio).ToString() + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
        //                                        }

        //                                        #endregion
        //                                    }
        //                                    else
        //                                    {
        //                                        #region Unwind Trade entry in Log file
        //                                        if (packetHeader.StrategyId == 2211)
        //                                        {
        //                                            TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                            + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
        //                                                                            + (watch1.Leg1.ContDetail.LotSize * watch1.Leg1.Ratio).ToString() + "," + "0" + "," + "0" + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
        //                                            TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                            + watch1.Expiry2 + "," + watch1.Leg2.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg2.ContractInfo.Series + ","
        //                                                                            + (watch1.Leg1.ContDetail.LotSize * watch1.Leg2.Ratio).ToString() + "," + "0" + "," + "0" + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
        //                                        }

        //                                        #endregion
        //                                    }
        //                                }
        //                                #endregion

        //                                AppGlobal.Count_Strangle = AppGlobal.Count_Strangle + 1;
        //                                AppGlobal.TotalTrade = AppGlobal.TotalTrade + 1;
        //                                lblTotalTrade.Text = Convert.ToString(AppGlobal.TotalTrade);
        //                                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
        //                            }
        //                        }
        //                        #endregion
        //                    }                            
        //                    else if (packetHeader.StrategyId == 32211)
        //                    {
        //                        #region TransCode 1 for strategy id 32211
        //                        if (packetHeader.TransCode == 1)
        //                        {
        //                            if (packetHeader.StrategyId == 32211)
        //                            {
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    int i = watch1.RowData.Index;
        //                                    watch1.Wind = Convert.ToDecimal(packetHeader.Wind / 100);
        //                                    watch1.unWind = Convert.ToDecimal(packetHeader.Unwind / 100);

        //                                    watch1.RowData.Cells[WatchConst.Wind].Value = watch1.Wind;
        //                                    watch1.RowData.Cells[WatchConst.UnWind].Value = watch1.unWind;
        //                                }
        //                            }
        //                        }
        //                        #endregion

        //                        #region TransCode 2 for strategy id 32211
        //                        if (packetHeader.TransCode == 2)
        //                        {
        //                            if (packetHeader.StrategyId == 32211)
        //                            {
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    int i = watch1.RowData.Index;
        //                                    dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.White;
        //                                }
        //                            }
        //                        }
        //                        #endregion

        //                        #region TransCode 5 for strategy id 32211
        //                        if (packetHeader.TransCode == 5)
        //                        {
        //                            if (packetHeader.StrategyId == 32211)
        //                            {
        //                                AllInsertTrade(packetHeader);

        //                                #region trade for Watch
        //                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
        //                                {
        //                                    double PrvMargin = 0;
        //                                    int i = watch1.RowData.Index;
        //                                    string side = "";
        //                                    double mtm = 0;
        //                                    if (packetHeader.isWind)
        //                                        dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
        //                                    else
        //                                        dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;

        //                                    if (packetHeader.isWind)
        //                                        side = "Wind";
        //                                    else
        //                                        side = "UnWind";
        //                                    PrvMargin = Convert.ToDouble(watch1.MarginUtilise);
        //                                    mtm = Convert.ToDouble(watch1.premium);
        //                                    if (side == "Wind")
        //                                    {
        //                                        if (watch1.Leg1.MidPrice != 0)
        //                                        {
        //                                            if (watch1.Leg1.ContractInfo.Series == "XX")
        //                                                watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0001 * watch1.Leg1.Ratio);
        //                                            else
        //                                                watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0007 * watch1.Leg1.Ratio);
        //                                        }
        //                                        else
        //                                            watch1.UnwindTrnCost = 0;
        //                                        watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) + watch1.UnwindTrnCost);
        //                                        watch1.Leg1.B_Qty = watch1.Leg1.B_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                        watch1.Leg1.B_Value = watch1.Leg1.B_Value + ((packetHeader.TradePrice + watch1.UnwindTrnCost) * watch1.Leg1.ContDetail.LotSize);
        //                                        watch1.Leg1.Buy_Qty = watch1.Leg1.Buy_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (watch1.Leg1.MidPrice != 0)
        //                                        {
        //                                            if (watch1.Leg1.ContractInfo.Series == "XX")
        //                                                watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0001 * watch1.Leg1.Ratio);
        //                                            else
        //                                                watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0011 * watch1.Leg1.Ratio);
        //                                        }
        //                                        else
        //                                            watch1.WindTrnCost = 0;
        //                                        watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) - watch1.WindTrnCost);
        //                                        watch1.Leg1.S_Qty = watch1.Leg1.S_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                        watch1.Leg1.S_Value = watch1.Leg1.S_Value + ((packetHeader.TradePrice - watch1.WindTrnCost) * watch1.Leg1.ContDetail.LotSize);
        //                                        watch1.Leg1.Sell_Qty = watch1.Leg1.Sell_Qty + watch1.Leg1.ContDetail.LotSize;
        //                                    }
        //                                    watch1.Leg1.N_Qty = (watch1.Leg1.B_Qty - watch1.Leg1.S_Qty);
        //                                    watch1.Leg1.Net_Qty = (watch1.Leg1.Sell_Qty - watch1.Leg1.Buy_Qty);
        //                                    watch1.Leg1.A_Value = (watch1.Leg1.S_Value - watch1.Leg1.B_Value);

        //                                    watch1.ProfitFlg = false;
        //                                    watch1.DrawDownFlg = false;
        //                                    if (watch1.Leg1.N_Qty != 0)
        //                                    {
        //                                        watch1.Leg1.N_Price = Math.Round(watch1.Leg1.A_Value / watch1.Leg1.Net_Qty, 2);

        //                                        watch1.NetAvgPrice = (watch1.Leg1.S_Value - watch1.Leg1.B_Value) / (watch1.Leg1.Net_Qty);
        //                                        watch1.RowData.Cells[WatchConst.AvgPrice].Value = Math.Round(watch1.Leg1.N_Price, 2);
        //                                    }
        //                                    else
        //                                    {
        //                                        watch1.Sqpnl = watch1.Sqpnl + (watch1.Leg1.S_Value - watch1.Leg1.B_Value);
        //                                        AppGlobal.OverAllPnl = AppGlobal.OverAllPnl + (watch1.Leg1.S_Value - watch1.Leg1.B_Value);
        //                                        OverAll_pnl.Text = Math.Round(AppGlobal.OverAllPnl, 2).ToString();
        //                                        watch1.Leg1.N_Price = 0;
        //                                        watch1.avgPrice = 0;
        //                                        watch1.NetAvgPrice = 0;
        //                                        watch1.RowData.Cells[WatchConst.AvgPrice].Value = watch1.Leg1.N_Price;
        //                                        watch1.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch1.Sqpnl, 2);

        //                                        #region Strategy Square off Pnl
        //                                        foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch1.Strategy) &&
        //                                                                                                         (Convert.ToString(x.StrategyId) == "0")))
        //                                        {
        //                                            watch.Sqpnl = watch.Sqpnl + watch1.Sqpnl;
        //                                            watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
        //                                        }
        //                                        #endregion
        //                                    }
        //                                    if (watch1.Leg1.N_Qty > 0)
        //                                    {
        //                                        watch1.PosType = "Wind";
        //                                        watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
        //                                        watch1.posInt = (watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize));
        //                                        watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                    }
        //                                    else if (watch1.Leg1.N_Qty < 0)
        //                                    {
        //                                        watch1.PosType = "UnWind";
        //                                        watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
        //                                        watch1.posInt = (watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize));
        //                                        watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                    }
        //                                    else
        //                                    {
        //                                        watch1.pnl = 0;
        //                                        watch1.RowData.Cells[WatchConst.PNL].Value = watch1.pnl;
        //                                        watch1.RowData.Cells[WatchConst.PosType].Value = "None";
        //                                        watch1.posInt = 0;
        //                                        watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
        //                                        watch1.avgPrice = 0;
        //                                        watch1.Leg1.B_Qty = 0;
        //                                        watch1.Leg1.S_Qty = 0;
        //                                        watch1.Leg1.B_Value = 0;
        //                                        watch1.Leg1.S_Value = 0;
        //                                        watch1.Leg1.N_Qty = 0;
        //                                        watch1.Leg1.N_Price = 0;
        //                                        watch1.Leg1.Buy_Qty = 0;
        //                                        watch1.Leg1.Sell_Qty = 0;
        //                                        watch1.Leg1.Net_Qty = 0;
        //                                    }


        //                                    watch1.premium = watch1.Leg1.N_Price * watch1.posInt * watch1.Leg1.ContDetail.LotSize;
        //                                    watch1.RowData.Cells[WatchConst.Premium].Value = Math.Round(watch1.premium, 2);
        //                                    watch1.TradedQty = watch1.Leg1.ContDetail.LotSize * watch1.posInt;
        //                                    watch1.RowData.Cells[WatchConst.TradedQty].Value = watch1.TradedQty;
        //                                    if (watch1.Leg1.ContractInfo.Series == "CE")
        //                                    {
        //                                        AppGlobal.CallMTM = AppGlobal.CallMTM - mtm + watch1.premium;
        //                                        if (watch1.posInt > 0)
        //                                        {
        //                                            AppGlobal.CallBuyMTM = AppGlobal.CallBuyMTM - mtm + watch1.premium;
        //                                        }
        //                                        else
        //                                        {
        //                                            AppGlobal.CallSellMTM = AppGlobal.CallSellMTM - mtm + watch1.premium;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        AppGlobal.PutMTM = AppGlobal.PutMTM - mtm + watch1.premium;
        //                                        if (watch1.posInt > 0)
        //                                        {
        //                                            AppGlobal.PutBuyMTM = AppGlobal.PutBuyMTM - mtm + watch1.premium;
        //                                        }
        //                                        else
        //                                        {
        //                                            AppGlobal.PutSellMTM = AppGlobal.PutSellMTM - mtm + watch1.premium;
        //                                        }
        //                                    }
        //                                    AppGlobal.overallPremium = (AppGlobal.overallPremium - mtm) + (watch1.premium);
        //                                    if (AppGlobal.overallPremium != 0)
        //                                    {
        //                                        premiumlbl.Text = Convert.ToString(Math.Round(AppGlobal.overallPremium / 10000000, 3));
        //                                    }
        //                                    else
        //                                    {
        //                                        premiumlbl.Text = "0";
        //                                    }

        //                                    #region Margin Calculate
        //                                    if (watch1.posInt != 0)
        //                                    {
        //                                        if (watch1.posInt < 0)
        //                                        {
        //                                            if (watch1.Leg1.ContractInfo.Symbol == "NIFTY")
        //                                                watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.niftyMargin), 2);
        //                                            if (watch1.Leg1.ContractInfo.Symbol == "BANKNIFTY")
        //                                                watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.bankniftyMargin), 2);
        //                                        }

        //                                    }
        //                                    else if (watch1.posInt == 0)
        //                                    {
        //                                        watch1.MarginUtilise = 0;
        //                                    }
        //                                    watch1.RowData.Cells[WatchConst.MarginUtilise].Value = watch1.MarginUtilise;
        //                                    AppGlobal.OverallMarginUtilize = (AppGlobal.OverallMarginUtilize - PrvMargin) + (watch1.MarginUtilise);
        //                                    if (AppGlobal.OverallMarginUtilize != 0)
        //                                    {
        //                                        lblMargin.Text = Convert.ToString(Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3));
        //                                    }
        //                                    else
        //                                    {
        //                                        lblMargin.Text = "0";
        //                                    }

        //                                    #endregion

        //                                    TransactionWatch.ErrorMessage("|UniqueId|" + watch1.uniqueId + "|Strategy|" + watch1.StrategyId + "|symbol|" + watch1.Leg1.ContractInfo.Symbol + "|strike|" + watch1.Leg1.ContractInfo.StrikePrice + "|lotsize|" + watch1.Leg1.ContDetail.LotSize + "|NetQty|" +
        //                                                                      watch1.Leg1.N_Qty + "|NetAvg|" + watch1.Leg1.N_Price + "|SqPnl|" + watch1.Sqpnl + "|CurrPnl|" + watch1.pnl + "|Type|" + side + "|TradePrice|" + packetHeader.TradePrice.ToString() + "|AvgPrice|" + watch1.NetAvgPrice
        //                                                                      + "|PosInt|" + watch1.posInt + "|BuyValue|" + watch1.Leg1.B_Value + "|SellValue|" + watch1.Leg1.S_Value + "|NetValue|" + watch1.Leg1.A_Value); 

        //                                    if (side == "Wind")
        //                                    {
        //                                        TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                      + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
        //                                                                      + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + "0" + "," + "0" + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
        //                                    }
        //                                    else
        //                                    {
        //                                        TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
        //                                                                      + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
        //                                                                      + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
        //                                    }
        //                                }
        //                                #endregion

        //                                AppGlobal.TotalTrade = AppGlobal.TotalTrade + 1;
        //                                lblTotalTrade.Text = Convert.ToString(AppGlobal.TotalTrade);
        //                                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);

        //                                LSL_Strangle_AvgPrice();
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        TransactionWatch.ErrorMessage("Wrong StrategyID");
        //                    }
        //                    lblcallbuy.Text = Math.Round(AppGlobal.CallBuyMTM, 2).ToString();
        //                    lblcallsell.Text = Math.Round(AppGlobal.CallSellMTM, 2).ToString();
        //                    lblputbuy.Text = Math.Round(AppGlobal.PutBuyMTM, 2).ToString();
        //                    lblputsell.Text = Math.Round(AppGlobal.PutSellMTM, 2).ToString();
        //                    CallMTM.Text = (Math.Round(AppGlobal.CallBuyMTM, 2) + Math.Round(AppGlobal.PutBuyMTM, 2)).ToString();
        //                    PutMTM.Text = (Math.Round(AppGlobal.CallSellMTM, 2) + Math.Round(AppGlobal.PutSellMTM, 2)).ToString();
        //                    if (packetHeader.TransCode == 5)
        //                    {
        //                        SendToTradeAdmin("Trade");
        //                    }
        //                    if (packetHeader.TransCode == 5)
        //                        FlashApplicationWindow("Straddle");
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            TransactionWatch.ErrorMessage("Error found in Trade");
        //        }
        //    }
        //}

        void heartBeatCheck_Tick(object sender, EventArgs e)
        { 
            if (AppGlobal.currentHeartBeat != AppGlobal.PreviousHeartBeat)
            {
                AppGlobal.PreviousHeartBeat = AppGlobal.currentHeartBeat;
            }
            else
            {
                if (AppGlobal.HeartbeatCount != 0)
                {
                    MessageBox.Show("HeartBeat not received frm last 3 Sec. Please check ur connection");
                }
            }
            AppGlobal.HeartbeatCount++;
        }

        void RunningPnl_Tick(object sender, EventArgs e)
        {
            SendToTradeAdmin("Timer");
        }

        void SendToTradeAdmin(string Message)
        {
            AppGlobal.Admin_Delta = AppGlobal.MarketWatch.Select(x => x.sumDelta).Sum();
            AppGlobal.Admin_Vega = AppGlobal.MarketWatch.Select(x => x.sumVega).Sum();
            AppGlobal.Admin_Theta = AppGlobal.MarketWatch.Select(x => x.sumTheta).Sum();
            AppGlobal.Admin_Gamma = AppGlobal.MarketWatch.Select(x => x.sumGamma).Sum();

            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 50;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.OverNightWindPos = Convert.ToInt32(AppGlobal.Pnl);
            newStrike.OverNightUnWindPos = Convert.ToInt32(AppGlobal.OverAllPnl);
            newStrike.Wind = Convert.ToDouble(Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3));
            newStrike.Unwind = Convert.ToDouble(Math.Round(AppGlobal.Admin_Delta, 2));
            newStrike.Netting = Convert.ToDouble(Math.Round(AppGlobal.Admin_Vega, 2));
            newStrike.AvgSpread = Convert.ToDouble(Math.Round(AppGlobal.Admin_Theta, 2));
            newStrike.TransactionCost = Convert.ToDouble(Math.Round(AppGlobal.Admin_Gamma, 4));

            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
            TransactionWatch.ErrorMessage(Message + "|Gui_Id|" + AppGlobal.GUI_ID + "|SqPnl|" + newStrike.OverNightUnWindPos + "|CurrentPnl|" + newStrike.OverNightWindPos
                                                  + "|Margin|" + newStrike.Wind + "|Delta|" + newStrike.Unwind + "|Vega|" + newStrike.Netting + "|Theta|" + newStrike.AvgSpread
                                                  + "|Gamma|" + newStrike.TransactionCost + "|SpotPrice|" + AppGlobal.SpotNifty);
            TransactionWatch.TransactionMessage(Message + "|Gui_Id|" + AppGlobal.GUI_ID + "|SqPnl|" + newStrike.OverNightUnWindPos + "|CurrentPnl|" + newStrike.OverNightWindPos
                                                  + "|Margin|" + newStrike.Wind + "|Delta|" + newStrike.Unwind + "|Vega|" + newStrike.Netting + "|Theta|" + newStrike.AvgSpread
                                                  + "|Gamma|" + newStrike.TransactionCost + "|SpotPrice|" + AppGlobal.SpotNifty, Color.Red);



        }

        public void readFileExcelScripCount(string file)
        {
            const char fieldSeparator = ',';
            using (StreamReader readFile = new StreamReader(file))
            {
                string line;
                int i = 0;
                while ((line = readFile.ReadLine()) != null)
                {
                    List<string> split = line.Split(fieldSeparator).ToList();
                    if (i == 2)
                        break;
                    foreach (string ln in split)
                    {
                        if (Convert.ToString(split[0].Trim()) == "Single")
                            continue;
                        else
                        {
                            AppGlobal.Count_single = Convert.ToInt32(split[0]);
                            AppGlobal.Count_Ratio = Convert.ToInt32(split[1]);
                            AppGlobal.Count_Strangle = Convert.ToInt32(split[2]);
                            AppGlobal.Count_Ladder = Convert.ToInt32(split[2]);
                        }
                    }
                    i++;
                }
            }
        }

        #region NetPosition Comment Code
        /*public static void insertTradeWatch11(BTPacket.GUIUpdate Trade)
        {
            if (AppGlobal.frmWatch != null && AppGlobal.frmWatch.InvokeRequired)
            {
                AppGlobal.frmWatch.BeginInvoke((MethodInvoker)(() => insertTradeWatch11(Trade)));
            }
            else
            {
                try
                {
                    NetPositionWatch NetPosWatch = new NetPositionWatch();
                    int exflg = 0;
                    int mktIndex = 0;
                    if (AppGlobal.NetMarketWatch != null)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                exflg = 1;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();
                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();
                        string side = "";
                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }

                        if (side == "Wind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }
                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Unwind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.windAvg = 0;
                            NetPosWatch.unwindAvg = 0;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {
                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;


                            if (watchT.Leg1.Counter == 1)
                            {
                                NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike1 = 0;
                            }
                            if (watchT.Leg2.Counter == 1)
                            {
                                NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike2 = 0;
                            }
                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = NetPosWatch.Strike1;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = NetPosWatch.Strike2;
                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = NetPosWatch.Series;
                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.Expiry2 = watchT.Expiry2;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = watchT.StrategyName;
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = NetPosWatch.StrategyName;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);
                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    if (exflg == 1)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                string side = "";

                                if (Trade.isWind)
                                {
                                    side = "Wind";
                                }
                                else
                                {
                                    side = "Unwind";
                                }

                                if (side == "Wind")
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                                    NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.B_Qty != 0)
                                    {
                                        NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                                    }
                                }
                                else
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                                    NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.S_Qty != 0)
                                    {
                                        NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                                    }
                                }
                                NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                                if (NetPosWatch.Leg.net_Qty != 0)
                                {
                                    NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);

                                }
                                else
                                {
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.avgPrice = 0;

                                }

                                if (NetPosWatch.Leg.net_Qty > 0)
                                {
                                    NetPosWatch.posType = "Wind";

                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);

                                }
                                else if (NetPosWatch.Leg.net_Qty < 0)
                                {
                                    NetPosWatch.posType = "Unwind";
                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                                }
                                else
                                {
                                    NetPosWatch.posType = "None";
                                    NetPosWatch.posInt = 0;
                                    NetPosWatch.avgPrice = 0;
                                    NetPosWatch.Leg.B_Qty = 0;
                                    NetPosWatch.Leg.S_Qty = 0;
                                    NetPosWatch.Leg.B_Value = 0;
                                    NetPosWatch.Leg.S_Value = 0;
                                    NetPosWatch.Leg.net_Qty = 0;
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.windAvg = 0;
                                    NetPosWatch.unwindAvg = 0;
                                }

                                NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                                NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                                NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                                NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                                NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                                NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                                NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();

                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();

                        string side = "";

                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }
                        if (side == "Wind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }

                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";

                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);

                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Unwind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.B_Value = 0;
                            NetPosWatch.Leg.S_Value = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.windAvg = 0;
                            NetPosWatch.unwindAvg = 0;
                        }

                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {

                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;

                            if (watchT.Leg1.Counter == 1)
                            {
                                NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike1 = 0;
                            }
                            if (watchT.Leg2.Counter == 1)
                            {
                                NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike2 = 0;
                            }

                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = NetPosWatch.Strike1;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = NetPosWatch.Strike2;


                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = NetPosWatch.Series;

                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.Expiry2 = watchT.Expiry2;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = watchT.StrategyName;
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = NetPosWatch.StrategyName;
                        }

                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);

                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    NetPositionWatch.WriteXmlProfile(ref AppGlobal.NetMarketWatch);

                    if (AppGlobal._NetMax_Min != null)
                    {
                        AppGlobal._NetMax_Min.LoadEvent();
                    }

                }
                catch (Exception)
                { }
            }
        }

        public static void insertTradeWatch2211(BTPacket.GUIUpdate Trade)
        {
            if (AppGlobal.frmWatch != null && AppGlobal.frmWatch.InvokeRequired)
            {
                AppGlobal.frmWatch.BeginInvoke((MethodInvoker)(() => insertTradeWatch2211(Trade)));
            }
            else
            {
                try
                {
                    NetPositionWatch NetPosWatch = new NetPositionWatch();
                    int exflg = 0;
                    int mktIndex = 0;
                    if (AppGlobal.NetMarketWatch != null)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                exflg = 1;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();
                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();
                        string side = "";
                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }

                        if (side == "Unwind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }
                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20) * -1;
                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Unwind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20) * -1;
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.windAvg = 0;
                            NetPosWatch.unwindAvg = 0;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {
                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;


                            if (watchT.Leg1.Counter == 1)
                            {
                                NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike1 = 0;
                            }
                            if (watchT.Leg2.Counter == 1)
                            {
                                NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike2 = 0;
                            }
                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = NetPosWatch.Strike1;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = NetPosWatch.Strike2;
                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = NetPosWatch.Series;
                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.Expiry2 = watchT.Expiry2;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = watchT.StrategyName;
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = NetPosWatch.StrategyName;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);
                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    if (exflg == 1)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                string side = "";

                                if (Trade.isWind)
                                {
                                    side = "Wind";
                                }
                                else
                                {
                                    side = "Unwind";
                                }

                                if (side == "Unwind")
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                                    NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.B_Qty != 0)
                                    {
                                        NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                                    }
                                }
                                else
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                                    NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.S_Qty != 0)
                                    {
                                        NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                                    }
                                }
                                NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                                if (NetPosWatch.Leg.net_Qty != 0)
                                {
                                    NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);

                                }
                                else
                                {
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.avgPrice = 0;

                                }

                                if (NetPosWatch.Leg.net_Qty > 0)
                                {
                                    NetPosWatch.posType = "Wind";

                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20) * -1;

                                }
                                else if (NetPosWatch.Leg.net_Qty < 0)
                                {
                                    NetPosWatch.posType = "Unwind";
                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20) * -1;
                                }
                                else
                                {
                                    NetPosWatch.posType = "None";
                                    NetPosWatch.posInt = 0;
                                    NetPosWatch.avgPrice = 0;
                                    NetPosWatch.Leg.B_Qty = 0;
                                    NetPosWatch.Leg.S_Qty = 0;
                                    NetPosWatch.Leg.B_Value = 0;
                                    NetPosWatch.Leg.S_Value = 0;
                                    NetPosWatch.Leg.net_Qty = 0;
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.windAvg = 0;
                                    NetPosWatch.unwindAvg = 0;
                                }

                                NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                                NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                                NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                                NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                                NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                                NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                                NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();

                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();

                        string side = "";

                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }
                        if (side == "Unwind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }

                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";

                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20) * -1;

                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Unwind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20) * -1;
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.B_Value = 0;
                            NetPosWatch.Leg.S_Value = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.windAvg = 0;
                            NetPosWatch.unwindAvg = 0;
                        }

                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {

                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;

                            if (watchT.Leg1.Counter == 1)
                            {
                                NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike1 = 0;
                            }
                            if (watchT.Leg2.Counter == 1)
                            {
                                NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike2 = 0;
                            }

                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = NetPosWatch.Strike1;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = NetPosWatch.Strike2;


                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = NetPosWatch.Series;

                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.Expiry2 = watchT.Expiry2;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = watchT.StrategyName;
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = NetPosWatch.StrategyName;
                        }

                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);

                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    NetPositionWatch.WriteXmlProfile(ref AppGlobal.NetMarketWatch);

                    if (AppGlobal._NetMax_Min != null)
                    {
                        AppGlobal._NetMax_Min.LoadEvent();
                    }

                }
                catch (Exception)
                { }
            }
        }

        public static void insertTradeWatch888(BTPacket.GUIUpdate Trade)
        {
            if (AppGlobal.frmWatch != null && AppGlobal.frmWatch.InvokeRequired)
            {
                AppGlobal.frmWatch.BeginInvoke((MethodInvoker)(() => insertTradeWatch888(Trade)));
            }
            else
            {
                try
                {
                    NetPositionWatch NetPosWatch = new NetPositionWatch();
                    int exflg = 0;
                    int mktIndex = 0;
                    if (AppGlobal.NetMarketWatch != null)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                exflg = 1;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();
                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();
                        string side = "";
                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }

                        if (side == "Wind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }
                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Unwind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.windAvg = 0;
                            NetPosWatch.unwindAvg = 0;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {
                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;


                            if (watchT.Leg1.Counter == 1)
                            {
                                NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike1 = 0;
                            }
                            if (watchT.Leg2.Counter == 1)
                            {
                                NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike2 = 0;
                            }
                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = NetPosWatch.Strike1;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = NetPosWatch.Strike2;
                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = NetPosWatch.Series;
                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.Expiry2 = watchT.Expiry2;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = watchT.StrategyName;
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = NetPosWatch.StrategyName;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);
                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    if (exflg == 1)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                string side = "";

                                if (Trade.isWind)
                                {
                                    side = "Wind";
                                }
                                else
                                {
                                    side = "Unwind";
                                }

                                if (side == "Wind")
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                                    NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.B_Qty != 0)
                                    {
                                        NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                                    }
                                }
                                else
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                                    NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.S_Qty != 0)
                                    {
                                        NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                                    }
                                }
                                NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                                if (NetPosWatch.Leg.net_Qty != 0)
                                {
                                    NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);

                                }
                                else
                                {
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.avgPrice = 0;

                                }

                                if (NetPosWatch.Leg.net_Qty > 0)
                                {
                                    NetPosWatch.posType = "Wind";

                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);

                                }
                                else if (NetPosWatch.Leg.net_Qty < 0)
                                {
                                    NetPosWatch.posType = "Unwind";
                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                                }
                                else
                                {
                                    NetPosWatch.posType = "None";
                                    NetPosWatch.posInt = 0;
                                    NetPosWatch.avgPrice = 0;
                                    NetPosWatch.Leg.B_Qty = 0;
                                    NetPosWatch.Leg.S_Qty = 0;
                                    NetPosWatch.Leg.B_Value = 0;
                                    NetPosWatch.Leg.S_Value = 0;
                                    NetPosWatch.Leg.net_Qty = 0;
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.windAvg = 0;
                                    NetPosWatch.unwindAvg = 0;
                                }

                                NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                                NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                                NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                                NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                                NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                                NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                                NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();

                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();

                        string side = "";

                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }
                        if (side == "Wind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }

                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";

                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);

                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Unwind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.B_Value = 0;
                            NetPosWatch.Leg.S_Value = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.windAvg = 0;
                            NetPosWatch.unwindAvg = 0;
                        }

                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {

                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;

                            if (watchT.Leg1.Counter == 1)
                            {
                                NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike1 = 0;
                            }
                            if (watchT.Leg2.Counter == 1)
                            {
                                NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike2 = 0;
                            }
                            if (watchT.Leg3.Counter == 1)
                            {
                                NetPosWatch.Strike3 = Convert.ToInt32(watchT.Leg3.ContractInfo.StrikePrice);
                            }
                            else
                                NetPosWatch.Strike3 = 0;



                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = NetPosWatch.Strike1;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = NetPosWatch.Strike2;


                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = NetPosWatch.Series;

                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.Expiry2 = watchT.Expiry2;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = watchT.StrategyName;
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = NetPosWatch.StrategyName;
                        }

                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);

                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    NetPositionWatch.WriteXmlProfile(ref AppGlobal.NetMarketWatch);

                    if (AppGlobal._NetMax_Min != null)
                    {
                        AppGlobal._NetMax_Min.LoadEvent();
                    }

                }
                catch (Exception)
                { }
            }
        }

        public static void insertTradeWatch7121(BTPacket.GUIUpdate Trade)
        {
            if (AppGlobal.frmWatch != null && AppGlobal.frmWatch.InvokeRequired)
            {
                AppGlobal.frmWatch.BeginInvoke((MethodInvoker)(() => insertTradeWatch7121(Trade)));
            }
            else
            {
                try
                {
                    NetPositionWatch NetPosWatch = new NetPositionWatch();
                    int exflg = 0;
                    int mktIndex = 0;
                    if (AppGlobal.NetMarketWatch != null)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                exflg = 1;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();
                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();
                        string side = "";
                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }

                        if (side == "Wind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }
                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Unwind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.windAvg = 0;
                            NetPosWatch.unwindAvg = 0;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {
                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;


                            if (watchT.Leg1.Counter == 1)
                            {
                                NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike1 = 0;
                            }
                            if (watchT.Leg2.Counter == 1)
                            {
                                NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike2 = 0;
                            }
                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = NetPosWatch.Strike1;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = NetPosWatch.Strike2;
                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = NetPosWatch.Series;
                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.Expiry2 = watchT.Expiry2;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = watchT.StrategyName;
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = NetPosWatch.StrategyName;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);
                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    if (exflg == 1)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                string side = "";

                                if (Trade.isWind)
                                {
                                    side = "Wind";
                                }
                                else
                                {
                                    side = "Unwind";
                                }

                                if (side == "Wind")
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                                    NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.B_Qty != 0)
                                    {
                                        NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                                    }
                                }
                                else
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                                    NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.S_Qty != 0)
                                    {
                                        NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                                    }
                                }
                                NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                                if (NetPosWatch.Leg.net_Qty != 0)
                                {
                                    NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);

                                }
                                else
                                {
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.avgPrice = 0;

                                }

                                if (NetPosWatch.Leg.net_Qty > 0)
                                {
                                    NetPosWatch.posType = "Wind";

                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);

                                }
                                else if (NetPosWatch.Leg.net_Qty < 0)
                                {
                                    NetPosWatch.posType = "Unwind";
                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                                }
                                else
                                {
                                    NetPosWatch.posType = "None";
                                    NetPosWatch.posInt = 0;
                                    NetPosWatch.avgPrice = 0;
                                    NetPosWatch.Leg.B_Qty = 0;
                                    NetPosWatch.Leg.S_Qty = 0;
                                    NetPosWatch.Leg.B_Value = 0;
                                    NetPosWatch.Leg.S_Value = 0;
                                    NetPosWatch.Leg.net_Qty = 0;
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.windAvg = 0;
                                    NetPosWatch.unwindAvg = 0;
                                }

                                NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                                NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                                NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                                NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                                NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                                NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                                NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();

                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();

                        string side = "";

                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }
                        if (side == "Wind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }

                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";

                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);

                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Unwind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.B_Value = 0;
                            NetPosWatch.Leg.S_Value = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.windAvg = 0;
                            NetPosWatch.unwindAvg = 0;
                        }

                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {

                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;

                            if (watchT.Leg1.Counter == 1)
                            {
                                NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike1 = 0;
                            }
                            if (watchT.Leg2.Counter == 1)
                            {
                                NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            }
                            else
                            {
                                NetPosWatch.Strike2 = 0;
                            }
                            if (watchT.Leg3.Counter == 1)
                            {
                                NetPosWatch.Strike3 = Convert.ToInt32(watchT.Leg3.ContractInfo.StrikePrice);
                            }
                            else
                                NetPosWatch.Strike3 = 0;



                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = NetPosWatch.Strike1;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = NetPosWatch.Strike2;


                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = NetPosWatch.Series;

                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.Expiry2 = watchT.Expiry2;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = watchT.StrategyName;
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = NetPosWatch.StrategyName;
                        }

                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);

                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    NetPositionWatch.WriteXmlProfile(ref AppGlobal.NetMarketWatch);

                    if (AppGlobal._NetMax_Min != null)
                    {
                        AppGlobal._NetMax_Min.LoadEvent();
                    }

                }
                catch (Exception)
                { }
            }
        }

        public static void insertTradeWatch91(BTPacket.GUIUpdate Trade)
        {
            if (AppGlobal.frmWatch != null && AppGlobal.frmWatch.InvokeRequired)
            {
                AppGlobal.frmWatch.BeginInvoke((MethodInvoker)(() => insertTradeWatch91(Trade)));
            }
            else
            {
                try
                {
                    NetPositionWatch NetPosWatch = new NetPositionWatch();
                    int exflg = 0;
                    int mktIndex = 0;
                    if (AppGlobal.NetMarketWatch != null)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                exflg = 1;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();
                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {
                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = watchT.Leg1.ContractInfo.StrikePrice;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = watchT.Leg2.ContractInfo.StrikePrice;
                            NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = "91";
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = "91";
                        }
                        string side = "";
                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }
                        if (side == "Wind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;

                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }

                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "Wind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "UnWind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.Leg.B_Value = 0;
                            NetPosWatch.Leg.S_Value = 0;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);
                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    if (exflg == 1)
                    {
                        for (int i = 0; i < AppGlobal.NetMarketWatch.Count; i++)
                        {
                            NetPosWatch = AppGlobal.NetMarketWatch[i];
                            NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[i];
                            if (AppGlobal.NetMarketWatch[i].Leg.uniqueId == Trade.UniqueID)
                            {
                                string side = "";
                                if (Trade.isWind)
                                {
                                    side = "Wind";
                                }
                                else
                                {
                                    side = "Unwind";
                                }
                                if (side == "Wind")
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                                    NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.B_Qty != 0)
                                    {
                                        NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                                    }
                                }
                                else
                                {
                                    NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                                    NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                                    NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                                    if (NetPosWatch.Leg.S_Qty != 0)
                                    {
                                        NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                                    }
                                }
                                NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                                if (NetPosWatch.Leg.net_Qty != 0)
                                {
                                    NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                                }
                                else
                                {
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.avgPrice = 0;
                                }
                                if (NetPosWatch.Leg.net_Qty > 0)
                                {
                                    NetPosWatch.posType = "Wind";
                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                                }
                                else if (NetPosWatch.Leg.net_Qty < 0)
                                {
                                    NetPosWatch.posType = "UnWind";
                                    NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                                }
                                else
                                {
                                    NetPosWatch.posType = "None";
                                    NetPosWatch.posInt = 0;
                                    NetPosWatch.avgPrice = 0;
                                    NetPosWatch.Leg.B_Qty = 0;
                                    NetPosWatch.Leg.S_Qty = 0;
                                    NetPosWatch.Leg.net_Qty = 0;
                                    NetPosWatch.Leg.N_Price = 0;
                                    NetPosWatch.Leg.B_Value = 0;
                                    NetPosWatch.Leg.S_Value = 0;
                                }
                                NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                                NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                                NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                                NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                                NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                                NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                                NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                            }
                        }
                    }
                    else
                    {
                        NetPosWatch = new NetPositionWatch();
                        mktIndex = AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1;
                        NetPosWatch.RowData = AppGlobal.frmWatch.mtDataGridView1.Rows[mktIndex];
                        NetPosWatch.Leg = new Legx();
                        NetPosWatch.Leg.uniqueId = Trade.UniqueID;
                        NetPosWatch.Leg.displayUniqueId = NetPosWatch.Leg.uniqueId.ToString();
                        foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(NetPosWatch.Leg.displayUniqueId))))
                        {
                            NetPosWatch.Symbol = watchT.Leg1.ContractInfo.Symbol;
                            NetPosWatch.Token1 = watchT.Leg1.ContractInfo.TokenNo;
                            NetPosWatch.Token2 = watchT.Leg2.ContractInfo.TokenNo;
                            NetPosWatch.RowData.Cells[TradeConst.L1Ser].Value = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.RowData.Cells[TradeConst.L1Stk].Value = watchT.Leg1.ContractInfo.StrikePrice;
                            NetPosWatch.RowData.Cells[TradeConst.L2Stk].Value = watchT.Leg2.ContractInfo.StrikePrice;
                            NetPosWatch.Strike1 = Convert.ToInt32(watchT.Leg1.ContractInfo.StrikePrice);
                            NetPosWatch.Strike2 = Convert.ToInt32(watchT.Leg2.ContractInfo.StrikePrice);
                            NetPosWatch.Series = watchT.Leg1.ContractInfo.Series;
                            NetPosWatch.Expiry = watchT.Expiry;
                            NetPosWatch.RowData.Cells[TradeConst.Expiry].Value = NetPosWatch.Expiry;
                            NetPosWatch.StrategyName = "91";
                            NetPosWatch.RowData.Cells[TradeConst.StrategyName].Value = "91";
                        }

                        string side = "";
                        if (Trade.isWind)
                        {
                            side = "Wind";
                        }
                        else
                        {
                            side = "Unwind";
                        }
                        if (side == "Wind")
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.B_Qty = NetPosWatch.Leg.B_Qty + 20;
                            NetPosWatch.Leg.B_Value = NetPosWatch.Leg.B_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.B_Qty != 0)
                            {
                                NetPosWatch.unwindAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.B_Value / NetPosWatch.Leg.B_Qty), 2);
                            }
                        }
                        else
                        {
                            NetPosWatch.avgPrice = NetPosWatch.avgPrice + Convert.ToDouble(Trade.TradePrice);
                            NetPosWatch.Leg.S_Qty = NetPosWatch.Leg.S_Qty + 20;
                            NetPosWatch.Leg.S_Value = NetPosWatch.Leg.S_Value + Convert.ToDouble(Trade.TradePrice) * 20;
                            if (NetPosWatch.Leg.S_Qty != 0)
                            {
                                NetPosWatch.windAvg = Math.Round(Convert.ToDouble(NetPosWatch.Leg.S_Value / NetPosWatch.Leg.S_Qty), 2);
                            }
                        }
                        NetPosWatch.Leg.net_Qty = NetPosWatch.Leg.B_Qty - NetPosWatch.Leg.S_Qty;
                        if (NetPosWatch.Leg.net_Qty != 0)
                        {
                            NetPosWatch.Leg.N_Price = Math.Round((NetPosWatch.avgPrice / (Math.Abs(NetPosWatch.Leg.net_Qty) / 20)), 2);
                        }
                        else
                        {
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.avgPrice = 0;
                        }
                        if (NetPosWatch.Leg.net_Qty > 0)
                        {
                            NetPosWatch.posType = "UnWind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else if (NetPosWatch.Leg.net_Qty < 0)
                        {
                            NetPosWatch.posType = "Wind";
                            NetPosWatch.posInt = ((NetPosWatch.Leg.net_Qty) / 20);
                        }
                        else
                        {
                            NetPosWatch.posType = "None";
                            NetPosWatch.posInt = 0;
                            NetPosWatch.avgPrice = 0;
                            NetPosWatch.Leg.B_Qty = 0;
                            NetPosWatch.Leg.S_Qty = 0;
                            NetPosWatch.Leg.net_Qty = 0;
                            NetPosWatch.Leg.N_Price = 0;
                            NetPosWatch.Leg.B_Value = 0;
                            NetPosWatch.Leg.S_Value = 0;
                        }
                        NetPosWatch.RowData.Cells[TradeConst.Symbol].Value = NetPosWatch.Symbol;
                        NetPosWatch.RowData.Cells[TradeConst.uniqueId].Value = NetPosWatch.Leg.displayUniqueId;
                        NetPosWatch.RowData.Cells[TradeConst.AvgPrice].Value = NetPosWatch.Leg.N_Price;
                        NetPosWatch.RowData.Cells[TradeConst.posInt].Value = NetPosWatch.posInt;
                        NetPosWatch.RowData.Cells[TradeConst.posType].Value = NetPosWatch.posType;
                        NetPosWatch.RowData.Cells[TradeConst.windAvg].Value = NetPosWatch.windAvg;
                        NetPosWatch.RowData.Cells[TradeConst.unwindAvg].Value = NetPosWatch.unwindAvg;
                        if (mktIndex == AppGlobal.frmWatch.mtDataGridView1.Rows.Count - 1)
                            AppGlobal.frmWatch.mtDataGridView1.Rows.Add();
                        else
                            AppGlobal.NetMarketWatch.RemoveAt(mktIndex);
                        AppGlobal.NetMarketWatch.Insert(mktIndex, NetPosWatch);
                    }
                    NetPositionWatch.WriteXmlProfile(ref AppGlobal.NetMarketWatch);
                }
                catch (Exception)
                { }
            }
        }*/
        #endregion

        public void AllInsertTrade(BTPacket.GUIUpdate Trade)
        {
            if (AppGlobal.frmWatch != null && AppGlobal.frmWatch.InvokeRequired)
            {
                AppGlobal.frmWatch.BeginInvoke((MethodInvoker)(() => AllInsertTrade(Trade)));
            }
            else
            {
                TransactionWatch.ErrorMessage("AllInsertTrade insert");
                DataRow row = _tradeBookTable1.NewRow();
                string displayunique = Trade.UniqueID.ToString();
                row[TradeConst.Time] = DateTime.Now.ToString("HH:mm:ss:ffff");
                row[TradeConst.uniqueId] = displayunique;
                row[TradeConst.TrdPrice] = Trade.TradePrice;
                string isWind = "";
                if (Trade.isWind)
                {
                    isWind = "Wind";
                    row[TradeConst.IsWind] = "Wind";
                }
                else
                {
                    isWind = "UnWind";
                    row[TradeConst.IsWind] = "UnWind";
                }
                foreach (var watchT in AppGlobal.MarketWatch.Where(x => (x.uniqueId == Convert.ToUInt64(Trade.UniqueID))))
                {

                    row[TradeConst.L1Stk] = watchT.Leg1.ContractInfo.StrikePrice;
                    if (watchT.StrategyId != 91)
                        row[TradeConst.L2Stk] = watchT.Leg2.ContractInfo.StrikePrice;

                    row[TradeConst.L1Ser] = watchT.Leg1.ContractInfo.Series;

                    if (Trade.isWind)
                        row[TradeConst.UserRate] = Convert.ToString(watchT.RowData.Cells[WatchConst.Wind].Value);
                    else
                        row[TradeConst.UserRate] = Convert.ToString(watchT.RowData.Cells[WatchConst.UnWind].Value);
                    row[TradeConst.Expiry] = watchT.Expiry;

                    if (watchT.StrategyId == 3434)
                        row[TradeConst.StrategyName] = "Box";
                    else if (watchT.StrategyId == 23434)
                        row[TradeConst.StrategyName] = "2Legs_Diagonal";
                    else if (watchT.StrategyId == 33434)
                        row[TradeConst.StrategyName] = "3Legs_Diagonal";
                    else if (watchT.StrategyId == 43434)
                        row[TradeConst.StrategyName] = watchT.StrategyName;
                    else if (watchT.StrategyId == 111 || watchT.StrategyId == 211 || watchT.StrategyId == 311)
                        row[TradeConst.StrategyName] = watchT.StrategyName;
                    else if (watchT.StrategyId == 2211)
                        row[TradeConst.StrategyName] = watchT.StrategyName;
                    else if (watchT.StrategyId == 888)
                        row[TradeConst.StrategyName] = watchT.StrategyName;
                    else if (watchT.StrategyId == 91)
                        row[TradeConst.StrategyName] = watchT.StrategyName;
                    if (!AppGlobal.RuleTradeCount.ContainsKey(Convert.ToUInt64(Trade.UniqueID)))
                    {
                        AppGlobal.RuleTradeCount.Add(Convert.ToUInt64(Trade.UniqueID), 1);
                    }
                    else
                    {
                        int count = AppGlobal.RuleTradeCount[Trade.UniqueID];
                        AppGlobal.RuleTradeCount[Trade.UniqueID] = count + 1;
                    }
                    row[TradeConst.TrdCount] = AppGlobal.RuleTradeCount[Trade.UniqueID];
                }
                _tradeBookTable1.Rows.InsertAt(row, 0);
                tradeBookDataGrid1.Rows[0].Selected = true;
              
            }
        }

        public static decimal GetValueAsTickMultiple(decimal value, Straddle.AppClasses.Leg Leg1)
        {
            if (Leg1.ContractInfo.PriceDivisor != MTConstant.PriceDivisor100)
                return Math.Round(Math.Round(value / Leg1.ContDetail.PriceTick) * Leg1.ContDetail.PriceTick, 4);
            return Math.Round(Math.Round(value / Leg1.ContDetail.PriceTick) * Leg1.ContDetail.PriceTick, 2);
        }

        internal static void FlashOtherWindow(IntPtr windowHandle)
        {
            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.dwFlags = 2;
            fInfo.dwTimeout = 2;
            fInfo.hwnd = windowHandle;
            fInfo.uCount = 3;

            FlashWindowEx(ref fInfo);
        }

        internal static void FlashApplicationWindow(string application)
        {
            Process[] processCollection = Process.GetProcesses();
            foreach (Process p in processCollection.Where(x => x.ProcessName == application))
            {
                FlashOtherWindow(p.MainWindowHandle);
                Console.WriteLine(p.ProcessName);
            }
        }

        void CreateTable()
        {
            _tradeBookTable1.Columns.Add(TradeConst.Time);
            _tradeBookTable1.Columns.Add(TradeConst.uniqueId);
            _tradeBookTable1.Columns.Add(TradeConst.Expiry);
            _tradeBookTable1.Columns.Add(TradeConst.StrategyName);
            _tradeBookTable1.Columns.Add(TradeConst.UserRate);
            _tradeBookTable1.Columns.Add(TradeConst.TrdPrice);
            _tradeBookTable1.Columns.Add(TradeConst.L1Stk);
            _tradeBookTable1.Columns.Add(TradeConst.L2Stk);
            _tradeBookTable1.Columns.Add(TradeConst.L1Ser);
            _tradeBookTable1.Columns.Add(TradeConst.IsWind);
            _tradeBookTable1.Columns.Add(TradeConst.TrdCount);
        }

        void connection_MKTClientDisconnect(Socket socket)
        {

        }

        void connection_MKTClientConnect(Socket socket)
        {

        }

        void connection_MKTMessageRecived(Socket socket, byte[] message)
        {
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)(() => connection_MKTMessageRecived(socket, message)));
            else
            {
                try
                {
                    UInt64 TransCode = BitConverter.ToUInt64(message, 0);
                    if (TransCode == 3)
                    {
                        #region marketWatch

                        double buyPrice = BitConverter.ToDouble(message, 16);
                        double AskPrice = BitConverter.ToDouble(message, 24);
                        double Ltp = BitConverter.ToDouble(message, 32);
                        double Volumn = BitConverter.ToDouble(message, 40);
                        UInt64 uni = BitConverter.ToUInt64(message, 48);
                        UInt64 seq = BitConverter.ToUInt64(message, 8);
                        //AppGlobal.Token = Convert.ToUInt64(txtToken.Text);
                        if (AppGlobal.NiftyToken == uni)
                            txtNiftyValue.Text = (Ltp / 100).ToString();

                        if (AppGlobal.BKToken == uni)
                            txtbankValue.Text = (Ltp / 100).ToString();

                        if (!AppGlobal.MapList.ContainsKey(uni))
                        {
                            return;
                        }

                        #region Leg1

                        foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.Leg1.ContractInfo.TokenNo) == uni)))
                        {
                            int i = watch.RowData.Index;
                            if (watch.Leg1.BuyPrice != 0)
                                watch.Leg1.OldBuyPrice = watch.Leg1.BuyPrice;
                            if (watch.Leg1.SellPrice != 0)
                                watch.Leg1.OldSellPrice = watch.Leg1.SellPrice;
                            watch.Leg1.BuyPrice = buyPrice / 100;
                            watch.Leg1.SellPrice = AskPrice / 100;
                            watch.Leg1.Sequence = seq;
                            watch.Leg1.LastTradedPrice = Convert.ToDecimal(Ltp) / 100;
                            watch.Leg1.MidPrice = Math.Round(Convert.ToDouble((watch.Leg1.BuyPrice + watch.Leg1.SellPrice) / 2), 2);
                            watch.RowData.Cells[WatchConst.L1buyPrice].Value = watch.Leg1.BuyPrice;
                            watch.RowData.Cells[WatchConst.L1sellPrice].Value = watch.Leg1.SellPrice;

                            if (AppGlobal.Record)
                            {
                                if (watch.uniqueId == AppGlobal.RuleRecord)
                                {
                                    TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg1.BuyPrice + "|SellPrice|" + watch.Leg1.SellPrice + "|Sequence|" + seq);
                                }
                            }
                            CalculateGreek(watch);
                            if (watch.StrategyId == 111 || watch.StrategyId == 211 || watch.StrategyId == 311)
                            {
                                CalculateSpread(watch);
                                CalculateSpreadRatio11_12(watch);
                            }
                            else if (watch.StrategyId == 91)
                            {
                                CalculateSpreadSingle(watch);
                            }
                            else if (watch.StrategyId == 2211)
                                CalculateStrangleSpread(watch);
                            else if (watch.StrategyId == 888)
                                CalculateLadderSpread(watch);

                        }
                        #endregion

                        #region FUTURE

                        foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.niftyLeg.ContractInfo.TokenNo) == uni)))
                        {
                            int i = watch.RowData.Index;
                            if (watch.niftyLeg.BuyPrice != 0)
                                watch.niftyLeg.OldBuyPrice = watch.niftyLeg.BuyPrice;
                            if (watch.niftyLeg.SellPrice != 0)
                                watch.niftyLeg.OldSellPrice = watch.niftyLeg.SellPrice;

                            watch.niftyLeg.BuyPrice = buyPrice / 100;
                            watch.niftyLeg.SellPrice = AskPrice / 100;
                            watch.niftyLeg.LastTradedPrice = Convert.ToDecimal(Ltp) / 100;
                            watch.RowData.Cells[WatchConst.FLTP].Value = watch.niftyLeg.LastTradedPrice;
                            CalculateGreek(watch);
                            CalculateSpread(watch);
                        }
                        #endregion

                        #region Leg2

                        foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.Leg2.ContractInfo.TokenNo) == uni)))
                        {
                            int i = watch.RowData.Index;
                            if (watch.Leg2.BuyPrice != 0)
                                watch.Leg2.OldBuyPrice = watch.Leg2.BuyPrice;
                            if (watch.Leg2.SellPrice != 0)
                                watch.Leg2.OldSellPrice = watch.Leg2.SellPrice;
                            watch.Leg2.BuyPrice = buyPrice / 100;
                            watch.Leg2.SellPrice = AskPrice / 100;
                            watch.Leg2.Sequence = seq;
                            watch.Leg2.LastTradedPrice = Convert.ToDecimal(Ltp) / 100;
                            watch.Leg2.MidPrice = Math.Round(Convert.ToDouble((watch.Leg2.BuyPrice + watch.Leg2.SellPrice) / 2), 2);
                            watch.RowData.Cells[WatchConst.L2buyPrice].Value = watch.Leg2.BuyPrice;
                            watch.RowData.Cells[WatchConst.L2sellPrice].Value = watch.Leg2.SellPrice;

                            if (AppGlobal.Record)
                            {
                                if (watch.uniqueId == AppGlobal.RuleRecord)
                                {
                                    TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg2.BuyPrice + "|SellPrice|" + watch.Leg2.SellPrice + "|Sequence|" + seq);
                                }
                            }
                            CalculateGreek(watch);
                            if (watch.StrategyId == 111 || watch.StrategyId == 211 || watch.StrategyId == 311)
                            {
                                CalculateSpread(watch);
                                CalculateSpreadRatio11_12(watch);
                            }
                            if (watch.StrategyId == 2211)
                                CalculateStrangleSpread(watch);
                            else if (watch.StrategyId == 2211)
                                CalculateStrangleSpread(watch);
                            else if (watch.StrategyId == 888)
                                CalculateLadderSpread(watch);
                        }
                        #endregion

                        #region Leg3

                        foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.Leg3.ContractInfo.TokenNo) == uni)))
                        {
                            int i = watch.RowData.Index;
                            if (watch.Leg3.BuyPrice != 0)
                                watch.Leg3.OldBuyPrice = watch.Leg3.BuyPrice;
                            if (watch.Leg3.SellPrice != 0)
                                watch.Leg3.OldSellPrice = watch.Leg3.SellPrice;
                            watch.Leg3.BuyPrice = buyPrice / 100;
                            watch.Leg3.SellPrice = AskPrice / 100;
                            watch.Leg3.Sequence = seq;
                            watch.Leg3.LastTradedPrice = Convert.ToDecimal(Ltp) / 100;
                            watch.Leg3.MidPrice = Math.Round(Convert.ToDouble((watch.Leg3.BuyPrice + watch.Leg3.SellPrice) / 2), 2);
                            watch.RowData.Cells[WatchConst.L3buyPrice].Value = watch.Leg3.BuyPrice;
                            watch.RowData.Cells[WatchConst.L3sellPrice].Value = watch.Leg3.SellPrice;

                            if (AppGlobal.Record)
                            {
                                if (watch.uniqueId == AppGlobal.RuleRecord)
                                {
                                    TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg2.BuyPrice + "|SellPrice|" + watch.Leg2.SellPrice + "|Sequence|" + seq);
                                }
                            }
                            CalculateGreek(watch);
                            if (watch.StrategyId == 888)
                                CalculateLadderSpread(watch);
                        }
                        #endregion

                        #endregion
                    }
                    else if (TransCode == 4)
                    {
                        #region tradeWatch

                        #endregion
                    }
                    else if (TransCode == 20)
                    {
                        #region Banknifty and Nifty Roll Spread
                        BTPacket.SpreadMarketUpdate packetHeader = PinnedPacket<BTPacket.SpreadMarketUpdate>(message);
                        if (AppGlobal.BKToken == Convert.ToUInt64(packetHeader.UniqueId1) && AppGlobal.BKToken2 == Convert.ToUInt64(packetHeader.UniqueId2))
                        {
                            lblBkRoll.Text = Convert.ToString(Math.Round(packetHeader.AskPrice / 100, 2));
                        }
                        if (AppGlobal.NiftyToken == Convert.ToUInt64(packetHeader.UniqueId1) && AppGlobal.NiftyToken2 == Convert.ToUInt64(packetHeader.UniqueId2))
                        {
                            lblNiftyRoll.Text = Convert.ToString(Math.Round(packetHeader.AskPrice / 100, 2));
                        }
                        #endregion
                    }
                    else if (TransCode == 85)
                    {
                        double buyPrice = BitConverter.ToDouble(message, 16);
                        double AskPrice = BitConverter.ToDouble(message, 24);
                        double Ltp = BitConverter.ToDouble(message, 32);
                        double Volumn = BitConverter.ToDouble(message, 40);
                        UInt64 uni = BitConverter.ToUInt64(message, 48);
                        UInt64 seq = BitConverter.ToUInt64(message, 8);

                        if (uni == 1)
                        {
                            double FutRate = (Ltp);
                            lblcashNifty.Text = Convert.ToString(Math.Round(FutRate, 2));
                            if (Convert.ToDouble(txtNiftyValue.Text) != 0 && Convert.ToDouble(lblcashNifty.Text) != 0)
                            {
                                double diff = Convert.ToDouble(txtNiftyValue.Text) - Convert.ToDouble(lblcashNifty.Text);
                                txtDiffNifty.Text = Convert.ToString(Math.Round(diff, 2));
                            }
                        }
                        else if (uni == 2)
                        {
                            double FutRate = (Ltp);
                            lblcashbk.Text = Convert.ToString(Math.Round(FutRate, 2));
                            if (Convert.ToDouble(txtbankValue.Text) != 0 && Convert.ToDouble(lblcashbk.Text) != 0)
                            {
                                double diff = Convert.ToDouble(txtbankValue.Text) - Convert.ToDouble(lblcashbk.Text);
                                txtDiffBk.Text = Convert.ToString(Math.Round(diff, 2));
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public void initializeDis()
        {
            IClaimStrategy ClaimstrategyProcessPacket = new MultiThreadedClaimStrategy(8192);
            IWaitStrategy WaitStrategyProcessPacket = new BlockingWaitStrategy();
            ClassDisruptor.RequestDisruptor = new Disruptor.Dsl.Disruptor<Straddle.AppClasses.PacketProcess>(() => new Straddle.AppClasses.PacketProcess(), ClaimstrategyProcessPacket, WaitStrategyProcessPacket, TaskScheduler.Default);
            ClassDisruptor.RequestDisruptor.HandleEventsWith(new Straddle.AppClasses.HandleTradeNotifications());
            ClassDisruptor.ringBufferRequest = ClassDisruptor.RequestDisruptor.Start();
        }

        void CheckSpreadValidation(MarketWatch watch)
        {
            if (watch.Over != 0)
            {
                if ((Convert.ToDouble(watch.Wind) - watch.Leg2.BidDeriveDiff) < (watch.Threshold * -1))
                {
                    int iRow = watch.RowData.Index;
                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    watch.Over = 0;
                    watch.Round = 0;
                    watch.Wind = 1000;
                    watch.unWind = 1000;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                    snd.TransCode = 1;

                    snd.UniqueID = unique;
                    snd.Wind = Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
                    snd.Unwind = Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
                    snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                    snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                    snd.Token = Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo);
                    snd.gui_id = watch.Gui_id;
                    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Red;

                    long seq = ClassDisruptor.ringBufferRequest.Next();
                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                    ClassDisruptor.ringBufferRequest.Publish(seq);

                    TransactionWatch.ErrorMessage("Wind Spread difference widden");
                }
            }
            if (watch.Round != 0)
            {
                if ((Convert.ToDouble(watch.unWind) - watch.Leg2.DeriveDiff) < (watch.Threshold * -1))
                {
                    int iRow = watch.RowData.Index;
                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    watch.Over = 0;
                    watch.Round = 0;
                    watch.Wind = 1000;
                    watch.unWind = 1000;
                    watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                    watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                    watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                    watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                    snd.TransCode = 1;

                    snd.UniqueID = unique;
                    snd.Wind = Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
                    snd.Unwind = Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
                    snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                    snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                    snd.Token = Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo);
                    snd.gui_id = watch.Gui_id;
                    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Red;

                    long seq = ClassDisruptor.ringBufferRequest.Next();
                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                    ClassDisruptor.ringBufferRequest.Publish(seq);
                    TransactionWatch.ErrorMessage("UnWind Spread difference widden");
                }
            }
        }

        void CalculateGreek(MarketWatch watch)
        {
            if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0
                && watch.niftyLeg.SellPrice != 0 && watch.niftyLeg.BuyPrice != 0)
            {
                #region buy Greek Leg1
                GreeksVariable ThetaCalculation = new GreeksVariable();
                ThetaCalculation.SpotPrice = Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                ThetaCalculation.IntrestRate = 0;
                ThetaCalculation.StrikePrice = (double)(Convert.ToDecimal(watch.Leg1.ContractInfo.StrikePrice));
                UInt32 Thetaexpiry = Convert.ToUInt32(watch.Leg1.expiryUniqueID);
                double ThetatimeToExpiry = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, Thetaexpiry));
                string today1 = DateTime.Now.ToString("ddMMMyyyy");

                //if (watch.Expiry == today1)
                //{
                //    ThetaCalculation.TimeToExpiry = 0.50;
                //}
                //else
                //{
                //    ThetaCalculation.TimeToExpiry = ThetatimeToExpiry;
 
                //}
                if (ThetatimeToExpiry >= 1.00)
                {

                    ThetaCalculation.TimeToExpiry = ThetatimeToExpiry + 1;
                }
                else
                {
                    ThetaCalculation.TimeToExpiry = 1;
                }

                ThetaCalculation.DividentYield = 0;
                ThetaCalculation.ActualValue = (double)watch.Leg1.LastTradedPrice;

                if (watch.Leg1.ContractInfo.Series == "CE")
                {
                    ThetaCalculation.Volatility = Convert.ToDouble(CalculatorUtils.CallVolatility(ThetaCalculation));
                    watch.Theta = Math.Round(Convert.ToDouble(CalculatorUtils.CallTheta(ThetaCalculation)), 4);
                }
                else if (watch.Leg1.ContractInfo.Series == "PE")
                {
                    ThetaCalculation.Volatility = Convert.ToDouble(CalculatorUtils.PutVolatility(ThetaCalculation));
                    watch.Theta = Math.Round(Convert.ToDouble(CalculatorUtils.PutTheta(ThetaCalculation)), 4);
                }
                else
                {
                    watch.Theta = 0;
                }

                if (ThetaCalculation.TimeToExpiry <= 4)
                {
                    double thetacal = (double)watch.Leg1.LastTradedPrice * 0.60;
                    if (thetacal < Math.Abs(watch.Theta))
                    {
                        if (ThetaCalculation.TimeToExpiry > 1)
                            watch.Theta = Math.Round(((((double)watch.Leg1.LastTradedPrice / ThetaCalculation.TimeToExpiry) + Math.Abs(watch.Theta)) / 2), 4) * -1;
                        else
                            watch.Theta = Math.Round((double)watch.Leg1.LastTradedPrice / ThetaCalculation.TimeToExpiry, 4) * -1;
                    }
                    //else
                    //{
                    //    watch.Theta = Math.Round(thetacal, 4) * -1;
                    //}
                }
                GreeksVariable stk1 = new GreeksVariable();
                stk1.SpotPrice = Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                stk1.IntrestRate = 0;
                stk1.StrikePrice = (double)(Convert.ToDecimal(watch.Leg1.ContractInfo.StrikePrice));
                UInt32 expiry = Convert.ToUInt32(watch.Leg1.expiryUniqueID);
                double timeToExpiry = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, expiry));

                if (watch.Expiry == today1)
                {
                    stk1.TimeToExpiry = 0.5;
                }
                else
                {
                    if (timeToExpiry >= 1.00)
                        stk1.TimeToExpiry = timeToExpiry;
                    else
                        stk1.TimeToExpiry = 0.50;
                }
                stk1.DividentYield = 0;
                stk1.ActualValue = (double)watch.Leg1.BuyPrice;

                if (watch.Leg1.ContractInfo.Series == "CE")
                {
                    watch.Leg1.BuyIV = Math.Round(Convert.ToDouble(CalculatorUtils.CallVolatility(stk1)), 2);
                    watch.RowData.Cells[WatchConst.BidIv].Value = watch.Leg1.BuyIV;
                }
                else if (watch.Leg1.ContractInfo.Series == "PE")
                {
                    watch.Leg1.BuyIV = Math.Round(Convert.ToDouble(CalculatorUtils.PutVolatility(stk1)), 2);
                    watch.RowData.Cells[WatchConst.BidIv].Value = watch.Leg1.BuyIV;
                }
                #endregion

                #region sell Greek Leg1
                GreeksVariable stk2 = new GreeksVariable();
                stk2.SpotPrice = Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                stk2.IntrestRate = 0;
                stk2.StrikePrice = (double)(Convert.ToDecimal(watch.Leg1.ContractInfo.StrikePrice));
                //stk2.TimeToExpiry = CalculatorUtils.CalculateDay(Convert.ToDateTime(watch.Leg1.ContractInfo.ExpiryDate));
                if (watch.Expiry == today1)
                {
                    stk2.TimeToExpiry = 0.5;
                }
                else
                {
                    if (timeToExpiry >= 1.0)
                        stk2.TimeToExpiry = timeToExpiry;//CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, watch.Leg1.ContractInfo.ExpiryDate));
                    else
                        stk2.TimeToExpiry = 0.50;
                }
                stk2.DividentYield = 0;
                stk2.ActualValue = (double)watch.Leg1.SellPrice;

                if (watch.Leg1.ContractInfo.Series == "CE")
                {
                    watch.Leg1.SellIV = Math.Round(Convert.ToDouble(CalculatorUtils.CallVolatility(stk2)), 2);
                    watch.RowData.Cells[WatchConst.SellIv].Value = watch.Leg1.SellIV;
                }
                else if (watch.Leg1.ContractInfo.Series == "PE")
                {
                    watch.Leg1.SellIV = Math.Round(Convert.ToDouble(CalculatorUtils.PutVolatility(stk2)), 2);
                    watch.RowData.Cells[WatchConst.SellIv].Value = watch.Leg1.SellIV;
                }
                #endregion

                #region Ltp Greeks
                GreeksVariable stk3 = new GreeksVariable();

                stk3.SpotPrice = Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                stk3.IntrestRate = 0;
                stk3.StrikePrice = (double)(Convert.ToDecimal(watch.Leg1.ContractInfo.StrikePrice));
                int expiry1 = Convert.ToInt32(watch.Leg1.expiryUniqueID);
                double timeToExpiry1 = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, expiry1));
                string today = DateTime.Now.ToString("ddMMMyyyy");
                if (watch.Expiry == today)
                {
                    stk3.TimeToExpiry = 0.5;
                }
                else
                {
                    if (timeToExpiry1 > 1.0)
                        stk3.TimeToExpiry = timeToExpiry1;
                    else
                        stk3.TimeToExpiry = 1;
                }
                stk3.DividentYield = 0;
                stk3.ActualValue = (double)watch.Leg1.LastTradedPrice;
                if (watch.Leg1.ContractInfo.Series == "CE")
                {
                    stk3.Volatility = Convert.ToDouble(CalculatorUtils.CallVolatility(stk3));
                    watch.Delta = Math.Round(Convert.ToDouble(CalculatorUtils.CallDelta(stk3)), 4);
                    watch.Vega = Math.Round(Convert.ToDouble(CalculatorUtils.CallVega(stk3)), 4);
                    watch.Gamma = Math.Round(Convert.ToDouble(CalculatorUtils.CallGamma(stk3)), 4);
                }
                else if (watch.Leg1.ContractInfo.Series == "PE")
                {
                    stk3.Volatility = Convert.ToDouble(CalculatorUtils.PutVolatility(stk3));
                    watch.Delta = Math.Round(Convert.ToDouble(CalculatorUtils.PutDelta(stk3)), 4);
                    watch.Vega = Math.Round(Convert.ToDouble(CalculatorUtils.PutVega(stk3)), 4);
                    watch.Gamma = Math.Round(Convert.ToDouble(CalculatorUtils.PutGamma(stk3)), 4);
                }
                else
                {
                    watch.Delta = 1;
                    watch.Vega = 0;
                    watch.Theta = 0;
                    watch.Gamma = 0;
                }
                watch.RowData.Cells[WatchConst.Delta].Value = watch.Delta;
                watch.RowData.Cells[WatchConst.Vega].Value = watch.Vega;
                watch.RowData.Cells[WatchConst.Theta].Value = watch.Theta;
                watch.RowData.Cells[WatchConst.Gamma].Value = watch.Gamma;
                if (watch.posInt != 0)
                {
                    if (watch.StrategyId != 91)
                    {
                        if (watch.StrategyId == 121)
                        {
                            if (watch.posInt > 0)
                            {
                                watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                            }
                            else
                            {
                                watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                            }
                        }
                        else if (watch.StrategyId == 1331 || watch.StrategyId == 1221)
                        {
                            if (watch.posInt < 0)
                            {
                                watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                            }
                            else
                            {
                                watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                            }
                        }
                        else if (watch.StrategyId == 111 || watch.StrategyId == 211 || watch.StrategyId == 311)
                        {
                            if (watch.posInt > 0)
                            {
                                watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);

                            }
                            else if (watch.posInt < 0)
                            {
                                watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                            }
                        }
                        else if (watch.StrategyId == 2211)
                        {
                            if (watch.posInt > 0)
                            {
                                watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);

                            }
                            else if (watch.posInt < 0)
                            {
                                watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                                watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * -1 * Math.Abs(watch.posInt)), 4);
                            }
                        }
                        else
                        {
                            watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                            watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                            watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                            watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt)), 4);
                        }
                    }
                    else
                    {
                        watch.Leg1.DeltaV = Math.Round((watch.Delta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * watch.posInt), 4);
                        watch.Leg1.VegaV = Math.Round((watch.Vega * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * watch.posInt), 4);
                        watch.Leg1.ThetaV = Math.Round((watch.Theta * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * watch.posInt), 4);
                        watch.Leg1.GammaV = Math.Round((watch.Gamma * watch.Leg1.Ratio * watch.Leg1.ContDetail.LotSize * watch.posInt), 4);
                    }
                }
                else
                {
                    watch.Leg1.DeltaV = 0;
                    watch.Leg1.VegaV = 0;
                    watch.Leg1.ThetaV = 0;
                    watch.Leg1.GammaV = 0;
                }
                #endregion
            }
            if (watch.StrategyId == 91)
            {
                watch.RowData.Cells[WatchConst.WindDelta].Value = Math.Round(watch.Delta * watch.Leg1.Ratio, 4);
                watch.RowData.Cells[WatchConst.UnwindDelta].Value = Math.Round(watch.Delta * watch.Leg1.Ratio * -1, 4);
            }
            if (watch.posInt != 0)
            {
                if (watch.StrategyId == 91)
                {
                    watch.sumDelta = Math.Round(watch.Leg1.DeltaV, 4);
                    watch.sumVega = Math.Round(watch.Leg1.VegaV, 4);
                    watch.sumTheta = Math.Round(watch.Leg1.ThetaV, 4);
                    watch.sumGamma = Math.Round(watch.Leg1.GammaV, 4);

                    watch.RowData.Cells[WatchConst.DeltaV].Value = watch.sumDelta;
                    watch.RowData.Cells[WatchConst.VegaV].Value = watch.sumVega;
                    watch.RowData.Cells[WatchConst.ThetaV].Value = watch.sumTheta;
                    watch.RowData.Cells[WatchConst.GammaV].Value = watch.sumGamma;
                }
            }
            else
            {
                watch.sumDelta = 0;
                watch.sumVega = 0;
                watch.sumTheta = 0;
                watch.sumGamma = 0;

                watch.RowData.Cells[WatchConst.DeltaV].Value = watch.sumDelta;
                watch.RowData.Cells[WatchConst.VegaV].Value = watch.sumVega;
                watch.RowData.Cells[WatchConst.ThetaV].Value = watch.sumTheta;
                watch.RowData.Cells[WatchConst.GammaV].Value = watch.sumGamma;
            }
        }

        void CalculateSpread(MarketWatch watch)
        {
            #region SellIV spread
            if (watch.Leg1.SellPrice != 0 && watch.Leg2.SellPrice != 0)
            {
                watch.AskPxDiff = Convert.ToDouble(watch.Leg1.SellPrice) - Convert.ToDouble(watch.Leg2.SellPrice);
                watch.AskIVDiff = Convert.ToDouble(watch.Leg1.SellIV) - Convert.ToDouble(watch.Leg2.SellIV);
                watch.RowData.Cells[WatchConst.AskPxdiff].Value = Math.Round(watch.AskPxDiff, 2);
                watch.RowData.Cells[WatchConst.AskIVDiff].Value = Math.Round(watch.AskIVDiff, 2);
            }
            #endregion

            #region Bid IV Spread
            if (watch.Leg1.BuyPrice != 0 && watch.Leg2.BuyPrice != 0)
            {
                watch.BidPxDiff = Convert.ToDouble(watch.Leg2.BuyPrice) - Convert.ToDouble(watch.Leg1.BuyPrice);
                watch.BidIVDiff = Convert.ToDouble(watch.Leg2.BuyIV) - Convert.ToDouble(watch.Leg1.BuyIV);
                watch.RowData.Cells[WatchConst.BidPxdiff].Value = Math.Round(watch.BidPxDiff, 2);
                watch.RowData.Cells[WatchConst.BidIVDiff].Value = Math.Round(watch.BidIVDiff, 2);
            }
            #endregion
        }

        bool CalculateMisprice(MarketWatch watch)
        {
            // to be called only for ratio
            if (watch.posInt > 0)
            {
                if (watch.Leg2.SellPrice <= watch.Leg1.BuyPrice)
                    return false;

                else
                {
                    TransactionWatch.ErrorMessage("|CalculateMisprice|" + "|UniqueID|" + watch.uniqueId + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice);
                    return true;
                }
            }
            return false;
        }

        bool CalculateMisSpread(MarketWatch watch)
        {
            bool ret = false;
            bool leg1_bid_ask_normal = true;
            bool leg2_bid_ask_normal = true;
            bool leg3_bid_ask_normal = true;
            //Rs 30
            if (watch.Leg1.SellPrice - watch.Leg1.BuyPrice < 30.0 / (watch.Leg1.ContDetail.LotSize))
                leg1_bid_ask_normal = false;
            if (watch.Leg2.SellPrice - watch.Leg2.BuyPrice < 30.0 / (watch.Leg1.ContDetail.LotSize) && watch.Leg2.SellPrice > 0 && watch.Leg2.BuyPrice > 0)
                leg2_bid_ask_normal = false;
            if (watch.Leg3.SellPrice - watch.Leg3.BuyPrice < 30.0 / (watch.Leg1.ContDetail.LotSize) && watch.Leg3.SellPrice > 0 && watch.Leg2.BuyPrice > 0)
                leg3_bid_ask_normal = false;
            if (watch.Leg1.BuyPrice > 0)
            {
                if (((watch.Leg1.SellPrice - watch.Leg1.BuyPrice) / watch.Leg1.BuyPrice * 100) > 10)
                {

                    if (!leg1_bid_ask_normal)
                        ret = false;
                    else
                    {
                        TransactionWatch.ErrorMessage("|CalculateMisSpread|" + "|UniqueID|" + watch.uniqueId + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice);
                        ret = true;
                    }
                }
            }
            if (watch.Leg2.BuyPrice > 0)
            {
                if (((watch.Leg2.SellPrice - watch.Leg2.BuyPrice) / watch.Leg2.BuyPrice * 100) > 10)
                {

                    if (!leg2_bid_ask_normal)
                        ret = ret || false;
                    else
                    {
                        ret = true;
                        TransactionWatch.ErrorMessage("|CalculateMisSpread|" + "|UniqueID|" + watch.uniqueId + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice);
                    }
                }
            }
            if (watch.Leg3.BuyPrice > 0)
            {
                if (((watch.Leg3.SellPrice - watch.Leg3.BuyPrice) / watch.Leg3.BuyPrice * 100) > 10)
                {
                    if (!leg3_bid_ask_normal)
                        ret = ret || false;
                    else
                    {
                        TransactionWatch.ErrorMessage("|CalculateMisSpread|" + "|UniqueID|" + watch.uniqueId + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                        ret = true;
                    }
                }
            }
            return ret;
        }

        void CalculateSpreadRatio11_12(MarketWatch watch)
        {
            try
            {
                if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0
                                       && watch.Leg2.BuyPrice != 0 && watch.Leg2.SellPrice != 0)
                {
                    #region spread

                    double forwardspread = 0, reversespread = 0;
                    forwardspread = -(watch.Leg1.SellPrice * watch.Leg1.Ratio) + (watch.Leg2.BuyPrice * watch.Leg2.Ratio);
                    reversespread = (watch.Leg1.BuyPrice * watch.Leg1.Ratio) - (watch.Leg2.SellPrice * watch.Leg2.Ratio);

                    watch.MktWind = forwardspread;
                    watch.MktunWind = reversespread;
                    //if (watch.MktWind != 0)
                    watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktWind), watch.Leg1), 2);
                    // if (watch.MktunWind != 0)
                    watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktunWind), watch.Leg1), 2);
                    watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio) + (watch.Leg2.MidPrice * watch.Leg2.Ratio)) * 0.0018, 4);
                    watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;

                    if (watch.Leg1.N_Qty > 0)
                    {
                        watch.pnl = (watch.Leg1.N_Price + reversespread) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                        watch.S_pnl = (watch.Leg1.N_Price + reversespread) * watch.Leg1.ContDetail.LotSize;
                    }
                    else if (watch.Leg1.N_Qty < 0)
                    {
                        watch.pnl = (watch.Leg1.N_Price + forwardspread) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                        watch.S_pnl = (watch.Leg1.N_Price + forwardspread) * watch.Leg1.ContDetail.LotSize;
                    }
                    watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);
                    watch.RowData.Cells[WatchConst.S_Pnl].Value = Math.Round(watch.S_pnl, 2);
                    #endregion
                }
            }
            catch (Exception)
            {

            }
        }

        void CalculateStrangleSpread(MarketWatch watch)
        {
            if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0
                                                 && watch.Leg2.BuyPrice != 0 && watch.Leg2.SellPrice != 0)
            {
                #region spread
                double forwardspread = 0, reversespread = 0;
                reversespread = (Convert.ToDouble(watch.Leg1.BuyPrice) * watch.Leg1.Ratio) + (Convert.ToDouble(watch.Leg2.BuyPrice) * watch.Leg2.Ratio);
                forwardspread = (Convert.ToDouble(watch.Leg1.SellPrice) * -1 * watch.Leg1.Ratio) + (Convert.ToDouble(watch.Leg2.SellPrice) * -1 * watch.Leg2.Ratio);

                watch.MktWind = forwardspread;
                watch.MktunWind = reversespread;
                if (watch.MktWind != 0)
                    watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktWind), watch.Leg1), 2);
                if (watch.MktunWind != 0)
                    watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktunWind), watch.Leg1), 2);
                watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio) + (watch.Leg2.MidPrice * watch.Leg2.Ratio)) * 0.0018, 2);
                watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;

                if (watch.Leg2.ContractInfo.TokenNo != "0")
                {
                    if (watch.Leg1.N_Qty > 0)
                    {
                        watch.pnl = (watch.Leg1.N_Price + reversespread) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                        watch.S_pnl = (watch.Leg1.N_Price + reversespread) * watch.Leg1.ContDetail.LotSize;
                    }
                    else if (watch.Leg1.N_Qty < 0)
                    {
                        watch.pnl = (watch.Leg1.N_Price + forwardspread) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                        watch.S_pnl = (watch.Leg1.N_Price + forwardspread) * watch.Leg1.ContDetail.LotSize;
                    }
                }
                else
                {
                    if (watch.Leg1.Net_Qty > 0)
                    {
                        watch.pnl = (watch.Leg1.N_Price - watch.MktWind) * watch.Leg1.Net_Qty;
                    }
                    else if (watch.Leg1.Net_Qty < 0)
                    {
                        watch.pnl = (watch.Leg1.N_Price - watch.MktunWind) * watch.Leg1.Net_Qty;
                    }
                }
                watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);



                #endregion
            }

            if (watch.Leg2.ContractInfo.TokenNo == "0")
            {
                if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0)
                {
                    #region Spread Single Leg of Strategy 12211
                    //double forwardspread = 0, reversespread = 0;
                    //reversespread = (Convert.ToDouble(watch.Leg1.BuyPrice) * watch.Leg1.Ratio) + (Convert.ToDouble(watch.Leg2.BuyPrice) * watch.Leg2.Ratio);
                    //forwardspread = (Convert.ToDouble(watch.Leg1.SellPrice) * -1 * watch.Leg1.Ratio) + (Convert.ToDouble(watch.Leg2.SellPrice) * -1 * watch.Leg2.Ratio);

                    watch.MktunWind = (watch.Leg1.Ratio * watch.Leg1.BuyPrice);
                    watch.MktWind = (watch.Leg1.Ratio * watch.Leg1.SellPrice);

                    if (watch.MktWind != 0)
                        watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktWind), watch.Leg1), 2);
                    if (watch.MktunWind != 0)
                        watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktunWind), watch.Leg1), 2);
                    watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio) + (watch.Leg2.MidPrice * watch.Leg2.Ratio)) * 0.0018, 2);
                    watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;

                    if (watch.Leg1.Net_Qty > 0)
                    {
                        watch.pnl = (watch.Leg1.N_Price - watch.MktWind) * watch.Leg1.Net_Qty;
                        //  watch.S_pnl = watch.pnl / Math.Abs(watch.posInt);
                    }
                    else if (watch.Leg1.Net_Qty < 0)
                    {
                        watch.pnl = (watch.Leg1.N_Price - watch.MktunWind) * watch.Leg1.Net_Qty;
                        //  watch.S_pnl = watch.pnl / Math.Abs(watch.posInt);
                    }
                    watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);

                    #endregion
                }
            }
        }

        void LSL_StranglePnl(MarketWatch watch)
        {
            if (watch.L1PosInt != 0 && watch.L2PosInt != 0)
            {
                watch.LSL_StrategyLive = ((watch.Leg1.BuyPrice * Math.Abs(watch.L1PosInt)) + (watch.Leg2.BuyPrice * Math.Abs(watch.L2PosInt)));
                watch.RowData.Cells[WatchConst.LSL_StrategyLive].Value = watch.LSL_StrategyLive;
            }
            else
            {
                watch.LSL_StrategyLive = 0;
                watch.RowData.Cells[WatchConst.LSL_StrategyLive].Value = watch.LSL_StrategyLive;
            }
        }

        void LSL_StrangleCheckFlgStoploss(MarketWatch watch)
        {
            if (watch.TLI_StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo != "0")
            {
                if (watch.LSL_StopLossFlg)
                {
                    if (watch.LSL_StrategyLive >= (watch.StrategyAvgPrice + watch.LSL_StopLossValue))
                    {
                        watch.LSL_StopLossFlg = false;
                        MessageBox.Show("LSL_Strategy|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.Leg2.ContractInfo.StrikePrice + "|" + watch.Leg2.ContractInfo.Series + "|"
                                        + watch.LSL_StrategyLive + "|" + watch.StrategyAvgPrice + "|" + watch.LSL_StopLossValue);

                    }
                }
            }
            if (watch.TLI_StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo == "0")
            {
                if (watch.LSL_StopLossFlg)
                {
                    if (watch.Leg1.BuyPrice > watch.LSL_StopLossValue)
                    {
                        watch.LSL_StopLossFlg = false;
                        MessageBox.Show("LSL_Strategy|Single|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Leg1.ContractInfo.Series + "|"
                                        + watch.Leg1.BuyPrice + "|" + watch.LSL_StopLossValue);
                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToInt32(x.LSL_UniqueId) == watch.LSL_UniqueId) && (x.Leg2.ContractInfo.TokenNo != "0") && (x.StrategyId == 32211)))
                        {
                            watch1.LSL_StopLossPercent = 1;
                            watch1.RowData.Cells[WatchConst.LSL_StrategyPercent].Value = watch1.LSL_StopLossPercent;
                            watch1.LSL_StopLossValue = (watch1.StrategyAvgPrice * watch1.LSL_StopLossPercent / 100);
                            watch1.RowData.Cells[WatchConst.LSL_StrategyValue].Value = watch1.LSL_StopLossValue;
                        }
                    }
                }
            }
        }

        void CalculateButterflySpread(MarketWatch watch)
        {
            if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0
                && watch.Leg2.BuyPrice != 0 && watch.Leg2.SellPrice != 0
                && watch.Leg3.BuyPrice != 0 && watch.Leg3.SellPrice != 0)
            {

                watch.MktunWind = (-1 * watch.Leg1.Ratio * watch.Leg1.SellPrice) + (watch.Leg2.Ratio * watch.Leg2.BuyPrice) + (-1 * watch.Leg3.Ratio * watch.Leg3.SellPrice);
                watch.MktWind = (watch.Leg1.Ratio * watch.Leg1.BuyPrice) + (-1 * watch.Leg2.Ratio * watch.Leg2.SellPrice) + (watch.Leg3.Ratio * watch.Leg3.BuyPrice);

                watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio) + (watch.Leg2.MidPrice * watch.Leg2.Ratio) + (watch.Leg3.MidPrice * watch.Leg3.Ratio)) * 0.0018, 2);
                watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;

                if (watch.MktWind != 0)
                    watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktWind), watch.Leg1), 2);
                if (watch.MktunWind != 0)
                    watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktunWind), watch.Leg1), 2);



                if (watch.Leg1.N_Qty > 0)
                {
                    watch.pnl = (watch.MktWind + watch.Leg1.N_Price) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                    watch.S_pnl = (watch.MktWind + watch.Leg1.N_Price) * watch.Leg1.ContDetail.LotSize;
                }
                else if (watch.Leg1.N_Qty < 0)
                {
                    watch.pnl = (watch.MktunWind + watch.Leg1.N_Price) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                    watch.S_pnl = (watch.MktunWind + watch.Leg1.N_Price) * watch.Leg1.ContDetail.LotSize;
                }
                else
                {
                    watch.pnl = 0;
                }
                watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);
                watch.RowData.Cells[WatchConst.S_Pnl].Value = Math.Round(watch.S_pnl, 2);
                watch.misSpread = CalculateMisSpread(watch);
                if (watch.misSpread == true)
                    return;
                #region Profit and StopLoss
                if (watch.Leg1.N_Qty != 0)
                {
                    if (watch.pnl > watch.MaxPnl)
                    {
                        if (watch.posInt > 0)
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|unwind|" + Math.Round(watch.MktunWind, 2));
                        else
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|wind|" + Math.Round(watch.MktWind, 2));
                        watch.MaxPnl = watch.pnl;
                        watch.S_MaxPnl = Math.Round(watch.pnl / Math.Abs(watch.posInt), 2);
                        watch.RowData.Cells[WatchConst.MaxPnl].Value = Math.Round(watch.MaxPnl, 2);
                        watch.RowData.Cells[WatchConst.S_MaxPnl].Value = watch.S_MaxPnl;
                        if (watch.posInt > 0)
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|unwind|" + Math.Round(watch.MktunWind, 2));
                        else
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|wind|" + Math.Round(watch.MktWind, 2));

                    }
                }
                if (watch.misSpread == true)
                    return;
                if (watch.Profit != 0)
                {
                    if (watch.Leg1.N_Qty != 0)
                    {
                        if (watch.posInt != 0)
                        {
                            if (watch.Profit <= watch.pnl / Math.Abs(watch.posInt))
                            {
                                UInt64 currentTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                if (watch.NotificationTimeProfit <= currentTime)
                                {
                                    watch.NotificationTimeProfit = currentTime + 1;
                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                    snd.TransCode = 10;
                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                    snd.UniqueID = unique;
                                    snd.gui_id = AppGlobal.GUI_ID;
                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                    snd.Open = 1;
                                    if (watch.posInt < 0)
                                        snd.isWind = true;
                                    else
                                        snd.isWind = false;
                                    if (watch.ProfitFlg == false)
                                    {
                                        watch.ProfitFlg = true;
                                        if (watch.posInt < 0)
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "|True|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize);
                                            TransactionWatch.TransactionMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True", Color.Red);
                                        }
                                        else
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize);
                                            TransactionWatch.TransactionMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False", Color.Red);
                                        }
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);

                                    }
                                    watch.NotificationTimeProfit = currentTime + 1;
                                }
                            }
                        }
                    }
                }
                if (watch.DrawDown != 0)
                {
                    if (watch.Leg1.N_Qty != 0)
                    {
                        if (watch.posInt != 0)
                        {
                            if (watch.DrawDown <= (Math.Abs(watch.MaxPnl - watch.pnl)) / Math.Abs(watch.posInt))
                            {
                                UInt64 currentTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                if (watch.NotificationTimeDrawdown <= currentTime)
                                {
                                    if (watch.IsStrikeReq == false)
                                    {
                                        TransactionWatch.TransactionMessage("UniqueId |" + watch.uniqueId + "|Please Strike Request|", Color.Red);
                                        return;
                                    }


                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                    snd.TransCode = 10;
                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                    snd.UniqueID = unique;
                                    snd.gui_id = AppGlobal.GUI_ID;
                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                    snd.Open = 1;
                                    if (watch.posInt < 0)
                                        snd.isWind = true;
                                    else
                                        snd.isWind = false;
                                    if (watch.DrawDownFlg == false)
                                    {
                                        watch.DrawDownFlg = true;
                                        if (watch.posInt < 0)
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "|True|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize);
                                            TransactionWatch.TransactionMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True", Color.Red);
                                        }
                                        else
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize);
                                            TransactionWatch.TransactionMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False", Color.Red);
                                        }
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                    }
                                    watch.NotificationTimeDrawdown = currentTime + 1;
                                }
                            }
                        }
                    }
                }
                #endregion

            }
        }

        void CalculateSpreadSingle(MarketWatch watch)
        {
            if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0)
            {
                watch.MktunWind = (watch.Leg1.Ratio * watch.Leg1.BuyPrice);
                watch.MktWind = (watch.Leg1.Ratio * watch.Leg1.SellPrice);

                if (watch.Leg1.ContractInfo.Series == "XX")
                {
                    watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio)) * 0.0002, 4);
                    watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;
                }
                else
                {
                    watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio)) * 0.0018, 4);
                    watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;
                }
                watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(watch.MktWind, 2);
                watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(watch.MktunWind, 2);
                if (watch.Leg1.Net_Qty > 0)
                {
                    watch.pnl = (watch.Leg1.N_Price - watch.MktWind) * watch.Leg1.Net_Qty;
                }
                else if (watch.Leg1.Net_Qty < 0)
                {
                    watch.pnl = (watch.Leg1.N_Price - watch.MktunWind) * watch.Leg1.Net_Qty;
                }
                watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);

                #region Profit and StopLoss
                /*if (watch.Leg1.N_Qty != 0)
                {
                    if (watch.pnl > watch.MaxPnl)
                    {
                        if(watch.posInt > 0)
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|unwind|" + Math.Round(watch.MktunWind, 2));
                        else
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|wind|" + Math.Round(watch.MktWind, 2));
                        watch.MaxPnl = watch.pnl;
                        watch.S_MaxPnl = Math.Round(watch.pnl / Math.Abs(watch.posInt), 2);
                        watch.RowData.Cells[WatchConst.MaxPnl].Value = Math.Round(watch.MaxPnl, 2);
                        watch.RowData.Cells[WatchConst.S_MaxPnl].Value = watch.S_MaxPnl;
                        if (watch.posInt > 0)
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|unwind|" + Math.Round(watch.MktunWind, 2));
                        else
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|wind|" + Math.Round(watch.MktWind, 2));

                    }
                }
                if (watch.misSpread == true)
                    return;
                if (watch.Profit != 0)
                {
                    if (watch.Leg1.N_Qty != 0)
                    {
                        if (watch.posInt != 0)
                        {
                            if (watch.Profit <= watch.pnl / Math.Abs(watch.posInt))
                            {
                                UInt64 currentTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                if (watch.NotificationTimeProfit <= currentTime)
                                {
                                    watch.NotificationTimeProfit = currentTime + 1;
                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                    snd.TransCode = 10;
                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                    snd.UniqueID = unique;
                                    snd.gui_id = AppGlobal.GUI_ID;
                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                    snd.Open = 1;
                                    if (watch.posInt < 0)
                                        snd.isWind = true;
                                    else
                                        snd.isWind = false;                                   

                                    if (watch.ProfitFlg == false)
                                    {
                                        watch.ProfitFlg = true;
                                        if (watch.posInt < 0)
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "|True|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True", Color.Red);
                                        }
                                        else
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False", Color.Red);
                                        }
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                    }
                                    watch.NotificationTimeProfit = currentTime + 1;
                                }
                            }
                        }
                    }
                }
                if (watch.DrawDown != 0)
                {
                    if (watch.Leg1.N_Qty != 0)
                    {
                        if (watch.posInt != 0)
                        {
                            if (watch.DrawDown <= (Math.Abs(watch.MaxPnl - watch.pnl)) / Math.Abs(watch.posInt))
                            {
                                UInt64 currentTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                if (watch.NotificationTimeDrawdown <= currentTime)
                                {
                                    if (watch.IsStrikeReq == false)
                                    {
                                        TransactionWatch.TransactionMessage("UniqueId |" + watch.uniqueId + "|Please Strike Request|", Color.Red);
                                        return;
                                    }
                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                    snd.TransCode = 10;
                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                    snd.UniqueID = unique;
                                    snd.gui_id = AppGlobal.GUI_ID;
                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                    snd.Open = 1;
                                    if (watch.posInt < 0)
                                        snd.isWind = true;
                                    else
                                        snd.isWind = false;
                                    if (watch.DrawDownFlg == false)
                                    {
                                        watch.DrawDownFlg = true;
                                        if (watch.posInt < 0)
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "|True|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True", Color.Red);
                                        }
                                        else
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False", Color.Red);
                                        }
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                       
                                    }
                                    watch.NotificationTimeDrawdown = currentTime + 1;
                                }
                            }
                        }
                    }
                }*/
                #endregion
            }
        }

        void CalculateLadderSpread(MarketWatch watch)
        {

            if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0
                && watch.Leg2.BuyPrice != 0 && watch.Leg2.SellPrice != 0
                && watch.Leg3.BuyPrice != 0 && watch.Leg3.SellPrice != 0)
            {
                #region spread

                double forwardspread = 0, reversespread = 0;

                forwardspread = (Convert.ToDouble(watch.Leg1.SellPrice) * -1 * watch.Leg1.Ratio) + (Convert.ToDouble(watch.Leg2.BuyPrice) * watch.Leg2.Ratio) + (Convert.ToDouble(watch.Leg3.BuyPrice) * watch.Leg3.Ratio);
                reversespread = (Convert.ToDouble(watch.Leg1.BuyPrice) * watch.Leg1.Ratio) + (Convert.ToDouble(watch.Leg2.SellPrice) * -1 * watch.Leg2.Ratio) + (Convert.ToDouble(watch.Leg3.SellPrice) * -1 * watch.Leg3.Ratio);

                watch.MktWind = forwardspread;
                watch.MktunWind = reversespread;
                if (watch.MktWind != 0)
                    watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktWind), watch.Leg1), 2);
                if (watch.MktunWind != 0)
                    watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktunWind), watch.Leg1), 2);
                watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio) + (watch.Leg2.MidPrice * watch.Leg2.Ratio)) * 0.0018, 2);
                watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;

                if (watch.Leg1.N_Qty > 0)
                {
                    watch.pnl = (watch.Leg1.N_Price + reversespread) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                    watch.S_pnl = (watch.Leg1.N_Price + reversespread) * watch.Leg1.ContDetail.LotSize;
                }
                else if (watch.Leg1.N_Qty < 0)
                {
                    watch.pnl = (watch.Leg1.N_Price + forwardspread) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                    watch.S_pnl = (watch.Leg1.N_Price + forwardspread) * watch.Leg1.ContDetail.LotSize;
                }

                watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);
                if (watch.Profit != 0 || watch.DrawDown != 0)
                {
                    watch.misSpread = CalculateMisSpread(watch);
                }

                #region Profit and StopLoss
                if (watch.Leg1.N_Qty != 0)
                {
                    if (watch.pnl > watch.MaxPnl)
                    {
                        if (watch.posInt > 0)
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|unwind|" + Math.Round(watch.MktunWind, 2));
                        else
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|wind|" + Math.Round(watch.MktWind, 2));
                        watch.MaxPnl = watch.pnl;
                        watch.S_MaxPnl = Math.Round(watch.pnl / Math.Abs(watch.posInt), 2);
                        watch.RowData.Cells[WatchConst.MaxPnl].Value = Math.Round(watch.MaxPnl, 2);
                        watch.RowData.Cells[WatchConst.S_MaxPnl].Value = watch.S_MaxPnl;
                        if (watch.posInt > 0)
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|unwind|" + Math.Round(watch.MktunWind, 2));
                        else
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|wind|" + Math.Round(watch.MktWind, 2));
                    }
                }
                if (watch.misSpread == true)
                    return;
                if (watch.Profit != 0)
                {
                    if (watch.Leg1.N_Qty != 0)
                    {
                        if (watch.posInt != 0)
                        {
                            if (watch.Profit <= watch.pnl / Math.Abs(watch.posInt))
                            {
                                UInt64 currentTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                if (watch.NotificationTimeProfit <= currentTime)
                                {
                                    watch.NotificationTimeProfit = currentTime + 1;
                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                    snd.TransCode = 10;
                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                    snd.UniqueID = unique;
                                    snd.gui_id = AppGlobal.GUI_ID;
                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                    snd.Open = 1;
                                    if (watch.posInt < 0)
                                        snd.isWind = true;
                                    else
                                        snd.isWind = false;
                                    if (watch.ProfitFlg == false)
                                    {
                                        watch.ProfitFlg = true;
                                        //TransactionWatch.ErrorMessage("Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True");
                                        if (watch.posInt < 0)
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "|True|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True", Color.Red);
                                        }
                                        else
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False", Color.Red);
                                        }
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                    }
                                    watch.NotificationTimeProfit = currentTime + 1;
                                }
                            }
                        }
                    }
                }
                if (watch.DrawDown != 0)
                {
                    if (watch.Leg1.N_Qty != 0)
                    {
                        if (watch.posInt != 0)
                        {
                            if (watch.DrawDown <= (Math.Abs(watch.MaxPnl - watch.pnl)) / Math.Abs(watch.posInt))
                            {
                                UInt64 currentTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                if (watch.NotificationTimeDrawdown <= currentTime)
                                {
                                    if (watch.IsStrikeReq == false)
                                    {
                                        //MessageBox.Show("Please Strike Request first...");

                                        TransactionWatch.TransactionMessage("UniqueId |" + watch.uniqueId + "|Please Strike Request|", Color.Red);
                                        return;
                                    }

                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                    snd.TransCode = 10;
                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                    snd.UniqueID = unique;
                                    snd.gui_id = AppGlobal.GUI_ID;
                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                    snd.Open = 1;
                                    if (watch.posInt < 0)
                                        snd.isWind = true;
                                    else
                                        snd.isWind = false;
                                    if (watch.DrawDownFlg == false)
                                    {
                                        watch.DrawDownFlg = true;
                                        if (watch.posInt < 0)
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "|True|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True", Color.Red);
                                        }
                                        else
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False", Color.Red);
                                        }
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);

                                    }
                                    watch.NotificationTimeDrawdown = currentTime + 1;
                                }
                            }
                        }
                    }
                }
                #endregion
                #endregion
            }

        }

        void CalculateSpread1331(MarketWatch watch)
        {
            if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0
                                         && watch.Leg2.BuyPrice != 0 && watch.Leg2.SellPrice != 0
                                         && watch.Leg3.BuyPrice != 0 && watch.Leg3.SellPrice != 0
                                         && watch.Leg4.BuyPrice != 0 && watch.Leg4.SellPrice != 0)
            {

                if (watch.MktWind != 0)
                    watch.oldMktWind = (watch.MktWind + watch.oldMktWind) / 2;
                if (watch.MktunWind != 0)
                    watch.oldMktUnWind = (watch.MktunWind + watch.oldMktUnWind) / 2;


                watch.MktunWind = (-1 * watch.Leg1.Ratio * watch.Leg1.SellPrice) + (watch.Leg2.Ratio * watch.Leg2.BuyPrice) + (-1 * watch.Leg3.Ratio * watch.Leg3.SellPrice) + (watch.Leg4.Ratio * watch.Leg4.BuyPrice);
                watch.MktWind = (watch.Leg1.Ratio * watch.Leg1.BuyPrice) + (-1 * watch.Leg2.Ratio * watch.Leg2.SellPrice) + (watch.Leg3.Ratio * watch.Leg3.BuyPrice) + (-1 * watch.Leg4.Ratio * watch.Leg4.SellPrice);

                if (watch.oldMktWind != 0 && watch.MktWind != 0)
                    watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round((watch.MktWind + watch.oldMktWind) / 2, 2);
                if (watch.MktunWind != 0 && watch.oldMktUnWind != 0)
                    watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round((watch.MktunWind + watch.oldMktUnWind) / 2, 2);

                watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio) + (watch.Leg2.MidPrice * watch.Leg2.Ratio) + (watch.Leg3.MidPrice * watch.Leg3.Ratio) + (watch.Leg4.MidPrice * watch.Leg4.Ratio)) * 0.0018, 2);
                watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;


                if (watch.Leg1.N_Qty > 0)
                {
                    watch.pnl = (watch.MktWind + watch.Leg1.N_Price) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                }
                else if (watch.Leg1.N_Qty < 0)
                {
                    watch.pnl = (watch.MktunWind + watch.Leg1.N_Price) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                }
                else
                {

                }
                watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);
            }
        }

        void CalculateSpread1221(MarketWatch watch)
        {
            if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0
                                         && watch.Leg2.BuyPrice != 0 && watch.Leg2.SellPrice != 0
                                         && watch.Leg3.BuyPrice != 0 && watch.Leg3.SellPrice != 0
                                         && watch.Leg4.BuyPrice != 0 && watch.Leg4.SellPrice != 0)
            {

                if (watch.MktWind != 0)
                    watch.oldMktWind = (watch.MktWind + watch.oldMktWind) / 2;
                if (watch.MktunWind != 0)
                    watch.oldMktUnWind = (watch.MktunWind + watch.oldMktUnWind) / 2;


                watch.MktunWind = (-1 * watch.Leg1.Ratio * watch.Leg1.SellPrice) + (watch.Leg2.Ratio * watch.Leg2.BuyPrice) + (-1 * watch.Leg3.Ratio * watch.Leg3.SellPrice) + (watch.Leg4.Ratio * watch.Leg4.BuyPrice);
                watch.MktWind = (watch.Leg1.Ratio * watch.Leg1.BuyPrice) + (-1 * watch.Leg2.Ratio * watch.Leg2.SellPrice) + (watch.Leg3.Ratio * watch.Leg3.BuyPrice) + (-1 * watch.Leg4.Ratio * watch.Leg4.SellPrice);

                if (watch.oldMktWind != 0 && watch.MktWind != 0)
                    watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round((watch.MktWind + watch.oldMktWind) / 2, 2);
                if (watch.MktunWind != 0 && watch.oldMktUnWind != 0)
                    watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round((watch.MktunWind + watch.oldMktUnWind) / 2, 2);

                watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio) + (watch.Leg2.MidPrice * watch.Leg2.Ratio) + (watch.Leg3.MidPrice * watch.Leg3.Ratio) + (watch.Leg4.MidPrice * watch.Leg4.Ratio)) * 0.0018, 2);
                watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;


                if (watch.Leg1.N_Qty > 0)
                {
                    watch.pnl = (watch.MktWind + watch.Leg1.N_Price) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                }
                else if (watch.Leg1.N_Qty < 0)
                {
                    watch.pnl = (watch.MktunWind + watch.Leg1.N_Price) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                }
                else
                {

                }
                watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);
            }
        }

        void CalculateRatioConShortSpread(MarketWatch watch)
        {
            if (watch.Leg1.BuyPrice != 0 && watch.Leg1.SellPrice != 0
                           && watch.Leg2.BuyPrice != 0 && watch.Leg2.SellPrice != 0
                           && watch.Leg3.BuyPrice != 0 && watch.Leg3.SellPrice != 0)
            {
                #region spread

                double forwardspread = 0, reversespread = 0;

                forwardspread = (Convert.ToDouble(watch.Leg1.SellPrice) * -1 * watch.Leg1.Ratio) + (Convert.ToDouble(watch.Leg2.SellPrice) * -1 * watch.Leg2.Ratio) + (Convert.ToDouble(watch.Leg3.BuyPrice) * watch.Leg3.Ratio);
                reversespread = (Convert.ToDouble(watch.Leg1.BuyPrice) * watch.Leg1.Ratio) + (Convert.ToDouble(watch.Leg2.BuyPrice) * watch.Leg2.Ratio) + (Convert.ToDouble(watch.Leg3.SellPrice) * -1 * watch.Leg3.Ratio);

                watch.MktWind = forwardspread;
                watch.MktunWind = reversespread;
                if (watch.MktWind != 0)
                    watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktWind), watch.Leg1), 2);
                if (watch.MktunWind != 0)
                    watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(GetValueAsTickMultiple(Convert.ToDecimal(watch.MktunWind), watch.Leg1), 2);
                watch.TransCost = Math.Round(Convert.ToDouble((watch.Leg1.MidPrice * watch.Leg1.Ratio) + (watch.Leg2.MidPrice * watch.Leg2.Ratio)) * 0.0018, 2);
                watch.RowData.Cells[WatchConst.TrnCost].Value = watch.TransCost;

                if (watch.Leg1.N_Qty > 0)
                {
                    watch.pnl = (watch.Leg1.N_Price + reversespread) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                    watch.S_pnl = (watch.Leg1.N_Price + reversespread) * watch.Leg1.ContDetail.LotSize;
                }
                else if (watch.Leg1.N_Qty < 0)
                {
                    watch.pnl = (watch.Leg1.N_Price + forwardspread) * watch.Leg1.ContDetail.LotSize * Math.Abs(watch.posInt);
                    watch.S_pnl = (watch.Leg1.N_Price + forwardspread) * watch.Leg1.ContDetail.LotSize;
                }

                watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(watch.pnl, 2);
                /*
                watch.misSpread = CalculateMisSpread(watch);
                if (watch.misSpread == true)
                    return;
                #region Profit and StopLoss
                if (watch.Leg1.N_Qty != 0)
                {
                    if (watch.pnl > watch.MaxPnl)
                    {
                        if (watch.posInt > 0)
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|unwind|" + Math.Round(watch.MktunWind, 2));
                        else
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|wind|" + Math.Round(watch.MktWind, 2));
                        watch.MaxPnl = watch.pnl;
                        watch.S_MaxPnl = Math.Round(watch.pnl / Math.Abs(watch.posInt), 2);
                        watch.RowData.Cells[WatchConst.MaxPnl].Value = Math.Round(watch.MaxPnl, 2);
                        watch.RowData.Cells[WatchConst.S_MaxPnl].Value = watch.S_MaxPnl;
                        if (watch.posInt > 0)
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|unwind|" + Math.Round(watch.MktunWind, 2));
                        else
                            TransactionWatch.ErrorMessage("Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|PNL|" + Math.Round(watch.pnl, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|SMaxPNL|" + watch.S_MaxPnl + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|wind|" + Math.Round(watch.MktWind, 2));
                    }
                }
                if (watch.Profit != 0)
                {
                    if (watch.Leg1.N_Qty != 0)
                    {
                        if (watch.posInt != 0)
                        {
                            if (watch.Profit <= watch.pnl / Math.Abs(watch.posInt))
                            {
                                UInt64 currentTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                if (watch.NotificationTimeProfit <= currentTime)
                                {
                                    watch.NotificationTimeProfit = currentTime + 1;
                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                    snd.TransCode = 10;
                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                    snd.UniqueID = unique;
                                    snd.gui_id = AppGlobal.GUI_ID;
                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                    snd.Open = 1;
                                    if (watch.posInt < 0)
                                        snd.isWind = true;
                                    else
                                        snd.isWind = false;
                                    if (watch.ProfitFlg == false)
                                    {
                                        watch.ProfitFlg = true;
                                        //TransactionWatch.ErrorMessage("Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True");
                                        if (watch.posInt < 0)
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "|True|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True", Color.Red);
                                        }
                                        else
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_Profit|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False", Color.Red);
                                        }
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                    }
                                    watch.NotificationTimeProfit = currentTime + 1;
                                }
                            }
                        }
                    }
                }
                if (watch.DrawDown != 0)
                {
                    if (watch.Leg1.N_Qty != 0)
                    {
                        if (watch.posInt != 0)
                        {
                            if (watch.DrawDown <= (Math.Abs(watch.MaxPnl - watch.pnl)) / Math.Abs(watch.posInt))
                            {
                                UInt64 currentTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                if (watch.NotificationTimeDrawdown <= currentTime)
                                {
                                    if (watch.IsStrikeReq == false)
                                    {
                                        //MessageBox.Show("Please Strike Request first...");

                                        TransactionWatch.TransactionMessage("UniqueId |" + watch.uniqueId + "|Please Strike Request|", Color.Red);
                                        return;
                                    }

                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                    snd.TransCode = 10;
                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                    snd.UniqueID = unique;
                                    snd.gui_id = AppGlobal.GUI_ID;
                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                    snd.Open = 1;
                                    if (watch.posInt < 0)
                                        snd.isWind = true;
                                    else
                                        snd.isWind = false;
                                    if (watch.DrawDownFlg == false)
                                    {
                                        watch.DrawDownFlg = true;
                                        if (watch.posInt < 0)
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "|True|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|UnWindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True", Color.Red);
                                        }
                                        else
                                        {
                                            TransactionWatch.ErrorMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False|" + "|Profit|" + Math.Round(watch.Profit, 2) + "|DrawDown|" + Math.Round(watch.DrawDown, 2) + "|MaxPnl|" + Math.Round(watch.MaxPnl, 2) + "|PNL|" + Math.Round(watch.pnl, 2) + "|WindSpread|" + Math.Round(watch.MktWind, 2) + "|unWindSpread|" + Math.Round(watch.MktunWind, 2) + "|WindPos|" + watch.posInt + "|AvgSpread|" + Math.Round(watch.Leg1.N_Price, 2) + "|LotSize|" + watch.Leg1.ContDetail.LotSize + "|L1Bid|" + watch.Leg1.BuyPrice + "|L1Ask|" + watch.Leg1.SellPrice + "|L2Bid|" + watch.Leg2.BuyPrice + "|L2Ask|" + watch.Leg2.SellPrice + "|L3Bid|" + watch.Leg3.BuyPrice + "|L3Ask|" + watch.Leg3.SellPrice);
                                            TransactionWatch.TransactionMessage("Immidiate_send_DrawDownFlg|Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Unwind|" + "False", Color.Red);
                                        }
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);

                                    }
                                    watch.NotificationTimeDrawdown = currentTime + 1;
                                }
                            }
                        }
                    }
                }
                #endregion

                */
                #endregion
            }
        }

        #region ToolStrip Click Events
        void tlsmiActiveDeActive_Click(object sender, EventArgs e)
        {
            try
            {
                ActiveDeactiveScript(dgvMarketWatch.CurrentRow.Index, false);

                //lblNoOfActiveScript.Text = AppGlobal.ActiveScript.ToString();
                //lblNoOfDeActiveScript.Text = AppGlobal.DeActiveScript.ToString();
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "tlsmiActiveDeActive_Click")
                                , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        #endregion

        public void readFileExcelScrip(string file)
        {

            try
            {
                const char fieldSeparator = ',';
                using (StreamReader readFile = new StreamReader(file))
                {
                    string line;
                    int i = 0;
                    while ((line = readFile.ReadLine()) != null)
                    {
                        List<string> split = line.Split(fieldSeparator).ToList();
                        if (i == 2)
                            break;
                        foreach (string ln in split)
                        {

                            if (Convert.ToString(split[0].Trim()) == "Name")
                                continue;
                            else
                            {
                                AppGlobal.OverAllPnl = Convert.ToDouble(split[1]);
                                AppGlobal.Pnl = Convert.ToDouble(split[2]);
                            }
                        }
                        i++;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        #region Form Events

        private void FrmWatch_Load(object sender, EventArgs e)
        {
            try
            {
                Text = AppGlobal.Watch;
                GenerateColumns();
                tradeBookDataGrid1.DataSource = _tradeBookTable1;
                tradeBookDataGrid1.Columns[TradeConst.Time].Width = 80;
                tradeBookDataGrid1.Columns[TradeConst.uniqueId].Width = 50;
                tradeBookDataGrid1.Columns[TradeConst.Expiry].Width = 40;
                tradeBookDataGrid1.Columns[TradeConst.StrategyName].Width = 35;
                tradeBookDataGrid1.Columns[TradeConst.TrdPrice].Width = 40;
                tradeBookDataGrid1.Columns[TradeConst.L1Stk].Width = 40;
                tradeBookDataGrid1.Columns[TradeConst.L2Stk].Width = 40;
                tradeBookDataGrid1.Columns[TradeConst.L1Ser].Width = 30;
                tradeBookDataGrid1.Columns[TradeConst.UserRate].Width = 40;
                tradeBookDataGrid1.Columns[TradeConst.IsWind].Width = 50;
                dgvMarketWatch.MultiSelect = false;
                dgvMarketWatch.UniqueName = MTEnums.StrategyType.DeltaHedging.ToString();
                dgvMarketWatch.LoadSaveSettings();

                cmbStrategyName.Items.Clear();
                AllowedStrategy();
                ReadVersionFile();

                threeExpiry = GetExpiryDates(ArisApi_a._arisApi.DsContract.Tables["NSEFO"]);
                threeFinNiftyExpiry = GetFinNiftyExpiryDates(ArisApi_a._arisApi.DsContract.Tables["NSEFO"]);


                DateTime dt1 = Convert.ToDateTime(threeExpiry[0].ToString());
                DateTime dt2 = Convert.ToDateTime(threeExpiry[1].ToString());
                DateTime dt3 = Convert.ToDateTime(threeExpiry[2].ToString());
                AppGlobal.enterCount = 0;
                string filter2 = "GatewayId = 1";
                DataTable GatewayId = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                GatewayId.DefaultView.RowFilter = filter2;
                string filter3 = "InstrumentName='" + "OPTIDX" + "'";
                DataTable symbol = ArisApi_a._arisApi.DsContract.Tables["NseFo"];
                symbol.DefaultView.RowFilter = filter3;
                string filter = "InstrumentName='" + "OPTIDX" + "' AND Symbol = '" + "NIFTY" + "'";
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
                    AppGlobal.AllExpiry.Add(s5);
                }
                cmbExp.DataSource = exp2.DefaultView.ToTable(true, "ExpiryDate");
                cmbExp.DisplayMember = "ExpiryDate";

                for (int i = 0; i < dgvMarketWatch.Columns.Count - 1; i++)
                {
                    dgvMarketWatch.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgvMarketWatch.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                dgvMarketWatch.Columns[WatchConst.Avg_Theta].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvMarketWatch.Columns[WatchConst.Avg_ThetaV].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvMarketWatch.Columns[WatchConst.Avg_IV].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvMarketWatch.Columns[WatchConst.FutPrice].SortMode = DataGridViewColumnSortMode.NotSortable;

                for (int i = 0; i < dgvMarketWatch.Rows.Count - 1; i++)
                {
                    dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.Aqua;
                }
                tradeBookDataGrid1.ReadOnly = false;
                tradeBookDataGrid1.EditMode = DataGridViewEditMode.EditOnF2;
                for (int i = 0; i < tradeBookDataGrid1.Columns.Count - 1; i++)
                {
                    tradeBookDataGrid1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    tradeBookDataGrid1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                cmbStrategyName.SelectedIndex = 0;
                AppGlobal.ActiveScript = 0;
                AppGlobal.DeActiveScript = dgvMarketWatch.Rows.Count - 1;
                uint expiryfut = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[0]));
                string expiry1Fut = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, expiryfut).ToString("yyyyMMMdd");
                string sf12 = Convert.ToString(expiry1Fut);
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
                string BKStr = "InstrumentName='" + "FUTIDX" + "' AND Symbol = '" + "BANKNIFTY" + "' AND ExpiryDate = '" + sf52 + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(BKStr);
                foreach (DataRow dr in dr11)
                {
                    AppGlobal.BKToken = Convert.ToUInt64(dr["TokenNo"].ToString());
                    TransactionWatch.TransactionMessage("BK Token | " + AppGlobal.BKToken, Color.Red);
                }
                uint expiryfut2 = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[1]));
                string expiry1Fut2 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, expiryfut2).ToString("yyyyMMMdd");
                string sf122 = Convert.ToString(expiry1Fut2);
                string sf222 = sf122.Substring(0, 4);
                string sf322 = sf122.Substring(4, 3);
                string sf422 = sf122.Substring(7, 2);
                int montf2 = DateTime.ParseExact(sf322, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mffi12 = new System.Globalization.DateTimeFormatInfo();
                string monStringf2 = "";
                if (montf2 <= 9)
                {
                    monStringf2 = "0" + Convert.ToString(montf2);
                }
                else
                {
                    monStringf2 = Convert.ToString(montf2);
                }
                string sf522 = sf222 + monStringf2 + sf422;


                uint FinNiftyExpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeFinNiftyExpiry[0]));

                string expiry1Fin2 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, FinNiftyExpiry).ToString("yyyyMMMdd");
                string sf12Fin2 = Convert.ToString(expiry1Fin2);
                string sf22Fin2 = sf12Fin2.Substring(0, 4);
                string sf32Fin2 = sf12Fin2.Substring(4, 3);
                string sf42Fin2 = sf12Fin2.Substring(7, 2);
                int montfFin2 = DateTime.ParseExact(sf32Fin2, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo mffi1Fin2 = new System.Globalization.DateTimeFormatInfo();
                string monStringfFin2 = "";
                if (montfFin2 <= 9)
                {
                    monStringfFin2 = "0" + Convert.ToString(montfFin2);
                }
                else
                {
                    monStringfFin2 = Convert.ToString(montfFin2);
                }
                string sf52Fin2 = sf22Fin2 + monStringfFin2 + sf42Fin2;


                string FinNiftyStr = "InstrumentName='" + "FUTIDX" + "' AND Symbol = '" + "FINNIFTY" + "' AND ExpiryDate = '" + sf52Fin2 + "'";

                DataRow[] dr11Fin2 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(FinNiftyStr);
                foreach (DataRow dr in dr11Fin2)
                {
                    AppGlobal.FinNiftyToken = Convert.ToUInt64(dr["TokenNo"].ToString());
                    TransactionWatch.TransactionMessage("FN Token2 | " + AppGlobal.FinNiftyToken, Color.Red);
                }

                string BKStr2 = "InstrumentName='" + "FUTIDX" + "' AND Symbol = '" + "BANKNIFTY" + "' AND ExpiryDate = '" + sf522 + "'";
                DataRow[] dr112 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(BKStr2);
                foreach (DataRow dr in dr112)
                {
                    AppGlobal.BKToken2 = Convert.ToUInt64(dr["TokenNo"].ToString());
                    TransactionWatch.TransactionMessage("BK Token2 | " + AppGlobal.BKToken2, Color.Red);
                }
                string niftyStr = "InstrumentName='" + "FUTIDX" + "' AND Symbol = '" + "NIFTY" + "' AND ExpiryDate = '" + sf52 + "'";
                DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(niftyStr);
                foreach (DataRow dr in dr12)
                {
                    AppGlobal.NiftyToken = Convert.ToUInt64(dr["TokenNo"].ToString());
                    TransactionWatch.TransactionMessage("N Token | " + AppGlobal.NiftyToken, Color.Red);
                }
                string niftyStr2 = "InstrumentName='" + "FUTIDX" + "' AND Symbol = '" + "NIFTY" + "' AND ExpiryDate = '" + sf522 + "'";
                DataRow[] dr122 = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(niftyStr2);
                foreach (DataRow dr in dr122)
                {
                    AppGlobal.NiftyToken2 = Convert.ToUInt64(dr["TokenNo"].ToString());
                    TransactionWatch.TransactionMessage("N Token2 | " + AppGlobal.NiftyToken2, Color.Red);
                }
                if (ArisApi_a._arisApi.SystemConfig.Type == "UDP")
                {
                    ArisApi_a._arisApi._nseCmBroadcastConnection = new AppClasses.NseCmBroadcastConnection();
                    ArisApi_a._arisApi._nseFoBroadcastConnection = new AppClasses.NseFoBroadcastConnection();
                    TransactionWatch.TransactionMessage("Connected with UDP Connection", Color.Blue);
                }
                AppGlobal.EnterLots = Convert.ToInt32(ArisApi_a._arisApi.SystemConfig.EnterLots);
                //for Testing 
               
                if (ArisApi_a._arisApi.SystemConfig.RmsConnect == false)
                {
                    AppGlobal.MarketWatch = MarketWatch.ReadXmlProfile();
                    AssignMarketStructValue(AppGlobal.MarketWatch);

                    LSL_Strangle_AvgPrice();
                    ArisApi_a._arisApi.GenerateTradeFiles();

                    back_Files();
                    lblMargin.Text = Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 2).ToString();

                    //ArisApi_a._arisApi.OnMarketDepthUpdate += new ArisApi_a.MarketDepthUpdateDelegate(_arisApi_OnMarketDepthUpdate);
                    //ArisApi_a._arisApi.OnIndexBroadCast += new ArisApi_a.IndexBroadCastUpdateDelegate(_arisApi_OnIndexBroadCast);

                    BindBroadcastEvents();
                   

                    lblcallbuy.Text = Math.Round(AppGlobal.CallBuyMTM, 2).ToString();
                    lblcallsell.Text = Math.Round(AppGlobal.CallSellMTM, 2).ToString();

                    lblputbuy.Text = Math.Round(AppGlobal.PutBuyMTM, 2).ToString();
                    lblputsell.Text = Math.Round(AppGlobal.PutSellMTM, 2).ToString();

                    CallMTM.Text = (Math.Round(AppGlobal.CallBuyMTM, 2) + Math.Round(AppGlobal.PutBuyMTM, 2)).ToString();
                    PutMTM.Text = (Math.Round(AppGlobal.CallSellMTM, 2) + Math.Round(AppGlobal.PutSellMTM, 2)).ToString();
                    MatchUniqueNo();
                }

            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "FrmWatch_Load")
                                 , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        void ReadVersionFile()
        {
            string Version = "";
            string VersionFile = AppDomain.CurrentDomain.BaseDirectory + "\\" + "Version.txt";
            if (File.Exists(VersionFile))
            {
                using (StreamReader sr = File.OpenText(VersionFile))
                {
                    string s = String.Empty;
                    while ((s = sr.ReadLine()) != null)
                    {
                        //do minimal amount of work here
                        Version = s.ToString();

                    }
                }
            }
            lblVersion.Text = Version.ToString();
        }

        void AllowedStrategy()
        {
            string all_strategy = ArisApi_a._arisApi.SystemConfig.AllowStrategy.ToString();
            string[] strategy = all_strategy.Split(',');
            for (int i = 0; i < strategy.Count(); i++)
            {
                if (strategy[i] == "91")
                {
                    cmbStrategyName.Items.Add("Single");
                }
                else if (strategy[i] == "2211")
                {
                    cmbStrategyName.Items.Add("Strangle");
                }
                else if (strategy[i] == "3311")
                {
                    cmbStrategyName.Items.Add("MainStraddle");
                }
                else if (strategy[i] == "12211")
                {
                    cmbStrategyName.Items.Add("TLI_Strangle");
                }
                else if (strategy[i] == "32211")
                {
                    cmbStrategyName.Items.Add("LSL_Strangle");
                }
                else if(strategy[i] == "1113")
                {
                    cmbStrategyName.Items.Add("TLI_CE_Calender");
                }
                else if (strategy[i] == "1114")
                {
                    cmbStrategyName.Items.Add("TLI_PE_Calender");
                }



            }
            cmbStrategyName.Items.Add("Empty");
        }

        //void _arisApi_OnIndexBroadCast(ArisDev.NseCmApi.Broadcast.Indices _response)
        //{
        //    if (InvokeRequired)
        //        BeginInvoke((MethodInvoker)(() => _arisApi_OnIndexBroadCast(_response)));
        //    else
        //    {
        //        try
        //        {
        //            char[] Sym = _response.IndexName.ToCharArray();
        //            string SYM = new string(Sym);
        //            if (SYM.Trim() == "Nifty 50")
        //            {
        //                AppGlobal.SpotNifty = (Convert.ToDouble(_response.IndexValue) / 100);
        //                lblcashNifty.Text = (Convert.ToDouble(_response.IndexValue) / 100).ToString();
        //                if (AppGlobal.LastSpotPrice < AppGlobal.SpotNifty)
        //                {
        //                    AppGlobal.LastSpotPrice = AppGlobal.SpotNifty + (AppGlobal.SpotNifty * 0.005);
        //                    SendToTradeAdmin("Spot");
        //                }
        //            }
        //            else if (SYM.Trim() == "Nifty Bank")
        //            {
        //                lblcashbk.Text = (Convert.ToDouble(_response.IndexValue) / 100).ToString();
        //            }
        //            if (SYM.Trim() == "Nifty Fin Service")
        //            {
        //                lblFinNiftySpot.Text = (Convert.ToDouble(_response.IndexValue) / 100).ToString();
        //            }
        //            if (SYM.Trim() == "India VIX")
        //            {
        //                txtVIX.Text = (Math.Round(Convert.ToDouble(_response.IndexValue) / 100, 2)).ToString();
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //}

        public void Sum()
        {
            AppGlobal.Flags = true;
            foreach (var kvp in AppGlobal.RuleMap.Keys)
            {
                MarketWatch watch = new MarketWatch();
                double totalPnl = 0;
                double totalDelta = 0;
                double totalVega = 0;
                double totalGamma = 0;
                double totalTheta = 0;
                double totalSqPnl = 0;
                double totalAvgTheta = 0;
                double totalPremium = 0;
                double totalLivePremium = 0;

                totalPnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.pnl).Sum();
                totalDelta = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.sumDelta).Sum();
                totalGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.sumGamma).Sum();
                totalTheta = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.sumTheta).Sum();
                totalVega = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.sumVega).Sum();
                totalSqPnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp).Select(x => x.Sqpnl).Sum();
                totalAvgTheta = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.Avg_ThetaV).Sum();
                totalPremium = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.premium).Sum();
                totalLivePremium = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.LivePremium).Sum();

                AppGlobal.RuleMap[kvp].RulePnl = totalPnl;
                AppGlobal.RuleMap[kvp].RuleDelta = totalDelta;
                AppGlobal.RuleMap[kvp].RuleGamma = totalGamma;
                AppGlobal.RuleMap[kvp].RuleVega = totalVega;
                AppGlobal.RuleMap[kvp].RuleTheta = totalTheta;
                AppGlobal.RuleMap[kvp].RuleSqPnl = totalSqPnl;
                AppGlobal.RuleMap[kvp].avgTheta = totalAvgTheta;
                AppGlobal.RuleMap[kvp].Premium = totalPremium;
                AppGlobal.RuleMap[kvp].LivePremium = totalLivePremium;
            }
            MarketWatch _watch = new MarketWatch();
            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                _watch = AppGlobal.MarketWatch[i];
                if (_watch.StrategyId == 0)
                {
                    if (AppGlobal.RuleMap.ContainsKey(_watch.Strategy))
                    {
                        _watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RulePnl, 2);
                        _watch.pnl = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RulePnl, 2);
                        _watch.RowData.Cells[WatchConst.DeltaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleDelta, 4);
                        _watch.DeltaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleDelta, 4);
                        _watch.RowData.Cells[WatchConst.VegaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleVega, 4);
                        _watch.VegaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleVega, 4);
                        _watch.RowData.Cells[WatchConst.GammaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleGamma, 4);
                        _watch.GammaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleGamma, 4);
                        _watch.RowData.Cells[WatchConst.ThetaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleTheta, 4);
                        _watch.ThetaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleTheta, 4);
                        _watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleSqPnl, 2);
                        _watch.Sqpnl = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleSqPnl, 2);
                        _watch.StrategyPnl = AppGlobal.RuleMap[_watch.Strategy].RulePnl + AppGlobal.RuleMap[_watch.Strategy].RuleSqPnl + _watch.CarryForwardPnl;
                        _watch.RowData.Cells[WatchConst.StrategyPnl].Value = Math.Round(_watch.StrategyPnl, 2);
                        _watch.RowData.Cells[WatchConst.Avg_ThetaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].avgTheta, 4);
                        _watch.Avg_ThetaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].avgTheta, 4);
                        _watch.RowData.Cells[WatchConst.Premium].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].Premium, 2);
                        _watch.premium = Math.Round(AppGlobal.RuleMap[_watch.Strategy].Premium, 2);
                        _watch.RowData.Cells[WatchConst.LivePremium].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].LivePremium, 2);
                        _watch.LivePremium = Math.Round(AppGlobal.RuleMap[_watch.Strategy].LivePremium, 2);
                        if (_watch.SqTimeflg)
                        {
                            UInt64 nowTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                            UInt64 uintTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(_watch.SqTime));
                            if (uintTime < nowTime)
                            {
                                _watch.SqTimeflg = false;
                                strategy_sqoff_Time(_watch);
                            }
                        }
                        //if (_watch.SQVegaflg)
                        //{
                        //    if (_watch.VegaV < _watch.SQVegaPrice)
                        //    {
                        //        _watch.SQVegaflg = false;
                        //        _watch.SQPremiumflg = false;
                        //        _watch.SQLossflg = false;
                        //        _watch.SqTimeflg = false;
                        //        TransactionWatch.ErrorMessage("StrategyVegaSqOff|" + _watch.Strategy + "|"
                        //                + _watch.VegaV + "|" + _watch.SQVegaPrice + "|" + _watch.Per_SQVegaPrice);
                        //        foreach (var _watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == _watch.Strategy) && (x.StrategyId == 91)))
                        //        {
                        //            _watch1.SQVegaflg = false;
                        //            _watch1.SQPremiumflg = false;
                        //            _watch1.SQLossflg = false;
                        //            SqoffAll(_watch1, "StrategyVegaSqoff");
                        //        }
                        //    }
                        //}
                        //if (_watch.SQPremiumflg)
                        //{
                        //    if (_watch.SQPremiumPrice < 0)
                        //    {
                        //        if (_watch.SQPremiumPrice > _watch.LivePremium)
                        //        {
                        //            _watch.SQPremiumflg = false;
                        //            _watch.SQVegaflg = false;
                        //            _watch.SQLossflg = false;
                        //            _watch.SqTimeflg = false;
                        //            TransactionWatch.ErrorMessage("StrategyPremiumSqOff|" + _watch.Strategy + "|" + _watch.premium + "|" + _watch.LivePremium + "|" + _watch.SQPremiumPrice + "|" + _watch.Per_SQPremiumPrice);
                        //            foreach (var _watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == _watch.Strategy) && (x.StrategyId == 91)))
                        //            {
                        //                _watch1.SQPremiumflg = false;
                        //                _watch1.SQVegaflg = false;
                        //                _watch1.SQLossflg = false;
                        //                SqoffAll(_watch1, "StrategyPremiumSqOff");
                        //            }
                        //        }
                        //    }
                        //    else 
                        //    {
                        //        if (_watch.SQPremiumPrice < _watch.LivePremium)
                        //        {

                        //            _watch.SQPremiumflg = false;
                        //            _watch.SQVegaflg = false;
                        //            _watch.SQLossflg = false;
                        //            TransactionWatch.ErrorMessage("StrategyPremiumSqOff|" + _watch.Strategy + "|" + _watch.premium + "|" + _watch.LivePremium + "|" + _watch.SQPremiumPrice + "|" + _watch.Per_SQPremiumPrice);
                        //            foreach (var _watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == _watch.Strategy) && (x.StrategyId == 91)))
                        //            {
                        //                _watch1.SQPremiumflg = false;
                        //                _watch1.SQVegaflg = false;
                        //                _watch1.SQLossflg = false;
                        //                SqoffAll(_watch1, "StrategyPremiumSqOff");
                        //            }
                        //        }
                        //    }
                        //}
                        //if (_watch.SQLossflg)
                        //{
                        //    if (_watch.SQLossPrice > _watch.pnl)
                        //    {
                        //        _watch.SQLossflg = false;
                        //        _watch.SqTimeflg = false;
                        //        _watch.SQPremiumflg = false;
                        //        _watch.SQVegaflg = false;
                        //        TransactionWatch.ErrorMessage("StrategyLossOff|" + _watch.Strategy + "|" + _watch.pnl + "|" + _watch.SQLossPrice + "|" + _watch.SQLossPoint + "|" + _watch.Per_SQLossPrice);
                        //        foreach (var _watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == _watch.Strategy) && (x.StrategyId == 91)))
                        //        {
                        //            _watch1.SQPremiumflg = false;
                        //            _watch1.SQVegaflg = false;
                        //            _watch1.SQLossflg = false;
                        //            _watch1.SqTimeflg = false;
                        //            SqoffAll(_watch1, "StrategyLossOff");
                        //        }
                        //    }
                        //}
                    }
                }
            }
            AppGlobal.Flags = false;
        }

        public void _Sum()
        {
           
            foreach (var kvp in AppGlobal.RuleMap.Keys)
            {
                MarketWatch watch = new MarketWatch();
                double totalPnl = 0;
                double totalDelta = 0;
                double totalVega = 0;
                double totalGamma = 0;
                double totalTheta = 0;
                double totalSqPnl = 0;
                double totalAvgTheta = 0;
                double totalPremium = 0;
                double totalLivePremium = 0;

                totalPnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.pnl).Sum();
                totalDelta = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.sumDelta).Sum();
                totalGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.sumGamma).Sum();
                totalTheta = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.sumTheta).Sum();
                totalVega = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.sumVega).Sum();
                totalSqPnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp).Select(x => x.Sqpnl).Sum();
                totalAvgTheta = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.Avg_ThetaV).Sum();
                totalPremium = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.premium).Sum();
                totalLivePremium = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp && x.posInt != 0).Select(x => x.LivePremium).Sum();

                AppGlobal.RuleMap[kvp].RulePnl = totalPnl;
                AppGlobal.RuleMap[kvp].RuleDelta = totalDelta;
                AppGlobal.RuleMap[kvp].RuleGamma = totalGamma;
                AppGlobal.RuleMap[kvp].RuleVega = totalVega;
                AppGlobal.RuleMap[kvp].RuleTheta = totalTheta;
                AppGlobal.RuleMap[kvp].RuleSqPnl = totalSqPnl;
                AppGlobal.RuleMap[kvp].avgTheta = totalAvgTheta;
                AppGlobal.RuleMap[kvp].Premium = totalPremium;
                AppGlobal.RuleMap[kvp].LivePremium = totalLivePremium;
            }
            MarketWatch _watch = new MarketWatch();
            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                _watch = AppGlobal.MarketWatch[i];
                if (_watch.StrategyId == 0)
                {
                    if (AppGlobal.RuleMap.ContainsKey(_watch.Strategy))
                    {
                        _watch.RowData.Cells[WatchConst.PNL].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RulePnl, 2);
                        _watch.pnl = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RulePnl, 2);
                        _watch.RowData.Cells[WatchConst.DeltaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleDelta, 4);
                        _watch.DeltaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleDelta, 4);
                        _watch.RowData.Cells[WatchConst.VegaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleVega, 4);
                        _watch.VegaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleVega, 4);
                        _watch.RowData.Cells[WatchConst.GammaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleGamma, 4);
                        _watch.GammaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleGamma, 4);
                        _watch.RowData.Cells[WatchConst.ThetaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleTheta, 4);
                        _watch.ThetaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleTheta, 4);
                        _watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleSqPnl, 2);
                        _watch.Sqpnl = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleSqPnl, 2);
                        _watch.StrategyPnl = AppGlobal.RuleMap[_watch.Strategy].RulePnl + AppGlobal.RuleMap[_watch.Strategy].RuleSqPnl + _watch.CarryForwardPnl;
                        _watch.RowData.Cells[WatchConst.StrategyPnl].Value = Math.Round(_watch.StrategyPnl, 2);
                        _watch.RowData.Cells[WatchConst.Avg_ThetaV].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].avgTheta, 4);
                        _watch.Avg_ThetaV = Math.Round(AppGlobal.RuleMap[_watch.Strategy].avgTheta, 4);
                        _watch.RowData.Cells[WatchConst.Premium].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].Premium, 2);
                        _watch.premium = Math.Round(AppGlobal.RuleMap[_watch.Strategy].Premium, 2);
                        _watch.RowData.Cells[WatchConst.LivePremium].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].LivePremium, 2);
                        _watch.LivePremium = Math.Round(AppGlobal.RuleMap[_watch.Strategy].LivePremium, 2);
                        //if (_watch.SqTimeflg)
                        //{
                        //    UInt64 nowTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                        //    UInt64 uintTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(_watch.SqTime));
                        //    if (uintTime < nowTime)
                        //    {
                        //        _watch.SqTimeflg = false;
                        //        strategy_sqoff_Time(_watch);
                        //    }
                        //}
                        //if (_watch.SQVegaflg)
                        //{
                        //    if (_watch.VegaV < _watch.SQVegaPrice)
                        //    {
                        //        _watch.SQVegaflg = false;
                        //        _watch.SQPremiumflg = false;
                        //        _watch.SQLossflg = false;
                        //        _watch.SqTimeflg = false;
                        //        TransactionWatch.ErrorMessage("StrategyVegaSqOff|" + _watch.Strategy + "|"
                        //                + _watch.VegaV + "|" + _watch.SQVegaPrice + "|" + _watch.Per_SQVegaPrice);
                        //        foreach (var _watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == _watch.Strategy) && (x.StrategyId == 91)))
                        //        {
                        //            _watch1.SQVegaflg = false;
                        //            _watch1.SQPremiumflg = false;
                        //            _watch1.SQLossflg = false;
                        //            SqoffAll(_watch1, "StrategyVegaSqoff");
                        //        }
                        //    }
                        //}
                        //if (_watch.SQPremiumflg)
                        //{
                        //    if (_watch.SQPremiumPrice < 0)
                        //    {
                        //        if (_watch.SQPremiumPrice > _watch.LivePremium)
                        //        {
                        //            _watch.SQPremiumflg = false;
                        //            _watch.SQVegaflg = false;
                        //            _watch.SQLossflg = false;
                        //            _watch.SqTimeflg = false;
                        //            TransactionWatch.ErrorMessage("StrategyPremiumSqOff|" + _watch.Strategy + "|" + _watch.premium + "|" + _watch.LivePremium + "|" + _watch.SQPremiumPrice + "|" + _watch.Per_SQPremiumPrice);
                        //            foreach (var _watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == _watch.Strategy) && (x.StrategyId == 91)))
                        //            {
                        //                _watch1.SQPremiumflg = false;
                        //                _watch1.SQVegaflg = false;
                        //                _watch1.SQLossflg = false;
                        //                SqoffAll(_watch1, "StrategyPremiumSqOff");
                        //            }
                        //        }
                        //    }
                        //    else 
                        //    {
                        //        if (_watch.SQPremiumPrice < _watch.LivePremium)
                        //        {

                        //            _watch.SQPremiumflg = false;
                        //            _watch.SQVegaflg = false;
                        //            _watch.SQLossflg = false;
                        //            TransactionWatch.ErrorMessage("StrategyPremiumSqOff|" + _watch.Strategy + "|" + _watch.premium + "|" + _watch.LivePremium + "|" + _watch.SQPremiumPrice + "|" + _watch.Per_SQPremiumPrice);
                        //            foreach (var _watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == _watch.Strategy) && (x.StrategyId == 91)))
                        //            {
                        //                _watch1.SQPremiumflg = false;
                        //                _watch1.SQVegaflg = false;
                        //                _watch1.SQLossflg = false;
                        //                SqoffAll(_watch1, "StrategyPremiumSqOff");
                        //            }
                        //        }
                        //    }
                        //}
                        //if (_watch.SQLossflg)
                        //{
                        //    if (_watch.SQLossPrice > _watch.pnl)
                        //    {
                        //        _watch.SQLossflg = false;
                        //        _watch.SqTimeflg = false;
                        //        _watch.SQPremiumflg = false;
                        //        _watch.SQVegaflg = false;
                        //        TransactionWatch.ErrorMessage("StrategyLossOff|" + _watch.Strategy + "|" + _watch.pnl + "|" + _watch.SQLossPrice + "|" + _watch.SQLossPoint + "|" + _watch.Per_SQLossPrice);
                        //        foreach (var _watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == _watch.Strategy) && (x.StrategyId == 91)))
                        //        {
                        //            _watch1.SQPremiumflg = false;
                        //            _watch1.SQVegaflg = false;
                        //            _watch1.SQLossflg = false;
                        //            _watch1.SqTimeflg = false;
                        //            SqoffAll(_watch1, "StrategyLossOff");
                        //        }
                        //    }
                        //}
                    }
                }
            }
            AppGlobal.Flags = false;
        }

        public void strategy_sqoff_Time(MarketWatch watch)
        {
            foreach (var _watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch.Strategy) && (x.StrategyId == 91)))
            {
                _watch.SqTimeflg = false;
                SqoffAll(_watch,"StrategySqoff");
            } 
        }

        void AvgCalculatedGreek(MarketWatch watch)
        {
            if (watch.StrategyId == 91)
            {
                if (watch.posInt != 0)
                {
                    #region Avg Greeks
                    int expiry1 = Convert.ToInt32(watch.Leg1.expiryUniqueID);
                    double timeToExpiry1 = CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, expiry1));
                    string today1 = DateTime.Now.ToString("ddMMMyyyy");
                    GreeksVariable stk = new GreeksVariable();
                    stk.SpotPrice = Convert.ToDouble(watch.niftyLeg.LastTradedPrice);
                    stk.IntrestRate = 0;
                    stk.StrikePrice = (double)(Convert.ToDecimal(watch.Leg1.ContractInfo.StrikePrice));
                    double dates = timeToExpiry1;
                    if (watch.Expiry == today1)
                    {
                        stk.TimeToExpiry = 0.5;
                        dates = 1;
                    }
                    else
                    {
                        if (timeToExpiry1 >= 1.0)
                        {
                            stk.TimeToExpiry = timeToExpiry1 + 1;//CalculatorUtils.CalculateDay(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, watch.Leg1.ContractInfo.ExpiryDate));
                            dates = stk.TimeToExpiry;
                        }
                        else
                            stk.TimeToExpiry = 0.50;
                    }
                    stk.DividentYield = 0;
                    stk.ActualValue = (double)watch.Leg1.N_Price;
                    if (watch.Leg1.ContractInfo.Series == "CE")
                    {
                        stk.Volatility = Convert.ToDouble(CalculatorUtils.CallVolatility(stk));
                        watch.Avg_IV = Math.Round(stk.Volatility, 4);
                        watch.RowData.Cells[WatchConst.Avg_IV].Value = watch.Avg_IV;
                        if (watch.Leg1.ContractInfo.StrikePrice < watch.niftyLeg.LastTradedPrice)
                        {
                            double ITMPrice = Math.Abs(Convert.ToDouble(watch.Leg1.ContractInfo.StrikePrice - watch.niftyLeg.LastTradedPrice));
                            if (watch.Leg1.N_Price > ITMPrice)
                            {
                                watch.Avg_Theta = Math.Round(Convert.ToDouble(CalculatorUtils.CallTheta(stk)), 4);
                            }
                            else
                            {
                                watch.Avg_Theta = (ITMPrice - watch.Leg1.N_Price) / dates;
                            }
                        }
                        else
                        {
                            watch.Avg_Theta = Math.Round(Convert.ToDouble(CalculatorUtils.CallTheta(stk)), 4);
                        }
                    }
                    else if (watch.Leg1.ContractInfo.Series == "PE")
                    {
                        stk.Volatility = Convert.ToDouble(CalculatorUtils.PutVolatility(stk));
                        watch.Avg_IV = Math.Round(stk.Volatility, 4);
                        watch.RowData.Cells[WatchConst.Avg_IV].Value = watch.Avg_IV;

                        if (watch.Leg1.ContractInfo.StrikePrice > watch.niftyLeg.LastTradedPrice)
                        {
                            double ITMPrice = Math.Abs(Convert.ToDouble(watch.Leg1.ContractInfo.StrikePrice - watch.niftyLeg.LastTradedPrice));
                            if (watch.Leg1.N_Price > ITMPrice)
                            {
                                watch.Avg_Theta = Math.Round(Convert.ToDouble(CalculatorUtils.PutTheta(stk)), 4);
                            }
                            else
                            {
                                watch.Avg_Theta = (ITMPrice - watch.Leg1.N_Price) / dates;
                            }
                        }
                        else
                        {
                            watch.Avg_Theta = Math.Round(Convert.ToDouble(CalculatorUtils.PutTheta(stk)), 4);
                        }
                    }
                    else
                    {
                        watch.Avg_Theta = 0;
                    }
                    if (stk.TimeToExpiry <= 4)
                    {
                        double thetacal = (double)watch.Leg1.LastTradedPrice * 0.60;
                        if (thetacal < Math.Abs(watch.Theta))
                        {
                            if (stk.TimeToExpiry > 1)
                                watch.Avg_Theta = Math.Round(((((double)watch.Leg1.LastTradedPrice / stk.TimeToExpiry) + Math.Abs(watch.Theta)) / 2), 4) * -1;
                            else
                                watch.Avg_Theta = Math.Round((double)watch.Leg1.LastTradedPrice / stk.TimeToExpiry, 4) * -1;
                        }
                    }
                    watch.RowData.Cells[WatchConst.Avg_Theta].Value = Math.Round(watch.Avg_Theta, 4);
                    watch.Avg_ThetaV = watch.Avg_Theta * watch.posInt * watch.Leg1.ContDetail.LotSize;
                    watch.RowData.Cells[WatchConst.Avg_ThetaV].Value = Math.Round(watch.Avg_ThetaV, 4);
                    #endregion
                }
            }
        }

        public void LevelWiseTrade(MarketWatch watch)
        {
            if (watch.iteratorflg)
            {
                int iteratorCount = Convert.ToInt32(watch.iteratorCount);
                double TradePrice = Convert.ToDouble(watch._inputParameter[iteratorCount].Price);
                int TradeQty = Convert.ToInt32(watch._inputParameter[iteratorCount].Lots);
                if (watch.iteratorSide == "BUY")
                {
                    if (TradePrice > Math.Abs(watch.MktunWind))
                    {                       
                        TransactionWatch.ErrorMessage("Iterator|UniqueId" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|iterator|" + watch.iteratorCount + "|TotalIterator|" + watch.iterator + "|TradePrice|" + TradePrice
                                        + "|TradeQty|" + TradeQty + "|Side|" + watch.iteratorSide + "|wind|" + watch.MktWind + "|Unwind|" + watch.MktunWind + "|CurrentIterator|" + watch.iteratorCount);
                        Thread _trade = new Thread(() =>
                        {
                            
                            for (int trade = 0; trade < TradeQty; trade++)
                            {
                                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                snd.TransCode = 10;
                                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                snd.UniqueID = unique;
                                snd.gui_id = AppGlobal.GUI_ID;
                                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                snd.isWind = true;
                                snd.Open = 0;

                                long seq = ClassDisruptor.ringBufferRequest.Next();
                                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                ClassDisruptor.ringBufferRequest.Publish(seq);
                                
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(50);
                                TransactionWatch.ErrorMessage("TradeUniqueId|" + watch.uniqueId + "|strategy|" + watch.StrategyId + "|wind|" + trade);
                            }
                        });
                        _trade.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta     
                        _trade.Start();
                        watch._inputParameter[watch.iteratorCount].Lots = 0; 
                        watch._inputParameter[watch.iteratorCount].Price = 0;
                        watch._inputParameter[watch.iteratorCount].flg = true;
                        watch.RowData.Cells[WatchConst.LevelIterator].Value = TradePrice;
                        watch.iteratorCount++;
                        if (watch.iteratorCount == (watch.iterator))
                        {
                            watch.iteratorflg = false;
                            watch.iterator = 0;
                            watch.iteratorSide = "None";
                            watch.iteratorCount = 0;
                            watch.RowData.Cells[WatchConst.LevelIterator].Value = 0;
                            watch._inputParameter = null;
                        }   
                        TransactionWatch.TransactionMessage("Iterator|UniqueId" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|iterator|" + watch.iteratorCount + "|TotalIterator|" + watch.iterator + "|TradePrice|" + TradePrice
                                        + "|TradeQty|" + TradeQty + "|Side|" + watch.iteratorSide + "|wind|" + watch.MktWind + "|Unwind|" + watch.MktunWind + "|CurrentIterator|" + watch.iteratorCount,Color.Red);                                          
                    }
                }
                else if(watch.iteratorSide == "SELL")
                {
                    if (TradePrice < Math.Abs(watch.MktWind))
                    {
                        if (watch.iteratorCount == (watch.iterator - 1))    
                        {
                            watch.iteratorflg = false;
                        }
                        TransactionWatch.ErrorMessage("Iterator|UniqueId" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|iterator|" + watch.iteratorCount + "|TotalIterator|" + watch.iterator + "|TradePrice|" + TradePrice
                                        + "|TradeQty|" + TradeQty + "|Side|" + watch.iteratorSide + "|wind|" + watch.MktWind + "|Unwind|" + watch.MktunWind + "|CurrentIterator|" + watch.iteratorCount);
                        Thread _trade = new Thread(() =>
                        {
                            for (int trade = 0; trade < TradeQty; trade++)
                            {
                                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                snd.TransCode = 10;
                                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                snd.UniqueID = unique;
                                snd.gui_id = AppGlobal.GUI_ID;
                                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                snd.isWind = false;
                                snd.Open = 0;

                                long seq = ClassDisruptor.ringBufferRequest.Next();
                                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                ClassDisruptor.ringBufferRequest.Publish(seq);
                                TransactionWatch.ErrorMessage("TradeUniqueId|" + watch.uniqueId + "|strategy|" + watch.StrategyId + "|unwind|" + trade);
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(50);
                            }
                        });
                        _trade.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta     
                        _trade.Start();
                        watch._inputParameter[watch.iteratorCount].Lots = 0;
                        watch._inputParameter[watch.iteratorCount].Price = 0;
                        watch._inputParameter[watch.iteratorCount].flg = true;
                        watch.RowData.Cells[WatchConst.LevelIterator].Value = TradePrice;
                        watch.iteratorCount++;
                       
                        if (watch.iteratorCount == (watch.iterator))
                        {
                            watch.iteratorflg = false; 
                            watch.iterator = 0;
                            watch.iteratorSide = "None";
                            watch.iteratorCount = 0;
                            watch.RowData.Cells[WatchConst.LevelIterator].Value = watch.iteratorCount;
                            watch._inputParameter = null;
                        }   
                        TransactionWatch.TransactionMessage("Iterator|UniqueId" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|iterator|" + watch.iteratorCount + "|TotalIterator|" + watch.iterator + "|TradePrice|" + TradePrice
                                        + "|TradeQty|" + TradeQty + "|Side|" + watch.iteratorSide + "|wind|" + watch.MktWind + "|Unwind|" + watch.MktunWind + "|CurrentIterator|" + watch.iteratorCount,Color.Red);
                    }
                }
            } 
        }


        //void _arisApi_OnMarketDepthUpdate(MTApi.MTBCastPackets.MarketPicture _response)
        //{
        //    if (InvokeRequired)
        //        BeginInvoke((MethodInvoker)(() => _arisApi_OnMarketDepthUpdate(_response)));
        //    else
        //    {
        //        try
        //        {
        //            if (AppGlobal.NiftyToken == Convert.ToUInt64(_response.TokenNo))
        //            {
        //                txtNiftyValue.Text = (Convert.ToDecimal(_response.LastTradedPrice) / 100).ToString();
        //                if (Convert.ToDouble(txtNiftyValue.Text) != 0 && Convert.ToDouble(lblcashNifty.Text) != 0)
        //                {
        //                    double diff = Convert.ToDouble(txtNiftyValue.Text) - Convert.ToDouble(lblcashNifty.Text);
        //                    txtDiffNifty.Text = Convert.ToString(Math.Round(diff, 2));
        //                }
        //                if (AppGlobal.Flags == false)
        //                    Sum();
        //            }
        //            if (AppGlobal.BKToken == Convert.ToUInt64(_response.TokenNo))
        //            {
        //                txtbankValue.Text = (Convert.ToDecimal(_response.LastTradedPrice) / 100).ToString();
        //                if (Convert.ToDouble(txtbankValue.Text) != 0 && Convert.ToDouble(lblcashbk.Text) != 0)
        //                {
        //                    double diff = Convert.ToDouble(txtbankValue.Text) - Convert.ToDouble(lblcashbk.Text);
        //                    txtDiffBk.Text = Convert.ToString(Math.Round(diff, 2));
        //                }
        //                if (AppGlobal.Flags == false)
        //                    Sum();
        //            }

        //            if (AppGlobal.FinNiftyToken == Convert.ToUInt64(_response.TokenNo))
        //            {
        //                lblFinNiftyFut.Text = (Convert.ToDecimal(_response.LastTradedPrice) / 100).ToString();
        //            }

        //            #region Leg1
        //            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Leg1.ContractInfo.TokenNo) == _response.TokenNo)))
        //            {
        //                int i = watch.RowData.Index;
        //                if (watch.Leg1.BuyPrice != 0)

        //                    watch.Leg1.OldBuyPrice = watch.Leg1.BuyPrice;
        //                if (watch.Leg1.SellPrice != 0)
        //                    watch.Leg1.OldSellPrice = watch.Leg1.SellPrice;
        //                watch.Leg1.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
        //                watch.Leg1.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
        //                watch.Leg1.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
        //                watch.Leg1.MidPrice = Math.Round(Convert.ToDouble((watch.Leg1.BuyPrice + watch.Leg1.SellPrice) / 2), 2);
        //                watch.RowData.Cells[WatchConst.L1buyPrice].Value = watch.Leg1.BuyPrice;
        //                watch.RowData.Cells[WatchConst.L1sellPrice].Value = watch.Leg1.SellPrice;

        //                watch.Leg1.ATP = Convert.ToDouble(_response.AverageTradedPrice) / 100;
        //                watch.RowData.Cells[WatchConst.ATP].Value = Math.Round(watch.Leg1.ATP, 2);
        //                AppGlobal.Pnl = AppGlobal.MarketWatch.Where(x => x.posInt != 0).Select(item => item.pnl).Sum();
        //                lblPnl.Text = Math.Round(AppGlobal.Pnl, 2).ToString();
        //                if (AppGlobal.Pnl != 0)
        //                {
        //                    if (AppGlobal.LastPnl == 0)
        //                    {
        //                        AppGlobal.LastPnl = AppGlobal.Pnl;
        //                        SendToTradeAdmin("LastPnl");
        //                    }
        //                    else
        //                    {
        //                        if (AppGlobal.Pnl < (AppGlobal.LastPnl - ArisApi_a._arisApi.SystemConfig.LossPoints))
        //                        {
        //                            SendToTradeAdmin("LastPnl");
        //                            AppGlobal.LastPnl = AppGlobal.Pnl;
        //                        }
        //                    }
        //                }
        //                AppGlobal.Delta = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.sumDelta).Sum();
        //                lblDelta.Text = Math.Round(AppGlobal.Delta, 4).ToString();
        //                AppGlobal.Vega = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.sumVega).Sum();
        //                lblVega.Text = Math.Round(AppGlobal.Vega, 4).ToString();
        //                AppGlobal.Theta = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.sumTheta).Sum();
        //                lblTheta.Text = Math.Round(AppGlobal.Theta, 4).ToString();
        //                AppGlobal.Gamma = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.sumGamma).Sum();
        //                lblGamma.Text = Math.Round(AppGlobal.Gamma, 4).ToString();
        //                AppGlobal.upSideCallGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "CE").Select(x => x.sumGamma).Sum();
        //                AppGlobal.upSidePutGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "PE").Select(x => x.sumGamma).Sum();
        //                AppGlobal.downSideCallGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "CE").Select(x => x.sumGamma).Sum();
        //                AppGlobal.downSidePutGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "PE").Select(x => x.sumGamma).Sum();
        //                txtUpSideGamma.Text = (AppGlobal.upSideCallGamma + (AppGlobal.upSidePutGamma * -1)).ToString();
        //                txtdownSideGamma.Text = ((AppGlobal.downSideCallGamma * -1) + AppGlobal.downSidePutGamma).ToString();

        //                if (AppGlobal.Record)
        //                {
        //                    if (watch.uniqueId == AppGlobal.RuleRecord)
        //                    {
        //                        TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg1.BuyPrice + "|SellPrice|" + watch.Leg1.SellPrice);
        //                    }
        //                }
        //                CalculateGreek(watch);
        //                if (watch.StrategyId == 91)
        //                {
        //                    if (watch.Leg1.ContractInfo.Series == "CE")
        //                    {
        //                        if (watch.niftyLeg.LastTradedPrice > watch.Leg1.ContractInfo.StrikePrice)
        //                        {
        //                            double Intensic = Convert.ToDouble(watch.niftyLeg.LastTradedPrice - watch.Leg1.ContractInfo.StrikePrice) - Convert.ToDouble(watch.Leg1.LastTradedPrice);
        //                            watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(Intensic), 2);
        //                        }
        //                        else
        //                        {
        //                            watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(watch.Leg1.LastTradedPrice), 2);
        //                        }
        //                    }
        //                    else if (watch.Leg1.ContractInfo.Series == "PE")
        //                    {
        //                        if (watch.niftyLeg.LastTradedPrice < watch.Leg1.ContractInfo.StrikePrice)
        //                        {
        //                            double Intensic = Convert.ToDouble(watch.niftyLeg.LastTradedPrice - watch.Leg1.ContractInfo.StrikePrice) - Convert.ToDouble(watch.Leg1.LastTradedPrice);
        //                            watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(Intensic), 2);
        //                        }
        //                        else
        //                        {
        //                            watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(watch.Leg1.LastTradedPrice), 2);
        //                        }
        //                    }
        //                    if (watch.posInt != 0)
        //                    {
        //                        watch.LivePremium = Convert.ToDouble(watch.posInt * watch.Leg1.ContDetail.LotSize) * Convert.ToDouble(watch.Leg1.LastTradedPrice);
        //                        watch.RowData.Cells[WatchConst.LivePremium].Value = Math.Round(watch.LivePremium, 2);
        //                    }
        //                    else
        //                    {
        //                        watch.LivePremium = 0;
        //                        watch.RowData.Cells[WatchConst.LivePremium].Value = Math.Round(watch.LivePremium, 2);
        //                    }
        //                }
        //                if (watch.StrategyId == 91)
        //                {
        //                    CalculateSpreadSingle(watch);
        //                    AvgCalculatedGreek(watch);
        //                }
        //                if (watch.StrategyId == 1113 || watch.StrategyId == 1114)
        //                {
        //                    if (watch.Leg1.ATP != 0 && watch.Leg2.ATP != 0)
        //                    {
        //                        double atp = (watch.Leg2.ATP - watch.Leg1.ATP);
        //                        watch.RowData.Cells[WatchConst.ATP].Value = Math.Round(atp, 2);
        //                    }

        //                    CalculateSpreadRatio11_12(watch);

        //                }
        //                else if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
        //                {
        //                    if (watch.Leg1.ATP != 0 && watch.Leg2.ATP != 0)
        //                    {
        //                        double atp = (watch.Leg1.ATP + watch.Leg2.ATP);
        //                        watch.RowData.Cells[WatchConst.ATP].Value = Math.Round(atp, 2);
        //                    }
        //                    CalculateStrangleSpread(watch);
        //                }
        //                if (watch.StrategyName.Contains("MainJodiStraddle"))
        //                {
        //                    watch.straddleMktWind = AppGlobal.MarketWatch.Where(x => x.StrategyName == watch.StrategyName).Select(x => x.MktWind).Sum();
        //                    watch.straddleMktUnwind = AppGlobal.MarketWatch.Where(x => x.StrategyName == watch.StrategyName).Select(x => x.MktunWind).Sum();
        //                    watch.RowData.Cells[WatchConst.Straddle_MktWind].Value = watch.straddleMktWind;
        //                    watch.RowData.Cells[WatchConst.Straddle_MktUnwind].Value = watch.straddleMktUnwind;
        //                    if (watch.Hedgeflg)
        //                    {
        //                        if (watch.Track == "Hedge")
        //                        {
        //                            #region Hedge Straddle calcualtion
        //                            if (!watch.StrategyName.Contains("_Straddle") || !watch.StrategyName.Contains("_Strangle"))
        //                            {
        //                                if (watch.posInt != 0)
        //                                {
        //                                    string straddleHedgeStrategy = watch.StrategyName + "_Straddle";
        //                                    string strangleHedgeStrategy = watch.StrategyName + "_Strangle";
        //                                    double straddleAvg = AppGlobal.MarketWatch.Where(x => (x.StrategyName == straddleHedgeStrategy) || (x.StrategyName == strangleHedgeStrategy)).Select(x => x.MktunWind).Sum();
        //                                    if (straddleAvg < watch.StraddlAvg)
        //                                    {
        //                                        string _strategyName = watch.StrategyName;
        //                                        const char fieldSeparator = '_';
        //                                        List<string> split = _strategyName.Split(fieldSeparator).ToList();
        //                                        string _findStrategy = split[0] + "_" + split[1];
        //                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(_findStrategy))))
        //                                        {
        //                                            watch1.StraddlAvg = straddleAvg;
        //                                            watch1.RowData.Cells[WatchConst.StrategyAvg].Value = Math.Round(watch1.StraddlAvg, 2);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            #endregion
        //                        }
        //                    }
        //                    if (watch.Hedgeflg)
        //                    {
        //                        #region Hedge order send
        //                        if (watch.Track == "Main")
        //                        {
        //                            HedgeWithMain(watch);
        //                        }
        //                        else if (watch.Track == "Hedge")
        //                        {
        //                            if (watch.StrategyName.Contains("_Straddle") || watch.StrategyName.Contains("_Strangle"))
        //                            {
        //                                HedgeWithHedge(watch);
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                }
        //                else
        //                {
        //                    watch.straddleMktWind = watch.MktWind;
        //                    watch.straddleMktUnwind = watch.MktunWind;
        //                    watch.RowData.Cells[WatchConst.Straddle_MktWind].Value = watch.straddleMktWind;
        //                    watch.RowData.Cells[WatchConst.Straddle_MktUnwind].Value = watch.straddleMktUnwind;
        //                }
        //                if (watch.StrategyId == 91)
        //                {
        //                    SQ_OFF_Rule(watch);
        //                    System.Threading.Tasks.Task.Factory.StartNew(() =>
        //                        {
        //                            Initial_TrailingPos(watch);
        //                            FixPrice_Pos(watch);
        //                            LevelWiseTrade(watch);
        //                        });
        //                }
        //                if (watch.StrategyId == 91 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
        //                {
        //                    #region StopLoss Order
        //                    StopLossBuyOrder(watch);
        //                    StopLossSellOrder(watch);
        //                    #endregion
        //                }
        //                if (watch.StrategyId == 91 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
        //                {
        //                    #region DrawDown Order
        //                    DrawDownBuyOrder(watch);
        //                    DrawDownSellOrder(watch);
        //                    #endregion
        //                }
        //                if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 32211 || watch.StrategyId == 91 || watch.StrategyId == 1113 || watch.StrategyId == 1114)
        //                {
        //                    #region StopLoss Order

        //                    Thread t = new Thread(() =>
        //                        {
        //                            StraddleStopLoss(watch);

        //                        });
        //                    t.Start();
        //                    #endregion
        //                }

        //                if (watch.StrategyId == 32211)
        //                {
        //                    if (watch.Leg2.BuyPrice != 0 && watch.Leg1.BuyPrice != 0)
        //                    {
        //                        LSL_StranglePnl(watch);
        //                    }
        //                    LSL_StrangleCheckFlgStoploss(watch);
        //                }

        //                if (watch.StrategyId == 91)
        //                {
        //                    if (watch.PremiumAlert)
        //                    {
        //                        SqAll_Premium(watch, "PremiumLossHit");
        //                    }
        //                }

        //                if (watch.StrategyId == 91)
        //                {
        //                    if (watch.SqTimeflg)
        //                    {
        //                        UInt64 nowTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
        //                        UInt64 uintTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(watch.SqTime));
        //                        if (uintTime < nowTime)
        //                        {
        //                            watch.SqTimeflg = false;
        //                            SqoffAll(watch, "RuleSqOff");
        //                        }
        //                    }
        //                }

        //            }
        //            #endregion

        //            #region FUTURE
        //            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.niftyLeg.ContractInfo.TokenNo) == _response.TokenNo)))
        //            {
        //                int i = watch.RowData.Index;
        //                if (watch.niftyLeg.BuyPrice != 0)
        //                    watch.niftyLeg.OldBuyPrice = watch.niftyLeg.BuyPrice;
        //                if (watch.niftyLeg.SellPrice != 0)
        //                    watch.niftyLeg.OldSellPrice = watch.niftyLeg.SellPrice;

        //                watch.niftyLeg.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
        //                watch.niftyLeg.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
        //                watch.niftyLeg.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
        //                watch.RowData.Cells[WatchConst.FLTP].Value = watch.niftyLeg.LastTradedPrice;
        //                //CalculateGreek(watch);
        //                //CalculateSpread(watch);
        //            }
        //            #endregion

        //            #region Leg2

        //            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Leg2.ContractInfo.TokenNo) == _response.TokenNo)))
        //            {
        //                int i = watch.RowData.Index;
        //                if (watch.Leg2.BuyPrice != 0)
        //                    watch.Leg2.OldBuyPrice = watch.Leg2.BuyPrice;
        //                if (watch.Leg2.SellPrice != 0)
        //                    watch.Leg2.OldSellPrice = watch.Leg2.SellPrice;
        //                watch.Leg2.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
        //                watch.Leg2.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
        //                watch.Leg2.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
        //                watch.Leg2.MidPrice = Math.Round(Convert.ToDouble((watch.Leg2.BuyPrice + watch.Leg2.SellPrice) / 2), 2);
        //                watch.RowData.Cells[WatchConst.L2buyPrice].Value = watch.Leg2.BuyPrice;
        //                watch.RowData.Cells[WatchConst.L2sellPrice].Value = watch.Leg2.SellPrice;

        //                watch.Leg2.ATP = Convert.ToDouble(_response.AverageTradedPrice) / 100;

        //                if (AppGlobal.Record)
        //                {
        //                    if (watch.uniqueId == AppGlobal.RuleRecord)
        //                    {
        //                        TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg2.BuyPrice + "|SellPrice|" + watch.Leg2.SellPrice);
        //                    }
        //                }
        //                CalculateGreek(watch);
        //                if (watch.StrategyId == 111 || watch.StrategyId == 211 || watch.StrategyId == 311)
        //                {
        //                    CalculateSpread(watch);
        //                    CalculateSpreadRatio11_12(watch);
        //                }
        //                if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
        //                {
        //                    if (watch.Leg1.ATP != 0 && watch.Leg2.ATP != 0)
        //                    {
        //                        double atp = (watch.Leg1.ATP + watch.Leg2.ATP);
        //                        watch.RowData.Cells[WatchConst.ATP].Value = Math.Round(atp, 2);
        //                    }
        //                    CalculateStrangleSpread(watch);
        //                }
        //                else if (watch.StrategyId == 2211)
        //                    CalculateStrangleSpread(watch);
        //                else if (watch.StrategyId == 888)
        //                    CalculateLadderSpread(watch);
        //                else if (watch.StrategyId == 121)
        //                    CalculateButterflySpread(watch);
        //                else if (watch.StrategyId == 1331)
        //                    CalculateSpread1331(watch);
        //                else if (watch.StrategyId == 1221)
        //                    CalculateSpread1221(watch);

        //                else if (watch.StrategyId == 1113 || watch.StrategyId == 1114)
        //                {
        //                    CalculateSpreadRatio11_12(watch);

        //                }


        //                if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
        //                {
        //                    #region StopLoss Order
        //                    System.Threading.Tasks.Task.Factory.StartNew(() =>
        //                        {
        //                            StraddleStopLoss(watch);
        //                        });
        //                    #endregion
        //                }
        //                if (watch.StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo != "0")
        //                {
        //                    if (watch.Leg2.BuyPrice != 0 && watch.Leg1.BuyPrice != 0)
        //                    {
        //                        LSL_StranglePnl(watch);

        //                    }
        //                }
        //            }
        //            #endregion

        //            #region Leg3

        //            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Leg3.ContractInfo.TokenNo) == _response.TokenNo)))
        //            {
        //                int i = watch.RowData.Index;
        //                if (watch.Leg3.BuyPrice != 0)
        //                    watch.Leg3.OldBuyPrice = watch.Leg3.BuyPrice;
        //                if (watch.Leg3.SellPrice != 0)
        //                    watch.Leg3.OldSellPrice = watch.Leg3.SellPrice;
        //                watch.Leg3.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
        //                watch.Leg3.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
        //                //watch.Leg3.Sequence = seq;
        //                watch.Leg3.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
        //                watch.Leg3.MidPrice = Math.Round(Convert.ToDouble((watch.Leg3.BuyPrice + watch.Leg3.SellPrice) / 2), 2);
        //                watch.RowData.Cells[WatchConst.L3buyPrice].Value = watch.Leg3.BuyPrice;
        //                watch.RowData.Cells[WatchConst.L3sellPrice].Value = watch.Leg3.SellPrice;

        //                if (AppGlobal.Record)
        //                {
        //                    if (watch.uniqueId == AppGlobal.RuleRecord)
        //                    {
        //                        TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg2.BuyPrice + "|SellPrice|" + watch.Leg2.SellPrice);
        //                    }
        //                }
        //                CalculateGreek(watch);
        //                if (watch.StrategyId == 888)
        //                    CalculateLadderSpread(watch);
        //                else if (watch.StrategyId == 121)
        //                    CalculateButterflySpread(watch);
        //                else if (watch.StrategyId == 1331)
        //                    CalculateSpread1331(watch);
        //                else if (watch.StrategyId == 1221)
        //                    CalculateSpread1221(watch);
        //            }
        //            #endregion

        //            #region Leg4

        //            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Leg4.ContractInfo.TokenNo) == _response.TokenNo)))
        //            {
        //                int i = watch.RowData.Index;
        //                if (watch.Leg4.BuyPrice != 0)
        //                    watch.Leg4.OldBuyPrice = watch.Leg4.BuyPrice;
        //                if (watch.Leg4.SellPrice != 0)
        //                    watch.Leg4.OldSellPrice = watch.Leg4.SellPrice;
        //                watch.Leg4.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
        //                watch.Leg4.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
        //                //watch.Leg3.Sequence = seq;
        //                watch.Leg4.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
        //                watch.Leg4.MidPrice = Math.Round(Convert.ToDouble((watch.Leg4.BuyPrice + watch.Leg4.SellPrice) / 2), 2);
        //                watch.RowData.Cells[WatchConst.L4buyPrice].Value = watch.Leg4.BuyPrice;
        //                watch.RowData.Cells[WatchConst.L4sellPrice].Value = watch.Leg4.SellPrice;
        //                if (watch.StrategyId == 1331)
        //                    CalculateSpread1331(watch);
        //                else if (watch.StrategyId == 1221)
        //                    CalculateSpread1221(watch);


        //            }
        //            #endregion
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }
        //}
        #endregion

        public void Initial_TrailingPos(MarketWatch watch)
        {
            if (watch.I_Trailingflg)
            {

                if (watch.I_TrailingSide == "BUY")
                {
                    if (Convert.ToDouble(watch.MktWind) < watch.I_TrailingMinMaxPrice)
                    {
                        watch.I_TrailingMinMaxPrice = watch.MktWind;
                        watch.RowData.Cells[WatchConst.Init_TrailingMx].Value = watch.I_TrailingMinMaxPrice;
                        watch.I_TrailingTriggerPx = watch.I_TrailingMinMaxPrice + watch.I_TrailingPoint;
                        watch.RowData.Cells[WatchConst.Init_TrailingTg].Value = watch.I_TrailingTriggerPx;

                    }
                    if (Convert.ToDouble(watch.MktWind) > watch.I_TrailingTriggerPx)
                    {
                        watch.I_Trailingflg = false;

                        Thread _trade = new Thread(() =>
                        {
                            if (watch.I_TrailingTradeflg)
                            {
                                watch.I_TrailingTradeflg = false;
                                if (watch.I_TrailingQty != 0)
                                {
                                    int qty = Convert.ToInt32(watch.I_TrailingQty);
                                    for (int k = 1; k <= qty; k++)
                                    {
                                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                        snd.TransCode = 10;
                                        UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                        snd.UniqueID = unique;
                                        snd.gui_id = AppGlobal.GUI_ID;
                                        snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                        snd.isWind = true;
                                        snd.Open = 0;

                                        TransactionWatch.ErrorMessage("TrailTradeHit|" + unique + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Position|" + qty + "|Count|" + k);

                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                        System.Threading.Thread.Sleep(50);
                                    }
                                }
                            }
                        });
                        _trade.SetApartmentState(ApartmentState.STA);
                        _trade.Start();
                        TransactionWatch.ErrorMessage("TrailingBuy|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|Side|" + watch.I_TrailingSide + "|Initial|"
                                        + watch.I_TrailingInitial + "|Min/Max|" + watch.I_TrailingMinMaxPrice + "|Point|" + watch.I_TrailingPoint + "|Trigger|" + watch.I_TrailingTriggerPx + "|TradeFlg|" + watch.I_TrailingTradeflg + "|Wind|" + watch.MktWind + "|UnWind|" + watch.MktunWind);
                        MessageBox.Show("TrailingBuy|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|Side|" + watch.I_TrailingSide + "|Initial|"
                                        + watch.I_TrailingInitial + "|Min/Max|" + watch.I_TrailingMinMaxPrice + "|Point|" + watch.I_TrailingPoint + "|Trigger|" + watch.I_TrailingTriggerPx + "|TradeFlg|" + watch.I_TrailingTradeflg + "|Wind|" + watch.MktWind + "|UnWind|" + watch.MktunWind);              
                      
                        watch.I_TrailingInitial = 0;
                        watch.I_TrailingMinMaxPrice = 0;
                        watch.I_TrailingPoint = 0;
                        watch.I_TrailingTriggerPx = 0;
                        watch.I_TrailingQty = 0;
                        watch.I_TrailingSide = "None";

                        watch.RowData.Cells[WatchConst.Init_TrailingPx].Value = watch.I_TrailingInitial;
                        watch.RowData.Cells[WatchConst.Init_TrailingMx].Value = watch.I_TrailingMinMaxPrice;
                        watch.RowData.Cells[WatchConst.Init_TrailingPt].Value = watch.I_TrailingPoint;
                        watch.RowData.Cells[WatchConst.Init_TrailingTg].Value = watch.I_TrailingTriggerPx;
                        
                    }
                }
                else if (watch.I_TrailingSide == "SELL")
                {
                    if (Convert.ToDouble(watch.MktunWind) > watch.I_TrailingMinMaxPrice)
                    {
                        watch.I_TrailingMinMaxPrice = watch.MktunWind;
                        watch.RowData.Cells[WatchConst.Init_TrailingMx].Value = watch.I_TrailingMinMaxPrice;
                        watch.I_TrailingTriggerPx = watch.I_TrailingMinMaxPrice - watch.I_TrailingPoint;
                        watch.RowData.Cells[WatchConst.Init_TrailingTg].Value = watch.I_TrailingTriggerPx;
                    }
                    if (Convert.ToDouble(watch.MktunWind) < watch.I_TrailingTriggerPx)
                    {
                        watch.I_Trailingflg = false;
                        Thread _trade = new Thread(() =>
                        {
                            if (watch.I_TrailingTradeflg)
                            {
                                watch.I_TrailingTradeflg = false;
                                if (watch.I_TrailingQty != 0)
                                {
                                    int qty = Convert.ToInt32(watch.I_TrailingQty);
                                    for (int k = 1; k <= qty; k++)
                                    {
                                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                        snd.TransCode = 10;
                                        UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                        snd.UniqueID = unique;
                                        snd.gui_id = AppGlobal.GUI_ID;
                                        snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                        snd.isWind = false;
                                        snd.Open = 0;

                                        TransactionWatch.ErrorMessage("TrailTradeHit|" + unique + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Position|" + qty + "|Count|" + k);

                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                        System.Threading.Thread.Sleep(50);
                                    }
                                }
                            }
                        });
                        _trade.SetApartmentState(ApartmentState.STA);
                        _trade.Start();
                        TransactionWatch.ErrorMessage("TrailingSell|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|Side|" + watch.I_TrailingSide + "|Initial|"
                                       + watch.I_TrailingInitial + "|Min/Max|" + watch.I_TrailingMinMaxPrice + "|Point|" + watch.I_TrailingPoint + "|Trigger|" + watch.I_TrailingTriggerPx + "|TradeFlg|" + watch.I_TrailingTradeflg + "|Wind|" + watch.MktWind + "|UnWind|" + watch.MktunWind);
                        MessageBox.Show("TrailingSell|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Symbol|" + watch.Leg1.ContractInfo.Symbol + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Series|" + watch.Leg1.ContractInfo.Series + "|Side|" + watch.I_TrailingSide + "|Initial|"
                                        + watch.I_TrailingInitial + "|Min/Max|" + watch.I_TrailingMinMaxPrice + "|Point|" + watch.I_TrailingPoint + "|Trigger|" + watch.I_TrailingTriggerPx + "|TradeFlg|" + watch.I_TrailingTradeflg + "|Wind|" + watch.MktWind + "|UnWind|" + watch.MktunWind);
                        watch.I_TrailingInitial = 0;
                        watch.I_TrailingMinMaxPrice = 0;
                        watch.I_TrailingPoint = 0;
                        watch.I_TrailingTriggerPx = 0;
                        watch.I_TrailingQty = 0;
                        watch.I_TrailingSide = "None";
                        watch.RowData.Cells[WatchConst.Init_TrailingPx].Value = watch.I_TrailingInitial;
                        watch.RowData.Cells[WatchConst.Init_TrailingMx].Value = watch.I_TrailingMinMaxPrice;
                        watch.RowData.Cells[WatchConst.Init_TrailingPt].Value = watch.I_TrailingPoint;
                        watch.RowData.Cells[WatchConst.Init_TrailingTg].Value = watch.I_TrailingTriggerPx;
                    }
                }
            }
        }

        public void FixPrice_Pos(MarketWatch watch)
        {
            if (watch.I_Priceflg)
            {
                if (watch.I_PriceSide == "BUY")
                {
                    if (watch.I_Price >= Convert.ToDouble(watch.MktunWind))
                    {
                        watch.I_Priceflg = false;
                        watch.I_UserPriceflg = false;
                        Thread _trade = new Thread(() =>
                        {
                            if (watch.I_PriceTrade)
                            {
                                watch.I_PriceTrade = false;
                                if (watch.I_PriceQty != 0)
                                {
                                    int qty = Convert.ToInt32(watch.I_PriceQty);
                                    for (int k = 1; k <= qty; k++)
                                    {
                                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                        snd.TransCode = 10;
                                        UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                        snd.UniqueID = unique;
                                        snd.gui_id = AppGlobal.GUI_ID;
                                        snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                        snd.isWind = true;
                                        snd.Open = 0;

                                        TransactionWatch.ErrorMessage("TrailTradeHitBuy|" + unique + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Position|" + qty + "|Count|" + k);

                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                        System.Threading.Thread.Sleep(50);
                                    }
                                }
                            }
                        });
                        _trade.SetApartmentState(ApartmentState.STA);
                        _trade.Start();
                        TransactionWatch.ErrorMessage("FixedPrice|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|UserPrice|" + watch.I_Price + "|UserQty|" + watch.I_PriceQty +
                                        "|Priceflg|" + watch.I_Priceflg);
                        MessageBox.Show("FixedPrice|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|UserPrice|" + watch.I_Price + "|UserQty|" + watch.I_PriceQty +
                                        "|Priceflg|" + watch.I_Priceflg);
                        
                        watch.I_Price = 0;
                        watch.I_PriceQty = 0;
                        watch.I_PriceSide = "None";
                    }
                }
                else if (watch.I_PriceSide == "SELL")
                {
                    if (watch.I_Price <= Convert.ToDouble(watch.MktWind))
                    {
                        watch.I_Priceflg = false;
                        watch.I_UserPriceflg = false;
                        Thread _trade = new Thread(() =>
                        {
                            if (watch.I_PriceTrade)
                            {
                                watch.I_PriceTrade = false;
                                if (watch.I_PriceQty != 0)
                                {
                                    int qty = Convert.ToInt32(watch.I_PriceQty);
                                    for (int k = 1; k <= qty; k++)
                                    {
                                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                        snd.TransCode = 10;
                                        UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                        snd.UniqueID = unique;
                                        snd.gui_id = AppGlobal.GUI_ID;
                                        snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                        snd.isWind = false;
                                        snd.Open = 0;

                                        TransactionWatch.ErrorMessage("TrailTradeHitSell|" + unique + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Position|" + qty + "|Count|" + k);

                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                        System.Threading.Thread.Sleep(50);
                                    }
                                }
                            }
                        });
                        _trade.SetApartmentState(ApartmentState.STA);
                        _trade.Start();
                        TransactionWatch.ErrorMessage("FixedPrice|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|UserPrice|" + watch.I_Price + "|UserQty|" + watch.I_PriceQty +
                                      "|Priceflg|" + watch.I_Priceflg);
                        MessageBox.Show("FixedPrice|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|UserPrice|" + watch.I_Price + "|UserQty|" + watch.I_PriceQty +
                                        "|Priceflg|" + watch.I_Priceflg);
                      
                        watch.I_Price = 0;
                        watch.I_PriceQty = 0;
                        watch.I_PriceSide = "None";
                    }
                }
            }
        }

        public void SqAll_Premium(MarketWatch watch,string Msg)
        {
            if (watch.TG_Premium > watch.LivePremium)
            {
                watch.PremiumAlert = false;
                watch.PremiumUserpxAlert = false;
                watch.PremiumTrade = false; 
                if (watch.PremiumTrade)
                {
                    SqoffAll(watch, "PremiumLossHit");
                }
                TransactionWatch.ErrorMessage("");
                watch.Premium_dm = 0;
                watch.Premium_Percent = 0;
                watch.Init_Premium = 0;
                watch.TG_Premium = 0;
            } 
        }
   
        public void SQ_OFF_Rule(MarketWatch watch)
        {
            if (watch.SQVegaflg)
            {
                if (watch.Leg1.VegaV < watch.SQVegaPrice)
                {
                    TransactionWatch.ErrorMessage("VegaSqOff|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.Init_SQVegaPrice + "|"
                            + Math.Abs(watch.Leg1.VegaV) + "|" + Math.Abs(watch.SQVegaPrice));


                    watch.SQVegaflg = false;
                    SqoffAll(watch,"VegaSqOff");
                }
            }

            if (watch.SQPremiumflg)
            {
                if (watch.SQPremiumPrice > watch.LivePremium)
                {
                    TransactionWatch.ErrorMessage("PremiumSqOff|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.Init_SQPremiumPrice + "|"
                          + watch.premium + "|" + watch.SQPremiumPrice + "|" + watch.SQPremiumPoint);


                    watch.SQPremiumflg = false;
                    SqoffAll(watch,"PremiumSqOff");
                }
            }

            if (watch.SQLossflg)
            {
                if (watch.pnl < watch.SQLossPrice)
                {
                    watch.SQLossflg = false;
                    SqoffAll(watch,"LossSqOff");
                }
            }
        }

        public void StopLossBuyOrder(MarketWatch watch)
        {
            if (watch.TGBuyPrice != 999999 && watch.AP_BuySL != 999999)
            {
                if (watch.SL_BuyOrderflg == true)
                {
                    if (watch.SL_BuyQty != 0)
                    {
                        if (Convert.ToDouble(watch.MktunWind) >= watch.TGBuyPrice)
                        {
                            watch.SL_BuyOrderflg = false;
                            int _tobeBuyTrdQty = Convert.ToInt32(watch.SL_BuyQty);
                            for (int k = 1; k <= _tobeBuyTrdQty; k++)
                            {
                                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                snd.TransCode = 10;
                                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                snd.UniqueID = unique;
                                snd.gui_id = AppGlobal.GUI_ID;
                                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                snd.isWind = true;
                                snd.Open = 0;

                                long seq = ClassDisruptor.ringBufferRequest.Next();
                                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                ClassDisruptor.ringBufferRequest.Publish(seq);

                                System.Threading.Thread.Sleep(50);
                                TransactionWatch.ErrorMessage("BuyStopLoss|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.TGBuyPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + k + "|" + _tobeBuyTrdQty);
                                TransactionWatch.TransactionMessage("BuyStopLoss|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.TGBuyPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + k + "|" + _tobeBuyTrdQty, Color.Blue);
                            }
                            watch.SL_BuyQty = 0;
                            watch.TGBuyPrice = 999999;
                            watch.AP_BuySL = 999999;

                            watch.RowData.Cells[WatchConst.SL_BuyQty].Value = watch.SL_BuyQty;
                            watch.RowData.Cells[WatchConst.TGBuyPrice].Value = watch.TGBuyPrice;
                            watch.RowData.Cells[WatchConst.AP_BuySL].Value = watch.AP_BuySL;
                            int rowindex = watch.RowData.Index;
                            if (watch.SL_BuyOrderflg == false && watch.SL_SellOrderflg == false)
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.Unique].Style.BackColor = Color.White;
                        }
                        else
                        {
                            int rowindex = watch.RowData.Index;
                            dgvMarketWatch.Rows[rowindex].Cells[WatchConst.Unique].Style.BackColor = Color.MediumSeaGreen;
                        }
                    }
                }
            }
        }

        public void StopLossSellOrder(MarketWatch watch)
        {
            if (watch.TGSellPrice != 999999 && watch.AP_SellSL != 999999)
            {
                if (watch.SL_SellOrderflg == true)
                {
                    if (watch.SL_SellQty != 0)
                    {
                        if (Convert.ToDouble(watch.MktWind) <= watch.TGSellPrice)
                        {
                            watch.SL_SellOrderflg = false;

                            int _tobeSellTrdQty = Convert.ToInt32(watch.SL_SellQty);
                            for (int k = 1; k <= _tobeSellTrdQty; k++)
                            {
                                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                snd.TransCode = 10;
                                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                snd.UniqueID = unique;
                                snd.gui_id = AppGlobal.GUI_ID;
                                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                snd.isWind = false;
                                snd.Open = 0;

                                long seq = ClassDisruptor.ringBufferRequest.Next();
                                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                ClassDisruptor.ringBufferRequest.Publish(seq);

                                System.Threading.Thread.Sleep(50);
                                TransactionWatch.ErrorMessage("SellStopLoss|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + k + "|" + _tobeSellTrdQty);
                                TransactionWatch.TransactionMessage("SellStopLoss|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + k + "|" + _tobeSellTrdQty, Color.Blue);
                            }
                            watch.SL_SellQty = 0;
                            watch.RowData.Cells[WatchConst.SL_SellQty].Value = watch.SL_SellQty;
                            watch.AP_SellSL = 999999;
                            watch.TGSellPrice = 999999;
                            watch.RowData.Cells[WatchConst.AP_SellSL].Value = watch.AP_SellSL;
                            watch.RowData.Cells[WatchConst.TGSellPrice].Value = watch.TGSellPrice;

                            int rowindex = watch.RowData.Index;
                            if (watch.SL_BuyOrderflg == false && watch.SL_SellOrderflg == false)
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.Unique].Style.BackColor = Color.White;
                        }
                        else
                        {
                            int rowindex = watch.RowData.Index;
                            dgvMarketWatch.Rows[rowindex].Cells[WatchConst.Unique].Style.BackColor = Color.MediumSeaGreen;
                        }
                    }
                }
            }
        }

        public void StraddleStopLoss(MarketWatch watch)
        {

            if (watch.Leg1.ContractInfo.StrikePrice == 41500 && watch.Leg1.ContractInfo.Series == "PE")
            {

            }


            //if (watch.Leg2.ContractInfo.TokenNo != "0")
            {
                if (watch.DD_SellOrderflg)
                {
                    if (watch.Alert)
                    {
                        if (watch.DD_SellMinPrice != 0)
                        {
                            int rowindex = watch.RowData.Index;
                            if (watch.DD_SellMinPrice >= watch.MktunWind)
                            {
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.Strategy].Style.BackColor = Color.ForestGreen;
                            }
                            else
                            {
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.Strategy].Style.BackColor = Color.Red;
                            }
                            if (Convert.ToDouble(watch.MktunWind) >= watch.DD_TGSellPrice)
                            {

                                //TransactionWatch.TransactionMessage("SellDrawDown|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMin + "|" + watch.DD_bm_Sell_Percent, Color.Blue);
                                TransactionWatch.ErrorMessage("SellDrawDown|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMin + "|" + watch.DD_bm_Sell_Percent + "|" + watch.DD_bm_Sell);

                                watch.DD_SellOrderflg = false;
                                watch.DD_TGSellPrice = 0;
                                watch.DD_SellMinPrice = 0;
                                watch.DD_SetMin = 0;
                                watch.FutPrice = 0;
                                if (watch.StrategyId == 91)
                                {
                                    if (watch.StoplossTrade)
                                    {
                                        if (watch.posInt != 0)
                                        {
                                            int posInt = watch.posInt;
                                            if (posInt < 0)
                                            {
                                                watch.StoplossTrade = false;
                                                int _posInt = Math.Abs(Convert.ToInt32(watch.posInt));
                                                for (int k = 1; k <= _posInt; k++)
                                                {
                                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                                    snd.TransCode = 10;
                                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                                    snd.UniqueID = unique;
                                                    snd.gui_id = AppGlobal.GUI_ID;
                                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                                    snd.isWind = true;
                                                    snd.Open = 0;


                                                    TransactionWatch.ErrorMessage("StoplossHit|" + unique + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Position|"  + _posInt + "|Count|" + k);

                                                    long seq = ClassDisruptor.ringBufferRequest.Next();
                                                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                                    ClassDisruptor.ringBufferRequest.Publish(seq);
                                                    System.Threading.Thread.Sleep(50);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (watch.StrategyId == 12211)
                                {
                                    if (watch.StoplossTrade)
                                    {
                                        if (watch.L1PosInt != 0)
                                        {
                                            int posInt = watch.L1PosInt;
                                            if (posInt < 0)
                                            {
                                                int _posInt = Math.Abs(Convert.ToInt32(watch.L1PosInt));
                                                watch.StoplossTrade = false;
                                                for (int k = 1; k <= _posInt; k++)
                                                {
                                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                                    snd.TransCode = 10;
                                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                                    snd.UniqueID = unique;
                                                    snd.gui_id = AppGlobal.GUI_ID;
                                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                                    snd.isWind = true;
                                                    snd.Open = 0;

                                                    long seq = ClassDisruptor.ringBufferRequest.Next();
                                                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                                    ClassDisruptor.ringBufferRequest.Publish(seq);
                                                    System.Threading.Thread.Sleep(50);
                                                }
                                            } 
                                        }
                                    }
                                }
                                MessageBox.Show("SellDrawDown|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMin + "|" + watch.DD_bm_Sell_Percent);
                                TransactionWatch.TransactionMessage("SellDrawDown|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMin + "|" + watch.DD_bm_Sell_Percent, Color.Blue);
                                TransactionWatch.ErrorMessage("SellDrawDown|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMin + "|" + watch.DD_bm_Sell_Percent);
                                watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = watch.DD_TGSellPrice;
                                watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_SellMinPrice;
                                watch.RowData.Cells[WatchConst.FutPrice].Value = watch.FutPrice;
                                int iRow = watch.RowData.Index;
                                watch.thread = new System.Threading.Thread(() =>
                                {
                                    MarketWatch _watch = AppGlobal.MarketWatch[iRow];
                                    while (count > 0)
                                    {
                                        if (!_watch.go)
                                        {
                                            _watch.go = true;
                                            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.DarkOliveGreen;
                                            System.Threading.Thread.Sleep(500);
                                        }
                                        if (_watch.go)
                                        {
                                            _watch.go = false;
                                            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
                                            System.Threading.Thread.Sleep(500);
                                        }
                                    }
                                });
                                watch.thread.Start();
                            }
                        }
                    }
                }
                if (watch.DD_BuyOrderflg)
                {
                    if (watch.BuyAlert)
                    {
                        if (watch.DD_BuyMaxPrice != 0)
                        {
                            int rowindex = watch.RowData.Index;
                            if (watch.DD_BuyMaxPrice >= Math.Abs(Convert.ToDouble(watch.MktWind)))
                            {
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.StrategyName].Style.BackColor = Color.ForestGreen;
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;
                            }
                            else
                            {
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.StrategyName].Style.BackColor = Color.Red;
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Green;
                            }
                            if (Math.Abs(Convert.ToDouble(watch.MktWind)) <= watch.DD_TGBuyPrice)
                            {

                                //TransactionWatch.TransactionMessage("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.DD_BuyMaxPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMax + "|" + watch.DD_bm_Buy_Percent, Color.Blue);
                                TransactionWatch.ErrorMessage("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.DD_BuyMaxPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMax + "|" + watch.DD_bm_Buy_Percent);
                               

                                watch.DD_BuyOrderflg = false;
                                watch.DD_TGBuyPrice = 0;
                                watch.DD_BuyMaxPrice = 0;
                                watch.DD_SetMax = 0;
                                if (watch.StrategyId == 91)
                                {
                                    if (watch.ProfitTrade)
                                    {
                                        if (watch.posInt != 0)
                                        {
                                            int posInt = watch.posInt;
                                            if (posInt < 0)
                                            {
                                                watch.ProfitTrade = false;
                                                int _posInt = Math.Abs(Convert.ToInt32(watch.posInt));
                                                for (int k = 1; k <= _posInt; k++)
                                                {
                                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                                    snd.TransCode = 10;
                                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                                    snd.UniqueID = unique;
                                                    snd.gui_id = AppGlobal.GUI_ID;
                                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                                    snd.isWind = true;
                                                    snd.Open = 0;

                                                    TransactionWatch.ErrorMessage("ProfitTradeHit|" + unique + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Position|" + _posInt + "|Count|" + k);

                                                    long seq = ClassDisruptor.ringBufferRequest.Next();
                                                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                                    ClassDisruptor.ringBufferRequest.Publish(seq);
                                                    System.Threading.Thread.Sleep(50);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (watch.StrategyId == 12211)
                                {
                                    if (watch.ProfitTrade)
                                    {
                                        if (watch.L1PosInt != 0)
                                        {
                                            int posInt = watch.L1PosInt;
                                            if (posInt < 0)
                                            {
                                                int _posInt = Math.Abs(Convert.ToInt32(watch.L1PosInt));
                                                watch.ProfitTrade = false;
                                                for (int k = 1; k <= _posInt; k++)
                                                {
                                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                                    snd.TransCode = 10;
                                                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                                    snd.UniqueID = unique;
                                                    snd.gui_id = AppGlobal.GUI_ID;
                                                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                                    snd.isWind = true;
                                                    snd.Open = 0;

                                                    long seq = ClassDisruptor.ringBufferRequest.Next();
                                                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                                    ClassDisruptor.ringBufferRequest.Publish(seq);
                                                    System.Threading.Thread.Sleep(50);
                                                }
                                            }
                                        }
                                    }
                                }
                                MessageBox.Show("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.DD_BuyMaxPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMax + "|" + watch.DD_bm_Buy_Percent);
                                TransactionWatch.TransactionMessage("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.DD_BuyMaxPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMax + "|" + watch.DD_bm_Buy_Percent, Color.Blue);
                                TransactionWatch.ErrorMessage("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.DD_BuyMaxPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMax + "|" + watch.DD_bm_Buy_Percent);
                               
                                watch.RowData.Cells[WatchConst.DD_TGBuyPrice].Value = watch.DD_TGBuyPrice;
                                watch.RowData.Cells[WatchConst.DD_MinBuy].Value = watch.DD_BuyMaxPrice;
                                int iRow = watch.RowData.Index;
                                watch.thread1 = new System.Threading.Thread(() =>
                                {
                                    MarketWatch _watch = AppGlobal.MarketWatch[iRow];
                                    while (count > 0)
                                    {
                                        if (!_watch.go1)
                                        {
                                            _watch.go1 = true;
                                            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.GreenYellow;
                                            System.Threading.Thread.Sleep(500);
                                        }
                                        if (_watch.go1)
                                        {
                                            _watch.go1 = false;
                                            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
                                            System.Threading.Thread.Sleep(500);
                                        }
                                    }
                                });
                                watch.thread1.Start();
                            }
                        }
                    }
                }
                if (watch.ProfitTrail)
                {
                    int rowindex = watch.RowData.Index;
                    if (watch.UserPriceflg == true)
                    {
                        if (!watch.TrailingStart)
                        {
                            if (Math.Abs(Convert.ToDouble(watch.MktunWind)) <= watch.trail_MinPrice)
                            {
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.L1Strike].Style.BackColor = Color.YellowGreen;
                                watch.TrailingStart = true;
                            }
                            else
                                dgvMarketWatch.Rows[rowindex].Cells[WatchConst.L1Strike].Style.BackColor = Color.ForestGreen;
                        }
                    }
                    //if (watch.TrailingStart)
                    {
                        if (Math.Abs(Convert.ToDouble(watch.MktunWind)) < watch.trail_MinPrice)
                        {
                            watch.trail_MinPrice = Math.Abs(watch.MktunWind);
                            watch.RowData.Cells[WatchConst.trail_Mx].Value = watch.trail_MinPrice;
                            watch.trail_TGPrice = watch.trail_MinPrice + watch.trail_bm;
                            watch.RowData.Cells[WatchConst.trail_TGPrice].Value = watch.trail_TGPrice;
                        }
                        if (Math.Abs(Convert.ToDouble(watch.MktunWind)) >= watch.trail_TGPrice)
                        {
                            TransactionWatch.ErrorMessage("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.trail_TGPrice + "|" + watch.trail_MinPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.trail_SetMax + "|" + watch.trail_bm_Percent);

                            watch.ProfitTrail = false;
                            watch.TrailingStart = false;
                            watch.trail_TGPrice = 0;
                            watch.trail_MinPrice = 0;
                            watch.trail_SetMax = 0;
                            watch.trail_bm = 0;
                            if (watch.StrategyId == 91)
                            {
                                if (watch.TrailTrade)
                                {
                                    if (watch.posInt != 0)
                                    {
                                        int posInt = watch.posInt;
                                        if (posInt < 0)
                                        {
                                            watch.TrailTrade = false;
                                            int _posInt = Math.Abs(Convert.ToInt32(watch.posInt));
                                            for (int k = 1; k <= _posInt; k++)
                                            {
                                                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                                snd.TransCode = 10;
                                                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                                snd.UniqueID = unique;
                                                snd.gui_id = AppGlobal.GUI_ID;
                                                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                                snd.isWind = true;
                                                snd.Open = 0;

                                                TransactionWatch.ErrorMessage("TrailTradeHit|" + unique + "|Strike|" + watch.Leg1.ContractInfo.StrikePrice + "|Position|" + _posInt + "|Count|" + k);
                                                
                                                long seq = ClassDisruptor.ringBufferRequest.Next();
                                                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                                ClassDisruptor.ringBufferRequest.Publish(seq);
                                                System.Threading.Thread.Sleep(50);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (watch.StrategyId == 12211)
                            {
                                if (watch.TrailTrade)
                                {
                                    if (watch.L1PosInt != 0)
                                    {
                                        int posInt = watch.L1PosInt;
                                        if (posInt < 0)
                                        {
                                            int _posInt = Math.Abs(Convert.ToInt32(watch.L1PosInt));
                                            watch.TrailTrade = false;
                                            for (int k = 1; k <= _posInt; k++)
                                            {
                                                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                                snd.TransCode = 10;
                                                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                                snd.UniqueID = unique;
                                                snd.gui_id = AppGlobal.GUI_ID;
                                                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                                snd.isWind = true;
                                                snd.Open = 0;

                                                long seq = ClassDisruptor.ringBufferRequest.Next();
                                                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                                ClassDisruptor.ringBufferRequest.Publish(seq);
                                                System.Threading.Thread.Sleep(50);
                                            }
                                        }
                                    }
                                }
                            }
                            MessageBox.Show(new Form { TopMost = true }, "BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.trail_TGPrice + "|" + watch.trail_MinPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.trail_SetMax + "|" + watch.trail_bm_Percent);
                            TransactionWatch.TransactionMessage("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.trail_TGPrice + "|" + watch.trail_MinPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.trail_SetMax + "|" + watch.trail_bm_Percent, Color.Blue);
                            TransactionWatch.ErrorMessage("BuyProfitBook|" + watch.ProfitTrail + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.trail_TGPrice + "|" + watch.trail_MinPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.trail_SetMax + "|" + watch.trail_bm_Percent);
                          
                            watch.RowData.Cells[WatchConst.trail_TGPrice].Value = watch.trail_TGPrice;
                            watch.RowData.Cells[WatchConst.trail_Mx].Value = watch.trail_MinPrice;
                            watch.RowData.Cells[WatchConst.trail_bm].Value = watch.trail_SetMax;
                            int iRow = watch.RowData.Index;
                            watch.thread2 = new System.Threading.Thread(() =>
                            {
                                MarketWatch _watch = AppGlobal.MarketWatch[iRow];
                                while (count > 0)
                                {
                                    if (!_watch.go2)
                                    {
                                        _watch.go2 = true;
                                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.LawnGreen;
                                        System.Threading.Thread.Sleep(500);
                                    }
                                    if (_watch.go2)
                                    {
                                        _watch.go2 = false;
                                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
                                        System.Threading.Thread.Sleep(500);
                                    }
                                }
                            });
                            watch.thread2.Start();
                        }
                    }
                }
            }
        }

        public void DrawDownBuyOrder(MarketWatch watch)
        {
            if (watch.DD_BuyOrderflg)
            {
                if (watch.DD_BuyQty != 0 && watch.DD_bm_Buy != 0)
                {
                    if (watch.DD_BuyMaxPrice != 0)
                    {
                        if (watch.DD_BuyMaxPrice > watch.MktunWind)
                        {
                            watch.DD_BuyMaxPrice = watch.MktunWind;
                            watch.RowData.Cells[WatchConst.DD_MinBuy].Value = watch.DD_BuyMaxPrice;
                            watch.DD_TGBuyPrice = watch.DD_BuyMaxPrice + watch.DD_bm_Buy;
                            watch.RowData.Cells[WatchConst.DD_TGBuyPrice].Value = watch.DD_TGBuyPrice;
                        }
                        if (Convert.ToDouble(watch.MktunWind) >= watch.DD_TGBuyPrice)
                        {
                            watch.DD_BuyOrderflg = false;
                            int _tobeBuyTrdQty = Convert.ToInt32(watch.DD_BuyQty);
                            MessageBox.Show("BuyDrawDown|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMax);
                            for (int k = 1; k <= _tobeBuyTrdQty; k++)
                            {

                                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                snd.TransCode = 10;
                                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                snd.UniqueID = unique;
                                snd.gui_id = AppGlobal.GUI_ID;
                                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                snd.isWind = true;
                                snd.Open = 0;

                                long seq = ClassDisruptor.ringBufferRequest.Next();
                                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                ClassDisruptor.ringBufferRequest.Publish(seq);

                                System.Threading.Thread.Sleep(10);
                                TransactionWatch.ErrorMessage("BuyDrawDown|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + k + "|" + _tobeBuyTrdQty + "|" + watch.DD_BuyMaxPrice + "|" + watch.DD_SetMax);
                                TransactionWatch.TransactionMessage("BuyDrawDown|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGBuyPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + k + "|" + _tobeBuyTrdQty + "|" + watch.DD_BuyMaxPrice + "|" + watch.DD_SetMax, Color.Blue);
                            }
                            watch.DD_BuyQty = 0;
                            watch.DD_TGBuyPrice = 0;
                            watch.DD_BuyMaxPrice = 0;
                            watch.DD_SetMax = 0;
                            watch.RowData.Cells[WatchConst.DD_TGBuyPrice].Value = watch.DD_TGBuyPrice;
                            watch.RowData.Cells[WatchConst.DD_BuyQty].Value = watch.DD_BuyQty;
                            watch.RowData.Cells[WatchConst.DD_MinBuy].Value = watch.DD_BuyMaxPrice;
                            int rowindex = watch.RowData.Index;
                            if (watch.DD_BuyOrderflg == false && watch.DD_SellOrderflg == false)
                                dgvMarketWatch.Rows[rowindex].DefaultCellStyle.BackColor = Color.White;

                        }
                    }
                }
            }
        }

        public void DrawDownSellOrder(MarketWatch watch)
        {
            if (watch.DD_SellOrderflg)
            {
                if (watch.DD_SellQty != 0 && watch.DD_bm_Sell != 0)
                {
                    if (watch.DD_SellMinPrice != 0)
                    {
                        if (watch.DD_SellMinPrice < watch.MktWind)
                        {
                            watch.DD_SellMinPrice = watch.MktWind;
                            watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_SellMinPrice;
                            watch.DD_TGSellPrice = watch.DD_SellMinPrice - watch.DD_bm_Sell;
                            watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = watch.DD_TGSellPrice;
                        }

                        if (Convert.ToDouble(watch.MktWind) <= watch.DD_TGSellPrice)
                        {
                            watch.DD_SellOrderflg = false;
                            int _tobeSellTrdQty = Convert.ToInt32(watch.DD_SellQty);
                            MessageBox.Show("SellDrawDown|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + watch.DD_SetMin);
                            for (int k = 1; k <= _tobeSellTrdQty; k++)
                            {
                                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                snd.TransCode = 10;
                                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                snd.UniqueID = unique;
                                snd.gui_id = AppGlobal.GUI_ID;
                                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                snd.isWind = false;
                                snd.Open = 0;

                                long seq = ClassDisruptor.ringBufferRequest.Next();
                                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                ClassDisruptor.ringBufferRequest.Publish(seq);

                                System.Threading.Thread.Sleep(10);
                                TransactionWatch.ErrorMessage("SellDrawDown|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + k + "|" + _tobeSellTrdQty + "|" + watch.DD_SellMinPrice + "|" + watch.DD_SetMin);
                                TransactionWatch.TransactionMessage("SellDrawDown|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" + watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.DD_TGSellPrice + "|" + watch.MktWind + "|" + watch.MktunWind + "|" + k + "|" + _tobeSellTrdQty + "|" + watch.DD_SellMinPrice + "|" + watch.DD_SetMin, Color.Blue);
                            }
                            watch.DD_SellQty = 0;
                            watch.DD_TGSellPrice = 0;
                            watch.DD_SellMinPrice = 0;
                            watch.DD_SetMin = 0;
                            watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = watch.DD_TGSellPrice;
                            watch.RowData.Cells[WatchConst.DD_SellQty].Value = watch.DD_SellQty;
                            watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_SellMinPrice;

                            int rowindex = watch.RowData.Index;
                            if (watch.DD_BuyOrderflg == false && watch.DD_SellOrderflg == false)
                                dgvMarketWatch.Rows[rowindex].DefaultCellStyle.BackColor = Color.White;
                        }
                    }
                }
            }
        }

        public void HedgeWithMain(MarketWatch watch)
        {
            if (!watch.StrategyName.Contains("_Straddle") && !watch.StrategyName.Contains("_Strangle"))
            {
                if ((watch.StraddlAvg + watch.StrategyDrawDown) < watch.straddleMktWind)
                {
                    string straddleHedgeStrategy = watch.StrategyName + "_Straddle";
                    string strangleHedgeStrategy = watch.StrategyName + "_Strangle";
                    int hedgeQty = 0;
                    int PosInt = 0;
                    foreach (var watch2 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == watch.StrategyName)))
                    {
                        if (PosInt > watch2.posInt)
                        {
                            PosInt = watch2.posInt;
                        }
                    }

                    int round1Qty = Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round1Percent)) / 100);
                    int round2Qty = Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round1Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round2Percent)) / 100);
                    int round3Qty = Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round1Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round2Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round3Percent)) / 100);
                    int round4Qty = Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round1Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round2Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round3Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round4Percent)) / 100);

                    int roundNo = 0;

                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == straddleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "CE")))
                    {
                        if (PosInt != 0)
                        {
                            if (!watch.Alert)
                            {
                                if (round1Qty > Math.Abs(watch1.posInt))
                                {
                                    hedgeQty = round1Qty;
                                    roundNo = 1;
                                }
                                else if (round2Qty > Math.Abs(watch1.posInt))
                                {
                                    hedgeQty = round2Qty - round1Qty;
                                    roundNo = 2;
                                }
                                else if (round3Qty > Math.Abs(watch1.posInt))
                                {
                                    hedgeQty = round3Qty - round2Qty;
                                    roundNo = 3;
                                }
                                else if (round4Qty > Math.Abs(watch1.posInt))
                                {
                                    hedgeQty = round4Qty - (round3Qty);
                                    roundNo = 4;
                                }
                                else if (Math.Abs(PosInt) == Math.Abs(watch1.posInt))
                                    hedgeQty = 0;
                            }
                            else
                            {
                                if (watch.AlertLevel == 1)
                                {
                                    hedgeQty = round1Qty;
                                    roundNo = 1;
                                }
                                else if (watch.AlertLevel == 2)
                                {
                                    hedgeQty = round2Qty - round1Qty;
                                    roundNo = 2;
                                }
                                else if (watch.AlertLevel == 3)
                                {
                                    hedgeQty = round3Qty - (round2Qty);
                                    roundNo = 3;
                                }
                                else if (watch.AlertLevel == 4)
                                {
                                    hedgeQty = round4Qty - round3Qty;
                                    roundNo = 4;
                                }
                                else
                                    hedgeQty = 0;
                            }
                        }

                    }

                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == strangleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "CE")))
                    {
                        if (PosInt != 0)
                        {
                            if (!watch.Alert)
                            {
                                if (round1Qty > Math.Abs(watch1.posInt))
                                {
                                    hedgeQty = round1Qty;
                                    roundNo = 1;
                                }
                                else if (round2Qty > Math.Abs(watch1.posInt))
                                {
                                    hedgeQty = round2Qty - round1Qty;
                                    roundNo = 2;
                                }
                                else if (round3Qty > Math.Abs(watch1.posInt))
                                {
                                    hedgeQty = round3Qty - round2Qty;
                                    roundNo = 3;
                                }
                                else if (round4Qty > Math.Abs(watch1.posInt))
                                {
                                    hedgeQty = round4Qty - (round3Qty);
                                    roundNo = 4;
                                }
                                else if (Math.Abs(PosInt) == Math.Abs(watch1.posInt))
                                    hedgeQty = 0;
                            }
                            else
                            {
                                if (watch.AlertLevel == 1)
                                {
                                    hedgeQty = round1Qty;
                                    roundNo = 1;
                                }
                                else if (watch.AlertLevel == 2)
                                {
                                    hedgeQty = round2Qty - round1Qty;
                                    roundNo = 2;
                                }
                                else if (watch.AlertLevel == 3)
                                {
                                    hedgeQty = round3Qty - (round2Qty);
                                    roundNo = 3;
                                }
                                else if (watch.AlertLevel == 4)
                                {
                                    hedgeQty = round4Qty - (round3Qty);
                                    roundNo = 4;
                                }
                                else
                                    hedgeQty = 0;
                            }
                        }
                    }

                    if (hedgeQty == 0)
                        return;

                    int alert = 0;
                    if (!watch.Alert)
                    {

                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == straddleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "CE")))
                        {
                            for (int i = 0; i < hedgeQty; i++)
                            {
                                TransactionWatch.TransactionMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg, Color.Blue);
                                TransactionWatch.ErrorMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg);
                                System.Threading.Thread.Sleep(10);
                            }
                        }
                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == straddleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "PE")))
                        {
                            for (int i = 0; i < hedgeQty; i++)
                            {
                                TransactionWatch.TransactionMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg, Color.Blue);
                                TransactionWatch.ErrorMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg);
                                System.Threading.Thread.Sleep(10);
                            }
                        }
                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == strangleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "CE")))
                        {
                            for (int i = 0; i < hedgeQty; i++)
                            {
                                TransactionWatch.TransactionMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg, Color.Blue);
                                TransactionWatch.ErrorMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg);
                                System.Threading.Thread.Sleep(10);
                            }
                        }
                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == strangleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "PE")))
                        {
                            for (int i = 0; i < hedgeQty; i++)
                            {
                                TransactionWatch.TransactionMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg, Color.Blue);
                                TransactionWatch.ErrorMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg);
                                System.Threading.Thread.Sleep(10);
                            }
                        }

                        watch.AlertLevel = watch.AlertLevel + 1;
                        alert = watch.AlertLevel;
                        double straddleAvg = watch.StraddlAvg;
                        foreach (var watch2 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(watch.StrategyName))))
                        {
                            double prvDrawDown = 0;
                            if (watch.AlertLevel == 2)
                            {
                                prvDrawDown = watch2.round1Point;
                                watch2.StrategyDrawDown = watch2.round2Point;
                            }
                            if (watch.AlertLevel == 3)
                            {
                                prvDrawDown = watch2.round2Point;
                                watch2.StrategyDrawDown = watch2.round3Point;
                            }
                            if (watch.AlertLevel == 4)
                            {
                                prvDrawDown = watch2.round3Point;
                                watch2.StrategyDrawDown = watch2.round4Point;
                            }
                            watch2.AlertLevel = alert;
                            watch2.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch2.StrategyDrawDown;
                            watch2.StraddlAvg = (straddleAvg + prvDrawDown);
                            watch2.RowData.Cells[WatchConst.PrvStrategyAvg].Value = straddleAvg;
                            watch2.prvStraddleAvg = straddleAvg;
                            watch2.RowData.Cells[WatchConst.StrategyAvg].Value = Math.Round(watch2.StraddlAvg, 2);
                        }
                    }
                    else
                    {
                        watch.AlertLevel = watch.AlertLevel + 1;
                        alert = watch.AlertLevel;

                        double straddleAvg = watch.StraddlAvg;

                        TransactionWatch.ErrorMessage("Alert|" + watch.Leg1.ContractInfo.Symbol + "|Straddle|" + straddleAvg + "|Level|" + (watch.AlertLevel - 1) + "|DrawDown|" + watch.StrategyDrawDown + "|HedgeQty|" + hedgeQty);
                        foreach (var watch2 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(watch.StrategyName))))
                        {
                            double prvDrawDown = 0;
                            if (watch.AlertLevel == 2)
                            {
                                prvDrawDown = watch2.round1Point;
                                watch2.StrategyDrawDown = watch2.round2Point;
                            }
                            if (watch.AlertLevel == 3)
                            {
                                prvDrawDown = watch2.round2Point;
                                watch2.StrategyDrawDown = watch2.round3Point;
                            }
                            if (watch.AlertLevel == 4)
                            {
                                prvDrawDown = watch2.round3Point;
                                watch2.StrategyDrawDown = watch2.round4Point;
                            }
                            watch2.AlertLevel = alert;
                            watch2.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch2.StrategyDrawDown;
                            watch2.StraddlAvg = (straddleAvg + prvDrawDown);
                            watch2.RowData.Cells[WatchConst.PrvStrategyAvg].Value = straddleAvg;
                            watch2.prvStraddleAvg = straddleAvg;
                            watch2.RowData.Cells[WatchConst.StrategyAvg].Value = Math.Round(watch2.StraddlAvg, 2);
                        }

                        MessageBox.Show("Hedge Alert Straddle Avg Price =  " + watch.StraddlAvg + " StraddleDD = " + watch.StrategyDrawDown + " HedgeQty = " + hedgeQty);
                    }
                }
            }
        }

        public void HedgeWithHedge(MarketWatch watch)
        {
            string _strategyName = watch.StrategyName;
            const char fieldSeparator = '_';
            List<string> split = _strategyName.Split(fieldSeparator).ToList();
            string _findStrategy = split[0] + "_" + split[1];
            int PosInt = 0;
            int hedgeQty = 0;
            string straddleHedgeStrategy = _findStrategy + "_Straddle";
            string strangleHedgeStrategy = _findStrategy + "_Strangle";

            foreach (var watch2 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == _findStrategy)))
            {
                if (PosInt > watch2.posInt)
                {
                    PosInt = watch2.posInt;
                }
            }
            int round1Qty = Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round1Percent)) / 100);
            int round2Qty = Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round1Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round2Percent)) / 100);
            int round3Qty = Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round1Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round2Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round3Percent)) / 100);
            int round4Qty = Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round1Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round2Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round3Percent)) / 100) + Convert.ToInt32((Math.Abs(PosInt) * Convert.ToInt32(watch.round4Percent)) / 100);
            int roundNo = 0;
            foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == straddleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "CE")))
            {
                if (PosInt != 0)
                {
                    if (!watch.Alert)
                    {
                        if (round1Qty > Math.Abs(watch1.posInt))
                        {
                            hedgeQty = round1Qty;
                            roundNo = 1;
                        }
                        else if (round2Qty > Math.Abs(watch1.posInt))
                        {
                            hedgeQty = round2Qty - round1Qty;
                            roundNo = 2;
                        }
                        else if (round3Qty > Math.Abs(watch1.posInt))
                        {
                            hedgeQty = round3Qty - (round2Qty);
                            roundNo = 3;
                        }
                        else if (round4Qty > Math.Abs(watch1.posInt))
                        {
                            hedgeQty = round4Qty - (round3Qty);
                            roundNo = 4;
                        }
                        else if (Math.Abs(PosInt) == Math.Abs(watch1.posInt))
                            hedgeQty = 0;
                    }
                    else
                    {
                        if (watch.AlertLevel == 1)
                        {
                            hedgeQty = round1Qty;
                            roundNo = 1;
                        }
                        else if (watch.AlertLevel == 2)
                        {
                            hedgeQty = round2Qty - round1Qty;
                            roundNo = 2;
                        }
                        else if (watch.AlertLevel == 3)
                        {
                            hedgeQty = round3Qty - (round2Qty);
                            roundNo = 3;
                        }
                        else if (watch.AlertLevel == 4)
                        {
                            hedgeQty = round4Qty - (round3Qty);
                            roundNo = 4;
                        }
                        else
                            hedgeQty = 0;
                    }
                }
            }
            foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == strangleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "CE")))
            {
                if (PosInt != 0)
                {
                    if (!watch.Alert)
                    {
                        if (round1Qty > Math.Abs(watch1.posInt))
                        {
                            hedgeQty = round1Qty;
                            roundNo = 1;
                        }
                        else if (round2Qty > Math.Abs(watch1.posInt))
                        {
                            hedgeQty = round2Qty - round1Qty;
                            roundNo = 2;
                        }
                        else if (round3Qty > Math.Abs(watch1.posInt))
                        {
                            hedgeQty = round3Qty - (round2Qty);
                            roundNo = 3;
                        }
                        else if (round4Qty > Math.Abs(watch1.posInt))
                        {
                            hedgeQty = round4Qty - (round3Qty);
                            roundNo = 4;
                        }
                        else if (Math.Abs(PosInt) == Math.Abs(watch1.posInt))
                            hedgeQty = 0;
                    }
                    else
                    {
                        if (watch.AlertLevel == 1)
                        {
                            hedgeQty = round1Qty;
                            roundNo = 1;
                        }
                        else if (watch.AlertLevel == 2)
                        {
                            hedgeQty = round2Qty - round1Qty;
                            roundNo = 2;
                        }
                        else if (watch.AlertLevel == 3)
                        {
                            hedgeQty = round3Qty - (round2Qty);
                            roundNo = 3;
                        }
                        else if (watch.AlertLevel == 4)
                        {
                            hedgeQty = round4Qty - (round3Qty);
                            roundNo = 4;
                        }
                        else
                            hedgeQty = 0;
                    }
                }
            }
            if (hedgeQty == 0)
                return;
            if ((watch.StraddlAvg + watch.StrategyDrawDown) < watch.straddleMktWind)
            {
                int alert = 0;
                if (!watch.Alert)
                {
                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == straddleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "CE")))
                    {
                        for (int i = 0; i < hedgeQty; i++)
                        {
                            TransactionWatch.TransactionMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg, Color.Blue);
                            TransactionWatch.ErrorMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg);
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == straddleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "PE")))
                    {
                        for (int i = 0; i < hedgeQty; i++)
                        {
                            TransactionWatch.TransactionMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg, Color.Blue);
                            TransactionWatch.ErrorMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg);
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == strangleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "CE")))
                    {
                        for (int i = 0; i < hedgeQty; i++)
                        {
                            TransactionWatch.TransactionMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg, Color.Blue);
                            TransactionWatch.ErrorMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg);
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                    foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName) == strangleHedgeStrategy) && (x.Leg1.ContractInfo.Series == "PE")))
                    {
                        for (int i = 0; i < hedgeQty; i++)
                        {
                            TransactionWatch.TransactionMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg, Color.Blue);
                            TransactionWatch.ErrorMessage("Hedge|" + watch1.Leg1.ContractInfo.Symbol + "|" + watch1.Leg1.ContractInfo.StrikePrice + "|" + watch1.Leg1.ContractInfo.Series + "|" + "Traded" + "|PrvAvg|" + watch1.prvStraddleAvg + "|StraddlAvg|" + watch.StraddlAvg);
                            System.Threading.Thread.Sleep(10);
                        }
                    }
                    watch.AlertLevel = watch.AlertLevel + 1;
                    alert = watch.AlertLevel;
                    double straddleAvg = watch.StraddlAvg;
                    foreach (var watch2 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(_findStrategy))))
                    {
                        double prvDrawDown = 0;
                        if (watch.AlertLevel == 2)
                        {
                            prvDrawDown = watch2.round1Point;
                            watch2.StrategyDrawDown = watch2.round2Point;
                        }
                        if (watch.AlertLevel == 3)
                        {
                            prvDrawDown = watch2.round2Point;
                            watch2.StrategyDrawDown = watch2.round3Point;
                        }
                        if (watch.AlertLevel == 4)
                        {
                            prvDrawDown = watch2.round3Point;
                            watch2.StrategyDrawDown = watch2.round4Point;
                        }
                        watch2.AlertLevel = alert;
                        watch2.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch2.StrategyDrawDown;
                        watch2.StraddlAvg = (straddleAvg + prvDrawDown);
                        watch2.RowData.Cells[WatchConst.PrvStrategyAvg].Value = straddleAvg;
                        watch2.prvStraddleAvg = straddleAvg;
                        watch2.RowData.Cells[WatchConst.StrategyAvg].Value = Math.Round(watch2.StraddlAvg, 2);
                    }
                }
                else
                {
                    watch.AlertLevel = watch.AlertLevel + 1;
                    alert = watch.AlertLevel;
                    double straddleAvg = watch.StraddlAvg;

                    TransactionWatch.ErrorMessage("Alert|" + watch.Leg1.ContractInfo.Symbol + "|Straddle|" + straddleAvg + "|Level|" + (watch.AlertLevel - 1) + "|DrawDown|" + watch.StrategyDrawDown + "|HedgeQty|" + hedgeQty);
                    foreach (var watch2 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(_findStrategy))))
                    {
                        double prvDrawDown = 0;
                        if (watch.AlertLevel == 2)
                        {
                            prvDrawDown = watch2.round1Point;
                            watch2.StrategyDrawDown = watch2.round2Point;
                        }
                        if (watch.AlertLevel == 3)
                        {
                            prvDrawDown = watch2.round2Point;
                            watch2.StrategyDrawDown = watch2.round3Point;
                        }
                        if (watch.AlertLevel == 4)
                        {
                            prvDrawDown = watch2.round3Point;
                            watch2.StrategyDrawDown = watch2.round4Point;
                        }
                        watch2.AlertLevel = alert;
                        watch2.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch2.StrategyDrawDown;
                        watch2.StraddlAvg = (straddleAvg + prvDrawDown);
                        watch2.RowData.Cells[WatchConst.PrvStrategyAvg].Value = straddleAvg;
                        watch2.prvStraddleAvg = straddleAvg;
                        watch2.RowData.Cells[WatchConst.StrategyAvg].Value = Math.Round(watch2.StraddlAvg, 2);
                    }
                    MessageBox.Show("Hedge Alert Straddle Avg Price =  " + watch.StraddlAvg + " StraddleDD = " + watch.StrategyDrawDown + " HedgeQty = " + hedgeQty);
                }
            }
        }


        private string[] GetFinNiftyExpiryDates(DataTable expTable)
        {
            try
            {
                var dateList = new HashSet<String>();
                var dateList1 = new HashSet<String>();
                AppGlobal.monthint = new List<int>();
                foreach (DataRow r1 in expTable.Rows)
                {
                    if ((r1[DBConst.InstrumentName].ToString() == "FUTIDX" || r1[DBConst.InstrumentName].ToString() == "FUTSTK") && r1[DBConst.Symbol].ToString() == "FINNIFTY")
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

        #region AssignMarketStructValue

        public void LSL_Strangle_AvgPrice()
        {
            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watch = AppGlobal.MarketWatch[i];
                if (watch.StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo != "0")
                {
                    double avgPrice = 0;
                    double avgPrice_CE = 0;
                    double avgPrice_PE = 0;
                    foreach (var _watch in AppGlobal.MarketWatch.Where(x => x.uniqueId == watch.UniqueIdLeg1 || x.uniqueId == watch.UniqueIdLeg2))
                    {
                        if (_watch.posInt != 0)
                        {
                            if (_watch.posInt < 0)
                            {
                                if (_watch.Leg1.ContractInfo.Series == "CE")
                                {
                                    avgPrice_CE = _watch.Leg1.N_Price;

                                }
                                else if (_watch.Leg1.ContractInfo.Series == "PE")
                                {
                                    avgPrice_PE = _watch.Leg1.N_Price;

                                }
                                avgPrice = avgPrice + (Math.Abs(_watch.posInt) * _watch.Leg1.N_Price);
                            }
                        }
                    }
                    watch.LSL_AvgPriceCE = Convert.ToDouble(avgPrice_CE);
                    watch.RowData.Cells[WatchConst.LSL_AvgPriceCE].Value = watch.LSL_AvgPriceCE;
                    watch.LSL_AvgPricePE = Convert.ToDouble(avgPrice_PE);
                    watch.RowData.Cells[WatchConst.LSL_AvgPricePE].Value = watch.LSL_AvgPricePE;
                    watch.StrategyAvgPrice = Convert.ToDouble(avgPrice);
                    watch.RowData.Cells[WatchConst.StrategyAvgPrice].Value = watch.StrategyAvgPrice;
                }
            }
        }

        public void AssignMarketStructValue(List<MarketWatch> marketWatchStructure)
        {
            try
            {
                #region AssignMarketWatch
                if (AppGlobal.MarketWatch == null)
                    return;
                AppGlobal.RuleIndexNo = 1;
                dgvMarketWatch.Rows.Clear();
                dgvMarketWatch.Rows.Add();

                int i = 0;
                for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
                {
                    MarketWatch watch = AppGlobal.MarketWatch[index];
                    if (watch.StrategyId == 0)
                    {
                        watch.RowData = dgvMarketWatch.Rows[index];
                        if (watch.Strategy == null)
                        {
                            watch.Strategy = "Strategy_1";
                        }                                             
                        watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                        watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;

                        if (!AppGlobal.RuleMap.ContainsKey(watch.Strategy))
                            AppGlobal.RuleMap.Add(watch.Strategy, new AllDetailsStrategy());
                        AppGlobal.Global_StrategyName = watch.Strategy;
                        watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                        watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
                        watch.Ruleno = AppGlobal.RuleIndexNo;
                        watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                        watch.RowData.Cells[WatchConst.CarryForwardPnl].Value = Math.Round(watch.CarryForwardPnl, 2);
                        watch.StrategyPnl = watch.pnl + watch.Sqpnl + watch.CarryForwardPnl;
                        watch.RowData.Cells[WatchConst.StrategyPnl].Value = Math.Round(watch.StrategyPnl, 2);
                        AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.LightSalmon;
                        DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                        if (watch.Checked)
                        {
                            ToggleButton.Value = "ON";
                            ToggleButton.Style.ForeColor = Color.Green;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;
                        }
                        else
                        {
                            ToggleButton.Value = "OFF";
                            ToggleButton.Style.ForeColor = Color.Red;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;
                        }
                        ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvMarketWatch.Rows[index].Cells[WatchConst.Checked] = ToggleButton;

                        dgvMarketWatch.Rows.Add();
                        AppGlobal.RuleIndexNo++;
                    }
                    else
                    {
                        int dateno = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, DateTime.Now);
                        int expDate = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(watch.Expiry)) + 84800;
                        if (Convert.ToUInt64(expDate) < Convert.ToUInt64(dateno))
                        {
                            if (watch.posInt != 0)
                            {
                                foreach (var _watch in AppGlobal.MarketWatch.Where(x => (Convert.ToInt32(x.StrategyId) == 0) && (Convert.ToString(x.Strategy) == watch.Strategy)))
                                {
                                    _watch.CarryForwardPnl = _watch.CarryForwardPnl + watch.Sqpnl;
                                    _watch.RowData.Cells[WatchConst.CarryForwardPnl].Value = Math.Round(_watch.CarryForwardPnl, 2);
                                }
                                AppGlobal.DuePnl = AppGlobal.DuePnl + Math.Round(watch.pnl, 2);
                                TransactionWatch.ErrorMessage("Strategy|" + watch.StrategyId + "|Unique|" + watch.uniqueId + "|L1Strike|" + watch.Leg1.ContractInfo.StrikePrice
                                                               + "|L2Strike|" + watch.Leg2.ContractInfo.StrikePrice + "|Expiry|" + watch.Expiry + "|DuePnl|" + Math.Round(watch.pnl, 2));
                            }
                            AppGlobal.MarketWatch.RemoveAt(index);
                            index--;
                            continue;
                        }
                       

                        if (watch.TLI_StrategyId == 12211 || watch.TLI_StrategyId == 1113 || watch.TLI_StrategyId == 1114)
                        {
                            if (AppGlobal.TLI_Strangle < watch.TLI_UniqueId)
                            {
                                AppGlobal.TLI_Strangle = watch.TLI_UniqueId;
                            }
                        }
                        else if (watch.TLI_StrategyId == 32211)
                        {
                            if (AppGlobal.LSL_Strangle < watch.LSL_UniqueId)
                                AppGlobal.LSL_Strangle = watch.LSL_UniqueId;
                            watch.LSL_StopLossFlg = false;
                        }
                        watch.RowData = dgvMarketWatch.Rows[index];
                        watch.enterCount = 0;
                        int Lotsize = 0;
                        string symbol = "";
                        if (watch.Leg1.Counter == 1)
                        {
                            Lotsize = watch.Leg1.ContDetail.LotSize;
                            symbol = watch.Leg1.ContractInfo.Symbol;
                        }
                        string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, Convert.ToInt32(watch.Leg1.expiryUniqueID)));
                        int rem = (int)CalculatorUtils.CalculateDay(Convert.ToDateTime(month));
                        int max = Math.Max(rem, 0);
                        watch.RemainingDay = max;
                        watch.URem_Day = max;
                        watch.wCount = 0;
                        watch.uwCount = 0;
                        if (watch.StrategyId == 1331 || watch.StrategyId == 1221)
                        {

                        }
                        else
                        {
                            watch.Leg4 = new Straddle.AppClasses.Leg();
                            watch.Leg4.ContractInfo.TokenNo = "0";
                            watch.Leg4.Counter = 0;
                        }
                        if (watch.Strategy == null)
                        {
                            watch.Strategy = "Strategy_1";
                        }
                        watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                        if (watch.StrategyId == 32211 && watch.StrategyName.Contains("LSL"))
                            watch.RowData.Cells[WatchConst.TLI_Uniqueid].Value = watch.LSL_UniqueId;
                        else
                            watch.RowData.Cells[WatchConst.TLI_Uniqueid].Value = watch.TLI_UniqueId;
                        watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                        watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                        watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                        watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                        if (watch.TLI_StrategyId == 12211 || watch.TLI_StrategyId == 32211 || watch.TLI_StrategyId == 1113 || watch.TLI_StrategyId == 1114)
                        {
                            
                            if (watch.Leg2.ContractInfo.TokenNo != "0")
                            {
                                watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                                watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                                watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                                watch.UniqueIdLeg1 = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo + 1));
                                watch.UniqueIdLeg2 = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo + 2));
                                watch.RowData.Cells[WatchConst.UniqueIdL1].Value = watch.UniqueIdLeg1;
                                watch.RowData.Cells[WatchConst.UniqueIdL2].Value = watch.UniqueIdLeg2;
                                watch.Ruleno = AppGlobal.RuleIndexNo;
                                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                            }
                            else
                            {
                                watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                                watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                                watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                                watch.Ruleno = AppGlobal.RuleIndexNo;
                                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                            }
                        }
                        else
                        {
                            watch.uniqueId = Convert.ToUInt64(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                            watch.displayUniqueId = Convert.ToString(AppGlobal.GUI_ID + Convert.ToUInt64(AppGlobal.RuleIndexNo));
                            watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                            watch.Ruleno = AppGlobal.RuleIndexNo;
                            watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                        }
                        watch.Gui_id = AppGlobal.GUI_ID;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
                        watch.windCount = 0;
                        watch.UnwindCount = 0;
                        watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                        watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(watch.MktWind, 2);
                        watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(watch.MktunWind, 2);
                        watch.RowData.Cells[WatchConst.Track].Value = watch.Track;
                        watch.StrategyDrawDown = watch.round1Point;
                        watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                        watch.IsStrikeReq = false;
                        watch.not_got_first_tick = false;
                        watch.Hedgeflg = false;
                        watch.StraddlAvg = 0;
                        watch.prvStraddleAvg = 0;
                        watch.AlertLevel = 1;
                        if (watch.StrategyId != 91 && watch.StrategyId == 3311)
                        {
                            watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.Leg1.N_Price;
                        }
                        else if (watch.StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo == "0")
                        {
                            watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.Leg1.N_Price;
                        }
                        else
                        {
                            if (watch.Leg1.N_Qty != 0)
                            {
                                watch.Leg1.Net_Qty = (watch.Leg1.Sell_Qty - watch.Leg1.Buy_Qty);
                                watch.Leg1.A_Value = (watch.Leg1.S_Value - watch.Leg1.B_Value);
                                watch.RowData.Cells[WatchConst.AvgPrice].Value = Math.Round(watch.Leg1.A_Value / watch.Leg1.Net_Qty, 2);
                            }
                            else
                            {
                                watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.Leg1.N_Price;
                            }
                        }
                        if (watch.StrategyId == 23434)
                        {
                            watch.posInt = ((watch.Leg1.N_Qty) / (Lotsize));
                            watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                            if (watch.posInt > 0)
                            {
                                watch.PosType = "Wind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else if (watch.posInt < 0)
                            {
                                watch.PosType = "Unwind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else
                            {
                                watch.PosType = "None";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                        }
                        else if (watch.StrategyId == 121 || watch.StrategyId == 1331 || watch.StrategyId == 1221)
                        {
                            watch.posInt = (watch.Leg1.N_Qty / (Lotsize)) * -1;
                            watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                            if (watch.posInt > 0)
                            {
                                watch.PosType = "Wind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else if (watch.posInt < 0)
                            {
                                watch.PosType = "Unwind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else
                            {
                                watch.PosType = "None";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                        }
                        else
                        {
                            watch.posInt = ((watch.Leg1.N_Qty) / (Lotsize));
                            watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                            if (watch.posInt > 0)
                            {
                                watch.PosType = "Wind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else if (watch.posInt < 0)
                            {
                                watch.PosType = "Unwind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else
                            {
                                watch.PosType = "None";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                        }
                        if (watch.StrategyId == 91)
                        {
                            watch.premium = watch.Leg1.N_Price * watch.posInt * watch.Leg1.ContDetail.LotSize * -1;
                            watch.RowData.Cells[WatchConst.Premium].Value = Math.Round(watch.premium, 2);
                            if (watch.Leg1.ContractInfo.Series == "CE")
                            {
                                AppGlobal.CallMTM = AppGlobal.CallMTM + watch.premium;
                                if (watch.posInt > 0)
                                {
                                    AppGlobal.CallBuyMTM = AppGlobal.CallBuyMTM + watch.premium;
                                }
                                else
                                {
                                    AppGlobal.CallSellMTM = AppGlobal.CallSellMTM + watch.premium;
                                }
                            }
                            else
                            {
                                AppGlobal.PutMTM = AppGlobal.PutMTM + watch.premium;
                                if (watch.posInt > 0)
                                {
                                    AppGlobal.PutBuyMTM = AppGlobal.PutBuyMTM + watch.premium;
                                }
                                else
                                {
                                    AppGlobal.PutSellMTM = AppGlobal.PutSellMTM + watch.premium;
                                }
                            }
                        }
                        if (watch.StrategyId == 91 || watch.StrategyId == 3311)
                        {
                            if (watch.posInt < 0)
                            {
                                if (watch.Leg1.ContractInfo.Symbol == "NIFTY")
                                    watch.MarginUtilise = Math.Abs(watch.posInt) * AppGlobal.niftyMargin;
                                else if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                    watch.MarginUtilise = Math.Abs(watch.posInt) * AppGlobal.bankniftyMargin;
                                else if (watch.Leg1.ContractInfo.Symbol == "FINNIFTY")
                                    watch.MarginUtilise = Math.Abs(watch.posInt) * AppGlobal.niftyMargin;
                            }
                            else
                            {
                                watch.MarginUtilise = 0;
                            }
                        }

                        AppGlobal.OverallMarginUtilize = AppGlobal.OverallMarginUtilize + watch.MarginUtilise;
                        //premium
                        AppGlobal.overallPremium = AppGlobal.overallPremium + watch.premium;
                        premiumlbl.Text = Convert.ToString(Math.Round(AppGlobal.overallPremium / 10000000, 3));
                        watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                        watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                        watch.RowData.Cells[WatchConst.MarginUtilise].Value = watch.MarginUtilise;
                        watch.TradedQty = watch.Leg1.ContDetail.LotSize * watch.posInt;
                        watch.RowData.Cells[WatchConst.TradedQty].Value = watch.TradedQty;
                        watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                        watch.RowData.Cells[WatchConst.LSL_L1PosInt].Value = watch.L1PosInt;
                        watch.RowData.Cells[WatchConst.LSL_L2PosInt].Value = watch.L2PosInt;
                        #region Leg1
                        if (watch.Leg1.Counter == 1)
                        {
                            if (watch.Leg1 != null)
                            {
                                watch.RowData.Cells[WatchConst.L1Strike].Value = Convert.ToString(watch.Leg1.ContractInfo.StrikePrice);
                                watch.RowData.Cells[WatchConst.L1Series].Value = Convert.ToString(watch.Leg1.ContractInfo.Series);
                                watch.RowData.Cells[WatchConst.Token].Value = Convert.ToString(watch.Leg1.ContractInfo.TokenNo);
                                watch.RowData.Cells[WatchConst.Ratio1].Value = Convert.ToString(watch.Leg1.Ratio);
                                watch.RowData.Cells[WatchConst.Symbol].Value = Convert.ToString(watch.Leg1.ContractInfo.Symbol);
                                watch.RowData.Cells[WatchConst.Delta].Value = Convert.ToDouble(watch.Delta);
                                watch.RowData.Cells[WatchConst.Vega].Value = Convert.ToDouble(watch.Vega);
                                watch.RowData.Cells[WatchConst.Gamma].Value = Convert.ToDouble(watch.Gamma);
                                watch.RowData.Cells[WatchConst.Theta].Value = Convert.ToDouble(watch.Theta);
                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        #endregion

                        #region Leg2
                        if (watch.Leg2.Counter == 1)
                        {
                            if (watch.Leg1 != null)
                            {
                                watch.RowData.Cells[WatchConst.L2Strike].Value = Convert.ToString(watch.Leg2.ContractInfo.StrikePrice);
                                watch.RowData.Cells[WatchConst.L2Series].Value = Convert.ToString(watch.Leg2.ContractInfo.Series);

                                watch.RowData.Cells[WatchConst.Ratio2].Value = Convert.ToString(watch.Leg2.Ratio);
                                watch.RowData.Cells[WatchConst.Token2].Value = Convert.ToString(watch.Leg2
                                    .ContractInfo.TokenNo);

                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)];
                                    list.Add(index);

                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        #endregion

                        #region Leg3
                        if (watch.Leg3.Counter == 1)
                        {
                            if (watch.Leg3 != null)
                            {
                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg3.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg3.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg3.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        #endregion

                        #region Leg4
                        if (watch.Leg4.Counter == 1)
                        {
                            if (watch.Leg4 != null)
                            {


                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg4.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg4.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg4.ContractInfo.TokenNo), list);
                                }

                            }
                        }

                        #endregion

                        uint _expiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(watch.Expiry));
                        string _expiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, _expiry).ToString("yyyyMMMdd");
                        string _sf12 = Convert.ToString(_expiry1);
                        string _sf22 = _sf12.Substring(0, 4);
                        string _sf32 = _sf12.Substring(4, 3);
                        string _sf42 = _sf12.Substring(7, 2);
                        int _montf = DateTime.ParseExact(_sf32, "MMM", new CultureInfo("en-US")).Month;
                        System.Globalization.DateTimeFormatInfo _mffi1 = new System.Globalization.DateTimeFormatInfo();
                        string _monStringf = "";
                        if (_montf <= 9)
                        {
                            _monStringf = "0" + Convert.ToString(_montf);
                        }
                        else
                        {
                            _monStringf = Convert.ToString(_montf);
                        }
                        string _sf52 = _sf22 + _monStringf + _sf42;

                        #region Future Exp
                        int currentmonth = _montf;
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

                        if (watch.Leg1.ContractInfo.Symbol != "FINNIFTY")
                        {
                            string strFilter2 = "";
                            string TokenNo = "";
                            if (watch.Leg1.ContractInfo.Symbol == "NIFTY" || watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + symbol + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                            else
                                strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + symbol + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                            DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                            foreach (DataRow dr in dr1F)
                            {
                                TokenNo = dr["TokenNo"].ToString();
                            }

                            if (watch.niftyLeg != null)
                            {
                                watch.niftyLeg.ContractInfo.TokenNo = TokenNo;
                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        else
                        {
                            if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                            {
                                List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                                list.Add(index);
                            }
                            else
                            {
                                List<int> list = new List<int>();
                                list.Add(index);
                                AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                            }
                        }
                        DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                        if (watch.Checked)
                        {
                            ToggleButton.Value = "ON";
                            ToggleButton.Style.ForeColor = Color.Green;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;
                        }
                        else
                        {
                            ToggleButton.Value = "OFF";
                            ToggleButton.Style.ForeColor = Color.Red;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;
                        }
                        ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvMarketWatch.Rows[index].Cells[WatchConst.Checked] = ToggleButton;

                        watch.SL_BuyOrderflg = false;
                        watch.SL_SellOrderflg = false;
                        watch.DD_BuyOrderflg = false;
                        watch.DD_SellOrderflg = false;
                        watch.Alert_BuyOrderflg = false;
                        watch.Alert_SellOrderflg = false;
                        watch.LSL_StopLossFlg = false;
                        watch.StoplossTrade = false;
                        watch.ProfitTrade = false;
                        watch.TrailTrade = false;

                        watch.LSL_StopLossPercent = 0;
                        watch.RowData.Cells[WatchConst.TGBuyPrice].Value = watch.TGBuyPrice;
                        watch.RowData.Cells[WatchConst.TGSellPrice].Value = watch.TGBuyPrice;
                        watch.RowData.Cells[WatchConst.AP_BuySL].Value = watch.AP_BuySL;
                        watch.RowData.Cells[WatchConst.AP_SellSL].Value = watch.AP_SellSL;
                        watch.RowData.Cells[WatchConst.SL_BuyQty].Value = watch.SL_BuyQty;
                        watch.RowData.Cells[WatchConst.SL_SellQty].Value = watch.SL_SellQty;
                        watch.RowData.Cells[WatchConst.DD_bm_Buy].Value = watch.DD_bm_Buy;
                        watch.RowData.Cells[WatchConst.DD_bm_Sell].Value = watch.DD_bm_Sell;
                        watch.RowData.Cells[WatchConst.DD_BuyQty].Value = watch.DD_BuyQty;
                        watch.RowData.Cells[WatchConst.DD_SellQty].Value = watch.DD_SellQty;
                        watch.Bidding_start = 0;
                        


                        if (watch.StrategyId == 91)
                        {
                            if (!watch.StrategyName.Contains("TLI"))
                            {
                                if (watch.IsStrikeReq)
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                else
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.Aqua;
                            }                          
                            else
                            {
                                if (watch.IsStrikeReq)
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                else
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.LightYellow;
                            }
                        }
                        else
                        {
                            if (watch.TLI_StrategyId == 12211 || watch.TLI_StrategyId == 32211 || watch.TLI_StrategyId == 1113 || watch.TLI_StrategyId == 1114)
                            {
                                if (watch.Leg2.ContractInfo.TokenNo == "0")
                                {
                                    if (watch.IsStrikeReq)
                                        AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                    else
                                        AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.LightYellow;
                                }
                                else
                                {
                                    if (watch.IsStrikeReq)
                                        AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                    else
                                        AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.Aqua;
                                }
                            }
                            else
                            {
                                if (watch.IsStrikeReq)
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                else
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.Aqua;
                            }
                        }
                        dgvMarketWatch.Rows.Add();
                        if (watch.TLI_StrategyId == 12211 || watch.TLI_StrategyId == 32211 || watch.TLI_StrategyId == 1113 || watch.TLI_StrategyId == 1114)
                        {
                            
                            if (watch.Leg2.ContractInfo.TokenNo != "0")
                            {
                                AppGlobal.RuleIndexNo++;
                                AppGlobal.RuleIndexNo++;
                                AppGlobal.RuleIndexNo++;
                            }
                            else
                            { AppGlobal.RuleIndexNo++; }
                            //else
                            //{
                                //for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
                                //{
                                //    MarketWatch _watch = AppGlobal.MarketWatch[i];

                                //    if (_watch.StrategyId == 91)
                                //    {
                                //        if (watch.TLI_StrategyId == _watch.TLI_StrategyId && watch.Leg1.ContractInfo.TokenNo == _watch.Leg1.ContractInfo.TokenNo && watch.UniqueIdLeg1 != 0 && _watch.TLI_UniqueId == watch.TLI_UniqueId)
                                //        {
                                           
                                //            _watch.uniqueId = watch.UniqueIdLeg1;
                                //            _watch.displayUniqueId = Convert.ToString(watch.UniqueIdLeg1);
                                //            watch.RowData.Cells[WatchConst.Unique].Value = _watch.displayUniqueId;
                                //            int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                                //            _watch.Ruleno = remainder + 1;
                                //            watch.RowData.Cells[WatchConst.Rule].Value = _watch.Ruleno;

                                //        }
                                //        if (watch.TLI_StrategyId == _watch.TLI_StrategyId && watch.Leg1.ContractInfo.TokenNo == _watch.Leg2.ContractInfo.TokenNo && watch.UniqueIdLeg2 != 0 && _watch.TLI_UniqueId == watch.TLI_UniqueId)
                                //        {
                                //            _watch.uniqueId = _watch.UniqueIdLeg2;
                                //            _watch.displayUniqueId = Convert.ToString(_watch.UniqueIdLeg2);
                                //            watch.RowData.Cells[WatchConst.Unique].Value = _watch.displayUniqueId;
                                //            int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                                //            _watch.Ruleno = remainder + 2;
                                //            watch.RowData.Cells[WatchConst.Rule].Value = _watch.Ruleno;
                                //        }
                                //    }
                                //    if (_watch.TLI_StrategyId == 91)
                                //    {
                                //        if (watch.TLI_StrategyId == _watch.TLI_StrategyId && watch.Leg1.ContractInfo.TokenNo == _watch.Leg1.ContractInfo.TokenNo && _watch.UniqueIdLeg1 != 0 && _watch.LSL_UniqueId == watch.LSL_UniqueId)
                                //        {
                                //            //watch.uniqueId = _watch.UniqueIdLeg1;
                                //            //watch.displayUniqueId = Convert.ToString(_watch.UniqueIdLeg1);
                                //            //watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                                //            //int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                                //            //watch.Ruleno = remainder + 1;
                                //            //watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;


                                //            _watch.uniqueId = watch.UniqueIdLeg1;
                                //            _watch.displayUniqueId = Convert.ToString(watch.UniqueIdLeg1);
                                //            watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                                //            int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                                //            _watch.Ruleno = remainder + 1;
                                //            watch.RowData.Cells[WatchConst.Rule].Value = _watch.Ruleno;

                                //        }
                                //        if (watch.TLI_StrategyId == _watch.TLI_StrategyId && watch.Leg1.ContractInfo.TokenNo == _watch.Leg2.ContractInfo.TokenNo && _watch.UniqueIdLeg2 != 0 && _watch.LSL_UniqueId == watch.LSL_UniqueId)
                                //        {
                                //            _watch.uniqueId = _watch.UniqueIdLeg2;
                                //            _watch.displayUniqueId = Convert.ToString(_watch.UniqueIdLeg2);
                                //            watch.RowData.Cells[WatchConst.Unique].Value = _watch.displayUniqueId;
                                //            int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                                //            _watch.Ruleno = remainder + 2;
                                //            watch.RowData.Cells[WatchConst.Rule].Value = _watch.Ruleno;
                                //        }
                                //    }
                                // }
                                
                            //}
                        }
                        else
                            AppGlobal.RuleIndexNo++;

                        if (watch.StrategyName.Contains("MainJodi"))
                        {
                            const char fieldSeparator = '_';
                            string strategyType = watch.StrategyName;
                            List<string> split = strategyType.Split(fieldSeparator).ToList();
                            int ModifyNo = Convert.ToInt32(split[1]);
                            if (AppGlobal.StrategyRuleIndexNo < ModifyNo)
                                AppGlobal.StrategyRuleIndexNo = ModifyNo;
                        }
                    }
                }
                AssignUniqueId();
                Sum();
                #endregion
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "AssignMarketStructValue")
                             , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }


        public void AssignUniqueId()
        {
            for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
            {
                MarketWatch watch = AppGlobal.MarketWatch[index];
                for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
                {
                    MarketWatch _watch = AppGlobal.MarketWatch[i];

                    if (_watch.StrategyId == 91)
                    {
                        if (watch.TLI_StrategyId == _watch.TLI_StrategyId && _watch.Leg1.ContractInfo.TokenNo == watch.Leg1.ContractInfo.TokenNo && watch.UniqueIdLeg1 != 0 && _watch.TLI_UniqueId == watch.TLI_UniqueId)
                        {

                            _watch.uniqueId = watch.UniqueIdLeg1;
                            _watch.displayUniqueId = Convert.ToString(watch.UniqueIdLeg1);
                            _watch.RowData.Cells[WatchConst.Unique].Value = _watch.displayUniqueId;
                            int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                            _watch.Ruleno = remainder;
                            _watch.RowData.Cells[WatchConst.Rule].Value = _watch.Ruleno;

                        }
                        if (watch.TLI_StrategyId == _watch.TLI_StrategyId && _watch.Leg1.ContractInfo.TokenNo == watch.Leg2.ContractInfo.TokenNo && watch.UniqueIdLeg2 != 0 && _watch.TLI_UniqueId == watch.TLI_UniqueId)
                        {
                            _watch.uniqueId = watch.UniqueIdLeg2;
                            _watch.displayUniqueId = Convert.ToString(watch.UniqueIdLeg2);
                            _watch.RowData.Cells[WatchConst.Unique].Value = _watch.displayUniqueId;
                            int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                            _watch.Ruleno = remainder;
                            _watch.RowData.Cells[WatchConst.Rule].Value = _watch.Ruleno;
                        }
                    }
                    if (_watch.TLI_StrategyId == 91)
                    {
                        if (watch.TLI_StrategyId == _watch.TLI_StrategyId && watch.Leg1.ContractInfo.TokenNo == _watch.Leg1.ContractInfo.TokenNo && _watch.UniqueIdLeg1 != 0 && _watch.LSL_UniqueId == watch.LSL_UniqueId)
                        {
                            _watch.uniqueId = watch.UniqueIdLeg1;
                            _watch.displayUniqueId = Convert.ToString(watch.UniqueIdLeg1);
                            _watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                            int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                            _watch.Ruleno = remainder;
                            _watch.RowData.Cells[WatchConst.Rule].Value = _watch.Ruleno;

                        }
                        if (watch.TLI_StrategyId == _watch.TLI_StrategyId && watch.Leg1.ContractInfo.TokenNo == _watch.Leg2.ContractInfo.TokenNo && _watch.UniqueIdLeg2 != 0 && _watch.LSL_UniqueId == watch.LSL_UniqueId)
                        {
                            _watch.uniqueId = watch.UniqueIdLeg2;
                            _watch.displayUniqueId = Convert.ToString(watch.UniqueIdLeg2);
                            _watch.RowData.Cells[WatchConst.Unique].Value = _watch.displayUniqueId;
                            int remainder = Convert.ToInt32(_watch.uniqueId) % 100000;
                            _watch.Ruleno = remainder;
                            _watch.RowData.Cells[WatchConst.Rule].Value = _watch.Ruleno;
                        }
                    }
                }

            }
        }


        public void AssignMarketStructValue_1(List<MarketWatch> marketWatchStructure)
        {
            try
            {
                if (AppGlobal.MarketWatch == null) return;
                dgvMarketWatch.Rows.Clear();
                dgvMarketWatch.Rows.Add();
                bool strike = false;
                for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
                {
                    MarketWatch watch = AppGlobal.MarketWatch[index];
                    if (watch.StrategyId == 0)
                    {
                        watch.RowData = dgvMarketWatch.Rows[index];
                        if (watch.Strategy == null)
                        {
                            watch.Strategy = "Strategy_1";
                        }
                        watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                        watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;

                        if (!AppGlobal.RuleMap.ContainsKey(watch.Strategy))
                            AppGlobal.RuleMap.Add(watch.Strategy, new AllDetailsStrategy());
                        AppGlobal.Global_StrategyName = watch.Strategy;
                        watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                        watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
                        watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                        watch.RowData.Cells[WatchConst.CarryForwardPnl].Value = Math.Round(watch.CarryForwardPnl, 2);
                        watch.StrategyPnl = watch.pnl + watch.Sqpnl + watch.CarryForwardPnl;
                        watch.RowData.Cells[WatchConst.StrategyPnl].Value = Math.Round(watch.StrategyPnl, 2);
                        AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.LightSalmon;
                        DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                        if (watch.Checked)
                        {
                            ToggleButton.Value = "ON";
                            ToggleButton.Style.ForeColor = Color.Green;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;
                        }
                        else
                        {
                            ToggleButton.Value = "OFF";
                            ToggleButton.Style.ForeColor = Color.Red;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;
                        }
                        ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvMarketWatch.Rows[index].Cells[WatchConst.Checked] = ToggleButton;
                        dgvMarketWatch.Rows.Add();
                        strike = true;
                    }
                    else
                    {
                        int dateno = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, DateTime.Now);
                        int expDate = (int)ArisApi_a._arisApi.DateTimeToSecond(Market.NseFO, Convert.ToDateTime(watch.Expiry)) + 84800;
                        if (Convert.ToUInt64(expDate) < Convert.ToUInt64(dateno))
                        {
                            if (watch.pnl != 0)
                            {
                                AppGlobal.DuePnl = AppGlobal.DuePnl + Math.Round(watch.pnl, 2);
                                TransactionWatch.ErrorMessage("Strategy|" + watch.StrategyId + "|Unique|" + watch.uniqueId + "|L1Strike|" + watch.Leg1.ContractInfo.StrikePrice
                                                               + "|L2Strike|" + watch.Leg2.ContractInfo.StrikePrice + "|Expiry|" + watch.Expiry + "|DuePnl|" + Math.Round(watch.pnl, 2));
                            }
                            AppGlobal.MarketWatch.RemoveAt(index);
                            index--;
                            continue;
                        }
                        watch.RowData = dgvMarketWatch.Rows[index];
                        watch.enterCount = 0;
                        int Lotsize = 0;
                        string symbol = "";
                        if (watch.Leg1.Counter == 1)
                        {
                            Lotsize = watch.Leg1.ContDetail.LotSize;
                            symbol = watch.Leg1.ContractInfo.Symbol;
                        }
                        string month = Convert.ToString(ArisApi_a._arisApi.SecondToDateTime(Market.NseFO, Convert.ToInt32(watch.Leg1.expiryUniqueID)));
                        int rem = (int)CalculatorUtils.CalculateDay(Convert.ToDateTime(month));
                        int max = Math.Max(rem, 0);
                        watch.RemainingDay = max;
                        watch.URem_Day = max;
                        watch.wCount = 0;
                        watch.uwCount = 0;
                        if (watch.StrategyId == 1331 || watch.StrategyId == 1221)
                        {

                        }
                        else
                        {
                            watch.Leg4 = new Straddle.AppClasses.Leg();
                            watch.Leg4.ContractInfo.TokenNo = "0";
                            watch.Leg4.Counter = 0;
                        }
                        if (watch.Strategy == null)
                        {
                            watch.Strategy = "Strategy_1";
                        }
                        watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;
                        watch.RowData.Cells[WatchConst.Wind].Value = watch.Wind;
                        watch.RowData.Cells[WatchConst.UnWind].Value = watch.unWind;
                        watch.RowData.Cells[WatchConst.FQty].Value = watch.Over;
                        watch.RowData.Cells[WatchConst.RQty].Value = watch.Round;
                        watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                        watch.Gui_id = AppGlobal.GUI_ID;
                        watch.RowData.Cells[WatchConst.UniqueIdL1].Value = watch.UniqueIdLeg1;
                        watch.RowData.Cells[WatchConst.UniqueIdL2].Value = watch.UniqueIdLeg2;
                        if (watch.StrategyId == 32211 && watch.StrategyName.Contains("LSL"))
                            watch.RowData.Cells[WatchConst.TLI_Uniqueid].Value = watch.LSL_UniqueId;
                        else
                            watch.RowData.Cells[WatchConst.TLI_Uniqueid].Value = watch.TLI_UniqueId;
                        watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                        watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                        watch.RowData.Cells[WatchConst.Track].Value = watch.Track;
                        watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                        watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
                        watch.windCount = 0;
                        watch.UnwindCount = 0;
                        watch.RowData.Cells[WatchConst.Expiry].Value = watch.Expiry;
                        watch.RowData.Cells[WatchConst.FSpread].Value = Math.Round(watch.MktWind, 2);
                        watch.RowData.Cells[WatchConst.RSpread].Value = Math.Round(watch.MktunWind, 2);
                        watch.ProfitFlg = false;
                        watch.DrawDownFlg = false;
                        watch.misPricing = false;
                        watch.misSpread = false;
                        watch.RowData.Cells[WatchConst.Strategy_Type].Value = watch.Strategy_Type;
                        watch.not_got_first_tick = false;
                        if (watch.StrategyId != 91 && watch.StrategyId != 3311 || watch.StrategyId != 32211)
                        {

                            watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.Leg1.N_Price;
                        }
                        else
                        {
                            if (watch.Leg1.N_Qty != 0)
                            {
                                watch.Leg1.Net_Qty = (watch.Leg1.Sell_Qty - watch.Leg1.Buy_Qty);
                                watch.Leg1.A_Value = (watch.Leg1.S_Value - watch.Leg1.B_Value);
                                watch.RowData.Cells[WatchConst.AvgPrice].Value = Math.Round(watch.Leg1.A_Value / watch.Leg1.Net_Qty, 2);
                            }
                            else
                            {
                                watch.RowData.Cells[WatchConst.AvgPrice].Value = watch.Leg1.N_Price;
                            }
                        }
                        if (watch.StrategyId == 23434)
                        {
                            watch.posInt = ((watch.Leg1.N_Qty) / (Lotsize));
                            watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                            if (watch.posInt > 0)
                            {
                                watch.PosType = "Wind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else if (watch.posInt < 0)
                            {
                                watch.PosType = "Unwind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else
                            {
                                watch.PosType = "None";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;

                            }
                        }
                        else if (watch.StrategyId == 121 || watch.StrategyId == 1331 || watch.StrategyId == 1221)
                        {
                            watch.posInt = (watch.Leg1.N_Qty / (Lotsize)) * -1;
                            watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                            if (watch.posInt > 0)
                            {
                                watch.PosType = "Wind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else if (watch.posInt < 0)
                            {
                                watch.PosType = "Unwind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else
                            {
                                watch.PosType = "None";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                        }
                        else
                        {
                            watch.posInt = ((watch.Leg1.N_Qty) / (Lotsize));
                            watch.RowData.Cells[WatchConst.PosInt].Value = watch.posInt;
                            if (watch.posInt > 0)
                            {
                                watch.PosType = "Wind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else if (watch.posInt < 0)
                            {
                                watch.PosType = "Unwind";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                            else
                            {
                                watch.PosType = "None";
                                watch.RowData.Cells[WatchConst.PosType].Value = watch.PosType;
                            }
                        }
                        if (watch.StrategyId == 91 || watch.StrategyId == 3311)
                        {
                            watch.premium = watch.Leg1.N_Price * watch.posInt * watch.Leg1.ContDetail.LotSize * -1;
                            watch.RowData.Cells[WatchConst.Premium].Value = Math.Round(watch.premium, 2);
                        }
                        if (watch.StrategyId == 91 || watch.StrategyId == 3311)
                        {
                            if (watch.posInt < 0)
                            {
                                if (watch.Leg1.ContractInfo.Symbol == "NIFTY")
                                    watch.MarginUtilise = Math.Abs(watch.posInt) * AppGlobal.niftyMargin;
                                else if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                    watch.MarginUtilise = Math.Abs(watch.posInt) * AppGlobal.bankniftyMargin;
                                else if (watch.Leg1.ContractInfo.Symbol == "FINNIFTY")
                                    watch.MarginUtilise = Math.Abs(watch.posInt) * AppGlobal.niftyMargin;
                            }
                            else
                            {
                                watch.MarginUtilise = 0;
                            }
                        }
                        else if (watch.StrategyId == 2211 || watch.StrategyId == 3311)
                        {
                            if (watch.posInt != 0)
                            {
                                if (watch.posInt < 0)
                                {
                                    if (watch.Leg1.ContractInfo.Symbol == "NIFTY")
                                        watch.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch.posInt) * watch.Leg1.Ratio * AppGlobal.niftyMargin * 2), 2);
                                    if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                        watch.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch.posInt) * watch.Leg1.Ratio * AppGlobal.bankniftyMargin * 2), 2);
                                }
                                else if (watch.posInt > 0)
                                {
                                    watch.MarginUtilise = Math.Round(Convert.ToDouble(watch.Leg1.N_Price * Math.Abs(watch.posInt) * watch.Leg1.ContDetail.LotSize) / 2, 2);
                                }
                            }
                            else if (watch.posInt == 0)
                            {
                                watch.MarginUtilise = 0;
                            }
                        }
                        else if (watch.StrategyId == 888)
                        {
                            if (watch.posInt > 0)
                            {
                                if (watch.Leg1.ContractInfo.Symbol == "NIFTY")
                                    watch.MarginUtilise = (watch.posInt) * AppGlobal.niftyMargin * 2;
                                else if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                    watch.MarginUtilise = (watch.posInt) * AppGlobal.bankniftyMargin * 2;

                            }
                            else if (watch.posInt < 0)
                            {
                                if (watch.Leg1.ContractInfo.Symbol == "NIFTY")
                                    watch.MarginUtilise = Math.Abs(watch.posInt) * AppGlobal.niftyMargin;
                                else if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                    watch.MarginUtilise = Math.Abs(watch.posInt) * AppGlobal.bankniftyMargin;
                            }
                            else
                            {
                                watch.MarginUtilise = 0;
                            }
                        }
                        else if (watch.StrategyId == 121)
                        {
                            if (watch.posInt != 0)
                            {
                                if (watch.Leg1.ContractInfo.Symbol == "NIFTY")
                                {
                                    double extraMargin = Convert.ToDouble(watch.strikediff) / ArisApi_a._arisApi.SystemConfig.StrikeDifference;
                                    double _extraNiftyMargin = 0;
                                    extraMargin = (extraMargin - 1) * 2;
                                    _extraNiftyMargin = (ArisApi_a._arisApi.SystemConfig.NiftyButterflyExtraMargin * extraMargin) + ArisApi_a._arisApi.SystemConfig.NiftyButterflyExtraMargin;
                                    watch.MarginUtilise = (ArisApi_a._arisApi.SystemConfig.NiftyButterflyMargin + _extraNiftyMargin) * Math.Abs(watch.posInt);
                                }
                                else if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                {
                                    double extraMargin = Convert.ToDouble(watch.strikediff) / ArisApi_a._arisApi.SystemConfig.StrikeDifference;
                                    double _extraNiftyMargin = 0;

                                    extraMargin = (extraMargin - 1) * 2;
                                    _extraNiftyMargin = (ArisApi_a._arisApi.SystemConfig.BankNiftyButterflyExtraMargin * extraMargin) + ArisApi_a._arisApi.SystemConfig.BankNiftyButterflyExtraMargin;
                                    watch.MarginUtilise = (ArisApi_a._arisApi.SystemConfig.BankNiftyButterflyMargin + _extraNiftyMargin) * Math.Abs(watch.posInt);
                                }
                            }
                            else
                            {
                                watch.MarginUtilise = 0;
                            }
                        }
                        else if (watch.StrategyId == 1331 || watch.StrategyId == 1221)
                        {
                            if (watch.posInt != 0)
                            {
                                if (watch.Leg1.ContractInfo.Symbol == "NIFTY")
                                {
                                    double extraMargin = Convert.ToDouble(watch.strikediff) / ArisApi_a._arisApi.SystemConfig.StrikeDifference;
                                    double _extraNiftyMargin = 0;
                                    extraMargin = (extraMargin - 1) * 2;
                                    _extraNiftyMargin = (ArisApi_a._arisApi.SystemConfig.Nifty1331ExtraMargin * extraMargin) + ArisApi_a._arisApi.SystemConfig.Nifty1331ExtraMargin;
                                    watch.MarginUtilise = (ArisApi_a._arisApi.SystemConfig.Nifty1331Margin + _extraNiftyMargin) * Math.Abs(watch.posInt);
                                }
                                else if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                {
                                    double extraMargin = Convert.ToDouble(watch.strikediff) / ArisApi_a._arisApi.SystemConfig.StrikeDifference;
                                    double _extraNiftyMargin = 0;
                                    extraMargin = (extraMargin - 1) * 2;
                                    _extraNiftyMargin = (ArisApi_a._arisApi.SystemConfig.BankNifty1331ExtraMargin * extraMargin) + ArisApi_a._arisApi.SystemConfig.BankNifty1331ExtraMargin;
                                    watch.MarginUtilise = (ArisApi_a._arisApi.SystemConfig.BankNifty1331Margin + _extraNiftyMargin) * Math.Abs(watch.posInt);
                                }
                            }
                            else
                            {
                                watch.MarginUtilise = 0;
                            }
                        }
                        watch.RowData.Cells[WatchConst.MarginUtilise].Value = watch.MarginUtilise;
                        watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                        watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                        watch.TradedQty = watch.Leg1.ContDetail.LotSize * watch.posInt;
                        watch.RowData.Cells[WatchConst.TradedQty].Value = watch.TradedQty;
                        watch.RowData.Cells[WatchConst.PrvStrategyAvg].Value = watch.prvStraddleAvg;
                        watch.RowData.Cells[WatchConst.StrategyAvg].Value = watch.StraddlAvg;
                        watch.RowData.Cells[WatchConst.StrategyDrawDown].Value = watch.StrategyDrawDown;
                        watch.RowData.Cells[WatchConst.LSL_L1PosInt].Value = watch.L1PosInt;
                        watch.RowData.Cells[WatchConst.LSL_L2PosInt].Value = watch.L2PosInt;

                        #region Leg1
                        if (watch.Leg1.Counter == 1)
                        {
                            if (watch.Leg1 != null)
                            {
                                watch.RowData.Cells[WatchConst.L1Strike].Value = Convert.ToString(watch.Leg1.ContractInfo.StrikePrice);
                                watch.RowData.Cells[WatchConst.L1Series].Value = Convert.ToString(watch.Leg1.ContractInfo.Series);
                                watch.RowData.Cells[WatchConst.Token].Value = Convert.ToString(watch.Leg1.ContractInfo.TokenNo);
                                watch.RowData.Cells[WatchConst.Ratio1].Value = Convert.ToString(watch.Leg1.Ratio);
                                watch.RowData.Cells[WatchConst.Symbol].Value = Convert.ToString(watch.Leg1.ContractInfo.Symbol);
                                watch.RowData.Cells[WatchConst.Delta].Value = Convert.ToDouble(watch.Delta);
                                watch.RowData.Cells[WatchConst.Vega].Value = Convert.ToDouble(watch.Vega);
                                watch.RowData.Cells[WatchConst.Gamma].Value = Convert.ToDouble(watch.Gamma);
                                watch.RowData.Cells[WatchConst.Theta].Value = Convert.ToDouble(watch.Theta);
                                watch.RowData.Cells[WatchConst.L1buyPrice].Value = watch.Leg1.BuyPrice;
                                watch.RowData.Cells[WatchConst.L1sellPrice].Value = watch.Leg1.SellPrice;
                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg1.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        #endregion

                        #region Leg2
                        if (watch.Leg2.Counter == 1)
                        {
                            if (watch.Leg2 != null)
                            {
                                watch.RowData.Cells[WatchConst.L2Strike].Value = Convert.ToString(watch.Leg2.ContractInfo.StrikePrice);
                                watch.RowData.Cells[WatchConst.L2Series].Value = Convert.ToString(watch.Leg2.ContractInfo.Series);
                                watch.RowData.Cells[WatchConst.Ratio2].Value = Convert.ToString(watch.Leg2.Ratio);
                                watch.RowData.Cells[WatchConst.L2buyPrice].Value = watch.Leg2.BuyPrice;
                                watch.RowData.Cells[WatchConst.L2sellPrice].Value = watch.Leg2.SellPrice;

                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg2.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        #endregion

                        #region Leg3
                        if (watch.Leg3.Counter == 1)
                        {
                            if (watch.Leg3 != null)
                            {
                                watch.RowData.Cells[WatchConst.L3buyPrice].Value = watch.Leg3.BuyPrice;
                                watch.RowData.Cells[WatchConst.L3sellPrice].Value = watch.Leg3.SellPrice;
                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg3.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg3.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg3.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        #endregion

                        #region Leg4
                        if (watch.Leg4.Counter == 1)
                        {
                            if (watch.Leg4 != null)
                            {
                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.Leg4.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.Leg4.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.Leg4.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        #endregion

                        uint _expiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(watch.Expiry));
                        string _expiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, _expiry).ToString("yyyyMMMdd");
                        string _sf12 = Convert.ToString(_expiry1);
                        string _sf22 = _sf12.Substring(0, 4);
                        string _sf32 = _sf12.Substring(4, 3);
                        string _sf42 = _sf12.Substring(7, 2);
                        int _montf = DateTime.ParseExact(_sf32, "MMM", new CultureInfo("en-US")).Month;
                        System.Globalization.DateTimeFormatInfo _mffi1 = new System.Globalization.DateTimeFormatInfo();
                        string _monStringf = "";
                        if (_montf <= 9)
                        {
                            _monStringf = "0" + Convert.ToString(_montf);
                        }
                        else
                        {
                            _monStringf = Convert.ToString(_montf);
                        }
                        string _sf52 = _sf22 + _monStringf + _sf42;

                        #region Future Exp
                        int currentmonth = _montf;

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

                        if (watch.Leg1.ContractInfo.Symbol != "FINNIFTY")
                        {
                            string strFilter2 = "";
                            string TokenNo = "";
                            if (watch.Leg1.ContractInfo.Symbol == "NIFTY" || watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                strFilter2 = DBConst.InstrumentName + " = '" + "FUTIDX" + "' AND " + DBConst.Symbol + " = '" + symbol + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                            else
                                strFilter2 = DBConst.InstrumentName + " = '" + "FUTSTK" + "' AND " + DBConst.Symbol + " = '" + symbol + "' AND " + DBConst.ExpiryDate + " = '" + selectFut + "'";
                            DataRow[] dr1F = ArisApi_a._arisApi.DsContract.Tables["NseFo"].Select(strFilter2);
                            foreach (DataRow dr in dr1F)
                            {
                                TokenNo = dr["TokenNo"].ToString();
                            }

                            if (watch.niftyLeg != null)
                            {
                                watch.niftyLeg.ContractInfo.TokenNo = TokenNo;
                                if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                                {
                                    List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                                    list.Add(index);
                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(index);
                                    AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                                }
                            }
                        }
                        else
                        {
                            if (AppGlobal.MapList.ContainsKey(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)))
                            {
                                List<int> list = AppGlobal.MapList[Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo)];
                                list.Add(index);
                            }
                            else
                            {
                                List<int> list = new List<int>();
                                list.Add(index);
                                AppGlobal.MapList.Add(Convert.ToUInt64(watch.niftyLeg.ContractInfo.TokenNo), list);
                            }
                        }
                        DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
                        if (watch.Checked)
                        {
                            ToggleButton.Value = "ON";
                            ToggleButton.Style.ForeColor = Color.Green;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;
                        }
                        else
                        {
                            ToggleButton.Value = "OFF";
                            ToggleButton.Style.ForeColor = Color.Red;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;
                        }
                        ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvMarketWatch.Rows[index].Cells[WatchConst.Checked] = ToggleButton;

                        watch.RowData.Cells[WatchConst.TGBuyPrice].Value = watch.TGBuyPrice;
                        watch.RowData.Cells[WatchConst.TGSellPrice].Value = watch.TGBuyPrice;

                        watch.RowData.Cells[WatchConst.AP_BuySL].Value = watch.AP_BuySL;
                        watch.RowData.Cells[WatchConst.AP_SellSL].Value = watch.AP_SellSL;

                        watch.RowData.Cells[WatchConst.SL_BuyQty].Value = watch.SL_BuyQty;
                        watch.RowData.Cells[WatchConst.SL_SellQty].Value = watch.SL_SellQty;

                        watch.RowData.Cells[WatchConst.DD_bm_Buy].Value = watch.DD_bm_Buy;
                        watch.RowData.Cells[WatchConst.DD_bm_Sell].Value = watch.DD_bm_Sell;

                        watch.RowData.Cells[WatchConst.DD_BuyQty].Value = watch.DD_BuyQty;
                        watch.RowData.Cells[WatchConst.DD_SellQty].Value = watch.DD_SellQty;

                        watch.RowData.Cells[WatchConst.DD_MxSell].Value = watch.DD_SellMinPrice;
                        watch.RowData.Cells[WatchConst.DD_TGSellPrice].Value = watch.DD_TGSellPrice;

                        watch.Bidding_start = 0;
                        if (watch.StrategyId == 1113)
                        {
 
                        }


                        TransactionWatch.ErrorMessage("AssignStruct|uniqueid|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|avg|" + watch.Leg1.N_Price + "|pos|" + watch.posInt + "|sqpnl|" + watch.Sqpnl);

                        if (watch.StrategyId == 91)
                        {
                            if (!watch.StrategyName.Contains("TLI"))
                            {
                                if (watch.IsStrikeReq)
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                else
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.Aqua;
                            }
                            else
                            {
                                if (watch.IsStrikeReq)
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                else
                                    AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.LightYellow;
                            }
                        }
                        else
                        {
                            if (watch.DD_SellOrderflg == true)
                            {
                                AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.ForestGreen;
                            }
                            else
                            {
                                if (watch.StrategyId == 12211 || watch.StrategyId == 32211 || watch.StrategyId == 1113 || watch.TLI_StrategyId == 1114)
                                {
                                    if (watch.Leg2.ContractInfo.TokenNo == "0")
                                    {
                                        if (watch.IsStrikeReq)
                                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                        else
                                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.LightYellow;
                                    }
                                    else
                                    {
                                        if (watch.IsStrikeReq)
                                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                        else
                                            AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.Aqua;

                                    }
                                }
                                else
                                {
                                    if (watch.IsStrikeReq)
                                        AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.White;
                                    else
                                        AppGlobal.frmWatch.dgvMarketWatch.Rows[index].DefaultCellStyle.BackColor = Color.Aqua;
                                }
                            }
                        }
                        dgvMarketWatch.Rows.Add();
                    }
                }
                Sum();
                MatchUniqueNo();
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "AssignMarketStructValue")
                             , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        #endregion

        delegate void dldgvMarketWatchDeleteRow(int index);

        public void dgvMarketWatchDeleteRow(int index)
        {
            if (InvokeRequired)
            {
                dldgvMarketWatchDeleteRow dl = dgvMarketWatchDeleteRow;
                Invoke(dl, new object[] { index });
            }
            else
            {
                AppGlobal.MarketWatch.RemoveAt(index);
                dgvMarketWatch.Rows.RemoveAt(index);
            }
        }

        #region DGV Market Watch

        public void dgvMarketWatch_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

            try
            {
                if (e.Value == null || AppGlobal.MarketWatch == null)
                    return;

                if (e.ColumnIndex < 0 || e.RowIndex < 0
                    || e.ColumnIndex >= dgvMarketWatch.Columns.Count || e.RowIndex >= dgvMarketWatch.Rows.Count)
                    return;
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "dgvMarketWatch_CellFormatting")
                                 , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        void dgvMarketWatch_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    DataGridView.HitTestInfo hti = dgvMarketWatch.HitTest(e.X, e.Y);

                    if (hti.RowIndex == dgvMarketWatch.Rows.Count - 1)
                    {
                        tlsSeparator.Visible = false;
                        tlsmiActiveDeActive.Visible = false;
                    }
                    else if (hti.ColumnIndex >= 0 && hti.RowIndex >= 0)
                    {
                        dgvMarketWatch.CurrentCell = dgvMarketWatch[hti.ColumnIndex, hti.RowIndex];
                        tlsmiActiveDeActive.Visible = true;
                        tlsSeparator.Visible = true;
                        tlsmiActiveDeActive.Checked = AppGlobal.MarketWatch[hti.RowIndex].IsActive;
                    }
                }
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "dgvMarketWatch_MouseClick")
                                , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        #endregion

        #region general functions

        private void GenerateColumn(string clName, MTEnums.FieldType fieldType, bool Editable)
        {
            dgvMarketWatch.Columns.Add(clName, clName);
            dgvMarketWatch.Columns[clName].ReadOnly = Editable;


            switch (fieldType)
            {
                case MTEnums.FieldType.None:
                    break;
                case MTEnums.FieldType.Date:
                    dgvMarketWatch.Columns[clName].DefaultCellStyle.Format = MTConstant.DateFormatGrid;
                    break;
                case MTEnums.FieldType.Time:
                    dgvMarketWatch.Columns[clName].DefaultCellStyle.Format = MTConstant.TimeFormatGrid;
                    break;
                case MTEnums.FieldType.Price:
                    dgvMarketWatch.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Quantity:
                    dgvMarketWatch.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Percentage:
                    dgvMarketWatch.Columns[clName].DefaultCellStyle.Format = "0.00%";
                    dgvMarketWatch.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Indicator:
                    dgvMarketWatch.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    break;
                case MTEnums.FieldType.DateTime:
                    break;
            }
        }

        private void GenerateColumns()
        {
            try
            {
                GenerateColumn(WatchConst.Checked, MTEnums.FieldType.None, false);
                GenerateColumn(WatchConst.Strategy, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.Unique, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Token, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.Token2, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.Expiry, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.Expiry2, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.StrategyName, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.StrategyId, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.L1Strike, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.L1Series, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.L2Strike, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.L2Series, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.L3Strike, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.L3Series, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.L4Strike, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.L4Series, MTEnums.FieldType.None, true);

                GenerateColumn(WatchConst.FLTP, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.StrikeDiff, MTEnums.FieldType.Quantity, true);
                GenerateColumn(WatchConst.FSpread, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Wind, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.UnWind, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.FQty, MTEnums.FieldType.Quantity, false);
                GenerateColumn(WatchConst.RQty, MTEnums.FieldType.Quantity, false);
                GenerateColumn(WatchConst.RSpread, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.AvgPrice, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.PosInt, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.PosType, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.TrnCost, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.PNL, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.SqPnl, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Premium, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.LivePremium, MTEnums.FieldType.Price, true);

                //// for Testing
                GenerateColumn(WatchConst.L1buyPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.L1sellPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.L2buyPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.L2sellPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.L3buyPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.L3sellPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.L4buyPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.L4sellPrice, MTEnums.FieldType.Price, false);

                GenerateColumn(WatchConst.BidIv, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.SellIv, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.BidIv2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.SellIv2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.BidIv3, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.SellIv3, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DerivePrice1, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DerivePrice2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DeriveL1Diff, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DeriveL2Diff, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Delta, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Vega, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Theta, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Gamma, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Delta2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Vega2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Theta2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Gamma2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Delta3, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Vega3, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Theta3, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Gamma3, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DeltaV, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.VegaV, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.ThetaV, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.GammaV, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Rule, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Ratio1, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Ratio2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Symbol, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.MarginUtilise, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.WindDelta, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.UnwindDelta, MTEnums.FieldType.Price, true);

                GenerateColumn(WatchConst.CarryForwardPnl, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.TradedQty, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.StrategyPnl, MTEnums.FieldType.Price, true);

                /////////
                GenerateColumn(WatchConst.SL_BuyOrder, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.TGBuyPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.AP_BuySL, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.SL_BuyQty, MTEnums.FieldType.Price, false);

                /////////
                GenerateColumn(WatchConst.SL_SellOrder, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.TGSellPrice, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.AP_SellSL, MTEnums.FieldType.Price, false);
                GenerateColumn(WatchConst.SL_SellQty, MTEnums.FieldType.Price, false);

                /////                   
                GenerateColumn(WatchConst.DD_bm_Buy, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DD_BuyQty, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DD_TGBuyPrice, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DD_MinBuy, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DD_MxSell, MTEnums.FieldType.Price, true);

                /////
                GenerateColumn(WatchConst.DD_bm_Sell, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DD_SellQty, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.DD_TGSellPrice, MTEnums.FieldType.Price, true);

                ////
                GenerateColumn(WatchConst.Strategy_Type, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.Straddle_MktWind, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Straddle_MktUnwind, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Track, MTEnums.FieldType.None, true);
                GenerateColumn(WatchConst.StrategyDrawDown, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.StrategyAvg, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.PrvStrategyAvg, MTEnums.FieldType.Price, true);

                /// <summary>
                /// Avg Iv,Vega,Delta,Gamma
                /// </summary>
                GenerateColumn(WatchConst.Avg_IV, MTEnums.FieldType.Price, true);               
                GenerateColumn(WatchConst.Avg_Theta, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Avg_ThetaV, MTEnums.FieldType.Price, true);

                GenerateColumn(WatchConst.UniqueIdL1, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.UniqueIdL2, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.TLI_Uniqueid, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.FutPrice, MTEnums.FieldType.Price, true);

                GenerateColumn(WatchConst.StrategyAvgPrice, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.LSL_StrategyPercent, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.LSL_StrategyValue, MTEnums.FieldType.Price, true);

                GenerateColumn(WatchConst.LSL_L1PosInt, MTEnums.FieldType.Quantity, true);
                GenerateColumn(WatchConst.LSL_L2PosInt, MTEnums.FieldType.Quantity, true);
                GenerateColumn(WatchConst.LSL_StrategyLive, MTEnums.FieldType.Quantity, true);

                GenerateColumn(WatchConst.LSL_AvgPriceCE, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.LSL_AvgPricePE, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.LSL_StrategyAvg, MTEnums.FieldType.Price, true);

                GenerateColumn(WatchConst.trail_bm, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.trail_TGPrice, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.trail_Mx, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.SQ_Time, MTEnums.FieldType.None, true);

                GenerateColumn(WatchConst.SQ_TVega, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.SQ_TPremium, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.SQ_TLoss, MTEnums.FieldType.Price, true);

                GenerateColumn(WatchConst.Intensic, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.ATP, MTEnums.FieldType.Price, true);



                ///// 
                GenerateColumn(WatchConst.Premium_dm, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Init_Premium, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.TG_Premium, MTEnums.FieldType.Price, true);

                /////
                GenerateColumn(WatchConst.Init_TrailingPx, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Init_TrailingMx, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Init_TrailingPt, MTEnums.FieldType.Price, true);
                GenerateColumn(WatchConst.Init_TrailingTg, MTEnums.FieldType.Price, true);

                ///
                GenerateColumn(WatchConst.LevelIterator, MTEnums.FieldType.Price, true);


            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "Column Creation...")
                              , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
                StackTrace st = new StackTrace(ex, true);
            }
        }

        void ActiveDeactiveScript(int rowindex, bool auto)
        {
            #region Active

            if (!AppGlobal.MarketWatch[rowindex].IsActive)
            {
                if (!AppGlobal.isStart)
                {
                    //AppGlobal.Logger.ShowToMessageLog("Please Start Application First.");
                    tlsmiActiveDeActive.Checked = false;
                    return;
                }
                AppGlobal.MarketWatch[rowindex].IsActive = true;
                AppGlobal.ActiveScript++;
                AppGlobal.DeActiveScript--;
                if (AppGlobal.MarketWatch[rowindex].IsActive)
                {
                    dgvMarketWatch.Rows[rowindex].DefaultCellStyle.SelectionForeColor = dgvMarketWatch.Rows[rowindex].DefaultCellStyle.ForeColor = Preference.Instance.ActiveForeColor;
                    dgvMarketWatch.Rows[rowindex].DefaultCellStyle.SelectionBackColor = dgvMarketWatch.Rows[rowindex].DefaultCellStyle.BackColor = Preference.Instance.ActiveBackColor;
                }
            }
            #endregion

            #region DeActive
            else
            {
                if (!auto)
                {
                    DialogResult dr = MessageBox.Show(this,
                                                      "Are you sure want to deactive this script?\n" + "Exchange- " + AppGlobal.MarketWatch[rowindex].Leg1.ContractInfo.Exchange + ",Symbol- " + AppGlobal.MarketWatch[rowindex].Leg1.ContractInfo.Symbol +
                                                      ",Expiry- " + MTMethods.SecondsToDateTime(AppGlobal.MarketWatch[rowindex].Leg1.ContractInfo.ExpiryDate).ToString(MTConstant.DateFormatGrid), Application.ProductName, MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question);
                    if (dr == DialogResult.No)
                        return;
                }
                OrderFunction.CancelOrderOnDeActive(rowindex);
                //AppGlobal.Logger.ShowToMessageLog(AppGlobal.MarketWatch[rowindex].Leg1.GatewayId + " Script: " + AppGlobal.MarketWatch[rowindex].Leg1.ContractInfo.Symbol + " " +
                //                                   MTMethods.SecondsToGridString(AppGlobal.MarketWatch[rowindex].Leg1.ContractInfo.ExpiryDate)
                //                                   + " has been deactivate at position " + rowindex);

                AppGlobal.MarketWatch[rowindex].IsActive = false;
                dgvMarketWatch.Rows[rowindex].DefaultCellStyle.SelectionForeColor = dgvMarketWatch.DefaultCellStyle.SelectionForeColor;
                dgvMarketWatch.Rows[rowindex].DefaultCellStyle.ForeColor = dgvMarketWatch.DefaultCellStyle.ForeColor;
                dgvMarketWatch.Rows[rowindex].DefaultCellStyle.BackColor = dgvMarketWatch.DefaultCellStyle.BackColor;
                dgvMarketWatch.Rows[rowindex].DefaultCellStyle.SelectionBackColor = dgvMarketWatch.DefaultCellStyle.SelectionBackColor;
                AppGlobal.ActiveScript--;
                AppGlobal.DeActiveScript++;
            }
            #endregion
        }

        #endregion

        public string getFormatedSymbol(string symb, string exp, string strike, string calput)
        {
            string fsymb = "";

            string month = "", expiry = "";
            month = exp.Substring(0, 2);
            expiry = exp.Substring(3, 2);

            if (expiry.Contains('/'))
            {
                expiry = exp.Substring(2, 1);
            }
            switch (month)
            {
                case "01":
                    month = "Jan";
                    break;
                case "02":
                    month = "Feb";
                    break;
                case "03":
                    month = "Mar";
                    break;
                case "04":
                    month = "Apr";
                    break;
                case "05":
                    month = "May";
                    break;
                case "06":
                    month = "Jun";
                    break;
                case "07":
                    month = "Jul";
                    break;
                case "08":
                    month = "Aug";
                    break;
                case "09":
                    month = "Sep";
                    break;
                case "10":
                    month = "Oct";
                    break;
                case "11":
                    month = "Nov";
                    break;
                case "12":
                    month = "Dec";
                    break;
            }
            if (calput == "CE" || calput == "PE")
            {
                fsymb = symb + expiry + month.ToUpper() + strike + calput;
            }
            if (calput == "XX")
            {
                fsymb = symb + expiry + month.ToUpper() + "FUT";
            }
            return fsymb;
        }

        public void SqPnl()
        {
            string path = Application.StartupPath + "\\" + "Logs" + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string date = DateTime.Now.ToString("ddMMMyyyy") + ".csv";
            string fileName = path + "SQPnl" + ".csv";
            StreamWriter sw = new StreamWriter(fileName);
            string strHead = "";
            strHead = "Name,PNL,LivePnl";
            sw.WriteLine(strHead);
            string SqrtPnl = ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "," + Math.Round(AppGlobal.OverAllPnl, 2).ToString();
            sw.WriteLine(SqrtPnl);
            sw.Close();

        }


        public void Live_SQpnl()
        {
            string path = Application.StartupPath + "\\" + "Logs" + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string date = DateTime.Now.ToString("ddMMMyyyy") + ".csv";
            string fileName = path + "SQPnl" + ".csv";
            StreamWriter sw = new StreamWriter(fileName);

            string strHead = "";
            strHead = "Name,PNL,LivePnl";
            sw.WriteLine(strHead);
            string SqrtPnl = ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "," + Math.Round(AppGlobal.OverAllPnl, 2).ToString();
            sw.WriteLine(SqrtPnl);
            sw.Close();

            string fileName1 = path + "Count.csv";
            StreamWriter sw1 = new StreamWriter(fileName1);
            string strHead1 = "";
            strHead1 = "Single,Ratio,Strangle,Ladder";
            sw1.WriteLine(strHead1);
            string position = AppGlobal.Count_single.ToString() + "," + AppGlobal.Count_Ratio.ToString() + "," + AppGlobal.Count_Strangle.ToString() + "," + AppGlobal.Count_Ladder.ToString();
            sw1.WriteLine(position);
            sw1.Close();


            string Pnl_Margin = path + ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "_Pnl_Margin" + ".csv";
            StreamWriter sw2 = new StreamWriter(Pnl_Margin);
            string strHead2 = "";
            strHead2 = "PNL,Margin,PremiumMargin";
            sw2.WriteLine(strHead2);
            string PnlMargin = Math.Round(AppGlobal.OverAllPnl, 2).ToString() + "," + AppGlobal.OverallMarginUtilize.ToString() + "," + AppGlobal.overallPremium.ToString();
            sw2.WriteLine(PnlMargin);
            sw2.Close();
        }

        private void FrmWatch_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                TransactionWatch.ErrorMessage("Straddle Application Closing Start");
               
                RunningPnl.Stop();
                if (ArisApi_a._arisApi.SystemConfig.RmsConnect == false)
                {
                    MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                    Live_SQpnl();
                    SendBackFile();
                    SendBackFile2();
                }
                //TransactionWatch.ErrorMessage("Straddle 1");
                if (AppGlobal.GUI_ID != 0)
                {
                    //TransactionWatch.ErrorMessage("Straddle 2");
                    if (!AppGlobal.isStart)
                    {
                       // TransactionWatch.ErrorMessage("Straddle 3");
                        if (AppGlobal.MarketWatch != null)
                        {
                           // TransactionWatch.ErrorMessage("Straddle 4");
                            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                            Live_SQpnl();
                        }
                        //TransactionWatch.ErrorMessage("Straddle 5");

                        SendBackFile();
                        //TransactionWatch.ErrorMessage("Straddle 6");
                        SendBackFile2();
                       // TransactionWatch.ErrorMessage("Straddle 7");
                    }
                    else
                    {
                       // TransactionWatch.ErrorMessage("Straddle 8");
                        e.Cancel = true;
                    }
                    if (AppGlobal.MarketWatch == null) return;
                    //TransactionWatch.ErrorMessage("Straddle 9");
                    for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
                    {
                        MarketWatch watch = AppGlobal.MarketWatch[index];
                        watch.RowData = dgvMarketWatch.Rows[index];
                        watch.Wind = Convert.ToDecimal(watch.RowData.Cells[WatchConst.Wind].Value);
                        watch.unWind = Convert.ToDecimal(watch.RowData.Cells[WatchConst.UnWind].Value);
                        watch.Over = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                        watch.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                        // watch.BiduserIV = Convert.ToDouble(watch.RowData.Cells[WatchConst.BidUserIv].Value);
                        // watch.AskuserIV = Convert.ToDouble(watch.RowData.Cells[WatchConst.AskUserIv].Value);

                    }
                    //TransactionWatch.ErrorMessage("Straddle 10");
                }
                for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
                {
                    MarketWatch watch = AppGlobal.MarketWatch[index];
                    if (watch.thread != null)
                    {
                        if (watch.thread.IsAlive)
                            watch.thread.Suspend();
                    }
                    if (watch.thread1 != null)
                    {
                        if (watch.thread1.IsAlive)
                            watch.thread1.Suspend();
                    }
                    if (watch.thread2 != null)
                    {
                        if (watch.thread2.IsAlive)
                            watch.thread2.Suspend();
                    }
                }
                TransactionWatch.ErrorMessage("Straddle Application Closing End");
                Environment.Exit(0);
             
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "frmMarketWatch_FormClosing")
                                , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        /////////////////////// Capture Grid /////////////////////////////////////
        private void FrmWatch_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (AppGlobal.closingflg)
                {
                    if (AppGlobal.GUI_ID != 0)
                    {
                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                        snd.TransCode = 17;
                        snd.gui_id = Convert.ToUInt64(AppGlobal.GUI_ID);
                        snd.WindPos = ArisApi_a._arisApi.SystemConfig.Uniqueid;
                        byte[] bytesToSend = StructureToByte(snd);
                        foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
                        {
                            connectedClient.Send(bytesToSend);
                        }
                        AppGlobal.closingflg = false;
                        TransactionWatch.ErrorMessage("GUI|" + AppGlobal.GUI_ID + "|Closed|");
                    }
                    else
                    {
                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                        snd.TransCode = 97;
                        snd.gui_id = Convert.ToUInt64(AppGlobal.GUI_ID);
                        byte[] bytesToSend = StructureToByte(snd);
                        foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
                        {
                            connectedClient.Send(bytesToSend);
                        }
                        TransactionWatch.ErrorMessage("GUI|" + AppGlobal.GUI_ID + " " + "|Closed| Duplicate");
                        AppGlobal.closingflg = false;
                    }
                }
                if (AppGlobal.frmWatch != null)
                {
                    AppGlobal.frmWatch = null;
                }
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "frmMarketWatch_FormClosed")
                              , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        public void TokenRequest83434(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int Token3, int Token4, int FutToken, UInt64 strategyid)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.WindPos = Token3;
            newStrike.UnWindPos = Token4;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest8343(UInt64 uniqueID, UInt64 guiID, int Token1, int Token3, int Token4, int FutToken, UInt64 strategyid)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token3;
            newStrike.WindPos = Token4;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest834(UInt64 uniqueID, UInt64 guiID, int Token1, int Token3, int FutToken, UInt64 strategyid)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token3;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest11_12(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int FutToken, UInt64 strategyid)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest311(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int FutToken, UInt64 strategyid, int ratio1, int ratio2)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.WindPos = ratio1;
            newStrike.UnWindPos = ratio2;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest2211(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int FutToken, UInt64 strategyid, int ratio1, int ratio2)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest12211(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int FutToken, UInt64 strategyid, int ratio1, int ratio2, int Uniqueid_Leg1, int Uniqueid_Leg2)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.Token = FutToken;
            newStrike.OverNightWindPos = Uniqueid_Leg1;
            newStrike.OverNightUnWindPos = Uniqueid_Leg2;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }

        }

        public void TokenRequest888(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int Token3, int FutToken, UInt64 strategyid, int ratio1, int ratio2)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.WindPos = Token3;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest7121(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int Token3, int FutToken, UInt64 strategyid, int ratio1, int ratio2, bool series)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.WindPos = Token3;
            newStrike.Token = FutToken;
            newStrike.NonIOCStrike = 0;
            if (series)
                newStrike.isWind = true;
            else
                newStrike.isWind = false;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest91(UInt64 uniqueID, UInt64 guiID, int Token1, int FutToken, UInt64 strategyid)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest912211(UInt64 uniqueID, UInt64 guiID, int Token1, int FutToken, UInt64 strategyid)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest1331(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int Token3, int Token4, int FutToken, UInt64 strategyid, bool Series)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.WindPos = Token3;
            newStrike.UnWindPos = Token4;
            newStrike.Token = FutToken;
            newStrike.NonIOCStrike = 0;
            if (Series)
                newStrike.isWind = true;
            else
                newStrike.isWind = false;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequest1221(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int Token3, int Token4, int FutToken, UInt64 strategyid)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.WindPos = Token3;
            newStrike.UnWindPos = Token4;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequestCalender(UInt64 uniqueID, UInt64 guiID, int Token1, int Token2, int FutToken, UInt64 strategyid, int ratio1, int ratio2, int Uniqueid_Leg1, int Uniqueid_Leg2)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Round = Token2;
            newStrike.Token = FutToken;
            newStrike.OverNightWindPos = Uniqueid_Leg1;
            newStrike.OverNightUnWindPos = Uniqueid_Leg2;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public void TokenRequestCalender91(UInt64 uniqueID, UInt64 guiID, int Token1, int FutToken, UInt64 strategyid)
        {
            BTPacket.GUIUpdate newStrike = new BTPacket.GUIUpdate();
            newStrike.TransCode = 9;
            newStrike.StrategyId = strategyid;
            newStrike.UniqueID = uniqueID;
            newStrike.gui_id = AppGlobal.GUI_ID;
            newStrike.Open = Token1;
            newStrike.Token = FutToken;
            byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(newStrike);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        public byte[] StructureToByte(object packet)
        {
            try
            {
                int length = Marshal.SizeOf(packet);
                byte[] data = new byte[length];
                IntPtr intPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(packet, intPtr, true);
                Marshal.Copy(intPtr, data, 0, length);
                Marshal.FreeHGlobal(intPtr);
                return data;
            }
            catch (Exception)
            {

            }
            return null;
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
            catch (Exception)
            {

            }
            return (T)packet;
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.StrategyId == 0)
                return;
            if (watch.IsStrikeReq == false)
            {
                MessageBox.Show("Please Strike Request first...");
                return;
            }

            if (AppGlobal.EnterLots < watch.Over)
            {
                MessageBox.Show("Enter Wind Qty is more than Max Qty Limit | Max Qty Limit is " + Convert.ToString(AppGlobal.EnterLots));
                return;
            }
            if (AppGlobal.EnterLots < watch.Round)
            {
                MessageBox.Show("Enter Unwind Qty is more than Max Qty Limit | Max Qty Limit is " + Convert.ToString(AppGlobal.EnterLots));
                return;
            }
            BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
            snd.TransCode = 2;
            UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
            snd.UniqueID = unique;
            snd.Wind = Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
            snd.Unwind = Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
            snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
            snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
            snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
            snd.Token = Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo);
            snd.gui_id = watch.Gui_id;
            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
            byte[] bytesToSend = StructureToByte(snd);
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }

        private void ResetAll_Click(object sender, EventArgs e)
        {
            if (AppGlobal.MarketWatch == null)
                return;
            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                MarketWatch watch = AppGlobal.MarketWatch[i];
                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                snd.TransCode = 2;
                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                snd.UniqueID = unique;
                snd.Wind = Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
                snd.Unwind = Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
                snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                snd.StrategyId = Convert.ToUInt64(Convert.ToInt64(watch.StrategyId));
                snd.gui_id = watch.Gui_id;
                snd.Token = Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo);
                byte[] bytesToSend = StructureToByte(snd);
                foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
                {
                    connectedClient.Send(bytesToSend);
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void dgvMarketWatch_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            int cellIndex = dgvMarketWatch.CurrentCell.ColumnIndex;
            int rowIndex = dgvMarketWatch.CurrentCell.RowIndex;
            e.Control.KeyPress -= new KeyPressEventHandler(tb_KeyPress);
            e.Control.KeyPress -= new KeyPressEventHandler(tb1_KeyPress);
            if (dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.Wind].Index
                || dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.UnWind].Index
                || dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.TGBuyPrice].Index
                || dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.TGSellPrice].Index
                || dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.AP_BuySL].Index
                || dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.AP_SellSL].Index)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress += new KeyPressEventHandler(tb_KeyPress);
                    AppGlobal.GotKeyDownFromEditing = true;
                    tb.PreviewKeyDown -= new PreviewKeyDownEventHandler(dgvMarketWatch_PreviewKeyDown);
                    tb.PreviewKeyDown += new PreviewKeyDownEventHandler(dgvMarketWatch_PreviewKeyDown);
                }
            }
            if (dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.FQty].Index
                || dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.RQty].Index
                || dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.SL_BuyQty].Index
                || dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.SL_SellQty].Index)
            {
                TextBox tb1 = e.Control as TextBox;
                if (tb1 != null)
                {
                    tb1.KeyPress += new KeyPressEventHandler(tb1_KeyPress);
                    AppGlobal.GotKeyDownFromEditing = true;
                    tb1.PreviewKeyDown -= new PreviewKeyDownEventHandler(dgvMarketWatch_PreviewKeyDown);
                    tb1.PreviewKeyDown += new PreviewKeyDownEventHandler(dgvMarketWatch_PreviewKeyDown);
                }
            }
            if (dgvMarketWatch.CurrentCell.ColumnIndex == dgvMarketWatch.Columns[WatchConst.StrategyName].Index)
            {
                TextBox tb1 = e.Control as TextBox;
                if (tb1 != null)
                {
                    tb1.KeyPress += new KeyPressEventHandler(tb2_KeyPress);
                    AppGlobal.GotKeyDownFromEditing = true;
                    tb1.PreviewKeyDown -= new PreviewKeyDownEventHandler(dgvMarketWatch_PreviewKeyDown);
                    tb1.PreviewKeyDown += new PreviewKeyDownEventHandler(dgvMarketWatch_PreviewKeyDown);
                }
            }
        }

        void tb1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) || (e.KeyChar == '.' &&  e.KeyChar == '-'))
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        void tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != '.' && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        void Time_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && e.KeyChar != ':' && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        void tb2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '.' && char.IsDigit(e.KeyChar) && e.KeyChar != '-' && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            if (e.KeyChar == '.'
            && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void ManualTrade_Click(object sender, EventArgs e)
        {
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.StrategyId == 0)
                return;
            AppGlobal.ManualCount = 2;
            if (AppGlobal._manualTrade == null)
            {
                AppGlobal._manualTrade = new ManualTradeEntry();
                AppGlobal._manualTrade.Show();
            }
            else
            {
                AppGlobal._manualTrade.Show();
                AppGlobal._manualTrade.Activate();
            }
        }

        private void SquareOff_CheckedChanged(object sender, EventArgs e)
        {
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (SquareOff.Checked == true)
            {
                watch.SeqaureOff = 2;
            }
            else
            {
                watch.SeqaureOff = 1;
            }
        }

        private void dgvMarketWatch_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    #region Delete
                    if (!AppGlobal.isStart)
                    {
                        int iIndex = dgvMarketWatch.CurrentCell.RowIndex;
                        if (dgvMarketWatch.Rows.Count > 1 && iIndex < dgvMarketWatch.Rows.Count - 1)
                        {

                            if (MessageBox.Show(this, "Are you sure you Want to delete selected script?",
                                                                                      Application.ProductName, MessageBoxButtons.YesNo,
                                                                                      MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                MarketWatch watch2 = new MarketWatch();
                                watch2 = AppGlobal.MarketWatch[iIndex];
                                if (watch2.StrategyId == 0 && watch2.Strategy != "")
                                {
                                    bool isPosition = false;
                                    MarketWatch watch = new MarketWatch();
                                    for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
                                    {
                                        if (isPosition)
                                            continue;
                                        watch = AppGlobal.MarketWatch[i];
                                        if (watch2.Strategy == watch.Strategy)
                                        {
                                            if (watch.posInt != 0)
                                                isPosition = true;
                                        }
                                    }
                                    if (isPosition)
                                    {
                                        TransactionWatch.ErrorMessage("RuleNotDelete|Uniqueid|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|pos|" + watch.posInt);
                                        MessageBox.Show("Position is Not Zero! Can't delete this rule.");
                                        return;
                                    }
                                    else
                                    {
                                        string Strategy = watch2.Strategy;
                                        MarketWatch watch1 = new MarketWatch();
                                        for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
                                        {
                                            watch1 = AppGlobal.MarketWatch[i];
                                            if (Strategy == watch1.Strategy)
                                            {
                                                if (watch1.StrategyId != 0)
                                                {
                                                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                                    snd.TransCode = 18;
                                                    snd.gui_id = AppGlobal.GUI_ID;
                                                    snd.UniqueID = watch1.uniqueId;
                                                    snd.StrategyId = Convert.ToUInt64(watch1.StrategyId);
                                                    long seq = ClassDisruptor.ringBufferRequest.Next();
                                                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                                    ClassDisruptor.ringBufferRequest.Publish(seq);

                                                    TransactionWatch.ErrorMessage("RuleDelete|Uniqueid|" + watch.uniqueId + "|Strategy|" + watch.StrategyId);

                                                }
                                                AppGlobal.MarketWatch.RemoveAt(i);
                                                dgvMarketWatch.Rows.RemoveAt(i);
                                                i--;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (watch2.Leg1.N_Qty != 0)
                                    {
                                        TransactionWatch.ErrorMessage("RuleNotDelete|Uniqueid|" + watch2.uniqueId + "|Strategy|" + watch2.StrategyId + "|pos|" + watch2.Leg1.N_Qty);
                                        MessageBox.Show("Position is Not Zero! Can't delete this rule.");
                                        return;
                                    }

                                    if (watch2.StrategyId == 12211)
                                    {
                                        if (watch2.L1PosInt != 0 || watch2.L2PosInt != 0)
                                        {
                                            TransactionWatch.ErrorMessage("RuleNotDelete|Uniqueid|" + watch2.uniqueId + "|Strategy|" + watch2.StrategyId + "|L1pos|" + watch2.L1PosInt + "|L2pos|" + watch2.L2PosInt);
                                            MessageBox.Show("Position is Not Zero! Can't delete this rule.");
                                            return;
                                        }
                                    }

                                    if (watch2.StrategyId != 0)
                                    {
                                        foreach (var _watch in AppGlobal.MarketWatch.Where(x => (Convert.ToInt32(x.StrategyId) == 0) && (Convert.ToString(x.Strategy) == watch2.Strategy)))
                                        {
                                            _watch.CarryForwardPnl = _watch.CarryForwardPnl + watch2.Sqpnl;
                                            _watch.RowData.Cells[WatchConst.CarryForwardPnl].Value = Math.Round(_watch.CarryForwardPnl, 2);
                                        }

                                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                        snd.TransCode = 18;
                                        snd.gui_id = AppGlobal.GUI_ID;
                                        snd.UniqueID = watch2.uniqueId;
                                        snd.StrategyId = Convert.ToUInt64(watch2.StrategyId);
                                        long seq = ClassDisruptor.ringBufferRequest.Next();
                                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                        ClassDisruptor.ringBufferRequest.Publish(seq);
                                        TransactionWatch.ErrorMessage("RuleDelete|Uniqueid|" + watch2.uniqueId + "|Strategy|" + watch2.StrategyId);
                                    }
                                    DataGridViewSelectedRowCollection dr = dgvMarketWatch.SelectedRows;
                                    DataGridViewSelectedCellCollection dr1 = dgvMarketWatch.SelectedCells;
                                    for (int i = 0; i < dr1.Count; i++)
                                    {
                                        int idx = dr1[i].RowIndex;
                                        AppGlobal.MarketWatch.RemoveAt(idx);
                                        dgvMarketWatch.Rows.RemoveAt(idx);
                                    }
                                }
                                for (int i = 0; i < dgvMarketWatch.Rows.Count - 1; i++)
                                {
                                    string rule = Convert.ToString(dgvMarketWatch.Rows[i].Cells[WatchConst.Rule].Value);
                                    foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Ruleno) == rule)))
                                    {
                                        int k = AppGlobal.MarketWatch.IndexOf(watch);
                                        AppGlobal.MarketWatch.RemoveAt(k);
                                        AppGlobal.MarketWatch.Insert(i, watch);
                                        break;
                                    }
                                }
                                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                                MatchUniqueNo();
                            }
                            else
                            { }
                        }
                        else
                        { }
                    }
                    #endregion
                }
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    int iColumn = dgvMarketWatch.CurrentCell.ColumnIndex;
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();
                    watch = AppGlobal.MarketWatch[iRow];
                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    if (AppGlobal.GotKeyDownFromEditing == true)
                    {
                        AppGlobal.GotKeyDownFromEditing = false;
                        AppGlobal.GotEnterFromEditing = false;
                        AppGlobal.GotTabFromEditing = false;
                        TransactionWatch.ErrorMessage("Editting Without Pressing Enter!" + "|" + Convert.ToString(watch.uniqueId));
                        MessageBox.Show("Editting Without Pressing Enter!" + " | " + Convert.ToString(watch.uniqueId));
                    }
                }
                else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                {
                    if (AppGlobal.GotKeyDownFromEditing == true)
                    {
                        AppGlobal.GotKeyDownFromEditing = false;
                        AppGlobal.GotEnterFromEditing = false;
                        AppGlobal.GotTabFromEditing = false;
                    }
                }
                else if (e.KeyCode == Keys.Tab)
                {
                    if (AppGlobal.GotKeyDownFromEditing == true)
                    {
                        AppGlobal.GotKeyDownFromEditing = false;
                        AppGlobal.GotEnterFromEditing = false;
                        AppGlobal.GotTabFromEditing = false;
                    }
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    int iColumn = dgvMarketWatch.CurrentCell.ColumnIndex;
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();
                    watch = AppGlobal.MarketWatch[iRow];
                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    if (AppGlobal.GotKeyDownFromEditing == true)
                        AppGlobal.GotEnterFromEditing = true;
                    if (watch.StrategyId == 0)
                    {
                        watch.StrategyName = Convert.ToString(watch.RowData.Cells[WatchConst.StrategyName].Value);
                    }
                    else
                    {
                        watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                    }

                    #region Enter
                    if (AppGlobal.GotEnterFromEditing == false)
                    {
                        if (watch.StrategyId == 0)
                            return;
                        if (watch.IsStrikeReq == false)
                        {
                            AppGlobal.GotTabFromEditing = false;
                            AppGlobal.GotKeyDownFromEditing = false;
                            AppGlobal.GotEnterFromEditing = false;
                            MessageBox.Show("Please Strike Request first...");
                            return;
                        }
                        watch.Over = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                        watch.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                        if (AppGlobal.EnterLots < watch.Over)
                        {
                            MessageBox.Show("Enter Wind Qty is more than Max Qty Limit | Max Qty Limit is " + Convert.ToString(AppGlobal.EnterLots));
                            return;
                        }
                        if (AppGlobal.EnterLots < watch.Round)
                        {
                            MessageBox.Show("Enter Unwind Qty is more than Max Qty Limit | Max Qty Limit is " + Convert.ToString(AppGlobal.EnterLots));
                            return;
                        }
                        BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                        snd.TransCode = 1;
                        snd.UniqueID = unique;
                        snd.Wind = Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
                        snd.Unwind = Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
                        snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                        snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                        snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                        snd.Token = Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo);
                        snd.gui_id = watch.Gui_id;

                        watch.Wind = Convert.ToDecimal(watch.RowData.Cells[WatchConst.Wind].Value);
                        watch.unWind = Convert.ToDecimal(watch.RowData.Cells[WatchConst.UnWind].Value);

                        if (watch.StrategyId == 91 || watch.StrategyId == 12211)
                        {
                            if (watch.Leg2.ContractInfo.TokenNo == "0")
                            {
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) > 0)
                                {
                                    double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) * 2;
                                    if (Fspread != 0)
                                    {
                                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) > Fspread)
                                        {
                                            MessageBox.Show("Please Check Wind Spread!!!!");
                                            return;
                                        }
                                    }
                                }
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) > 0)
                                {

                                    double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) / 2;
                                    if (Rspread != 0)
                                    {
                                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                                        {
                                            MessageBox.Show("Please Check Unwind Spread!!!!");
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) < 0)
                                {
                                    double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) * 2;
                                    if (Fspread != 0)
                                    {
                                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) < Fspread)
                                        {
                                            MessageBox.Show("Please Check Wind Spread!!!!");
                                            return;
                                        }
                                    }
                                }
                                else if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) > 0)
                                {

                                    double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) / 2;
                                    if (Fspread != 0)
                                    {
                                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) < Fspread)
                                        {
                                            MessageBox.Show("Please Check Wind Spread!!!!");
                                            return;
                                        }
                                    }
                                }
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) < 0)
                                {
                                    double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) * 2;
                                    if (Rspread != 0)
                                    {
                                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                                        {
                                            MessageBox.Show("Please Check Unwind Spread!!!!");
                                            return;
                                        }
                                    }
                                }
                                else if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) > 0)
                                {
                                    double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) / 2;
                                    if (Rspread != 0)
                                    {
                                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                                        {
                                            MessageBox.Show("Please Check Unwind Spread!!!!");
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) < 0)
                            {
                                double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) * 2;
                                if (Fspread != 0)
                                {
                                    if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) < Fspread)
                                    {
                                        MessageBox.Show("Please Check Wind Spread!!!!");
                                        return;
                                    }
                                }
                            }
                            else if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) > 0)
                            {

                                double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) / 2;
                                if (Fspread != 0)
                                {
                                    if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) < Fspread)
                                    {
                                        MessageBox.Show("Please Check Wind Spread!!!!");
                                        return;
                                    }
                                }
                            }
                            if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) < 0)
                            {
                                double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) * 2;
                                if (Rspread != 0)
                                {
                                    if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                                    {
                                        MessageBox.Show("Please Check Unwind Spread!!!!");
                                        return;
                                    }
                                }
                            }
                            else if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) > 0)
                            {
                                double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) / 2;
                                if (Rspread != 0)
                                {
                                    if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                                    {
                                        MessageBox.Show("Please Check Unwind Spread!!!!");
                                        return;
                                    }
                                }
                            }
                        }
                        if (((snd.Wind / 100) + (snd.Unwind / 100)) < 0)
                        {
                            MessageBox.Show("Please Check Wind and Unwind Parameter!!!!");
                            return;
                        }
                        TransactionWatch.ErrorMessage("UserUniqueId|" + snd.UniqueID + "|wind|" + snd.Wind + "|unwind|" + snd.Unwind + "|Long|" + snd.Open + "|short|" + snd.Round + "|trail_Pts|" + watch.trail_No + "|trail_lots|" + watch.trail_Lots + "|profit|" + watch.trail_Profit + "|BuyStopLoss|" + watch.TGBuyPrice + "|SellStopLoss|" + watch.TGBuyPrice + "|BuyDD|" + watch.DD_TGBuyPrice + "|sellDD|" + watch.DD_TGSellPrice + "|BuyBmDD|" + watch.DD_bm_Buy + "|SellBmDD|" + watch.DD_bm_Sell);
                        //if (watch.DD_BuyOrderflg == true || watch.DD_SellOrderflg == true)
                        //    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.MediumSeaGreen;
                        //else if (watch.SL_BuyOrderflg == true || watch.SL_SellOrderflg == true)
                        //    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.MediumSpringGreen;
                        //else
                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
                        long seq = ClassDisruptor.ringBufferRequest.Next();
                        ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                        ClassDisruptor.ringBufferRequest.Publish(seq);
                    }
                    else
                    {
                        TransactionWatch.ErrorMessage("Enter not perform Unique|" + unique + "|wind|" + watch.RowData.Cells[WatchConst.Wind].Value + "|Unwind|" + watch.RowData.Cells[WatchConst.UnWind].Value + "|Long|" + watch.RowData.Cells[WatchConst.FQty].Value + "|short|" + watch.RowData.Cells[WatchConst.RQty].Value);
                        TransactionWatch.ErrorMessage("GotTabFromEditing|" + AppGlobal.GotTabFromEditing + "|GotKeyDownFromEditing|" + AppGlobal.GotKeyDownFromEditing + "|GotEnterFromEditing|" + AppGlobal.GotEnterFromEditing);
                    }
                    #endregion
                }
                else if (e.KeyCode == Keys.Oemtilde)
                {
                    #region Tilde
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();
                    watch = AppGlobal.MarketWatch[iRow];
                    if (watch.StrategyId == 0)
                        return;

                    if (watch.IsStrikeReq == false)
                    {
                        MessageBox.Show("Please Strike Request first...");
                        return;
                    }
                    if (AppGlobal.EnterLots < watch.Over)
                    {
                        MessageBox.Show("Enter Wind Qty is more than Max Qty Limit | Max Qty Limit is " + Convert.ToString(AppGlobal.EnterLots));
                        return;
                    }
                    if (AppGlobal.EnterLots < watch.Round)
                    {
                        MessageBox.Show("Enter Unwind Qty is more than Max Qty Limit | Max Qty Limit is " + Convert.ToString(AppGlobal.EnterLots));
                        return;
                    }

                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                    snd.TransCode = 2;

                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    snd.UniqueID = unique;
                    snd.Wind = Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
                    snd.Unwind = Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
                    snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                    snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                    snd.Token = Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo);
                    snd.gui_id = watch.Gui_id;
                    if (((snd.Wind / 100) + (snd.Unwind / 100)) < 0)
                    {
                        MessageBox.Show("Please Check Wind and Unwind Parameter!!!!");
                        return;
                    }

                    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
                    long seq = ClassDisruptor.ringBufferRequest.Next();
                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                    ClassDisruptor.ringBufferRequest.Publish(seq);

                    #endregion
                }
                else if (e.KeyCode == Keys.F12)
                {
                    DataGridView dgv = dgvMarketWatch;
                    try
                    {
                        int totalRows = dgv.Rows.Count;
                        //get index of the row for the selected cell
                        int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                        if (rowIndex == 0)
                            return;
                        //get index of the column for the selected cell
                        int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                        DataGridViewRow selectedRow = dgv.Rows[rowIndex];
                        dgv.Rows.Remove(selectedRow);
                        dgv.Rows.Insert(rowIndex - 1, selectedRow);
                        dgv.ClearSelection();
                        dgv.Rows[rowIndex - 1].Cells[colIndex].Selected = true;

                        for (int i = 0; i < dgvMarketWatch.Rows.Count - 1; i++)
                        {
                            string rule = Convert.ToString(dgvMarketWatch.Rows[i].Cells[WatchConst.Rule].Value);
                            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Ruleno) == rule)))
                            {
                                int k = AppGlobal.MarketWatch.IndexOf(watch);
                                AppGlobal.MarketWatch.RemoveAt(k);
                                AppGlobal.MarketWatch.Insert(i, watch);
                                break;
                            }
                        }
                        MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                        dgv.CurrentCell = dgv.Rows[rowIndex - 1].Cells[colIndex];
                    }
                    catch { }
                }
                else if (e.KeyCode == Keys.F11)
                {
                    DataGridView dgv = dgvMarketWatch;
                    try
                    {
                        int totalRows = dgv.Rows.Count;
                        // get index of the row for the selected cell
                        int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                        if (rowIndex == totalRows - 1)
                            return;
                        // get index of the column for the selected cell
                        int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                        DataGridViewRow selectedRow = dgv.Rows[rowIndex];
                        dgv.Rows.Remove(selectedRow);
                        dgv.Rows.Insert(rowIndex + 1, selectedRow);
                        dgv.ClearSelection();
                        dgv.Rows[rowIndex + 1].Cells[colIndex].Selected = true;
                        for (int i = 0; i < dgvMarketWatch.Rows.Count - 1; i++)
                        {
                            string rule = Convert.ToString(dgvMarketWatch.Rows[i].Cells[WatchConst.Rule].Value);
                            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Ruleno) == rule)))
                            {
                                int k = AppGlobal.MarketWatch.IndexOf(watch);
                                AppGlobal.MarketWatch.RemoveAt(k);
                                AppGlobal.MarketWatch.Insert(i, watch);
                                break;
                            }
                        }
                        MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                        MatchUniqueNo();

                        dgv.CurrentCell = dgv.Rows[rowIndex + 1].Cells[colIndex];
                    }
                    catch { }
                }
                else if (e.KeyCode == Keys.Space)
                {
                    #region Strike Request
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();
                    watch = AppGlobal.MarketWatch[iRow];

                    if (watch.StrategyId == 0)
                        return;

                    #region Future Exp
                    /*if (watch.StrategyId == 2211 || watch.StrategyId == 888)
                    {
                        string n11 = Convert.ToString(watch.Expiry);
                        string n21 = n11.Substring(0, 2);
                        string n31 = n11.Substring(2, 3);
                        string n41 = n11.Substring(7, 2);
                        string L1Expiry = "20" + n41 + n31 + n21;

                        string n12 = Convert.ToString(watch.Expiry2);
                        string n22 = n12.Substring(0, 2);
                        string n32 = n12.Substring(2, 3);
                        string n42 = n12.Substring(7, 2);
                        string L3Expiry = "20" + n42 + n32 + n22;

                        int mont0 = DateTime.ParseExact(n31, "MMM", new CultureInfo("en-US")).Month;
                        int _currentmonth = mont0;
                        uint _expiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[0]));
                        string _expiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, _expiry).ToString("yyyyMMMdd");
                        string _sf12 = Convert.ToString(_expiry1);
                        string _sf22 = _sf12.Substring(0, 4);
                        string _sf32 = _sf12.Substring(4, 3);
                        string _sf42 = _sf12.Substring(7, 2);
                        int _montf = DateTime.ParseExact(_sf32, "MMM", new CultureInfo("en-US")).Month;
                        System.Globalization.DateTimeFormatInfo _mffi1 = new System.Globalization.DateTimeFormatInfo();
                        string _monStringf = "";
                        if (_montf <= 9)
                        {
                            _monStringf = "0" + Convert.ToString(_montf);
                        }
                        else
                        {
                            _monStringf = Convert.ToString(_montf);
                        }
                        string _sf52 = _sf22 + _sf32 + _sf42;
                        string _selectFut = _sf52;
                        uint _nxtexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[1]));
                        string _nxtexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, _nxtexpiry).ToString("yyyyMMMdd");
                        string _nxtsf12 = Convert.ToString(_nxtexpiry1);
                        string _nxtsf22 = _nxtsf12.Substring(0, 4);
                        string _nxtsf32 = _nxtsf12.Substring(4, 3);
                        string _nxtsf42 = _nxtsf12.Substring(7, 2);
                        int _nxtmontf = DateTime.ParseExact(_nxtsf32, "MMM", new CultureInfo("en-US")).Month;
                        System.Globalization.DateTimeFormatInfo _nxtmffi1 = new System.Globalization.DateTimeFormatInfo();
                        string _nxtmonStringf = "";
                        if (_nxtmontf <= 9)
                        {
                            _nxtmonStringf = "0" + Convert.ToString(_nxtmontf);
                        }
                        else
                        {
                            _nxtmonStringf = Convert.ToString(_nxtmontf);
                        }
                        string _nxtsf52 = _nxtsf22 + _nxtsf32 + _nxtsf42;
                        if (_currentmonth == _nxtmontf)
                            _selectFut = _nxtsf52;
                        uint _farexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[2]));
                        string _farexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, _farexpiry).ToString("yyyyMMMdd");
                        string _farsf12 = Convert.ToString(_farexpiry1);
                        string _farsf22 = _farsf12.Substring(0, 4);
                        string _farsf32 = _farsf12.Substring(4, 3);
                        string _farsf42 = _farsf12.Substring(7, 2);
                        int _farmontf = DateTime.ParseExact(_farsf32, "MMM", new CultureInfo("en-US")).Month;
                        System.Globalization.DateTimeFormatInfo _farmffi1 = new System.Globalization.DateTimeFormatInfo();
                        string _farmonStringf = "";
                        if (_farmontf <= 9)
                        {
                            _farmonStringf = "0" + Convert.ToString(_farmontf);
                        }
                        else
                        {
                            _farmonStringf = Convert.ToString(_farmontf);
                        }
                        string _farsf52 = _farsf22 + _farsf32 + _farsf42;
                        if (_currentmonth == _farmontf)
                            _selectFut = _farsf52;
                        string Sym = Convert.ToString(watch.Leg1.ContractInfo.Symbol);
                        if (Sym == "BANKNIFTY" && ((watch.Leg1.ContractInfo.Series == "CE" && watch.Leg2.ContractInfo.Series == "PE") || (watch.Leg1.ContractInfo.Series == "PE" && watch.Leg2.ContractInfo.Series == "CE")))
                        {
                            if (L1Expiry != _selectFut)
                            {
                                MessageBox.Show("BankNifty not allowed weekly rule");
                                return;
                            }

                            if (L3Expiry != _selectFut)
                            {
                                MessageBox.Show("BankNifty not allowed weekly rule");
                                return;
                            }
                        }
                    }*/

                    #endregion

                    string segment1 = "";
                    string segment2 = "";
                    string segment3 = "";
                    string segment4 = "";

                    string LotSize1 = "";
                    string LotSize2 = "";
                    string LotSize3 = "";
                    string LotSize4 = "";

                    if (watch.StrategyId == 111 || watch.StrategyId == 211)
                    {
                        string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                        DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                        foreach (DataRow dr in dr11)
                        {
                            segment1 = dr["Segment"].ToString();
                            LotSize1 = dr["LotSize"].ToString();
                        }
                        string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                        DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                        foreach (DataRow dr in dr12)
                        {
                            segment2 = dr["Segment"].ToString();
                            LotSize2 = dr["LotSize"].ToString();
                        }
                        if (segment1 != segment2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2);
                            return;
                        }
                        if (LotSize1 != LotSize2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n LotSize 1: " + LotSize1 +
                                             "\n LotSize 2: " + LotSize2);
                            return;
                        }
                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                        watch.IsStrikeReq = true;
                        TokenRequest11_12(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                        TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
                    }
                    else if (watch.StrategyId == 311)
                    {
                        string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                        DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                        foreach (DataRow dr in dr11)
                        {
                            segment1 = dr["Segment"].ToString();
                            LotSize1 = dr["LotSize"].ToString();
                        }
                        string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                        DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                        foreach (DataRow dr in dr12)
                        {
                            segment2 = dr["Segment"].ToString();
                            LotSize2 = dr["LotSize"].ToString();
                        }
                        if (segment1 != segment2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2);
                            return;
                        }
                        if (LotSize1 != LotSize2)
                        {

                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n LotSize 1: " + LotSize1 +
                                             "\n LotSize 2: " + LotSize2);
                            return;
                        }
                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                        watch.IsStrikeReq = true;
                        TokenRequest311(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio);
                        TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
                    }
                    else if (watch.StrategyId == 91)
                    {

                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                        watch.IsStrikeReq = true;
                        TokenRequest91(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                        TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID
                                                        + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) +
                                                        "|Series|" + watch.Leg1.ContractInfo.Series + "|SingleLeg");
                        TransactionWatch.TransactionMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID
                                                        + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) +
                                                        "|Series|" + watch.Leg1.ContractInfo.Series + "|SingleLeg", Color.Blue);

                    }
                    else if (watch.StrategyId == 2211)
                    {

                        string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                        DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                        foreach (DataRow dr in dr11)
                        {
                            segment1 = dr["Segment"].ToString();
                            LotSize1 = dr["LotSize"].ToString();
                        }
                        string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                        DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                        foreach (DataRow dr in dr12)
                        {
                            segment2 = dr["Segment"].ToString();
                            LotSize2 = dr["LotSize"].ToString();
                        }
                        if (segment1 != segment2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2);
                            return;
                        }
                        if (LotSize1 != LotSize2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n LotSize 1: " + LotSize1 +
                                             "\n LotSize 2: " + LotSize2);
                            return;
                        }
                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                        watch.IsStrikeReq = true;

                        TokenRequest2211(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio);
                        TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
                    }
                    else if (watch.StrategyId == 888)
                    {
                        string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                        DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                        foreach (DataRow dr in dr11)
                        {
                            segment1 = dr["Segment"].ToString();
                            LotSize1 = dr["LotSize"].ToString();
                        }

                        string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                        DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                        foreach (DataRow dr in dr12)
                        {
                            segment2 = dr["Segment"].ToString();
                            LotSize2 = dr["LotSize"].ToString();
                        }

                        string str3 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg3.ContractInfo.TokenNo) + "'";
                        DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str3);
                        foreach (DataRow dr in dr13)
                        {
                            segment3 = dr["Segment"].ToString();
                            LotSize3 = dr["LotSize"].ToString();
                        }
                        if (segment1 != segment2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2 +
                                            "\n Segment 3: " + segment3);
                            return;
                        }
                        else if (segment2 != segment3)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2 +
                                            "\n Segment 3: " + segment3);
                            return;
                        }
                        if (LotSize1 != LotSize2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n LotSize 1: " + LotSize1 +
                                             "\n LotSize 2: " + LotSize2 +
                                             "\n LotSize 3: " + LotSize3);
                            return;
                        }
                        else if (LotSize2 != LotSize3)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n LotSize 1: " + LotSize1 +
                                             "\n LotSize 2: " + LotSize2 +
                                             "\n LotSize 3: " + LotSize3);
                            return;
                        }
                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                        watch.IsStrikeReq = true;
                        TokenRequest888(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio);
                        TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
                    }
                    else if (watch.StrategyId == 121)
                    {

                        string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                        DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                        foreach (DataRow dr in dr11)
                        {
                            segment1 = dr["Segment"].ToString();
                            LotSize1 = dr["LotSize"].ToString();
                        }

                        string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                        DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                        foreach (DataRow dr in dr12)
                        {
                            segment2 = dr["Segment"].ToString();
                            LotSize2 = dr["LotSize"].ToString();
                        }

                        string str3 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg3.ContractInfo.TokenNo) + "'";
                        DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str3);
                        foreach (DataRow dr in dr13)
                        {
                            segment3 = dr["Segment"].ToString();
                            LotSize3 = dr["LotSize"].ToString();
                        }
                        if (segment1 != segment2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2 +
                                            "\n Segment 3: " + segment3);
                            return;
                        }
                        else if (segment2 != segment3)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2 +
                                            "\n Segment 3: " + segment3);
                            return;
                        }
                        if (LotSize1 != LotSize2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n LotSize 1: " + LotSize1 +
                                             "\n LotSize 2: " + LotSize2 +
                                             "\n LotSize 3: " + LotSize3);
                            return;
                        }
                        else if (LotSize2 != LotSize3)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n LotSize 1: " + LotSize1 +
                                             "\n LotSize 2: " + LotSize2 +
                                             "\n LotSize 3: " + LotSize3);
                            return;
                        }
                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                        watch.IsStrikeReq = true;
                        if (Convert.ToString(watch.Leg1.ContractInfo.Series) == "CE")
                            TokenRequest7121(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio, true);
                        else
                            TokenRequest7121(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio, false);
                        TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
                    }
                    else if (watch.StrategyId == 1331)
                    {

                        string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                        DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                        foreach (DataRow dr in dr11)
                        {
                            segment1 = dr["Segment"].ToString();
                            LotSize1 = dr["LotSize"].ToString();
                        }

                        string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                        DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                        foreach (DataRow dr in dr12)
                        {
                            segment2 = dr["Segment"].ToString();
                            LotSize2 = dr["LotSize"].ToString();
                        }
                        string str3 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg3.ContractInfo.TokenNo) + "'";
                        DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str3);
                        foreach (DataRow dr in dr13)
                        {
                            segment3 = dr["Segment"].ToString();
                            LotSize3 = dr["LotSize"].ToString();

                        }
                        string str4 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg4.ContractInfo.TokenNo) + "'";
                        DataRow[] dr14 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str4);
                        foreach (DataRow dr in dr14)
                        {
                            segment4 = dr["Segment"].ToString();
                            LotSize4 = dr["LotSize"].ToString();
                        }
                        if (segment1 != segment2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2 +
                                            "\n Segment 3: " + segment3 +
                                            "\n Segment 4: " + segment4);
                            return;
                        }
                        else if (segment2 != segment3)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2 +
                                            "\n Segment 3: " + segment3 +
                                            "\n Segment 4: " + segment4);
                            return;
                        }
                        else if (segment3 != segment4)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n Segment 1: " + segment1 +
                                             "\n Segment 2: " + segment2 +
                                             "\n Segment 3: " + segment3 +
                                             "\n Segment 4: " + segment4);
                            return;
                        }
                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                        watch.IsStrikeReq = true;

                        if (Convert.ToString(watch.Leg1.ContractInfo.Series) == "CE")
                            TokenRequest1331(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg4.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), true);
                        else
                            TokenRequest1331(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg4.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), false);
                        TransactionWatch.ErrorMessage("NewStrikeReq|" + "UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) + "|Series1|" + Convert.ToString(watch.Leg1.ContractInfo.Series)
                                                       + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series2|" + Convert.ToString(watch.Leg2.ContractInfo.Series)
                                                       + "|Strike3|" + Convert.ToInt32(watch.Leg3.ContractInfo.StrikePrice) + "|Series3|" + Convert.ToString(watch.Leg3.ContractInfo.Series)
                                                       + "|Strike4|" + Convert.ToInt32(watch.Leg4.ContractInfo.StrikePrice) + "|Series4|" + Convert.ToString(watch.Leg4.ContractInfo.Series));

                    }
                    else if (watch.StrategyId == 1221)
                    {
                        string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                        DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                        foreach (DataRow dr in dr11)
                        {
                            segment1 = dr["Segment"].ToString();
                            LotSize1 = dr["LotSize"].ToString();
                        }
                        string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                        DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                        foreach (DataRow dr in dr12)
                        {
                            segment2 = dr["Segment"].ToString();
                            LotSize2 = dr["LotSize"].ToString();
                        }
                        string str3 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg3.ContractInfo.TokenNo) + "'";
                        DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str3);
                        foreach (DataRow dr in dr13)
                        {
                            segment3 = dr["Segment"].ToString();
                            LotSize3 = dr["LotSize"].ToString();

                        }
                        string str4 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg4.ContractInfo.TokenNo) + "'";
                        DataRow[] dr14 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str4);
                        foreach (DataRow dr in dr14)
                        {
                            segment4 = dr["Segment"].ToString();
                            LotSize4 = dr["LotSize"].ToString();
                        }
                        if (segment1 != segment2)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2 +
                                            "\n Segment 3: " + segment3 +
                                            "\n Segment 4: " + segment4);
                            return;
                        }
                        else if (segment2 != segment3)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                            "\n Segment 1: " + segment1 +
                                            "\n Segment 2: " + segment2 +
                                            "\n Segment 3: " + segment3 +
                                            "\n Segment 4: " + segment4);
                            return;
                        }
                        else if (segment3 != segment4)
                        {
                            MessageBox.Show("This rule not allowed for Trading" +
                                             "\n Segment 1: " + segment1 +
                                             "\n Segment 2: " + segment2 +
                                             "\n Segment 3: " + segment3 +
                                             "\n Segment 4: " + segment4);
                            return;
                        }
                        dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                        watch.IsStrikeReq = true;

                        TokenRequest1221(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg4.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                        TransactionWatch.ErrorMessage("NewStrikeReq|" + "UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) + "|Series1|" + Convert.ToString(watch.Leg1.ContractInfo.Series)
                                                       + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series2|" + Convert.ToString(watch.Leg2.ContractInfo.Series)
                                                       + "|Strike3|" + Convert.ToInt32(watch.Leg3.ContractInfo.StrikePrice) + "|Series3|" + Convert.ToString(watch.Leg3.ContractInfo.Series)
                                                       + "|Strike4|" + Convert.ToInt32(watch.Leg4.ContractInfo.StrikePrice) + "|Series4|" + Convert.ToString(watch.Leg4.ContractInfo.Series));
                    }
                    else if (watch.StrategyId == 12211 || watch.StrategyId == 32211)
                    {
                        if (watch.Leg2.ContractInfo.TokenNo != "0")
                        {
                            string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                            DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                            foreach (DataRow dr in dr11)
                            {
                                segment1 = dr["Segment"].ToString();
                                LotSize1 = dr["LotSize"].ToString();
                            }
                            string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                            DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                            foreach (DataRow dr in dr12)
                            {
                                segment2 = dr["Segment"].ToString();
                                LotSize2 = dr["LotSize"].ToString();
                            }                           
                            if (LotSize1 != LotSize2)
                            {
                                MessageBox.Show("This rule not allowed for Trading" +
                                                 "\n LotSize 1: " + LotSize1 +
                                                 "\n LotSize 2: " + LotSize2);
                                return;
                            }
                            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                            watch.IsStrikeReq = true;
                            TokenRequest12211(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio, (int)(watch.UniqueIdLeg1), (int)(watch.UniqueIdLeg2));
                            TransactionWatch.ErrorMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                               + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                               + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice)
                                                               + "|Series|" + watch.Leg1.ContractInfo.Series + "|UniqueLeg1|" + watch.UniqueIdLeg1 + "|UniqueLeg2|" + watch.UniqueIdLeg2 + "|Strangle");

                            TransactionWatch.TransactionMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                               + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                               + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice)
                                                               + "|Series|" + watch.Leg1.ContractInfo.Series + "|UniqueLeg1|" + watch.UniqueIdLeg1 + "|UniqueLeg2|" + watch.UniqueIdLeg2 + "|Strangle", Color.Blue);
                        }
                        else
                        {
                            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                            watch.IsStrikeReq = true;
                            TokenRequest912211(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                            TransactionWatch.ErrorMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                            + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Series|" + watch.Leg1.ContractInfo.Series + "|StrangleSingleLeg");
                            TransactionWatch.TransactionMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                            + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Series|" + watch.Leg1.ContractInfo.Series + "|StrangleSingleLeg", Color.Blue);
                        }
                    }
                    else if (watch.StrategyId == 1113 || watch.StrategyId == 1114)
                    {
                        if (watch.Leg2.ContractInfo.TokenNo != "0")
                        {
                            string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                            DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                            foreach (DataRow dr in dr11)
                            {
                                segment1 = dr["Segment"].ToString();
                                LotSize1 = dr["LotSize"].ToString();
                            }
                            string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                            DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                            foreach (DataRow dr in dr12)
                            {
                                segment2 = dr["Segment"].ToString();
                                LotSize2 = dr["LotSize"].ToString();
                            }
                            if (LotSize1 != LotSize2)
                            {
                                MessageBox.Show("This rule not allowed for Trading" +
                                                 "\n LotSize 1: " + LotSize1 +
                                                 "\n LotSize 2: " + LotSize2);
                                return;
                            }
                            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                            watch.IsStrikeReq = true;
                            TokenRequestCalender(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio, (int)(watch.UniqueIdLeg1), (int)(watch.UniqueIdLeg2));
                            TransactionWatch.ErrorMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                               + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                               + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice)
                                                               + "|Series|" + watch.Leg1.ContractInfo.Series + "|UniqueLeg1|" + watch.UniqueIdLeg1 + "|UniqueLeg2|" + watch.UniqueIdLeg2 + "|Strangle");

                            TransactionWatch.TransactionMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                               + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                               + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice)
                                                               + "|Series|" + watch.Leg1.ContractInfo.Series + "|UniqueLeg1|" + watch.UniqueIdLeg1 + "|UniqueLeg2|" + watch.UniqueIdLeg2 + "|Strangle", Color.Blue);
                        }
                        else
                        {
                            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                            watch.IsStrikeReq = true;
                            TokenRequestCalender91(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                            TransactionWatch.ErrorMessage("NewStrikeReq|" + watch.StrategyName  + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                            + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Series|" + watch.Leg1.ContractInfo.Series + "|StrangleSingleLeg");
                            TransactionWatch.TransactionMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                            + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                            + "|Series|" + watch.Leg1.ContractInfo.Series + "|StrangleSingleLeg", Color.Blue);
                        }

                    }


                    #endregion
                }
                else if (e.KeyCode == Keys.F6)
                {
                    AppGlobal._StrategySelection = new StrategySelection();
                    AppGlobal._StrategySelection.Show();
                }
                else if (e.KeyCode == Keys.F7)
                {
                    AppGlobal._GuiLevelPayoff = new GuiLevelPayoff();
                    AppGlobal._GuiLevelPayoff.Show();
                }
                else if (e.KeyCode == Keys.S && e.Control)
                {
                    if (AppGlobal.MarketWatch.Count() == 0)
                    {
                        MessageBox.Show("Add Strategy First!!!");
                        return;
                    }
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch _watch = new MarketWatch();
                    _watch = AppGlobal.MarketWatch[iRow];
                    AppGlobal.SelectedStrategy = _watch.Strategy;
                    if (AppGlobal.__singleLeg == null)
                    {
                        AppGlobal.__singleLeg = new SingleLeg();
                        AppGlobal.__singleLeg.Show();
                    }
                    else
                    {
                        AppGlobal.__singleLeg.Show();
                        AppGlobal.__singleLeg.Activate();
                    }
                }
                else if (e.KeyCode == Keys.Y && e.Control)
                {
                    if (AppGlobal._strategySqOff == null)
                    {
                        AppGlobal._strategySqOff = new StrategySqOff();
                        AppGlobal._strategySqOff.Show();
                    }
                    else
                    {
                        AppGlobal._strategySqOff.Show();
                        AppGlobal._strategySqOff.Activate(); 
                    }
                }
                else if (e.KeyCode == Keys.F5)
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = AppGlobal.MarketWatch[iRow];
                    if (watch.StrategyId != 0)
                    {
                        AppGlobal._VARAnalysis = new VARAnalysis();
                        AppGlobal._VARAnalysis.Show();
                        AppGlobal._VARAnalysis.Text = watch.Strategy.ToString();
                    }
                }
                else if (e.KeyCode == Keys.B && e.Alt)
                {
                    if (AppGlobal._dd_BuyParameter == null)
                    {
                        AppGlobal._dd_BuyParameter = new DD_BuyParameter();
                        AppGlobal._dd_BuyParameter.Show();
                    }
                    else
                    {
                        AppGlobal._dd_BuyParameter.Show();
                        AppGlobal._dd_BuyParameter.Activate();
                    }
                }
                else if (e.KeyCode == Keys.S && e.Alt)
                {
                    if (AppGlobal._dd_SellParameter == null)
                    {
                        AppGlobal._dd_SellParameter = new DD_SellParameter();
                        AppGlobal._dd_SellParameter.Show();
                    }
                    else
                    {
                        AppGlobal._dd_SellParameter.Show();
                        AppGlobal._dd_SellParameter.Activate();
                    }
                }
                else if (e.KeyCode == Keys.Q && e.Control)
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = AppGlobal.MarketWatch[iRow];
                    if (watch.IsStrikeReq != true)
                    {
                        MessageBox.Show("Please Strike Req First !!!!");
                        return;
                    }
                    string message = "";
                    if (watch.StrategyId == 91)
                    {
                        if (watch.posInt != 0)
                        {
                            message = "Enter Password for SqAll UniqueID " + watch.uniqueId + " | Position | " + watch.posInt + " | Avg Price | " + watch.Leg1.N_Price;
                            string password = Microsoft.VisualBasic.Interaction.InputBox(message, "Confimation", "").ToString();
                            if (password == "123")
                            {
                                System.Threading.Tasks.Task.Factory.StartNew(() =>
                                {
                                    SqoffAll(watch, "PasswordSqOff");
                                });
                            }
                            else
                            {
                                MessageBox.Show(("WrongPassword|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                     watch.Expiry));
                                TransactionWatch.ErrorMessage("WrongPassword|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                     watch.Expiry);
                                TransactionWatch.TransactionMessage("WrongPassword|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                     watch.Expiry, Color.Blue);
                            }
                        }
                        else
                        {
                            MessageBox.Show("NoPosition|UniqueID| " + watch.uniqueId);
                            TransactionWatch.ErrorMessage("NoPosition|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                     watch.Expiry);
                            TransactionWatch.TransactionMessage("NoPosition|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                 watch.Expiry, Color.Blue);
                        }
                    }
                }
                else if (e.KeyCode == Keys.Z && e.Control)
                {
                    if (AppGlobal.MarketWatch.Count() == 0)
                    {
                        MessageBox.Show("Add Strategy First!!!");
                        return;
                    }
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch _watch = new MarketWatch();
                    _watch = AppGlobal.MarketWatch[iRow];
                    AppGlobal.SelectedStrategy = _watch.Strategy;
                    if (AppGlobal._straddleJodi == null)
                    {
                        AppGlobal._straddleJodi = new StraddleJodi();
                        AppGlobal._straddleJodi.Show();
                    }
                    else
                    {
                        AppGlobal._straddleJodi.Show();
                        AppGlobal._straddleJodi.Activate();
                    }
                }
                else if (e.KeyCode == Keys.X && e.Control)
                {
                    if (AppGlobal.MarketWatch.Count() == 0)
                    {
                        MessageBox.Show("Add Strategy First!!!");
                        return;
                    }
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch _watch = new MarketWatch();
                    _watch = AppGlobal.MarketWatch[iRow];

                    if (!_watch.StrategyName.Contains("MainJodiStraddle"))
                    {
                        MessageBox.Show("Please select Proper Strategy!!!!");
                        return;
                    }
                    if (AppGlobal._ruleModifyJodi == null)
                    {
                        AppGlobal._ruleModifyJodi = new RuleModifyJodi();
                        AppGlobal._ruleModifyJodi.Show();
                    }
                    else
                    {
                        AppGlobal._ruleModifyJodi.Show();
                        AppGlobal._ruleModifyJodi.Activate();
                    }
                }
                else if (e.KeyCode == Keys.R && e.Control)
                {
                    if (AppGlobal._BuyStopLoss == null)
                    {
                        AppGlobal._BuyStopLoss = new BuyStopLoss();
                        AppGlobal._BuyStopLoss.Show();
                    }
                    else
                    {
                        AppGlobal._BuyStopLoss.Show();
                        AppGlobal._BuyStopLoss.Activate();
                    }
                }
                else if (e.KeyCode == Keys.T && e.Control)
                {
                    if (AppGlobal._SellStopLoss == null)
                    {
                        AppGlobal._SellStopLoss = new SellStopLoss();
                        AppGlobal._SellStopLoss.Show();
                    }
                    else
                    {
                        AppGlobal._SellStopLoss.Show();
                        AppGlobal._SellStopLoss.Activate();
                    }
                }
                else if (e.KeyCode == Keys.W && e.Control)
                {

                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = AppGlobal.MarketWatch[iRow];
                    if (watch.StrategyId != 91)
                    {
                        MessageBox.Show("Please not allowed other than single leg Strategy !!!!");
                        return;
                    }
                    if (AppGlobal._ImmediateWind == null)
                    {
                        AppGlobal._ImmediateWind = new ImmediateWind();
                        AppGlobal._ImmediateWind.Show();
                    }
                    else
                    {
                        AppGlobal._ImmediateWind.Show();
                        AppGlobal._ImmediateWind.Activate();
                    }
                }
                else if (e.KeyCode == Keys.U && e.Control)
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = AppGlobal.MarketWatch[iRow];
                    if (watch.StrategyId != 91)
                    {
                        MessageBox.Show("Please not allowed other than single leg Strategy !!!!");
                        return;
                    }
                    if (AppGlobal._ImmediateUnWind == null)
                    {
                        AppGlobal._ImmediateUnWind = new ImmediateUnwind();
                        AppGlobal._ImmediateUnWind.Show();
                    }
                    else
                    {
                        AppGlobal._ImmediateUnWind.Show();
                        AppGlobal._ImmediateUnWind.Activate();
                    }
                }
                else if (e.KeyCode == Keys.E && e.Control)
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch _watch = new MarketWatch();
                    _watch = AppGlobal.MarketWatch[iRow];
                    if (_watch.StrategyId == 0)
                        return;

                    if (AppGlobal._straddleSellStopLoss == null)
                    {
                        AppGlobal._straddleSellStopLoss = new StraddleSellStopLoss();
                        AppGlobal._straddleSellStopLoss.Show();
                    }
                    else
                    {
                        AppGlobal._straddleSellStopLoss.Show();
                        AppGlobal._straddleSellStopLoss.Activate();
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch _watch = AppGlobal.MarketWatch[iRow];

                    if (_watch.StrategyId == 0)
                        return;


                    if (_watch.thread != null)
                    {
                        if (_watch.thread.IsAlive)
                            _watch.thread.Suspend();
                    }
                    if (_watch.thread1 != null)
                    {
                        if (_watch.thread1.IsAlive)
                            _watch.thread1.Suspend();
                    }
                    if (_watch.thread2 != null)
                    {
                        if (_watch.thread2.IsAlive)
                            _watch.thread2.Suspend();
                    }
                    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;

                    TransactionWatch.ErrorMessage("|Unique|" + _watch.uniqueId + "|strategy|" + _watch.StrategyId + "|Alert Stop|");

                }
                else if (e.KeyCode == Keys.F && e.Control)
                {
                    return;
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch _watch = new MarketWatch();
                    _watch = AppGlobal.MarketWatch[iRow];
                    if (_watch.StrategyId == 0)
                        return;

                    if (AppGlobal._PositionAction == null)
                    {
                        AppGlobal._PositionAction = new Position_Action();
                        AppGlobal._PositionAction.Show();
                    }
                    else
                    {
                        AppGlobal._PositionAction.Show();
                        AppGlobal._PositionAction.Activate();
                    }
                }
                else if (e.KeyCode == Keys.G && e.Control)
                {                   
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch _watch = new MarketWatch();
                    _watch = AppGlobal.MarketWatch[iRow];
                    if (_watch.StrategyId == 0)
                        return;
                    if (AppGlobal._RuleAction == null)
                    {
                        AppGlobal._RuleAction = new RuleAction();
                        AppGlobal._RuleAction.Show();
                    }
                    else
                    {
                        AppGlobal._RuleAction.Show();
                        AppGlobal._RuleAction.Activate();
                    }
                }
                else if (e.KeyCode == Keys.M && e.Control)
                {
                    if (AppGlobal._limitset == null)
                    {
                        AppGlobal._limitset = new LimitSet();
                        AppGlobal._limitset.Show();
                    }
                    else
                    {
                        AppGlobal._limitset.Show();
                        AppGlobal._limitset.Activate();
                    }
                }

                else if (e.KeyCode == Keys.N && e.Control)
                {
                    if (AppGlobal._buyorder == null)
                    {
                        AppGlobal._buyorder = new BuyOrder();
                        AppGlobal._buyorder.Show();
                    }
                    else
                    {
                        AppGlobal._buyorder.Show();
                        AppGlobal._buyorder.Activate();
                    }
                }

                else if (e.KeyCode == Keys.K && e.Control)
                {
                    if (AppGlobal._sellorder == null)
                    {
                        AppGlobal._sellorder = new SellOrder();
                        AppGlobal._sellorder.Show();
                    }
                    else
                    {
                        AppGlobal._sellorder.Show();
                        AppGlobal._sellorder.Activate();
                    }
                }

            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "dgvMarketWatch_KeyDown")
                                 , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        public void SqoffAll(MarketWatch watch,string msg)
        {
            if (watch.StrategyId != 0)
            {
                if (watch.StrategyId == 91)
                {
                    if (watch.posInt != 0)
                    {
                        AppGlobal.SQAllFlg = true;
                        //System.Threading.Tasks.Task.Factory.StartNew(() =>

                        //Thread t = new Thread(() =>
                           {
                               int pos = Convert.ToInt32(watch.posInt);
                               for (int i = 0; i < Math.Abs(pos); i++)
                               {
                                   if (pos > 0)
                                   {
                                       BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                       snd.TransCode = 10;
                                       UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                       snd.UniqueID = unique;
                                       snd.gui_id = AppGlobal.GUI_ID;
                                       snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                       snd.isWind = false;
                                       snd.Open = 0;

                                       long seq = ClassDisruptor.ringBufferRequest.Next();
                                       ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                       ClassDisruptor.ringBufferRequest.Publish(seq);

                                       TransactionWatch.ErrorMessage("SQALL|" + msg + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                    watch.Expiry + "|" + "Sell|" + watch.MktWind + "|" + pos + "|" + (i + 1))
                                                                    ;
                                       //TransactionWatch.TransactionMessage("SQALL|" + msg + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                //    watch.Expiry + "|" + "Sell|" + watch.MktWind + "|" + pos + "|" + (i + 1), Color.Blue);
                                   }
                                   else if (pos < 0)
                                   {
                                       BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                                       snd.TransCode = 10;
                                       UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                                       snd.UniqueID = unique;
                                       snd.gui_id = AppGlobal.GUI_ID;
                                       snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                                       snd.isWind = true;
                                       snd.Open = 0;

                                       long seq = ClassDisruptor.ringBufferRequest.Next();
                                       ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                                       ClassDisruptor.ringBufferRequest.Publish(seq);

                                       TransactionWatch.ErrorMessage("SQALL|" + msg + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                    watch.Expiry + "|" + "Buy|" + watch.MktunWind + "|" + pos + "|" + (i + 1));
                                       // TransactionWatch.TransactionMessage("SQALL|" + msg + "|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                              //      watch.Expiry + "|" + "Buy|" + watch.MktunWind + "|" + pos + "|" + (i + 1), Color.Blue);
                                   }
                                   Application.DoEvents();
                                   System.Threading.Thread.Sleep(50);                                    
                               }
                           }
                      //  t.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta     
                        //t.Start();
                    }
                    else
                    {
                        TransactionWatch.ErrorMessage("NoPosition|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                             watch.Expiry);
                        TransactionWatch.TransactionMessage("NoPosition|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                             watch.Expiry, Color.Blue);
                    }
                }
            }
        }


        void sendOrder(MarketWatch watch, int Qty, string Side)
        {
            if (Side == "BUY")
            {
                for (int i = 0; i < Qty; i++)
                {
                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                    snd.TransCode = 10;
                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    snd.UniqueID = unique;
                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                    snd.gui_id = AppGlobal.GUI_ID;
                    snd.isWind = false;
                    snd.Open = 0;

                    long seq = ClassDisruptor.ringBufferRequest.Next();
                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                    ClassDisruptor.ringBufferRequest.Publish(seq);

                    Application.DoEvents();
                    System.Threading.Thread.Sleep(50);
                }
            }
            else if (Side == "SELL")
            {
                for (int i = 0; i < Qty; i++)
                {
                    BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                    snd.TransCode = 10;
                    UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                    snd.UniqueID = unique;
                    snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                    snd.gui_id = AppGlobal.GUI_ID;
                    snd.isWind = true;
                    snd.Open = 0;

                    long seq = ClassDisruptor.ringBufferRequest.Next();
                    ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                    ClassDisruptor.ringBufferRequest.Publish(seq);

                    Application.DoEvents();
                    System.Threading.Thread.Sleep(50);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            #region Strike Request
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.StrategyId == 0)
                return;

            #region Future Exp
            if (watch.StrategyId == 2211 || watch.StrategyId == 888)
            {
                string n11 = Convert.ToString(watch.Expiry);
                string n21 = n11.Substring(0, 2);
                string n31 = n11.Substring(2, 3);
                string n41 = n11.Substring(7, 2);
                string L1Expiry = "20" + n41 + n31 + n21;

                string n12 = Convert.ToString(watch.Expiry2);
                string n22 = n12.Substring(0, 2);
                string n32 = n12.Substring(2, 3);
                string n42 = n12.Substring(7, 2);
                string L3Expiry = "20" + n42 + n32 + n22;

                int mont0 = DateTime.ParseExact(n31, "MMM", new CultureInfo("en-US")).Month;
                int _currentmonth = mont0;
                uint _expiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[0]));
                string _expiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, _expiry).ToString("yyyyMMMdd");
                string _sf12 = Convert.ToString(_expiry1);
                string _sf22 = _sf12.Substring(0, 4);
                string _sf32 = _sf12.Substring(4, 3);
                string _sf42 = _sf12.Substring(7, 2);
                int _montf = DateTime.ParseExact(_sf32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo _mffi1 = new System.Globalization.DateTimeFormatInfo();
                string _monStringf = "";
                if (_montf <= 9)
                {
                    _monStringf = "0" + Convert.ToString(_montf);
                }
                else
                {
                    _monStringf = Convert.ToString(_montf);
                }
                string _sf52 = _sf22 + _sf32 + _sf42;
                string _selectFut = _sf52;
                uint _nxtexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[1]));
                string _nxtexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, _nxtexpiry).ToString("yyyyMMMdd");
                string _nxtsf12 = Convert.ToString(_nxtexpiry1);
                string _nxtsf22 = _nxtsf12.Substring(0, 4);
                string _nxtsf32 = _nxtsf12.Substring(4, 3);
                string _nxtsf42 = _nxtsf12.Substring(7, 2);
                int _nxtmontf = DateTime.ParseExact(_nxtsf32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo _nxtmffi1 = new System.Globalization.DateTimeFormatInfo();
                string _nxtmonStringf = "";
                if (_nxtmontf <= 9)
                {
                    _nxtmonStringf = "0" + Convert.ToString(_nxtmontf);
                }
                else
                {
                    _nxtmonStringf = Convert.ToString(_nxtmontf);
                }
                string _nxtsf52 = _nxtsf22 + _nxtsf32 + _nxtsf42;
                if (_currentmonth == _nxtmontf)
                    _selectFut = _nxtsf52;
                uint _farexpiry = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(threeExpiry[2]));
                string _farexpiry1 = ArisApi_a._arisApi.SecondToDateTime(Market.NseCm, _farexpiry).ToString("yyyyMMMdd");
                string _farsf12 = Convert.ToString(_farexpiry1);
                string _farsf22 = _farsf12.Substring(0, 4);
                string _farsf32 = _farsf12.Substring(4, 3);
                string _farsf42 = _farsf12.Substring(7, 2);
                int _farmontf = DateTime.ParseExact(_farsf32, "MMM", new CultureInfo("en-US")).Month;
                System.Globalization.DateTimeFormatInfo _farmffi1 = new System.Globalization.DateTimeFormatInfo();
                string _farmonStringf = "";
                if (_farmontf <= 9)
                {
                    _farmonStringf = "0" + Convert.ToString(_farmontf);
                }
                else
                {
                    _farmonStringf = Convert.ToString(_farmontf);
                }
                string _farsf52 = _farsf22 + _farsf32 + _farsf42;
                if (_currentmonth == _farmontf)
                    _selectFut = _farsf52;



                string Sym = Convert.ToString(watch.Leg1.ContractInfo.Symbol);


                if (Sym == "BANKNIFTY" && ((watch.Leg1.ContractInfo.Series == "CE" && watch.Leg2.ContractInfo.Series == "PE") || (watch.Leg1.ContractInfo.Series == "PE" && watch.Leg2.ContractInfo.Series == "CE")))
                {
                    if (L1Expiry != _selectFut)
                    {
                        MessageBox.Show("BankNifty not allowed weekly rule");
                        return;
                    }

                    if (L3Expiry != _selectFut)
                    {
                        MessageBox.Show("BankNifty not allowed weekly rule");
                        return;
                    }
                }
            }

            #endregion

            string segment1 = "";
            string segment2 = "";
            string segment3 = "";
            string segment4 = "";

            string LotSize1 = "";
            string LotSize2 = "";
            string LotSize3 = "";
            string LotSize4 = "";

            dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
            watch.IsStrikeReq = true;

            if (watch.StrategyId == 111 || watch.StrategyId == 211)
            {

                string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                foreach (DataRow dr in dr11)
                {
                    segment1 = dr["Segment"].ToString();
                    LotSize1 = dr["LotSize"].ToString();
                }

                string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                foreach (DataRow dr in dr12)
                {
                    segment2 = dr["Segment"].ToString();
                    LotSize2 = dr["LotSize"].ToString();
                }
                if (segment1 != segment2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2);
                    return;
                }
                if (LotSize1 != LotSize2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2);
                    return;
                }
                dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                watch.IsStrikeReq = true;

                TokenRequest11_12(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                    + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
            }
            else if (watch.StrategyId == 311)
            {

                string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                foreach (DataRow dr in dr11)
                {
                    segment1 = dr["Segment"].ToString();
                    LotSize1 = dr["LotSize"].ToString();
                }
                string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                foreach (DataRow dr in dr12)
                {
                    segment2 = dr["Segment"].ToString();
                    LotSize2 = dr["LotSize"].ToString();
                }
                if (segment1 != segment2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2);
                    return;
                }
                if (LotSize1 != LotSize2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2);
                    return;
                }
                dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                watch.IsStrikeReq = true;

                TokenRequest311(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio);
                TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                    + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
            }
            else if (watch.StrategyId == 91)
            {
                dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                watch.IsStrikeReq = true;
                TokenRequest91(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                TransactionWatch.ErrorMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID
                                                    + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) +
                                                    "|Series|" + watch.Leg1.ContractInfo.Series);
            }
            else if (watch.StrategyId == 2211)
            {
                string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                foreach (DataRow dr in dr11)
                {
                    segment1 = dr["Segment"].ToString();
                    LotSize1 = dr["LotSize"].ToString();
                }
                string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                foreach (DataRow dr in dr12)
                {
                    segment2 = dr["Segment"].ToString();
                    LotSize2 = dr["LotSize"].ToString();
                }
                if (segment1 != segment2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2);
                    return;
                }
                if (LotSize1 != LotSize2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2);
                    return;
                }
                dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                watch.IsStrikeReq = true;
                TokenRequest2211(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio);
                TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                    + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
            }
            else if (watch.StrategyId == 888)
            {


                string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                foreach (DataRow dr in dr11)
                {
                    segment1 = dr["Segment"].ToString();
                    LotSize1 = dr["LotSize"].ToString();
                }

                string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                foreach (DataRow dr in dr12)
                {
                    segment2 = dr["Segment"].ToString();
                    LotSize2 = dr["LotSize"].ToString();
                }

                string str3 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg3.ContractInfo.TokenNo) + "'";
                DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str3);
                foreach (DataRow dr in dr13)
                {
                    segment3 = dr["Segment"].ToString();
                    LotSize3 = dr["LotSize"].ToString();
                }
                if (segment1 != segment2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2 +
                                    "\n Segment 3: " + segment3);
                    return;
                }
                else if (segment2 != segment3)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2 +
                                    "\n Segment 3: " + segment3);
                    return;
                }
                if (LotSize1 != LotSize2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3);
                    return;
                }
                else if (LotSize2 != LotSize3)
                {

                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3);
                    return;
                }
                TokenRequest888(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio);
                TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                    + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
            }
            else if (watch.StrategyId == 121)
            {
                string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                foreach (DataRow dr in dr11)
                {
                    segment1 = dr["Segment"].ToString();
                    LotSize1 = dr["LotSize"].ToString();
                }

                string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                foreach (DataRow dr in dr12)
                {
                    segment2 = dr["Segment"].ToString();
                    LotSize2 = dr["LotSize"].ToString();
                }

                string str3 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg3.ContractInfo.TokenNo) + "'";
                DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str3);
                foreach (DataRow dr in dr13)
                {
                    segment3 = dr["Segment"].ToString();
                    LotSize3 = dr["LotSize"].ToString();
                }
                if (segment1 != segment2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2 +
                                    "\n Segment 3: " + segment3);
                    return;
                }
                else if (segment2 != segment3)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2 +
                                    "\n Segment 3: " + segment3);
                    return;
                }
                if (LotSize1 != LotSize2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3);
                    return;
                }
                else if (LotSize2 != LotSize3)
                {

                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3);
                    return;
                }

                if (Convert.ToString(watch.Leg1.ContractInfo.Series) == "CE")
                    TokenRequest7121(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio, true);
                else
                    TokenRequest7121(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio, false);
                TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                    + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series);
            }
            else if (watch.StrategyId == 1331)
            {
                string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                foreach (DataRow dr in dr11)
                {
                    segment1 = dr["Segment"].ToString();
                    LotSize1 = dr["LotSize"].ToString();
                }
                string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                foreach (DataRow dr in dr12)
                {
                    segment2 = dr["Segment"].ToString();
                    LotSize2 = dr["LotSize"].ToString();
                }
                string str3 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg3.ContractInfo.TokenNo) + "'";
                DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str3);
                foreach (DataRow dr in dr13)
                {
                    segment3 = dr["Segment"].ToString();
                    LotSize3 = dr["LotSize"].ToString();

                }
                string str4 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg4.ContractInfo.TokenNo) + "'";
                DataRow[] dr14 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str4);
                foreach (DataRow dr in dr14)
                {
                    segment4 = dr["Segment"].ToString();
                    LotSize4 = dr["LotSize"].ToString();
                }
                if (segment1 != segment2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2 +
                                    "\n Segment 3: " + segment3 +
                                    "\n Segment 4: " + segment4);
                    return;
                }
                else if (segment2 != segment3)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2 +
                                    "\n Segment 3: " + segment3 +
                                    "\n Segment 4: " + segment4);
                    return;
                }
                else if (segment3 != segment4)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n Segment 1: " + segment1 +
                                     "\n Segment 2: " + segment2 +
                                     "\n Segment 3: " + segment3 +
                                     "\n Segment 4: " + segment4);
                    return;
                }
                if (LotSize1 != LotSize2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3 +
                                     "\n LotSize 4: " + LotSize4);
                    return;
                }
                else if (LotSize2 != LotSize3)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3 +
                                     "\n LotSize 4: " + LotSize4);
                    return;
                }
                else if (LotSize3 != LotSize4)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3 +
                                     "\n LotSize 4: " + LotSize4);
                    return;

                }
                if (Convert.ToString(watch.Leg1.ContractInfo.Series) == "CE")
                    TokenRequest1331(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg4.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), true);
                else
                    TokenRequest1331(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg4.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), false);
                TransactionWatch.ErrorMessage("NewStrikeReq|" + "UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) + "|Series1|" + Convert.ToString(watch.Leg1.ContractInfo.Series)
                                               + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series2|" + Convert.ToString(watch.Leg2.ContractInfo.Series)
                                               + "|Strike3|" + Convert.ToInt32(watch.Leg3.ContractInfo.StrikePrice) + "|Series3|" + Convert.ToString(watch.Leg3.ContractInfo.Series)
                                               + "|Strike4|" + Convert.ToInt32(watch.Leg4.ContractInfo.StrikePrice) + "|Series4|" + Convert.ToString(watch.Leg4.ContractInfo.Series));

            }
            else if (watch.StrategyId == 1221)
            {
                string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                foreach (DataRow dr in dr11)
                {
                    segment1 = dr["Segment"].ToString();
                    LotSize1 = dr["LotSize"].ToString();
                }

                string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                foreach (DataRow dr in dr12)
                {
                    segment2 = dr["Segment"].ToString();
                    LotSize2 = dr["LotSize"].ToString();
                }

                string str3 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg3.ContractInfo.TokenNo) + "'";
                DataRow[] dr13 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str3);
                foreach (DataRow dr in dr13)
                {
                    segment3 = dr["Segment"].ToString();
                    LotSize3 = dr["LotSize"].ToString();

                }

                string str4 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg4.ContractInfo.TokenNo) + "'";
                DataRow[] dr14 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str4);
                foreach (DataRow dr in dr14)
                {
                    segment4 = dr["Segment"].ToString();
                    LotSize4 = dr["LotSize"].ToString();
                }

                if (segment1 != segment2)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2 +
                                    "\n Segment 3: " + segment3 +
                                    "\n Segment 4: " + segment4);
                    return;
                }
                else if (segment2 != segment3)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                    "\n Segment 1: " + segment1 +
                                    "\n Segment 2: " + segment2 +
                                    "\n Segment 3: " + segment3 +
                                    "\n Segment 4: " + segment4);
                    return;
                }
                else if (segment3 != segment4)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n Segment 1: " + segment1 +
                                     "\n Segment 2: " + segment2 +
                                     "\n Segment 3: " + segment3 +
                                     "\n Segment 4: " + segment4);
                    return;
                }
                if (LotSize1 != LotSize2)
                {

                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3 +
                                     "\n LotSize 4: " + LotSize4);
                    return;
                }
                else if (LotSize2 != LotSize3)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3 +
                                     "\n LotSize 4: " + LotSize4);
                    return;
                }
                else if (LotSize3 != LotSize4)
                {
                    MessageBox.Show("This rule not allowed for Trading" +
                                     "\n LotSize 1: " + LotSize1 +
                                     "\n LotSize 2: " + LotSize2 +
                                     "\n LotSize 3: " + LotSize3 +
                                     "\n LotSize 4: " + LotSize4);
                    return;

                }
                TokenRequest1221(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg3.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg4.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                TransactionWatch.ErrorMessage("NewStrikeReq|" + "UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) + "|Series1|" + Convert.ToString(watch.Leg1.ContractInfo.Series)
                                               + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series2|" + Convert.ToString(watch.Leg2.ContractInfo.Series)
                                               + "|Strike3|" + Convert.ToInt32(watch.Leg3.ContractInfo.StrikePrice) + "|Series3|" + Convert.ToString(watch.Leg3.ContractInfo.Series)
                                               + "|Strike4|" + Convert.ToInt32(watch.Leg4.ContractInfo.StrikePrice) + "|Series4|" + Convert.ToString(watch.Leg4.ContractInfo.Series));
            }
            else if (watch.StrategyId == 12211)
            {

                if (watch.Leg2.ContractInfo.TokenNo != "0")
                {
                    string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                    foreach (DataRow dr in dr11)
                    {
                        segment1 = dr["Segment"].ToString();
                        LotSize1 = dr["LotSize"].ToString();
                    }
                    string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                    DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                    foreach (DataRow dr in dr12)
                    {
                        segment2 = dr["Segment"].ToString();
                        LotSize2 = dr["LotSize"].ToString();
                    }
                    if (LotSize1 != LotSize2)
                    {
                        MessageBox.Show("This rule not allowed for Trading" +
                                         "\n LotSize 1: " + LotSize1 +
                                         "\n LotSize 2: " + LotSize2);
                        return;
                    }
                    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                    watch.IsStrikeReq = true;
                    TokenRequest12211(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio, (int)(watch.UniqueIdLeg1), (int)(watch.UniqueIdLeg2));
                    TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                       + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice) + "|Series|" + watch.Leg1.ContractInfo.Series + "|UniqueLeg1|" + watch.UniqueIdLeg1 + "|UniqueLeg2|" + watch.UniqueIdLeg2);
                }
                else
                {
                    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                    watch.IsStrikeReq = true;
                    TokenRequest912211(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                    TransactionWatch.ErrorMessage("NewStrikeReq|" + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Leg1.expiryUniqueID
                                                    + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice) +
                                                    "|Series|" + watch.Leg1.ContractInfo.Series);
                }
            }
            else if (watch.StrategyId == 1113 || watch.StrategyId == 1114)
            {
                if (watch.Leg2.ContractInfo.TokenNo != "0")
                {
                    string str1 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg1.ContractInfo.TokenNo) + "'";
                    DataRow[] dr11 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str1);
                    foreach (DataRow dr in dr11)
                    {
                        segment1 = dr["Segment"].ToString();
                        LotSize1 = dr["LotSize"].ToString();
                    }
                    string str2 = DBConst.TokenNo + " = '" + Convert.ToString(watch.Leg2.ContractInfo.TokenNo) + "'";
                    DataRow[] dr12 = ArisApi_a._arisApi.DsContract.Tables["NSEFO"].Select(str2);
                    foreach (DataRow dr in dr12)
                    {
                        segment2 = dr["Segment"].ToString();
                        LotSize2 = dr["LotSize"].ToString();
                    }
                    if (LotSize1 != LotSize2)
                    {
                        MessageBox.Show("This rule not allowed for Trading" +
                                         "\n LotSize 1: " + LotSize1 +
                                         "\n LotSize 2: " + LotSize2);
                        return;
                    }
                    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                    watch.IsStrikeReq = true;
                    TokenRequestCalender(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.Leg2.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId), watch.Leg1.Ratio, watch.Leg2.Ratio, (int)(watch.UniqueIdLeg1), (int)(watch.UniqueIdLeg2));
                    TransactionWatch.ErrorMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                       + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                       + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice)
                                                       + "|Series|" + watch.Leg1.ContractInfo.Series + "|UniqueLeg1|" + watch.UniqueIdLeg1 + "|UniqueLeg2|" + watch.UniqueIdLeg2 + "|Strangle");

                    TransactionWatch.TransactionMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                       + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                       + "|Strike2|" + Convert.ToInt32(watch.Leg2.ContractInfo.StrikePrice)
                                                       + "|Series|" + watch.Leg1.ContractInfo.Series + "|UniqueLeg1|" + watch.UniqueIdLeg1 + "|UniqueLeg2|" + watch.UniqueIdLeg2 + "|Strangle", Color.Blue);
                }
                else
                {
                    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.Gray;
                    watch.IsStrikeReq = true;
                    TokenRequestCalender91(watch.uniqueId, watch.Gui_id, Convert.ToInt32(watch.Leg1.ContractInfo.TokenNo), Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo), Convert.ToUInt64(watch.StrategyId));
                    TransactionWatch.ErrorMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                    + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                    + "|Series|" + watch.Leg1.ContractInfo.Series + "|StrangleSingleLeg");
                    TransactionWatch.TransactionMessage("NewStrikeReq|" + watch.StrategyName + "|UniqueId|" + watch.uniqueId + "|Strategy_id|" + Convert.ToUInt64(watch.StrategyId) + "|Gui_id|" + watch.Gui_id + "|Expiry|" + watch.Expiry
                                                    + "|Strike1|" + Convert.ToInt32(watch.Leg1.ContractInfo.StrikePrice)
                                                    + "|Series|" + watch.Leg1.ContractInfo.Series + "|StrangleSingleLeg", Color.Blue);
                }

            }


            #endregion
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DataGridView dgv = dgvMarketWatch;
            try
            {
                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                string strategy = Convert.ToString(dgvMarketWatch.Rows[rowIndex].Cells[WatchConst.Strategy].Value);
                string empty = Convert.ToString(dgvMarketWatch.Rows[rowIndex].Cells[WatchConst.StrategyId].Value);
                string PrvStrategy = Convert.ToString(dgvMarketWatch.Rows[rowIndex - 1].Cells[WatchConst.Strategy].Value);
                string UniqueId = Convert.ToString(dgvMarketWatch.Rows[rowIndex].Cells[WatchConst.Rule].Value);
                string PrvUniqueId = Convert.ToString(dgvMarketWatch.Rows[rowIndex - 1].Cells[WatchConst.Rule].Value);
                if (empty == "0" && strategy.Contains("Strategy"))
                    return;
                if (rowIndex == 0)
                    return;
                // get index of the column for the selected cell
                int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                DataGridViewRow selectedRow = dgv.Rows[rowIndex];
                dgv.Rows.Remove(selectedRow);
                dgv.Rows.Insert(rowIndex - 1, selectedRow);
                dgv.ClearSelection();
                dgv.Rows[rowIndex - 1].Cells[colIndex].Selected = true;

                for (int i = 0; i < dgvMarketWatch.Rows.Count - 1; i++)
                {
                    string rule = Convert.ToString(dgvMarketWatch.Rows[i].Cells[WatchConst.Rule].Value);
                    foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Ruleno) == rule)))
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watch);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watch);
                        break;
                    }
                }
                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                if (empty != "0")
                {
                    int RIndex = dgvMarketWatch.CurrentRow.Index;
                    MarketWatch watch_Prv = new MarketWatch();
                    watch_Prv = AppGlobal.MarketWatch[RIndex - 1];
                    string strategyTemp = watch_Prv.Strategy;

                    MarketWatch watch_new = new MarketWatch();
                    watch_new = AppGlobal.MarketWatch[RIndex];
                    List<string> Unique = new List<string>();
                    for (int k = 0; k < AppGlobal.MarketWatch.Count; k++)
                    {
                        MarketWatch _watch = new MarketWatch();
                        _watch = AppGlobal.MarketWatch[k];
                        if (watch_new.Leg1.ContractInfo.TokenNo == _watch.Leg1.ContractInfo.TokenNo
                            && watch_new.Leg2.ContractInfo.TokenNo == _watch.Leg2.ContractInfo.TokenNo
                            && watch_new.Leg3.ContractInfo.TokenNo == _watch.Leg3.ContractInfo.TokenNo
                            && watch_new.Leg4.ContractInfo.TokenNo == _watch.Leg4.ContractInfo.TokenNo
                            && watch_new.Strategy != _watch.Strategy)
                        {
                            Unique.Add((_watch.Strategy));
                        }

                    }
                    if (RIndex != 0)
                    {
                        foreach (var str in Unique)
                        {                                                        
                                watch_new.Strategy = watch_Prv.Strategy;
                                watch_new.RowData.Cells[WatchConst.Strategy].Value = watch_new.Strategy;
                                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);                            
                        }
                        if (Unique.Count() == 0)
                        {
                            watch_new.Strategy = watch_Prv.Strategy;
                            watch_new.RowData.Cells[WatchConst.Strategy].Value = watch_new.Strategy;
                            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                        }
                    }
                }
            }
            catch { }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DataGridView dgv = dgvMarketWatch;
            try
            {
                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                string strategy = Convert.ToString(dgvMarketWatch.Rows[rowIndex].Cells[WatchConst.Strategy].Value);
                string empty = Convert.ToString(dgvMarketWatch.Rows[rowIndex].Cells[WatchConst.StrategyId].Value);
                if (empty == "0" && strategy.Contains("Strategy"))
                    return;
                if (rowIndex == totalRows - 1)
                    return;
                // get index of the column for the selected cell
                int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                DataGridViewRow selectedRow = dgv.Rows[rowIndex];
                dgv.Rows.Remove(selectedRow);
                dgv.Rows.Insert(rowIndex + 1, selectedRow);
                dgv.ClearSelection();
                dgv.Rows[rowIndex + 1].Cells[colIndex].Selected = true;
                for (int i = 0; i < dgvMarketWatch.Rows.Count - 1; i++)
                {
                    string rule = Convert.ToString(dgvMarketWatch.Rows[i].Cells[WatchConst.Rule].Value);
                    foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Ruleno) == rule)))
                    {
                        int k = AppGlobal.MarketWatch.IndexOf(watch);
                        AppGlobal.MarketWatch.RemoveAt(k);
                        AppGlobal.MarketWatch.Insert(i, watch);
                        break;
                    }
                }
                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                if (empty != "0")
                {
                    int RIndex = dgvMarketWatch.CurrentRow.Index;
                    MarketWatch watch_Prv = new MarketWatch();
                    watch_Prv = AppGlobal.MarketWatch[RIndex - 1];

                    MarketWatch watch_new = new MarketWatch();
                    watch_new = AppGlobal.MarketWatch[RIndex];
                    List<string> Unique = new List<string>();
                    for (int k = 0; k < AppGlobal.MarketWatch.Count; k++)
                    {
                        MarketWatch _watch = new MarketWatch();
                        _watch = AppGlobal.MarketWatch[k];
                        if (watch_new.Leg1.ContractInfo.TokenNo == _watch.Leg1.ContractInfo.TokenNo
                            && watch_new.Leg2.ContractInfo.TokenNo == _watch.Leg2.ContractInfo.TokenNo
                            && watch_new.Leg3.ContractInfo.TokenNo == _watch.Leg3.ContractInfo.TokenNo
                            && watch_new.Leg4.ContractInfo.TokenNo == _watch.Leg4.ContractInfo.TokenNo
                            && watch_new.Strategy != _watch.Strategy)
                        {
                            Unique.Add((_watch.Strategy));
                        }
                    }
                    if (RIndex != 0)
                    {
                        foreach (var str in Unique)
                        {
                            //if (str != watch_Prv.Strategy)
                            {
                                watch_new.Strategy = watch_Prv.Strategy;
                                watch_new.RowData.Cells[WatchConst.Strategy].Value = watch_new.Strategy;
                                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                            }
                        }
                        if (Unique.Count() == 0)
                        {
                            watch_new.Strategy = watch_Prv.Strategy;
                            watch_new.RowData.Cells[WatchConst.Strategy].Value = watch_new.Strategy;
                            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                        }
                    }
                }
            }
            catch { }
            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (AppGlobal.MarketWatch.Count() == 0)
            {
                MessageBox.Show("Add Strategy First!!!");
                return;
            }
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch _watch = new MarketWatch();
            _watch = AppGlobal.MarketWatch[iRow];
            AppGlobal.SelectedStrategy = _watch.Strategy;
            if (cmbStrategyName.Text == "Single")
            {
                if (AppGlobal.__singleLeg == null)
                {
                    AppGlobal.__singleLeg = new SingleLeg();
                    AppGlobal.__singleLeg.Show();
                }
                else
                {
                    AppGlobal.__singleLeg.Show();
                    AppGlobal.__singleLeg.Activate();
                }
            }
            else if (cmbStrategyName.Text == "Strangle")
            {
                if (AppGlobal._Strangle == null)
                {
                    AppGlobal._Strangle = new Stragle();
                    AppGlobal._Strangle.Show();
                }
                else
                {
                    AppGlobal._Strangle.Show();
                    AppGlobal._Strangle.Activate();
                }
            }
            else if (cmbStrategyName.Text == "Straddle")
            {
                if (AppGlobal._Stradder == null)
                {
                    AppGlobal._Stradder = new Stradder();
                    AppGlobal._Stradder.Show();
                }
                else
                {
                    AppGlobal._Stradder.Show();
                    AppGlobal._Stradder.Activate();
                }
            }
            else if (cmbStrategyName.Text == "MainStraddle")
            {
                if (AppGlobal._MainStraddle == null)
                {
                    AppGlobal._MainStraddle = new MainStraddle();
                    AppGlobal._MainStraddle.Show();
                }
                else
                {
                    AppGlobal._MainStraddle.Show();
                    AppGlobal._MainStraddle.Activate();
                }
            }
            else if (cmbStrategyName.Text == "TLI_Strangle")
            {
                if (AppGlobal._TLI_Strangle == null)
                {
                    AppGlobal._TLI_Strangle = new TLI_Strangle();
                    AppGlobal._TLI_Strangle.Show();
                }
                else
                {
                    AppGlobal._TLI_Strangle.Show();
                    AppGlobal._TLI_Strangle.Activate();
                }
            }
            else if (cmbStrategyName.Text == "TLI_CE_Calender")
            {
                if (AppGlobal._TLI_Calender == null)
                {
                    AppGlobal._TLI_Calender = new TLI_CE_Calender();
                    AppGlobal._TLI_Calender.Show();
                }
                else
                {
                    AppGlobal._TLI_Calender.Show();
                    AppGlobal._TLI_Calender.Activate();
                }
            }
            else if (cmbStrategyName.Text == "TLI_PE_Calender")
            {
                if (AppGlobal._TLI_PE_Calender == null)
                {
                    AppGlobal._TLI_PE_Calender = new TLI_PE_Calender();
                    AppGlobal._TLI_PE_Calender.Show();
                }
                else
                {
                    AppGlobal._TLI_PE_Calender.Show();
                    AppGlobal._TLI_PE_Calender.Activate();
                }
            }
            else if (cmbStrategyName.Text == "LSL_Strangle")
            {
                if (AppGlobal._LSL_Strangle == null)
                {
                    AppGlobal._LSL_Strangle = new LSL_Strangle();
                    AppGlobal._LSL_Strangle.Show();
                }
                else
                {
                    AppGlobal._LSL_Strangle.Show();
                    AppGlobal._LSL_Strangle.Activate();
                }
            }
            else if (cmbStrategyName.Text == "Empty")
            {

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                string rulename = Convert.ToString(selectindex);
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.StrategyId = 0;
                watch.StrategyName = "Empty";
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.IsStrikeReq = false;

                #region Row 1

                #region Leg1
                watch.Leg1 = new Straddle.AppClasses.Leg();
                watch.Leg1.ContractInfo.TokenNo = "0";
                watch.Leg1.Counter = 0;
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

                #region Unique ID

                watch.uniqueId = 0;
                watch.displayUniqueId = "0";
                watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                #endregion

                #region FutLeg
                watch.niftyLeg = new Straddle.AppClasses.Leg();
                watch.niftyLeg.ContractInfo.TokenNo = "0";
                watch.niftyLeg.Counter = 0;


                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.LightSalmon;
                AppGlobal.RuleIndexNo++;
                #endregion

                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            }
        }

        public void Insert_Empty(string Strategy)
        {

            MarketWatch watch = new MarketWatch();
            int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
            watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
            string rulename = Convert.ToString(selectindex);
            watch.Ruleno = AppGlobal.RuleIndexNo;
            watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
            watch.StrategyId = 0;
            watch.Strategy = Strategy;
            watch.StrategyName = "Empty";
            watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
            watch.Gui_id = AppGlobal.GUI_ID;
            watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
            watch.RowData.Cells[WatchConst.Strategy].Value = watch.Strategy;

            watch.IsStrikeReq = false;

            if (!AppGlobal.RuleMap.ContainsKey(watch.Strategy))
                AppGlobal.RuleMap.Add(watch.Strategy, new AllDetailsStrategy());

            #region Row 1

            #region Leg1
            watch.Leg1 = new Straddle.AppClasses.Leg();
            watch.Leg1.ContractInfo.TokenNo = "0";
            watch.Leg1.Counter = 0;
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

            #region Unique ID

            watch.uniqueId = 0;
            watch.displayUniqueId = "0";
            watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
            #endregion

            #region FutLeg
            watch.niftyLeg = new Straddle.AppClasses.Leg();
            watch.niftyLeg.ContractInfo.TokenNo = "0";
            watch.niftyLeg.Counter = 0;


            #endregion





            DataGridViewButtonCell ToggleButton = new DataGridViewButtonCell();
            if (watch.Checked)
            {
                ToggleButton.Value = "ON";
                ToggleButton.Style.ForeColor = Color.Green;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;

            }
            else
            {
                ToggleButton.Value = "OFF";
                ToggleButton.Style.ForeColor = Color.Red;
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;
            }
            ToggleButton.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvMarketWatch.Rows[selectindex].Cells[WatchConst.Checked] = ToggleButton;


            if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
            {
                AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
            }
            else
                AppGlobal.MarketWatch.RemoveAt(selectindex);
            AppGlobal.MarketWatch.Insert(selectindex, watch);
            AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.LightSalmon;

            AppGlobal.RuleIndexNo++;
            #endregion

            MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
        }

        private void mtDataGridView1_DoubleClick(object sender, EventArgs e)
        {
            if (AppGlobal._NetMax_Min == null)
            {
                AppGlobal._NetMax_Min = new NetPositionMin_Max();
                AppGlobal._NetMax_Min.Show();
            }
            else
            {
                AppGlobal._NetMax_Min.Show();
                AppGlobal._NetMax_Min.Activate();
            }
        }

        private void dgvMarketWatch_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvMarketWatch.Columns[WatchConst.Checked].Index)
            {
                #region Enter

                int row = e.RowIndex;
                if (row < 0)
                    return;

                DataGridViewCell cell = dgvMarketWatch.Rows[row].Cells[WatchConst.Checked]; //Column Index for the dataGridViewButtonColumn
                if (cell.Value == "OFF")
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();


                    watch = AppGlobal.MarketWatch[iRow];
                    if (watch.StrategyId == 0)
                    {
                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch.Strategy) && (x.StrategyId != 0)))
                        {
                            int i = watch1.RowData.Index;
                            DataGridViewCell _cell = dgvMarketWatch.Rows[i].Cells[WatchConst.Checked];
                            watch1.Checked = true;

                            _cell.Value = "ON";
                            _cell.Style.ForeColor = Color.Green;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;
                        }
                        //watch.Checked = true;
                        //cell.Value = "ON";
                        //cell.Style.ForeColor = Color.Green;
                        //AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;
                        //return;
                    }
                    watch.Checked = true;

                    cell.Value = "ON";
                    cell.Style.ForeColor = Color.Green;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;

                }
                else
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();

                    watch = AppGlobal.MarketWatch[iRow];
                    if (watch.StrategyId == 0)
                    {
                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch.Strategy) && (x.StrategyId != 0)))
                        {
                            int i = watch1.RowData.Index;
                            DataGridViewCell _cell = dgvMarketWatch.Rows[i].Cells[WatchConst.Checked];
                            watch1.Checked = false;

                            _cell.Value = "OFF";
                            _cell.Style.ForeColor = Color.Red;
                            AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;
                        }
                        //watch.Checked = false;
                        //cell.Value = "OFF";
                        //cell.Style.ForeColor = Color.Red;
                        //AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;
                        //return;
                    }
                    watch.Checked = false;
                    cell.Value = "OFF";
                    cell.Style.ForeColor = Color.Red;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[iRow].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;

                }


                #endregion
            }
            else if (e.ColumnIndex == dgvMarketWatch.Columns[WatchConst.SL_BuyOrder].Index)
            {
                #region Buy Stop Loss Order
                int row = e.RowIndex;
                if (row < 0)
                    return;
                DataGridViewCell cell = dgvMarketWatch.Rows[row].Cells[WatchConst.SL_BuyOrder]; //Column Index for the dataGridViewButtonColumn
                if (cell.Value == "OFF")
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();
                    watch = AppGlobal.MarketWatch[iRow];
                    if (watch.StrategyId != 0)
                    {
                        if (watch.StrategyId == 91)
                        {
                            if (watch.IsStrikeReq == false)
                            {
                                MessageBox.Show("Please Strike Request first...");
                                return;
                            }
                            if (watch.TGBuyPrice != 999999 || watch.AP_BuySL != 999999)
                            {
                                if (watch.TGBuyPrice >= watch.AP_BuySL)
                                {
                                    TransactionWatch.ErrorMessage("BuyStopLossOrder|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                  watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.TGBuyPrice + "|" + watch.AP_BuySL);
                                    MessageBox.Show("Trigger Price Should be greater than Actual Price");

                                    return;
                                }
                                else
                                {
                                    cell.Value = "ON";
                                    cell.Style.ForeColor = Color.Green;
                                    watch.SL_BuyOrderflg = true;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please check Stop Loss order");
                                return;
                            }
                        }
                    }
                }
                else if (cell.Value == "ON")
                {

                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();
                    watch = AppGlobal.MarketWatch[iRow];

                    cell.Value = "OFF";
                    cell.Style.ForeColor = Color.Red;
                    watch.SL_BuyOrderflg = false;
                }
                #endregion
            }
            else if (e.ColumnIndex == dgvMarketWatch.Columns[WatchConst.SL_SellOrder].Index)
            {
                #region Sell Stop Loss Order
                int row = e.RowIndex;
                if (row < 0)
                    return;
                DataGridViewCell cell = dgvMarketWatch.Rows[row].Cells[WatchConst.SL_SellOrder]; //Column Index for the dataGridViewButtonColumn
                if (cell.Value == "OFF")
                {
                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();
                    watch = AppGlobal.MarketWatch[iRow];
                    if (watch.StrategyId != 0)
                    {
                        if (watch.StrategyId == 91)
                        {
                            if (watch.IsStrikeReq == false)
                            {
                                MessageBox.Show("Please Strike Request first...");
                                return;
                            }
                            if (watch.TGSellPrice != 999999 || watch.AP_SellSL != 999999)
                            {
                                if (watch.TGSellPrice <= watch.AP_SellSL)
                                {
                                    TransactionWatch.ErrorMessage("SellStopLossOrder|" + watch.uniqueId + "|" + watch.Leg1.ContractInfo.Symbol + "|" + watch.Leg1.ContractInfo.StrikePrice + "|" +
                                                                  watch.Expiry + "|" + watch.Leg1.ContractInfo.Series + "|" + watch.TGSellPrice + "|" + watch.AP_SellSL);
                                    MessageBox.Show("Trigger Price Should be less than Actual Price");

                                    return;
                                }
                                else
                                {
                                    cell.Value = "ON";
                                    cell.Style.ForeColor = Color.Green;
                                    watch.SL_SellOrderflg = true;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please check Stop Loss order");
                                return;
                            }
                        }
                    }
                }
                else if (cell.Value == "ON")
                {

                    int iRow = dgvMarketWatch.CurrentCell.RowIndex;
                    MarketWatch watch = new MarketWatch();
                    watch = AppGlobal.MarketWatch[iRow];

                    cell.Value = "OFF";
                    cell.Style.ForeColor = Color.Red;
                    watch.SL_BuyOrderflg = false;
                }
                #endregion
            }
        }

        private void dgvMarketWatch_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (AppGlobal.GotEnterFromEditing == true)
            {
                AppGlobal.GotEnterFromEditing = false;
                AppGlobal.GotKeyDownFromEditing = false;
                AppGlobal.GotTabFromEditing = false;

                #region Enter
                int iColumn = dgvMarketWatch.CurrentCell.ColumnIndex;
                int iRow = dgvMarketWatch.CurrentCell.RowIndex;

                MarketWatch watch = new MarketWatch();
                watch = AppGlobal.MarketWatch[iRow];
                if (watch.StrategyId == 0)
                {
                    watch.StrategyName = Convert.ToString(watch.RowData.Cells[WatchConst.StrategyName].Value);
                }
                else
                {
                    watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                }
                if (watch.StrategyId == 0)
                    return;
                if (watch.IsStrikeReq == false)
                {
                    MessageBox.Show("Please Strike Request first...");
                    return;
                }
                watch.Over = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                watch.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                if (AppGlobal.EnterLots < watch.Over)
                {
                    MessageBox.Show("Enter Wind Qty is more than Max Qty Limit | Max Qty Limit is " + Convert.ToString(AppGlobal.EnterLots));
                    return;
                }
                if (AppGlobal.EnterLots < watch.Round)
                {
                    MessageBox.Show("Enter Unwind Qty is more than Max Qty Limit | Max Qty Limit is " + Convert.ToString(AppGlobal.EnterLots));
                    return;
                }

                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                snd.TransCode = 1;
                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                snd.UniqueID = unique;
                snd.Wind = Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) * 100;
                snd.Unwind = Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) * 100;
                snd.Open = Convert.ToInt32(watch.RowData.Cells[WatchConst.FQty].Value);
                snd.Round = Convert.ToInt32(watch.RowData.Cells[WatchConst.RQty].Value);
                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                snd.Token = Convert.ToInt32(watch.niftyLeg.ContractInfo.TokenNo);
                snd.gui_id = watch.Gui_id;
                watch.Wind = Convert.ToDecimal(watch.RowData.Cells[WatchConst.Wind].Value);
                watch.unWind = Convert.ToDecimal(watch.RowData.Cells[WatchConst.UnWind].Value);
                if (watch.StrategyId == 91 || watch.StrategyId == 12211)
                {
                    if (watch.Leg2.ContractInfo.TokenNo == "0")
                    {
                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) > 0)
                        {

                            double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) * 2;
                            if (Fspread != 0)
                            {
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) > Fspread)
                                {
                                    MessageBox.Show("Please Check Wind Spread!!!!");
                                    return;
                                }
                            }
                        }
                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) > 0)
                        {

                            double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) / 2;
                            if (Rspread != 0)
                            {
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                                {
                                    MessageBox.Show("Please Check Unwind Spread!!!!");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) < 0)
                        {
                            double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) * 2;
                            if (Fspread != 0)
                            {
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) < Fspread)
                                {
                                    MessageBox.Show("Please Check Wind Spread!!!!");
                                    return;
                                }
                            }
                        }
                        else if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) > 0)
                        {

                            double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) / 2;
                            if (Fspread != 0)
                            {
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) < Fspread)
                                {
                                    MessageBox.Show("Please Check Wind Spread!!!!");
                                    return;
                                }
                            }
                        }
                        if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) < 0)
                        {
                            double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) * 2;
                            if (Rspread != 0)
                            {
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                                {
                                    MessageBox.Show("Please Check Unwind Spread!!!!");
                                    return;
                                }
                            }
                        }
                        else if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) > 0)
                        {
                            double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) / 2;
                            if (Rspread != 0)
                            {
                                if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                                {
                                    MessageBox.Show("Please Check Unwind Spread!!!!");
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) < 0)
                    {
                        double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) * 2;
                        if (Fspread != 0)
                        {
                            if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) < Fspread)
                            {
                                MessageBox.Show("Please Check Wind Spread!!!!");
                                return;
                            }
                        }
                    }
                    else if (Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) > 0)
                    {

                        double Fspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.FSpread].Value) / 2;
                        if (Fspread != 0)
                        {
                            if (Convert.ToDouble(watch.RowData.Cells[WatchConst.Wind].Value) < Fspread)
                            {
                                MessageBox.Show("Please Check Wind Spread!!!!");
                                return;
                            }
                        }
                    }
                    if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) < 0)
                    {
                        double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) * 2;
                        if (Rspread != 0)
                        {
                            if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                            {
                                MessageBox.Show("Please Check Unwind Spread!!!!");
                                return;
                            }
                        }
                    }
                    else if (Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) > 0)
                    {
                        double Rspread = Convert.ToDouble(watch.RowData.Cells[WatchConst.RSpread].Value) / 2;
                        if (Rspread != 0)
                        {
                            if (Convert.ToDouble(watch.RowData.Cells[WatchConst.UnWind].Value) < Rspread)
                            {
                                MessageBox.Show("Please Check Unwind Spread!!!!");
                                return;
                            }
                        }
                    }
                }
                if (((snd.Wind / 100) + (snd.Unwind / 100)) < 0)
                {
                    MessageBox.Show("Please Check Wind and Unwind Parameter!!!!");
                    return;
                }
                TransactionWatch.ErrorMessage("User UniqueId|" + snd.UniqueID + "|wind|" + snd.Wind + "|unwind|" + snd.Unwind + "|Long|" + snd.Open + "|short|" + snd.Round + "|trail_Pts|" + watch.trail_No + "|trail_lots|" + watch.trail_Lots + "|profit|" + watch.trail_Profit + "|BuyStopLoss|" + watch.TGBuyPrice + "|SellStopLoss|" + watch.TGBuyPrice + "|BuyDD|" + watch.DD_TGBuyPrice + "|sellDD|" + watch.DD_TGSellPrice + "|BuyBmDD|" + watch.DD_bm_Buy + "|SellBmDD|" + watch.DD_bm_Sell);
                //if (watch.DD_BuyOrderflg == true || watch.DD_SellOrderflg == true)
                //    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.MediumSeaGreen;
                //else if (watch.SL_BuyOrderflg == true || watch.SL_SellOrderflg == true)
                //    dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.MediumSpringGreen;
                //else


                dgvMarketWatch.Rows[iRow].DefaultCellStyle.BackColor = Color.White;
                
                long seq = ClassDisruptor.ringBufferRequest.Next();
                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                ClassDisruptor.ringBufferRequest.Publish(seq);
                #endregion
            }
            else
            {
                TransactionWatch.ErrorMessage("dgvMarketWatch_CellEndEdit Got end cell edit without enter being true " + AppGlobal.GotEnterFromEditing);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.StrategyId == 0)
                return;
            if (watch.IsStrikeReq == false)
            {
                MessageBox.Show("Please Strike Request first...");
                return;
            }
            BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
            snd.TransCode = 10;
            UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
            snd.UniqueID = unique;
            snd.gui_id = AppGlobal.GUI_ID;
            snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
            snd.isWind = true;
            snd.Open = 0;
            long seq = ClassDisruptor.ringBufferRequest.Next();
            ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
            ClassDisruptor.ringBufferRequest.Publish(seq);

            TransactionWatch.ErrorMessage("ImWind|UniqueId|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|iswind|" + snd.isWind); 

        }

        private void button11_Click(object sender, EventArgs e)
        {
            #region SqUnwind
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.StrategyId == 0)
                return;
            if (watch.IsStrikeReq == false)
            {
                MessageBox.Show("Please Strike Request first...");
                return;
            }
            #endregion

            BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
            snd.TransCode = 10;
            UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
            snd.UniqueID = unique;
            snd.gui_id = AppGlobal.GUI_ID;
            snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
            snd.isWind = false;
            snd.Open = 0;
            //TransactionWatch.ErrorMessage("Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + "True");
            long seq = ClassDisruptor.ringBufferRequest.Next();
            ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
            ClassDisruptor.ringBufferRequest.Publish(seq);

            TransactionWatch.ErrorMessage("ImUnWind|UniqueId|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|iswind|" + snd.isWind); 
        }

        private void button12_Click(object sender, EventArgs e)
        {
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            int iCount = dgvMarketWatch.RowCount - 1;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            if (iRow == dgvMarketWatch.RowCount - 1)
                return;
            if (watch.StrategyId == 0)
                return;
            watch.ProfitFlg = false;
            watch.DrawDownFlg = false;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (AppGlobal.MarketWatch.Count == 0)
                return;
            MarketWatch Watch = new MarketWatch();
            for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
            {
                Watch = AppGlobal.MarketWatch[i];
                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                snd.TransCode = 22;
                snd.UniqueID = Convert.ToUInt64(Watch.Leg1.ContractInfo.TokenNo);
                snd.Wind = Convert.ToDouble(Watch.Delta);
                snd.Unwind = Convert.ToDouble(Watch.Gamma);
                snd.AvgSpread = Convert.ToDouble(Watch.Vega);
                snd.Netting = Convert.ToDouble(Watch.Theta);
                snd.Token = Convert.ToInt32(Watch.niftyLeg.ContractInfo.TokenNo);
                long seq = ClassDisruptor.ringBufferRequest.Next();
                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                ClassDisruptor.ringBufferRequest.Publish(seq);

                TransactionWatch.TransactionMessage("Sym|" + Watch.Leg1.ContractInfo.Symbol + "|Strike|" + Watch.Leg1.ContractInfo.StrikePrice + "|Series|" +
                                                    "Token" + Watch.Leg1.ContractInfo.TokenNo +
                                                    Watch.Leg1.ContractInfo.Series + "|Delta|" + Watch.Delta + "|Vega|" + Watch.Vega + "|Theta|" + Watch.Theta
                                                    + "|Gamma|" + Watch.Gamma, Color.Blue);
                System.Threading.Thread.Sleep(100);
            }
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            #region Square Position

            int iRow = dgvMarketWatch.CurrentCell.RowIndex;

            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.StrategyId == 0)
                return;
            if (watch.IsStrikeReq == false)
            {
                MessageBox.Show("Please Strike Request first...");
                return;
            }

            int pos = 0;
            if (watch.StrategyId == 12211)
                pos = watch.L1PosInt;
            else
                pos = watch.posInt;


            if (pos != 0)
            {
                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                snd.TransCode = 10;
                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));

                snd.UniqueID = unique;
                snd.gui_id = AppGlobal.GUI_ID;
                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                if (pos > 0)
                    snd.isWind = false;
                else
                    snd.isWind = true;
                snd.Open = 0;

                long seq = ClassDisruptor.ringBufferRequest.Next();
                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                ClassDisruptor.ringBufferRequest.Publish(seq);
                TransactionWatch.ErrorMessage("Sqoff|UniqueId|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|iswind|" + snd.isWind); 
            }
            #endregion
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            #region Increase_Position
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            if (watch.StrategyId == 0)
                return;
            if (watch.IsStrikeReq == false)
            {
                MessageBox.Show("Please Strike Request first...");
                return;
            }
            int pos = 0;
            if (watch.StrategyId == 12211)
                pos = watch.L1PosInt;
            else
                pos = watch.posInt;

            if (pos != 0)
            {
                BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
                snd.TransCode = 10;
                UInt64 unique = Convert.ToUInt64(Convert.ToInt64(watch.uniqueId));
                snd.UniqueID = unique;
                snd.gui_id = AppGlobal.GUI_ID;
                snd.StrategyId = Convert.ToUInt64(watch.StrategyId);
                if (pos > 0)
                    snd.isWind = true;
                else
                    snd.isWind = false;
                snd.Open = 0;
                //TransactionWatch.ErrorMessage("Gui_id|" + AppGlobal.GUI_ID + "|StrategyId|" + watch.StrategyId + "|UniqueId|" + watch.uniqueId + "|Wind|" + snd.isWind + "|Immediate|");
                long seq = ClassDisruptor.ringBufferRequest.Next();
                ClassDisruptor.ringBufferRequest[seq].PacketNotification = snd;
                ClassDisruptor.ringBufferRequest.Publish(seq);
                TransactionWatch.ErrorMessage("IncreasePos|UniqueId|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|iswind|" + snd.isWind); 
            }
            #endregion
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string path = ArisApi_a._arisApi.SystemConfig.LogFilePath.ToString() + "Logs" + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string date = path + ArisApi_a._arisApi.SystemConfig.UserName.ToString() + DateTime.Now.ToString("ddMMMyyyy") + "Trade.csv";
            StreamWriter sw = new StreamWriter(date);
            string strHead = "";
            strHead = "Time,Unique,Expiry,Strategy,TradePx,Type,count";
            sw.WriteLine(strHead);
            for (int i = 0; i < _tradeBookTable1.Rows.Count; i++)
            {
                string trdInfo = _tradeBookTable1.Rows[i][TradeConst.Time].ToString() + "," + _tradeBookTable1.Rows[i][TradeConst.uniqueId].ToString() + "," +
                                 _tradeBookTable1.Rows[i][TradeConst.Expiry].ToString() + "," + _tradeBookTable1.Rows[i][TradeConst.StrategyName].ToString() + "," +
                                 _tradeBookTable1.Rows[i][TradeConst.TrdPrice].ToString() + "," + _tradeBookTable1.Rows[i][TradeConst.IsWind].ToString() + "," +
                                 _tradeBookTable1.Rows[i][TradeConst.TrdCount].ToString();
                sw.WriteLine(trdInfo);
            }
            sw.Close();
            TransactionWatch.ErrorMessage("TradeFileSave"); 
        }

        private void button14_Click(object sender, EventArgs e)
        {
            SendBackFile();
            SendBackFile2();
        }

        public void SendBackFile()
        {
            string ip = ArisApi_a._arisApi.SystemConfig.Gateway;
            string path = ArisApi_a._arisApi.SystemConfig.LogFilePath.ToString() + "Logs" + "\\";
            string Pos_path = ArisApi_a._arisApi.SystemConfig.LogFilePath.ToString() + "Default" + "\\";
            string date1 = DateTime.Now.ToString("ddMMMyyyy") + ".txt";
            string TradeFile = path + "TradeLog" + ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "-" + date1;
            string UserName = ArisApi_a._arisApi.SystemConfig.UserName.ToString();
            string v = @"\\";
            string remotepath = v + ArisApi_a._arisApi.SystemConfig.BackUpPath_1 + @"\" + ArisApi_a._arisApi.SystemConfig.BackUpFilePath + @"\Default\";
            string TraderRemotePath = v + ArisApi_a._arisApi.SystemConfig.BackUpPath_1 + @"\" + ArisApi_a._arisApi.SystemConfig.BackUpFilePath + @"\Logs\";

            string date = DateTime.Now.ToString("ddMMMyyyy");
            string fileName = AppGlobal.Watch + date + ".tst";
            string NetPos = AppGlobal.netWatch.ToString() + date + ".tst";
            string fileName2 = "TradeLog" + ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "-" + date + ".txt";
            string fileName3 = "ErrorLog" + "-" + date + "*.txt";

            var localContract = Path.Combine(Pos_path, fileName);
            var remoteContract = Path.Combine(remotepath, fileName);
            var NetPoslocalContract = Path.Combine(Pos_path, NetPos);
            var NetPosremoteContract = Path.Combine(remotepath, NetPos);
            var TraderRemoteContract = Path.Combine(TraderRemotePath, fileName2);
            var TradelocalContract = Path.Combine(path, fileName2);
            var ErrorRemoteContract = Path.Combine(TraderRemotePath, fileName3);
            var ErrorlocalContract = Path.Combine(path, fileName3);

            string fileName4 = "OnlyTradeLog" + ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "-" + date + ".txt";
            var OnlyTraderRemoteContract = Path.Combine(TraderRemotePath, fileName2);
            var OnlyTradelocalContract = Path.Combine(path, fileName4);

            string Pnl_Margin = ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "_Pnl_Margin" + ".csv";
            var ProfitLossRemoteContract = Path.Combine(TraderRemotePath, Pnl_Margin);
            var ProfitLosslocalContract = Path.Combine(path, Pnl_Margin);

            string SourceDir = path;
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(ArisApi_a._arisApi.SystemConfig.BackUpPath_1);
            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(remotepath))
                {
                    Directory.CreateDirectory(remotepath);
                }
                if (File.Exists(Path.Combine(remoteContract)))
                {
                    File.Delete(remoteContract);
                    File.Copy(localContract, remoteContract);
                }
                else
                {
                    File.Copy(localContract, remoteContract);
                }
            }
            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(TraderRemotePath))
                {
                    Directory.CreateDirectory(TraderRemotePath);
                }
                if (File.Exists(Path.Combine(TraderRemoteContract)))
                {
                    File.Delete(TraderRemoteContract);
                    File.Copy(TradelocalContract, TraderRemoteContract);
                }
                else
                {
                    File.Copy(TradelocalContract, TraderRemoteContract);
                }
            }
            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(TraderRemotePath))
                {
                    Directory.CreateDirectory(TraderRemotePath);
                }
                if (File.Exists(Path.Combine(OnlyTraderRemoteContract)))
                {
                    File.Delete(OnlyTraderRemoteContract);
                    File.Copy(OnlyTradelocalContract, OnlyTraderRemoteContract);
                }
                else
                {
                    File.Copy(OnlyTradelocalContract, OnlyTraderRemoteContract);
                }
            }

            string TodaysDate = DateTime.Now.ToString("yyyyMMdd");
            string DailyTradeFile = v + ArisApi_a._arisApi.SystemConfig.BackUpPath_1 + @"\" + ArisApi_a._arisApi.SystemConfig.BackUpDailyTradeFilePath + @"\" + TodaysDate + @"\";
            var remoteDailyBackTrade = Path.Combine(DailyTradeFile, fileName2);
            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(DailyTradeFile))
                {
                    Directory.CreateDirectory(DailyTradeFile);
                }
                if (File.Exists(Path.Combine(remoteDailyBackTrade)))
                {
                    File.Delete(remoteDailyBackTrade);
                    File.Copy(TradelocalContract, remoteDailyBackTrade);
                }
                else
                {
                    File.Copy(TradelocalContract, remoteDailyBackTrade);
                }
            }
            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(TraderRemotePath))
                {
                    Directory.CreateDirectory(TraderRemotePath);
                }
                if (File.Exists(Path.Combine(ProfitLossRemoteContract)))
                {
                    File.Delete(ProfitLossRemoteContract);
                    File.Copy(ProfitLosslocalContract, ProfitLossRemoteContract);
                }
                else
                {
                    File.Copy(ProfitLosslocalContract, ProfitLossRemoteContract);
                }
            }
            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(TraderRemotePath))
                {
                    Directory.CreateDirectory(TraderRemotePath);
                }
                string[] picList = Directory.GetFiles(SourceDir, fileName3);

                foreach (string f in picList)
                {
                    string[] specificName = f.Split('\\');
                    string Name = specificName[specificName.Length - 1];
                    string ErrorRemotePath = v + ArisApi_a._arisApi.SystemConfig.BackUpPath_1 + @"\" + ArisApi_a._arisApi.SystemConfig.BackUpFilePath + @"\Logs\";
                    var ErrorRemoteContract1 = Path.Combine(ErrorRemotePath, Name);
                    var ErrorlocalContract1 = Path.Combine(path, Name);
                    if (File.Exists(Path.Combine(ErrorRemoteContract1)))
                    {
                        File.Delete(ErrorRemoteContract1);
                        File.Copy(ErrorlocalContract1, ErrorRemoteContract1);
                    }
                    else
                    {
                        File.Copy(ErrorlocalContract1, ErrorRemoteContract1);
                    }
                }
            }
        }

        public void SendBackFile2()
        {
            string ip = ArisApi_a._arisApi.SystemConfig.Gateway;
            string path = ArisApi_a._arisApi.SystemConfig.LogFilePath.ToString() + "Logs" + "\\";
            string Pos_path = ArisApi_a._arisApi.SystemConfig.LogFilePath.ToString() + "Default" + "\\";
            string date1 = DateTime.Now.ToString("ddMMMyyyy") + ".txt";
            string TradeFile = path + "TradeLog" + ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "-" + date1;
            string UserName = ArisApi_a._arisApi.SystemConfig.UserName.ToString();
            string v = @"\\";
            string remotepath = v + ArisApi_a._arisApi.SystemConfig.BackUpPath_2 + @"\" + ArisApi_a._arisApi.SystemConfig.BackUpFilePath + @"\Default\";
            string TraderRemotePath = v + ArisApi_a._arisApi.SystemConfig.BackUpPath_2 + @"\" + ArisApi_a._arisApi.SystemConfig.BackUpFilePath + @"\Logs\";
            string date = DateTime.Now.ToString("ddMMMyyyy");
            string fileName = AppGlobal.Watch + date + ".tst";
            string NetPos = AppGlobal.netWatch.ToString() + date + ".tst";
            string fileName2 = "TradeLog" + ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "-" + date + ".txt";
            string fileName3 = "ErrorLog" + "-" + date + "*.txt";
            var localContract = Path.Combine(Pos_path, fileName);
            var remoteContract = Path.Combine(remotepath, fileName);
            var NetPoslocalContract = Path.Combine(Pos_path, NetPos);
            var NetPosremoteContract = Path.Combine(remotepath, NetPos);
            var TraderRemoteContract = Path.Combine(TraderRemotePath, fileName2);
            var TradelocalContract = Path.Combine(path, fileName2);
            var ErrorRemoteContract = Path.Combine(TraderRemotePath, fileName3);
            var ErrorlocalContract = Path.Combine(path, fileName3);

            string fileName4 = "OnlyTradeLog" + ArisApi_a._arisApi.SystemConfig.UserName.ToString() + "-" + date + ".txt";
            var OnlyTraderRemoteContract = Path.Combine(TraderRemotePath, fileName2);
            var OnlyTradelocalContract = Path.Combine(path, fileName4);

            string SourceDir = path;
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(ArisApi_a._arisApi.SystemConfig.BackUpPath_2);
            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(remotepath))
                {
                    Directory.CreateDirectory(remotepath);
                }
                if (File.Exists(Path.Combine(remoteContract)))
                {
                    File.Delete(remoteContract);
                    File.Copy(localContract, remoteContract);
                }
                else
                {
                    File.Copy(localContract, remoteContract);
                }
            }
            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(TraderRemotePath))
                {
                    Directory.CreateDirectory(TraderRemotePath);
                }
                if (File.Exists(Path.Combine(TraderRemoteContract)))
                {
                    File.Delete(TraderRemoteContract);
                    File.Copy(TradelocalContract, TraderRemoteContract);
                }
                else
                {
                    File.Copy(TradelocalContract, TraderRemoteContract);
                }
            }

            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(TraderRemotePath))
                {
                    Directory.CreateDirectory(TraderRemotePath);
                }
                if (File.Exists(Path.Combine(OnlyTraderRemoteContract)))
                {
                    File.Delete(OnlyTraderRemoteContract);
                    File.Copy(OnlyTradelocalContract, OnlyTraderRemoteContract);
                }
                else
                {
                    File.Copy(OnlyTradelocalContract, OnlyTraderRemoteContract);
                }
            }

            if (pingReply.Status == IPStatus.Success)
            {
                if (!Directory.Exists(TraderRemotePath))
                {
                    Directory.CreateDirectory(TraderRemotePath);
                }
                string[] picList = Directory.GetFiles(SourceDir, fileName3);
                foreach (string f in picList)
                {
                    string[] specificName = f.Split('\\');
                    string Name = specificName[specificName.Length - 1];
                    string ErrorRemotePath = v + ArisApi_a._arisApi.SystemConfig.BackUpPath_2 + @"\" + ArisApi_a._arisApi.SystemConfig.BackUpFilePath + @"\Logs\";
                    var ErrorRemoteContract1 = Path.Combine(ErrorRemotePath, Name);
                    var ErrorlocalContract1 = Path.Combine(path, Name);
                    if (File.Exists(Path.Combine(ErrorRemoteContract1)))
                    {
                        File.Delete(ErrorRemoteContract1);
                        File.Copy(ErrorlocalContract1, ErrorRemoteContract1);
                    }
                    else
                    {
                        File.Copy(ErrorlocalContract1, ErrorRemoteContract1);
                    }
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            lblLimitHit.Text = "-";
        }

        private void OptionWatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.L)
            {
                if (AppGlobal.__singleLeg == null)
                {
                    AppGlobal.__singleLeg = new SingleLeg();
                    AppGlobal.__singleLeg.Show();
                }
                else
                {
                    AppGlobal.__singleLeg.Show();
                    AppGlobal.__singleLeg.Activate();
                }
            }
        }

        private void Strategy_Click(object sender, EventArgs e)
        {
            for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
            {
                MarketWatch watch = AppGlobal.MarketWatch[index];
                string Strategy_name = Convert.ToString(watch.Strategy);
                if (!_StrategyList.Contains(Strategy_name))
                {
                    _StrategyList.Add(Strategy_name);
                }
            }
            if (_StrategyList.Count() == 0)
            {
                AppGlobal.Global_StrategyName = "Strategy_1";
            }
            else
            {
                List<int> strategyCount = new List<int>();
                foreach (var _strategy in _StrategyList)
                {
                    string[] strategyArray = _strategy.Split('_');
                    int strategy_count = Convert.ToInt32(strategyArray[1]);
                    if (!strategyCount.Contains(strategy_count))
                        strategyCount.Add(strategy_count);
                }
                strategyCount.Sort();
                int count = strategyCount.Max() + 1;
                AppGlobal.Global_StrategyName = "Strategy_" + Convert.ToString(count);
            }
            AppGlobal.frmWatch.Insert_Empty(AppGlobal.Global_StrategyName);
        }

        private void txtOnOff_Click(object sender, EventArgs e)
        {
            if (txtOnOff.Text == "G_ON")
            {
                MarketWatch _watch = new MarketWatch();
                for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
                {
                    _watch = AppGlobal.MarketWatch[i];
                    DataGridViewCell _cell = dgvMarketWatch.Rows[i].Cells[WatchConst.Checked];
                    _watch.Checked = false;
                    _cell.Value = "OFF";
                    _cell.Style.ForeColor = Color.Red;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Black;
                }
                txtOnOff.Text = "G_OFF";
            }
            else
            {
                MarketWatch _watch = new MarketWatch();
                for (int i = 0; i < AppGlobal.MarketWatch.Count; i++)
                {
                    _watch = AppGlobal.MarketWatch[i];
                    DataGridViewCell _cell = dgvMarketWatch.Rows[i].Cells[WatchConst.Checked];
                    _watch.Checked = true;
                    _cell.Value = "ON";
                    _cell.Style.ForeColor = Color.Green;
                    AppGlobal.frmWatch.dgvMarketWatch.Rows[i].Cells[WatchConst.StrategyName].Style.ForeColor = Color.Red;
                }
                txtOnOff.Text = "G_ON";
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            int iRow = dgvMarketWatch.CurrentCell.RowIndex;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            foreach (var _watch in AppGlobal.MarketWatch.Where(x => (Convert.ToInt32(x.StrategyId) == 0) && (Convert.ToString(x.Strategy) == watch.Strategy)))
            {
                _watch.CarryForwardPnl = 0;
                _watch.RowData.Cells[WatchConst.CarryForwardPnl].Value = Math.Round(_watch.CarryForwardPnl, 2);
                TransactionWatch.ErrorMessage("StrategyName|" + _watch.Strategy + "|CarryForwardPnl|" + _watch.CarryForwardPnl);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (AppGlobal._sqoffTimeRule == null)
            {
                AppGlobal._sqoffTimeRule = new SqOffTime_Rule();
                AppGlobal._sqoffTimeRule.Show();
            }
            else
            {
                AppGlobal._sqoffTimeRule.Activate();

            }
        }

        private void lblMargin_Click(object sender, EventArgs e)
        {

        }
    }

    public struct FLASHWINFO
    {
        public UInt32 cbSize;
        public IntPtr hwnd;
        public UInt32 dwFlags;
        public UInt32 uCount;
        public UInt32 dwTimeout;
    }
}
