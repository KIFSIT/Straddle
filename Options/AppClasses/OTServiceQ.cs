using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using MTApi;
using System.IO;

namespace Straddle.AppClasses
{
    public struct ordKey
    {
        public int key;
        public MTPackets.OrderRequest trade;
    }
    public class OTServiceQ : IDisposable
    {
        #region Variables
        readonly EventWaitHandle waitHandler;
        readonly Thread Worker;
        readonly Thread Worker2;
        ConcurrentQueue<ordKey> Tasks;
        ConcurrentQueue<ordKey> Tasks2;
        object TLock;
        object TLock2;
        public bool trdFlag;

        #endregion

        #region Constructor
        public OTServiceQ()
        {
            trdFlag = true;
            TLock = new object();
            TLock2 = new object();
            waitHandler = new AutoResetEvent(false);
            Tasks = new ConcurrentQueue<ordKey>();
            Tasks2 = new ConcurrentQueue<ordKey>();
            Worker = new Thread(Work);
            Worker.Start();
            Worker2 = new Thread(Work2);
            Worker2.Start();           
        }
        #endregion

        public void AddPacket(MTPackets.OrderRequest order,int i)
        {           
                try
                {
                    try
                    {
                        MTApi.MTPackets.OrderRequest o = new MTApi.MTPackets.OrderRequest();
                        o = order;
                        ordKey ok = new ordKey();
                        ok.trade = o;
                        ok.key = i;                        
                        Tasks.Enqueue(ok);                        
                    }
                    catch (Exception)
                    {
                        //AppGlobal.Logger.WriteToErrorLogFile("Writing queue - One to One assignment error.");
                        ordKey ok = new ordKey();
                        ok.trade = order;
                        ok.key = i;       
                        Tasks.Enqueue(ok);
                    }
                    waitHandler.Set();                    
                }
                catch (Exception)
                {
                   // AppGlobal.Logger.WriteToErrorLogFile("Add Packet Error : " + ex.StackTrace);
                }
            
        }

        public void AddSpread(MTPackets.OrderRequest order, int i)
        {
            try
            {
                try
                {
                    MTApi.MTPackets.OrderRequest o = new MTApi.MTPackets.OrderRequest();
                    o = order;
                    ordKey ok = new ordKey();
                    ok.trade = o;
                    ok.key = i;
                    Tasks2.Enqueue(ok);
                }
                catch (Exception)
                {
                    //AppGlobal.Logger.WriteToErrorLogFile("Writing spread queue - One to One assignment error.");
                    ordKey ok = new ordKey();
                    ok.trade = order;
                    ok.key = i;
                    Tasks2.Enqueue(ok);
                }
                waitHandler.Set();
            }
            catch (Exception)
            {
                //AppGlobal.Logger.WriteToErrorLogFile("Add Spread Error : " + ex.StackTrace);
            }

        }

        public void Dispose()
        {
            Worker.Join();
            Worker2.Join();
            waitHandler.Close();            
        }

        void Work()
        {
            while (trdFlag)
            {
                
                if (Tasks.Count > 0 )
                {
                    lock (TLock)
                    {
                        try
                        {
                            ordKey order;
                            Tasks.TryDequeue(out order);
                           
                            if (order.trade.IntOrderNo != 0)
                            {
                              //  OrderTradeWriter.OrderTradeProcess(order);
                            }
                        }
                        catch (Exception )
                        {
                           // AppGlobal.Logger.WriteToErrorLogFile("Dequeue error : " + ex.StackTrace);
                        }
                    }
                }
                else
                    waitHandler.WaitOne(1);
            }
        }

        void Work2()
        {
            while (trdFlag)
            {

                if (Tasks2.Count > 0)
                {
                    lock (TLock2)
                    {
                        try
                        {
                            ordKey order;
                            Tasks2.TryDequeue(out order);

                            if (order.trade.IntOrderNo != 0)
                            {                                
                                //OrderTradeWriter.SpreadProcess(order);
                            }
                        }
                        catch (Exception)
                        {
                           // AppGlobal.Logger.WriteToErrorLogFile("Dequeue  Spread error : " + ex.StackTrace);
                        }
                    }
                }
                else
                    waitHandler.WaitOne(1);
            }
        }

        public  string ReadFileContents(string fileName)
        {
            string line = "";
            bool isReadComp = false;
            while (!isReadComp)
            {
                lock (TLock)
                {
                    FileStream oStreamTrade = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                    StreamReader swTrade = new StreamReader(oStreamTrade);
                    line = swTrade.ReadToEnd();
                    isReadComp = true;
                    swTrade.Close();
                    oStreamTrade.Close();
                }
            }
            return line;
        }
    }
}
