using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;
using ClientCommon;
using LogWriter;
using MTApi;
using System.IO;
using MTCommon;
using ArisDev;
using System.Threading;

namespace Straddle.AppClasses
{
    [Serializable]
    public class MarketWatch
    {
        #region CashFuture Trader
        public bool IsActive;
        public bool IsStrikeReq;
        public string AccountType;

        public double StrategyAvgPrice;

        public Leg Leg1;
        public Leg Leg2;
        public Leg Leg3;
        public Leg Leg4;


       

        public double DeltaV;
        public double VegaV;
        public double GammaV;
        public double ThetaV;

        public double Avg_IV;
        public double Avg_Delta;
        public double Avg_Vega;
        public double Avg_Theta;
        public double Avg_Gamma;

        public double Avg_DeltaV;
        public double Avg_VegaV;
        public double Avg_ThetaV;
        public double Avg_GammaV;


        public int SeqaureOff;

        public bool Checked = true;
        public bool trail;
        public double trail_No;
        public double trail_Lots;
        public string trail_Side;
        public string trail_Time;
        public double trail_Profit;
        public bool trail_reset;
        public int ExeTime;
        public double Maxtrail;
        public double startingTrail;
        public Leg niftyLeg;
        public int strikediff;
        public int Ruleno;
        public string posSummary;
        public string Prev_posSummary;
        public string Strategy_Name;
        public string Strategy;
        public int Bidding_start;
        public double AskPxDiff;
        public double AskIVDiff;
        public double BidPxDiff;
        public double BidIVDiff;
        public int nonDrvStrike;
        public string nonDrvSeries;
        public decimal Wind;
        public decimal unWind;
        public double Threshold;
        public int Over;
        public int Round;
        public double MktWind;
        public double MktunWind;
        public double oldMktWind;
        public double oldMktUnWind;
        public UInt64 uniqueId;
        public int StrategyId;
        public int TLI_StrategyId;

        public UInt64 Gui_id;
        public string Expiry;
        public string Expiry2;
        public string StrategyName;
        public double CarryForwardPnl;
        public double StrategyPnl;
        public double AskuserIV;
        public double BiduserIV;
        public int RemainingDay;
        public int URem_Day;
        public bool sendStrikeRequest;
        public UInt64 MarketUniqueId;
        public string Type;
        public string displayUniqueId;
        public UInt64 UniqueIdLeg1;
        public UInt64 UniqueIdLeg2;
        public int TLI_UniqueId;
        public int LSL_UniqueId;
        public double FutPrice;

        public int windCount;
        public int UnwindCount;
        public double LastWind;
        public double LastUnWind;
        public double avgPrice;
        public double NetAvgPrice;
        public string PosType;
        public int posInt;
        public int TradedQty;
        public int wCount;
        public int uwCount;
        public int enterCount;
        public double WindTrnCost;
        public double UnwindTrnCost;
        public double TransCost;
        public double interest;
        public double pnl;
        public double S_pnl;
        public double Sqpnl;
        public string Rule;
        public bool not_got_first_tick;
        public double Delta;
        public double Vega;
        public double Theta;
        public double Gamma;
        public double MaxPnl;
        public double S_MaxPnl;
        public double Profit;
        public double DrawDown;
        public string Strategy_Type;
        public double straddleMktWind;
        public double straddleMktUnwind;
        public double StraddlAvg;
        public double prvStraddleAvg;
        public double sumDelta;
        public double sumVega;
        public double sumGamma;
        public double sumTheta;
        public double NotificationTimeProfit;
        public double NotificationTimeDrawdown;
        public bool ProfitFlg = false;
        public bool DrawDownFlg = false;
        public double PrvPosInt;
        public double MarginUtilise;
        public double AddorSubMargin;
        public double premium;
        public double LivePremium;
        public bool misPricing = false;
        public bool misSpread = false;
        public double var_watch_avg_price;
        public double varexp_watch_avg_price;
        public double var_shortest_period;

        public int HedgePosition;
        public double round1Percent;
        public double round2Percent;
        public double round3Percent;
        public double round4Percent;

        public double round1Point;
        public double round2Point;
        public double round3Point;
        public double round4Point;

        public bool LSL_StopLossFlg;
        public double LSL_StopLossPercent;

        public double LSL_StopLossValue;
        public double LSL_StrategyLive;

