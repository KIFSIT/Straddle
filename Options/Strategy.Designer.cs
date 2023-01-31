namespace Straddle
{
    partial class Strategy
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
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtStrategy = new System.Windows.Forms.Label();
            this.cmbStrategyName = new System.Windows.Forms.ComboBox();
            this.cmbRule = new System.Windows.Forms.ComboBox();
            this.txtRuleName = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbType
            // 
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "New",
            "Existing"});
            this.cmbType.Location = new System.Drawing.Point(72, 10);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(109, 21);
            this.cmbType.TabIndex = 148;
            this.cmbType.SelectionChangeCommitted += new System.EventHandler(this.cmbType_SelectionChangeCommitted);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 149;
            this.label1.Text = "Type";
            // 
            // txtStrategy
            // 
            this.txtStrategy.AutoSize = true;
            this.txtStrategy.Location = new System.Drawing.Point(12, 45);
            this.txtStrategy.Name = "txtStrategy";
            this.txtStrategy.Size = new System.Drawing.Size(46, 13);
            this.txtStrategy.TabIndex = 150;
            this.txtStrategy.Text = "Strategy";
            // 
            // cmbStrategyName
            // 
            this.cmbStrategyName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStrategyName.FormattingEnabled = true;
            this.cmbStrategyName.Location = new System.Drawing.Point(72, 42);
            this.cmbStrategyName.Name = "cmbStrategyName";
            this.cmbStrategyName.Size = new System.Drawing.Size(109, 21);
            this.cmbStrategyName.TabIndex = 151;
            // 
            // cmbRule
            // 
            this.cmbRule.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRule.FormattingEnabled = true;
            this.cmbRule.Items.AddRange(new object[] {
            "Single",
            "Ratio1_1",
            "Ratio1_2",
            "RatioUserDefined",
            "Strangle",
            "Straddle",
            "Ladder",
            "RatioCovShort",
            "ButterFly",
            "1331",
            "1221",
            "Empty"});
            this.cmbRule.Location = new System.Drawing.Point(72, 75);
            this.cmbRule.Name = "cmbRule";
            this.cmbRule.Size = new System.Drawing.Size(109, 21);
            this.cmbRule.TabIndex = 152;
            // 
            // txtRuleName
            // 
            this.txtRuleName.AutoSize = true;
            this.txtRuleName.Location = new System.Drawing.Point(12, 79);
            this.txtRuleName.Name = "txtRuleName";
            this.txtRuleName.Size = new System.Drawing.Size(29, 13);
            this.txtRuleName.TabIndex = 153;
            this.txtRuleName.Text = "Rule";
            // 
            // button5
            // 
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button5.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.Location = new System.Drawing.Point(72, 110);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 25);
            this.button5.TabIndex = 154;
            this.button5.Text = "Strike Add";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Strategy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(192, 147);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.txtRuleName);
            this.Controls.Add(this.cmbRule);
            this.Controls.Add(this.cmbStrategyName);
            this.Controls.Add(this.txtStrategy);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbType);
            this.Name = "Strategy";
            this.Text = "Strategy";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Strategy_FormClosing);
            this.Load += new System.EventHandler(this.Strategy_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label txtStrategy;
        public System.Windows.Forms.ComboBox cmbStrategyName;
        public System.Windows.Forms.ComboBox cmbRule;
        private System.Windows.Forms.Label txtRuleName;
        private System.Windows.Forms.Button button5;
    }
}