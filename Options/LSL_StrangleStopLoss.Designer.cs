namespace Straddle
{
    partial class LSL_StrangleStopLoss
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.StrategyInfo = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLSL_StrategyPercent = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lblUniqueId = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.LegsInfo = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.LegsInfo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.lblUniqueId);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.txtLSL_StrategyPercent);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.StrategyInfo);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(327, 153);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // StrategyInfo
            // 
            this.StrategyInfo.AutoSize = true;
            this.StrategyInfo.Location = new System.Drawing.Point(169, 16);
            this.StrategyInfo.Name = "StrategyInfo";
            this.StrategyInfo.Size = new System.Drawing.Size(10, 13);
            this.StrategyInfo.TabIndex = 0;
            this.StrategyInfo.Text = "-";
            this.StrategyInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "StopLossParameter";
            // 
            // txtLSL_StrategyPercent
            // 
            this.txtLSL_StrategyPercent.Location = new System.Drawing.Point(119, 83);
            this.txtLSL_StrategyPercent.Name = "txtLSL_StrategyPercent";
            this.txtLSL_StrategyPercent.Size = new System.Drawing.Size(100, 20);
            this.txtLSL_StrategyPercent.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(200, 117);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 30;
            this.button2.Text = "OFF";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(62, 117);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 29;
            this.button1.Text = "Set";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblUniqueId
            // 
            this.lblUniqueId.AutoSize = true;
            this.lblUniqueId.Location = new System.Drawing.Point(96, 16);
            this.lblUniqueId.Name = "lblUniqueId";
            this.lblUniqueId.Size = new System.Drawing.Size(10, 13);
            this.lblUniqueId.TabIndex = 38;
            this.lblUniqueId.Text = "-";
            this.lblUniqueId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 16);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 13);
            this.label7.TabIndex = 37;
            this.label7.Text = "UniqueId";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 39;
            this.label1.Text = "LegsInfo";
            // 
            // LegsInfo
            // 
            this.LegsInfo.AutoSize = true;
            this.LegsInfo.Location = new System.Drawing.Point(96, 45);
            this.LegsInfo.Name = "LegsInfo";
            this.LegsInfo.Size = new System.Drawing.Size(10, 13);
            this.LegsInfo.TabIndex = 40;
            this.LegsInfo.Text = "-";
            this.LegsInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LSL_StrangleStopLoss
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 153);
            this.Controls.Add(this.groupBox1);
            this.Name = "LSL_StrangleStopLoss";
            this.Text = "LSL_StrangleStopLoss";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LSL_StrangleStopLoss_FormClosing);
            this.Load += new System.EventHandler(this.LSL_StrangleStopLoss_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtLSL_StrategyPercent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label StrategyInfo;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblUniqueId;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label LegsInfo;
        private System.Windows.Forms.Label label1;
    }
}