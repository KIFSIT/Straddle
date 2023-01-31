using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;
using ArisDev;
using System.Threading;
using System.Drawing;

namespace Straddle
{
    partial class OptionWatch
    {
        public void BindBroadcastEvents()
        {
            ArisApi_a._arisApi.OnMarketDepthUpdate += new ArisApi_a.MarketDepthUpdateDelegate(_arisApi_OnMarketDepthUpdateCommom);
            ArisApi_a._arisApi.OnIndexBroadCast += new ArisApi_a.IndexBroadCastUpdateDelegate(_arisApi_OnIndexBroadCastCommon);
        }


        void _arisApi_OnMarketDepthUpdateCommom(MTApi.MTBCastPackets.MarketPicture _response)
        {
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)(() => _arisApi_OnMarketDepthUpdateCommom(_response)));
            else
            {
                try
                {
                    if (AppGlobal.NiftyToken == Convert.ToUInt64(_response.TokenNo))
                    {
                        txtNiftyValue.Text = (Convert.ToDecimal(_response.LastTradedPrice) / 100).ToString();
                        if (Convert.ToDouble(txtNiftyValue.Text) != 0 && Convert.ToDouble(lblcashNifty.Text) != 0)
                        {
                            double diff = Convert.ToDouble(txtNiftyValue.Text) - Convert.ToDouble(lblcashNifty.Text);
                            txtDiffNifty.Text = Convert.ToString(Math.Round(diff, 2));
                        }
                        if (AppGlobal.Flags == false)
                            Sum();
                    }
                    if (AppGlobal.BKToken == Convert.ToUInt64(_response.TokenNo))
                    {
                        txtbankValue.Text = (Convert.ToDecimal(_response.LastTradedPrice) / 100).ToString();
                        if (Convert.ToDouble(txtbankValue.Text) != 0 && Convert.ToDouble(lblcashbk.Text) != 0)
                        {
                            double diff = Convert.ToDouble(txtbankValue.Text) - Convert.ToDouble(lblcashbk.Text);
                            txtDiffBk.Text = Convert.ToString(Math.Round(diff, 2));
                        }
                        if (AppGlobal.Flags == false)
                            Sum();
                    }

                    if (AppGlobal.FinNiftyToken == Convert.ToUInt64(_response.TokenNo))
                    {
                        lblFinNiftyFut.Text = (Convert.ToDecimal(_response.LastTradedPrice) / 100).ToString();
                    }

                    #region Leg1
                    foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Leg1.ContractInfo.TokenNo) == _response.TokenNo)))
                    {
                        int i = watch.RowData.Index;
                        if (watch.Leg1.BuyPrice != 0)

                            watch.Leg1.OldBuyPrice = watch.Leg1.BuyPrice;
                        if (watch.Leg1.SellPrice != 0)
                            watch.Leg1.OldSellPrice = watch.Leg1.SellPrice;
                        watch.Leg1.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
                        watch.Leg1.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
                        watch.Leg1.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
                        watch.Leg1.MidPrice = Math.Round(Convert.ToDouble((watch.Leg1.BuyPrice + watch.Leg1.SellPrice) / 2), 2);
                        watch.RowData.Cells[WatchConst.L1buyPrice].Value = watch.Leg1.BuyPrice;
                        watch.RowData.Cells[WatchConst.L1sellPrice].Value = watch.Leg1.SellPrice;

                        watch.Leg1.ATP = Convert.ToDouble(_response.AverageTradedPrice) / 100;
                        watch.RowData.Cells[WatchConst.ATP].Value = Math.Round(watch.Leg1.ATP, 2);
                        AppGlobal.Pnl = AppGlobal.MarketWatch.Where(x => x.posInt != 0).Select(item => item.pnl).Sum();
                        lblPnl.Text = Math.Round(AppGlobal.Pnl, 2).ToString();
                        if (AppGlobal.Pnl != 0)
                        {
                            if (AppGlobal.LastPnl == 0)
                            {
                                AppGlobal.LastPnl = AppGlobal.Pnl;
                                SendToTradeAdmin("LastPnl");
                            }
                            else
                            {
                                if (AppGlobal.Pnl < (AppGlobal.LastPnl - ArisApi_a._arisApi.SystemConfig.LossPoints))
                                {
                                    SendToTradeAdmin("LastPnl");
                                    AppGlobal.LastPnl = AppGlobal.Pnl;
                                }
                            }
                        }
                        AppGlobal.Delta = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.sumDelta).Sum();
                        lblDelta.Text = Math.Round(AppGlobal.Delta, 4).ToString();
                        AppGlobal.Vega = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.sumVega).Sum();
                        lblVega.Text = Math.Round(AppGlobal.Vega, 4).ToString();
                        AppGlobal.Theta = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.sumTheta).Sum();
                        lblTheta.Text = Math.Round(AppGlobal.Theta, 4).ToString();
                        AppGlobal.Gamma = AppGlobal.MarketWatch.Where(x => x.Checked == true).Select(x => x.sumGamma).Sum();
                        lblGamma.Text = Math.Round(AppGlobal.Gamma, 4).ToString();
                        AppGlobal.upSideCallGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "CE").Select(x => x.sumGamma).Sum();
                        AppGlobal.upSidePutGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "PE").Select(x => x.sumGamma).Sum();
                        AppGlobal.downSideCallGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "CE").Select(x => x.sumGamma).Sum();
                        AppGlobal.downSidePutGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "PE").Select(x => x.sumGamma).Sum();
                        txtUpSideGamma.Text = (AppGlobal.upSideCallGamma + (AppGlobal.upSidePutGamma * -1)).ToString();
                        txtdownSideGamma.Text = ((AppGlobal.downSideCallGamma * -1) + AppGlobal.downSidePutGamma).ToString();

                        if (AppGlobal.Record)
                        {
                            if (watch.uniqueId == AppGlobal.RuleRecord)
                            {
                                TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg1.BuyPrice + "|SellPrice|" + watch.Leg1.SellPrice);
                            }
                        }
                        CalculateGreek(watch);
                        double SpotPrice = 0;
                        if (watch.Leg1.ContractInfo.Symbol == "NIFTY")
                        {
                            SpotPrice = AppGlobal.SpotNifty;
                        }
                        else if (watch.Leg1.ContractInfo.Symbol == "BANKNIFTY")
                        {
                            SpotPrice = AppGlobal.SpotBankNifty;
                        }
                        else if (watch.Leg1.ContractInfo.Symbol == "FINNIFTY")
                        {
                            SpotPrice = AppGlobal.SpotFinNifty;
                        }
                        else
                            SpotPrice = Convert.ToDouble(watch.niftyLeg.LastTradedPrice);

                        if (watch.StrategyId == 91)
                        {
                            if (watch.Leg1.ContractInfo.Series == "CE")
                            {
                                if (SpotPrice > watch.Leg1.ContractInfo.StrikePrice)
                                {
                                    double Intensic = Convert.ToDouble(SpotPrice - watch.Leg1.ContractInfo.StrikePrice) - Convert.ToDouble(watch.Leg1.LastTradedPrice);
                                    watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(Intensic), 2);
                                }
                                else
                                {
                                    watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(watch.Leg1.LastTradedPrice), 2);
                                }
                            }
                            else if (watch.Leg1.ContractInfo.Series == "PE")
                             {
                                if (SpotPrice < watch.Leg1.ContractInfo.StrikePrice)
                                {
                                    double Intensic = Convert.ToDouble(SpotPrice - watch.Leg1.ContractInfo.StrikePrice) + Convert.ToDouble(watch.Leg1.LastTradedPrice);
                                    watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(Intensic), 2);
                                }
                                else
                                {
                                    watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(watch.Leg1.LastTradedPrice), 2);
                                }
                            }
                            if (watch.posInt != 0)
                            {
                                watch.LivePremium = Convert.ToDouble(watch.posInt * watch.Leg1.ContDetail.LotSize) * Convert.ToDouble(watch.Leg1.LastTradedPrice);
                                watch.RowData.Cells[WatchConst.LivePremium].Value = Math.Round(watch.LivePremium, 2);
                            }
                            else
                            {
                                watch.LivePremium = 0;
                                watch.RowData.Cells[WatchConst.LivePremium].Value = Math.Round(watch.LivePremium, 2);
                            }
                        }
                        else if (watch.StrategyId == 12211)
                        {
                            if (watch.Leg1.ContractInfo.Series == "CE" && watch.Leg2.ContractInfo.Series == "PE")
                            {
                                double Intensic1 = 0;
                                double Intensic2 = 0;
                                if (SpotPrice > watch.Leg1.ContractInfo.StrikePrice)
                                {
                                    Intensic1 = Math.Abs(Convert.ToDouble(SpotPrice - watch.Leg1.ContractInfo.StrikePrice) - Convert.ToDouble(watch.Leg1.LastTradedPrice));
                                }
                                else
                                {
                                    Intensic1 = Math.Round(Math.Abs(Convert.ToDouble(watch.Leg1.LastTradedPrice)), 2);
                                }

                                if (SpotPrice < watch.Leg2.ContractInfo.StrikePrice)
                                {
                                    Intensic2 = Math.Abs(Convert.ToDouble(SpotPrice - watch.Leg2.ContractInfo.StrikePrice) + Convert.ToDouble(watch.Leg2.LastTradedPrice));
                                }
                                else
                                {
                                    Intensic2 = Math.Round(Math.Abs(Convert.ToDouble(watch.Leg2.LastTradedPrice)), 2);
                                }


                                watch.RowData.Cells[WatchConst.Intensic].Value = Math.Round(Math.Abs(Intensic1 + Intensic2), 2);
                            }

                        }
                        if (watch.StrategyId == 91)
                        {
                            CalculateSpreadSingle(watch);
                            AvgCalculatedGreek(watch);
                        }
                        if (watch.StrategyId == 1113 || watch.StrategyId == 1114)
                        {
                            if (watch.Leg1.ATP != 0 && watch.Leg2.ATP != 0)
                            {
                                double atp = (watch.Leg2.ATP - watch.Leg1.ATP);
                                watch.RowData.Cells[WatchConst.ATP].Value = Math.Round(atp, 2);
                            }

                            CalculateSpreadRatio11_12(watch);

                        }
                        else if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
                        {
                            if (watch.Leg1.ATP != 0 && watch.Leg2.ATP != 0)
                            {
                                double atp = (watch.Leg1.ATP + watch.Leg2.ATP);
                                watch.RowData.Cells[WatchConst.ATP].Value = Math.Round(atp, 2);
                            }
                            CalculateStrangleSpread(watch);
                        }
                        if (watch.StrategyName.Contains("MainJodiStraddle"))
                        {
                            watch.straddleMktWind = AppGlobal.MarketWatch.Where(x => x.StrategyName == watch.StrategyName).Select(x => x.MktWind).Sum();
                            watch.straddleMktUnwind = AppGlobal.MarketWatch.Where(x => x.StrategyName == watch.StrategyName).Select(x => x.MktunWind).Sum();
                            watch.RowData.Cells[WatchConst.Straddle_MktWind].Value = watch.straddleMktWind;
                            watch.RowData.Cells[WatchConst.Straddle_MktUnwind].Value = watch.straddleMktUnwind;
                            if (watch.Hedgeflg)
                            {
                                if (watch.Track == "Hedge")
                                {
                                    #region Hedge Straddle calcualtion
                                    if (!watch.StrategyName.Contains("_Straddle") || !watch.StrategyName.Contains("_Strangle"))
                                    {
                                        if (watch.posInt != 0)
                                        {
                                            string straddleHedgeStrategy = watch.StrategyName + "_Straddle";
                                            string strangleHedgeStrategy = watch.StrategyName + "_Strangle";
                                            double straddleAvg = AppGlobal.MarketWatch.Where(x => (x.StrategyName == straddleHedgeStrategy) || (x.StrategyName == strangleHedgeStrategy)).Select(x => x.MktunWind).Sum();
                                            if (straddleAvg < watch.StraddlAvg)
                                            {
                                                string _strategyName = watch.StrategyName;
                                                const char fieldSeparator = '_';
                                                List<string> split = _strategyName.Split(fieldSeparator).ToList();
                                                string _findStrategy = split[0] + "_" + split[1];
                                                foreach (var watch1 in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.StrategyName).Contains(_findStrategy))))
                                                {
                                                    watch1.StraddlAvg = straddleAvg;
                                                    watch1.RowData.Cells[WatchConst.StrategyAvg].Value = Math.Round(watch1.StraddlAvg, 2);
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                            if (watch.Hedgeflg)
                            {
                                #region Hedge order send
                                if (watch.Track == "Main")
                                {
                                    HedgeWithMain(watch);
                                }
                                else if (watch.Track == "Hedge")
                                {
                                    if (watch.StrategyName.Contains("_Straddle") || watch.StrategyName.Contains("_Strangle"))
                                    {
                                        HedgeWithHedge(watch);
                                    }
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            watch.straddleMktWind = watch.MktWind;
                            watch.straddleMktUnwind = watch.MktunWind;
                            watch.RowData.Cells[WatchConst.Straddle_MktWind].Value = watch.straddleMktWind;
                            watch.RowData.Cells[WatchConst.Straddle_MktUnwind].Value = watch.straddleMktUnwind;
                        }
                        if (watch.StrategyId == 91)
                        {
                            SQ_OFF_Rule(watch);
                            System.Threading.Tasks.Task.Factory.StartNew(() =>
                            {
                                RuleActionfunct(watch);
                            });
                        }
                        if (watch.StrategyId == 91 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
                        {
                            #region StopLoss Order
                            StopLossBuyOrder(watch);
                            StopLossSellOrder(watch);
                            #endregion
                        }
                        if (watch.StrategyId == 91 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
                        {
                            #region DrawDown Order
                            DrawDownBuyOrder(watch);
                            DrawDownSellOrder(watch);
                            #endregion
                        }
                        if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 32211 || watch.StrategyId == 91 || watch.StrategyId == 1113 || watch.StrategyId == 1114)
                        {
                            #region StopLoss Order

                            Thread t = new Thread(() =>
                            {
                                StraddleStopLoss(watch);

                            });
                            t.Start();
                            #endregion
                        }

                        if (watch.StrategyId == 32211)
                        {
                            if (watch.Leg2.BuyPrice != 0 && watch.Leg1.BuyPrice != 0)
                            {
                                LSL_StranglePnl(watch);
                            }
                            LSL_StrangleCheckFlgStoploss(watch);
                        }

                        if (watch.StrategyId == 91)
                        {
                            if (watch.PremiumAlert)
                            {
                                SqAll_Premium(watch, "PremiumLossHit");
                            }
                        }

                        if (watch.StrategyId == 91)
                        {
                            if (watch.SqTimeflg)
                            {
                                UInt64 nowTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(DateTime.Now));
                                UInt64 uintTime = ArisApi_a._arisApi.DateTimeToSecond(Market.NseCm, Convert.ToDateTime(watch.SqTime));
                                if (uintTime < nowTime)
                                {
                                    watch.SqTimeflg = false;
                                    SqoffAll(watch, "RuleSqOff");
                                }
                            }
                        }

                    }
                    #endregion

                    #region FUTURE
                    foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.niftyLeg.ContractInfo.TokenNo) == _response.TokenNo)))
                    {
                        int i = watch.RowData.Index;
                        if (watch.niftyLeg.BuyPrice != 0)
                            watch.niftyLeg.OldBuyPrice = watch.niftyLeg.BuyPrice;
                        if (watch.niftyLeg.SellPrice != 0)
                            watch.niftyLeg.OldSellPrice = watch.niftyLeg.SellPrice;

                        watch.niftyLeg.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
                        watch.niftyLeg.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
                        watch.niftyLeg.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
                        watch.RowData.Cells[WatchConst.FLTP].Value = watch.niftyLeg.LastTradedPrice;
                        //CalculateGreek(watch);
                        //CalculateSpread(watch);
                    }
                    #endregion

                    #region Leg2

                    foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Leg2.ContractInfo.TokenNo) == _response.TokenNo)))
                    {
                        int i = watch.RowData.Index;
                        if (watch.Leg2.BuyPrice != 0)
                            watch.Leg2.OldBuyPrice = watch.Leg2.BuyPrice;
                        if (watch.Leg2.SellPrice != 0)
                            watch.Leg2.OldSellPrice = watch.Leg2.SellPrice;
                        watch.Leg2.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
                        watch.Leg2.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
                        watch.Leg2.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
                        watch.Leg2.MidPrice = Math.Round(Convert.ToDouble((watch.Leg2.BuyPrice + watch.Leg2.SellPrice) / 2), 2);
                        watch.RowData.Cells[WatchConst.L2buyPrice].Value = watch.Leg2.BuyPrice;
                        watch.RowData.Cells[WatchConst.L2sellPrice].Value = watch.Leg2.SellPrice;

                        watch.Leg2.ATP = Convert.ToDouble(_response.AverageTradedPrice) / 100;

                        if (AppGlobal.Record)
                        {
                            if (watch.uniqueId == AppGlobal.RuleRecord)
                            {
                                TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg2.BuyPrice + "|SellPrice|" + watch.Leg2.SellPrice);
                            }
                        }
                        CalculateGreek(watch);
                        if (watch.StrategyId == 111 || watch.StrategyId == 211 || watch.StrategyId == 311)
                        {
                            CalculateSpread(watch);
                            CalculateSpreadRatio11_12(watch);
                        }
                        if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
                        {
                            if (watch.Leg1.ATP != 0 && watch.Leg2.ATP != 0)
                            {
                                double atp = (watch.Leg1.ATP + watch.Leg2.ATP);
                                watch.RowData.Cells[WatchConst.ATP].Value = Math.Round(atp, 2);
                            }
                            CalculateStrangleSpread(watch);
                        }
                        else if (watch.StrategyId == 2211)
                            CalculateStrangleSpread(watch);
                        else if (watch.StrategyId == 888)
                            CalculateLadderSpread(watch);
                        else if (watch.StrategyId == 121)
                            CalculateButterflySpread(watch);
                        else if (watch.StrategyId == 1331)
                            CalculateSpread1331(watch);
                        else if (watch.StrategyId == 1221)
                            CalculateSpread1221(watch);

                        else if (watch.StrategyId == 1113 || watch.StrategyId == 1114)
                        {
                            CalculateSpreadRatio11_12(watch);

                        }


                        if (watch.StrategyId == 2211 || watch.StrategyId == 12211 || watch.StrategyId == 32211)
                        {
                            #region StopLoss Order
                            System.Threading.Tasks.Task.Factory.StartNew(() =>
                            {
                                StraddleStopLoss(watch);
                            });
                            #endregion
                        }
                        if (watch.StrategyId == 32211 && watch.Leg2.ContractInfo.TokenNo != "0")
                        {
                            if (watch.Leg2.BuyPrice != 0 && watch.Leg1.BuyPrice != 0)
                            {
                                LSL_StranglePnl(watch);

                            }
                        }
                    }
                    #endregion

                    #region Leg3

                    foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Leg3.ContractInfo.TokenNo) == _response.TokenNo)))
                    {
                        int i = watch.RowData.Index;
                        if (watch.Leg3.BuyPrice != 0)
                            watch.Leg3.OldBuyPrice = watch.Leg3.BuyPrice;
                        if (watch.Leg3.SellPrice != 0)
                            watch.Leg3.OldSellPrice = watch.Leg3.SellPrice;
                        watch.Leg3.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
                        watch.Leg3.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
                        //watch.Leg3.Sequence = seq;
                        watch.Leg3.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
                        watch.Leg3.MidPrice = Math.Round(Convert.ToDouble((watch.Leg3.BuyPrice + watch.Leg3.SellPrice) / 2), 2);
                        watch.RowData.Cells[WatchConst.L3buyPrice].Value = watch.Leg3.BuyPrice;
                        watch.RowData.Cells[WatchConst.L3sellPrice].Value = watch.Leg3.SellPrice;

                        if (AppGlobal.Record)
                        {
                            if (watch.uniqueId == AppGlobal.RuleRecord)
                            {
                                TransactionWatch.ErrorMessage("Leg1|" + "BuyPrice|" + watch.Leg2.BuyPrice + "|SellPrice|" + watch.Leg2.SellPrice);
                            }
                        }
                        CalculateGreek(watch);
                        if (watch.StrategyId == 888)
                            CalculateLadderSpread(watch);
                        else if (watch.StrategyId == 121)
                            CalculateButterflySpread(watch);
                        else if (watch.StrategyId == 1331)
                            CalculateSpread1331(watch);
                        else if (watch.StrategyId == 1221)
                            CalculateSpread1221(watch);
                    }
                    #endregion

                    #region Leg4

                    foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Leg4.ContractInfo.TokenNo) == _response.TokenNo)))
                    {
                        int i = watch.RowData.Index;
                        if (watch.Leg4.BuyPrice != 0)
                            watch.Leg4.OldBuyPrice = watch.Leg4.BuyPrice;
                        if (watch.Leg4.SellPrice != 0)
                            watch.Leg4.OldSellPrice = watch.Leg4.SellPrice;
                        watch.Leg4.BuyPrice = Convert.ToDouble(_response.Best5Buy[0].OrderPrice) / 100;
                        watch.Leg4.SellPrice = Convert.ToDouble(_response.Best5Sell[0].OrderPrice) / 100;
                        //watch.Leg3.Sequence = seq;
                        watch.Leg4.LastTradedPrice = Convert.ToDecimal(_response.LastTradedPrice) / 100;
                        watch.Leg4.MidPrice = Math.Round(Convert.ToDouble((watch.Leg4.BuyPrice + watch.Leg4.SellPrice) / 2), 2);
                        watch.RowData.Cells[WatchConst.L4buyPrice].Value = watch.Leg4.BuyPrice;
                        watch.RowData.Cells[WatchConst.L4sellPrice].Value = watch.Leg4.SellPrice;
                        if (watch.StrategyId == 1331)
                            CalculateSpread1331(watch);
                        else if (watch.StrategyId == 1221)
                            CalculateSpread1221(watch);


                    }
                    #endregion
                }
                catch (Exception)
                {
                }
            }
        }

        void _arisApi_OnIndexBroadCastCommon(ArisDev.NseCmApi.Broadcast.Indices _response)
        {
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)(() => _arisApi_OnIndexBroadCastCommon(_response)));
            else
            {
                try
                {
                    char[] Sym = _response.IndexName.ToCharArray();
                    string SYM = new string(Sym);
                    if (SYM.Trim() == "Nifty 50")
                    {
                        AppGlobal.SpotNifty = (Convert.ToDouble(_response.IndexValue) / 100);
                        lblcashNifty.Text = (Convert.ToDouble(_response.IndexValue) / 100).ToString();
                        if (AppGlobal.LastSpotPrice < AppGlobal.SpotNifty)
                        {
                            AppGlobal.LastSpotPrice = AppGlobal.SpotNifty + (AppGlobal.SpotNifty * 0.005);
                            SendToTradeAdmin("Spot");
                        }
                    }
                    else if (SYM.Trim() == "Nifty Bank")
                    {
                        AppGlobal.SpotBankNifty = (Convert.ToDouble(_response.IndexValue) / 100);

                        lblcashbk.Text = (Convert.ToDouble(_response.IndexValue) / 100).ToString();
                    }
                    if (SYM.Trim() == "Nifty Fin Service")
                    {
                        AppGlobal.SpotFinNifty = (Convert.ToDouble(_response.IndexValue) / 100);
                        lblFinNiftySpot.Text = (Convert.ToDouble(_response.IndexValue) / 100).ToString();
                    }
                    if (SYM.Trim() == "India VIX")
                    {
                        txtVIX.Text = (Math.Round(Convert.ToDouble(_response.IndexValue) / 100, 2)).ToString();
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        void RuleActionfunct(MarketWatch watch)
        {
            if (watch.RuleAction.Count != 0)
            {
                foreach (var kvp in watch.RuleAction.Keys)
                {
                    if (watch.RuleAction[kvp].Preform == false)
                    {
                        if (watch.RuleAction[kvp].Side == "BUY")
                        {
                            if (watch.RuleAction[kvp].Price >= Math.Abs(watch.MktunWind))
                            {
                                watch.RuleAction[kvp].Preform = true;
                                sendOrder(watch, watch.RuleAction[kvp].Lots, watch.RuleAction[kvp].Side);
                                TransactionWatch.TransactionMessage("RuleAction|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Price|" + watch.RuleAction[kvp].Price + "|Qty|" + watch.RuleAction[kvp].Lots
                                                                    + "|Side|" + watch.RuleAction[kvp].Side + "|action|" + watch.RuleAction[kvp].Preform + "|Wind|" + watch.MktWind + "|Unwind|" + watch.MktunWind,Color.Red);
                                TransactionWatch.ErrorMessage("RuleAction|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Price|" + watch.RuleAction[kvp].Price + "|Qty|" + watch.RuleAction[kvp].Lots
                                                                    + "|Side|" + watch.RuleAction[kvp].Side + "|action|" + watch.RuleAction[kvp].Preform + "|Wind|" + watch.MktWind + "|Unwind|" + watch.MktunWind);
                                                                     
                            }
                        }
                        else if (watch.RuleAction[kvp].Side == "SELL")
                        {
                            if (watch.RuleAction[kvp].Price <= Math.Abs(watch.MktWind))
                            {
                                watch.RuleAction[kvp].Preform = true;
                                sendOrder(watch, watch.RuleAction[kvp].Lots, watch.RuleAction[kvp].Side);
                                TransactionWatch.TransactionMessage("RuleAction|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Price|" + watch.RuleAction[kvp].Price + "|Qty|" + watch.RuleAction[kvp].Lots
                                                                    + "|Side|" + watch.RuleAction[kvp].Side + "|action|" + watch.RuleAction[kvp].Preform + "|Wind|" + watch.MktWind + "|Unwind|" + watch.MktunWind, Color.Red);
                                TransactionWatch.ErrorMessage("RuleAction|Unique|" + watch.uniqueId + "|Strategy|" + watch.StrategyId + "|Price|" + watch.RuleAction[kvp].Price + "|Qty|" + watch.RuleAction[kvp].Lots
                                                                    + "|Side|" + watch.RuleAction[kvp].Side + "|action|" + watch.RuleAction[kvp].Preform + "|Wind|" + watch.MktWind + "|Unwind|" + watch.MktunWind);

                            }
                        }
                    }
                }
            }
        }
    }
}
