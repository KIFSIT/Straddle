using System;
using System.Data;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using Sockets;
using System.Linq;
using Straddle.NseCmApi.Header;
using Straddle.NseFoApi.LogonProcess;
using Straddle.NseCmApi.DownloadProcess;
using Straddle.NseCmApi.Broadcast;
using Straddle.AppClasses;
using Straddle.NseCmApi;
namespace Straddle.AppClasses
{
    public class NseCmConnection
    {
        #region Constructor

        internal NseCmConnection(string ctclId, string tradingMemberId, short branchId, long nnfId, string password, string newPassword)
        {
            //_branchId = branchId;
            //_nnfId = nnfId;
            //_logonRequest = new LogonRequest();
            //_logonRequest.UserID = Convert.ToInt32(ctclId);
            //_logonRequest.Password = password;
            //_logonRequest.NewPassword = newPassword;
            //_logonRequest.TraderName = tradingMemberId;
            //_logonRequest.BrokerID = tradingMemberId;
            //_logonRequest.VersionNumber = int.Parse(ArisApi_a._arisApi.SystemConfig.NseCmApiVersion);
            //_logonRequest.UserType = (short)UserType.Dealer;
            //ConnectToSocket(_logonRequest);
        }

        #endregion

        #region Tcp Sockets Method