        public int L1PosInt;
        public int L2PosInt;

              
        public bool Alert; 
        public bool BuyAlert;
        public int AlertLevel;



        public bool PremiumAlert;
        public bool PremiumUserpxAlert;
        public bool PremiumTrade;

        public double PremiumCurrent;
        public double PremiumUserPx;
        public double PremiumPoint;

        public double Init_Premium;

        public string Premium_indicator = "Point";
        public double Premium_Percent;
        public double Premium_dm;
        public double TG_Premium;





        [XmlIgnore]
        public bool go = false;

        [XmlIgnore]
        public bool go1 = false;

        [XmlIgnore]
        public bool go2 = false;

        [XmlIgnore]
        public Thread thread;

        [XmlIgnore]
        public Thread thread1;

        [XmlIgnore]
        public Thread thread2;

        [XmlIgnore]
        public bool StoplossTrade = false;

        [XmlIgnore]
        public bool ProfitTrade = false;

        [XmlIgnore]
        public bool TrailTrade = false;
        
        [XmlIgnore]
        public bool SL_BuyOrderflg = false;

        [XmlIgnore]
        public double TGBuyPrice = 999999;

        [XmlIgnore]
        public double AP_BuySL = 999999;

        [XmlIgnore]
        public int SL_BuyQty = 0;

        [XmlIgnore]
        public bool SL_SellOrderflg = false;

        [XmlIgnore]
        public double TGSellPrice = 999999;

        [XmlIgnore]
        public double AP_SellSL = 999999;

        [XmlIgnore]
        public int SL_SellQty = 0;

        [XmlIgnore]
        public bool DD_BuyOrderflg = false;

        [XmlIgnore]
        public bool Alert_BuyOrderflg = false;

        [XmlIgnore]
        public double DD_bm_Buy = 0;

        [XmlIgnore]
        public double DD_BuyMaxPrice;

        [XmlIgnore]
        public double DD_SetMax = 0;

        [XmlIgnore]
        public double DD_bm_Sell_Percent = 0;

        [XmlIgnore]
        public string DD_Sell_indicator = "Point";


        [XmlIgnore]
        public double DD_TGBuyPrice;

        [XmlIgnore]
        public double DD_BuyQty = 0;

        [XmlIgnore]
        public bool DD_SellOrderflg = false;

        [XmlIgnore]
        public bool Alert_SellOrderflg = false;

        [XmlIgnore]
        public double DD_bm_Sell = 0;

        [XmlIgnore]
        public double DD_TGSellPrice;

        [XmlIgnore]
        public double DD_SellQty = 0;

        [XmlIgnore]
        public double DD_SellMinPrice;

        [XmlIgnore]
        public double DD_SetMin = 0;


        [XmlIgnore]
        public double DD_bm_Buy_Percent = 0;

        [XmlIgnore]
        public string DD_Buy_indicator = "Point";

        [XmlIgnore]
        public bool ProfitTrail;


        [XmlIgnore]
        public bool TrailingStart;

        [XmlIgnore]
        public bool UserPriceflg;


        [XmlIgnore]
        public double trail_bm = 0;

        [XmlIgnore]
        public double trail_TGPrice = 0;

        [XmlIgnore]
        public double trail_MinPrice = 0;

        [XmlIgnore]
        public double trail_SetMax = 0;


        [XmlIgnore]
        public double trail_bm_Percent = 0;

        [XmlIgnore]
        public string trail_indicator = "Point";

        [XmlIgnore]
        public bool LSL_Stoplossflg;


        [XmlIgnore]
        public double LSL_AvgPriceCE;

        [XmlIgnore]
        public double LSL_AvgPricePE;


        [XmlIgnore]
        public double DD_InitialBuyPrice;

        [XmlIgnore]
        public double DD_InitialSellPrice;

        [XmlIgnore]
        public double trail_InitialPrice;


        [XmlIgnore]
        public bool userbuy;

        [XmlIgnore]
        public bool usersell;

        [XmlIgnore]
        public bool usertrail;


        [XmlIgnore]
        public DataGridViewComboBoxCell ColumnCombo;
        
        [XmlIgnore]
        public DataGridViewRow RowData;

        [XmlIgnore]
        public decimal OldWind;

        [XmlIgnore]
        public decimal OldUnWind;

