using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Sockets;
using ArisDev.Api;
using System.Drawing;
using Disruptor;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;



namespace Straddle.AppClasses
{
    
    public class Connection
    {
        
        public Tcp _tcpRMS;
        public Tcp _tcpGUIPort;
        private Socket M_Socket;

        private Socket RMS_Socket;

        public delegate void MarketDepthUpdateDelegate(BTPacket.FUTLtp _response);
        public event MarketDepthUpdateDelegate OnMarketDepthUpdate;

        public delegate void MarketSpreadUpdateDelegate(byte[] _response);
        public event  MarketSpreadUpdateDelegate OnSpreadUpdate;

        public delegate void TradeInfoDelegate(BTPacket.GUIUpdate _Trdupdate3L);
        public event TradeInfoDelegate OnTradeUpdate;

     

       


        public event AppGlobal.MKTTerminal_MessageRecivedDel MKTMessageRecived;
        public event AppGlobal.MKTTerminal_ConnectDel MKTClientConnect;
        public event AppGlobal.MKTTerminal_DisconnectDel MKTClientDisconnect;


        public event AppGlobal.RMSTerminal_MessageRecivedDel RMSMessageRecived;
        //public event AppGlobal.RMSTerminal_ConnectDel RMSClientConnect;
        //public event AppGlobal.RMSTerminal_DisconnectDel RMSClientDisconnect;

        //public static Dictionary<long, MarketdataConnected> m_Ser =
        //new Dictionary<long, MarketdataConnected>();





        public System.Timers.Timer timer = new System.Timers.Timer();

        public void setTcpMdSocket()
        {
            // ArisDev.ArisApi_a._arisApi.SystemConfig.
            // _dataLock = new object();
            //_tcpMarketData = new Tcp("127.0.0.1", 5000, "", TypeOfCompression.None);
            //_tcpMarketData = new Tcp("168.17.2.66", 6661, "", TypeOfCompression.None);
            //_tcpMarketData.DataArrival += new Tcp.DataArrivalHandler(_tcpMarketData_DataArrival);
            //_tcpMarketData.Connect += new Tcp.ConnectHandler(_tcpMarketData_Connect);
            //_tcpMarketData.Disconnect += new Tcp.DisconnectHandler(_tcpMarketData_Disconnect);
            //_tcpMarketData.ConnectedTo();



            IPAddress ip = IPAddress.Parse(ArisApi_a._arisApi.SystemConfig.MarketDataIP);
            IPEndPoint ipLocal = new IPEndPoint(ip, ArisApi_a._arisApi.SystemConfig.MarketDataPort);
            M_Socket = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream, ProtocolType.Tcp);
            M_Socket.BeginConnect(ipLocal, new AsyncCallback(OnSeverConnection), M_Socket);

        }

        private void OnSeverConnection(IAsyncResult asyn)
        {
            Socket ClientSocket = (Socket)asyn.AsyncState;
            if (ClientSocket.Connected)
            {
                ClientSocket.EndConnect(asyn);
            }
            MKTRaiseClientConnected(ClientSocket);
            MarketdataConnected mkt = new MarketdataConnected(ClientSocket);
            mkt.MKTMessageRecived += OnMKTMessageRecived;
            mkt.MKTClientDisconnect += mktConnect_Disconnected;
            mkt.StartListen();
            // checking heartbeat
            //long key = ClientSocket.Handle.ToInt64();
            //if (m_Serv.ContainsKey(key))
            //{
            //    Debug.Fail(string.Format(
            //        "Client with handle key '{0}' already exist!", key));
            //}
            //m_Serv[key] = mkt;

            //timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            //timer.Interval = 1500;
            //timer.Start();
            

        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (AppGlobal.heartbeat)
            {
                //foreach (MarketdataConnected connectedClient in AppGlobal.m_Servers.Values)
                //{
                //    BTPacket.HeartBeat snd = new BTPacket.HeartBeat();
                //    snd.TransCode = 60;
                //    snd.counter = 1;
                //    byte[] bytesToSend = AppGlobal.frmWatch.StructureToByte(snd);
                //    connectedClient.Send(bytesToSend);
                //}
            }
        }

        void mktConnect_Disconnected(Socket socket)
        {

        }

        void OnMKTMessageRecived(Socket socket, byte[] message)
        {
            if (MKTMessageRecived != null)
            {
                MKTMessageRecived(socket, message);
            }
        }

