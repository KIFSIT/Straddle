using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using MTCommon;
using LogWriter;
using ClientCommon;
using System.Windows.Forms;
using MTApi;

namespace Straddle.AppClasses
{
    [Serializable]
    public class AnalysisWatch
    {
        #region CashFuture Trader
        public bool IsActive;
        public bool IsStrikeReq;
        public string AccountType;

        public AnalysisLeg Leg1;

        public string Strategy;
        public string StrategyName;

        [XmlIgnore]
        public DataGridViewComboBoxCell ColumnCombo;

        [XmlIgnore]
        public DataGridViewRow RowData;

        [XmlIgnore]
        public decimal OldWind;

        [XmlIgnore]
        public decimal OldUnWind;

        #endregion

        #region Read/Write
        public static void WriteXmlProfile(ref List<AnalysisWatch> watch)
        {
            try
            {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<AnalysisWatch>));
                StreamWriter streamWriter = new StreamWriter(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.AnaWatch + ".tst");
             
                xmlSerializer.Serialize(streamWriter, watch);
                streamWriter.Close();
               
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "WriteXmlProfile")
                                          , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }



        public static List<AnalysisWatch> ReadXmlProfile()
        {
            List<AnalysisWatch> Result = new List<AnalysisWatch>();
            try
            {
                if (File.Exists(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.AnaWatch + ".tst"))
                {
                    FileStream fileStream = null;
                    try
                    {
                        fileStream = new FileStream(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.AnaWatch + ".tst", FileMode.Open);
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<AnalysisWatch>));
                        return Result = (List<AnalysisWatch>)xmlSerializer.Deserialize(fileStream);
                    }
                    catch (Exception)
                    {
                        Result = new List<AnalysisWatch>();
                        Result[0] = new AppClasses.AnalysisWatch();
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
    public class AnalysisLeg
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

        [XmlIgnore]
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

        [XmlIgnore]
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

        // Maintain BQty , SQty and NQty

        public int Net_Qty;
        public int Buy_Qty;
        public int Sell_Qty;


        public double DeltaV;
        public double VegaV;
        public double ThetaV;
        public double GammaV;



    }
}