        private void ConnectToSocket(LogonRequest logonRequest)
        {
            try
            {
                //_isDownloadCompleted = false;

                //_dataLock = new object();
                //_sequenceLock = new object();
                //_recSequenceLock = new object();
                //_invitationLock = new object();
                //_tcp = new Tcp(ArisApi_a._arisApi.SystemConfig.NseCmHostIp, ArisApi_a._arisApi.SystemConfig.NseCmHostPort, AppDomain.CurrentDomain.BaseDirectory, TypeOfCompression.None);
                //_tcp.Connect += _tcp_Connect;
                //_tcp.DataArrival += _tcp_DataArrival;
                //_tcp.Disconnect += _tcp_Disconnect;
                //_tcp.Error += _tcp_Error;
                //_tcp.ConnectedTo();

                //if (!string.IsNullOrEmpty(_tcp.ComressLibError))
                //{
                //    //messageInfo.Message = _tcp.ComressLibError;
                //}
            }
            catch (Exception ex)
            {
                TransactionWatch.ErrorMessage(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void _tcp_Error(string error)
        {
            //if (error.Contains("A request to send or receive data was disallowed"))
            //{
            //    MessageInfo messageInfo = new MessageInfo();
            //    messageInfo.Message = "Connection Busy Error. Check Connection Integrity NseCm!";
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
            //    messageInfo.Message = "Connection close from NseCm!\nPlease, Login again !";
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
                SendData(_logonRequest);
                ArisApi_a._arisApi._transactionLog.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ffff >> ") + "Logon request sent to Host.");
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
                ArisApi_a._arisApi.OnLogonStatusChangedProcess(1, false, "");
                ArisApi_a._arisApi.OnSystemUpdateProcess("NseCm : " + _logonRequest.UserID + " Disconnected from the exchange");
                ArisApi_a._arisApi.WriteToErrorLog("User Id : " + _logonRequest.UserID + "Disconnected from the exchange");
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

                    //            if (errorMessageResponse.MessageHeader.TransCode == (short)NseCmMessageCode.LogOnResponse)
                    //            {
                    //                MessageInfo messageInfo = new MessageInfo();
                    //                messageInfo.Icon = MessageBoxIcon.Error;
                    //                messageInfo.Message = Market.NseCm + GeneralConstants.COLON + _logonRequest.UserID + GeneralConstants.COLON +
                    //                    errorMessageResponse.ErrorMessage.Trim();
                    //                Global.Main.MessageThread(messageInfo);

                    //                Global.Main.LoginDetail.Login.EnableControl(true);
                    //            }
                    //            TransactionWatch.ErrorMessage("NseCm intractive : " + messageHeader.ErrorCode + GeneralConstants.COLON +
                    //                Utility.GetNseCmErrorMessage(messageHeader.ErrorCode) +
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

        private void SaveSequenceNumber(object data)
        {
            lock (_recSequenceLock)
            {
                try
                {
                    MessageHeader messageHeader = (MessageHeader)data;
                    if (_isDownloadCompleted && messageHeader.TimeStamp1 != 0)
                    {
                        //save sequence no
                    }
                }
                catch (Exception ex)
                {
                    ArisApi_a._arisApi.WriteToErrorLog(
                        MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
                }
            }
        }

        private void ProcessPacket(byte[] receivePacket)
        {
            try
            {
                MessageHeader messageHeader = PinnedPacket<MessageHeader>(receivePacket);

                switch ((NseCmMessageCode)messageHeader.TransCode)
                {
                    #region Login Proces

                    case NseCmMessageCode.LogOnResponse:
                        LogonResponseProcess(PinnedPacket<LogonResponse>(receivePacket));
                        break;
                    case NseCmMessageCode.LogoffRespose:
                        LogoffResponseProcess(PinnedPacket<LogoffResponse>(receivePacket));
                        _tcp.DisconnectAllConnection();
                        break;

                    #endregion

                    #region Download Process

                    case NseCmMessageCode.SystemInformationResponse:
                        SystemInformationResponseProcess(PinnedPacket<SystemInformationResponse>(receivePacket));
                        break;

                    #region Update local database

                    case NseCmMessageCode.PartialSystemInformation:
                        PartialSystemInformationProcess(PinnedPacket<PartialSystemInformation>(receivePacket));
                        break;

                    case NseCmMessageCode.UpdateLocalDatabaseHeader:
                        break;

                    case NseCmMessageCode.UpdateLocalDatabaseData:
                        UpdateLocalDatabaseDataProcess(PinnedPacket<UpdateLocalDatabaseData>(receivePacket));
                        break;

                    case NseCmMessageCode.UpdateLoacalDataBaseTrailer:
                        _isDownloadCompleted = true;
                        break;

                    #endregion

                    #region Message Download

                    case NseCmMessageCode.MessageDownloadHeaderResponse:
                        _isDownloadCompleted = false;
                        break;

                    case NseCmMessageCode.MessageDownloadDataResponse:
                        MessageDownloadDataResponseProcess(PinnedPacket<MessageDownloadDataResponse>(receivePacket));
                        break;

                    case NseCmMessageCode.MessageDownloadTrailerResponse:
                        _isDownloadCompleted = true;
                        _attempt++;

                        if (!ArisApi_a._arisApi.OrderDownloadCollection.Any(x => x.Value.Exchange == "NseCm"))
                        {
                            MessageDownloadRequest messageDownloadRequest = new MessageDownloadRequest();
                            messageDownloadRequest.MessageHeader.AlphaChar = _attempt.ToString();
                            messageDownloadRequest.SequenceNumber = 0;
                            SendData(messageDownloadRequest);
                        }
                        else
                        {
                            //ArisApi_a._arisApi._isDownloadCompleteNseCm = true;
                            ArisApi_a._arisApi.OnDownloadComplete();
                            ArisApi_a._arisApi.OnSystemUpdateProcess("NseCm : " + _logonRequest.UserID + " Message Download Finished.");
                        }
                      break;

                    #endregion

                    #endregion

                    #region Order Management

                    //case NseCmMessageCode.MarketOrderResponse:
                    //    PacketConvertion.getOwnOrderEntryNseCm(_logonRequest.UserID.ToString(), PinnedPacket<MarketOrderResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseCmMessageCode.OrderEntryConfirm:
                    //    PacketConvertion.getOwnOrderEntryNseCm(_logonRequest.UserID.ToString(), PinnedPacket<OrderConfirmationResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseCmMessageCode.OrderFreeze:
                    //    PacketConvertion.getOwnOrderEntryNseCm(_logonRequest.UserID.ToString(), PinnedPacket<OrderFreezeResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseCmMessageCode.OrderEntryError:
                    //    PacketConvertion.getOwnOrderEntryNseCm(_logonRequest.UserID.ToString(), PinnedPacket<OrderEntryErrorResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseCmMessageCode.OrderModifyConfirm:
                    //    PacketConvertion.getOwnOrderEntryNseCm(_logonRequest.UserID.ToString(), PinnedPacket<OrderModificationConfirmationResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseCmMessageCode.OrderModifyError:
                    //    PacketConvertion.getOwnOrderEntryNseCm(_logonRequest.UserID.ToString(), PinnedPacket<OrderModificationErrorResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseCmMessageCode.OrderCancelConfirm:
                    //    PacketConvertion.getOwnOrderEntryNseCm(_logonRequest.UserID.ToString(), PinnedPacket<OrderCancellationConfirmationResponse>(receivePacket).OrderEntry);
                    //    break;

                    //case NseCmMessageCode.OrderCancelError:
                    //    PacketConvertion.getOwnOrderEntryNseCm(_logonRequest.UserID.ToString(), PinnedPacket<OrderCancellationErrorResponse>(receivePacket).OrderEntry);
                    //    break;

                    #endregion

                    #region Unsolicited Messages

                    //case NseCmMessageCode.StopLossTrigger:
                    //    PacketConvertion.getOwnSLTriggerOrTradeNseCm(_logonRequest.UserID.ToString(), PinnedPacket<TradeConfirmation>(receivePacket));
                    //    break;

                    //case NseCmMessageCode.TradeConfirmation:
                    //    PacketConvertion.getOwnSLTriggerOrTradeNseCm(_logonRequest.UserID.ToString(), PinnedPacket<TradeConfirmation>(receivePacket));
                    //    break;

                    //case NseCmMessageCode.BroadMsgSendFromControlInt:
                    //    break;

                    //case NseCmMessageCode.BatchOrderCancel:
                    //    break;

                    //case NseCmMessageCode.InvitationMessage:
                    //    InvitationMessageProcess(PinnedPacket<InvitationMessage>(receivePacket));
                    //    ArisApi_a._arisApi.OnSystemUpdateProcess("NseCm : Available Invitations : " + _availableInvitation);
                    //    break;

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

        #region Message download

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageDownloadDataResponse"></param>
        private void MessageDownloadDataResponseProcess(MessageDownloadDataResponse messageDownloadDataResponse)
        {
            try
            {
                switch ((NseCmMessageCode)messageDownloadDataResponse.MessageHeaderInner.TransCode)
                {
                    #region The Logon Process

                    case NseCmMessageCode.LogOnResponse:
                    case NseCmMessageCode.LogoffRespose:
                        LogonResponse LogoffRequest = PinnedPacket<LogonResponse>(messageDownloadDataResponse.Data);
                        break;

                    #endregion

                    #region Order and Trade Management

                    case NseCmMessageCode.OrderEntryConfirm:
                    case NseCmMessageCode.OrderEntryError:
                    case NseCmMessageCode.OrderModifyConfirm:
                    case NseCmMessageCode.OrderModifyError:
                    case NseCmMessageCode.MarketOrderResponse:
                    case NseCmMessageCode.OrderFreeze:
                    case NseCmMessageCode.OrderCancelConfirm:
                    case NseCmMessageCode.OrderCancelError:
                        //ArisApi_a._arisApi.ProcessOrderDownload(PinnedPacket<OrderEntryRequest>(messageDownloadDataResponse.Data));
                        break;

                    #endregion

                    #region Unsolicited Messages

                    case NseCmMessageCode.StopLossTrigger:
                        break;

                    case NseCmMessageCode.TradeConfirmation:
                        //TradeConfirmation trade = PinnedPacket<TradeConfirmation>(messageDownloadDataResponse.Data);
                        //ArisApi_a._arisApi.ProcessTradeDownload(_logonRequest.UserID.ToString(), trade);
                        break;

                    #endregion

                    #region Broadcast Messages

                    case NseCmMessageCode.MarketStatusClose:
                    case NseCmMessageCode.MarketStatusOpen:
                    case NseCmMessageCode.MarketStatusPostClose:
                    case NseCmMessageCode.MarketStatusPreClose:
                    case NseCmMessageCode.MarketStatusPreOrPostDay:
                        //ChangeofMarketStatusProcess(PinnedPacket<ChangeofMarketStatus>(messageDownloadDataResponse.Data));
                        break;

                    case NseCmMessageCode.BroadMsgSendFromControlInt:
                        //BroadMsgSendFromControlIntProcess(messageDownloadDataResponse.Data);
                        break;

                    case NseCmMessageCode.SuspendedSymbolTrigger:
                        //todo:future develope
                        break;

                    case NseCmMessageCode.SpotGlobalSuspended:
                        //todo:future develope
                        break;

                    case NseCmMessageCode.SpotNonGlobalSuspended:
                        //todo:future develope
                        break;

                    case NseCmMessageCode.GeneralMessageBroadcast:
                        //GeneralMessageBroadcastProcess(PinnedPacket<GeneralMessageBroadcast>(messageDownloadDataResponse.Data));
                        break;

                    case NseCmMessageCode.SecurityMasterUpdate:
                        //SecurityMasterUpdateProcess(Utilities.PinnedPacket<SecurityMasterUpdate>(messageDownloadDataResponse.Data));
                        break;

                    case NseCmMessageCode.BatchOrderCancel:
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

        private void GeneralMessageBroadcastProcess(GeneralMessageBroadcast generalMessageBroadcast)
        {
            try
            {
                //TransactionWatch.TransactionMessage(Market.NseCm.ToString() + GeneralConstants.COLON + generalMessageBroadcast.BroadcastMessage.Trim(), Color.Black);
            }
            catch (Exception ex)
            {
                TransactionWatch.ErrorMessage(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="securityMasterUpdate"></param>
        private void SecurityMasterUpdateProcess(SecurityMasterUpdate securityMasterUpdate)
        {
            try
            {
                //ContractBhavCopy.Contract.UpdateContractMaster(Market.NseCm, securityMasterUpdate);
                //ContractBhavCopy.Contract.Save(Market.NseCm);
            }
            catch (Exception ex)
            {
                TransactionWatch.ErrorMessage(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="changeofMarketStatus"></param>
        private void ChangeofMarketStatusProcess(ChangeofMarketStatus changeofMarketStatus)
        {
            try
            {
                //TransactionWatch.TransactionMessage(Market.NseCm.ToString() + GeneralConstants.COLON + "ExchangeMessages" + GeneralConstants.COLON +
                //    changeofMarketStatus.BroadcastMessage.Trim(), Color.Black);
            }
            catch (Exception ex)
            {
                TransactionWatch.ErrorMessage(
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
                switch ((NseCmMessageCode)updateLocalDatabaseData.MessageHeaderInner.TransCode)
                {
                    case NseCmMessageCode.ChangeInSystemStatus:
                    case NseCmMessageCode.SecurityMasterUpdate:
                        //ContractBhavCopy.UpdateContractMaster(Market.NseCm, Utilities.PinnedPacket<SecurityMasterUpdate>(updateLocalDatabaseData.Data));
                        break;

                    case NseCmMessageCode.ChangeParticipantStatus:
                        //todo:future develope
                        break;

                    case NseCmMessageCode.ChangeofSecurityStatus:
                        //ContractBhavCopy.UpdateSecurityStatus(Market.NseCm, Utilities.PinnedPacket<ChangeOfSecurityStatus>(updateLocalDatabaseData.Data));
                        break;

                    //case NseCmMessageCode.OrderEntryConfirm:
                    //    OrdersDownloadProcess(Utilities.PinnedPacket<OrderEntryRequest>(updateLocalDatabaseData.Data));
                    //    break;

                    case NseCmMessageCode.ExerciseAndDeliveryConfirmation:
                        //todo:future develope
                        break;

                    default:
                        //TransactionWatch.TransactionMessage(Market.NseCm + GeneralConstants.COLON + "updateLocalDatabaseData" +
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
                //UpdateLocalDatabaseRequest updateLocalDatabaseRequest = new UpdateLocalDatabaseRequest();
                //updateLocalDatabaseRequest.RequestForOpenOrders = 'G';
                ////todo: set data of updateLocalDatabaseRequest
                //updateLocalDatabaseRequest.MarketStatus = partialSystemInformation.SystemInformationResponse.MarketStatus;

                //Thread thread = new Thread(SendData);
                //thread.IsBackground = true;
                //thread.Priority = ThreadPriority.Highest;
                //thread.Start(updateLocalDatabaseRequest);
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
                updateLocalDatabaseRequest.RequestForOpenOrders = 'N';
                ////todo: set data of updateLocalDatabaseRequest
                //updateLocalDatabaseRequest.MarketStatus = systemInformationResponse.MarketStatus;

                Thread thread = new Thread(SendData);
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.Highest;
                thread.Start(updateLocalDatabaseRequest);

                MessageDownloadRequest messageDownloadRequest = new MessageDownloadRequest();
                messageDownloadRequest.MessageHeader.AlphaChar = Convert.ToInt16(Convert.ToByte(systemInformationResponse.MessageHeader.AlphaChar[0])).ToString();
                //messageDownloadRequest.SequenceNumber = _oldSequenceNumber;

                Thread thread1 = new Thread(SendData);
                thread1.IsBackground = true;
                thread1.Priority = ThreadPriority.Highest;
                thread1.Start(messageDownloadRequest);

                ArisApi_a._arisApi.OnSystemUpdateProcess("NseCm : " + _logonRequest.UserID + " Message Download Started...");
            }
            catch (Exception ex)
            {
                ArisApi_a._arisApi.WriteToErrorLog(
                    MethodBase.GetCurrentMethod().DeclaringType.Name + " : " + MethodBase.GetCurrentMethod().Name + " : " + ex.Message);
            }
        }

        private void LogonResponseProcess(LogonResponse logonResponse)
        {
            try
            {
                if (logonResponse.MessageHeader.ErrorCode == 0)
                {
                    SendData(new SystemInformationRequest());
                    ArisApi_a._arisApi.OnLogonStatusChangedProcess(1, true, "");
                    Console.WriteLine(logonResponse.UserID + " Logon Sucessful");
                }
                else
                {
                    ArisApi_a._arisApi.OnLogonStatusChangedProcess(1, false, "");
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
                //messageInfo.Message = "Logoff sucessfully from " + Market.NseCm + " " + ".";
                //Global.Main.MessageThread(messageInfo);
            }
            catch (Exception ex)
            {
                TransactionWatch.ErrorMessage(
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
                        Thread.Sleep(100);

                    if (_availableInvitation > 0)
                    {
                        _tcp.Send(AddPacketHeader(ArisApi_a.StructureToByte(packet)));
                        _availableInvitation--;
                    }
                }
                catch (Exception ex)
                { }
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
        public bool _isDownloadCompleted;
        private int _sequenceNo;
        private short _availableInvitation;
        internal long _nnfId;
        internal short _branchId;
        internal int _attempt = 1;
        #endregion
    }
}
