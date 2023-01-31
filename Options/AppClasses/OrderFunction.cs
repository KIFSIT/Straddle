using System;
using System.Linq;
using System.Windows.Forms;
using LogWriter;
using MTCommon;
using ArisDev;

namespace Straddle.AppClasses
{
    internal class OrderFunction
    {
        #region Common

        /// <summary>
        /// Send Cancel order request when strategy stops 
        /// OR
        /// Upper and lower limit reached
        /// </summary>
        /// <param name="rowindex">Script Index</param>
        /// 

        public static void CancelOrderOnDeActive(int rowindex)
        {
            try
            {
                if (!AppGlobal.MarketWatch[rowindex].IsActive) return;
                var temp = from ord in AppGlobal.OrdStrategy.Keys
                           where (AppGlobal.OrdStrategy[ord].Rowindex == rowindex
                                  && AppGlobal.OrdStrategy[ord].Response.OrderStatus == (byte)MTEnums.OrderStatus.EPending)
                           select AppGlobal.OrdStrategy[ord].Response;

                foreach (var item in temp)
                {
                    ushort key = item.IntOrderNo;//MTUtils.GetKeyCode(item.UniqueId, item.IntOrderNo);

                    if (ArisApi_a._arisApi.OrderCollection.ContainsKey(key) &&
                         !ArisApi_a._arisApi.OrderCollection[key].IsCancelSend)
                    {
                        //ArisApi_a._arisApi.CancelOrderRequest(item.IntOrderNo, item.UniqueId);
                    }
                }
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "CancelOrderOnDeActive")
                                          , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
            }
        }

        #endregion

    }
}
