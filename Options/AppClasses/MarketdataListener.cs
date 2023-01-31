using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;


namespace Straddle.AppClasses
{
    class MarketdataListener
    {
        public byte[] left_over = new byte[128];
        int left_over_len = 0;

        public class CSocketPacket
        {
            public Socket thisSocket;
            public byte[] dataBuffer;
            public CSocketPacket(int buffeLength)
            {
                dataBuffer = new byte[buffeLength];
            }
        }

        public const int BufferLength = 8192;
        public AsyncCallback pfnWorkerCallBack;
        Socket m_socWorker;

        public event AppGlobal.MKTTerminal_MessageRecivedDel MKTMessageRecived;
        public event AppGlobal.MKTTerminal_DisconnectDel Disconnected;
       // public event AppGlobal.MKTTerminal_ConnectDel MKTConnect;

        public void StartReciving(Socket socket)
        {
            m_socWorker = socket;
            WaitForData(socket);
        }

        public void StopListening()
        {
            // Incase connection has been established with remote client - 
            // Raise the OnDisconnection event.
            if (m_socWorker != null)
            {
                // m_socWorker.Shutdown(SocketShutdown.Both);                        
                m_socWorker.Close();
                m_socWorker = null;
            }
        }

        private void WaitForData(Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                CSocketPacket theSocPkt = new CSocketPacket(BufferLength);
                theSocPkt.thisSocket = soc;
                // now start to listen for any data...
                soc.BeginReceive(
                    theSocPkt.dataBuffer,
                    0,
                    theSocPkt.dataBuffer.Length,
                    SocketFlags.None,
                    pfnWorkerCallBack,
                    theSocPkt);
            }
            catch (SocketException)
            {
               
               // Debug.Fail(sex.ToString(), "WaitForData: Socket failed");
            }
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

        public void OnDataReceived(IAsyncResult asyn)
        {
            CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
            Socket socket = theSockId.thisSocket;
            if (!socket.Connected)
            {
                return;
            }
            try
            {
                int iRx;
                try
                {
                    iRx = socket.EndReceive(asyn);
                }
                catch (SocketException)
                {
                    //Debug.Write("Apperently client has been closed and cannot answer!");
                    OnConnectionDroped(socket);
                    return;
                }
                if (iRx == 0)
                {

                    return;
                }
                byte[] bytes1;
                UInt64 TransCode = 0;
                bytes1 = new byte[iRx + left_over_len];
                int start_index = 0;
                int end_index = bytes1.Length;
                if (left_over_len != 0)
                {
                    Buffer.BlockCopy(left_over, 0, bytes1, 0, left_over_len);
                }
                Buffer.BlockCopy(theSockId.dataBuffer, 0, bytes1, left_over_len, bytes1.Length - left_over_len);
                while (true)
                {
                    TransCode = BitConverter.ToUInt64(bytes1, 0);
                    if (TransCode == 4)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.FUTLtp)))
                        {
                            Buffer.BlockCopy(bytes1, 0, left_over, 0, end_index - start_index);                            
                            left_over_len = end_index - start_index;
                            break;
                        }
                        BTPacket.FUTLtp packetHeader = PinnedPacket<BTPacket.FUTLtp>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.FUTLtp));
                    }
                    else if (TransCode == 3)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.L4_LC_LP_UC_UP_SpreadBiddingUpdate)))
                        {
                            Buffer.BlockCopy(bytes1, 0, left_over, 0, end_index - start_index);
                            // Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        BTPacket.L4_LC_LP_UC_UP_SpreadBiddingUpdate packetHeader = PinnedPacket<BTPacket.L4_LC_LP_UC_UP_SpreadBiddingUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.L4_LC_LP_UC_UP_SpreadBiddingUpdate));
                    }
                    else if (TransCode == 10)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.TradeMessage)))
                        {
                            Buffer.BlockCopy(bytes1, 0, left_over, 0, end_index - start_index);
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        BTPacket.TradeMessage packetHeader = PinnedPacket<BTPacket.TradeMessage>(bytes1);
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.TradeMessage));
                    }
                    else if (TransCode == 430)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.ConRev)))
                        {
                            Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        BTPacket.ConRev packetHeader = PinnedPacket<BTPacket.ConRev>(bytes1);
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.ConRev));
                    }
                    else if (TransCode == 85)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.L4_LC_LP_UC_UP_SpreadBiddingUpdate)))
                        {
                            Buffer.BlockCopy(bytes1, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        BTPacket.L4_LC_LP_UC_UP_SpreadBiddingUpdate packetHeader = PinnedPacket<BTPacket.L4_LC_LP_UC_UP_SpreadBiddingUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.L4_LC_LP_UC_UP_SpreadBiddingUpdate));
                    }

                    else if (TransCode == 86)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.OI_Ticker)))
                        {
                            Buffer.BlockCopy(bytes1, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        BTPacket.OI_Ticker packetHeader = PinnedPacket<BTPacket.OI_Ticker>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        // RaiseMessageRecived(bytesToSend);
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.OI_Ticker));
                    }
                    else if (TransCode == 20)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.SpreadMarketUpdate)))
                        {
                            Buffer.BlockCopy(bytes1, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        BTPacket.SpreadMarketUpdate packetHeader = PinnedPacket<BTPacket.SpreadMarketUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                         RaiseMessageRecived(bytesToSend);
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.SpreadMarketUpdate));
                    }
                    else
                    {
                        Array.Clear(left_over, 0, 128);
                        left_over_len = 0;
                        break;
                        //TransactionWatch.TransactionMessage("transCode|" + TransCode + "|StartIndex|" + start_index + "|EndIndex|" + end_index, Color.Red);
                    }
                    Buffer.BlockCopy(theSockId.dataBuffer, start_index - left_over_len, bytes1, 0, bytes1.Length - start_index);
                    if (start_index == end_index)
                    {
                        Array.Clear(left_over, 0, 128);
                        left_over_len = 0;
                        break;
                    }
                }
                WaitForData(m_socWorker);
            }
            catch (Exception ex)
            {
               // Debug.Fail(ex.ToString(), "OnClientConnection: Socket failed");
            }
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
            catch (Exception)
            {

            }
            return (T)packet;
        }

        private void RaiseMessageRecived(byte[] bytes)
        {
            if (MKTMessageRecived != null)
            {
                MKTMessageRecived(m_socWorker, bytes);

            }
        }

        private void OnDisconnection(Socket socket)
        {
            if (Disconnected != null)
            {
                Disconnected(socket);
            }
        }

        private void OnConnectionDroped(Socket socket)
        {
            m_socWorker = null;
            OnDisconnection(socket);
        }
    }
}