        private void MKTRaiseClientConnected(Socket socket)
        {
            if (MKTClientConnect != null)
            {
                MKTClientConnect(socket);
            }
        }


        void _tcpMarketData_Disconnect(ConnectionData connectionData)
        {
            TransactionWatch.TransactionMessage("Market Data Server is Closed", Color.Red);
        }

        public void StartServer()
        {
            //_tcpGUIPort = new Tcp("127.0.0.1", 5001, "", TypeOfCompression.None);
            //_tcpGUIPort = new Tcp("172.16.2.201", 2331, "", TypeOfCompression.None);
            //_tcpGUIPort.DataArrival += new Tcp.DataArrivalHandler(_tcpGUIPort_DataArrival);
            //_tcpGUIPort.Connect += new Tcp.ConnectHandler(_tcpGUIPort_Connect);
            //_tcpGUIPort.Accept += new Tcp.AcceptHandler(_tcpGUIPort_Accept);
            //_tcpGUIPort.Disconnect += new Tcp.DisconnectHandler(_tcpGUIPort_Disconnect);
            //_tcpGUIPort.ListeningStart();

            //_tcpGUIPort = new Tcp(ArisDev.ArisApi_a._arisApi.SystemConfig.GuiIP, ArisDev.ArisApi_a._arisApi.SystemConfig.GuiPort, "", TypeOfCompression.None);
            //_tcpGUIPort.DataArrival += new Tcp.DataArrivalHandler(_tcpGUIPort_DataArrival);
            //_tcpGUIPort.Connect += new Tcp.ConnectHandler(_tcpGUIPort_Connect);
            //_tcpGUIPort.Accept += new Tcp.AcceptHandler(_tcpGUIPort_Accept);
            //_tcpGUIPort.Disconnect += new Tcp.DisconnectHandler(_tcpGUIPort_Disconnect);
            //_tcpGUIPort.ListeningStart();
        }

        void _tcpGUIPort_Disconnect(ConnectionData connectionData)
        {
           
        }

        void _tcpGUIPort_Accept(ConnectionData connectionData)
        {
                TransactionWatch.TransactionMessage("GUI Port Connected...",Color.Red);  
                //AppGlobal.connection.setTcpRmsSocket();
                AppGlobal.connection._setRMSConnection();
        }

        void _tcpGUIPort_Connect(ConnectionData connectionData)
        {
          
        }

        void _tcpGUIPort_DataArrival(ConnectionData connectionData)
        {
         //   BTPacket.MessageHeader messageHeader = PinnedPacket<BTPacket.MessageHeader>(connectionData.RecieveData);
            //BTPacket.GUIUpdate packetHeader1 = PinnedPacket<BTPacket.GUIUpdate>(connectionData.RecieveData);
            //OnguiModifyEntry(packetHeader1);
        }
        void _tcpMarketData_Connect(ConnectionData connectionData)
        {
            TransactionWatch.TransactionMessage("Market Data Server is Open", Color.Red);
        }

