using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Straddle.AppClasses
{
    class MarketdataConnected
    {
         private Socket m_clientSocket;
        MarketdataListener m_listener;

        public event AppGlobal.MKTTerminal_MessageRecivedDel MKTMessageRecived
        {
            add
            {
                m_listener.MKTMessageRecived += value;
            }
            remove
            {
                m_listener.MKTMessageRecived -= value;
            }
        }

        public event AppGlobal.MKTTerminal_DisconnectDel MKTClientDisconnect
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

        public MarketdataConnected(Socket clientSocket)
        {
            m_clientSocket = clientSocket;

            m_listener = new MarketdataListener();
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
