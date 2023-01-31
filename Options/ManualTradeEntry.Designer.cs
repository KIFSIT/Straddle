namespace Straddle
{
    partial class ManualTradeEntry
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtunique = new System.Windows.Forms.TextBox();
            this.txtrate = new System.Windows.Forms.TextBox();
            this.txtiswind = new System.Windows.Forms.ComboBox();
            this.btnsend = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtQty = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFutToken = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lblStrategy = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 137;
            this.label1.Text = "Unique_id";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(199, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 138;
            this.label2.Text = "Rate";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(118, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 139;
            this.label3.Text = "Wind";
            // 
            // txtunique
            // 
            this.txtunique.Enabled = false;
            this.txtunique.Location = new System.Drawing.Point(12, 26);
            this.txtunique.Name = "txtunique";
            this.txtunique.Size = new System.Drawing.Size(70, 20);
            this.txtunique.TabIndex = 1;
            // 
            // txtrate
            // 
            this.txtrate.Location = new System.Drawing.Point(178, 25);
            this.txtrate.Name = "txtrate";
            this.txtrate.Size = new System.Drawing.Size(70, 20);
            this.txtrate.TabIndex = 3;
            // 
            // txtiswind
            // 
            this.txtiswind.DisplayMember = "1";
            this.txtiswind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.txtiswind.FormattingEnabled = true;
            this.txtiswind.Items.AddRange(new object[] {
            "Wind",
            "UnWind"});
            this.txtiswind.Location = new System.Drawing.Point(100, 25);
            this.txtiswind.Name = "txtiswind";
            this.txtiswind.Size = new System.Drawing.Size(72, 21);
            this.txtiswind.TabIndex = 2;
            this.txtiswind.ValueMember = "1";
            // 
            // btnsend
            // 
            this.btnsend.Location = new System.Drawing.Point(431, 22);
            this.btnsend.Name = "btnsend";
            this.btnsend.Size = new System.Drawing.Size(75, 23);
            this.btnsend.TabIndex = 5;
            this.btnsend.Text = "Send";
            this.btnsend.UseVisualStyleBackColor = true;
            this.btnsend.Click += new System.EventHandler(this.btnsend_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(166, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(130, 13);
            this.label5.TabIndex = 145;
            this.label5.Text = "Rate should be in Rupees";
            // 
            // txtQty
            // 
            this.txtQty.Location = new System.Drawing.Point(252, 25);
            this.txtQty.Name = "txtQty";
            this.txtQty.Size = new System.Drawing.Size(70, 20);
            this.txtQty.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(273, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 13);
            this.label4.TabIndex = 144;
            this.label4.Text = "Qty";
            // 
            // txtFutToken
            // 
            this.txtFutToken.Enabled = false;
            this.txtFutToken.Location = new System.Drawing.Point(328, 25);
            this.txtFutToken.Name = "txtFutToken";
            this.txtFutToken.Size = new System.Drawing.Size(81, 20);
            this.txtFutToken.TabIndex = 146;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(340, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 147;
            this.label6.Text = "FutToken";
            // 
            // lblStrategy
            // 
            this.lblStrategy.AutoSize = true;
            this.lblStrategy.Location = new System.Drawing.Point(455, 6);
            this.lblStrategy.Name = "lblStrategy";
            this.lblStrategy.Size = new System.Drawing.Size(0, 13);
            this.lblStrategy.TabIndex = 148;
            // 
            // ManualTradeEntry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 69);
            this.Controls.Add(this.lblStrategy);
            this.Controls.Add(this.txtFutToken);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtQty);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnsend);
            this.Controls.Add(this.txtiswind);
            this.Controls.Add(this.txtrate);
            this.Controls.Add(this.txtunique);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ManualTradeEntry";
            this.Text = "ManualTradeEntry";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ManualTradeEntry_FormClosing);
            this.Load += new System.EventHandler(this.ManualTradeEntry_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtunique;
        private System.Windows.Forms.TextBox txtrate;
        public System.Windows.Forms.ComboBox txtiswind;
        private System.Windows.Forms.Button btnsend;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtQty;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFutToken;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblStrategy;
    }
}