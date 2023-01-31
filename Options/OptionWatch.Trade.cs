using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Straddle.AppClasses;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;

namespace Straddle
{
    partial class OptionWatch
    {
        public void BindOrderTradeEvents()
        {
            AppGlobal.connection.RMSMessageRecived += new AppGlobal.RMSTerminal_MessageRecivedDel(connection_TradeUpdate);
        }


        void connection_TradeUpdate(Socket socket, byte[] message)
        {
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)(() => connection_TradeUpdate(socket, message)));
            else
            {
                try
                {
                    BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(message);
                    if (packetHeader.TransCode == 99)
                    {
                        if (packetHeader.UniqueID != 0)
                        {
                            threeExpiry = GetExpiryDates(ArisApi_a._arisApi.DsContract.Tables["NSEFO"]);
                            DateTime dt1 = Convert.ToDateTime(threeExpiry[0].ToString());
                            DateTime dt2 = Convert.ToDateTime(threeExpiry[1].ToString());
                            DateTime dt3 = Convert.ToDateTime(threeExpiry[2].ToString());
                            ArisApi_a._arisApi.GenerateTradeFiles();
                            AppGlobal.GUI_ID = packetHeader.UniqueID;
                            txtUniqueid.Text = AppGlobal.GUI_ID.ToString();
                            AppGlobal.MarketWatch = MarketWatch.ReadXmlProfile();
                            AssignMarketStructValue(AppGlobal.MarketWatch);
                            LSL_Strangle_AvgPrice();
                            back_Files();
                            lblMargin.Text = Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3).ToString();
                            lblcallbuy.Text = Math.Round(AppGlobal.CallBuyMTM, 2).ToString();
                            lblcallsell.Text = Math.Round(AppGlobal.CallSellMTM, 2).ToString();
                            lblputbuy.Text = Math.Round(AppGlobal.PutBuyMTM, 2).ToString();
                            lblputsell.Text = Math.Round(AppGlobal.PutSellMTM, 2).ToString();
                            CallMTM.Text = (Math.Round(AppGlobal.CallBuyMTM, 2) + Math.Round(AppGlobal.PutBuyMTM, 2)).ToString();
                            PutMTM.Text = (Math.Round(AppGlobal.CallSellMTM, 2) + Math.Round(AppGlobal.PutSellMTM, 2)).ToString();
                            CrashRMS.Text = "ON";
                            CrashRMS.BackColor = Color.Green;
                            //AppGlobal.connection.MKTMessageRecived += new AppGlobal.MKTTerminal_MessageRecivedDel(connection_MKTMessageRecived);
                            //ArisApi_a._arisApi.OnMarketDepthUpdate += new ArisApi_a.MarketDepthUpdateDelegate(_arisApi_OnMarketDepthUpdate);
                            //ArisApi_a._arisApi.OnIndexBroadCast += new ArisApi_a.IndexBroadCastUpdateDelegate(_arisApi_OnIndexBroadCast);

                            BindBroadcastEvents();

                            SendToTradeAdmin("Connect");
                            RunningPnl.Interval = Convert.ToInt32(ArisApi_a._arisApi.SystemConfig.updateMin * 60000);
                            RunningPnl.Tick += new EventHandler(RunningPnl_Tick);
                            RunningPnl.Start();

                            heartBeatCheck.Interval = Convert.ToInt32(2 * 60000);
                            heartBeatCheck.Tick += new EventHandler(heartBeatCheck_Tick);
                            heartBeatCheck.Start();
                            MatchUniqueNo();
                        }
                        else
                        {
                            TransactionWatch.ErrorMessage("Application Already open with GUI id" + AppGlobal.GUI_ID);
                            AppGlobal.frmWatch.Close();
                        }
                    }
                    else if (packetHeader.TransCode == 17)
                    {
                        if (packetHeader.gui_id == AppGlobal.GUI_ID)
                        {
                            CrashRMS.Text = "OFF";
                            CrashRMS.BackColor = Color.Red;
                            MessageBox.Show("Trading Has been Stopped!!!!");
                        }
                    }
                    else if (packetHeader.TransCode == 20)
                    {
                        MessageBox.Show("User id | " + packetHeader.Token + " Limit Hit");
                    }
                    else if (packetHeader.TransCode == 1 || packetHeader.TransCode == 2 || packetHeader.TransCode == 5)
                    {
                        if (packetHeader.gui_id == AppGlobal.GUI_ID)
                        {
                            if (packetHeader.StrategyId == 91)
                            {
                                #region TransCode 1 for strategy id 91
                                if (packetHeader.TransCode == 1)
                                {
                                    if (packetHeader.StrategyId == 91)
                                    {
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            int i = watch1.RowData.Index;
                                            watch1.Wind = Convert.ToDecimal(packetHeader.Wind / 100);
                                            watch1.unWind = Convert.ToDecimal(packetHeader.Unwind / 100);

                                            watch1.RowData.Cells[WatchConst.Wind].Value = watch1.Wind;
                                            watch1.RowData.Cells[WatchConst.UnWind].Value = watch1.unWind;


                                        }
                                    }
                                }
                                #endregion

                                #region TransCode 2 for strategy id 91
                                if (packetHeader.TransCode == 2)
                                {
                                    if (packetHeader.StrategyId == 91)
                                    {
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            int i = watch1.RowData.Index;
                                            dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.White;
                                        }
                                    }
                                }
                                #endregion

                                #region TransCode 5 for strategy id 91
                                if (packetHeader.TransCode == 5)
                                {
                                    if (packetHeader.StrategyId == 91)
                                    {
                                        //Thread write1 = new Thread(() =>
                                        //{
                                        AllInsertTrade(packetHeader);
                                        //});
                                        //write1.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta
                                        //write1.Start();

                                        #region trade for Watch
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            //Thread write = new Thread(() =>
                                            //{
                                            double PrvMargin = 0;
                                            int i = watch1.RowData.Index;
                                            string side = "";
                                            double mtm = 0;
                                            if (packetHeader.OverNightWindPos >= watch1.Over || packetHeader.OverNightUnWindPos >= watch1.Round)
                                            {

                                            }
                                            else
                                            {
                                                TransactionWatch.ErrorMessage("over night wind : " + packetHeader.OverNightWindPos + " : over night unwind : " + packetHeader.OverNightUnWindPos
                                                                                + " : my open : " + watch1.Over + " : my Round : " + watch1.Round);
                                            }

                                            if (packetHeader.isWind)
                                                dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                                            else
                                                dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;

                                            if (packetHeader.isWind)
                                                side = "Wind";
                                            else
                                                side = "UnWind";
                                            PrvMargin = Convert.ToDouble(watch1.MarginUtilise);
                                            mtm = Convert.ToDouble(watch1.premium);
                                            if (side == "Wind")
                                            {
                                                if (watch1.Leg1.MidPrice != 0)
                                                {
                                                    if (watch1.Leg1.ContractInfo.Series == "XX")
                                                        watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0001 * watch1.Leg1.Ratio);
                                                    else
                                                        watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0007 * watch1.Leg1.Ratio);
                                                }
                                                else
                                                    watch1.UnwindTrnCost = 0;
                                                watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) + watch1.UnwindTrnCost);
                                                watch1.Leg1.B_Qty = watch1.Leg1.B_Qty + watch1.Leg1.ContDetail.LotSize;
                                                watch1.Leg1.B_Value = watch1.Leg1.B_Value + ((packetHeader.TradePrice + watch1.UnwindTrnCost) * watch1.Leg1.ContDetail.LotSize);
                                                watch1.Leg1.Buy_Qty = watch1.Leg1.Buy_Qty + watch1.Leg1.ContDetail.LotSize;
                                            }
                                            else
                                            {
                                                if (watch1.Leg1.MidPrice != 0)
                                                {

                                                    if (watch1.Leg1.ContractInfo.Series == "XX")
                                                        watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0001 * watch1.Leg1.Ratio);
                                                    else
                                                        watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0011 * watch1.Leg1.Ratio);
                                                }
                                                else
                                                    watch1.WindTrnCost = 0;
                                                watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) - watch1.WindTrnCost);
                                                watch1.Leg1.S_Qty = watch1.Leg1.S_Qty + watch1.Leg1.ContDetail.LotSize;
                                                watch1.Leg1.S_Value = watch1.Leg1.S_Value + ((packetHeader.TradePrice - watch1.WindTrnCost) * watch1.Leg1.ContDetail.LotSize);
                                                watch1.Leg1.Sell_Qty = watch1.Leg1.Sell_Qty + watch1.Leg1.ContDetail.LotSize;
                                            }
                                            watch1.Leg1.N_Qty = (watch1.Leg1.B_Qty - watch1.Leg1.S_Qty);
                                            watch1.Leg1.Net_Qty = (watch1.Leg1.Sell_Qty - watch1.Leg1.Buy_Qty);
                                            watch1.Leg1.A_Value = (watch1.Leg1.S_Value - watch1.Leg1.B_Value);

                                            watch1.ProfitFlg = false;
                                            watch1.DrawDownFlg = false;
                                            if (watch1.Leg1.N_Qty != 0)
                                            {
                                                watch1.Leg1.N_Price = Math.Round(watch1.Leg1.A_Value / watch1.Leg1.Net_Qty, 2);
                                                watch1.NetAvgPrice = (watch1.Leg1.S_Value - watch1.Leg1.B_Value) / (watch1.Leg1.Net_Qty);
                                                watch1.RowData.Cells[WatchConst.AvgPrice].Value = Math.Round(watch1.Leg1.N_Price, 2);
                                            }
                                            else
                                            {

                                                watch1.Sqpnl = watch1.Sqpnl + (watch1.Leg1.S_Value - watch1.Leg1.B_Value);
                                                AppGlobal.OverAllPnl = AppGlobal.OverAllPnl + (watch1.Leg1.S_Value - watch1.Leg1.B_Value);
                                                OverAll_pnl.Text = Math.Round(AppGlobal.OverAllPnl, 2).ToString();
                                                watch1.Leg1.N_Price = 0;
                                                watch1.avgPrice = 0;
                                                watch1.NetAvgPrice = 0;
                                                watch1.RowData.Cells[WatchConst.AvgPrice].Value = watch1.Leg1.N_Price;
                                                watch1.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch1.Sqpnl, 2);
                                                #region Strategy Square off Pnl
                                                foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch1.Strategy) &&
                                                                                                                 (Convert.ToString(x.StrategyId) == "0")))
                                                {
                                                    watch.Sqpnl = watch.Sqpnl + watch1.Sqpnl;
                                                    watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
                                                }
                                                #endregion
                                            }
                                            if (watch1.Leg1.N_Qty > 0)
                                            {
                                                watch1.PosType = "Wind";
                                                watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
                                                watch1.posInt = (watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize));
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                            }
                                            else if (watch1.Leg1.N_Qty < 0)
                                            {
                                                watch1.PosType = "UnWind";
                                                watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
                                                watch1.posInt = (watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize));
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                            }
                                            else
                                            {
                                                watch1.pnl = 0;
                                                watch1.RowData.Cells[WatchConst.PNL].Value = watch1.pnl;
                                                watch1.RowData.Cells[WatchConst.PosType].Value = "None";
                                                watch1.posInt = 0;
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                                watch1.avgPrice = 0;
                                                watch1.Leg1.B_Qty = 0;
                                                watch1.Leg1.S_Qty = 0;
                                                watch1.Leg1.B_Value = 0;
                                                watch1.Leg1.S_Value = 0;
                                                watch1.Leg1.N_Qty = 0;
                                                watch1.Leg1.N_Price = 0;
                                                watch1.Leg1.Buy_Qty = 0;
                                                watch1.Leg1.Sell_Qty = 0;
                                                watch1.Leg1.Net_Qty = 0;
                                            }

                                            foreach (var Stranglewatch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.UniqueIdLeg1) == packetHeader.UniqueID) && x.Leg2.ContractInfo.TokenNo != "0"))
                                            {
                                                Stranglewatch.L1PosInt = watch1.posInt;
                                                Stranglewatch.RowData.Cells[WatchConst.LSL_L1PosInt].Value = Stranglewatch.L1PosInt;

                                                Stranglewatch.LSL_AvgPriceCE = watch1.Leg1.N_Price;
                                                Stranglewatch.RowData.Cells[WatchConst.LSL_AvgPriceCE].Value = Stranglewatch.LSL_AvgPriceCE;
                                            }
                                            foreach (var Stranglewatch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.UniqueIdLeg2) == packetHeader.UniqueID) && x.Leg2.ContractInfo.TokenNo != "0"))
                                            {
                                                Stranglewatch.L2PosInt = watch1.posInt;
                                                Stranglewatch.RowData.Cells[WatchConst.LSL_L2PosInt].Value = Stranglewatch.L2PosInt;

                                                Stranglewatch.LSL_AvgPricePE = watch1.Leg1.N_Price;
                                                Stranglewatch.RowData.Cells[WatchConst.LSL_AvgPricePE].Value = Stranglewatch.LSL_AvgPricePE;
                                            }

                                            watch1.premium = watch1.Leg1.N_Price * watch1.posInt * watch1.Leg1.ContDetail.LotSize * -1;
                                            watch1.RowData.Cells[WatchConst.Premium].Value = Math.Round(watch1.premium, 2);
                                            watch1.TradedQty = watch1.Leg1.ContDetail.LotSize * watch1.posInt;
                                            watch1.RowData.Cells[WatchConst.TradedQty].Value = watch1.TradedQty;
                                            if (watch1.Leg1.ContractInfo.Series == "CE")
                                            {
                                                AppGlobal.CallMTM = AppGlobal.CallMTM - mtm + watch1.premium;
                                                if (watch1.posInt > 0)
                                                {
                                                    AppGlobal.CallBuyMTM = AppGlobal.CallBuyMTM - mtm + watch1.premium;
                                                }
                                                else
                                                {
                                                    AppGlobal.CallSellMTM = AppGlobal.CallSellMTM - mtm + watch1.premium;
                                                }
                                            }
                                            else
                                            {
                                                AppGlobal.PutMTM = AppGlobal.PutMTM - mtm + watch1.premium;
                                                if (watch1.posInt > 0)
                                                {
                                                    AppGlobal.PutBuyMTM = AppGlobal.PutBuyMTM - mtm + watch1.premium;
                                                }
                                                else
                                                {
                                                    AppGlobal.PutSellMTM = AppGlobal.PutSellMTM - mtm + watch1.premium;
                                                }
                                            }
                                            AppGlobal.overallPremium = (AppGlobal.overallPremium - mtm) + (watch1.premium);
                                            if (AppGlobal.overallPremium != 0)
                                            {
                                                premiumlbl.Text = Convert.ToString(Math.Round(AppGlobal.overallPremium / 10000000, 3));
                                            }
                                            else
                                            {
                                                premiumlbl.Text = "0";
                                            }

                                            #region Margin Calculate
                                            if (watch1.posInt != 0)
                                            {
                                                if (watch1.posInt < 0)
                                                {
                                                    if (watch1.Leg1.ContractInfo.Symbol == "NIFTY")
                                                        watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.niftyMargin), 2);
                                                    if (watch1.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                                        watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.bankniftyMargin), 2);
                                                    if(watch1.Leg1.ContractInfo.Symbol == "FINNIFTY")
                                                        watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.niftyMargin), 2);
                                                }
                                            }
                                            else if (watch1.posInt == 0)
                                            {
                                                watch1.MarginUtilise = 0;
                                            }
                                            watch1.RowData.Cells[WatchConst.MarginUtilise].Value = watch1.MarginUtilise;
                                            AppGlobal.OverallMarginUtilize = (AppGlobal.OverallMarginUtilize - PrvMargin) + (watch1.MarginUtilise);
                                            if (AppGlobal.OverallMarginUtilize != 0)
                                            {
                                                lblMargin.Text = Convert.ToString(Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3));
                                            }
                                            else
                                            {
                                                lblMargin.Text = "0";
                                            }

                                            TransactionWatch.ErrorMessage("|UniqueId|" + watch1.uniqueId + "|Strategy|" + watch1.StrategyId + "|symbol|" + watch1.Leg1.ContractInfo.Symbol + "|strike|" + watch1.Leg1.ContractInfo.StrikePrice + "|lotsize|" + watch1.Leg1.ContDetail.LotSize + "|NetQty|" +
                                                                           watch1.Leg1.N_Qty + "|NetAvg|" + watch1.Leg1.N_Price + "|SqPnl|" + watch1.Sqpnl + "|CurrPnl|" + watch1.pnl + "|Type|" + side + "|TradePrice|" + packetHeader.TradePrice.ToString() + "|AvgPrice|" + watch1.NetAvgPrice
                                                                           + "|PosInt|" + watch1.posInt + "|BuyValue|" + watch1.Leg1.B_Value + "|SellValue|" + watch1.Leg1.S_Value + "|NetValue|" + watch1.Leg1.A_Value + "|Margin|" + watch1.MarginUtilise);
                                            #endregion

                                            if (side == "Wind")
                                            {
                                                TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                              + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
                                                                              + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + packetHeader.TradePrice.ToString() + "," + "0" + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());

                                                TransactionWatch.OnlyTradeMessage(DateTime.Now.ToString("HH:mm:ss") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.TokenNo + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                               + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
                                                                               + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + packetHeader.TradePrice.ToString() + "," + "0" + "," + "0" + "," + "Wind" + "," + packetHeader.WindPos + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());

                                            }
                                            else
                                            {
                                                TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                              + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
                                                                              + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + packetHeader.TradePrice.ToString() + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());

                                                TransactionWatch.OnlyTradeMessage(DateTime.Now.ToString("HH:mm:ss") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.TokenNo + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                             + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
                                                                             + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + packetHeader.TradePrice.ToString() + "," + "UnWind" + "," + packetHeader.UnWindPos + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());

                                            }
                                            if (AppGlobal.SQAllFlg)
                                            {
                                                if (watch1.posInt == 0)
                                                {
                                                    MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                                                    AppGlobal.SQAllFlg = true;
                                                }
                                            }
                                            else
                                                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                                            //});
                                            //write.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta
                                            //write.Start();
                                        }
                                        #endregion

                                        _Sum();

                                        AppGlobal.Count_single = AppGlobal.Count_single + 1;
                                        AppGlobal.TotalTrade = AppGlobal.TotalTrade + 1;
                                        lblTotalTrade.Text = Convert.ToString(AppGlobal.TotalTrade);
                                        //Thread write3 = new Thread(() =>
                                        //{
                                        //    //if (AppGlobal.SQAllFlg)
                                        //    //{
                                        //        MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                                        //    //}

                                        //});
                                        //write3.SetApartmentState(ApartmentState.STA);//actually no matter sta or mta
                                        //write3.Start();

                                    }
                                }
                                #endregion
                            }
                            else if (packetHeader.StrategyId == 2211)
                            {

                                #region TransCode 1 for strategy id 2211
                                if (packetHeader.TransCode == 1)
                                {
                                    if (packetHeader.StrategyId == 2211)
                                    {
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            int i = watch1.RowData.Index;
                                            watch1.Wind = Convert.ToDecimal(packetHeader.Wind / 100);
                                            watch1.unWind = Convert.ToDecimal(packetHeader.Unwind / 100);

                                            watch1.RowData.Cells[WatchConst.Wind].Value = watch1.Wind;
                                            watch1.RowData.Cells[WatchConst.UnWind].Value = watch1.unWind;


                                        }
                                    }
                                }
                                #endregion

                                #region TransCode 2 for strategy id 2211
                                if (packetHeader.TransCode == 2)
                                {
                                    if (packetHeader.StrategyId == 2211)
                                    {
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            int i = watch1.RowData.Index;
                                            dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.White;
                                        }
                                    }
                                }
                                #endregion

                                #region TransCode 5 for strategy id 2211
                                if (packetHeader.TransCode == 5)
                                {
                                    if (packetHeader.StrategyId == 2211)
                                    {
                                        AllInsertTrade(packetHeader);

                                        #region Trade for Watch
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            double PrvMargin = 0;
                                            string side = "";
                                            double mtm = 0;
                                            int i = watch1.RowData.Index;
                                            if (packetHeader.isWind)
                                                dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                                            else
                                                dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                                            if (packetHeader.isWind)
                                                side = "Wind";
                                            else
                                                side = "UnWind";
                                            PrvMargin = Convert.ToDouble(watch1.MarginUtilise);
                                            mtm = Convert.ToDouble(watch1.premium);
                                            if (side == "Wind")
                                            {
                                                if (watch1.Leg1.MidPrice != 0 && watch1.Leg2.MidPrice != 0)
                                                    watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0007) + (watch1.Leg2.MidPrice * 0.0011 * watch1.Leg2.Ratio);
                                                else
                                                    watch1.WindTrnCost = 0;
                                                watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) - watch1.WindTrnCost);
                                                watch1.Leg1.B_Qty = watch1.Leg1.B_Qty + watch1.Leg1.ContDetail.LotSize;
                                                watch1.Leg1.B_Value = watch1.Leg1.B_Value + (((packetHeader.TradePrice) - watch1.WindTrnCost) * watch1.Leg1.ContDetail.LotSize);
                                                watch1.Leg1.Buy_Qty = watch1.Leg1.Buy_Qty + (watch1.Leg1.ContDetail.LotSize * watch1.Leg1.Ratio);
                                                watch1.Leg2.Buy_Qty = watch1.Leg2.Buy_Qty + (watch1.Leg2.ContDetail.LotSize * watch1.Leg2.Ratio);
                                            }
                                            else
                                            {
                                                if (watch1.Leg1.MidPrice != 0 && watch1.Leg2.MidPrice != 0)
                                                    watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0011) + (watch1.Leg2.MidPrice * 0.0007 * watch1.Leg2.Ratio);
                                                else
                                                    watch1.UnwindTrnCost = 0;
                                                watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) - watch1.UnwindTrnCost);
                                                watch1.Leg1.S_Qty = watch1.Leg1.S_Qty + watch1.Leg1.ContDetail.LotSize;
                                                watch1.Leg1.S_Value = watch1.Leg1.S_Value + ((packetHeader.TradePrice) - watch1.UnwindTrnCost) * watch1.Leg1.ContDetail.LotSize;
                                                watch1.Leg2.Sell_Qty = watch1.Leg2.Sell_Qty + (watch1.Leg2.ContDetail.LotSize * watch1.Leg2.Ratio);
                                                watch1.Leg1.Sell_Qty = watch1.Leg1.Sell_Qty + (watch1.Leg1.ContDetail.LotSize * watch1.Leg1.Ratio);
                                            }
                                            watch1.Leg1.N_Qty = watch1.Leg1.B_Qty - watch1.Leg1.S_Qty;
                                            watch1.Leg1.Net_Qty = watch1.Leg1.Buy_Qty - watch1.Leg1.Sell_Qty;
                                            watch1.Leg2.Net_Qty = watch1.Leg2.Buy_Qty - watch1.Leg2.Sell_Qty;
                                            watch1.ProfitFlg = false;
                                            watch1.DrawDownFlg = false;
                                            if (watch1.Leg1.N_Qty != 0)
                                            {
                                                watch1.Leg1.N_Price = Math.Round((watch1.avgPrice / (Math.Abs(watch1.Leg1.N_Qty) / watch1.Leg1.ContDetail.LotSize)), 2);
                                                watch1.RowData.Cells[WatchConst.AvgPrice].Value = watch1.Leg1.N_Price;
                                            }
                                            else
                                            {
                                                watch1.Leg1.N_Price = 0;
                                                watch1.avgPrice = 0;
                                                watch1.Sqpnl = watch1.Sqpnl + (watch1.Leg1.B_Value - watch1.Leg1.S_Value);
                                                AppGlobal.OverAllPnl = AppGlobal.OverAllPnl + (watch1.Leg1.B_Value - watch1.Leg1.S_Value);
                                                OverAll_pnl.Text = Math.Round(AppGlobal.OverAllPnl, 2).ToString();
                                                watch1.RowData.Cells[WatchConst.AvgPrice].Value = watch1.Leg1.N_Price;
                                                watch1.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch1.Sqpnl, 2);
                                                #region Strategy Square off Pnl
                                                foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch1.Strategy) &&
                                                                                                                    (Convert.ToString(x.StrategyId) == "0")))
                                                {
                                                    watch.Sqpnl = watch.Sqpnl + watch1.Sqpnl;
                                                    watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
                                                }
                                                #endregion
                                            }
                                            if (watch1.Leg1.N_Qty > 0)
                                            {
                                                watch1.PosType = "Wind";
                                                watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
                                                watch1.posInt = ((watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize)));
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                            }
                                            else if (watch1.Leg1.N_Qty < 0)
                                            {
                                                watch1.PosType = "UnWind";
                                                watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
                                                watch1.posInt = ((watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize)));
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                            }
                                            else
                                            {
                                                watch1.pnl = 0;
                                                watch1.RowData.Cells[WatchConst.PNL].Value = watch1.pnl;
                                                watch1.RowData.Cells[WatchConst.PosType].Value = "None";
                                                watch1.posInt = 0;
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                                watch1.avgPrice = 0;
                                                watch1.Leg1.B_Qty = 0;
                                                watch1.Leg1.S_Qty = 0;
                                                watch1.Leg1.N_Qty = 0;
                                                watch1.Leg1.N_Price = 0;
                                                watch1.Leg1.B_Value = 0;
                                                watch1.Leg1.S_Value = 0;
                                                watch1.Leg1.Buy_Qty = 0;
                                                watch1.Leg1.Sell_Qty = 0;
                                                watch1.Leg2.Buy_Qty = 0;
                                                watch1.Leg2.Sell_Qty = 0;
                                                watch1.Leg1.Net_Qty = 0;
                                                watch1.Leg2.Net_Qty = 0;
                                            }
                                            watch1.premium = watch1.Leg1.N_Price * watch1.posInt * watch1.Leg1.ContDetail.LotSize;
                                            watch1.RowData.Cells[WatchConst.Premium].Value = Math.Round(watch1.premium, 2);
                                            watch1.TradedQty = watch1.Leg1.ContDetail.LotSize * watch1.posInt;
                                            watch1.RowData.Cells[WatchConst.TradedQty].Value = watch1.TradedQty;
                                            if (watch1.Leg1.ContractInfo.Series == "CE")
                                            {
                                                AppGlobal.CallMTM = AppGlobal.CallMTM - mtm + (watch1.premium / 2);
                                            }
                                            else
                                            {
                                                AppGlobal.PutMTM = AppGlobal.PutMTM - mtm + (watch1.premium / 2);
                                            }
                                            if (watch1.posInt > 0)
                                            {
                                                AppGlobal.CallBuyMTM = AppGlobal.CallBuyMTM + (watch1.premium / 2);
                                                AppGlobal.PutBuyMTM = AppGlobal.PutBuyMTM + (watch1.premium / 2);
                                            }
                                            else
                                            {
                                                AppGlobal.CallSellMTM = AppGlobal.CallSellMTM + (watch1.premium / 2);
                                                AppGlobal.PutSellMTM = AppGlobal.PutSellMTM + (watch1.premium / 2);
                                            }
                                            #region Margin Calculate
                                            if (watch1.posInt != 0)
                                            {
                                                if (watch1.posInt < 0)
                                                {
                                                    if (watch1.Leg1.ContractInfo.Symbol == "NIFTY")
                                                        watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * watch1.Leg1.Ratio * AppGlobal.niftyMargin * 2), 2);
                                                    if (watch1.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                                        watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * watch1.Leg1.Ratio * AppGlobal.bankniftyMargin * 2), 2);
                                                }
                                                else if (watch1.posInt > 0)
                                                {
                                                    watch1.MarginUtilise = Math.Round(Convert.ToDouble(watch1.Leg1.N_Price * Math.Abs(watch1.posInt) * watch1.Leg1.ContDetail.LotSize) / 2, 2);
                                                }

                                            }
                                            else if (watch1.posInt == 0)
                                            {
                                                watch1.MarginUtilise = 0;
                                            }
                                            watch1.RowData.Cells[WatchConst.MarginUtilise].Value = watch1.MarginUtilise;
                                            AppGlobal.OverallMarginUtilize = (AppGlobal.OverallMarginUtilize - PrvMargin) + (watch1.MarginUtilise);
                                            if (AppGlobal.OverallMarginUtilize != 0)
                                            {
                                                lblMargin.Text = Convert.ToString(Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3));
                                            }
                                            else
                                            {
                                                lblMargin.Text = "0";
                                            }

                                            TransactionWatch.ErrorMessage("|UniqueId|" + watch1.uniqueId + "|Strategy|" + watch1.StrategyId + "|symbol|" + watch1.Leg1.ContractInfo.Symbol + "|strike|" + watch1.Leg1.ContractInfo.StrikePrice + "|lotsize|" + watch1.Leg1.ContDetail.LotSize + "|NetQty|" +
                                                                              watch1.Leg1.N_Qty + "|NetAvg|" + watch1.Leg1.N_Price + "|SqPnl|" + watch1.Sqpnl + "|CurrPnl|" + watch1.pnl + "|Type|" + side + "|TradePrice|" + packetHeader.TradePrice.ToString() + "|AvgPrice|" + watch1.NetAvgPrice
                                                                              + "|PosInt|" + watch1.posInt + "|BuyValue|" + watch1.Leg1.B_Value + "|SellValue|" + watch1.Leg1.S_Value + "|NetValue|" + watch1.Leg1.A_Value);
                                            #endregion
                                            if (side == "UnWind")
                                            {
                                                #region Wind Trade entry in Log file
                                                if (packetHeader.StrategyId == 2211)
                                                {
                                                    TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                                    + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
                                                                                    + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize * watch1.Leg1.Ratio).ToString() + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
                                                    TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                                    + watch1.Expiry2 + "," + watch1.Leg2.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg2.ContractInfo.Series + ","
                                                                                    + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize * watch1.Leg2.Ratio).ToString() + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
                                                }

                                                #endregion
                                            }
                                            else
                                            {
                                                #region Unwind Trade entry in Log file
                                                if (packetHeader.StrategyId == 2211)
                                                {
                                                    TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                                    + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
                                                                                    + (watch1.Leg1.ContDetail.LotSize * watch1.Leg1.Ratio).ToString() + "," + "0" + "," + "0" + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
                                                    TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                                    + watch1.Expiry2 + "," + watch1.Leg2.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg2.ContractInfo.Series + ","
                                                                                    + (watch1.Leg1.ContDetail.LotSize * watch1.Leg2.Ratio).ToString() + "," + "0" + "," + "0" + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
                                                }

                                                #endregion
                                            }
                                        }
                                        #endregion

                                        AppGlobal.Count_Strangle = AppGlobal.Count_Strangle + 1;
                                        AppGlobal.TotalTrade = AppGlobal.TotalTrade + 1;
                                        lblTotalTrade.Text = Convert.ToString(AppGlobal.TotalTrade);
                                        MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
                                    }
                                }
                                #endregion
                            }
                            else if (packetHeader.StrategyId == 32211)
                            {
                                #region TransCode 1 for strategy id 32211
                                if (packetHeader.TransCode == 1)
                                {
                                    if (packetHeader.StrategyId == 32211)
                                    {
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            int i = watch1.RowData.Index;
                                            watch1.Wind = Convert.ToDecimal(packetHeader.Wind / 100);
                                            watch1.unWind = Convert.ToDecimal(packetHeader.Unwind / 100);

                                            watch1.RowData.Cells[WatchConst.Wind].Value = watch1.Wind;
                                            watch1.RowData.Cells[WatchConst.UnWind].Value = watch1.unWind;
                                        }
                                    }
                                }
                                #endregion

                                #region TransCode 2 for strategy id 32211
                                if (packetHeader.TransCode == 2)
                                {
                                    if (packetHeader.StrategyId == 32211)
                                    {
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            int i = watch1.RowData.Index;
                                            dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.White;
                                        }
                                    }
                                }
                                #endregion

                                #region TransCode 5 for strategy id 32211
                                if (packetHeader.TransCode == 5)
                                {
                                    if (packetHeader.StrategyId == 32211)
                                    {
                                        AllInsertTrade(packetHeader);

                                        #region trade for Watch
                                        foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == packetHeader.UniqueID)))
                                        {
                                            double PrvMargin = 0;
                                            int i = watch1.RowData.Index;
                                            string side = "";
                                            double mtm = 0;
                                            if (packetHeader.isWind)
                                                dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                                            else
                                                dgvMarketWatch.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;

                                            if (packetHeader.isWind)
                                                side = "Wind";
                                            else
                                                side = "UnWind";
                                            PrvMargin = Convert.ToDouble(watch1.MarginUtilise);
                                            mtm = Convert.ToDouble(watch1.premium);
                                            if (side == "Wind")
                                            {
                                                if (watch1.Leg1.MidPrice != 0)
                                                {
                                                    if (watch1.Leg1.ContractInfo.Series == "XX")
                                                        watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0001 * watch1.Leg1.Ratio);
                                                    else
                                                        watch1.UnwindTrnCost = (watch1.Leg1.MidPrice * 0.0007 * watch1.Leg1.Ratio);
                                                }
                                                else
                                                    watch1.UnwindTrnCost = 0;
                                                watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) + watch1.UnwindTrnCost);
                                                watch1.Leg1.B_Qty = watch1.Leg1.B_Qty + watch1.Leg1.ContDetail.LotSize;
                                                watch1.Leg1.B_Value = watch1.Leg1.B_Value + ((packetHeader.TradePrice + watch1.UnwindTrnCost) * watch1.Leg1.ContDetail.LotSize);
                                                watch1.Leg1.Buy_Qty = watch1.Leg1.Buy_Qty + watch1.Leg1.ContDetail.LotSize;
                                            }
                                            else
                                            {
                                                if (watch1.Leg1.MidPrice != 0)
                                                {
                                                    if (watch1.Leg1.ContractInfo.Series == "XX")
                                                        watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0001 * watch1.Leg1.Ratio);
                                                    else
                                                        watch1.WindTrnCost = (watch1.Leg1.MidPrice * 0.0011 * watch1.Leg1.Ratio);
                                                }
                                                else
                                                    watch1.WindTrnCost = 0;
                                                watch1.avgPrice = watch1.avgPrice + ((packetHeader.TradePrice) - watch1.WindTrnCost);
                                                watch1.Leg1.S_Qty = watch1.Leg1.S_Qty + watch1.Leg1.ContDetail.LotSize;
                                                watch1.Leg1.S_Value = watch1.Leg1.S_Value + ((packetHeader.TradePrice - watch1.WindTrnCost) * watch1.Leg1.ContDetail.LotSize);
                                                watch1.Leg1.Sell_Qty = watch1.Leg1.Sell_Qty + watch1.Leg1.ContDetail.LotSize;
                                            }
                                            watch1.Leg1.N_Qty = (watch1.Leg1.B_Qty - watch1.Leg1.S_Qty);
                                            watch1.Leg1.Net_Qty = (watch1.Leg1.Sell_Qty - watch1.Leg1.Buy_Qty);
                                            watch1.Leg1.A_Value = (watch1.Leg1.S_Value - watch1.Leg1.B_Value);

                                            watch1.ProfitFlg = false;
                                            watch1.DrawDownFlg = false;
                                            if (watch1.Leg1.N_Qty != 0)
                                            {
                                                watch1.Leg1.N_Price = Math.Round(watch1.Leg1.A_Value / watch1.Leg1.Net_Qty, 2);

                                                watch1.NetAvgPrice = (watch1.Leg1.S_Value - watch1.Leg1.B_Value) / (watch1.Leg1.Net_Qty);
                                                watch1.RowData.Cells[WatchConst.AvgPrice].Value = Math.Round(watch1.Leg1.N_Price, 2);
                                            }
                                            else
                                            {
                                                watch1.Sqpnl = watch1.Sqpnl + (watch1.Leg1.S_Value - watch1.Leg1.B_Value);
                                                AppGlobal.OverAllPnl = AppGlobal.OverAllPnl + (watch1.Leg1.S_Value - watch1.Leg1.B_Value);
                                                OverAll_pnl.Text = Math.Round(AppGlobal.OverAllPnl, 2).ToString();
                                                watch1.Leg1.N_Price = 0;
                                                watch1.avgPrice = 0;
                                                watch1.NetAvgPrice = 0;
                                                watch1.RowData.Cells[WatchConst.AvgPrice].Value = watch1.Leg1.N_Price;
                                                watch1.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch1.Sqpnl, 2);

                                                #region Strategy Square off Pnl
                                                foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch1.Strategy) &&
                                                                                                                 (Convert.ToString(x.StrategyId) == "0")))
                                                {
                                                    watch.Sqpnl = watch.Sqpnl + watch1.Sqpnl;
                                                    watch.RowData.Cells[WatchConst.SqPnl].Value = Math.Round(watch.Sqpnl, 2);
                                                }
                                                #endregion
                                            }
                                            if (watch1.Leg1.N_Qty > 0)
                                            {
                                                watch1.PosType = "Wind";
                                                watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
                                                watch1.posInt = (watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize));
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                            }
                                            else if (watch1.Leg1.N_Qty < 0)
                                            {
                                                watch1.PosType = "UnWind";
                                                watch1.RowData.Cells[WatchConst.PosType].Value = watch1.PosType;
                                                watch1.posInt = (watch1.Leg1.N_Qty / (watch1.Leg1.ContDetail.LotSize));
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                            }
                                            else
                                            {
                                                watch1.pnl = 0;
                                                watch1.RowData.Cells[WatchConst.PNL].Value = watch1.pnl;
                                                watch1.RowData.Cells[WatchConst.PosType].Value = "None";
                                                watch1.posInt = 0;
                                                watch1.RowData.Cells[WatchConst.PosInt].Value = watch1.posInt;
                                                watch1.avgPrice = 0;
                                                watch1.Leg1.B_Qty = 0;
                                                watch1.Leg1.S_Qty = 0;
                                                watch1.Leg1.B_Value = 0;
                                                watch1.Leg1.S_Value = 0;
                                                watch1.Leg1.N_Qty = 0;
                                                watch1.Leg1.N_Price = 0;
                                                watch1.Leg1.Buy_Qty = 0;
                                                watch1.Leg1.Sell_Qty = 0;
                                                watch1.Leg1.Net_Qty = 0;
                                            }


                                            watch1.premium = watch1.Leg1.N_Price * watch1.posInt * watch1.Leg1.ContDetail.LotSize;
                                            watch1.RowData.Cells[WatchConst.Premium].Value = Math.Round(watch1.premium, 2);
                                            watch1.TradedQty = watch1.Leg1.ContDetail.LotSize * watch1.posInt;
                                            watch1.RowData.Cells[WatchConst.TradedQty].Value = watch1.TradedQty;
                                            if (watch1.Leg1.ContractInfo.Series == "CE")
                                            {
                                                AppGlobal.CallMTM = AppGlobal.CallMTM - mtm + watch1.premium;
                                                if (watch1.posInt > 0)
                                                {
                                                    AppGlobal.CallBuyMTM = AppGlobal.CallBuyMTM - mtm + watch1.premium;
                                                }
                                                else
                                                {
                                                    AppGlobal.CallSellMTM = AppGlobal.CallSellMTM - mtm + watch1.premium;
                                                }
                                            }
                                            else
                                            {
                                                AppGlobal.PutMTM = AppGlobal.PutMTM - mtm + watch1.premium;
                                                if (watch1.posInt > 0)
                                                {
                                                    AppGlobal.PutBuyMTM = AppGlobal.PutBuyMTM - mtm + watch1.premium;
                                                }
                                                else
                                                {
                                                    AppGlobal.PutSellMTM = AppGlobal.PutSellMTM - mtm + watch1.premium;
                                                }
                                            }
                                            AppGlobal.overallPremium = (AppGlobal.overallPremium - mtm) + (watch1.premium);
                                            if (AppGlobal.overallPremium != 0)
                                            {
                                                premiumlbl.Text = Convert.ToString(Math.Round(AppGlobal.overallPremium / 10000000, 3));
                                            }
                                            else
                                            {
                                                premiumlbl.Text = "0";
                                            }

                                            #region Margin Calculate
                                            if (watch1.posInt != 0)
                                            {
                                                if (watch1.posInt < 0)
                                                {
                                                    if (watch1.Leg1.ContractInfo.Symbol == "NIFTY")
                                                        watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.niftyMargin), 2);
                                                    if (watch1.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                                                        watch1.MarginUtilise = Math.Round(Convert.ToDouble(Math.Abs(watch1.posInt) * AppGlobal.bankniftyMargin), 2);
                                                }

                                            }
                                            else if (watch1.posInt == 0)
                                            {
                                                watch1.MarginUtilise = 0;
                                            }
                                            watch1.RowData.Cells[WatchConst.MarginUtilise].Value = watch1.MarginUtilise;
                                            AppGlobal.OverallMarginUtilize = (AppGlobal.OverallMarginUtilize - PrvMargin) + (watch1.MarginUtilise);
                                            if (AppGlobal.OverallMarginUtilize != 0)
                                            {
                                                lblMargin.Text = Convert.ToString(Math.Round(AppGlobal.OverallMarginUtilize / 10000000, 3));
                                            }
                                            else
                                            {
                                                lblMargin.Text = "0";
                                            }

                                            #endregion

                                            TransactionWatch.ErrorMessage("|UniqueId|" + watch1.uniqueId + "|Strategy|" + watch1.StrategyId + "|symbol|" + watch1.Leg1.ContractInfo.Symbol + "|strike|" + watch1.Leg1.ContractInfo.StrikePrice + "|lotsize|" + watch1.Leg1.ContDetail.LotSize + "|NetQty|" +
                                                                              watch1.Leg1.N_Qty + "|NetAvg|" + watch1.Leg1.N_Price + "|SqPnl|" + watch1.Sqpnl + "|CurrPnl|" + watch1.pnl + "|Type|" + side + "|TradePrice|" + packetHeader.TradePrice.ToString() + "|AvgPrice|" + watch1.NetAvgPrice
                                                                              + "|PosInt|" + watch1.posInt + "|BuyValue|" + watch1.Leg1.B_Value + "|SellValue|" + watch1.Leg1.S_Value + "|NetValue|" + watch1.Leg1.A_Value);

                                            if (side == "Wind")
                                            {
                                                TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                              + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
                                                                              + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + "0" + "," + "0" + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
                                            }
                                            else
                                            {
                                                TransactionWatch.TradeMessage(DateTime.Now.ToString("HH:mm:ss:ffff") + "," + Convert.ToString(watch1.StrategyId) + "," + Convert.ToString(watch1.uniqueId) + "," + DateTime.Now.ToString("ddMMMyyyy") + "," + watch1.Leg1.ContractInfo.Symbol + ","
                                                                              + watch1.Expiry + "," + watch1.Leg1.ContractInfo.StrikePrice.ToString() + "," + watch1.Leg1.ContractInfo.Series + ","
                                                                              + "0" + "," + "0" + "," + (watch1.Leg1.ContDetail.LotSize).ToString() + "," + "0" + "," + ArisApi_a._arisApi.SystemConfig.UserName.ToString());
                                            }
                                        }
                                        #endregion

                                        AppGlobal.TotalTrade = AppGlobal.TotalTrade + 1;
                                        lblTotalTrade.Text = Convert.ToString(AppGlobal.TotalTrade);
                                        MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);

                                        LSL_Strangle_AvgPrice();
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                TransactionWatch.ErrorMessage("Wrong StrategyID");
                            }
                            lblcallbuy.Text = Math.Round(AppGlobal.CallBuyMTM, 2).ToString();
                            lblcallsell.Text = Math.Round(AppGlobal.CallSellMTM, 2).ToString();
                            lblputbuy.Text = Math.Round(AppGlobal.PutBuyMTM, 2).ToString();
                            lblputsell.Text = Math.Round(AppGlobal.PutSellMTM, 2).ToString();
                            CallMTM.Text = (Math.Round(AppGlobal.CallBuyMTM, 2) + Math.Round(AppGlobal.PutBuyMTM, 2)).ToString();
                            PutMTM.Text = (Math.Round(AppGlobal.CallSellMTM, 2) + Math.Round(AppGlobal.PutSellMTM, 2)).ToString();
                            if (packetHeader.TransCode == 5)
                            {
                                SendToTradeAdmin("Trade");
                            }
                            if (packetHeader.TransCode == 5)
                                FlashApplicationWindow("Straddle");
                        }
                    }

                }
                catch (Exception)
                { 
                }
            }

        }
    }
}
