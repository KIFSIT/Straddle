namespace Straddle
{
    partial class SingleLeg
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
            this.max = new System.Windows.Forms.TextBox();
            this.min = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.addRule1 = new System.Windows.Forms.Button();
            this.cmbExpiry1 = new System.Windows.Forms.ComboBox();
            this.cmbInstrument1 = new System.Windows.Forms.ComboBox();
            this.cmbSeries1 = new System.Windows.Forms.ComboBox();
            this.cmbStrike1 = new System.Windows.Forms.ComboBox();
            this.cmbSymbol1 = new System.Windows.Forms.ComboBox();
            this.cmbStrategy = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // max
            // 
            this.max.Enabled = false;
            this.max.Location = new System.Drawing.Point(121, 9);
            this.max.Name = "max";
            this.max.Size = new System.Drawing.Size(45, 20);
            this.max.TabIndex = 201;
            this.max.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // min
            // 
            this.min.Enabled = false;
            this.min.Location = new System.Drawing.Point(41, 8);
            this.min.Name = "min";
            this.min.Size = new System.Drawing.Size(45, 20);
            this.min.TabIndex = 200;
            this.min.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(89, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 199;
            this.label4.Text = "max";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 13);
            this.label2.TabIndex = 198;
            this.label2.Text = "min";
            // 
            // addRule1
            // 
            this.addRule1.Location = new System.Drawing.Point(194, 122);
            this.addRule1.Name = "addRule1";
            this.addRule1.Size = new System.Drawing.Size(63, 33);
            this.addRule1.TabIndex = 207;
            this.addRule1.Text = "addRule";
            this.addRule1.UseVisualStyleBackColor = true;
            this.addRule1.Click += new System.EventHandler(this.addRule1_Click);
            // 
            // cmbExpiry1
            // 
            this.cmbExpiry1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExpiry1.FormattingEnabled = true;
            this.cmbExpiry1.Location = new System.Drawing.Point(194, 83);
            this.cmbExpiry1.Name = "cmbExpiry1";
            this.cmbExpiry1.Size = new System.Drawing.Size(92, 21);
            this.cmbExpiry1.TabIndex = 205;
            this.cmbExpiry1.SelectionChangeCommitted += new System.EventHandler(this.cmbExpiry1_SelectionChangeCommitted);
            // 
            // cmbInstrument1
            // 
            this.cmbInstrument1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInstrument1.FormattingEnabled = true;
            this.cmbInstrument1.Location = new System.Drawing.Point(8, 83);
            this.cmbInstrument1.Name = "cmbInstrument1";
            this.cmbInstrument1.Size = new System.Drawing.Size(74, 21);
            this.cmbInstrument1.TabIndex = 202;
            this.cmbInstrument1.SelectionChangeCommitted += new System.EventHandler(this.cmbInstrument1_SelectionChangeCommitted);
            // 
            // cmbSeries1
            // 
            this.cmbSeries1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSeries1.FormattingEnabled = true;
            this.cmbSeries1.Location = new System.Drawing.Point(378, 83);
            this.cmbSeries1.Name = "cmbSeries1";
            this.cmbSeries1.Size = new System.Drawing.Size(71, 21);
            this.cmbSeries1.TabIndex = 206;
            // 
            // cmbStrike1
            // 
            this.cmbStrike1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbStrike1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbStrike1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStrike1.FormattingEnabled = true;
            this.cmbStrike1.Location = new System.Drawing.Point(288, 83);
            this.cmbStrike1.Name = "cmbStrike1";
            this.cmbStrike1.Size = new System.Drawing.Size(84, 21);
            this.cmbStrike1.TabIndex = 204;
            this.cmbStrike1.SelectionChangeCommitted += new System.EventHandler(this.cmbStrike1_SelectionChangeCommitted);
            // 
            // cmbSymbol1
            // 
            this.cmbSymbol1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSymbol1.FormattingEnabled = true;
            this.cmbSymbol1.Location = new System.Drawing.Point(88, 83);
            this.cmbSymbol1.Name = "cmbSymbol1";
            this.cmbSymbol1.Size = new System.Drawing.Size(104, 21);
            this.cmbSymbol1.TabIndex = 203;
            this.cmbSymbol1.SelectionChangeCommitted += new System.EventHandler(this.cmbSymbol1_SelectionChangeCommitted);
            // 
            // cmbStrategy
            // 
            this.cmbStrategy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStrategy.FormattingEnabled = true;
            this.cmbStrategy.Location = new System.Drawing.Point(338, 27);
            this.cmbStrategy.Name = "cmbStrategy";
            this.cmbStrategy.Size = new System.Drawing.Size(96, 21);
            this.cmbStrategy.TabIndex = 242;
            // 
            // SingleLeg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 178);
            this.Controls.Add(this.cmbStrategy);
            this.Controls.Add(this.addRule1);
            this.Controls.Add(this.cmbExpiry1);
            this.Controls.Add(this.cmbInstrument1);
            this.Controls.Add(this.cmbSeries1);
            this.Controls.Add(this.cmbStrike1);
            this.Controls.Add(this.cmbSymbol1);
            this.Controls.Add(this.max);
            this.Controls.Add(this.min);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Name = "SingleLeg";
            this.Text = "SingleLeg";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SingleLeg_FormClosing);
            this.Load += new System.EventHandler(this.SingleLeg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox max;
        private System.Windows.Forms.TextBox min;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button addRule1;
        public System.Windows.Forms.ComboBox cmbExpiry1;
        public System.Windows.Forms.ComboBox cmbInstrument1;
        public System.Windows.Forms.ComboBox cmbSeries1;
        public System.Windows.Forms.ComboBox cmbStrike1;
        public System.Windows.Forms.ComboBox cmbSymbol1;
        public System.Windows.Forms.ComboBox cmbStrategy;
    }
}