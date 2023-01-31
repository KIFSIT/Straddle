using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTApi;
using MTCommon;

namespace Straddle.AppClasses
{

    /// <summary>
    /// 
    /// </summary>
    public class OrderRefrence
    {
        public int Rowindex;
        public MTPackets.OrderRequest Request;
        public MTPackets.OrderRequest Response;
        public OrderType RequestType;
        public MTOrderInfo OrdInfo;
        public int EntryTime;
        public int OrdQty;
    }
    /// <summary>
    /// Used for Order when usgin from book or marketwatch
    /// </summary>
    public enum OrderType : short
    {
        None = 0,
        InitialOrder = 1,
        NormalSLOrder=2,
        NormalRLOrder = 3,
        SendReverseOrder = 4,
        MarketOrder = 5,
    }

    public enum CalculationMethod : short
    {
        None = 0,
        IVOrder = 1,
        ITMOrder = 2,
    }

    public enum BuySellVol : short
    {
        BothVol = 0,
        BuyVol = 1,
        SellVol =2,
    }
}
