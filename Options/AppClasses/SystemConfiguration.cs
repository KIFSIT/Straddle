using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Straddle
{
    /// <summary>
    /// System Configuration
    /// </summary>
    [Serializable]
    public class SystemConfiguration
    {
        [XmlElement]
        public string ApplicationName { get; set; }
        //[XmlElement]
        //public string NseMemberId { get; set; }
        //[XmlElement]
        //public string NseCmSebiCode { get; set; }
        //[XmlElement]
        //public string NseCmApiVersion { get; set; }
        //[XmlElement]
        //public bool EnableBroadcastNseCm { get; set; }
        [XmlElement]
        public string NseCmBroadcastIp { get; set; }
        [XmlElement]
        public int NseCmBroadcastPort { get; set; }
        //[XmlElement]
        //public string NseCmHostIp { get; set; }
        //[XmlElement]
        //public int NseCmHostPort { get; set; }
        //[XmlElement]
        //public int NseCmBranchId { get; set; }
        //[XmlElement]
        //public int NseCmCtclId { get; set; }
        //[XmlElement]
        //public double NseCmNnfId { get; set; }
        //[XmlElement]
        //public string NseFoSebiCode { get; set; }
        //[XmlElement]
        //public string NseFoApiVersion { get; set; }
        //[XmlElement]
        //public bool EnableBroadcastNseFo { get; set; }
        [XmlElement]
        public string NseFoBroadcastIp { get; set; }
        [XmlElement]
        public int NseFoBroadcastPort { get; set; }
        //[XmlElement]
        //public string NseFoHostIp { get; set; }
        //[XmlElement]
        //public int NseFoHostPort { get; set; }
        //[XmlElement]
        //public int NseFoBranchId { get; set; }
        //[XmlElement]
        //public int NseFoCtclId { get; set; }
        //[XmlElement]
        //public double NseFoNnfId { get; set; }

        [XmlElement]
        public string MarketDataIP { get; set; }
        [XmlElement]
        public int MarketDataPort { get; set; }


        //[XmlElement]
        //public string GuiIP { get; set; }
        //[XmlElement]
        //public int GuiPort { get; set; }


        [XmlElement]
        public string RMSIP { get; set; }
        [XmlElement]
        public int RMSPort { get; set; }

        [XmlElement]
        public int GUIid { get; set; }
        [XmlElement]
        public string UserName { get; set; }

        [XmlElement]
        public int Uniqueid { get; set; }

        [XmlElement]
        public string Gateway { get; set; }

        [XmlElement]
        public string Type { get; set; }

        [XmlElement]
        public bool RmsConnect { get; set; }

        [XmlElement]
        public string LogFilePath { get; set; }

        [XmlElement]
        public string SymbolFilter { get; set; }

        [XmlElement]
        public string BackUpPath_1 { get; set; }

        [XmlElement]
        public string BackUpPath_2 { get; set; }

        [XmlElement]
        public string BackUpFilePath { get; set; }

        [XmlElement]
        public string BackUpDailyTradeFilePath { get; set; }

        [XmlElement]
        public int EnterLots { get; set; }

        [XmlElement]
        public string AllowStrategy { get; set; }
         
        [XmlElement]
        public double NiftyButterflyMargin { get; set; }

        [XmlElement]
        public double BankNiftyButterflyMargin { get; set; }

        [XmlElement]
        public double NiftyButterflyExtraMargin { get; set; }

        [XmlElement]
        public double BankNiftyButterflyExtraMargin { get; set; }

        [XmlElement]
        public double Nifty1331Margin { get; set; }

        [XmlElement]
        public double BankNifty1331Margin { get; set; }

        [XmlElement]
        public double Nifty1331ExtraMargin { get; set; }

        [XmlElement]        
        public double BankNifty1331ExtraMargin { get; set; }

        [XmlElement]
        public double StrikeDifference { get; set; }

        [XmlElement]
        public double LossPoints { get; set; }

        [XmlElement]
        public double updateMin { get; set; }
 
    }
}
