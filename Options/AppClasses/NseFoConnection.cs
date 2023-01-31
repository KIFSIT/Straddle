using System;
using System.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using Sockets;
using System.Net;
using ArisDev.NseFoApi.Broadcast;
using ArisDev;
using System.Linq;
using System.Windows.Forms;
using Disruptor;
using Straddle.NseFoApi.DownloadProcess;
using Straddle.NseFoApi.LogonProcess;
using Straddle.NseFoApi.Header;
using Straddle.NseFoApi;
using Straddle.NseFoApi.UnsolicitedMessages;
using Straddle.AppClasses;

namespace Straddle.AppClasses
{
    public class NseFoConnection
    {
        #region Constructor

        internal NseFoConnection(string ctclId, string tradingMemberId, short branchId, long nnfId, string password, string newPassword)
        {
            //_branchId = branchId;
            //_nnfId = nnfId;
            //LogonRequest logonRequest = new LogonRequest();
            //logonRequest.UserID = Convert.ToInt32(ctclId);
            //logonRequest.Password = password;
            //logonRequest.NewPassword = newPassword;
            //logonRequest.TraderName = tradingMemberId;
            //logonRequest.BrokerID = tradingMemberId;
            //logonRequest.VersionNumber = int.Parse(ArisApi_a._arisApi.SystemConfig.NseFoApiVersion);
            //logonRequest.UserType = (short)UserType.Dealer;
            //ConnectToSocket(logonRequest);
        }

        #endregion

        #region Tcp Sockets Method

