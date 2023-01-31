using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace Straddle.AppClasses
{
    class RMSSendSocket
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

        public event AppGlobal.RMSTerminal_MessageRecivedDel RMSMessageRecived;
        public event AppGlobal.RMSTerminal_DisconnectDel Disconnected;
        

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
                TransactionWatch.ErrorMessage("WaitForData: Socket failed");
                //Debug.Fail(sex.ToString(), "WaitForData: Socket failed");
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
                TransactionWatch.ErrorMessage("Intermediate Connection is not Connected!!!");
                TransactionWatch.TransactionMessage("Intermediate Connection is not Connected!!!", Color.Blue);
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
                    TransactionWatch.ErrorMessage("Intermediate Connection issue occured!!!");
                    TransactionWatch.TransactionMessage("Intermediate Connection issue occured!!!", Color.Blue);
                    OnConnectionDroped(socket);
                    return;
                }
                if (iRx == 0)
                {
                    TransactionWatch.ErrorMessage("Intermediate Connection is Droped!!!");
                    TransactionWatch.TransactionMessage("Intermediate Connection is Droped!!!", Color.Blue);
                    OnConnectionDroped(socket);
                    return;
                }
                // byte[] bytes = theSockId.dataBuffer;
                byte[] bytes1;
                UInt64 TransCode = 0;
                bytes1 = new byte[iRx + left_over_len];
                int start_index = 0;
                int remainingSize = bytes1.Length;
                int end_index = bytes1.Length;
                if (left_over_len != 0)
                {
                    Buffer.BlockCopy(left_over, 0, bytes1, 0, left_over_len);
                }
                Buffer.BlockCopy(theSockId.dataBuffer, 0, bytes1, left_over_len, bytes1.Length - left_over_len);
                while (true)
                {
                    TransCode = BitConverter.ToUInt64(bytes1, 0);                    
                    if (TransCode == 5)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {                            
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);                                               
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        if(AppGlobal.GUI_ID == packetHeader.gui_id)
                            TransactionWatch.ErrorMessage(packetHeader.toString());                        
                    }
                    else if (TransCode == 1)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;

                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);                        
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        if (AppGlobal.GUI_ID == packetHeader.gui_id)
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                    }
                    else if (TransCode == 2)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;

                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);

                        RaiseMessageRecived(bytesToSend);

                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        if (AppGlobal.GUI_ID == packetHeader.gui_id)
                            TransactionWatch.ErrorMessage(packetHeader.toString());
                    }
                    else if (TransCode == 9)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;

                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        if (AppGlobal.GUI_ID == packetHeader.gui_id)
                            TransactionWatch.ErrorMessage(packetHeader.toString());

                    }
                    else if (TransCode == 7)//  send netposition from server to intermediate and back to client
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        //RaiseMessageRecived(bytesToSend);
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        TransactionWatch.ErrorMessage(packetHeader.toString());
                       
                    }
                    else if (TransCode == 99)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);

                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        TransactionWatch.ErrorMessage(packetHeader.toString());
                    }
                    else if (TransCode == 19)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        TransactionWatch.ErrorMessage(packetHeader.toString());
                       
                    }

                    else if (TransCode == 20)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        TransactionWatch.ErrorMessage(packetHeader.toString());
                        TransactionWatch.TransactionMessage("User id | " + packetHeader.Token + " Limit Hit", Color.Blue);
                        //AppGlobal.frmWatch.lblLimitHit.Text = "User id | " + packetHeader.Token + " Limit Hit";       
                    }
                    else if (TransCode == 21)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }

                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        
                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);

                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        TransactionWatch.ErrorMessage(packetHeader.toString());
                    }
                    else if (TransCode == 11)
                    {
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }

                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);

                        byte[] bytesToSend = StructureToByte(packetHeader);
                        RaiseMessageRecived(bytesToSend);
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        TransactionWatch.ErrorMessage(packetHeader.toString());
                    }
                    else if (TransCode == 22)
                    {

                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);
                            left_over_len = end_index - start_index;
                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        BTPacket.GUIUpdate packetHeader = PinnedPacket<BTPacket.GUIUpdate>(bytes1);
                        //byte[] bytesToSend = StructureToByte(packetHeader);                       
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
                        AppGlobal.currentHeartBeat = packetHeader.Open;
                        TransactionWatch.ErrorMessage("Got Heart beat from RMS | " + packetHeader.Open);
                        TransactionWatch.TransactionMessage("Got Heart beat from RMS | " + packetHeader.Open,Color.Blue);
                    }
                    else
                    {
                        // TransactionWatch.TransactionMessage(start_index.ToString() + " | " + bytes1.Length, Color.Blue);
                        TransactionWatch.ErrorMessage("|NotHandledTransCode=" + TransCode);
                        if (end_index - start_index < Marshal.SizeOf(typeof(BTPacket.GUIUpdate)))
                        {
                            //Buffer.BlockCopy(theSockId.dataBuffer, iRx - (end_index - start_index), left_over, 0, end_index - start_index);
                            Buffer.BlockCopy(theSockId.dataBuffer, 0, left_over, 0, end_index - start_index);

                            left_over_len = end_index - start_index;
                            break;
                        }
                        start_index = start_index + Marshal.SizeOf(typeof(BTPacket.GUIUpdate));

                        // TransactionWatch.ErrorMessage("AcutalSize|" + iRx + "|RemainingSize|" + (iRx - remainingSize) + "|trans code : " + packetHeader.toString());
                        remainingSize = remainingSize - Marshal.SizeOf(typeof(BTPacket.GUIUpdate));
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
                TransactionWatch.ErrorMessage(" OnDataReceived: Socket failed ");
                //Debug.Fail(ex.ToString(), "OnClientConnection: Socket failed");
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
            if (RMSMessageRecived != null)
            {
                RMSMessageRecived(m_socWorker, bytes);

            }
        }

        private void OnDisconnection(Socket socket)
        {
            if (Disconnected != null)
            {
                TransactionWatch.ErrorMessage("OnDisconnection RMS");
                AppGlobal.frmWatch.CrashRMS.Text = "OFF";
                TransactionWatch.TransactionMessage("Trading is disconnected!!!", Color.Blue);
                Disconnected(socket);
            }
        }

        private void OnConnectionDroped(Socket socket)
        {
            AppGlobal.frmWatch.CrashRMS.Text = "OFF";
            m_socWorker = null;
            OnDisconnection(socket);
        }
    }
}
