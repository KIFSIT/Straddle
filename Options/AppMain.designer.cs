namespace Straddle
{
    partial class AppMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        /// 
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppMain));
            this.cmConnectivityLbl = new System.Windows.Forms.Label();
            this.foConnectivityLbl = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.loginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nseCmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nseFoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cocCmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nseCMToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.nseFoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadContractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orderBookToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tradeBookToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.netPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tradeinfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.straddleStrangleStopLossCtlrEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.singleBuyStopLossCtrlSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.singleSellStopLossCtrlBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.singleBuyDrawDownAltSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.singleSellDrawDownAltSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tbDebugLog = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.version = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmConnectivityLbl
            // 
            this.cmConnectivityLbl.AutoSize = true;
            this.cmConnectivityLbl.Location = new System.Drawing.Point(826, 7);
            this.cmConnectivityLbl.Name = "cmConnectivityLbl";
            this.cmConnectivityLbl.Size = new System.Drawing.Size(35, 13);
            this.cmConnectivityLbl.TabIndex = 1;
            this.cmConnectivityLbl.Text = "  CM  ";
            // 
            // foConnectivityLbl
            // 
            this.foConnectivityLbl.AutoSize = true;
            this.foConnectivityLbl.Location = new System.Drawing.Point(870, 7);
            this.foConnectivityLbl.Name = "foConnectivityLbl";
            this.foConnectivityLbl.Size = new System.Drawing.Size(33, 13);
            this.foConnectivityLbl.TabIndex = 2;
            this.foConnectivityLbl.Text = "  FO  ";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loginToolStripMenuItem,
            this.cancelToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuStrip1.Size = new System.Drawing.Size(1195, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // loginToolStripMenuItem
            // 
            this.loginToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nseCmToolStripMenuItem,
            this.nseFoToolStripMenuItem});
            this.loginToolStripMenuItem.Name = "loginToolStripMenuItem";
            this.loginToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.loginToolStripMenuItem.Text = "Login";
            // 
            // nseCmToolStripMenuItem
            // 
            this.nseCmToolStripMenuItem.Name = "nseCmToolStripMenuItem";
            this.nseCmToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.nseCmToolStripMenuItem.Text = "Nse Cm";
            this.nseCmToolStripMenuItem.Click += new System.EventHandler(this.nseCMToolStripMenuItem_Click);
            // 
            // nseFoToolStripMenuItem
            // 
            this.nseFoToolStripMenuItem.Name = "nseFoToolStripMenuItem";
            this.nseFoToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.nseFoToolStripMenuItem.Text = "Nse Fo";
            this.nseFoToolStripMenuItem.Click += new System.EventHandler(this.nseFoToolStripMenuItem_Click);
            // 
            // cancelToolStripMenuItem
            // 
            this.cancelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cocCmToolStripMenuItem,
            this.downloadContractToolStripMenuItem});
            this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
            this.cancelToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.cancelToolStripMenuItem.Text = "File";
            // 
            // cocCmToolStripMenuItem
            // 
            this.cocCmToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nseCMToolStripMenuItem1,
            this.nseFoToolStripMenuItem1});
            this.cocCmToolStripMenuItem.Name = "cocCmToolStripMenuItem";
            this.cocCmToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.cocCmToolStripMenuItem.Text = "Coc";
            // 
            // nseCMToolStripMenuItem1
            // 
            this.nseCMToolStripMenuItem1.Name = "nseCMToolStripMenuItem1";
            this.nseCMToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.nseCMToolStripMenuItem1.Text = "NseCM";
            // 
            // nseFoToolStripMenuItem1
            // 
            this.nseFoToolStripMenuItem1.Name = "nseFoToolStripMenuItem1";
            this.nseFoToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.nseFoToolStripMenuItem1.Text = "NseFo";
            // 
            // downloadContractToolStripMenuItem
            // 
            this.downloadContractToolStripMenuItem.Name = "downloadContractToolStripMenuItem";
            this.downloadContractToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.downloadContractToolStripMenuItem.Text = "Download Contract";
            this.downloadContractToolStripMenuItem.Click += new System.EventHandler(this.downloadContractToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.orderBookToolStripMenuItem,
            this.tradeBookToolStripMenuItem,
            this.netPositionToolStripMenuItem,
            this.tradeinfoToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // orderBookToolStripMenuItem
            // 
            this.orderBookToolStripMenuItem.Name = "orderBookToolStripMenuItem";
            this.orderBookToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.orderBookToolStripMenuItem.Text = "OrderBook";
            // 
            // tradeBookToolStripMenuItem
            // 
            this.tradeBookToolStripMenuItem.Name = "tradeBookToolStripMenuItem";
            this.tradeBookToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.tradeBookToolStripMenuItem.Text = "TradeBook";
            // 
            // netPositionToolStripMenuItem
            // 
            this.netPositionToolStripMenuItem.Name = "netPositionToolStripMenuItem";
            this.netPositionToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.netPositionToolStripMenuItem.Text = "NetPosition";
            // 
            // tradeinfoToolStripMenuItem
            // 
            this.tradeinfoToolStripMenuItem.Name = "tradeinfoToolStripMenuItem";
            this.tradeinfoToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.tradeinfoToolStripMenuItem.Text = "Tradeinfo";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.straddleStrangleStopLossCtlrEToolStripMenuItem,
            this.singleBuyStopLossCtrlSToolStripMenuItem,
            this.singleSellStopLossCtrlBToolStripMenuItem,
            this.singleBuyDrawDownAltSToolStripMenuItem,
            this.singleSellDrawDownAltSToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // straddleStrangleStopLossCtlrEToolStripMenuItem
            // 
            this.straddleStrangleStopLossCtlrEToolStripMenuItem.Name = "straddleStrangleStopLossCtlrEToolStripMenuItem";
            this.straddleStrangleStopLossCtlrEToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
            this.straddleStrangleStopLossCtlrEToolStripMenuItem.Text = "Straddle/Strangle StopLoss      Ctlr+E";
            // 
            // singleBuyStopLossCtrlSToolStripMenuItem
            // 
            this.singleBuyStopLossCtrlSToolStripMenuItem.Name = "singleBuyStopLossCtrlSToolStripMenuItem";
            this.singleBuyStopLossCtrlSToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
            this.singleBuyStopLossCtrlSToolStripMenuItem.Text = "Single BuyStopLoss                   Ctrl+A";
            // 
            // singleSellStopLossCtrlBToolStripMenuItem
            // 
            this.singleSellStopLossCtrlBToolStripMenuItem.Name = "singleSellStopLossCtrlBToolStripMenuItem";
            this.singleSellStopLossCtrlBToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
            this.singleSellStopLossCtrlBToolStripMenuItem.Text = "Single SellStopLoss                    Ctrl+D";
            // 
            // singleBuyDrawDownAltSToolStripMenuItem
            // 
            this.singleBuyDrawDownAltSToolStripMenuItem.Name = "singleBuyDrawDownAltSToolStripMenuItem";
            this.singleBuyDrawDownAltSToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
            this.singleBuyDrawDownAltSToolStripMenuItem.Text = "Single BuyDrawDown                Alt+S";
            // 
            // singleSellDrawDownAltSToolStripMenuItem
            // 
            this.singleSellDrawDownAltSToolStripMenuItem.Name = "singleSellDrawDownAltSToolStripMenuItem";
            this.singleSellDrawDownAltSToolStripMenuItem.Size = new System.Drawing.Size(256, 22);
            this.singleSellDrawDownAltSToolStripMenuItem.Text = "Single SellDrawDown                Alt+S";
            // 
            // tbDebugLog
            // 
            this.tbDebugLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbDebugLog.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbDebugLog.Location = new System.Drawing.Point(0, 420);
            this.tbDebugLog.Name = "tbDebugLog";
            this.tbDebugLog.Size = new System.Drawing.Size(1195, 71);
            this.tbDebugLog.TabIndex = 11;
            this.tbDebugLog.Text = "";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // version
            // 
            this.version.AutoSize = true;
            this.version.Location = new System.Drawing.Point(940, 6);
            this.version.Name = "version";
            this.version.Size = new System.Drawing.Size(0, 13);
            this.version.TabIndex = 13;
            // 
            // AppMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1195, 491);
            this.Controls.Add(this.version);
            this.Controls.Add(this.tbDebugLog);
            this.Controls.Add(this.foConnectivityLbl);
            this.Controls.Add(this.cmConnectivityLbl);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "AppMain";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AppMain_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion

        private System.Windows.Forms.Label cmConnectivityLbl;
        private System.Windows.Forms.Label foConnectivityLbl;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem loginToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nseCmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nseFoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orderBookToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tradeBookToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem netPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cocCmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadContractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nseCMToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem nseFoToolStripMenuItem1;
        public System.Windows.Forms.RichTextBox tbDebugLog;
        private System.Windows.Forms.ToolStripMenuItem tradeinfoToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label version;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem straddleStrangleStopLossCtlrEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem singleBuyStopLossCtrlSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem singleSellStopLossCtrlBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem singleBuyDrawDownAltSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem singleSellDrawDownAltSToolStripMenuItem;
    }
}

