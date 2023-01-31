using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ArisDev
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
        //[XmlElement]
        //public string NseCmBroadcastIp { get; set; }
        //[XmlElement]
        //public int NseCmBroadcastPort { get; set; }
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
        //[XmlElement]
        //public string NseFoBroadcastIp { get; set; }
        //[XmlElement]
        //public int NseFoBroadcastPort { get; set; }
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
    }
}
