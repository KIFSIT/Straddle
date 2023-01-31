namespace Straddle
{
    partial class StrategySqOff
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.lblLossLive = new System.Windows.Forms.Label();
            this.lblPremiumLive = new System.Windows.Forms.Label();
            this.lblStrategyVegaLive = new System.Windows.Forms.Label();
            this.lblStrategyLossPrice = new System.Windows.Forms.Label();
            this.lblStrategyPremiumPrice = new System.Windows.Forms.Label();
            this.lblStrategyVegaPrice = new System.Windows.Forms.Label();
            this.cmbStrategyLoss = new System.Windows.Forms.ComboBox();
            this.txtStrategyLoss = new System.Windows.Forms.TextBox();
            this.chkStrategyLoss = new System.Windows.Forms.CheckBox();
            this.label16 = new System.Windows.Forms.Label();
            this.cmbStrategyPremium = new System.Windows.Forms.ComboBox();
            this.txtStrategyPremium = new System.Windows.Forms.TextBox();
            this.chkStrategyPremium = new System.Windows.Forms.CheckBox();
            this.label17 = new System.Windows.Forms.Label();
            this.cmbStrategyVega = new System.Windows.Forms.ComboBox();
            this.txtStrategyVega = new System.Windows.Forms.TextBox();
            this.chkStrategyVega = new System.Windows.Forms.CheckBox();
            this.label18 = new System.Windows.Forms.Label();
            this.lblStrategyName = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.chksqoffTime = new System.Windows.Forms.CheckBox();
            this.dtpSqOff = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chksqoffTime);
            this.groupBox2.Controls.Add(this.dtpSqOff);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.button4);
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.lblLossLive);
            this.groupBox2.Controls.Add(this.lblPremiumLive);
            this.groupBox2.Controls.Add(this.lblStrategyVegaLive);
            this.groupBox2.Controls.Add(this.lblStrategyLossPrice);
            this.groupBox2.Controls.Add(this.lblStrategyPremiumPrice);
            this.groupBox2.Controls.Add(this.lblStrategyVegaPrice);
            this.groupBox2.Controls.Add(this.cmbStrategyLoss);
            this.groupBox2.Controls.Add(this.txtStrategyLoss);
            this.groupBox2.Controls.Add(this.chkStrategyLoss);
            this.groupBox2.Controls.Add(this.label16);
            this.groupBox2.Controls.Add(this.cmbStrategyPremium);
            this.groupBox2.Controls.Add(this.txtStrategyPremium);
            this.groupBox2.Controls.Add(this.chkStrategyPremium);
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.cmbStrategyVega);
            this.groupBox2.Controls.Add(this.txtStrategyVega);
            this.groupBox2.Controls.Add(this.chkStrategyVega);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.lblStrategyName);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.shapeContainer2);
            this.groupBox2.Location = new System.Drawing.Point(1, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(449, 194);
            this.groupBox2.TabIndex = 339;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Strategy";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(252, 159);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 360;
            this.button4.Text = "OFF";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(126, 159);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 359;
            this.button3.Text = "Set";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // lblLossLive
            // 
            this.lblLossLive.AutoSize = true;
            this.lblLossLive.Location = new System.Drawing.Point(386, 135);
            this.lblLossLive.Name = "lblLossLive";
            this.lblLossLive.Size = new System.Drawing.Size(10, 13);
            this.lblLossLive.TabIndex = 358;
            this.lblLossLive.Text = "-";
            // 
            // lblPremiumLive
            // 
            this.lblPremiumLive.AutoSize = true;
            this.lblPremiumLive.Location = new System.Drawing.Point(386, 105);
            this.lblPremiumLive.Name = "lblPremiumLive";
            this.lblPremiumLive.Size = new System.Drawing.Size(10, 13);
            this.lblPremiumLive.TabIndex = 357;
            this.lblPremiumLive.Text = "-";
            // 
            // lblStrategyVegaLive
            // 
            this.lblStrategyVegaLive.AutoSize = true;
            this.lblStrategyVegaLive.Location = new System.Drawing.Point(386, 76);
            this.lblStrategyVegaLive.Name = "lblStrategyVegaLive";
            this.lblStrategyVegaLive.Size = new System.Drawing.Size(10, 13);
            this.lblStrategyVegaLive.TabIndex = 356;
            this.lblStrategyVegaLive.Text = "-";
            // 
            // lblStrategyLossPrice
            // 
            this.lblStrategyLossPrice.AutoSize = true;
            this.lblStrategyLossPrice.Location = new System.Drawing.Point(305, 135);
            this.lblStrategyLossPrice.Name = "lblStrategyLossPrice";
            this.lblStrategyLossPrice.Size = new System.Drawing.Size(10, 13);
            this.lblStrategyLossPrice.TabIndex = 354;
            this.lblStrategyLossPrice.Text = "-";
            // 
            // lblStrategyPremiumPrice
            // 
            this.lblStrategyPremiumPrice.AutoSize = true;
            this.lblStrategyPremiumPrice.Location = new System.Drawing.Point(305, 105);
            this.lblStrategyPremiumPrice.Name = "lblStrategyPremiumPrice";
            this.lblStrategyPremiumPrice.Size = new System.Drawing.Size(10, 13);
            this.lblStrategyPremiumPrice.TabIndex = 353;
            this.lblStrategyPremiumPrice.Text = "-";
            // 
            // lblStrategyVegaPrice
            // 
            this.lblStrategyVegaPrice.AutoSize = true;
            this.lblStrategyVegaPrice.Location = new System.Drawing.Point(305, 76);
            this.lblStrategyVegaPrice.Name = "lblStrategyVegaPrice";
            this.lblStrategyVegaPrice.Size = new System.Drawing.Size(10, 13);
            this.lblStrategyVegaPrice.TabIndex = 352;
            this.lblStrategyVegaPrice.Text = "-";
            // 
            // cmbStrategyLoss
            // 
            this.cmbStrategyLoss.FormattingEnabled = true;
            this.cmbStrategyLoss.Items.AddRange(new object[] {
            "Point",
            "Percent"});
            this.cmbStrategyLoss.Location = new System.Drawing.Point(191, 127);
            this.cmbStrategyLoss.Name = "cmbStrategyLoss";
            this.cmbStrategyLoss.Size = new System.Drawing.Size(91, 21);
            this.cmbStrategyLoss.TabIndex = 351;
            // 
            // txtStrategyLoss
            // 
            this.txtStrategyLoss.Location = new System.Drawing.Point(106, 127);
            this.txtStrategyLoss.Name = "txtStrategyLoss";
            this.txtStrategyLoss.Size = new System.Drawing.Size(79, 20);
            this.txtStrategyLoss.TabIndex = 350;
            this.txtStrategyLoss.Text = "0";
            this.txtStrategyLoss.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chkStrategyLoss
            // 
            this.chkStrategyLoss.AutoSize = true;
            this.chkStrategyLoss.Location = new System.Drawing.Point(89, 130);
            this.chkStrategyLoss.Name = "chkStrategyLoss";
            this.chkStrategyLoss.Size = new System.Drawing.Size(15, 14);
            this.chkStrategyLoss.TabIndex = 349;
            this.chkStrategyLoss.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(13, 128);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(47, 13);
            this.label16.TabIndex = 348;
            this.label16.Text = "SQ Loss";
            // 
            // cmbStrategyPremium
            // 
            this.cmbStrategyPremium.FormattingEnabled = true;
            this.cmbStrategyPremium.Items.AddRange(new object[] {
            "Point",
            "Percent"});
            this.cmbStrategyPremium.Location = new System.Drawing.Point(191, 101);
            this.cmbStrategyPremium.Name = "cmbStrategyPremium";
            this.cmbStrategyPremium.Size = new System.Drawing.Size(91, 21);
            this.cmbStrategyPremium.TabIndex = 347;
            // 
            // txtStrategyPremium
            // 
            this.txtStrategyPremium.Location = new System.Drawing.Point(106, 101);
            this.txtStrategyPremium.Name = "txtStrategyPremium";
            this.txtStrategyPremium.Size = new System.Drawing.Size(79, 20);
            this.txtStrategyPremium.TabIndex = 346;
            this.txtStrategyPremium.Text = "0";
            this.txtStrategyPremium.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chkStrategyPremium
            // 
            this.chkStrategyPremium.AutoSize = true;
            this.chkStrategyPremium.Location = new System.Drawing.Point(89, 104);
            this.chkStrategyPremium.Name = "chkStrategyPremium";
            this.chkStrategyPremium.Size = new System.Drawing.Size(15, 14);
            this.chkStrategyPremium.TabIndex = 345;
            this.chkStrategyPremium.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(13, 102);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(65, 13);
            this.label17.TabIndex = 344;
            this.label17.Text = "SQ Premium";
            // 
            // cmbStrategyVega
            // 
            this.cmbStrategyVega.FormattingEnabled = true;
            this.cmbStrategyVega.Items.AddRange(new object[] {
            "Point",
            "Percent"});
            this.cmbStrategyVega.Location = new System.Drawing.Point(191, 73);
            this.cmbStrategyVega.Name = "cmbStrategyVega";
            this.cmbStrategyVega.Size = new System.Drawing.Size(91, 21);
            this.cmbStrategyVega.TabIndex = 343;
            // 
            // txtStrategyVega
            // 
            this.txtStrategyVega.Location = new System.Drawing.Point(106, 73);
            this.txtStrategyVega.Name = "txtStrategyVega";
            this.txtStrategyVega.Size = new System.Drawing.Size(79, 20);
            this.txtStrategyVega.TabIndex = 342;
            this.txtStrategyVega.Text = "0";
            this.txtStrategyVega.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // chkStrategyVega
            // 
            this.chkStrategyVega.AutoSize = true;
            this.chkStrategyVega.Location = new System.Drawing.Point(89, 76);
            this.chkStrategyVega.Name = "chkStrategyVega";
            this.chkStrategyVega.Size = new System.Drawing.Size(15, 14);
            this.chkStrategyVega.TabIndex = 341;
            this.chkStrategyVega.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(13, 74);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(50, 13);
            this.label18.TabIndex = 340;
            this.label18.Text = "SQ Vega";
            // 
            // lblStrategyName
            // 
            this.lblStrategyName.AutoSize = true;
            this.lblStrategyName.Location = new System.Drawing.Point(117, 16);
            this.lblStrategyName.Name = "lblStrategyName";
            this.lblStrategyName.Size = new System.Drawing.Size(10, 13);
            this.lblStrategyName.TabIndex = 29;
            this.lblStrategyName.Text = "-";
            this.lblStrategyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 16);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(83, 13);
            this.label11.TabIndex = 28;
            this.label11.Text = "StrategyName : ";
            // 
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(3, 16);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape2});
            this.shapeContainer2.Size = new System.Drawing.Size(443, 175);
            this.shapeContainer2.TabIndex = 355;
            this.shapeContainer2.TabStop = false;
            // 
            // lineShape2
            // 
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = 357;
            this.lineShape2.X2 = 357;
            this.lineShape2.Y1 = 61;
            this.lineShape2.Y2 = 133;
            // 
            // chksqoffTime
            // 
            this.chksqoffTime.AutoSize = true;
            this.chksqoffTime.Location = new System.Drawing.Point(90, 47);
            this.chksqoffTime.Name = "chksqoffTime";
            this.chksqoffTime.Size = new System.Drawing.Size(15, 14);
            this.chksqoffTime.TabIndex = 363;
            this.chksqoffTime.UseVisualStyleBackColor = true;
            // 
            // dtpSqOff
            // 
            this.dtpSqOff.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtpSqOff.Location = new System.Drawing.Point(110, 43);
            this.dtpSqOff.Name = "dtpSqOff";
            this.dtpSqOff.ShowUpDown = true;
            this.dtpSqOff.Size = new System.Drawing.Size(76, 20);
            this.dtpSqOff.TabIndex = 362;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 361;
            this.label6.Text = "SQ_OFFTime";
            // 
            // StrategySqOff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 200);
            this.Controls.Add(this.groupBox2);
            this.Name = "StrategySqOff";
            this.Text = "StrategySqOff";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StrategySqOff_FormClosing);
            this.Load += new System.EventHandler(this.StrategySqOff_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label lblLossLive;
        private System.Windows.Forms.Label lblPremiumLive;
        private System.Windows.Forms.Label lblStrategyVegaLive;
        private System.Windows.Forms.Label lblStrategyLossPrice;
        private System.Windows.Forms.Label lblStrategyPremiumPrice;
        private System.Windows.Forms.Label lblStrategyVegaPrice;
        private System.Windows.Forms.ComboBox cmbStrategyLoss;
        private System.Windows.Forms.TextBox txtStrategyLoss;
        private System.Windows.Forms.CheckBox chkStrategyLoss;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox cmbStrategyPremium;
        private System.Windows.Forms.TextBox txtStrategyPremium;
        private System.Windows.Forms.CheckBox chkStrategyPremium;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox cmbStrategyVega;
        private System.Windows.Forms.TextBox txtStrategyVega;
        private System.Windows.Forms.CheckBox chkStrategyVega;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label lblStrategyName;
        private System.Windows.Forms.Label label11;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        private System.Windows.Forms.CheckBox chksqoffTime;
        private System.Windows.Forms.DateTimePicker dtpSqOff;
        private System.Windows.Forms.Label label6;
    }
}