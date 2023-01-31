using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArisDev;
using Straddle.AppClasses;
using System.IO;

namespace Straddle
{
    public partial class AppMain : Form
    {
        public AppMain()
        {
            InitializeComponent();
            ArisApi_a._arisApi.OnLogonStatusChanged += new ArisApi_a.LoginStatusChangeDelegate(_arisApi_OnLogonStatusChanged);
            ArisApi_a._arisApi.OnSystemUpdate += new ArisApi_a.SystemUpdates(_arisApi_OnSystemUpdate);
        }

        void _arisApi_OnLogonStatusChanged(uint Gateway, bool _isLoggedOn, string _reason)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (_isLoggedOn == false)
                    {
                        if (Gateway == 1)
                        {
                            cmConnectivityLbl.BackColor = Color.Red;
                        }
                        else
                        {
                            foConnectivityLbl.BackColor = Color.Red;
                        }
                    }
                    else
                    {
                        if (Gateway == 1)
                        {
                            cmConnectivityLbl.BackColor = Color.Green;
                        }
                        else
                        {
                            foConnectivityLbl.BackColor = Color.Green;
                        }
                    }
                });
            }
            catch (Exception)
            { }
        }

        void _arisApi_OnSystemUpdate(string message)
        {
           
        }

        private void nseCMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_login != null)
                    _login.Close();
                _login = new Login();
                _login.MdiParent = this;
                _login.Text = "Login NseCm";
                _login.Show();
            }
            catch (Exception) { }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                ArisApi_a._arisApi.InitializeAPI();
                AppGlobal.frmWatch = new OptionWatch();
                AppGlobal.frmWatch.MdiParent = this;
                AppGlobal.frmWatch.WindowState = FormWindowState.Maximized;
                AppGlobal.frmWatch.Show();
                timer1.Enabled = true;
                timer1.Interval = 1000;
                version.Text = AppGlobal.Version.ToString();
                AppGlobal.frmWatch.Text = AppGlobal.Watch.ToString();
              }
            catch (Exception) { }
        }

        private void nseFoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_login != null)
                    _login.Close();
                _login = new Login();
                _login.MdiParent = this;
                _login.Text = "Login NseFo";
                _login.StartPosition = FormStartPosition.CenterScreen;
                _login.Show();
            }
            catch (Exception) { }
        }

        internal void WriteToTransactionWatch(string p, LogWriter.LogEnums.WriteOption writeOption, string color)
        {
            try
            {
                switch (writeOption)
                {
                    case LogWriter.LogEnums.WriteOption.ErrorLogFile:
                    case LogWriter.LogEnums.WriteOption.ErrorLogFile_MessageBox:
                    case LogWriter.LogEnums.WriteOption.LogFile_ErrorLogFile:
                    case LogWriter.LogEnums.WriteOption.LogFile_ErrorLogFile_MessageBox:
                    case LogWriter.LogEnums.WriteOption.LogFile_LogWindow_ErrorLogFile:
                        ArisApi_a._arisApi.WriteToErrorLog(p);
                        break;
                    default:
                        ArisApi_a._arisApi.WriteToTransactionLog(p);
                        break;
                }
            }
            catch (Exception) { }
        }

        void WriteToTransactionWatch(string msg, Color color)
        {
            try
            {
                TransactionWatch.TransactionMessage(msg, color);
            }
            catch (Exception) { }
        }

   
       
       
        internal Login _login;
     
       

        private void downloadContractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ArisApi_a._arisApi.DownloadLatestContract();
        }
       

        private void timer1_Tick(object sender, EventArgs e)
        {
          
        }

        private void AppMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            TransactionWatch.ErrorMessage("Main Window Closed");
        }
    }
}
