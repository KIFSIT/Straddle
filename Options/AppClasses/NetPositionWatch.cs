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

namespace Straddle.AppClasses
{
    public class NetPositionWatch
    {
        public Legx Leg;
        public string Symbol;
        public double avgPrice;
        public int posInt;
        public string posType;
        public int Strike1;
        public int Strike2;
        public int Strike3;
        public int Strike4;
        public double pnl;
        public double windAvg;
        public double unwindAvg;
        public string Expiry;
        public string Expiry2;


        public string Token1;
        public string Token2;
        public string Token3;
        public string Token4;
        public string StrategyName;
        public string Series;

        [XmlIgnore]
        public DataGridViewRow RowData;

        #region Read/Write
        public static void WriteXmlProfile(ref List<NetPositionWatch> watch)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<NetPositionWatch>));
                StreamWriter streamWriter = new StreamWriter(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.netWatch + ".tst");
                string date = DateTime.Now.ToString("ddMMMyyyy");
                StreamWriter streamWriterDaily = new StreamWriter(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.netWatch + date +".tst");
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

        public static List<NetPositionWatch> ReadXmlProfile()
        {
            List<NetPositionWatch> Result = new List<NetPositionWatch>();
            try
            {
                if (File.Exists(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.netWatch + ".tst"))
                {
                    FileStream fileStream = null;
                    try
                    {
                        fileStream = new FileStream(MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.netWatch + ".tst", FileMode.Open);

                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<NetPositionWatch>));
                        return Result = (List<NetPositionWatch>)xmlSerializer.Deserialize(fileStream);
                    }
                    catch (Exception)
                    {
                        Result = new List<NetPositionWatch>();
                        Result[0] = new AppClasses.NetPositionWatch();
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
                TransactionWatch.ErrorMessage("File not found " + MTClientEnvironment.SpecialFolder.CurrentDirectory + AppGlobal.netWatch + ".tst");
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "ReadXmlProfile")
                                           , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
            return Result;
        }

        #endregion
    }
    public class Legx
    {
        public UInt64 uniqueId;
        public string displayUniqueId;
        public string Wind;
        public string UnWind;
       
        public double TrdPrice;
        public double Netting;
        public string IsWind;


        public int B_Qty;
        public double B_Value;
        public int S_Qty;
        public double S_Value;
        public int net_Qty;

        public double N_Price;

       
    }
}
