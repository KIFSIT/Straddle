namespace Straddle
{
    partial class ContractPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbGateway = new System.Windows.Forms.ComboBox();
            this.cmbInstrumentName = new System.Windows.Forms.ComboBox();
            this.cmbSymbol = new System.Windows.Forms.ComboBox();
            this.cmbStrikePrice = new System.Windows.Forms.ComboBox();
            this.cmbExpiryDate = new System.Windows.Forms.ComboBox();
            this.cmbSeries = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cmbGateway
            // 
            this.cmbGateway.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGateway.FormattingEnabled = true;
            this.cmbGateway.Location = new System.Drawing.Point(5, 5);
            this.cmbGateway.Name = "cmbGateway";
            this.cmbGateway.Size = new System.Drawing.Size(69, 21);
            this.cmbGateway.TabIndex = 0;
            this.cmbGateway.SelectedIndexChanged += new System.EventHandler(this.cmbGateway_SelectedIndexChanged);
            // 
            // cmbInstrumentName
            // 
            this.cmbInstrumentName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInstrumentName.FormattingEnabled = true;
            this.cmbInstrumentName.Location = new System.Drawing.Point(79, 5);
            this.cmbInstrumentName.Name = "cmbInstrumentName";
            this.cmbInstrumentName.Size = new System.Drawing.Size(80, 21);
            this.cmbInstrumentName.TabIndex = 1;
            this.cmbInstrumentName.SelectedIndexChanged += new System.EventHandler(this.cmbInstrumentName_SelectedIndexChanged);
            this.cmbInstrumentName.SelectionChangeCommitted += new System.EventHandler(this.cmbInstrumentName_SelectionChangeCommitted);
            // 
            // cmbSymbol
            // 
            this.cmbSymbol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSymbol.FormattingEnabled = true;
            this.cmbSymbol.Location = new System.Drawing.Point(165, 5);
            this.cmbSymbol.Name = "cmbSymbol";
            this.cmbSymbol.Size = new System.Drawing.Size(104, 21);
            this.cmbSymbol.TabIndex = 2;
            this.cmbSymbol.SelectedIndexChanged += new System.EventHandler(this.cmbSymbol_SelectedIndexChanged);
            this.cmbSymbol.SelectionChangeCommitted += new System.EventHandler(this.cmbSymbol_SelectionChangeCommitted);
            // 
            // cmbStrikePrice
            // 
            this.cmbStrikePrice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStrikePrice.FormattingEnabled = true;
            this.cmbStrikePrice.Location = new System.Drawing.Point(378, 5);
            this.cmbStrikePrice.Name = "cmbStrikePrice";
            this.cmbStrikePrice.Size = new System.Drawing.Size(78, 21);
            this.cmbStrikePrice.TabIndex = 4;
            this.cmbStrikePrice.SelectedIndexChanged += new System.EventHandler(this.cmbStrikePrice_SelectedIndexChanged);
            this.cmbStrikePrice.SelectionChangeCommitted += new System.EventHandler(this.cmbStrikePrice_SelectionChangeCommitted);
            // 
            // cmbExpiryDate
            // 
            this.cmbExpiryDate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExpiryDate.FormattingEnabled = true;
            this.cmbExpiryDate.Location = new System.Drawing.Point(273, 5);
            this.cmbExpiryDate.Name = "cmbExpiryDate";
            this.cmbExpiryDate.Size = new System.Drawing.Size(101, 21);
            this.cmbExpiryDate.TabIndex = 3;
            this.cmbExpiryDate.SelectedIndexChanged += new System.EventHandler(this.cmbExpiryDate_SelectedIndexChanged);
            this.cmbExpiryDate.SelectionChangeCommitted += new System.EventHandler(this.cmbExpiryDate_SelectionChangeCommitted);
            // 
            // cmbSeries
            // 
            this.cmbSeries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSeries.FormattingEnabled = true;
            this.cmbSeries.Location = new System.Drawing.Point(462, 5);
            this.cmbSeries.Name = "cmbSeries";
            this.cmbSeries.Size = new System.Drawing.Size(53, 21);
            this.cmbSeries.TabIndex = 5;
            this.cmbSeries.SelectionChangeCommitted += new System.EventHandler(this.cmbSeries_SelectionChangeCommitted);
            // 
            // ContractPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cmbSeries);
            this.Controls.Add(this.cmbExpiryDate);
            this.Controls.Add(this.cmbStrikePrice);
            this.Controls.Add(this.cmbSymbol);
            this.Controls.Add(this.cmbInstrumentName);
            this.Controls.Add(this.cmbGateway);
            this.Name = "ContractPanel";
            this.Size = new System.Drawing.Size(519, 30);
            this.Load += new System.EventHandler(this.ContractPanel_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ComboBox cmbGateway;
        public System.Windows.Forms.ComboBox cmbInstrumentName;
        public System.Windows.Forms.ComboBox cmbSymbol;
        public System.Windows.Forms.ComboBox cmbStrikePrice;
        public System.Windows.Forms.ComboBox cmbExpiryDate;
        public System.Windows.Forms.ComboBox cmbSeries;

    }
}