        private void ConnectToSocket(LogonRequest logonRequest)
        {
            try
            {
                _isDownloadCompleted = false;
                _logonRequest = logonRequest;

                _dataLock = new object();
                _sequenceLock = new object();
                _recSequenceLock = new object();
                _invitationLock = new object();
                //_tcp = new Tcp(ArisApi_a._arisApi.SystemConfig., ArisApi_a._arisApi.SystemConfig.NseFoHostPort, AppDomain.CurrentDomain.BaseDirectory, TypeOfCompression.None);
                //_tcp.Connect += _tcp_Connect;
                //_tcp.DataArrival += _tcp_DataArrival;
                //_tcp.Disconnect += _tcp_Disconnect;
                //_tcp.Error += _tcp_Error;
                //_tcp.ConnectedTo();

                if (!string.IsNullOrEmpty(_tcp.ComressLibError))
                {
                    //error
                }
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void _tcp_Error(string error)
        {
            //LogWriter.WriteToErrorLog(MethodBase.GetCurrentMethod().DeclaringType.Name + GeneralConstants.COLON +
            //    MethodBase.GetCurrentMethod().Name + GeneralConstants.COLON + error);

            //if (error.Contains("A request to send or receive data was disallowed"))
            //{
            //    MessageInfo messageInfo = new MessageInfo();
            //    messageInfo.Message = "Connection Busy Error. Check Connection Integrity NseFo!";
            //    messageInfo.Icon = MessageBoxIcon.Error;
            //    Global.Main.MessageThread(messageInfo);

            //    if (Global.Main.LoginDetail.Login != null)
            //        Global.Main.LoginDetail.Login.EnableControl(true);
            //}
            //else if (error == "An existing connection was forcibly closed by the remote host" ||
            //    error == "An established connection was aborted by the software in your host machine" ||
            //    error.Contains("Cannot access a disposed object"))
            //{
            //    MessageInfo messageInfo = new MessageInfo();
            //    messageInfo.Message = "Connection close from NseFo!\nPlease, Login again !";
            //    messageInfo.Icon = MessageBoxIcon.Error;
            //    Global.Main.MessageThread(messageInfo);
            //}
        }

        private void _tcp_Connect(ConnectionData connectionData)
        {
            try
            {
                _logonRequest.WsClassName = _tcp.LocalHostIpAddress;
                _isDownloadCompleted = false;

                Thread thread = new Thread(SendData);
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.Highest;
                thread.Start(_logonRequest);

                //LogWriter.WriteToConnectionLog("Manager connected to " + Market.NseFO + GeneralConstants.COLON +
                //    _logonRequest.UserID + GeneralConstants.COLON + _tcp.RemoteHostIpAddress + GeneralConstants.COLON + _tcp.RemoteHostPort);
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void _tcp_Disconnect(ConnectionData connectionData)
        {
            try
            {
                ArisApi_a._Connections.Remove(_logonRequest.UserID);
                ArisApi_a._arisApi.OnLogonStatusChangedProcess(2, false, "");
                ArisApi_a._arisApi.OnSystemUpdateProcess("NseFo : " + _logonRequest.UserID + " Disconnected from the exchange");
                ArisApi_a._arisApi.WriteToErrorLog("User Id : " + _logonRequest.UserID + " Disconnected from the exchange");
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void _tcp_DataArrival(ConnectionData connectionData)
        {
            lock (_dataLock)
            {
                try
                {
                    int i = 0;
                    byte[] part;

                    while (connectionData.PartialPacket.Length - i > 2)
                    {
                        part = new byte[2];
                        Array.Copy(connectionData.PartialPacket, i, part, 0, 2);
                        part = new byte[IPAddress.NetworkToHostOrder(BitConverter.ToInt16(part, 0)) - 22];

                        if (part.Length <= (connectionData.PartialPacket.Length - (i + 22)))
                        {
                            Array.Copy(connectionData.PartialPacket, i + 22, part, 0, part.Length);
                            if (part.Length > 0)
                                ProcessPacket(part);

                            i += part.Length + 22;
                        }
                        else
                            break;
                    }

                    if (connectionData.PartialPacket.Length != i)
                    {
                        part = new byte[connectionData.PartialPacket.Length - i];
                        Array.Copy(connectionData.PartialPacket, i, part, 0, part.Length);
                        connectionData.PartialPacket = part;
                    }
                    else
                        connectionData.PartialPacket = new byte[0];

                    #region Comment code

                    //while (connectionData.PartialPacket.Length > Marshal.SizeOf(typeof(MessageHeader)))
                    //{
                    //    MessageHeader messageHeader = Utilities.PinnedPacket<MessageHeader>(connectionData.PartialPacket);
                    //    if (connectionData.PartialPacket.Length >= messageHeader.MessageLength)
                    //    {
                    //        byte[] packet = new byte[messageHeader.MessageLength];
                    //        Array.Copy(connectionData.PartialPacket, packet, messageHeader.MessageLength);
                    //        ProcessPacket(messageHeader.TransCode, packet);

                    //        if (messageHeader.ErrorCode != 0)
                    //        {
                    //            ErrorMessageResponse errorMessageResponse = Utilities.PinnedPacket<ErrorMessageResponse>(connectionData.PartialPacket);

                    //            if (errorMessageResponse.MessageHeader.TransCode == (short)NseFoMessageCode.LogOnResponse)
                    //            {
                    //                MessageInfo messageInfo = new MessageInfo();
                    //                messageInfo.Icon = MessageBoxIcon.Error;
                    //                messageInfo.Message =  Market.NseFO + GeneralConstants.COLON + _logonRequest.UserID + GeneralConstants.COLON +
                    //                    errorMessageResponse.ErrorMessage.Trim();
                    //                Global.Main.MessageThread(messageInfo);

                    //                Global.Main.LoginDetail.Login.EnableControl(true);
                    //            }
                    //            TransactionWatch.ErrorMessage("NseFo intractive : " + messageHeader.ErrorCode + GeneralConstants.COLON +
                    //                Utility.GetNseFoErrorMessage(messageHeader.ErrorCode) +
                    //                GeneralConstants.COLON + messageHeader.ErrorCode + GeneralConstants.COLON +
                    //                errorMessageResponse.ErrorMessage);
                    //        }

                    //        byte[] processData = new byte[connectionData.PartialPacket.Length - messageHeader.MessageLength];
                    //        Array.Copy(connectionData.PartialPacket, messageHeader.MessageLength, processData, 0, processData.Length);
                    //        connectionData.PartialPacket = processData;
                    //    }
                    //    else
                    //        break;

                    //    if (_isDownloadCompleted && messageHeader.TimeStamp1 != 0)
                    //    {
                    //        Thread report = new Thread(SaveSequenceNumber);
                    //        report.IsBackground = true;
                    //        report.Priority = ThreadPriority.Highest;
                    //        report.Start(messageHeader);

                    //        //ThreadPool.QueueUserWorkItem(new WaitCallback(SaveSequenceNumber), messageHeader);
                    //    }
                    //} 

                    #endregion
                }
                catch (Exception ex)
                {
                    ArisApi_a._arisApi.WriteToErrorLog(
                        MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
                }
            }
        }

        #endregion

        #region Methods

        private void ProcessPacket(byte[] receivePacket)
        {
            try
            {
                MessageHeader messageHeader = PinnedPacket<MessageHeader>(receivePacket);

                switch ((NseFoMessageCode)messageHeader.TransCode)
                {
                    #region Login Proces

                    case NseFoMessageCode.LogOnResponse:
                        LogonResponseProcess(PinnedPacket<LogonResponse>(receivePacket));
                        break;
                    case NseFoMessageCode.LogoffResponse:
                        LogoffResponseProcess(PinnedPacket<LogoffResponse>(receivePacket));
                        _tcp.DisconnectAllConnection();
                        break;
                    #endregion

                    #region Download Process

                    case NseFoMessageCode.SystemInformationResponse:
                        SystemInformationResponseProcess(PinnedPacket<SystemInformationResponse>(receivePacket));
                        break;

                    #region Update local database

                    case NseFoMessageCode.PartialSystemInformation:
                        //PartialSystemInformationProcess(Utilities.PinnedPacket<PartialSystemInformation>(receivePacket));
                        break;

                    case NseFoMessageCode.UpdateLocalDatabaseHeader:
                        //TransactionWatch.TransactionMessage(Market.NseFO.ToString() + GeneralConstants.COLON +
                        //    _logonRequest.UserID + GeneralConstants.COLON + "Update local database started.", Color.Black);
                        break;

                    case NseFoMessageCode.UpdateLocalDatabaseData:
                        //UpdateLocalDatabaseDataProcess(Utilities.PinnedPacket<UpdateLocalDatabaseData>(receivePacket));
                        break;

                    case NseFoMessageCode.UpdateLoacalDataBaseTrailer:
                        //TransactionWatch.TransactionMessage(Market.NseFO.ToString() + GeneralConstants.COLON +
                        //    _logonRequest.UserID + GeneralConstants.COLON + "Update local database completed.", Color.Black);
                        //_isDownloadCompleted = true;
                        break;

                    #endregion

                    #region Message Download

                    case NseFoMessageCode.MessageDownloadHeaderResponse:
                        _isDownloadCompleted = false;
                        break;

                    case NseFoMessageCode.MessageDownloadDataResponse:
                        MessageDownloadDataResponseProcess(PinnedPacket<MessageDownloadDataResponse>(receivePacket));
                        break;

                    case NseFoMessageCode.MessageDownloadTrailerResponse:
                        _isDownloadCompleted = true;
                        _attempt++;
                   
                        break;

                    #endregion

                    #endregion

                    #region Order and Trade Management

                    #region Order entry

                    //case NseFoMessageCode.MarketOrderResponse:
                    //    ArisDev.PacketConvertion.getOwnOrderEntryNseFo(_logonRequest.UserID.ToString(), PinnedPacket<MarketOrderResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseFoMessageCode.OrderEntryConfirm:
                    //    ArisDev.PacketConvertion.getOwnOrderEntryNseFo(_logonRequest.UserID.ToString(), PinnedPacket<OrderConfirmationResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseFoMessageCode.OrderFreeze:
                    //    ArisDev.PacketConvertion.getOwnOrderEntryNseFo(_logonRequest.UserID.ToString(), PinnedPacket<OrderFreezeResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseFoMessageCode.OrderEntryError:
                    //    ArisDev.PacketConvertion.getOwnOrderEntryNseFo(_logonRequest.UserID.ToString(), PinnedPacket<OrderEntryErrorResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseFoMessageCode.OrderModifyConfirm:
                    //    ArisDev.PacketConvertion.getOwnOrderEntryNseFo(_logonRequest.UserID.ToString(), PinnedPacket<OrderModificationConfirmationResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseFoMessageCode.OrderModifyError:
                    //    ArisDev.PacketConvertion.getOwnOrderEntryNseFo(_logonRequest.UserID.ToString(), PinnedPacket<OrderModificationErrorResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseFoMessageCode.OrderCancelConfirm:
                    //    ArisDev.PacketConvertion.getOwnOrderEntryNseFo(_logonRequest.UserID.ToString(), PinnedPacket<OrderCancellationConfirmationResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseFoMessageCode.OrderCancelError:
                    //    ArisDev.PacketConvertion.getOwnOrderEntryNseFo(_logonRequest.UserID.ToString(), PinnedPacket<OrderCancellationErrorResponse>(receivePacket).OrderEntry);
                    //    break;

                    #endregion

                    #endregion

                    #region Bhavcopy

                    case NseFoMessageCode.ReportOnMarketStatistics:
                        ReportOnMarketStatisticsProcess(receivePacket);
                        break;

                    #endregion

                    #region Unsolicited Messages

                    case NseFoMessageCode.StopLossTrigger:
                    case NseFoMessageCode.TradeConfirmation:
                      //  PacketConvertion.getOwnSLTriggerOrTradeNseFo(_logonRequest.UserID.ToString(), PinnedPacket<TradeConfirmation>(receivePacket));
                        break;

                    case NseFoMessageCode.HeartBeatMessageResponse:
                        break;

                    case NseFoMessageCode.BroadMsgSendFromControlInt:
                        break;

                    case NseFoMessageCode.BatchOrderCancel:
                        break;

                    case NseFoMessageCode.InvitationMessage:
                        InvitationMessageProcess(PinnedPacket<InvitationMessage>(receivePacket));
                        //ArisApi_a._arisApi.OnSystemUpdateProcess("NseFo : Available Invitations : " + _availableInvitation);
                        break;


                    #endregion

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void InvitationMessageProcess(InvitationMessage invitationMessage)
        {
            try
            {
                _availableInvitation += invitationMessage.InvitationCount;
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void ReportOnMarketStatisticsProcess(byte[] receivePacket)
        {
            try
            {
                char messageType = Convert.ToChar(receivePacket[Marshal.SizeOf(typeof(MessageHeader)) + 1]);

                switch (messageType)
                {
                    case 'H':
                        //TransactionWatch.TransactionMessage(Market.NseFO + GeneralConstants.COLON + "Bhav copy started.", Color.Black);
                        break;

                    case 'R':
                        //ContractBhavCopy.Bhavcopy.UpdateBhavCopy(Market.NseFO, Utilities.PinnedPacket<ReportOnMarketStatistics>(receivePacket));
                        break;

                    case 'T':
                        //TransactionWatch.TransactionMessage(Market.NseFO + GeneralConstants.COLON + "Bhav copy completed.", Color.Black);
                        break;
                }
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        #region Message download

        private void MessageDownloadDataResponseProcess(MessageDownloadDataResponse messageDownloadDataResponse)
        {
            try
            {
                switch ((NseFoMessageCode)messageDownloadDataResponse.MessageHeaderInner.TransCode)
                {
                    #region The Logon Process

                    case NseFoMessageCode.LogOnResponse:
                    case NseFoMessageCode.LogoffResponse:
                        LogonResponse LogoffRequest = PinnedPacket<LogonResponse>(messageDownloadDataResponse.Data);
                        break;

                    #endregion

                    #region Order and Trade Management

                    case NseFoMessageCode.OrderEntryConfirm:
                    case NseFoMessageCode.OrderEntryError:
                    case NseFoMessageCode.OrderModifyConfirm:
                    case NseFoMessageCode.OrderModifyError:
                    case NseFoMessageCode.OrderCancelConfirm:
                    case NseFoMessageCode.OrderCancelError:
                    case NseFoMessageCode.MarketOrderResponse:
                    case NseFoMessageCode.OrderFreeze:
                        //ArisApi_a._arisApi.ProcessOrderDownload(PinnedPacket<OrderEntryRequest>(messageDownloadDataResponse.Data));
                        break;

                    #endregion

                    #region Unsolicited Messages

                    case NseFoMessageCode.StopLossTrigger:
                        break;

                    case NseFoMessageCode.TradeConfirmation:
                        //TradeConfirmation trade = PinnedPacket<TradeConfirmation>(messageDownloadDataResponse.Data);
                      //  ArisApi_a._arisApi.ProcessTradeDownload(_logonRequest.UserID.ToString(), trade);
                        break;

                    #endregion

                    #region Broadcast Messages

                    case NseFoMessageCode.MarketStatusClose:
                    case NseFoMessageCode.MarketStatusOpen:
                    case NseFoMessageCode.MarketStatusPostClose:
                    case NseFoMessageCode.MarketStatusPreClose:
                    case NseFoMessageCode.MarketStatusPreOrPostDay:
                        ChangeofMarketStatusProcess(PinnedPacket<ChangeofMarketStatus>(messageDownloadDataResponse.Data));
                        break;

                    case NseFoMessageCode.BroadMsgSendFromControlInt:
                        BroadMsgSendFromControlIntProcess(messageDownloadDataResponse.Data);
                        break;

                    case NseFoMessageCode.SuspendedSymbolTrigger:
                        //todo:future develope
                        break;

                    case NseFoMessageCode.SpotGlobalSuspended:
                        //todo:future develope
                        break;

                    case NseFoMessageCode.SpotNonGlobalSuspended:
                        //todo:future develope
                        break;

                    case NseFoMessageCode.GeneralMessageBroadcast:
                        //GeneralMessageBroadcastProcess(Utilities.PinnedPacket<GeneralMessageBroadcast>(messageDownloadDataResponse.Data));
                        break;

                    case NseFoMessageCode.ChangeInSecurityMaster:
                        //ChangeInSecurityMasterProcess(Utilities.PinnedPacket<ChangeInSecurityMaster>(messageDownloadDataResponse.Data));
                        break;

                    case NseFoMessageCode.BatchOrderCancel:
                        //BatchOrderCancelProcess(Utilities.PinnedPacket<OrderEntryRequest>(messageDownloadDataResponse.Data));
                        break;

                    #endregion

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void BroadMsgSendFromControlIntProcess(object data)
        {
            try
            {
                byte[] receivePacket = (byte[])data;
                BroadMsgSendFromControl broadMsgSendFromControl = PinnedPacket<BroadMsgSendFromControl>(receivePacket);
                if (broadMsgSendFromControl.BroadcastMessage.Contains("The FMC code is"))
                {
                    //Global.SystemConfig.NseFoSebiCode = broadMsgSendFromControl.BroadcastMessage.Replace("The FMC code is", "").Trim();
                    //Utility.SaveSystemConfiguration();
                }
                //TransactionWatch.TransactionMessage(Market.NseFO.ToString() + GeneralConstants.COLON + broadMsgSendFromControl.BroadcastMessage.Trim(), Color.Black);
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void ChangeInSecurityMasterProcess(ChangeInSecurityMaster changeInSecurityMaster)
        {
            try
            {
                //ContractBhavCopy.Contract.UpdateContractMaster(Market.NseFO, changeInSecurityMaster);
                //ContractBhavCopy.Contract.Save(Market.NseFO);
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void ChangeofMarketStatusProcess(ChangeofMarketStatus changeofMarketStatus)
        {
            try
            {
                //TransactionWatch.TransactionMessage(Market.NseFO.ToString() + GeneralConstants.COLON + "ExchangeMessages" + GeneralConstants.COLON +
                //    changeofMarketStatus.BroadcastMessage.Trim(), Color.Black);
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        #endregion

        #region Update Local Database

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateLocalDatabaseData"></param>
        private void UpdateLocalDatabaseDataProcess(UpdateLocalDatabaseData updateLocalDatabaseData)
        {
            try
            {
                switch ((NseFoMessageCode)updateLocalDatabaseData.MessageHeaderInner.TransCode)
                {
                    case NseFoMessageCode.ChangeInSystemStatus:
                    case NseFoMessageCode.ChangeInSecurityMaster:
                        //ContractBhavCopy.UpdateContractMaster(Market.NseFO, Utilities.PinnedPacket<ChangeInSecurityMaster>(updateLocalDatabaseData.Data));
                        break;

                    case NseFoMessageCode.ChangeInInstrumentMaster:
                        //ContractBhavCopy.UpdateInstrument(Market.NseFO, Utilities.PinnedPacket<ChangeInInstrumentMaster>(updateLocalDatabaseData.Data));
                        break;

                    case NseFoMessageCode.ChangeParticipantStatus:
                        //todo:future develope
                        break;

                    case NseFoMessageCode.ChangeofSecurityStatus:
                        //ContractBhavCopy.UpdateSecurityStatus(Market.NseFO, Utilities.PinnedPacket<ChangeOfSecurityStatus>(updateLocalDatabaseData.Data));
                        break;

                    //case NseFoMessageCode.OrderEntryConfirm:
                    //    OrdersDownloadProcess(Utilities.PinnedPacket<OrderEntryRequest>(updateLocalDatabaseData.Data));
                    //    break;

                    case NseFoMessageCode.ExerciseAndDeliveryConfirmation:
                        //todo:future develope
                        break;

                    case NseFoMessageCode.IndexMapTable:
                        //todo:future develope
                        //IndexMapTable indexMapTable = Utilities.PinnedPacket<IndexMapTable>(updateLocalDatabaseData.Data);
                        break;

                    case NseFoMessageCode.ChangeinIndexMaster:
                        //todo:future develope
                        //ChangeinIndexMaster ChangeinIndexMaster = Utilities.PinnedPacket<ChangeinIndexMaster>(updateLocalDatabaseData.Data);
                        break;

                    default:
                        //TransactionWatch.TransactionMessage(Market.NseFO + GeneralConstants.COLON + "updateLocalDatabaseData" +
                        //    GeneralConstants.COLON + updateLocalDatabaseData.MessageHeaderInner.TransCode, Color.Red);
                        break;
                }
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        #endregion

        private void PartialSystemInformationProcess(PartialSystemInformation partialSystemInformation)
        {
            try
            {
                UpdateLocalDatabaseRequest updateLocalDatabaseRequest = new UpdateLocalDatabaseRequest();
                updateLocalDatabaseRequest.RequestForOpenOrders = 'G';
                //todo: set data of updateLocalDatabaseRequest
                updateLocalDatabaseRequest.MarketStatus = partialSystemInformation.SystemInformationResponse.MarketStatus;
                updateLocalDatabaseRequest.ExMarketStatus = partialSystemInformation.SystemInformationResponse.ExMarketStatus;
                updateLocalDatabaseRequest.PlMarketStatus = partialSystemInformation.SystemInformationResponse.PlMarketStatus;

                Thread thread = new Thread(SendData);
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.Highest;
                thread.Start(updateLocalDatabaseRequest);
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void SystemInformationResponseProcess(SystemInformationResponse systemInformationResponse)
        {
            try
            {


                UpdateLocalDatabaseRequest updateLocalDatabaseRequest = new UpdateLocalDatabaseRequest();
                updateLocalDatabaseRequest.RequestForOpenOrders = 'G';
                //todo: set data of updateLocalDatabaseRequest
                //updateLocalDatabaseRequest.MarketStatus = systemInformationResponse.MarketStatus;
                //updateLocalDatabaseRequest.ExMarketStatus = systemInformationResponse.ExMarketStatus;
                //updateLocalDatabaseRequest.PlMarketStatus = systemInformationResponse.PlMarketStatus;

                Thread thread = new Thread(SendData);
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.Highest;
                thread.Start(updateLocalDatabaseRequest);


                for (int i = 0; i <= 6; i++)
                {
                    MessageDownloadRequest messageDownloadRequest = new MessageDownloadRequest();
                    messageDownloadRequest.MessageHeader.AlphaChar = i.ToString();

                    Thread thread1 = new Thread(SendData);
                    thread1.IsBackground = true;
                    thread1.Priority = ThreadPriority.Highest;
                    thread1.Start(messageDownloadRequest);
                }
                    //MessageDownloadRequest messageDownloadRequest = new MessageDownloadRequest();
                    //messageDownloadRequest.MessageHeader.AlphaChar = _attempt.ToString();
                    ////messageDownloadRequest.SequenceNumber = _oldSequenceNumber;

                    //Thread thread1 = new Thread(SendData);
                    //thread1.IsBackground = true;
                    //thread1.Priority = ThreadPriority.Highest;
                    //thread1.Start(messageDownloadRequest);

                    messageTimer = new System.Timers.Timer();
                    messageTimer.Elapsed += new System.Timers.ElapsedEventHandler(messageTimer_Elapsed);
                    messageTimer.Interval = 5000;
                    messageTimer.Start();

                    ArisApi_a._arisApi.OnSystemUpdateProcess("NseFo : " + _logonRequest.UserID + " Message Download Started...");
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        void messageTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //ArisApi_a._arisApi._isDownloadCompleteNseFo = true;
            //ArisApi_a._arisApi.OnDownloadComplete();
            //ArisApi_a._arisApi.OnSystemUpdateProcess("NseFo : " + _logonRequest.UserID + " Message Download Finished.");
                messageTimer.Stop();
            
        }

        private void LogonResponseProcess(LogonResponse logonResponse)
        {
            try
            {
                if (logonResponse.MessageHeader.ErrorCode == 0)
                {
                  //  ArisApi_a._arisApi.OnLogonStatusChangedProcess(2, true, "");
                    SendData(new SystemInformationRequest());
                    Console.WriteLine(logonResponse.UserID + " Logon Sucessful");
                }
                else
                {
                  //  ArisApi_a._arisApi.OnLogonStatusChangedProcess(2, false, "");
                    Console.WriteLine(logonResponse.UserID + " Logon Failed");
                }
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void LogoffResponseProcess(LogoffResponse logoffResponse)
        {
            try
            {
                //MessageInfo messageInfo = new MessageInfo();
                //messageInfo.Icon = MessageBoxIcon.Information;
                //messageInfo.Message = "Logoff sucessfully from " + Market.NseFO + " " + ".";
                //Global.Main.MessageThread(messageInfo);
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        public void SendData(object packet)
        {
            lock (_invitationLock)
            {
                try
                {
                    while (_availableInvitation < 1)
                    
                    {
                        Thread.Sleep(100); 
                    }

                    if (_availableInvitation > 0)
                    {
                        _tcp.Send(AddPacketHeader(ArisApi_a.StructureToByte(packet)));
                        _availableInvitation--;
                    }
                }
                catch (Exception ex)
                {
                    ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
                }
            }
        }

        public void Disconnect()
        {
            if (_tcp != null)
                _tcp.DisconnectConnection("0");
        }

        private byte[] AddPacketHeader(byte[] packet)
        {
            byte[] destinationArray = new byte[packet.Length + 22];
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)destinationArray.Length)), destinationArray, 2);

            byte[] sequenceNumber = GetSequenceNumber();
            Array.Copy(sequenceNumber, 0, destinationArray, 2, sequenceNumber.Length);

            byte[] checkSum = GetCheckSum(packet);
            Array.Copy(checkSum, 0, destinationArray, 6, checkSum.Length);

            Array.Copy(packet, 0, destinationArray, 22, packet.Length);
            return destinationArray;
        }

        private byte[] GetSequenceNumber()
        {
            lock (_sequenceLock)
            {
                _sequenceNo++;
            }
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_sequenceNo));
        }

        private byte[] GetCheckSum(byte[] packet)
        {
            byte[] data = MD5.Create().ComputeHash(packet);
            if (data.Length > 0)
                return data;
            return new byte[0];
        }

        #endregion

        public T PinnedPacket<T>(byte[] data)
        {
            object packet = new object();
            try
            {
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr IntPtrOfObject = handle.AddrOfPinnedObject();
                packet = Marshal.PtrToStructure(IntPtrOfObject, typeof(T));
                handle.Free();
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
            return (T)packet;
        }

        #region Member variables

        private Tcp _tcp;
        private object _dataLock;
        private object _recSequenceLock;
        private object _sequenceLock;
        private object _invitationLock;
        private LogonRequest _logonRequest;
        private double _oldSequenceNumber;
        public bool _isDownloadCompleted;
        private int _sequenceNo;
        private short _availableInvitation;
        internal long _nnfId;
        internal short _branchId;
        internal int _attempt = 0;
        System.Timers.Timer messageTimer;
        #endregion
    }

    // <summary>
    /// 
    /// </summary>
    public class ProcessPacket
    {
        /// <summary>
        /// 
        /// </summary>
        public object PacketNotification { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
   


}