        [XmlIgnore]
        public string SqTime;

        [XmlIgnore]
        public bool SqTimeflg = false;


        public string Track;
        public double StrategyDrawDown;
        public bool Hedgeflg;


        [XmlIgnore]
        public bool SQVegaflg = false;

        [XmlIgnore]
        public bool SQPremiumflg = false;

        [XmlIgnore]
        public bool SQLossflg = false;

        [XmlIgnore]
        public double SQVegaPrice = 0;

        [XmlIgnore]
        public double SQVegaPoint = 0;

        [XmlIgnore]
        public double SQPremiumPrice = 0;

        [XmlIgnore]
        public double SQPremiumPoint = 0;

        [XmlIgnore]
        public double SQLossPrice = 0;

        [XmlIgnore]
        public double SQLossPoint = 0;

        [XmlIgnore]
        public double Init_SQVegaPrice = 0;

        [XmlIgnore]
        public double Init_SQPremiumPrice = 0;

        [XmlIgnore]
        public double Init_SQLossPrice = 0;


        [XmlIgnore]
        public string SQVegaType = "Point";

        [XmlIgnore]
        public string SQPremiumType = "Point";

        [XmlIgnore]
        public string SQLossType = "Point";

        [XmlIgnore]
        public double Per_SQVegaPrice = 0;

        [XmlIgnore]
        public double Per_SQPremiumPrice = 0;

        [XmlIgnore]
        public double Per_SQLossPrice = 0;



        [XmlIgnore]
        public bool I_Trailingflg = false;

        [XmlIgnore]
        public bool I_UserPxTrailingflg = false;

        [XmlIgnore]
        public bool I_TrailingTradeflg = false;

        [XmlIgnore]
        public double I_TrailingPrice = 0;

        [XmlIgnore]
        public double I_TrailingPoint = 0;

        [XmlIgnore]
        public int I_TrailingQty = 0;

        [XmlIgnore]
        public double I_TrailingMinMaxPrice = 0;

        [XmlIgnore]
        public double I_TrailingInitial = 0;

        [XmlIgnore]
        public double I_TrailingTriggerPx = 0;

        [XmlIgnore]
        public string I_TrailingSide = "None";

        [XmlIgnore]
        public InputParameter[] _inputParameter;

        [XmlIgnore]
        public int Itration;


        [XmlIgnore]
        public bool I_Priceflg = false;

        [XmlIgnore]
        public bool I_UserPriceflg = false;

        [XmlIgnore]
        public double I_Price = 0;

        [XmlIgnore]
        public int I_PriceQty = 0;

        [XmlIgnore]
        public string I_PriceSide = "None";

        [XmlIgnore]
        public bool I_PriceTrade = false;


        [XmlIgnore]
        public int iterator = 0;

        [XmlIgnore]
        public bool iteratorflg = false;

        [XmlIgnore]
        public int iteratorCount = 0;

        [XmlIgnore]
        public string iteratorSide = "None";

        [XmlIgnore]
        public bool itreatorTradeflg = false;


        [XmlIgnore]
        public SortedDictionary<int, RuleParameter> RuleAction = new SortedDictionary<int, RuleParameter>();

        [XmlIgnore]
        public int RuleActionNo = 1;
        #endregion

        #region Read/Write
        public static void WriteXmlProfile(ref List<MarketWatch> watch)
        {
            try
            {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<MarketWatch>));
                StreamWriter streamWriter = new StreamWriter(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.Watch + ".tst");

                string date = DateTime.Now.ToString("ddMMMyyyy");
                StreamWriter streamWriterDaily = new StreamWriter(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.Watch + date + ".tst");                


                xmlSerializer.Serialize(streamWriter, watch);
                xmlSerializer.Serialize(streamWriterDaily, watch);
                streamWriter.Close();
                streamWriterDaily.Close();
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "WriteXmlProfile")
                                          , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        

        public static List<MarketWatch> ReadXmlProfile()
        {
            List<MarketWatch> Result = new List<MarketWatch>();
            try
            {
                if (File.Exists(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.Watch + ".tst"))
                {
                    FileStream fileStream = null;
                    try
                    {
                        fileStream = new FileStream(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.Watch + ".tst", FileMode.Open);
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<MarketWatch>));
                        return Result = (List<MarketWatch>)xmlSerializer.Deserialize(fileStream);
                    }
                    catch (Exception)
                    {
                        Result = new List<MarketWatch>();
                        Result[0] = new AppClasses.MarketWatch();
                        return Result;
                    }
                    finally
                    {
                        if (fileStream != null)
                            fileStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "ReadXmlProfile")
                                           , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
            return Result;
        }

