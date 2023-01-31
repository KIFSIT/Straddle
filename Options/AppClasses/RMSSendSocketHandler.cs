using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Straddle.AppClasses
{
    public class RMSSendSocketHandler
    {
        private Socket m_clientSocket;
        RMSSendSocket m_listener;

        public event AppGlobal.RMSTerminal_MessageRecivedDel RMSMessageRecived
        {
            add
            {
                m_listener.RMSMessageRecived += value;
            }
            remove
            {
                m_listener.RMSMessageRecived -= value;
            }
        }

        public event AppGlobal.RMSTerminal_DisconnectDel RMSClientDisconnect
        {
            add
            {
                m_listener.Disconnected += value;
            }
            remove
            {
                m_listener.Disconnected -= value;
            }
        }

        public RMSSendSocketHandler(Socket clientSocket)
        {
            m_clientSocket = clientSocket;

            m_listener = new RMSSendSocket();
        }

        public void StartListen()
        {
            m_listener.StartReciving(m_clientSocket);
        }

        public void Send(byte[] buffer)
        {
            if (m_clientSocket == null)
            {
                throw new Exception("Can't send data. ConnectedClient is Closed!");
            }
            m_clientSocket.Send(buffer);

        }

        public void Stop()
        {
            m_listener.StopListening();
            m_clientSocket = null;
        }


    }
}
