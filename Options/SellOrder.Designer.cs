namespace Straddle
{
    partial class SellOrder
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
            this.button1 = new System.Windows.Forms.Button();
            this.txtPrice = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtNoOfLots = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblUniqueId = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSeries = new System.Windows.Forms.Label();
            this.lblStrike = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblSymbol = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblLtp = new System.Windows.Forms.Label();
            this.lblAsk = new System.Windows.Forms.Label();
            this.lblBid = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(95, 203);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 62;
            this.button1.Text = "Send";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPrice
            // 
            this.txtPrice.Location = new System.Drawing.Point(95, 130);
            this.txtPrice.Name = "txtPrice";
            this.txtPrice.Size = new System.Drawing.Size(88, 20);
            this.txtPrice.TabIndex = 61;
            this.txtPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 160);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 60;
            this.label4.Text = "No Of Lots";
            // 
            // txtNoOfLots
            // 
            this.txtNoOfLots.Location = new System.Drawing.Point(95, 160);
            this.txtNoOfLots.Name = "txtNoOfLots";
            this.txtNoOfLots.Size = new System.Drawing.Size(88, 20);
            this.txtNoOfLots.TabIndex = 59;
            this.txtNoOfLots.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 58;
            this.label3.Text = "Price";
            // 
            // lblUniqueId
            // 
            this.lblUniqueId.AutoSize = true;
            this.lblUniqueId.Location = new System.Drawing.Point(71, 9);
            this.lblUniqueId.Name = "lblUniqueId";
            this.lblUniqueId.Size = new System.Drawing.Size(10, 13);
            this.lblUniqueId.TabIndex = 57;
            this.lblUniqueId.Text = "-";
            this.lblUniqueId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 13);
            this.label7.TabIndex = 56;
            this.label7.Text = "UniqueId";
            // 
            // lblSeries
            // 
            this.lblSeries.AutoSize = true;
            this.lblSeries.Location = new System.Drawing.Point(216, 41);
            this.lblSeries.Name = "lblSeries";
            this.lblSeries.Size = new System.Drawing.Size(10, 13);
            this.lblSeries.TabIndex = 55;
            this.lblSeries.Text = "-";
            this.lblSeries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStrike
            // 
            this.lblStrike.AutoSize = true;
            this.lblStrike.Location = new System.Drawing.Point(173, 41);
            this.lblStrike.Name = "lblStrike";
            this.lblStrike.Size = new System.Drawing.Size(10, 13);
            this.lblStrike.TabIndex = 54;
            this.lblStrike.Text = "-";
            this.lblStrike.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(117, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 53;
            this.label2.Text = "Strike";
            // 
            // lblSymbol
            // 
            this.lblSymbol.AutoSize = true;
            this.lblSymbol.Location = new System.Drawing.Point(59, 41);
            this.lblSymbol.Name = "lblSymbol";
            this.lblSymbol.Size = new System.Drawing.Size(10, 13);
            this.lblSymbol.TabIndex = 52;
            this.lblSymbol.Text = "-";
            this.lblSymbol.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 51;
            this.label1.Text = "Symbol";
            // 
            // lblLtp
            // 
            this.lblLtp.AutoSize = true;
            this.lblLtp.Location = new System.Drawing.Point(198, 86);
            this.lblLtp.Name = "lblLtp";
            this.lblLtp.Size = new System.Drawing.Size(10, 13);
            this.lblLtp.TabIndex = 68;
            this.lblLtp.Text = "-";
            this.lblLtp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAsk
            // 
            this.lblAsk.AutoSize = true;
            this.lblAsk.Location = new System.Drawing.Point(72, 98);
            this.lblAsk.Name = "lblAsk";
            this.lblAsk.Size = new System.Drawing.Size(10, 13);
            this.lblAsk.TabIndex = 67;
            this.lblAsk.Text = "-";
            this.lblAsk.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBid
            // 
            this.lblBid.AutoSize = true;
            this.lblBid.Location = new System.Drawing.Point(72, 73);
            this.lblBid.Name = "lblBid";
            this.lblBid.Size = new System.Drawing.Size(10, 13);
            this.lblBid.TabIndex = 66;
            this.lblBid.Text = "-";
            this.lblBid.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(130, 86);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(22, 13);
            this.label8.TabIndex = 65;
            this.label8.Text = "Ltp";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 98);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(25, 13);
            this.label6.TabIndex = 64;
            this.label6.Text = "Ask";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 13);
            this.label5.TabIndex = 63;
            this.label5.Text = "Bid";
            // 
            // SellOrder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(265, 231);
            this.Controls.Add(this.lblLtp);
            this.Controls.Add(this.lblAsk);
            this.Controls.Add(this.lblBid);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtPrice);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtNoOfLots);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblUniqueId);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblSeries);
            this.Controls.Add(this.lblStrike);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblSymbol);
            this.Controls.Add(this.label1);
            this.Name = "SellOrder";
            this.Text = "SellOrder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SellOrder_FormClosing);
            this.Load += new System.EventHandler(this.SellOrder_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtPrice;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtNoOfLots;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblUniqueId;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblSeries;
        private System.Windows.Forms.Label lblStrike;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblSymbol;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblLtp;
        private System.Windows.Forms.Label lblAsk;
        private System.Windows.Forms.Label lblBid;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
    }
}