        void _tcpMarketData_DataArrival(ConnectionData connectionData)
        {
            try
            {
                byte[] bytes1 = new byte[40];
                Buffer.BlockCopy(connectionData.RecieveData, 0, bytes1, 0, bytes1.Length);
                UInt64 messageCode = BitConverter.ToUInt64(bytes1, 0);
                UInt64 SequenceNo = BitConverter.ToUInt64(bytes1, 32);
                switch (messageCode)
                {
                    case (UInt64)EnumMarketUpdate.L4_LC_LP_UC_UP_SpreadBiddingUpdate:
                       OnSpreadUpdate(bytes1);
                        break;
                    case (UInt64)EnumMarketUpdate.FUTLtp:
                        BTPacket.FUTLtp packetHeader1 = PinnedPacket<BTPacket.FUTLtp>(bytes1);
                        OnMarketDepthUpdate(packetHeader1);
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        public void setTcpRmsSocket()
        {           
            _tcpRMS = new Tcp(ArisApi_a._arisApi.SystemConfig.RMSIP, ArisApi_a._arisApi.SystemConfig.RMSPort, "", TypeOfCompression.None);
            _tcpRMS.DataArrival += new Tcp.DataArrivalHandler(_tcpRMS_DataArrival);
            _tcpRMS.Connect += new Tcp.ConnectHandler(_tcpRMS_Connect);
            _tcpRMS.Disconnect += new Tcp.DisconnectHandler(_tcpRMS_Disconnect);
            _tcpRMS.ConnectedTo();
        }

        public void _setRMSConnection()
        {
            IPAddress ip = IPAddress.Parse(ArisApi_a._arisApi.SystemConfig.RMSIP);
            IPEndPoint ipLocal = new IPEndPoint(ip, ArisApi_a._arisApi.SystemConfig.RMSPort);
            RMS_Socket = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream, ProtocolType.Tcp);
            RMS_Socket.BeginConnect(ipLocal, new AsyncCallback(OnRMSConnection), RMS_Socket);
        }


        private void OnRMSConnection(IAsyncResult asyn)
        {
            Socket ClientSocket = (Socket)asyn.AsyncState;
            if (ClientSocket.Connected)
            {
                ClientSocket.EndConnect(asyn);
            }
            MKTRaiseClientConnected(ClientSocket);
            RMSSendSocketHandler RMScon = new RMSSendSocketHandler(ClientSocket);
            RMScon.RMSMessageRecived += OnRMSMessageRecived;
            RMScon.RMSClientDisconnect += mktConnect_Disconnected;
            RMScon.StartListen();

            long key = ClientSocket.Handle.ToInt64();
            if (AppGlobal.R_clients.ContainsKey(key))
            {
                //Debug.Fail(string.Format(
                    //"Client with handle key '{0}' already exist!", key));
            }
            AppGlobal.R_clients[key] = RMScon;

            BTPacket.GUIUpdate snd = new BTPacket.GUIUpdate();
            snd.TransCode = 98;
            snd.WindPos = ArisApi_a._arisApi.SystemConfig.Uniqueid;
            TransactionWatch.TransactionMessage("Unique Id " + ArisApi_a._arisApi.SystemConfig.Uniqueid,Color.Red);


            byte[] bytesToSend = StructureToByte(snd);
            RMScon.Send(bytesToSend);
           
        }


        void OnRMSMessageRecived(Socket socket, byte[] message)
        {
            if (RMSMessageRecived != null)
            {
                RMSMessageRecived(socket, message);
            }
        }


        void _tcpRMS_Disconnect(ConnectionData connectionData)
        {
            TransactionWatch.TransactionMessage("RMS Server is Closed", Color.Red);
        }

        void _tcpRMS_Connect(ConnectionData connectionData)
        {
           
            
        }

        void _tcpRMS_DataArrival(ConnectionData connectionData)
        {
            BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(connectionData.RecieveData);
            OnTradeUpdate(packetHeader);
        }

        internal static byte[] StructureToByte(object packet)
        {
            try
            {

                int length = Marshal.SizeOf(packet);
                byte[] data = new byte[length];
                IntPtr intPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(packet, intPtr, true);
                Marshal.Copy(intPtr, data, 0, length);
                Marshal.FreeHGlobal(intPtr);
                return data;
            }
            catch (Exception)
            {

            }
            return null;
        }

        public T PinnedPacket<T>(byte[] data)
        {
            object packet = new object();
            try
            {
                //int length = Marshal.SizeOf(data);
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr IntPtrOfObject = handle.AddrOfPinnedObject();
                packet = Marshal.PtrToStructure(IntPtrOfObject, typeof(T));
                handle.Free();
            }
            catch (Exception )
            {

            }
            return (T)packet;
        }
        public void SendData(object packet)
        {
            byte[] bytesToSend = StructureToByte(packet);            
            foreach (RMSSendSocketHandler connectedClient in AppGlobal.R_clients.Values)
            {
                connectedClient.Send(bytesToSend);
            }
        }


         #region Member variables
        // private object _dataLock;
         #endregion

    }
    public class PacketProcess
    {
        /// <summary>
        /// 
        /// </summary>
        public object PacketNotification { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// 
    public class HandleTradeNotifications : IEventHandler<PacketProcess>
    {
        public HandleTradeNotifications()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sequence"></param>
        /// <param name="endofBatch"></param>
        /// 

        public void OnNext(PacketProcess obj, long sequence, bool endofBatch)
        {
            try
            {
                AppGlobal.connection.SendData(obj.PacketNotification);
            }
            catch (Exception)
            {

                
            }
        }
    }
}