        #endregion
    }

    [Serializable]
    public class Leg
    {
        public uint GatewayId;
        public MTPackets.ContractInformation ContractInfo;
        public ContractDetails ContDetail = new ContractDetails();
        [XmlIgnore()]
        public MTBCastPackets.MarketPicture MarketPicture;

        [XmlIgnore]
        public int OrdFlag;
        [XmlIgnore]
        public decimal LastTradedPrice;
        [XmlIgnore]
        public Int32 LastTradedQty;
        [XmlIgnore]
        public double TotalQtyTraded;
        [XmlIgnore]
        public double TotalTradedValue;
        [XmlIgnore]
        public DateTime LastTradeTime;
        [XmlIgnore]
        public decimal AverageTradedPrice;
        [XmlIgnore]
        public Int32 TotalTrades;

        //[XmlIgnore]
        public double BuyPrice;

        [XmlIgnore]
        public double MidPrice;

        [XmlIgnore]
        public decimal BuyPrice1;

        [XmlIgnore]
        public decimal BuyPrice2;

        [XmlIgnore]
        public Int32 BQty;
        [XmlIgnore]
        public Int32 BQty1;
        [XmlIgnore]
        public Int32 BQty2;

        [XmlIgnore]
        public double TotalBuyQty;

        //[XmlIgnore]
        public double SellPrice;
        [XmlIgnore]
        public double BuyIV;
        [XmlIgnore]
        public double SellIV;

        [XmlIgnore]
        public decimal SellPrice1;

        [XmlIgnore]
        public decimal SellPrice2;

        [XmlIgnore]
        public Int32 SQty;
        
        [XmlIgnore]
        public Int32 SQty1;
        [XmlIgnore]
        public Int32 SQty2;
        [XmlIgnore]
        public double TotalSellQty;
        [XmlIgnore]
        public double TotalQty;
        [XmlIgnore]
        public decimal OpenPrice;
        [XmlIgnore]
        public decimal HighPrice;
        [XmlIgnore]
        public decimal LowPrice;
        [XmlIgnore]
        public Int32 CurrentOpenInterest;
        [XmlIgnore]
        public decimal YearlyHigh;
        [XmlIgnore]
        public decimal YearlyLow;
        [XmlIgnore]
        public decimal PerChange;
        [XmlIgnore]
        public decimal NetChange;
        [XmlIgnore]
        public DateTime LastUpdateTime;
        [XmlIgnore]
        public string Trend;
        [XmlIgnore]
        public string TradeSide;
        [XmlIgnore]
        public decimal OldLTP;
        [XmlIgnore]
        public double OldBuyPrice;
        [XmlIgnore]
        public double OldSellPrice;
        [XmlIgnore]
        public decimal OldHighPrice;
        [XmlIgnore]
        public decimal OldLowPrice;
        [XmlIgnore]
        public DataGridViewRow RowData;
        [XmlIgnore]
        public System.Threading.Timer tmrInitialOrder;
        public bool IsActive;
        public int NetQty;
        public string Format = "N2";
        public double InterestRate;
        public double B_Price;
        public double B_Value;
        public int B_Qty;
        public double S_Price;
        public double S_Value;
        public int S_Qty;
        public double N_Price;
        public int N_Qty;
        public double N_Value;
        public double A_Value;
        public int Ratio;
        public UInt64 expiryUniqueID;
        public int Counter;
        public UInt64 Sequence;
        public double DerivePrice;
        public double DeriveIV;
        public double DeriveDiff;
        public double BidDeriveDiff;
        public double BidDrivePrice;
        public int Net_Qty;
        public int Buy_Qty;
        public int Sell_Qty;
        public double DeltaV;
        public double VegaV;
        public double ThetaV;
        public double GammaV;
        public double ATP = 0;
    }

    public struct InputParameter
    {
        public int Lots;
        public double Price;
        public bool flg;
      
    }

    public class RuleParameter
    {
        public int Lots;
        public double Price;
        public string Side;
        public bool Preform;
    }

}
