using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ArisDev;
using System.Diagnostics;

namespace Straddle
{
    /// <summary>
    /// 
    /// </summary>
    public static class TransactionWatch
    {
        #region Method

        private delegate void MsgData(string msg, Color color);

        public static void ErrorMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    StackFrame stackFrame = new StackFrame(1, true);
                    int line = stackFrame.GetFileLineNumber();
                    string filename = stackFrame.GetFileName();
                   
                    message = filename.Split('\\').Last() + "|" + line + "|" + message;
                    ArisApi_a._arisApi.WriteToErrorLog(message);                 
                }
            }
            catch (Exception) { }
        }


        public static void MessageLogs(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    StackFrame stackFrame = new StackFrame(1, true);
                    int line = stackFrame.GetFileLineNumber();
                    string filename = stackFrame.GetFileName();

                    message = filename.Split('\\').Last() + "|" + line + "|" + message;
                    ArisApi_a._arisApi.WriteMassageLog(message);
                }
            }
            catch (Exception) { }
        }




        public static void TradeMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ArisApi_a._arisApi.WriteToTradeLog(message);
                    // Message(message, Color.Red);
                }
            }
            catch (Exception) { }
        }

        public static void OnlyTradeMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    ArisApi_a._arisApi.WriteToOnlyTradeLog(message);
                }
            }
            catch (Exception)
            { }
        }



        public static void TransactionMessage(string message, Color color)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    //StackFrame stackFrame = new StackFrame(1, true);
                    //int line = stackFrame.GetFileLineNumber();
                    //string filename = stackFrame.GetFileName();

                    //message = filename.Split('\\').Last() + "|" + line + "|" + message;

                    Message(message, color);
                }
            }
            catch (Exception) { }
        }

        private static void Message(string message, Color color)
        {
            try
            {
                if (Program._form.tbDebugLog.InvokeRequired)
                {
                    MsgData obj = Message;
                    Program._form.tbDebugLog.Invoke(obj, new object[] { message, color });
                }
                else if (Program._form.tbDebugLog != null)
                {
                    if (Program._form.tbDebugLog.Text.Length > 50000)
                    {
                        var lines = Program._form.tbDebugLog.Lines;
                        int numOfLines = lines.ToArray().Length - 5;
                        var newLines = lines.Skip(numOfLines);
                        Program._form.tbDebugLog.Lines = newLines.ToArray();
                    }

                    Program._form.tbDebugLog.SelectionStart = Program._form.tbDebugLog.Text.Length;
                    Program._form.tbDebugLog.SelectionColor = color;
                    Program._form.tbDebugLog.SelectedText = DateTime.Now.ToString("HH:mm:ss:ffff >> ") + message + Environment.NewLine;
                    Program._form.tbDebugLog.ScrollToCaret();
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
