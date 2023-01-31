namespace Straddle
{
    partial class ImmediateUnwind
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
            this.txtNoOfLots = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.lblUniqueId = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSeries = new System.Windows.Forms.Label();
            this.lblStrike = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblSymbol = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtNoOfLots
            // 
            this.txtNoOfLots.Location = new System.Drawing.Point(95, 76);
            this.txtNoOfLots.Name = "txtNoOfLots";
            this.txtNoOfLots.Size = new System.Drawing.Size(88, 20);
            this.txtNoOfLots.TabIndex = 48;
            this.txtNoOfLots.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtNoOfLots.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNoOfLots_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 47;
            this.label3.Text = "No Of Lots";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(95, 144);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 46;
            this.button1.Text = "UnWind";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblUniqueId
            // 
            this.lblUniqueId.AutoSize = true;
            this.lblUniqueId.Location = new System.Drawing.Point(71, 9);
            this.lblUniqueId.Name = "lblUniqueId";
            this.lblUniqueId.Size = new System.Drawing.Size(10, 13);
            this.lblUniqueId.TabIndex = 45;
            this.lblUniqueId.Text = "-";
            this.lblUniqueId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 13);
            this.label7.TabIndex = 44;
            this.label7.Text = "UniqueId";
            // 
            // lblSeries
            // 
            this.lblSeries.AutoSize = true;
            this.lblSeries.Location = new System.Drawing.Point(216, 41);
            this.lblSeries.Name = "lblSeries";
            this.lblSeries.Size = new System.Drawing.Size(10, 13);
            this.lblSeries.TabIndex = 43;
            this.lblSeries.Text = "-";
            this.lblSeries.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStrike
            // 
            this.lblStrike.AutoSize = true;
            this.lblStrike.Location = new System.Drawing.Point(173, 41);
            this.lblStrike.Name = "lblStrike";
            this.lblStrike.Size = new System.Drawing.Size(10, 13);
            this.lblStrike.TabIndex = 42;
            this.lblStrike.Text = "-";
            this.lblStrike.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(117, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 41;
            this.label2.Text = "Strike";
            // 
            // lblSymbol
            // 
            this.lblSymbol.AutoSize = true;
            this.lblSymbol.Location = new System.Drawing.Point(59, 41);
            this.lblSymbol.Name = "lblSymbol";
            this.lblSymbol.Size = new System.Drawing.Size(10, 13);
            this.lblSymbol.TabIndex = 40;
            this.lblSymbol.Text = "-";
            this.lblSymbol.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 39;
            this.label1.Text = "Symbol";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(95, 102);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(88, 20);
            this.txtPassword.TabIndex = 49;
            this.txtPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(11, 102);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(53, 13);
            this.lblPassword.TabIndex = 50;
            this.lblPassword.Text = "Password";
            // 
            // ImmediateUnwind
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(244, 174);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtNoOfLots);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblUniqueId);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblSeries);
            this.Controls.Add(this.lblStrike);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblSymbol);
            this.Controls.Add(this.label1);
            this.Name = "ImmediateUnwind";
            this.Text = "ImmediateUnwind";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImmediateUnwind_FormClosing);
            this.Load += new System.EventHandler(this.ImmediateUnwind_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtNoOfLots;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblUniqueId;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblSeries;
        private System.Windows.Forms.Label lblStrike;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblSymbol;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
    }
}